namespace SaaS.Domain.Enums;

public enum EmailType
{
    UserInvitation = 1,
    PasswordReset = 2,
    EmailVerification = 3,
    WelcomeEmail = 4,
    InvoiceCreated = 5,
    PaymentReceived = 6,
    LowStockAlert = 7,
    SubscriptionExpiring = 8,
    MonthlyReport = 9,
    Custom = 99
}
