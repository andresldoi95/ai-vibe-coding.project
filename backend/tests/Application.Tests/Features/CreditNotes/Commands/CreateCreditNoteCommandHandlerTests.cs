using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;
using SaaS.Application.Features.CreditNotes.Commands.CreateCreditNote;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.CreditNotes.Commands;

public class CreateCreditNoteCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ITaxCalculationService> _taxCalculationServiceMock;
    private readonly Mock<ILogger<CreateCreditNoteCommandHandler>> _loggerMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IEmissionPointRepository> _emissionPointRepositoryMock;
    private readonly Mock<ISriConfigurationRepository> _sriConfigRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ITaxRateRepository> _taxRateRepositoryMock;
    private readonly Mock<ICreditNoteRepository> _creditNoteRepositoryMock;
    private readonly Mock<IRepository<CreditNoteItem>> _creditNoteItemRepositoryMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly CreateCreditNoteCommandHandler _handler;

    public CreateCreditNoteCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _taxCalculationServiceMock = new Mock<ITaxCalculationService>();
        _loggerMock = new Mock<ILogger<CreateCreditNoteCommandHandler>>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _emissionPointRepositoryMock = new Mock<IEmissionPointRepository>();
        _sriConfigRepositoryMock = new Mock<ISriConfigurationRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _taxRateRepositoryMock = new Mock<ITaxRateRepository>();
        _creditNoteRepositoryMock = new Mock<ICreditNoteRepository>();
        _creditNoteItemRepositoryMock = new Mock<IRepository<CreditNoteItem>>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.EmissionPoints).Returns(_emissionPointRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriConfigurations).Returns(_sriConfigRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TaxRates).Returns(_taxRateRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CreditNotes).Returns(_creditNoteRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CreditNoteItems).Returns(_creditNoteItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.StockMovements).Returns(_stockMovementRepositoryMock.Object);

        _handler = new CreateCreditNoteCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _taxCalculationServiceMock.Object,
            _loggerMock.Object);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateCreditNote()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "Acme Corp" };
        var establishment = new Establishment { Id = establishmentId, TenantId = tenantId, EstablishmentCode = "001" };
        var emissionPoint = new EmissionPoint
        {
            Id = emissionPointId,
            TenantId = tenantId,
            EstablishmentId = establishmentId,
            EmissionPointCode = "001",
            Name = "Main Point",
            CreditNoteSequence = 0,
            IsActive = true,
            Establishment = establishment
        };
        var originalInvoice = new Invoice
        {
            Id = originalInvoiceId,
            TenantId = tenantId,
            CustomerId = customerId,
            InvoiceNumber = "001-001-000000010",
            Status = InvoiceStatus.Authorized,
            PaymentMethod = SriPaymentMethod.Cash,
            Items = new List<InvoiceItem>()
        };
        var product = new Product { Id = productId, TenantId = tenantId, Name = "Product A", Code = "PA-001", UnitPrice = 100.00m };
        var taxRate = new TaxRate { Id = taxRateId, TenantId = tenantId, Name = "IVA 15%", Rate = 15.00m };
        var sriConfig = new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001", Environment = SriEnvironment.Test };

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(originalInvoice);
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>())).ReturnsAsync(emissionPoint);
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(sriConfig);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _taxRateRepositoryMock.Setup(r => r.GetByIdAsync(taxRateId, It.IsAny<CancellationToken>())).ReturnsAsync(taxRate);

        _taxCalculationServiceMock.Setup(s => s.CalculateLineItemTotals(2, 100.00m, 15.00m)).Returns((200.00m, 30.00m, 230.00m));
        _taxCalculationServiceMock.Setup(s => s.CalculateInvoiceTotals(It.IsAny<List<InvoiceItemDto>>())).Returns((200.00m, 30.00m, 230.00m));

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = emissionPointId,
            Reason = "Devolución por defecto",
            Items = new List<CreateCreditNoteItemDto>
            {
                new() { ProductId = productId, Quantity = 2, TaxRateId = taxRateId, Description = "Product A" }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CreditNoteNumber.Should().Be("001-001-000000001");
        result.Value.Status.Should().Be(InvoiceStatus.Draft);
        result.Value.TotalAmount.Should().Be(230.00m);
        result.Value.CustomerName.Should().Be("Acme Corp");
        result.Value.OriginalInvoiceNumber.Should().Be("001-001-000000010");
        result.Value.Reason.Should().Be("Devolución por defecto");
        result.Value.Items.Should().HaveCount(1);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _creditNoteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Tenant guard ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = Guid.NewGuid(),
            OriginalInvoiceId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        _creditNoteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Customer validation ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = Guid.NewGuid(),
            OriginalInvoiceId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Customer not found");
    }

    [Fact]
    public async Task Handle_CustomerFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var customer = new Customer { Id = Guid.NewGuid(), TenantId = otherTenantId, Name = "Other Tenant Customer" };
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customer.Id,
            OriginalInvoiceId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Customer not found");
    }

    // ── Original invoice validation ───────────────────────────────────────────

    [Fact]
    public async Task Handle_OriginalInvoiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId, Name = "Acme" });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = Guid.NewGuid(),
            EmissionPointId = Guid.NewGuid(),
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Original invoice not found");
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.PendingSignature)]
    [InlineData(InvoiceStatus.PendingAuthorization)]
    [InlineData(InvoiceStatus.Rejected)]
    [InlineData(InvoiceStatus.Cancelled)]
    public async Task Handle_OriginalInvoiceNotAuthorized_ShouldReturnFailure(InvoiceStatus status)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice
            {
                Id = originalInvoiceId,
                TenantId = tenantId,
                CustomerId = customerId,
                Status = status,
                Items = new List<InvoiceItem>()
            });

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = Guid.NewGuid(),
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("authorized invoices");
    }

    [Fact]
    public async Task Handle_CustomerDoesNotMatchOriginalInvoice_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice
            {
                Id = originalInvoiceId,
                TenantId = tenantId,
                CustomerId = otherCustomerId,  // different customer
                Status = InvoiceStatus.Authorized,
                Items = new List<InvoiceItem>()
            });

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = Guid.NewGuid(),
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Customer does not match the original invoice");
    }

    // ── EmissionPoint validation ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmissionPointNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice { Id = originalInvoiceId, TenantId = tenantId, CustomerId = customerId, Status = InvoiceStatus.Authorized, Items = new List<InvoiceItem>() });
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmissionPoint?)null);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = Guid.NewGuid(),
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Emission point not found");
    }

    [Fact]
    public async Task Handle_InactiveEmissionPoint_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice { Id = originalInvoiceId, TenantId = tenantId, CustomerId = customerId, Status = InvoiceStatus.Authorized, Items = new List<InvoiceItem>() });
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionPoint { Id = emissionPointId, TenantId = tenantId, IsActive = false });

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = emissionPointId,
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not active");
    }

    [Fact]
    public async Task Handle_EstablishmentNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice { Id = originalInvoiceId, TenantId = tenantId, CustomerId = customerId, Status = InvoiceStatus.Authorized, Items = new List<InvoiceItem>() });
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionPoint
            {
                Id = emissionPointId,
                TenantId = tenantId,
                IsActive = true,
                Establishment = null  // no establishment
            });

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = emissionPointId,
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Establishment not found");
    }

    // ── SRI Config validation ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SriConfigNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice { Id = originalInvoiceId, TenantId = tenantId, CustomerId = customerId, Status = InvoiceStatus.Authorized, Items = new List<InvoiceItem>() });
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionPoint
            {
                Id = emissionPointId,
                TenantId = tenantId,
                IsActive = true,
                Establishment = new Establishment { Id = establishmentId, TenantId = tenantId, EstablishmentCode = "001" }
            });
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SaaS.Domain.Entities.SriConfiguration?)null);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = emissionPointId,
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("SRI configuration not found");
    }

    // ── Item validation ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductNotFound_ShouldRollBackAndReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice { Id = originalInvoiceId, TenantId = tenantId, CustomerId = customerId, Status = InvoiceStatus.Authorized, Items = new List<InvoiceItem>() });
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionPoint
            {
                Id = emissionPointId,
                TenantId = tenantId,
                IsActive = true,
                Establishment = new Establishment { Id = establishmentId, EstablishmentCode = "001" }
            });
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001" });
        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = emissionPointId,
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1, TaxRateId = taxRateId, Description = "Test item" }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Product not found");
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TaxRateNotFound_ShouldRollBackAndReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var originalInvoiceId = Guid.NewGuid();
        var emissionPointId = Guid.NewGuid();
        var establishmentId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer { Id = customerId, TenantId = tenantId });
        _invoiceRepositoryMock.Setup(r => r.GetWithItemsAsync(originalInvoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice { Id = originalInvoiceId, TenantId = tenantId, CustomerId = customerId, Status = InvoiceStatus.Authorized, Items = new List<InvoiceItem>() });
        _emissionPointRepositoryMock.Setup(r => r.GetByIdAsync(emissionPointId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmissionPoint
            {
                Id = emissionPointId,
                TenantId = tenantId,
                IsActive = true,
                Establishment = new Establishment { Id = establishmentId, EstablishmentCode = "001" }
            });
        _sriConfigRepositoryMock.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SaaS.Domain.Entities.SriConfiguration { TenantId = tenantId, CompanyRuc = "1234567890001" });
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId, TenantId = tenantId, Name = "Product A", Code = "PA-001", UnitPrice = 100.00m });
        _taxRateRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRate?)null);

        var command = new CreateCreditNoteCommand
        {
            CustomerId = customerId,
            OriginalInvoiceId = originalInvoiceId,
            EmissionPointId = emissionPointId,
            Reason = "Test",
            Items = new List<CreateCreditNoteItemDto>
            {
                new() { ProductId = productId, Quantity = 1, TaxRateId = Guid.NewGuid(), Description = "Test item" }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tax rate not found");
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
