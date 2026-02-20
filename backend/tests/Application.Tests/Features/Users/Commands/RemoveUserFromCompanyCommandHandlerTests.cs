using Xunit;
using FluentAssertions;
using Moq;
using SaaS.Application.Features.Users.Commands.RemoveUserFromCompany;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Features.Users.Commands;

public class RemoveUserFromCompanyCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<ILogger<RemoveUserFromCompanyCommandHandler>> _loggerMock;
    private readonly RemoveUserFromCompanyCommandHandler _handler;

    public RemoveUserFromCompanyCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _loggerMock = new Mock<ILogger<RemoveUserFromCompanyCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.UserTenants).Returns(_userTenantRepositoryMock.Object);

        _handler = new RemoveUserFromCompanyCommandHandler(
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRemoval_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(currentUserId);

        var userTenant = new UserTenant
        {
            UserId = targetUserId,
            TenantId = tenantId,
            Role = new Role { Name = "Manager", IsSystemRole = false },
            IsActive = true
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(targetUserId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        var command = new RemoveUserFromCompanyCommand(targetUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        userTenant.IsActive.Should().BeFalse();
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(userTenant, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns((Guid?)null);

        var command = new RemoveUserFromCompanyCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tenant context is required");
    }

    [Fact]
    public async Task Handle_NoUserContext_ShouldReturnFailure()
    {
        // Arrange
        _tenantContextMock.Setup(t => t.TenantId).Returns(Guid.NewGuid());
        _tenantContextMock.Setup(t => t.UserId).Returns((Guid?)null);

        var command = new RemoveUserFromCompanyCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User context is required");
    }

    [Fact]
    public async Task Handle_SelfRemoval_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(userId);

        var command = new RemoveUserFromCompanyCommand(userId); // Same as current user

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("You cannot remove yourself from the company");
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserTenant>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFoundInCompany_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(currentUserId);

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(targetUserId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTenant?)null);

        var command = new RemoveUserFromCompanyCommand(targetUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found in this company");
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(currentUserId);

        var userTenant = new UserTenant
        {
            UserId = targetUserId,
            TenantId = tenantId,
            IsActive = false // Already inactive
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(targetUserId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenant);

        var command = new RemoveUserFromCompanyCommand(targetUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found in this company");
    }

    [Fact]
    public async Task Handle_RemovingLastOwner_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(currentUserId);

        var ownerRole = new Role { Name = "Owner", IsSystemRole = true };

        var ownerUserTenant = new UserTenant
        {
            UserId = ownerUserId,
            TenantId = tenantId,
            Role = ownerRole,
            IsActive = true
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(ownerUserId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerUserTenant);

        _userTenantRepositoryMock
            .Setup(r => r.GetByTenantIdWithDetailsAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserTenant> { ownerUserTenant }); // Only one owner

        var command = new RemoveUserFromCompanyCommand(ownerUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot remove the last owner");
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserTenant>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RemovingOwnerWithOtherOwnersPresent_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var owner1Id = Guid.NewGuid();
        var owner2Id = Guid.NewGuid();

        _tenantContextMock.Setup(t => t.TenantId).Returns(tenantId);
        _tenantContextMock.Setup(t => t.UserId).Returns(currentUserId);

        var ownerRole = new Role { Name = "Owner", IsSystemRole = true };

        var owner1UserTenant = new UserTenant
        {
            UserId = owner1Id,
            TenantId = tenantId,
            Role = ownerRole,
            IsActive = true
        };

        var owner2UserTenant = new UserTenant
        {
            UserId = owner2Id,
            TenantId = tenantId,
            Role = ownerRole,
            IsActive = true
        };

        _userTenantRepositoryMock
            .Setup(r => r.GetByUserAndTenantAsync(owner1Id, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner1UserTenant);

        _userTenantRepositoryMock
            .Setup(r => r.GetByTenantIdWithDetailsAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserTenant> { owner1UserTenant, owner2UserTenant }); // Two owners

        var command = new RemoveUserFromCompanyCommand(owner1Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        owner1UserTenant.IsActive.Should().BeFalse();
        _userTenantRepositoryMock.Verify(r => r.UpdateAsync(owner1UserTenant, It.IsAny<CancellationToken>()), Times.Once);
    }
}
