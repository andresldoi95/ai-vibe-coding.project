namespace SaaS.Application.DTOs;

public class SriErrorLogDto
{
    public Guid Id { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? AdditionalData { get; set; }
    public DateTime OccurredAt { get; set; }
}
