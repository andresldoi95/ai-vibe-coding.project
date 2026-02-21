using Xunit;
using FluentAssertions;
using SaaS.Domain.ValueObjects;
using SaaS.Domain.Enums;

namespace Domain.Tests.ValueObjects;

public class AccessKeyTests
{
    #region Generate Tests - Valid Inputs

    [Fact]
    public void Generate_ValidInputs_ReturnsAccessKeyWith49Digits()
    {
        // Arrange
        var issueDate = new DateTime(2024, 12, 15);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 123;

        // Act
        var accessKey = AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        accessKey.Value.Should().HaveLength(49);
        accessKey.Value.Should().MatchRegex("^[0-9]{49}$");
    }

    [Fact]
    public void Generate_ValidInvoice_StartsWithCorrectDateAndDocType()
    {
        // Arrange
        var issueDate = new DateTime(2024, 12, 15);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 123;

        // Act
        var accessKey = AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        // Should start with: 15122024 (DDMMYYYY) + 01 (Invoice)
        accessKey.Value.Should().StartWith("1512202401");
    }

    [Fact]
    public void Generate_WithTestEnvironment_ContainsCorrectEnvironmentDigit()
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Test;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 1;

        // Act
        var accessKey = AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        // Environment digit at position 23 (after date, doctype, and RUC)
        accessKey.Value[23].Should().Be('1'); // Test = 1
    }

    [Fact]
    public void Generate_WithProductionEnvironment_ContainsCorrectEnvironmentDigit()
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 1;

        // Act
        var accessKey = AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        // Environment digit at position 23
        accessKey.Value[23].Should().Be('2'); // Production = 2
    }

    [Fact]
    public void Generate_WithContingencyEmission_ContainsCorrectEmissionType()
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 1;
        var emissionType = EmissionType.Contingency;

        // Act
        var accessKey = AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential,
            emissionType);

        // Assert
        // Emission type at position 47 (second to last digit before check digit)
        accessKey.Value[47].Should().Be('2'); // Contingency = 2
    }

    [Theory]
    [InlineData(DocumentType.Invoice, "01")]
    [InlineData(DocumentType.PurchaseLiquidation, "03")]
    [InlineData(DocumentType.CreditNote, "04")]
    [InlineData(DocumentType.DebitNote, "05")]
    public void Generate_DifferentDocumentTypes_ContainsCorrectDocTypeCode(DocumentType docType, string expectedCode)
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 1;

        // Act
        var accessKey = AccessKey.Generate(
            issueDate,
            docType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        // Document type at positions 8-9 (after DDMMYYYY)
        accessKey.Value.Substring(8, 2).Should().Be(expectedCode);
    }

    [Fact]
    public void Generate_MultipleGenerations_ProducesDifferentAccessKeys()
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 1;

        // Act
        var accessKey1 = AccessKey.Generate(issueDate, documentType, ruc, environment, establishmentCode, emissionPointCode, sequential);
        var accessKey2 = AccessKey.Generate(issueDate, documentType, ruc, environment, establishmentCode, emissionPointCode, sequential);
        var accessKey3 = AccessKey.Generate(issueDate, documentType, ruc, environment, establishmentCode, emissionPointCode, sequential);

        // Assert
        // Should be different due to random numeric code
        accessKey1.Value.Should().NotBe(accessKey2.Value);
        accessKey2.Value.Should().NotBe(accessKey3.Value);
        accessKey1.Value.Should().NotBe(accessKey3.Value);
    }

    #endregion

    #region Generate Tests - Invalid Inputs

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Generate_NullOrEmptyRuc_ThrowsArgumentException(string? ruc)
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 1;

        // Act
        Action act = () => AccessKey.Generate(
            issueDate,
            documentType,
            ruc!,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("RUC must be 13 digits*")
            .And.ParamName.Should().Be("ruc");
    }

    [Theory]
    [InlineData("12345678900")]    // 11 digits
    [InlineData("123456789012")]   // 12 digits
    [InlineData("12345678900012")]  // 14 digits
    public void Generate_InvalidRucLength_ThrowsArgumentException(string ruc)
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";
        var sequential = 1;

        // Act
        Action act = () => AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("RUC must be 13 digits*")
            .And.ParamName.Should().Be("ruc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("01")]   // 2 digits
    [InlineData("0001")] // 4 digits
    public void Generate_InvalidEstablishmentCode_ThrowsArgumentException(string? establishmentCode)
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var emissionPointCode = "002";
        var sequential = 1;

        // Act
        Action act = () => AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode!,
            emissionPointCode,
            sequential);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Establishment code must be 3 digits*")
            .And.ParamName.Should().Be("establishmentCode");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("02")]   // 2 digits
    [InlineData("0002")] // 4 digits
    public void Generate_InvalidEmissionPointCode_ThrowsArgumentException(string? emissionPointCode)
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var sequential = 1;

        // Act
        Action act = () => AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode!,
            sequential);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Emission point code must be 3 digits*")
            .And.ParamName.Should().Be("emissionPointCode");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000000)]  // Greater than max (999999999)
    public void Generate_InvalidSequential_ThrowsArgumentException(int sequential)
    {
        // Arrange
        var issueDate = new DateTime(2024, 1, 1);
        var documentType = DocumentType.Invoice;
        var ruc = "1234567890001";
        var environment = SriEnvironment.Production;
        var establishmentCode = "001";
        var emissionPointCode = "002";

        // Act
        Action act = () => AccessKey.Generate(
            issueDate,
            documentType,
            ruc,
            environment,
            establishmentCode,
            emissionPointCode,
            sequential);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Sequential must be between 1 and 999999999*")
            .And.ParamName.Should().Be("sequential");
    }

    #endregion

    #region FromString Tests

    [Fact]
    public void FromString_ValidAccessKey_ReturnsAccessKeyInstance()
    {
        // Arrange - Generate a valid access key first
        var generated = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        // Act
        var fromString = AccessKey.FromString(generated.Value);

        // Assert
        fromString.Should().NotBeNull();
        fromString.Value.Should().Be(generated.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromString_NullOrEmpty_ThrowsArgumentException(string? value)
    {
        // Arrange & Act
        Action act = () => AccessKey.FromString(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Access key must be 49 digits*")
            .And.ParamName.Should().Be("value");
    }

    [Theory]
    [InlineData("1234567890")]                                    // Too short
    [InlineData("12345678901234567890123456789012345678901234567890")]  // Too long (50 digits)
    public void FromString_InvalidLength_ThrowsArgumentException(string value)
    {
        // Arrange & Act
        Action act = () => AccessKey.FromString(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Access key must be 49 digits*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void FromString_ContainsLetters_ThrowsArgumentException()
    {
        // Arrange
        var value = "1234567890123456789012345678901234567890123456A89"; // 49 chars with letter

        // Act
        Action act = () => AccessKey.FromString(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Access key must contain only digits*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void FromString_InvalidCheckDigit_ThrowsArgumentException()
    {
        // Arrange - Generate valid key and corrupt check digit
        var validKey = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        // Replace last digit with wrong check digit
        var invalidKey = validKey.Value.Substring(0, 48) + "9";

        // Act
        Action act = () => AccessKey.FromString(invalidKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid access key check digit*")
            .And.ParamName.Should().Be("value");
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_ValidAccessKey_ReturnsTrue()
    {
        // Arrange - Generate a valid access key
        var accessKey = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        // Act
        var result = AccessKey.IsValid(accessKey.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_NullOrEmpty_ReturnsFalse(string? value)
    {
        // Arrange & Act
        var result = AccessKey.IsValid(value);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890")]                                    // Too short
    [InlineData("12345678901234567890123456789012345678901234567890")]  // Too long
    public void IsValid_InvalidLength_ReturnsFalse(string value)
    {
        // Arrange & Act
        var result = AccessKey.IsValid(value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ContainsNonDigits_ReturnsFalse()
    {
        // Arrange
        var value = "1234567890123456789012345678901234567890123456A89";

        // Act
        var result = AccessKey.IsValid(value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_InvalidCheckDigit_ReturnsFalse()
    {
        // Arrange - Generate valid key and corrupt check digit
        var validKey = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        var currentCheckDigit = int.Parse(validKey.Value.Substring(48, 1));
        var wrongCheckDigit = (currentCheckDigit + 1) % 10;
        var invalidKey = validKey.Value.Substring(0, 48) + wrongCheckDigit.ToString();

        // Act
        var result = AccessKey.IsValid(invalidKey);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Check Digit Edge Cases

    [Fact]
    public void Generate_CheckDigitCalculation_HandlesModulo11Correctly()
    {
        // Arrange & Act - Generate multiple keys to test check digit calculation
        var keys = new List<AccessKey>();
        for (int i = 1; i <= 100; i++)
        {
            var key = AccessKey.Generate(
                new DateTime(2024, 1, 1).AddDays(i % 28),
                DocumentType.Invoice,
                "1234567890001",
                SriEnvironment.Production,
                "001",
                "002",
                i);
            keys.Add(key);
        }

        // Assert - All generated keys should be valid
        foreach (var key in keys)
        {
            AccessKey.IsValid(key.Value).Should().BeTrue();
        }
    }

    [Fact]
    public void Generate_SequentialBoundaryValues_GeneratesValidKeys()
    {
        // Arrange & Act
        var minSeq = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);  // Minimum

        var maxSeq = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            999999999);  // Maximum

        // Assert
        AccessKey.IsValid(minSeq.Value).Should().BeTrue();
        AccessKey.IsValid(maxSeq.Value).Should().BeTrue();
    }

    #endregion

    #region Equality and ToString Tests

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var generated = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        var fromString = AccessKey.FromString(generated.Value);

        // Act & Assert
        generated.Equals(fromString).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var key1 = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        var key2 = AccessKey.Generate(
            new DateTime(2024, 1, 2),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        // Act & Assert
        key1.Equals(key2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var generated = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        var fromString = AccessKey.FromString(generated.Value);

        // Act & Assert
        generated.GetHashCode().Should().Be(fromString.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var accessKey = AccessKey.Generate(
            new DateTime(2024, 1, 1),
            DocumentType.Invoice,
            "1234567890001",
            SriEnvironment.Production,
            "001",
            "002",
            1);

        // Act
        var result = accessKey.ToString();

        // Assert
        result.Should().Be(accessKey.Value);
        result.Should().HaveLength(49);
    }

    #endregion
}
