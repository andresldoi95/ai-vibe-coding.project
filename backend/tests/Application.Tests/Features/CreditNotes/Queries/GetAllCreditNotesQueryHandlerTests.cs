using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Queries.GetAllCreditNotes;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Queries;

public class GetAllCreditNotesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllCreditNotesQueryHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly GetAllCreditNotesQueryHandler _handler;

    public GetAllCreditNotesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllCreditNotesQueryHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);

        _handler = new GetAllCreditNotesQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    private (Customer customer, EmissionPoint emissionPoint, CreditNote creditNote) BuildCreditNoteWithRelations(
        Guid tenantId,
        InvoiceStatus status = InvoiceStatus.Draft,
        DateTime? issuedDate = null)
    {
        var customerId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishment = new Establishment { TenantId = tenantId, EstablishmentCode = "001" };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "Test Customer" };
        var emissionPoint = new EmissionPoint { Id = emissionPointId, TenantId = tenantId, Establishment = establishment };
        var creditNote = new CreditNote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Status = status,
            CustomerId = customerId,
            EmissionPointId = emissionPointId,
            IsDeleted = false,
            CreditNoteNumber = "001-001-000000001",
            Reason = "Return",
            SubtotalAmount = 100m,
            TaxAmount = 15m,
            TotalAmount = 115m,
            IssueDate = issuedDate ?? DateTime.UtcNow,
            CreatedAt = issuedDate ?? DateTime.UtcNow
        };
        return (customer, emissionPoint, creditNote);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExistingCreditNotes_ShouldReturnList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (customer, emissionPoint, creditNote) = BuildCreditNoteWithRelations(tenantId);
        var creditNotes = new List<CreditNote> { creditNote };

        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(creditNotes);
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPoint.Id, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnSuccessWithEmptyCollection()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditNote>());

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── Multi-tenant isolation ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNotesFromDifferentTenant_ShouldExcludeThem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var myNote = new CreditNote { Id = Guid.NewGuid(), TenantId = tenantId, IsDeleted = false, Status = InvoiceStatus.Draft };
        var otherNote = new CreditNote { Id = Guid.NewGuid(), TenantId = otherTenantId, IsDeleted = false, Status = InvoiceStatus.Draft };
        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditNote> { myNote, otherNote });

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(myNote.Id);
    }

    [Fact]
    public async Task Handle_DeletedCreditNotes_ShouldExcludeThem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var activeNote = new CreditNote { Id = Guid.NewGuid(), TenantId = tenantId, IsDeleted = false, Status = InvoiceStatus.Draft };
        var deletedNote = new CreditNote { Id = Guid.NewGuid(), TenantId = tenantId, IsDeleted = true, Status = InvoiceStatus.Draft };
        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditNote> { activeNote, deletedNote });

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(activeNote.Id);
    }

    // ── Filters ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CustomerIdFilter_ShouldReturnOnlyMatchingCreditNotes()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var targetCustomerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var matchNote = new CreditNote { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = targetCustomerId, IsDeleted = false, Status = InvoiceStatus.Draft };
        var otherNote = new CreditNote { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = Guid.NewGuid(), IsDeleted = false, Status = InvoiceStatus.Draft };
        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditNote> { matchNote, otherNote });

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery { CustomerId = targetCustomerId }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(matchNote.Id);
    }

    [Fact]
    public async Task Handle_StatusFilter_ShouldReturnOnlyMatchingCreditNotes()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var draftNote = new CreditNote { Id = Guid.NewGuid(), TenantId = tenantId, Status = InvoiceStatus.Draft, IsDeleted = false };
        var authorizedNote = new CreditNote { Id = Guid.NewGuid(), TenantId = tenantId, Status = InvoiceStatus.Authorized, IsDeleted = false };
        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditNote> { draftNote, authorizedNote });

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery { Status = InvoiceStatus.Draft }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(draftNote.Id);
    }

    [Fact]
    public async Task Handle_DateFromFilter_ShouldExcludeCreditNotesBeforeDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cutoff = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (_, _, beforeNote) = BuildCreditNoteWithRelations(tenantId, issuedDate: new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var (_, _, afterNote) = BuildCreditNoteWithRelations(tenantId, issuedDate: new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditNote> { beforeNote, afterNote });

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery { DateFrom = cutoff }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(afterNote.Id);
    }

    [Fact]
    public async Task Handle_DateToFilter_ShouldExcludeCreditNotesAfterDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cutoff = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (_, _, beforeNote) = BuildCreditNoteWithRelations(tenantId, issuedDate: new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var (_, _, afterNote) = BuildCreditNoteWithRelations(tenantId, issuedDate: new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        _creditNoteRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditNote> { beforeNote, afterNote });

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery { DateTo = cutoff }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(beforeNote.Id);
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(new GetAllCreditNotesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
