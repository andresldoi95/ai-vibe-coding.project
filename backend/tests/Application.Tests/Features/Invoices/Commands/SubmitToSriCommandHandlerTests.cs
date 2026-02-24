using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Features.Invoices.Commands.SubmitToSri;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.Invoices.Commands;

public class SubmitToSriCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ISriWebServiceClient> _sriWebServiceClientMock;
    private readonly Mock<ILogger<SubmitToSriCommandHandler>> _loggerMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<ISriErrorLogRepository> _sriErrorLogRepositoryMock;
    private readonly SubmitToSriCommandHandler _handler;

    public SubmitToSriCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _sriWebServiceClientMock = new Mock<ISriWebServiceClient>();
        _loggerMock = new Mock<ILogger<SubmitToSriCommandHandler>>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _sriErrorLogRepositoryMock = new Mock<ISriErrorLogRepository>();

        _unitOfWorkMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SriErrorLogs).Returns(_sriErrorLogRepositoryMock.Object);

        _handler = new SubmitToSriCommandHandler(
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

        var command = new SubmitToSriCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");
        _sriWebServiceClientMock.Verify(
            c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
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

        var invoice = new Invoice { Id = invoiceId, TenantId = otherTenantId };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new SubmitToSriCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invoice not found");
    }

    // -----------------------------------------------------------------------
    // Wrong status
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_InvoiceNotPendingAuthorization_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.Draft
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new SubmitToSriCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("PendingAuthorization");
        _sriWebServiceClientMock.Verify(
            c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_RejectedInvoiceWithErrorLogs_ShouldReturnFailureWithRejectionReasons()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

        var invoice = new Invoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            Status = InvoiceStatus.Rejected
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var errorLogs = new List<SriErrorLog>
        {
            new SriErrorLog
            {
                InvoiceId = invoiceId,
                TenantId = tenantId,
                ErrorCode = "035",
                ErrorMessage = "CLAVE DE ACCESO REGISTRADA"
            },
            new SriErrorLog
            {
                InvoiceId = invoiceId,
                TenantId = tenantId,
                ErrorCode = "043",
                ErrorMessage = "RUC del emisor no existe"
            }
        };

        _sriErrorLogRepositoryMock
            .Setup(r => r.GetByInvoiceIdAsync(invoiceId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorLogs);

        var command = new SubmitToSriCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("rejected by SRI");
        result.Error.Should().Contain("035");
        result.Error.Should().Contain("CLAVE DE ACCESO REGISTRADA");
    }

    // -----------------------------------------------------------------------
    // Signed XML file issues
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_MissingSignedXmlPath_ShouldReturnFailure()
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
            SignedXmlFilePath = null
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new SubmitToSriCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Signed XML file not found");
    }

    [Fact]
    public async Task Handle_SignedXmlFileNotOnDisk_ShouldReturnFailure()
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
            SignedXmlFilePath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xml")
        };
        _invoiceRepositoryMock
            .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var command = new SubmitToSriCommand { InvoiceId = invoiceId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Signed XML file not found on disk");
    }

    // -----------------------------------------------------------------------
    // SRI web service responses
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_SriSubmissionFails_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFile, "<signed-xml>test</signed-xml>");

            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

            var invoice = new Invoice
            {
                Id = invoiceId,
                TenantId = tenantId,
                Status = InvoiceStatus.PendingAuthorization,
                SignedXmlFilePath = tempFile
            };
            _invoiceRepositoryMock
                .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            _sriWebServiceClientMock
                .Setup(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SriSubmissionResponse
                {
                    IsSuccess = false,
                    Errors = new List<SriError>
                    {
                        new SriError { Code = "35", Message = "CLAVE DE ACCESO REGISTRADA" }
                    }
                });

            var command = new SubmitToSriCommand { InvoiceId = invoiceId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("SRI submission failed");
            result.Error.Should().Contain("35");
            result.Error.Should().Contain("CLAVE DE ACCESO REGISTRADA");

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Handle_SuccessfulSubmission_ShouldUpdateInvoiceAndReturnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFile, "<signed-xml>valid</signed-xml>");

            _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);

            var invoice = new Invoice
            {
                Id = invoiceId,
                TenantId = tenantId,
                Status = InvoiceStatus.PendingAuthorization,
                SignedXmlFilePath = tempFile
            };
            _invoiceRepositoryMock
                .Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            _sriWebServiceClientMock
                .Setup(c => c.SubmitDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SriSubmissionResponse
                {
                    IsSuccess = true,
                    Message = "RECIBIDA"
                });

            var command = new SubmitToSriCommand { InvoiceId = invoiceId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Contain("submitted successfully");

            _invoiceRepositoryMock.Verify(
                r => r.UpdateAsync(
                    It.Is<Invoice>(i => i.Id == invoiceId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
