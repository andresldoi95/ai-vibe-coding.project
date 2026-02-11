namespace SaaS.Domain.Enums;

/// <summary>
/// SRI Ecuador document types for electronic invoicing
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Factura - Invoice (01)
    /// </summary>
    Invoice = 1,

    /// <summary>
    /// Liquidación de Compra (03)
    /// </summary>
    PurchaseLiquidation = 3,

    /// <summary>
    /// Nota de Crédito - Credit Note (04)
    /// </summary>
    CreditNote = 4,

    /// <summary>
    /// Nota de Débito - Debit Note (05)
    /// </summary>
    DebitNote = 5,

    /// <summary>
    /// Comprobante de Retención - Retention Receipt (07)
    /// </summary>
    Retention = 7
}
