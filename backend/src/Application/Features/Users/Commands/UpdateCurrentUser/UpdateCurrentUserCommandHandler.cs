using MediatR;
using Microsoft.Extensions.Logging;
using SaaS.Application.Common.Interfaces;
using SaaS.Application.Common.Models;
using SaaS.Application.DTOs;

namespace SaaS.Application.Features.Users.Commands.UpdateCurrentUser;

/// <summary>
/// Handler for updating current user's profile
/// </summary>
public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCurrentUserCommandHandler> _logger;

    public UpdateCurrentUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateCurrentUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = request.UserId;
            if (userId == Guid.Empty)
            {
                return Result<UserDto>.Failure("User not authenticated");
            }

            // Get existing user
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (user == null || user.IsDeleted)
            {
                return Result<UserDto>.Failure("User not found");
            }

            // Update user properties
            user.Name = request.Name;

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} updated their profile", userId);

            // Return updated user DTO
            return Result<UserDto>.Success(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return Result<UserDto>.Failure("An error occurred while updating the profile");
        }
    }
}
