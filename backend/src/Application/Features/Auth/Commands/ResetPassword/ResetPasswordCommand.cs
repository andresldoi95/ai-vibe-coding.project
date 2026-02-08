using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<Result>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
