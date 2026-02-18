using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Query to get a specific user by ID in the current company
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<CompanyUserDto>>;
