using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Invoices.Queries.GetInvoiceById;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Invoices.Queries;

public class GetInvoiceByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetInvoiceByIdQueryHandler>> _loggerMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly GetInvoiceByIdQueryHandler _handler;

    public GetInvoiceByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetInvoiceByIdQueryHandler>>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();

        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);

        _handler = new GetInvoiceByIdQueryHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingInvoice_ShouldReturnInvoiceDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Test Customer"
        };

        var establishment = new Establishment
        {
            Id = establishmentId,
            TenantId = tenantId,
            EstablishmentCode = "001"
        };

        var emissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = tenantId,
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "Point 1",
            Establishment = establishment
        };

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Main Warehouse"
        };

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "001-001-000000001",
            CustomerId = customerId,
            EmissionPointId = emissionPointId,
            WarehouseId = warehouseId,
            Status = InvoiceStatus.Draft,
            IssueDate = DateTime.UtcNow,
            SubtotalAmount = 100.00m,
            TaxAmount = 12.00m,
            TotalAmount = 112.00m,
            IsDeleted = false,
            Customer = customer,
            EmissionPoint = emissionPoint,
            Warehouse = warehouse,
            Items = new List<InvoiceItem>()
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var query = new GetInvoiceByIdQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(invoiceId);
        result.Value.InvoiceNumber.Should().Be("001-001-000000001");
        result.Value.CustomerName.Should().Be("Test Customer");
        result.Value.EmissionPointCode.Should().Be("001");
        result.Value.WarehouseName.Should().Be("Main Warehouse");
        result.Value.IsEditable.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DraftNotDeletedInvoice_IsEditableShouldBeTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "001-001-000000001",
            Status = InvoiceStatus.Draft,
            IsDeleted = false,
            TotalAmount = 100.00m,
            Items = new List<InvoiceItem>()
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var query = new GetInvoiceByIdQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsEditable.Should().BeTrue();
    }

    [Theory]
    [InlineData(InvoiceStatus.Sent, false)]
    [InlineData(InvoiceStatus.Paid, false)]
    [InlineData(InvoiceStatus.Overdue, false)]
    [InlineData(InvoiceStatus.Cancelled, false)]
    public async Task Handle_NonDraftInvoice_IsEditableShouldBeFalse(InvoiceStatus status, bool isDeleted)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "001-001-000000001",
            Status = status,
            IsDeleted = isDeleted,
            TotalAmount = 100.00m,
            Items = new List<InvoiceItem>()
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var query = new GetInvoiceByIdQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsEditable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DeletedInvoice_IsEditableShouldBeFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "001-001-000000001",
            Status = InvoiceStatus.Draft,
            IsDeleted = true,
            TotalAmount = 100.00m,
            Items = new List<InvoiceItem>()
        };

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var query = new GetInvoiceByIdQuery(invoiceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsEditable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvoiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _invoiceRepositoryMock
            .Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var query = new GetInvoiceByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var query = new GetInvoiceByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}
