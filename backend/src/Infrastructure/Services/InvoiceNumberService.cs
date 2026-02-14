using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Service for generating sequential invoice numbers in Ecuador format (001-001-000000123)
/// </summary>
public class InvoiceNumberService : IInvoiceNumberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceNumberService> _logger;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public InvoiceNumberService(IUnitOfWork unitOfWork, ILogger<InvoiceNumberService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Use semaphore to ensure thread-safe number generation
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // TODO: Re-implement invoice number service once InvoiceConfiguration entity is created
            // For now, use a simple sequential number
            _logger.LogWarning("Using simple sequential number for tenant {TenantId} - InvoiceConfiguration not implemented", tenantId);

            // Get last invoice number for this tenant
            var lastInvoice = (await _unitOfWork.Invoices.GetAllAsync(cancellationToken))
                .Where(i => i.TenantId == tenantId)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNumber))
            {
                // Try to extract number from invoice number (format: XXX-XXX-000000001)
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            // Generate invoice number with default codes
            var invoiceNumber = $"001-001-{nextNumber:D9}";

            // Save changes (transaction will be managed by the caller)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated invoice number {InvoiceNumber} for tenant {TenantId}", invoiceNumber, tenantId);

            return invoiceNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice number for tenant {TenantId}", tenantId);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool ValidateFormat(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return false;

        // Ecuador format: 001-001-000000123 (3-3-9 digits with dashes)
        var parts = invoiceNumber.Split('-');
        if (parts.Length != 3)
            return false;

        return parts[0].Length == 3 && parts[0].All(char.IsDigit)
            && parts[1].Length == 3 && parts[1].All(char.IsDigit)
            && parts[2].Length == 9 && parts[2].All(char.IsDigit);
    }
}
