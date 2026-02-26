using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Domain.Enums;

namespace SaaS.Application.Features.CreditNotes.Commands.DeleteCreditNote;

public class DeleteCreditNoteCommandHandler : IRequestHandler<DeleteCreditNoteCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteCreditNoteCommandHandler> _logger;

    public DeleteCreditNoteCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteCreditNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCreditNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is not set");

            var creditNote = await _unitOfWork.CreditNotes.GetByIdAsync(request.Id, cancellationToken);
            if (creditNote == null || creditNote.TenantId != tenantId)
                return Result<bool>.Failure("Credit note not found");

            if (creditNote.Status != InvoiceStatus.Draft)
                return Result<bool>.Failure("Only draft credit notes can be deleted");

            creditNote.IsDeleted = true;
            creditNote.DeletedAt = DateTime.UtcNow;
            creditNote.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CreditNotes.UpdateAsync(creditNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Credit note {Id} deleted for tenant {TenantId}", request.Id, tenantId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting credit note {Id}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the credit note");
        }
    }
}
