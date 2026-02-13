using SaaS.Domain.Enums;

namespace SaaS.Application.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    // Emission Point (required for SRI Ecuador)
    public Guid? EmissionPointId { get; set; }
    public string? EmissionPointCode { get; set; }
    public string? EmissionPointName { get; set; }
    public string? EstablishmentCode { get; set; }

    // Ecuador SRI fields
    public string? SriAuthorization { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public DateTime? PaidDate { get; set; }

    // Items
    public List<InvoiceItemDto> Items { get; set; } = new();

    // Computed
    public bool IsEditable { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
