using SaaS.Domain.Common;

namespace SaaS.Domain.Entities;

/// <summary>
/// Represents a physical establishment where business operations occur.
/// Required by SRI Ecuador for electronic invoice generation.
/// Each establishment has a unique 3-digit code.
/// </summary>
public class Establishment : TenantEntity
{
    /// <summary>
    /// 3-digit establishment code (001-999)
    /// </summary>
    public string EstablishmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Establishment name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical address of the establishment
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Whether this establishment is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Emission points within this establishment
    /// </summary>
    public ICollection<EmissionPoint> EmissionPoints { get; set; } = new List<EmissionPoint>();

    /// <summary>
    /// Validates the establishment code format (must be exactly 3 digits)
    /// </summary>
    public bool IsValidCode()
    {
        if (string.IsNullOrWhiteSpace(EstablishmentCode))
            return false;

        if (EstablishmentCode.Length != 3)
            return false;

        if (!EstablishmentCode.All(char.IsDigit))
            return false;

        var code = int.Parse(EstablishmentCode);
        return code >= 1 && code <= 999;
    }
}
