using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs.Sri;
using SaaS.Application.Features.Invoices.Commands.CheckAuthorizationStatus;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.Jobs;

/// <summary>
/// Background job that checks SRI authorization status for invoices pending authorization.
/// Runs every 30 seconds to poll SRI web services for authorization responses.
/// </summary>
public class CheckPendingAuthorizationsJob
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<CheckPendingAuthorizationsJob> _logger;

    public CheckPendingAuthorizationsJob(
        IInvoiceRepository invoiceRepository,
        IMediator mediator,
        ILogger<CheckPendingAuthorizationsJob> logger)
    {
        _invoiceRepository = invoiceRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("CheckPendingAuthorizationsJob started at {Time}", DateTime.UtcNow);

        try
        {
            // Get all invoices pending authorization across all tenants
            var pendingInvoices = await _invoiceRepository.GetAllByStatusAsync(InvoiceStatus.PendingAuthorization);

            if (!pendingInvoices.Any())
            {
                _logger.LogInformation("No invoices pending authorization found");
                return;
            }

            _logger.LogInformation("Found {Count} invoices pending authorization", pendingInvoices.Count);

            var successCount = 0;
            var errorCount = 0;

            foreach (var invoice in pendingInvoices)
            {
                // Only check invoices that have been submitted (have AccessKey)
                if (string.IsNullOrEmpty(invoice.AccessKey))
                {
                    _logger.LogWarning("Invoice {InvoiceId} has PendingAuthorization status but no AccessKey", invoice.Id);
                    continue;
                }

                try
                {
                    var command = new CheckAuthorizationStatusCommand { InvoiceId = invoice.Id };
                    var result = await _mediator.Send(command);

                    if (result.IsSuccess && result.Value != null)
                    {
                        var response = result.Value;

                        if (response.IsAuthorized && response.AuthorizationNumber != null)
                        {
                            _logger.LogInformation("Invoice {InvoiceId} was authorized by SRI. Authorization: {AuthNumber}",
                                invoice.Id, response.AuthorizationNumber);
                            successCount++;
                        }
                        else if (!response.IsAuthorized && response.Errors.Any())
                        {
                            _logger.LogWarning("Invoice {InvoiceId} was rejected by SRI", invoice.Id);
                            errorCount++;
                        }
                        else if (response.Status == "EN PROCESAMIENTO")
                        {
                            _logger.LogInformation("Invoice {InvoiceId} is still pending authorization", invoice.Id);
                        }
                        else
                        {
                            _logger.LogInformation("Invoice {InvoiceId} status from SRI: {Status}", invoice.Id, response.Status);
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to check authorization for invoice {InvoiceId}: {Error}",
                            invoice.Id, result.Error);
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking authorization for invoice {InvoiceId}", invoice.Id);
                    errorCount++;
                }

                // Add small delay between checks to avoid overwhelming SRI servers
                await Task.Delay(500);
            }

            _logger.LogInformation(
                "CheckPendingAuthorizationsJob completed. Success: {SuccessCount}, Errors: {ErrorCount}, Still Pending: {PendingCount}",
                successCount, errorCount, pendingInvoices.Count - successCount - errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in CheckPendingAuthorizationsJob");
            throw;
        }
    }
}
