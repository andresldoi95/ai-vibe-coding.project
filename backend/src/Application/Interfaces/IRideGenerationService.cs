using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

/// <summary>
/// Service for generating RIDE (Representación Impresa del Comprobante Electrónico) PDFs
/// </summary>
public interface IRideGenerationService
{
    /// <summary>
    /// Generates a RIDE PDF for an authorized invoice
    /// </summary>
    /// <param name="invoice">Invoice with all details loaded</param>
    /// <param name="sriConfiguration">SRI configuration for company info</param>
    /// <param name="establishment">Establishment details</param>
    /// <param name="emissionPoint">Emission point details</param>
    /// <returns>Path to the generated PDF file</returns>
    Task<string> GenerateRidePdfAsync(
        Invoice invoice,
        SriConfiguration sriConfiguration,
        Establishment establishment,
        EmissionPoint emissionPoint);
}
