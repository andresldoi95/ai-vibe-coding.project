namespace SaaS.Domain.Enums;

/// <summary>
/// SRI Ecuador payment methods
/// </summary>
public enum SriPaymentMethod
{
    /// <summary>
    /// Sin utilización del sistema financiero - Cash/No financial system (01)
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Cheque - Check (02)
    /// </summary>
    Check = 2,

    /// <summary>
    /// Transferencia bancaria - Bank Transfer (03)
    /// </summary>
    BankTransfer = 3,

    /// <summary>
    /// Depósito en cuenta - Account Deposit (04)
    /// </summary>
    AccountDeposit = 4,

    /// <summary>
    /// Tarjeta de débito - Debit Card (16)
    /// </summary>
    DebitCard = 16,

    /// <summary>
    /// Dinero electrónico - Electronic Money (17)
    /// </summary>
    ElectronicMoney = 17,

    /// <summary>
    /// Tarjeta prepago - Prepaid Card (18)
    /// </summary>
    PrepaidCard = 18,

    /// <summary>
    /// Tarjeta de crédito - Credit Card (19)
    /// </summary>
    CreditCard = 19,

    /// <summary>
    /// Otros con utilización del sistema financiero - Other (20)
    /// </summary>
    Other = 20,

    /// <summary>
    /// Endoso de títulos - Title Endorsement (21)
    /// </summary>
    TitleEndorsement = 21
}
