using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new company and admin user
/// </summary>
public record RegisterCommand : IRequest<Result<LoginResponseDto>>
{
    public string CompanyName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
