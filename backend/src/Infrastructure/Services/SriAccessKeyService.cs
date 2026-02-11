using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;
using SaaS.Domain.ValueObjects;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Implementation of SRI Access Key generation and validation service
/// </summary>
public class SriAccessKeyService : ISriAccessKeyService
{
    public AccessKey GenerateAccessKey(
        DateTime issueDate,
        DocumentType documentType,
        string ruc,
        SriEnvironment environment,
        string establishmentCode,
        string emissionPointCode,
        int sequential,
        EmissionType emissionType = EmissionType.Normal)
    {
        return AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential,
            emissionType);
    }

    public bool ValidateAccessKey(string accessKey)
    {
        return AccessKey.IsValid(accessKey);
    }
}
