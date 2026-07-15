using SuiteCase.Core.Countries;

namespace SuiteCase.Server.Features.Customers.Validation;

/// <summary>
/// Normalizes customer residence country codes and verifies that they are supported.
/// </summary>
public static class CustomerCountryCodeResolver
{
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
