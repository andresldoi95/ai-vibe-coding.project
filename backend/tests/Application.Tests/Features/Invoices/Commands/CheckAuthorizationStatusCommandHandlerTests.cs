using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Features.Invoices.Commands.CheckAuthorizationStatus;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.Invoices.Commands;

public class CheckAuthorizationStatusCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ISriWebServiceClient> _sriWebServiceClientMock;
    private readonly Mock<ILogger<CheckAuthorizationStatusCommandHandler>> _loggerMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<ISriErrorLogRepository> _sriErrorLogRepositoryMock;
    private readonly CheckAuthorizationStatusCommandHandler _handler;

    public CheckAuthorizationStatusCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _sriWebServiceClientMock = new Mock<ISriWebServiceClient>();
        _loggerMock = new Mock<ILogger<CheckAuthorizationStatusCommandHandler>>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _sriErrorLogRepositoryMock = new Mock<ISriErrorLogRepository>();

        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriErrorLogs).Returns(_sriErrorLogRepositoryMock.Object);

        _handler = new CheckAuthorizationStatusCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _sriWebServiceClientMock.Object,
            _loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Invoice lookup failures
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_InvoiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");
        _sriWebServiceClientMock.Verify(
            c => c.CheckAuthorizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvoiceFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice { Id = invoiceId, TenantId = otherTenantId, Status = InvoiceStatus.PendingAuthorization };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");
    }

    [Fact]
    public async Task Handle_InvoiceInWrongStatus_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.Draft  // not PendingAuthorization or Authorized
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cannot check authorization");
    }

    [Fact]
    public async Task Handle_NoAccessKey_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.PendingAuthorization,
            AccessKey = null
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access key not found");
        _sriWebServiceClientMock.Verify(
            c => c.CheckAuthorizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // Authorized response — invoice should be marked Authorized
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_SriAuthorized_ShouldUpdateInvoiceToAuthorized()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var authorizationNumber = "1234567890";
        var authorizationDate = DateTime.UtcNow;
        const string accessKey = "1234567890123456789012345678901234567890123456789";

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.PendingAuthorization,
            AccessKey = accessKey
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _sriWebServiceClientMock
            .Setup(c => c.CheckAuthorizationAsync(accessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = true,
                Status = "AUTORIZADO",
                AuthorizationNumber = authorizationNumber,
                AuthorizationDate = authorizationDate
            });

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsAuthorized.Should().BeTrue();
        result.Value.AuthorizationNumber.Should().Be(authorizationNumber);

        invoice.Status.Should().Be(InvoiceStatus.Authorized);
        invoice.SriAuthorization.Should().Be(authorizationNumber);
        invoice.AuthorizationDate.Should().BeCloseTo(authorizationDate, TimeSpan.FromSeconds(1));

        _invoiceRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Invoice>(i => i.Id == invoiceId && i.Status == InvoiceStatus.Authorized),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // EN PROCESAMIENTO — invoice stays PendingAuthorization, no save
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_SriEnProcesamiento_ShouldKeepPendingAndNotSave()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        const string accessKey = "1234567890123456789012345678901234567890123456789";

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.PendingAuthorization,
            AccessKey = accessKey
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _sriWebServiceClientMock
            .Setup(c => c.CheckAuthorizationAsync(accessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "EN PROCESAMIENTO"
            });

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.PendingAuthorization);

        _invoiceRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _sriErrorLogRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<SriErrorLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // Transient errors — invoice stays PendingAuthorization, no SriErrorLog created
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("INVALID_RESPONSE")]
    [InlineData("PARSE_ERROR")]
    public async Task Handle_TransientError_ShouldKeepPendingAndNotCreateErrorLog(string errorCode)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        const string accessKey = "1234567890123456789012345678901234567890123456789";

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.PendingAuthorization,
            AccessKey = accessKey
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _sriWebServiceClientMock
            .Setup(c => c.CheckAuthorizationAsync(accessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "ERROR",
                Errors = new List<SriError>
                {
                    new SriError { Code = errorCode, Message = "Transient connectivity issue" }
                }
            });

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — invoice stays in PendingAuthorization and nothing is persisted
        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.PendingAuthorization);

        _invoiceRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _sriErrorLogRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<SriErrorLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // Real SRI rejection — invoice marked Rejected, SriErrorLog entries created
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_RealSriRejection_ShouldMarkRejectedAndCreateErrorLogs()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        const string accessKey = "1234567890123456789012345678901234567890123456789";

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.PendingAuthorization,
            AccessKey = accessKey
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _sriWebServiceClientMock
            .Setup(c => c.CheckAuthorizationAsync(accessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "NO AUTORIZADO",
                Errors = new List<SriError>
                {
                    new SriError { Code = "035", Message = "CLAVE DE ACCESO REGISTRADA", AdditionalInfo = "Duplicate" },
                    new SriError { Code = "043", Message = "RUC del emisor no existe" }
                }
            });

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Rejected);

        _invoiceRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Invoice>(i => i.Id == invoiceId && i.Status == InvoiceStatus.Rejected),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // One SriErrorLog per error
        _sriErrorLogRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SriErrorLog>(l =>
                    l.InvoiceId == invoiceId &&
                    l.TenantId == tenantId &&
                    l.Operation == "CheckAuthorization" &&
                    l.ErrorCode == "035"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _sriErrorLogRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SriErrorLog>(l =>
                    l.InvoiceId == invoiceId &&
                    l.ErrorCode == "043"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Mixed transient + real errors — real errors dominate → Rejected
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_MixedTransientAndRealErrors_ShouldMarkRejected()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        const string accessKey = "1234567890123456789012345678901234567890123456789";

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.PendingAuthorization,
            AccessKey = accessKey
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _sriWebServiceClientMock
            .Setup(c => c.CheckAuthorizationAsync(accessKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SriAuthorizationResponse
            {
                IsAuthorized = false,
                Status = "NO AUTORIZADO",
                Errors = new List<SriError>
                {
                    new SriError { Code = "PARSE_ERROR", Message = "Could not parse" },
                    new SriError { Code = "035", Message = "ACCESS KEY REGISTERED" }  // real SRI error
                }
            });

        var command = new CheckAuthorizationStatusCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — not all errors are transient, so invoice should be Rejected
        invoice.Status.Should().Be(InvoiceStatus.Rejected);
        _sriErrorLogRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<SriErrorLog>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
