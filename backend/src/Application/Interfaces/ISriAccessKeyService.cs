using SaaS.Domain.Enums;
using SaaS.Domain.ValueObjects;

namespace SaaS.Application.Interfaces;

/// <summary>
/// Service for generating and validating SRI Access Keys
/// </summary>
public interface ISriAccessKeyService
{
    /// <summary>
    /// Generates a new SRI access key
    /// </summary>
    AccessKey GenerateAccessKey(
        DateTime issueDate,
        DocumentType documentType,
        string ruc,
        SriEnvironment environment,
        string establishmentCode,
        string emissionPointCode,
        int sequential,
        EmissionType emissionType = EmissionType.Normal);

    /// <summary>
    /// Validates an existing access key
    /// </summary>
    bool ValidateAccessKey(string accessKey);
}
