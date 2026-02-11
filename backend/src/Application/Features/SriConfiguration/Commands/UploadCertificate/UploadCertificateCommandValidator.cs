using FluentValidation;

namespace SaaS.Application.Features.SriConfiguration.Commands.UploadCertificate;

public class UploadCertificateCommandValidator : AbstractValidator<UploadCertificateCommand>
{
    public UploadCertificateCommandValidator()
    {
        RuleFor(x => x.CertificateFile)
            .NotEmpty().WithMessage("Certificate file is required")
            .Must(file => file.Length > 0).WithMessage("Certificate file cannot be empty")
            .Must(file => file.Length <= 10 * 1024 * 1024).WithMessage("Certificate file cannot exceed 10MB");

        RuleFor(x => x.CertificatePassword)
            .NotEmpty().WithMessage("Certificate password is required");
    }
}
