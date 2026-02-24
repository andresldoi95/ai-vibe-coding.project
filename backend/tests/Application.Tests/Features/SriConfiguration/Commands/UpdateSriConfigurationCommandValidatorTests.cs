using FluentAssertions;
using FluentValidation;
using SaaS.Application.Features.SriConfiguration.Commands.UpdateSriConfiguration;
using SaaS.Domain.Enums;
using Xunit;

namespace Application.Tests.Features.SriConfiguration.Commands;

public class UpdateSriConfigurationCommandValidatorTests
{
    private readonly UpdateSriConfigurationCommandValidator _validator;

    public UpdateSriConfigurationCommandValidatorTests()
    {
        _validator = new UpdateSriConfigurationCommandValidator();
    }

    // -----------------------------------------------------------------------
    // Full valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = BuildValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // CompanyRuc
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyRuc_ShouldFail()
    {
        var command = BuildValidCommand() with { CompanyRuc = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CompanyRuc));
    }

    [Theory]
    [InlineData("123456789000")]   // 12 digits — too short
    [InlineData("12345678900011")] // 14 digits — too long
    public void Validate_RucWrongLength_ShouldFail(string ruc)
    {
        var command = BuildValidCommand() with { CompanyRuc = ruc };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CompanyRuc));
    }

    [Theory]
    [InlineData("1234567890A01")] // contains letter
    [InlineData("123456789 001")] // contains space
    public void Validate_RucNonNumeric_ShouldFail(string ruc)
    {
        var command = BuildValidCommand() with { CompanyRuc = ruc };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CompanyRuc));
    }

    [Fact]
    public void Validate_InvalidRucChecksum_ShouldFail()
    {
        // RUC with correct format but failing RucValidator.IsValid (bad check digits)
        var command = BuildValidCommand() with { CompanyRuc = "1234567890000" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(command.CompanyRuc) &&
            e.ErrorMessage.Contains("Invalid RUC"));
    }

    // -----------------------------------------------------------------------
    // LegalName
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyLegalName_ShouldFail()
    {
        var command = BuildValidCommand() with { LegalName = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.LegalName));
    }

    [Fact]
    public void Validate_LegalNameExceeds256Chars_ShouldFail()
    {
        var command = BuildValidCommand() with { LegalName = new string('A', 257) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.LegalName));
    }

    [Fact]
    public void Validate_LegalNameExactly256Chars_ShouldPass()
    {
        var command = BuildValidCommand() with { LegalName = new string('A', 256) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // TradeName (optional)
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_NullTradeName_ShouldPass()
    {
        var command = BuildValidCommand() with { TradeName = null };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TradeNameExceeds256Chars_ShouldFail()
    {
        var command = BuildValidCommand() with { TradeName = new string('B', 257) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TradeName));
    }

    // -----------------------------------------------------------------------
    // MainAddress
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyMainAddress_ShouldFail()
    {
        var command = BuildValidCommand() with { MainAddress = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MainAddress));
    }

    [Fact]
    public void Validate_MainAddressExceeds500Chars_ShouldFail()
    {
        var command = BuildValidCommand() with { MainAddress = new string('C', 501) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MainAddress));
    }

    // -----------------------------------------------------------------------
    // Email
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyEmail_ShouldFail()
    {
        var command = BuildValidCommand() with { Email = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public void Validate_InvalidEmailFormat_ShouldFail(string email)
    {
        var command = BuildValidCommand() with { Email = email };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_EmailExceeds256Chars_ShouldFail()
    {
        var localPart = new string('a', 250);
        var command = BuildValidCommand() with { Email = $"{localPart}@example.com" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    // -----------------------------------------------------------------------
    // Phone
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyPhone_ShouldFail()
    {
        var command = BuildValidCommand() with { Phone = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Phone));
    }

    [Fact]
    public void Validate_PhoneExceeds20Chars_ShouldFail()
    {
        var command = BuildValidCommand() with { Phone = new string('9', 21) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Phone));
    }

    // -----------------------------------------------------------------------
    // Environment
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_InvalidEnvironmentValue_ShouldFail()
    {
        var command = BuildValidCommand() with { Environment = (SriEnvironment)99 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Environment));
    }

    [Theory]
    [InlineData(SriEnvironment.Test)]
    [InlineData(SriEnvironment.Production)]
    public void Validate_ValidEnvironments_ShouldPass(SriEnvironment env)
    {
        var command = BuildValidCommand() with { Environment = env };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // SpecialTaxpayerNumber (optional)
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_NullSpecialTaxpayerNumber_ShouldPass()
    {
        var command = BuildValidCommand() with { SpecialTaxpayerNumber = null };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_SpecialTaxpayerNumberExceeds50Chars_ShouldFail()
    {
        var command = BuildValidCommand() with { SpecialTaxpayerNumber = new string('1', 51) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.SpecialTaxpayerNumber));
    }

    // -----------------------------------------------------------------------
    // Helper
    // -----------------------------------------------------------------------

    private static UpdateSriConfigurationCommand BuildValidCommand() =>
        new UpdateSriConfigurationCommand
        {
            // RUC 1234567897001 is a verified valid natural-person RUC (passes RucValidator)
            CompanyRuc = "1234567897001",
            LegalName = "ACME Ecuador S.A.",
            TradeName = "ACME",
            MainAddress = "Av. Republica del El Salvador N35-76, Quito",
            Phone = "0999999999",
            Email = "info@acme.com.ec",
            Environment = SriEnvironment.Test,
            AccountingRequired = false
        };
}
