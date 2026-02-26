using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Features.CreditNotes.Commands.GenerateCreditNoteRide;
using SaaS.Application.Features.Invoices.Commands.GenerateRide;
using SaaS.Domain.Enums;

namespace SaaS.Application.Jobs;

/// <summary>
/// Background job that automatically generates RIDE PDFs for newly authorized invoices.
/// Runs every 60 seconds to find authorized invoices without RIDE PDFs.
/// </summary>
public class GenerateRideForAuthorizedInvoicesJob
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateRideForAuthorizedInvoicesJob> _logger;

    public GenerateRideForAuthorizedInvoicesJob(
        IInvoiceRepository invoiceRepository,
        ICreditNoteRepository creditNoteRepository,
        IMediator mediator,
        ILogger<GenerateRideForAuthorizedInvoicesJob> logger)
    {
        _invoiceRepository = invoiceRepository;
        _creditNoteRepository = creditNoteRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("GenerateRideForAuthorizedInvoicesJob started at {Time}", DateTime.UtcNow);

        try
        {
            // Get all authorized invoices across all tenants
            var authorizedInvoices = await _invoiceRepository.GetAllByStatusAsync(InvoiceStatus.Authorized);

            if (!authorizedInvoices.Any())
            {
                _logger.LogInformation("No authorized invoices found");
                return;
            }

            // Filter for those without RIDE PDF
            var invoicesNeedingRide = authorizedInvoices
                .Where(i => string.IsNullOrEmpty(i.RideFilePath))
                .ToList();

            if (!invoicesNeedingRide.Any())
            {
                _logger.LogInformation("All authorized invoices already have RIDE PDFs");
                return;
            }

            _logger.LogInformation("Found {Count} authorized invoices needing RIDE generation", invoicesNeedingRide.Count);

            var successCount = 0;
            var errorCount = 0;

            foreach (var invoice in invoicesNeedingRide)
            {
                // Validate invoice has required SRI data
                if (string.IsNullOrEmpty(invoice.AccessKey))
                {
                    _logger.LogWarning("Invoice {InvoiceId} is authorized but missing AccessKey", invoice.Id);
                    continue;
                }

                if (string.IsNullOrEmpty(invoice.SriAuthorization))
                {
                    _logger.LogWarning("Invoice {InvoiceId} is authorized but missing SriAuthorization", invoice.Id);
                    continue;
                }

                try
                {
                    var command = new GenerateRideCommand { InvoiceId = invoice.Id };
                    var result = await _mediator.Send(command);

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Successfully generated RIDE PDF for invoice {InvoiceId}. Path: {FilePath}",
                            invoice.Id, result.Value);
                        successCount++;
                    }
                    else
                    {
                        _logger.LogError("Failed to generate RIDE for invoice {InvoiceId}: {Error}",
                            invoice.Id, result.Error);
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating RIDE for invoice {InvoiceId}", invoice.Id);
                    errorCount++;
                }

                // Add small delay between generations to avoid file system contention
                await Task.Delay(200);
            }

            _logger.LogInformation(
                "GenerateRideForAuthorizedInvoicesJob (Invoices) completed. Success: {SuccessCount}, Errors: {ErrorCount}",
                successCount, errorCount);

            // ── Credit Notes ──────────────────────────────────────────────────
            await ProcessAuthorizedCreditNotesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in GenerateRideForAuthorizedInvoicesJob");
            throw;
        }
    }

    private async Task ProcessAuthorizedCreditNotesAsync()
    {
        var authorizedCreditNotes = await _creditNoteRepository.GetAllByStatusAsync(InvoiceStatus.Authorized);
        var creditNotesNeedingRide = authorizedCreditNotes
            .Where(cn => string.IsNullOrEmpty(cn.RideFilePath))
            .ToList();

        if (!creditNotesNeedingRide.Any())
        {
            _logger.LogInformation("All authorized credit notes already have RIDE PDFs");
            return;
        }

        _logger.LogInformation("Found {Count} authorized credit notes needing RIDE generation", creditNotesNeedingRide.Count);

        var successCount = 0;
        var errorCount = 0;

        foreach (var creditNote in creditNotesNeedingRide)
        {
            if (string.IsNullOrEmpty(creditNote.AccessKey) || string.IsNullOrEmpty(creditNote.SriAuthorization))
            {
                _logger.LogWarning("CreditNote {CreditNoteId} is authorized but missing AccessKey or SriAuthorization", creditNote.Id);
                continue;
            }

            try
            {
                var command = new GenerateCreditNoteRideCommand { CreditNoteId = creditNote.Id };
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully generated RIDE PDF for credit note {CreditNoteId}. Path: {FilePath}",
                        creditNote.Id, result.Value);
                    successCount++;
                }
                else
                {
                    _logger.LogError("Failed to generate RIDE for credit note {CreditNoteId}: {Error}",
                        creditNote.Id, result.Error);
                    errorCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating RIDE for credit note {CreditNoteId}", creditNote.Id);
                errorCount++;
            }

            await Task.Delay(200);
        }

        _logger.LogInformation(
            "GenerateRideForAuthorizedInvoicesJob (CreditNotes) completed. Success: {SuccessCount}, Errors: {ErrorCount}",
            successCount, errorCount);    }
}
