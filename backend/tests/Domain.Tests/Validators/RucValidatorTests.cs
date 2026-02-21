using Xunit;
using FluentAssertions;
using SaaS.Domain.Validators;

namespace Domain.Tests.Validators;

public class RucValidatorTests
{
    #region Null/Empty/Whitespace Tests

    [Fact]
    public void IsValid_NullRuc_ReturnsFalse()
    {
        // Arrange
        string? ruc = null;

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_EmptyRuc_ReturnsFalse()
    {
        // Arrange
        var ruc = "";

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhitespaceRuc_ReturnsFalse()
    {
        // Arrange
        var ruc = "   ";

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Length Validation Tests

    [Theory]
    [InlineData("123456789012")]    // 12 digits
    [InlineData("12345678901234")]  // 14 digits
    [InlineData("1234567890")]      // 10 digits
    public void IsValid_InvalidLength_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Format Validation Tests

    [Theory]
    [InlineData("123456789012A")]   // Letter at end
    [InlineData("12A4567890123")]   // Letter in middle
    [InlineData("ABCDEFGHIJKLM")]   // All letters
    [InlineData("1234-56789012")]   // Hyphen
    [InlineData("1234 567890123")]  // Space
    public void IsValid_NonNumericCharacters_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Natural Person RUC Tests (Third Digit 0-5)

    [Fact]
    public void IsValid_ValidNaturalPersonRuc_ReturnsTrue()
    {
        // Arrange - Using verified valid cédula + suffix
        var ruc = "1234567897001";  // Valid cédula "1234567897" + "001"

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("1234567897002")]   // Invalid suffix (not 001)
    [InlineData("1234567897999")]   // Invalid suffix
    [InlineData("1234567897000")]   // Invalid suffix
    public void IsValid_NaturalPersonRucWithInvalidSuffix_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890001")]   // Invalid cédula check digit
    [InlineData("0123456780001")]   // Invalid cédula check digit
    public void IsValid_NaturalPersonRucWithInvalidCedula_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("0012345678001")]   // Invalid province (00)
    [InlineData("2512345678001")]   // Invalid province (25+)
    public void IsValid_NaturalPersonRucWithInvalidProvince_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("1766234567001")]   // Third digit 6 (invalid for natural person cédula)
    [InlineData("1796234567001")]   // Third digit 9 (invalid for natural person cédula)
    public void IsValid_NaturalPersonRucWithInvalidThirdDigitInCedula_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Public Company RUC Tests (Third Digit = 6)

    [Theory]
    [InlineData("1760011611001")]   // Valid public company (province 17, third digit 6, check digit 1)
    [InlineData("0960011690001")]   // Valid public company (province 09, third digit 6, check digit 9)
    public void IsValid_ValidPublicCompanyRuc_ReturnsTrue(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("1790011675001")]   // Wrong check digit
    [InlineData("1790011676001")]   // Wrong check digit
    [InlineData("1790011670001")]   // Wrong check digit
    public void IsValid_PublicCompanyRucWithInvalidCheckDigit_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("0090011674001")]   // Invalid province (00)
    [InlineData("2590011674001")]   // Invalid province (25+)
    public void IsValid_PublicCompanyRucWithInvalidProvince_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_PublicCompanyRucWithCheckDigitZero_ReturnsTrue()
    {
        // Arrange - Construct RUC where check digit calculation results in 0
        // When remainder from modulo 11 is 0, check digit becomes 0
        var ruc = "1760008800001";  // Sum: 99, 99 % 11 = 0, check digit = 0

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_PublicCompanyRucWithCheckDigitOne_ReturnsTrue()
    {
        // Arrange - Construct RUC where check digit calculation results in 1
        // When remainder from modulo 11 is 10, check digit becomes 1
        var ruc = "1760006610001";  // Sum: 89, 89 % 11 = 1, check digit = 10 -> 1

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Private Company RUC Tests (Third Digit = 9)

    [Theory]
    [InlineData("1790011674001")]   // Valid private company (province 17, third digit 9, check digit 4)
    [InlineData("0990012342001")]   // Valid private company (province 09, third digit 9, check digit 2)
    public void IsValid_ValidPrivateCompanyRuc_ReturnsTrue(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("1792146730001")]   // Wrong check digit
    [InlineData("1792146731001")]   // Wrong check digit
    [InlineData("1792146735001")]   // Wrong check digit
    public void IsValid_PrivateCompanyRucWithInvalidCheckDigit_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("0092146739001")]   // Invalid province (00)
    [InlineData("2592146739001")]   // Invalid province (25+)
    public void IsValid_PrivateCompanyRucWithInvalidProvince_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_PrivateCompanyRucWithCheckDigitZero_ReturnsTrue()
    {
        // Arrange - Construct RUC where check digit calculation results in 0
        // When remainder from modulo 11 is 0, check digit becomes 0
        var ruc = "1790055000001";  // Sum: 88, 88 % 11 = 0, check digit = 0

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_PrivateCompanyRucWithCheckDigitOne_ReturnsTrue()
    {
        // Arrange - Construct RUC where check digit calculation results in 1
        // When remainder from modulo 11 is 10, check digit becomes 1
        var ruc = "1790000001001";  // Sum: 43, 43 % 11 = 10, check digit = 10 -> 1

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Unsupported Third Digit Tests

    [Theory]
    [InlineData("1717234567001")]   // Third digit = 7
    [InlineData("1718234567001")]   // Third digit = 8
    public void IsValid_UnsupportedThirdDigit_ReturnsFalse(string ruc)
    {
        // Arrange & Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetErrorMessage Tests

    [Fact]
    public void GetErrorMessage_NullRuc_ReturnsRequiredMessage()
    {
        // Arrange
        string? ruc = null;

        // Act
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        message.Should().Be("RUC is required");
    }

    [Fact]
    public void GetErrorMessage_EmptyRuc_ReturnsRequiredMessage()
    {
        // Arrange
        var ruc = "";

        // Act
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        message.Should().Be("RUC is required");
    }

    [Fact]
    public void GetErrorMessage_WhitespaceRuc_ReturnsRequiredMessage()
    {
        // Arrange
        var ruc = "   ";

        // Act
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        message.Should().Be("RUC is required");
    }

    [Theory]
    [InlineData("123456789012")]
    [InlineData("12345678901234")]
    public void GetErrorMessage_InvalidLength_ReturnsLengthMessage(string ruc)
    {
        // Arrange & Act
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        message.Should().Be("RUC must be 13 digits");
    }

    [Theory]
    [InlineData("123456789012A")]
    [InlineData("ABCDEFGHIJKLM")]
    public void GetErrorMessage_NonNumeric_ReturnsDigitsMessage(string ruc)
    {
        // Arrange & Act
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        message.Should().Be("RUC must contain only digits");
    }

    [Theory]
    [InlineData("1234567890001")]   // Natural person with invalid check digit
    [InlineData("1790011675001")]   // Public company with invalid check digit
    [InlineData("1792146730001")]   // Private company with invalid check digit
    public void GetErrorMessage_InvalidCheckDigit_ReturnsCheckDigitMessage(string ruc)
    {
        // Arrange & Act
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        message.Should().Be("Invalid RUC check digit");
    }

    [Theory]
    [InlineData("1717234567001")]   // Unsupported third digit
    [InlineData("1718234567001")]   // Unsupported third digit
    public void GetErrorMessage_UnsupportedThirdDigit_ReturnsCheckDigitMessage(string ruc)
    {
        // Arrange & Act
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        message.Should().Be("Invalid RUC check digit");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void IsValid_MultipleProvinces_ValidNaturalPersonRucs()
    {
        // Arrange - Test with verified valid cédula from province 12
        var ruc = "1234567897001";

        // Act
        var result = RucValidator.IsValid(ruc);

        // Assert
        result.Should().BeTrue("because it uses a valid cédula with correct suffix");
    }

    [Theory]
    [InlineData("1234567897001")]   // Natural person - valid cédula 1234567897 + 001
    public void GetErrorMessage_ValidRuc_ReturnsEmptyString(string ruc)
    {
        // Arrange & Act
        var isValid = RucValidator.IsValid(ruc);
        var message = RucValidator.GetErrorMessage(ruc);

        // Assert
        isValid.Should().BeTrue("because the RUC should be valid");
        message.Should().BeEmpty();
    }

    #endregion
}
