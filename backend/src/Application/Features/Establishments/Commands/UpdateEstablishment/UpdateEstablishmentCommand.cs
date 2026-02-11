using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Establishments.Commands.UpdateEstablishment;

/// <summary>
/// Command to update an existing establishment
/// </summary>
public record UpdateEstablishmentCommand : IRequest<Result<EstablishmentDto>>
{
    public Guid Id { get; init; }
    public string EstablishmentCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public bool IsActive { get; init; } = true;
}
