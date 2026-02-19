using Xunit;
using FluentAssertions;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace Domain.Tests.Entities;

public class SriConfigurationTests
{
    [Fact]
    public void SriConfiguration_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var sriConfig = new SriConfiguration();

        // Assert
        sriConfig.CompanyRuc.Should().BeEmpty();
        sriConfig.LegalName.Should().BeEmpty();
        sriConfig.TradeName.Should().BeEmpty();
        sriConfig.MainAddress.Should().BeEmpty();
        sriConfig.Phone.Should().BeEmpty();
        sriConfig.Email.Should().BeEmpty();
        sriConfig.AccountingRequired.Should().BeTrue();
        sriConfig.SpecialTaxpayerNumber.Should().BeNull();
        sriConfig.IsRiseRegime.Should().BeFalse();
        sriConfig.Environment.Should().Be(SriEnvironment.Test);
        sriConfig.DigitalCertificate.Should().BeNull();
        sriConfig.CertificatePassword.Should().BeNull();
        sriConfig.CertificateExpiryDate.Should().BeNull();
    }

    [Fact]
    public void SriConfiguration_ShouldSet_AllRequiredProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var certificate = new byte[] { 1, 2, 3, 4, 5 };
        var expiryDate = DateTime.UtcNow.AddYears(1);

        // Act
        var sriConfig = new SriConfiguration
        {
            TenantId = tenantId,
            CompanyRuc = "1234567890001",
            LegalName = "Test Company S.A.",
            TradeName = "Test Brand",
            MainAddress = "123 Main St, City",
            Phone = "+593-2-1234567",
            Email = "info@testcompany.com",
            AccountingRequired = true,
            SpecialTaxpayerNumber = "RES-12345",
            IsRiseRegime = false,
            Environment = SriEnvironment.Production,
            DigitalCertificate = certificate,
            CertificatePassword = "encrypted-password",
            CertificateExpiryDate = expiryDate
        };

        // Assert
        sriConfig.TenantId.Should().Be(tenantId);
        sriConfig.CompanyRuc.Should().Be("1234567890001");
        sriConfig.LegalName.Should().Be("Test Company S.A.");
        sriConfig.TradeName.Should().Be("Test Brand");
        sriConfig.MainAddress.Should().Be("123 Main St, City");
        sriConfig.Phone.Should().Be("+593-2-1234567");
        sriConfig.Email.Should().Be("info@testcompany.com");
        sriConfig.AccountingRequired.Should().BeTrue();
        sriConfig.SpecialTaxpayerNumber.Should().Be("RES-12345");
        sriConfig.IsRiseRegime.Should().BeFalse();
        sriConfig.Environment.Should().Be(SriEnvironment.Production);
        sriConfig.DigitalCertificate.Should().Equal(certificate);
        sriConfig.CertificatePassword.Should().Be("encrypted-password");
        sriConfig.CertificateExpiryDate.Should().Be(expiryDate);
    }

    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsTrue_WhenCertificateDataPresent()
    {
        // Arrange
        var sriConfig = new SriConfiguration
        {
            DigitalCertificate = new byte[] { 1, 2, 3 },
            CertificatePassword = "password"
        };

        // Act & Assert
        sriConfig.IsCertificateConfigured.Should().BeTrue();
    }

    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsFalse_WhenCertificateMissing()
    {
        // Arrange
        var sriConfig = new SriConfiguration
        {
            DigitalCertificate = null,
            CertificatePassword = "password"
        };

        // Act & Assert
        sriConfig.IsCertificateConfigured.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateConfigured_ReturnsFalse_WhenPasswordMissing()
    {
        // Arrange
        var sriConfig = new SriConfiguration
        {
            DigitalCertificate = new byte[] { 1, 2, 3 },
            CertificatePassword = null
        };

        // Act & Assert
        sriConfig.IsCertificateConfigured.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateValid_ReturnsTrue_WhenNotExpired()
    {
        // Arrange
        var sriConfig = new SriConfiguration
        {
            CertificateExpiryDate = DateTime.UtcNow.AddYears(1)
        };

        // Act & Assert
        sriConfig.IsCertificateValid.Should().BeTrue();
    }

    [Fact]
    public void SriConfiguration_IsCertificateValid_ReturnsFalse_WhenExpired()
    {
        // Arrange
        var sriConfig = new SriConfiguration
        {
            CertificateExpiryDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        sriConfig.IsCertificateValid.Should().BeFalse();
    }

    [Fact]
    public void SriConfiguration_IsCertificateValid_ReturnsFalse_WhenExpiryDateNull()
    {
        // Arrange
        var sriConfig = new SriConfiguration
        {
            CertificateExpiryDate = null
        };

        // Act & Assert
        sriConfig.IsCertificateValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890001", true)]
    [InlineData("9876543210001", true)]
    [InlineData("", false)]
    [InlineData("123", false)]
    [InlineData("12345678901234", false)]
    [InlineData("12345A7890001", false)]
    public void SriConfiguration_IsValidRuc_ValidatesCorrectly(string ruc, bool expectedValid)
    {
        // Arrange
        var sriConfig = new SriConfiguration { CompanyRuc = ruc };

        // Act
        var result = sriConfig.IsValidRuc();

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SriConfiguration_AccountingRequired_CanBeSet(bool accountingRequired)
    {
        // Arrange & Act
        var sriConfig = new SriConfiguration { AccountingRequired = accountingRequired };

        // Assert
        sriConfig.AccountingRequired.Should().Be(accountingRequired);
    }

    [Theory]
    [InlineData(SriEnvironment.Test)]
    [InlineData(SriEnvironment.Production)]
    public void SriConfiguration_Environment_CanBeSet(SriEnvironment environment)
    {
        // Arrange & Act
        var sriConfig = new SriConfiguration { Environment = environment };

        // Assert
        sriConfig.Environment.Should().Be(environment);
    }
}
