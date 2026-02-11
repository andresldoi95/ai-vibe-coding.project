namespace SaaS.Domain.Validators;

/// <summary>
/// Validator for Ecuadorian Cédula (National ID)
/// 10-digit identifier with check digit validation
/// </summary>
public static class CedulaValidator
{
    /// <summary>
    /// Validates an Ecuadorian Cédula (10 digits with check digit)
    /// </summary>
    public static bool IsValid(string? cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return false;

        if (cedula.Length != 10)
            return false;

        if (!cedula.All(char.IsDigit))
            return false;

        // Province code must be valid (01-24)
        var province = int.Parse(cedula.Substring(0, 2));
        if (province < 1 || province > 24)
            return false;

        // Third digit must be less than 6 (for natural persons)
        var thirdDigit = int.Parse(cedula[2].ToString());
        if (thirdDigit >= 6)
            return false;

        // Extract check digit (last digit)
        var checkDigit = int.Parse(cedula[9].ToString());

        // Calculate expected check digit
        var expectedCheckDigit = CalculateCheckDigit(cedula);

        return checkDigit == expectedCheckDigit;
    }

    /// <summary>
    /// Calculates check digit using Ecuadorian algorithm (Modulo 10)
    /// </summary>
    private static int CalculateCheckDigit(string cedula)
    {
        int sum = 0;

        // Process first 9 digits
        for (int i = 0; i < 9; i++)
        {
            int digit = int.Parse(cedula[i].ToString());

            // Multiply by 2 for odd positions (0, 2, 4, 6, 8)
            // Multiply by 1 for even positions (1, 3, 5, 7)
            if (i % 2 == 0)
            {
                int result = digit * 2;
                // If result is greater than 9, subtract 9
                if (result > 9)
                    result -= 9;
                sum += result;
            }
            else
            {
                sum += digit;
            }
        }

        // Calculate check digit
        int remainder = sum % 10;
        int checkDigit = (remainder == 0) ? 0 : 10 - remainder;

        return checkDigit;
    }

    /// <summary>
    /// Gets validation error message for invalid Cédula
    /// </summary>
    public static string GetErrorMessage(string? cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return "Cédula is required";

        if (cedula.Length != 10)
            return "Cédula must be 10 digits";

        if (!cedula.All(char.IsDigit))
            return "Cédula must contain only digits";

        var province = int.Parse(cedula.Substring(0, 2));
        if (province < 1 || province > 24)
            return "Invalid province code in Cédula";

        var thirdDigit = int.Parse(cedula[2].ToString());
        if (thirdDigit >= 6)
            return "Invalid third digit in Cédula";

        return "Invalid Cédula check digit";
    }

    /// <summary>
    /// Generates a valid Cédula for testing purposes
    /// WARNING: Use only for testing!
    /// </summary>
    public static string GenerateValid(int province = 17)
    {
        if (province < 1 || province > 24)
            throw new ArgumentException("Province must be between 1 and 24", nameof(province));

        var random = new Random();
        var provinceStr = province.ToString("00");
        var middleDigits = random.Next(100000).ToString("00000");
        var thirdDigit = random.Next(6).ToString(); // 0-5 for natural persons

        var partial = $"{provinceStr}{thirdDigit}{middleDigits}";
        var checkDigit = CalculateCheckDigit(partial + "0");

        return partial + checkDigit;
    }
}
