using SaaS.Application.DTOs;

namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Service for calculating taxes on invoices
/// </summary>
public interface ITaxCalculationService
{
    /// <summary>
    /// Calculate invoice totals from line items
    /// </summary>
    (decimal subtotal, decimal tax, decimal total) CalculateInvoiceTotals(IEnumerable<InvoiceItemDto> items);

    /// <summary>
    /// Calculate line item totals
    /// </summary>
    (decimal subtotal, decimal tax, decimal total) CalculateLineItemTotals(int quantity, decimal unitPrice, decimal taxRate);
}
