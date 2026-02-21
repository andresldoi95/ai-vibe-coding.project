using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;

namespace Domain.Tests.Entities;

public class SriConfigurationBusinessLogicTests
{
    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsTrueWhenBothConfigured()
    {
        // Arrange
        var config = new SriConfiguration
        {
            DigitalCertificate = new byte[] { 1, 2, 3 },
            CertificatePassword = "password123"
        };

        // Act & Assert
        config.IsCertificateConfigured.Should().BeTrue();
    }

    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsFalseWhenCertificateNull()
    {
        // Arrange
        var config = new SriConfiguration
        {
            DigitalCertificate = null,
            CertificatePassword = "password123"
        };

        // Act & Assert
        config.IsCertificateConfigured.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsFalseWhenPasswordNull()
    {
        // Arrange
        var config = new SriConfiguration
        {
            DigitalCertificate = new byte[] { 1, 2, 3 },
            CertificatePassword = null
        };

        // Act & Assert
        config.IsCertificateConfigured.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsFalseWhenPasswordEmpty()
    {
        // Arrange
        var config = new SriConfiguration
        {
            DigitalCertificate = new byte[] { 1, 2, 3 },
            CertificatePassword = ""
        };

        // Act & Assert
        config.IsCertificateConfigured.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsFalseWhenBothNull()
    {
        // Arrange
        var config = new SriConfiguration
        {
            DigitalCertificate = null,
            CertificatePassword = null
        };

        // Act & Assert
        config.IsCertificateConfigured.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateValid_ReturnsTrueWhenNotExpired()
    {
        // Arrange
        var config = new SriConfiguration
        {
            CertificateExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        // Act & Assert
        config.IsCertificateValid.Should().BeTrue();
    }

    [Fact]
    public void SriConfiguration_IsCertificateValid_ReturnsFalseWhenExpired()
    {
        // Arrange
        var config = new SriConfiguration
        {
            CertificateExpiryDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        config.IsCertificateValid.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateValid_ReturnsFalseWhenExactlyExpired()
    {
        // Arrange
        var config = new SriConfiguration
        {
            CertificateExpiryDate = DateTime.UtcNow.AddSeconds(-1)
        };

        // Act & Assert
        config.IsCertificateValid.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateValid_ReturnsFalseWhenExpiryDateNull()
    {
        // Arrange
        var config = new SriConfiguration
        {
            CertificateExpiryDate = null
        };

        // Act & Assert
        config.IsCertificateValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890123", true)]  // Valid 13 digits
    [InlineData("0000000000000", true)]  // All zeros (valid format)
    [InlineData("9999999999999", true)]  // All nines (valid format)
    public void SriConfiguration_IsValidRuc_ReturnsTrueForValidFormats(string ruc, bool expected)
    {
        // Arrange
        var config = new SriConfiguration { CompanyRuc = ruc };

        // Act
        var result = config.IsValidRuc();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("123456789012", false)]   // Too short (12 digits)
    [InlineData("12345678901234", false)] // Too long (14 digits)
    [InlineData("12345678901AB", false)]  // Contains letters
    [InlineData("123456789012A", false)]  // Contains letter
    [InlineData("A234567890123", false)]  // Starts with letter
    [InlineData("", false)]               // Empty
    [InlineData(null, false)]             // Null
    [InlineData("   ", false)]            // Whitespace
    [InlineData("123-456-78901", false)]  // Contains hyphens
    public void SriConfiguration_IsValidRuc_ReturnsFalseForInvalidFormats(string ruc, bool expected)
    {
        // Arrange
        var config = new SriConfiguration { CompanyRuc = ruc };

        // Act
        var result = config.IsValidRuc();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SriConfiguration_CertificateValidation_CombinesMultipleConditions()
    {
        // Valid certificate (configured and not expired)
        var validConfig = new SriConfiguration
        {
            DigitalCertificate = new byte[] { 1, 2, 3 },
            CertificatePassword = "password",
            CertificateExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        // Configured but expired
        var expiredConfig = new SriConfiguration
        {
            DigitalCertificate = new byte[] { 1, 2, 3 },
            CertificatePassword = "password",
            CertificateExpiryDate = DateTime.UtcNow.AddDays(-1)
        };

        // Not configured but valid date
        var notConfiguredConfig = new SriConfiguration
        {
            DigitalCertificate = null,
            CertificatePassword = null,
            CertificateExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        validConfig.IsCertificateConfigured.Should().BeTrue();
        validConfig.IsCertificateValid.Should().BeTrue();

        expiredConfig.IsCertificateConfigured.Should().BeTrue();
        expiredConfig.IsCertificateValid.Should().BeFalse();

        notConfiguredConfig.IsCertificateConfigured.Should().BeFalse();
        notConfiguredConfig.IsCertificateValid.Should().BeTrue();
    }
}
