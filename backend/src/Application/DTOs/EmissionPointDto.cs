namespace SaaS.Application.DTOs;

public class EmissionPointDto
{
    public Guid Id { get; set; }
    public string EmissionPointCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int InvoiceSequence { get; set; }
    public int CreditNoteSequence { get; set; }
    public int DebitNoteSequence { get; set; }
    public int RetentionSequence { get; set; }
    public Guid EstablishmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEmissionPointDto
{
    public string EmissionPointCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid EstablishmentId { get; set; }
}

public class UpdateEmissionPointDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
