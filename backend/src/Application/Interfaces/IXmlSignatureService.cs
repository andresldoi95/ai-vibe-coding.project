namespace SaaS.Application.Interfaces;

/// <summary>
/// Service for digitally signing XML documents with PKCS#12 certificates
/// </summary>
public interface IXmlSignatureService
{
    /// <summary>
    /// Signs an XML file with a digital certificate
    /// </summary>
    /// <param name="xmlFilePath">Path to the XML file to sign</param>
    /// <param name="certificate">Certificate binary data (.p12)</param>
    /// <param name="password">Certificate password</param>
    /// <returns>Path to the signed XML file</returns>
    Task<string> SignXmlAsync(string xmlFilePath, byte[] certificate, string password);

    /// <summary>
    /// Validates the digital signature of a signed XML file
    /// </summary>
    Task<bool> ValidateSignatureAsync(string signedXmlFilePath);

    /// <summary>
    /// Extracts certificate information from PKCS#12 file
    /// </summary>
    Task<CertificateInfo> GetCertificateInfoAsync(byte[] certificate, string password);
}

/// <summary>
/// Information about a digital certificate
/// </summary>
public class CertificateInfo
{
    public string Subject { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsValid => DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo;
}
