using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Establishments.Commands.CreateEstablishment;

/// <summary>
/// Command to create a new establishment
/// </summary>
public record CreateEstablishmentCommand : IRequest<Result<EstablishmentDto>>
{
    public string EstablishmentCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public bool IsActive { get; init; } = true;
}
