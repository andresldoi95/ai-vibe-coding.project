using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
}
