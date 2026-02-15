using SaaS.Domain.Common;
using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

/// <summary>
/// SRI (Servicio de Rentas Internas) configuration for a tenant.
/// Stores tenant tax information and digital certificate for electronic invoicing.
/// </summary>
public class SriConfiguration : TenantEntity
{
    /// <summary>
    /// Company RUC (Registro Único de Contribuyentes) - 13 digits
    /// </summary>
    public string CompanyRuc { get; set; } = string.Empty;

    /// <summary>
    /// Legal name of the company (Razón Social)
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Trade name of the company (Nombre Comercial)
    /// </summary>
    public string TradeName { get; set; } = string.Empty;

    /// <summary>
    /// Main address (Dirección Matriz)
    /// </summary>
    public string MainAddress { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether the company is required to keep accounting records
    /// </summary>
    public bool AccountingRequired { get; set; } = true;

    /// <summary>
    /// Special taxpayer resolution number (Contribuyente Especial)
    /// </summary>
    public string? SpecialTaxpayerNumber { get; set; }

    /// <summary>
    /// Whether the company is in the RISE regime (Régimen Impositivo Simplificado Ecuatoriano)
    /// </summary>
    public bool IsRiseRegime { get; set; } = false;

    /// <summary>
    /// SRI environment (Test or Production)
    /// </summary>
    public SriEnvironment Environment { get; set; } = SriEnvironment.Test;

    /// <summary>
    /// Digital certificate (.p12) binary data
    /// </summary>
    public byte[]? DigitalCertificate { get; set; }

    /// <summary>
    /// Encrypted password for the digital certificate
    /// </summary>
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// Certificate expiration date
    /// </summary>
    public DateTime? CertificateExpiryDate { get; set; }

    /// <summary>
    /// Whether the certificate has been uploaded and configured
    /// </summary>
    public bool IsCertificateConfigured => DigitalCertificate != null && !string.IsNullOrEmpty(CertificatePassword);

    /// <summary>
    /// Whether the certificate is still valid (not expired)
    /// </summary>
    public bool IsCertificateValid => CertificateExpiryDate.HasValue && CertificateExpiryDate.Value > DateTime.UtcNow;

    /// <summary>
    /// Validates the RUC format (13 digits)
    /// </summary>
    public bool IsValidRuc()
    {
        if (string.IsNullOrWhiteSpace(CompanyRuc))
            return false;

        if (CompanyRuc.Length != 13)
            return false;

        if (!CompanyRuc.All(char.IsDigit))
            return false;

        return true;
    }
}
