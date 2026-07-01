using SuiteCase.Core.Countries;
using SuiteCase.Core.Entities;
using SuiteCase.Core.Security;
using SuiteCase.Server.Security;

namespace SuiteCase.Server.Features.Customers.Helpers;

/// <summary>
/// Provides customer endpoint helper methods for customer search, age calculation, and country validation.
/// </summary>
public static class CustomerHelper
{
    /// <summary>
    /// Applies the customer directory search across names, phone number, national ID, and passport number.
    /// </summary>
    /// <param name="query">The customer query to filter.</param>
    /// <param name="search">The search value supplied by the customer directory.</param>
    /// <param name="dataProtector">The sensitive data protector used for exact identifier matching.</param>
    /// <returns>The filtered customer query.</returns>
    public static IQueryable<Customer> ApplySearch(IQueryable<Customer> query, string? search, ISensitiveDataProtector dataProtector)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var searchTerm = search.Trim();
        var normalizedPhoneSearchTerm = NormalizePhoneSearchTerm(searchTerm);
        var normalizedSensitiveSearchTerm = searchTerm.NormalizeSensitiveValue();
        var sensitiveValueHash = normalizedSensitiveSearchTerm is null
            ? null
            : dataProtector.Hash(normalizedSensitiveSearchTerm);

        return query.Where(
             c => c.FirstName.Contains(searchTerm) ||
            (c.MiddleName != null && c.MiddleName.Contains(searchTerm)) ||
             c.LastName.Contains(searchTerm) ||
            (c.FirstNameLatin != null && c.FirstNameLatin.Contains(searchTerm)) ||
            (c.MiddleNameLatin != null && c.MiddleNameLatin.Contains(searchTerm)) ||
            (c.LastNameLatin != null && c.LastNameLatin.Contains(searchTerm)) ||
            (normalizedPhoneSearchTerm.Length > 0 &&
             c.PhoneNumber != null &&
             c.PhoneNumber
                 .Replace("+", "")
                 .Replace(" ", "")
                 .Replace("-", "")
                 .Replace("(", "")
                 .Replace(")", "")
                 .Contains(normalizedPhoneSearchTerm)) ||
            (sensitiveValueHash != null &&
             (c.NationalIdHash == sensitiveValueHash ||
              c.PassportNumberHash == sensitiveValueHash)));
    }

    private static string NormalizePhoneSearchTerm(string value)
        => value
            .Replace("+", "")
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

    /// <summary>
    /// Calculates the customer's age on the supplied date.
    /// </summary>
    /// <param name="dateOfBirth">The customer's date of birth, if known.</param>
    /// <param name="today">The date used as the reference point for the age calculation.</param>
    /// <returns>
    /// The calculated age when <paramref name="dateOfBirth" /> is present; otherwise, <see langword="null" />.
    /// </returns>
    public static int? CalculateAge(DateOnly? dateOfBirth, DateOnly today)
    {
        if (dateOfBirth is null) return null;

        var age = today.Year - dateOfBirth.Value.Year;

        if (dateOfBirth.Value > today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Normalizes and validates the customer's residence country code.
    /// </summary>
    /// <param name="value">The residence country code supplied by the customer request.</param>
    /// <param name="residenceCountryCode">The normalized country code when this method returns.</param>
    /// <returns>
    /// <see langword="true" /> when the normalized country code is supported; otherwise, <see langword="false" />.
    /// </returns>
    public static bool TryGetValidResidenceCountryCode(string? value, out string residenceCountryCode)
    {
        residenceCountryCode = Countries.NormalizeCodeOrDefault(value);

        return Countries.IsSupportedCode(residenceCountryCode);
    }
}
