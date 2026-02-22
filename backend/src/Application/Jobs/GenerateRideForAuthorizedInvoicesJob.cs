using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
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
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateRideForAuthorizedInvoicesJob> _logger;

    public GenerateRideForAuthorizedInvoicesJob(
        IInvoiceRepository invoiceRepository,
        IMediator mediator,
        ILogger<GenerateRideForAuthorizedInvoicesJob> logger)
    {
        _invoiceRepository = invoiceRepository;
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
                "GenerateRideForAuthorizedInvoicesJob completed. Success: {SuccessCount}, Errors: {ErrorCount}",
                successCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in GenerateRideForAuthorizedInvoicesJob");
            throw;
        }
    }
}
