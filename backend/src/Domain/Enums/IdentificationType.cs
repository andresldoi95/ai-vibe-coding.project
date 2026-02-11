namespace SaaS.Domain.Enums;

/// <summary>
/// SRI Ecuador identification types
/// </summary>
public enum IdentificationType
{
    /// <summary>
    /// RUC - Registro Único de Contribuyentes (04) - 13 digits
    /// </summary>
    Ruc = 4,

    /// <summary>
    /// Cédula - National ID (05) - 10 digits
    /// </summary>
    Cedula = 5,

    /// <summary>
    /// Pasaporte - Passport (06)
    /// </summary>
    Passport = 6,

    /// <summary>
    /// Consumidor Final - Final Consumer (07) - 9999999999999
    /// </summary>
    ConsumerFinal = 7,

    /// <summary>
    /// Identificación del exterior - Foreign identification (08)
    /// </summary>
    ForeignId = 8
}
