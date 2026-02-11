using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

/// <summary>
/// Service for generating SRI-compliant XML files for electronic invoices
/// </summary>
public interface IInvoiceXmlService
{
    /// <summary>
    /// Generates XML file for an invoice
    /// </summary>
    /// <param name="invoice">Invoice entity with all required data</param>
    /// <param name="sriConfiguration">SRI configuration for the company</param>
    /// <param name="establishment">Establishment details</param>
    /// <param name="emissionPoint">Emission point details</param>
    /// <returns>Path to the generated XML file</returns>
    Task<string> GenerateInvoiceXmlAsync(
        Invoice invoice,
        SriConfiguration sriConfiguration,
        Establishment establishment,
        EmissionPoint emissionPoint);

    /// <summary>
    /// Validates XML content against SRI schema
    /// </summary>
    Task<bool> ValidateXmlAsync(string xmlFilePath);

    /// <summary>
    /// Reads and returns XML content from file
    /// </summary>
    Task<string> ReadXmlContentAsync(string xmlFilePath);
}
