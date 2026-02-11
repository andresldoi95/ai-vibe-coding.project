using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Service for digitally signing XML documents with PKCS#12 certificates
/// </summary>
public class XmlSignatureService : IXmlSignatureService
{
    public async Task<string> SignXmlAsync(string xmlFilePath, byte[] certificate, string password)
    {
        try
        {
            // Load XML document
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(xmlFilePath);

            // Load certificate
            var cert = new X509Certificate2(certificate, password, X509KeyStorageFlags.Exportable);

            // Get RSA private key
            using var rsa = cert.GetRSAPrivateKey();
            if (rsa == null)
                throw new InvalidOperationException("Certificate does not contain RSA private key");

            // Create signed XML
            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = rsa
            };

            // Create reference to the document
            var reference = new Reference();
            reference.Uri = "#comprobante";
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            // Add key info
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            // Compute signature
            signedXml.ComputeSignature();

            // Get XML representation of signature
            var xmlDigitalSignature = signedXml.GetXml();

            // Append signature to document
            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

            // Save signed XML
            var signedFilePath = xmlFilePath.Replace(".xml", "_signed.xml");
            await Task.Run(() => xmlDoc.Save(signedFilePath));

            return signedFilePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to sign XML: {ex.Message}", ex);
        }
    }

    public async Task<bool> ValidateSignatureAsync(string signedXmlFilePath)
    {
        try
        {
            // Load signed XML
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(signedXmlFilePath);

            // Get signature node
            var signatureNode = xmlDoc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0] as XmlElement;
            if (signatureNode == null)
                return false;

            // Create SignedXml object
            var signedXml = new SignedXml(xmlDoc);
            signedXml.LoadXml(signatureNode);

            // Verify signature
            return await Task.Run(() => signedXml.CheckSignature());
        }
        catch
        {
            return false;
        }
    }

    public Task<CertificateInfo> GetCertificateInfoAsync(byte[] certificate, string password)
    {
        try
        {
            var cert = new X509Certificate2(certificate, password, X509KeyStorageFlags.Exportable);

            var info = new CertificateInfo
            {
                Subject = cert.Subject,
                Issuer = cert.Issuer,
                ValidFrom = cert.NotBefore.ToUniversalTime(),
                ValidTo = cert.NotAfter.ToUniversalTime()
            };

            return Task.FromResult(info);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read certificate: {ex.Message}", ex);
        }
    }
}
