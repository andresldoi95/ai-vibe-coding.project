using SaaS.Domain.Enums;

namespace SaaS.Domain.ValueObjects;

/// <summary>
/// SRI Access Key (Clave de Acceso) - 49 digit unique identifier for electronic documents
/// Format: DDMMYYYYTTRRRRRRRRRRRRREESSSP########NC
/// DD - Day
/// MM - Month
/// YYYY - Year
/// TT - Document Type (01, 04, 05, 07)
/// RRRRRRRRRRRR - RUC (13 digits)
/// E - Environment (1=Test, 2=Production)
/// EE - Establishment (001-999)
/// SSS - Emission Point (001-999)
/// ######### - Sequential (000000001-999999999)
/// N - Emission Type (1=Normal, 2=Contingency)
/// C - Check Digit (Modulo 11)
/// </summary>
public class AccessKey
{
    public string Value { get; }

    private AccessKey(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Generates a new SRI Access Key
    /// </summary>
    public static AccessKey Generate(
        DateTime issueDate,
        DocumentType documentType,
        string ruc,
        SriEnvironment environment,
        string establishmentCode,
        string emissionPointCode,
        int sequential,
        EmissionType emissionType = EmissionType.Normal)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(ruc) || ruc.Length != 13)
            throw new ArgumentException("RUC must be 13 digits", nameof(ruc));

        if (string.IsNullOrWhiteSpace(establishmentCode) || establishmentCode.Length != 3)
            throw new ArgumentException("Establishment code must be 3 digits", nameof(establishmentCode));

        if (string.IsNullOrWhiteSpace(emissionPointCode) || emissionPointCode.Length != 3)
            throw new ArgumentException("Emission point code must be 3 digits", nameof(emissionPointCode));

        if (sequential < 1 || sequential > 999999999)
            throw new ArgumentException("Sequential must be between 1 and 999999999", nameof(sequential));

        // Build the access key (48 digits without check digit)
        var dateStr = issueDate.ToString("ddMMyyyy");
        var docTypeStr = ((int)documentType).ToString("00");
        var envStr = ((int)environment).ToString();
        var seqStr = sequential.ToString("000000000");
        var numericCode = GenerateNumericCode(); // 8 random digits
        var emissionTypeStr = ((int)emissionType).ToString();

        var accessKeyWithoutCheckDigit =
            $"{dateStr}{docTypeStr}{ruc}{envStr}{establishmentCode}{emissionPointCode}{seqStr}{numericCode}{emissionTypeStr}";

        // Calculate check digit using Modulo 11
        var checkDigit = CalculateModulo11CheckDigit(accessKeyWithoutCheckDigit);

        var fullAccessKey = $"{accessKeyWithoutCheckDigit}{checkDigit}";

        return new AccessKey(fullAccessKey);
    }

    /// <summary>
    /// Creates an AccessKey from an existing value
    /// </summary>
    public static AccessKey FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 49)
            throw new ArgumentException("Access key must be 49 digits", nameof(value));

        if (!value.All(char.IsDigit))
            throw new ArgumentException("Access key must contain only digits", nameof(value));

        // Validate check digit
        var accessKeyWithoutCheckDigit = value.Substring(0, 48);
        var providedCheckDigit = int.Parse(value.Substring(48, 1));
        var calculatedCheckDigit = CalculateModulo11CheckDigit(accessKeyWithoutCheckDigit);

        if (providedCheckDigit != calculatedCheckDigit)
            throw new ArgumentException("Invalid access key check digit", nameof(value));

        return new AccessKey(value);
    }

    /// <summary>
    /// Validates an access key string
    /// </summary>
    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 49)
            return false;

        if (!value.All(char.IsDigit))
            return false;

        try
        {
            var accessKeyWithoutCheckDigit = value.Substring(0, 48);
            var providedCheckDigit = int.Parse(value.Substring(48, 1));
            var calculatedCheckDigit = CalculateModulo11CheckDigit(accessKeyWithoutCheckDigit);

            return providedCheckDigit == calculatedCheckDigit;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calculates Modulo 11 check digit per SRI specification
    /// </summary>
    private static int CalculateModulo11CheckDigit(string value)
    {
        // Modulo 11 algorithm: multiply each digit by factor (descending from 7 to 2, repeating)
        int[] factors = { 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

        int sum = 0;
        for (int i = 0; i < value.Length; i++)
        {
            int digit = int.Parse(value[i].ToString());
            sum += digit * factors[i];
        }

        int remainder = sum % 11;
        int checkDigit = 11 - remainder;

        // Per SRI rules:
        // If check digit is 10, use 1
        // If check digit is 11, use 0
        if (checkDigit == 10)
            return 1;
        if (checkDigit == 11)
            return 0;

        return checkDigit;
    }

    /// <summary>
    /// Generates an 8-digit numeric code (random for uniqueness)
    /// </summary>
    private static string GenerateNumericCode()
    {
        var random = new Random();
        return random.Next(10000000, 99999999).ToString();
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is AccessKey other)
            return Value == other.Value;
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
