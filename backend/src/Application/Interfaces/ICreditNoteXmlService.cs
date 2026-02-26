using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

/// <summary>
/// Service for generating SRI-compliant XML files for electronic credit notes (Nota de Crédito)
/// </summary>
public interface ICreditNoteXmlService
{
    /// <summary>
    /// Generates XML file for a credit note
    /// </summary>
    /// <param name="creditNote">Credit note entity with all required data</param>
    /// <param name="sriConfiguration">SRI configuration for the company</param>
    /// <param name="establishment">Establishment details</param>
    /// <param name="emissionPoint">Emission point details</param>
    /// <returns>Tuple with the file path and access key</returns>
    Task<(string FilePath, string AccessKey)> GenerateCreditNoteXmlAsync(
        CreditNote creditNote,
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
