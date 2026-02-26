using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

/// <summary>
/// Service for generating RIDE (Representación Impresa del Comprobante Electrónico) PDFs for credit notes
/// </summary>
public interface ICreditNoteRideService
{
    /// <summary>
    /// Generates a RIDE PDF for an authorized credit note
    /// </summary>
    /// <param name="creditNote">Credit note with all details loaded</param>
    /// <param name="sriConfiguration">SRI configuration for company info</param>
    /// <param name="establishment">Establishment details</param>
    /// <param name="emissionPoint">Emission point details</param>
    /// <returns>Path to the generated PDF file</returns>
    Task<string> GenerateRidePdfAsync(
        CreditNote creditNote,
        SriConfiguration sriConfiguration,
        Establishment establishment,
        EmissionPoint emissionPoint);
}
