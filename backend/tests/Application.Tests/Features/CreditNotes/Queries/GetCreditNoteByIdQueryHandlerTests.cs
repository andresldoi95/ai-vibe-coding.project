using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Queries.GetCreditNoteById;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Queries;

public class GetCreditNoteByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetCreditNoteByIdQueryHandler>> _loggerMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly GetCreditNoteByIdQueryHandler _handler;

    public GetCreditNoteByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetCreditNoteByIdQueryHandler>>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();

        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);

        _handler = new GetCreditNoteByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    private CreditNote BuildCreditNote(Guid tenantId, Guid creditNoteId, InvoiceStatus status = InvoiceStatus.Draft)
    {
        var customerId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishment = new Establishment { TenantId = tenantId, EstablishmentCode = "001", Name = "Main" };
        var emissionPoint = new EmissionPoint { Id = emissionPointId, TenantId = tenantId, EmissionPointCode = "002", Establishment = establishment };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "ACME Corp" };
        return new CreditNote
        {
            Id = creditNoteId,
            TenantId = tenantId,
            Status = status,
            CreditNoteNumber = "001-001-000000001",
            Reason = "Product defect",
            CustomerId = customerId,
            Customer = customer,
            EmissionPointId = emissionPointId,
            EmissionPoint = emissionPoint,
            SubtotalAmount = 100m,
            TaxAmount = 15m,
            TotalAmount = 115m,
            Notes = "Some notes",
            IsDeleted = false,
            CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Items = new List<CreditNoteItem>
            {
                new CreditNoteItem { Id = Guid.NewGuid(), Description = "Item 1", Quantity = 2, UnitPrice = 50m, TaxRate = 0.15m, SubtotalAmount = 100m, TaxAmount = 15m, TotalAmount = 115m }
            }
        };
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExistingCreditNote_ShouldReturnMappedDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var cn = BuildCreditNote(tenantId, creditNoteId);
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(cn);

        var query = new GetCreditNoteByIdQuery(creditNoteId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(creditNoteId);
        result.Value.CreditNoteNumber.Should().Be("001-001-000000001");
        result.Value.CustomerName.Should().Be("ACME Corp");
        result.Value.EmissionPointCode.Should().Be("002");
        result.Value.EstablishmentCode.Should().Be("001");
        result.Value.Reason.Should().Be("Product defect");
        result.Value.SubtotalAmount.Should().Be(100m);
        result.Value.TaxAmount.Should().Be(15m);
        result.Value.TotalAmount.Should().Be(115m);
        result.Value.Notes.Should().Be("Some notes");
    }

    [Fact]
    public async Task Handle_ExistingCreditNote_ShouldMapItemsCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var cn = BuildCreditNote(tenantId, creditNoteId);
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(cn);

        var query = new GetCreditNoteByIdQuery(creditNoteId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Description.Should().Be("Item 1");
        result.Value.Items.First().Quantity.Should().Be(2);
        result.Value.Items.First().UnitPrice.Should().Be(50m);
    }

    // ── IsEditable ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DraftNotDeleted_IsEditable_ShouldBeTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var cn = BuildCreditNote(tenantId, creditNoteId, InvoiceStatus.Draft);
        cn.IsDeleted = false;
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(cn);

        // Act
        var result = await _handler.Handle(new GetCreditNoteByIdQuery(creditNoteId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsEditable.Should().BeTrue();
    }

    [Theory]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.PendingAuthorization)]
    [InlineData(InvoiceStatus.Authorized)]
    [InlineData(InvoiceStatus.Rejected)]
    public async Task Handle_NonDraftStatus_IsEditable_ShouldBeFalse(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var cn = BuildCreditNote(tenantId, creditNoteId, status);
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(creditNoteId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(cn);

        // Act
        var result = await _handler.Handle(new GetCreditNoteByIdQuery(creditNoteId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsEditable.Should().BeFalse();
    }

    // ── Not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CreditNoteNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _creditNoteRepositoryMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>(), tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((CreditNote?)null);

        // Act
        var result = await _handler.Handle(new GetCreditNoteByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credit note not found");
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(new GetCreditNoteByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
