using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.Invoices.Commands.DeleteInvoice;

public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteInvoiceCommandHandler> _logger;

    public DeleteInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.Id, cancellationToken);
            if (invoice == null || invoice.TenantId != tenantId)
            {
                return Result<bool>.Failure("Invoice not found");
            }

            // Only draft invoices can be deleted
            if (invoice.Status != InvoiceStatus.Draft)
            {
                return Result<bool>.Failure("Only draft invoices can be deleted");
            }

            // Soft delete
            invoice.IsDeleted = true;
            invoice.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.Invoices.UpdateAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Invoice {InvoiceId} deleted successfully for tenant {TenantId}", request.Id, tenantId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {InvoiceId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the invoice");
        }
    }
}
