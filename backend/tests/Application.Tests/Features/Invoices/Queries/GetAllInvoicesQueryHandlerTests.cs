using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.Invoices.Queries.GetAllInvoices;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.Invoices.Queries;

public class GetAllInvoicesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetAllInvoicesQueryHandler>> _loggerMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly GetAllInvoicesQueryHandler _handler;

    public GetAllInvoicesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetAllInvoicesQueryHandler>>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();

        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);

        _handler = new GetAllInvoicesQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    private (Customer customer, EmissionPoint emissionPoint, Invoice invoice) BuildInvoiceWithRelations(
        Guid tenantId,
        InvoiceStatus status = InvoiceStatus.Draft,
        DateTime? issuedDate = null)
    {
        var customerId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishment = new Establishment { TenantId = tenantId, EstablishmentCode = "001" };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "Test Customer" };
        var emissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = tenantId,
            Establishment = establishment,
            EmissionPointCode = "001",
            Name = "Main Point"
        };
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Status = status,
            CustomerId = customerId,
            EmissionPointId = emissionPointId,
            IsDeleted = false,
            InvoiceNumber = "001-001-000000001",
            SubtotalAmount = 100m,
            TaxAmount = 15m,
            TotalAmount = 115m,
            IssueDate = issuedDate ?? DateTime.UtcNow,
            DueDate = (issuedDate ?? DateTime.UtcNow).AddDays(30),
            CreatedAt = issuedDate ?? DateTime.UtcNow
        };
        return (customer, emissionPoint, invoice);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExistingInvoices_ShouldReturnList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (customer, emissionPoint, invoice) = BuildInvoiceWithRelations(tenantId);

        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { invoice });
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPoint.Id, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().InvoiceNumber.Should().Be("001-001-000000001");
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnSuccessWithEmptyCollection()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice>());

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── Multi-tenant isolation ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InvoicesFromDifferentTenant_ShouldExcludeThem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var myInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = tenantId, IsDeleted = false, Status = InvoiceStatus.Draft, CustomerId = Guid.NewGuid(), IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        var otherInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = otherTenantId, IsDeleted = false, Status = InvoiceStatus.Draft, CustomerId = Guid.NewGuid(), IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { myInvoice, otherInvoice });

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(myInvoice.Id);
    }

    [Fact]
    public async Task Handle_DeletedInvoices_ShouldExcludeThem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var activeInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = tenantId, IsDeleted = false, Status = InvoiceStatus.Draft, CustomerId = Guid.NewGuid(), IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        var deletedInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = tenantId, IsDeleted = true, Status = InvoiceStatus.Draft, CustomerId = Guid.NewGuid(), IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { activeInvoice, deletedInvoice });

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(activeInvoice.Id);
    }

    // ── Filters ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CustomerIdFilter_ShouldReturnOnlyMatchingInvoices()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var targetCustomerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var matchInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = targetCustomerId, IsDeleted = false, Status = InvoiceStatus.Draft, IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        var otherInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = Guid.NewGuid(), IsDeleted = false, Status = InvoiceStatus.Draft, IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { matchInvoice, otherInvoice });

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery { CustomerId = targetCustomerId }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(matchInvoice.Id);
    }

    [Fact]
    public async Task Handle_StatusFilter_ShouldReturnOnlyMatchingInvoices()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var draftInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = tenantId, Status = InvoiceStatus.Draft, IsDeleted = false, CustomerId = Guid.NewGuid(), IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        var authorizedInvoice = new Invoice { Id = Guid.NewGuid(), TenantId = tenantId, Status = InvoiceStatus.Authorized, IsDeleted = false, CustomerId = Guid.NewGuid(), IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30) };
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { draftInvoice, authorizedInvoice });

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery { Status = InvoiceStatus.Draft }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(draftInvoice.Id);
    }

    [Fact]
    public async Task Handle_DateFromFilter_ShouldExcludeInvoicesBeforeDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cutoff = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (_, _, beforeInvoice) = BuildInvoiceWithRelations(tenantId, issuedDate: new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var (_, _, afterInvoice) = BuildInvoiceWithRelations(tenantId, issuedDate: new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { beforeInvoice, afterInvoice });

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery { DateFrom = cutoff }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(afterInvoice.Id);
    }

    [Fact]
    public async Task Handle_DateToFilter_ShouldExcludeInvoicesAfterDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cutoff = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (_, _, beforeInvoice) = BuildInvoiceWithRelations(tenantId, issuedDate: new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var (_, _, afterInvoice) = BuildInvoiceWithRelations(tenantId, issuedDate: new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { beforeInvoice, afterInvoice });

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery { DateTo = cutoff }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(beforeInvoice.Id);
    }

    // ── IsEditable flag ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DraftInvoice_IsEditableShouldBeTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (customer, emissionPoint, invoice) = BuildInvoiceWithRelations(tenantId, InvoiceStatus.Draft);
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { invoice });
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPoint.Id, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.First().IsEditable.Should().BeTrue();
    }

    [Theory]
    [InlineData(InvoiceStatus.Authorized)]
    [InlineData(InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Sent)]
    [InlineData(InvoiceStatus.Cancelled)]
    public async Task Handle_NonDraftInvoice_IsEditableShouldBeFalse(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var (customer, emissionPoint, invoice) = BuildInvoiceWithRelations(tenantId, status);
        _invoiceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Invoice> { invoice });
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPoint.Id, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.First().IsEditable.Should().BeFalse();
    }

    // ── No tenant ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(new GetAllInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
