namespace SaaS.Application.DTOs;

/// <summary>
/// Data transfer object for Country entity
/// </summary>
public class CountryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Alpha3Code { get; set; }
    public string? NumericCode { get; set; }
    public bool IsActive { get; set; }
}
