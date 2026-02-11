using SaaS.Domain.Enums;

namespace SaaS.Application.DTOs;

public class SriConfigurationDto
{
    public Guid Id { get; set; }
    public string CompanyRuc { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string MainAddress { get; set; } = string.Empty;
    public bool AccountingRequired { get; set; }
    public SriEnvironment Environment { get; set; }
    public bool IsCertificateConfigured { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }
    public bool IsCertificateValid { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateSriConfigurationDto
{
    public string CompanyRuc { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string MainAddress { get; set; } = string.Empty;
    public bool AccountingRequired { get; set; }
    public SriEnvironment Environment { get; set; }
}

public class UploadCertificateDto
{
    public byte[] CertificateData { get; set; } = Array.Empty<byte>();
    public string Password { get; set; } = string.Empty;
}
