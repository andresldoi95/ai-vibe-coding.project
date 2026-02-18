using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Users.Queries.GetCompanyUsers;

/// <summary>
/// Query to get all users in the current company
/// </summary>
public record GetCompanyUsersQuery : IRequest<Result<List<CompanyUserDto>>>;
