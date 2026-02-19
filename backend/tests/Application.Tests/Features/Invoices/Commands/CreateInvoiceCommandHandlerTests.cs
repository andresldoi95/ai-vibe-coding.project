using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using SaaS.Application.Features.Invoices.Commands.CreateInvoice;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Application.Tests.Features.Invoices.Commands;

public class CreateInvoiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IInvoiceNumberService> _invoiceNumberServiceMock;
    private readonly Mock<ITaxCalculationService> _taxCalculationServiceMock;
    private readonly Mock<ILogger<CreateInvoiceCommandHandler>> _loggerMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ITaxRateRepository> _taxRateRepositoryMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IRepository<InvoiceItem>> _invoiceItemRepositoryMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly CreateInvoiceCommandHandler _handler;

    public CreateInvoiceCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _invoiceNumberServiceMock = new Mock<IInvoiceNumberService>();
        _taxCalculationServiceMock = new Mock<ITaxCalculationService>();
        _loggerMock = new Mock<ILogger<CreateInvoiceCommandHandler>>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _taxRateRepositoryMock = new Mock<ITaxRateRepository>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _invoiceItemRepositoryMock = new Mock<IRepository<InvoiceItem>>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TaxRates).Returns(_taxRateRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.InvoiceItems).Returns(_invoiceItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);

        _handler = new CreateInvoiceCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _invoiceNumberServiceMock.Object,
            _taxCalculationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateInvoiceWithItems()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Test Customer",
            Email = "test@example.com"
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
            InvoiceSequence = 100,
            IsActive = true,
            Establishment = establishment
        };

        var product = new Product
        {
            Id = productId,
            TenantId = tenantId,
            Name = "Product 1",
            Code = "PROD-001",
            UnitPrice = 100.00m
        };

        var taxRate = new TaxRate
        {
            Id = taxRateId,
            TenantId = tenantId,
            Name = "IVA 12%",
            Rate = 12.00m
        };

        var sriConfig = new SaaS.Domain.Entities.SriConfiguration
        {
            TenantId = tenantId,
            CompanyRuc = "1234567890001"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoint);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _taxRateRepositoryMock
            .Setup(r => r.GetByIdAsync(taxRateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRate);

        _sriConfigRepositoryMock
            .Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sriConfig);

        _taxCalculationServiceMock
            .Setup(s => s.CalculateLineItemTotals(2, 100.00m, 12.00m))
            .Returns((200.00m, 24.00m, 224.00m));

        _taxCalculationServiceMock
            .Setup(s => s.CalculateInvoiceTotals(It.IsAny<List<InvoiceItemDto>>()))
            .Returns((200.00m, 24.00m, 224.00m));

        var command = new CreateInvoiceCommand
        {
            CustomerId = customerId,
            EmissionPointId = emissionPointId,
            WarehouseId = warehouseId,
            Items = new List<CreateInvoiceItemDto>
            {
                new CreateInvoiceItemDto
                {
                    ProductId = productId,
                    Quantity = 2,
                    TaxRateId = taxRateId,
                    Description = "Product Description"
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InvoiceNumber.Should().Be("001-001-000000101");
        result.Value.Status.Should().Be(InvoiceStatus.Draft);
        result.Value.TotalAmount.Should().Be(224.00m);
        result.Value.Items.Should().HaveCount(1);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _invoiceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
        _invoiceItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<InvoiceItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _stockMovementRepositoryMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Customer not found");

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _invoiceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveEmissionPoint_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Name = "Test Customer"
        };

        var emissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = tenantId,
            IsActive = false
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _emissionPointRepositoryMock
            .Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emissionPoint);

        var command = new CreateInvoiceCommand
        {
            CustomerId = customerId,
            EmissionPointId = emissionPointId,
            Items = new List<CreateInvoiceItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not active");
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateInvoiceCommand
        {
            CustomerId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Items = new List<CreateInvoiceItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}
