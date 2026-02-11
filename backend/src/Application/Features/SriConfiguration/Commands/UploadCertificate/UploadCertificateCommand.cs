using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.SriConfiguration.Commands.UploadCertificate;

/// <summary>
/// Command to upload digital certificate for SRI
/// </summary>
public record UploadCertificateCommand : IRequest<Result<SriConfigurationDto>>
{
    public byte[] CertificateFile { get; init; } = Array.Empty<byte>();
    public string CertificatePassword { get; init; } = string.Empty;
}
