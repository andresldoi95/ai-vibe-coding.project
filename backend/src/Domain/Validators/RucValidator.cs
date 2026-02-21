namespace SaaS.Domain.Validators;

/// <summary>
/// Validator for Ecuadorian RUC (Registro Único de Contribuyentes)
/// 13-digit identifier with check digit validation
/// </summary>
public static class RucValidator
{
    /// <summary>
    /// Validates an Ecuadorian RUC (13 digits with check digit)
    /// </summary>
    public static bool IsValid(string? ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc))
            return false;

        if (ruc.Length != 13)
            return false;

        if (!ruc.All(char.IsDigit))
            return false;

        // Third digit determines the type of taxpayer
        var thirdDigit = int.Parse(ruc[2].ToString());

        // Validate based on taxpayer type
        return thirdDigit switch
        {
            // Third digit < 6: Natural person (same as cédula + 001)
            < 6 => ValidateNaturalPersonRuc(ruc),

            // Third digit = 6: Public company
            6 => ValidatePublicCompanyRuc(ruc),

            // Third digit = 9: Private company or foreign
            9 => ValidatePrivateCompanyRuc(ruc),

            _ => false
        };
    }

    /// <summary>
    /// Validates RUC for natural person (third digit 0-5)
    /// Format: Cédula (10 digits) + 001
    /// </summary>
    private static bool ValidateNaturalPersonRuc(string ruc)
    {
        // Last 3 digits must be 001
        if (ruc.Substring(10, 3) != "001")
            return false;

        // First 10 digits should be a valid cédula
        var cedula = ruc.Substring(0, 10);
        return CedulaValidator.IsValid(cedula);
    }

    /// <summary>
    /// Validates RUC for public company (third digit = 6)
    /// Uses algorithm 2
    /// </summary>
    private static bool ValidatePublicCompanyRuc(string ruc)
    {
        // Province code must be valid (01-24)
        var province = int.Parse(ruc.Substring(0, 2));
        if (province < 1 || province > 24)
            return false;

        // Extract check digit (9th position)
        var checkDigit = int.Parse(ruc[8].ToString());

        // Calculate expected check digit
        var expectedCheckDigit = CalculatePublicCompanyCheckDigit(ruc);

        return checkDigit == expectedCheckDigit;
    }

    /// <summary>
    /// Validates RUC for private company (third digit = 9)
    /// Uses algorithm 3
    /// </summary>
    private static bool ValidatePrivateCompanyRuc(string ruc)
    {
        // Province code must be valid (01-24)
        var province = int.Parse(ruc.Substring(0, 2));
        if (province < 1 || province > 24)
            return false;

        // Extract check digit (10th position)
        var checkDigit = int.Parse(ruc[9].ToString());

        // Calculate expected check digit
        var expectedCheckDigit = CalculatePrivateCompanyCheckDigit(ruc);

        return checkDigit == expectedCheckDigit;
    }

    /// <summary>
    /// Calculates check digit for public companies (algorithm 2)
    /// </summary>
    private static int CalculatePublicCompanyCheckDigit(string ruc)
    {
        int[] coefficients = { 3, 2, 7, 6, 5, 4, 3, 2 };
        int sum = 0;

        for (int i = 0; i < 8; i++)
        {
            int digit = int.Parse(ruc[i].ToString());
            sum += digit * coefficients[i];
        }

        int remainder = sum % 11;
        int checkDigit = 11 - remainder;

        if (checkDigit == 11)
            return 0;
        if (checkDigit == 10)
            return 1;

        return checkDigit;
    }

    /// <summary>
    /// Calculates check digit for private companies (algorithm 3)
    /// </summary>
    private static int CalculatePrivateCompanyCheckDigit(string ruc)
    {
        int[] coefficients = { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            int digit = int.Parse(ruc[i].ToString());
            sum += digit * coefficients[i];
        }

        int remainder = sum % 11;
        int checkDigit = 11 - remainder;

        if (checkDigit == 11)
            return 0;
        if (checkDigit == 10)
            return 1;

        return checkDigit;
    }

    /// <summary>
    /// Gets validation error message for invalid RUC
    /// </summary>
    public static string GetErrorMessage(string? ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc))
            return "RUC is required";

        if (ruc.Length != 13)
            return "RUC must be 13 digits";

        if (!ruc.All(char.IsDigit))
            return "RUC must contain only digits";

        // Return empty string if valid
        if (IsValid(ruc))
            return string.Empty;

        return "Invalid RUC check digit";
    }
}
