using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_IsExpired_ReturnsFalse_WhenNotExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsExpired_ReturnsTrue_WhenExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsExpired_ReturnsTrue_WhenExactlyExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1)
        };

        // Act & Assert
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsRevoked_ReturnsFalse_WhenNotRevoked()
    {
        // Arrange
        var token = new RefreshToken
        {
            RevokedAt = null
        };

        // Act & Assert
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsRevoked_ReturnsTrue_WhenRevoked()
    {
        // Arrange
        var token = new RefreshToken
        {
            RevokedAt = DateTime.UtcNow
        };

        // Act & Assert
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsActive_ReturnsTrue_WhenNotRevokedAndNotExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null
        };

        // Act & Assert
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsActive_ReturnsFalse_WhenRevoked()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow
        };

        // Act & Assert
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsActive_ReturnsFalse_WhenExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = null
        };

        // Act & Assert
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsActive_ReturnsFalse_WhenBothRevokedAndExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = DateTime.UtcNow
        };

        // Act & Assert
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_CreatedAt_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var token = new RefreshToken();
        var after = DateTime.UtcNow;

        // Assert
        token.CreatedAt.Should().BeOnOrAfter(before);
        token.CreatedAt.Should().BeOnOrBefore(after);
        token.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void RefreshToken_CanStore_ReplacementToken()
    {
        // Arrange
        var replacementToken = "new-token-123";

        // Act
        var token = new RefreshToken
        {
            ReplacedByToken = replacementToken
        };

        // Assert
        token.ReplacedByToken.Should().Be(replacementToken);
    }

    [Fact]
    public void RefreshToken_CanStore_IpAddresses()
    {
        // Arrange
        var createdIp = "192.168.1.1";
        var revokedIp = "192.168.1.2";

        // Act
        var token = new RefreshToken
        {
            CreatedByIp = createdIp,
            RevokedByIp = revokedIp,
            RevokedAt = DateTime.UtcNow
        };

        // Assert
        token.CreatedByIp.Should().Be(createdIp);
        token.RevokedByIp.Should().Be(revokedIp);
    }
}
