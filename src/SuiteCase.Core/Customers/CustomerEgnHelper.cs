namespace SuiteCase.Core.Customers;

/// <summary>
/// Utilities for working with Bulgarian EGN (Единен граждански номер).
/// </summary>
public static class CustomerEgnHelper
{
    /// <summary>
    /// Attempts to extract a date of birth from a Bulgarian EGN (10-digit national ID).
    /// Foreign placeholder values like mmddyy0000 will fail checksum validation.
    /// </summary>
    /// <remarks>
    /// EGN month encoding (official GRAO specification):
    /// <list type="bullet">
    ///   <item><description>Month 1–12: born in 1900–1999 (year += 1900)</description></item>
    ///   <item><description>Month 21–32: born in 1800–1899 (month -= 20, year += 1800).
    ///     This range exists because the EGN system was introduced in the 1970s
    ///     and needed to encode citizens born in the 19th century who were still alive.
    ///     In practice this range will almost never trigger for a travel CRM.</description></item>
    ///   <item><description>Month 41–52: born in 2000–2099 (month -= 40, year += 2000)</description></item>
    /// </list>
    /// Checksum weights: 2, 4, 8, 5, 10, 9, 7, 3, 6.
    /// Check digit = (sum of first 9 digits × weights) mod 11; if result is 10, check digit is 0.
    /// </remarks>
    /// <param name="nationalId">
    /// The raw national ID string to parse. Expected to be exactly 10 ASCII digits for a valid EGN.
    /// May be <c>null</c>, empty, or any other string — invalid inputs return <c>null</c>.
    /// </param>
    /// <returns>
    /// The extracted <see cref="DateOnly"/> date of birth if <paramref name="nationalId"/> is a valid EGN;
    /// otherwise <c>null</c>.
    /// </returns>
    public static DateOnly? TryExtractDateOfBirth(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
            return null;

        // EGN must be exactly 10 digits
        if (nationalId.Length != 10 || !nationalId.All(character => character is >= '0' and <= '9'))
            return null;

        int year = int.Parse(nationalId[..2]);
        int month = int.Parse(nationalId[2..4]);
        int day = int.Parse(nationalId[4..6]);

        // Determine century from month encoding
        if (month >= 1 && month <= 12)
        {
            year += 1900;
        }
        else if (month >= 21 && month <= 32)
        {
            month -= 20;
            year += 1800;
        }
        else if (month >= 41 && month <= 52)
        {
            month -= 40;
            year += 2000;
        }
        else
        {
            return null;
        }

        // Validate the date
        if (day < 1 || day > DateTime.DaysInMonth(year, month))
            return null;

        // Validate EGN checksum (weights: 2,4,8,5,10,9,7,3,6)
        int[] weights = [2, 4, 8, 5, 10, 9, 7, 3, 6];
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (nationalId[i] - '0') * weights[i];
        }

        int remainder = sum % 11;
        int expectedCheckDigit = remainder == 10 ? 0 : remainder;
        int actualCheckDigit = nationalId[9] - '0';

        if (expectedCheckDigit != actualCheckDigit)
            return null;

        return new DateOnly(year, month, day);
    }
}
