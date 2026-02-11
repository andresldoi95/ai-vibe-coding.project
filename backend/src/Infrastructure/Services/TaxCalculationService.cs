using SaaS.Application.Common.Interfaces;
using SaaS.Application.DTOs;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Tax calculation service for Ecuador IVA rates
/// </summary>
public class TaxCalculationService : ITaxCalculationService
{
    public (decimal subtotal, decimal tax, decimal total) CalculateInvoiceTotals(IEnumerable<InvoiceItemDto> items)
    {
        decimal subtotal = 0;
        decimal tax = 0;

        foreach (var item in items)
        {
            var lineItemTotals = CalculateLineItemTotals(item.Quantity, item.UnitPrice, item.TaxRate);
            subtotal += lineItemTotals.subtotal;
            tax += lineItemTotals.tax;
        }

        // Round to 2 decimal places using banker's rounding
        subtotal = decimal.Round(subtotal, 2, MidpointRounding.ToEven);
        tax = decimal.Round(tax, 2, MidpointRounding.ToEven);
        var total = decimal.Round(subtotal + tax, 2, MidpointRounding.ToEven);

        return (subtotal, tax, total);
    }

    public (decimal subtotal, decimal tax, decimal total) CalculateLineItemTotals(int quantity, decimal unitPrice, decimal taxRate)
    {
        // Calculate subtotal (quantity * unit price)
        var subtotal = decimal.Round(quantity * unitPrice, 2, MidpointRounding.ToEven);

        // Calculate tax (subtotal * tax rate)
        var tax = decimal.Round(subtotal * taxRate, 2, MidpointRounding.ToEven);

        // Calculate total
        var total = decimal.Round(subtotal + tax, 2, MidpointRounding.ToEven);

        return (subtotal, tax, total);
    }
}
