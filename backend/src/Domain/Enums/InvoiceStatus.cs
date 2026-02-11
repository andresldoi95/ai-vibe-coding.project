namespace SaaS.Domain.Enums;

public enum InvoiceStatus
{
    Draft = 0,
    PendingSignature = 1,
    PendingAuthorization = 2,
    Authorized = 3,
    Rejected = 4,
    Sent = 5,
    Paid = 6,
    Overdue = 7,
    Cancelled = 8,
    Voided = 9
}
