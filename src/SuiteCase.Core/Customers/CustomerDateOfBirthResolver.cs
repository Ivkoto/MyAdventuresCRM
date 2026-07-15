namespace SuiteCase.Core.Customers;

/// <summary>
/// Resolves the customer's date of birth from supplied customer data.
/// </summary>
public static class CustomerDateOfBirthResolver
{
    /// <summary>
    /// Resolves the date of birth from the authoritative supplied value or, when missing, from an EGN-compatible identifier.
    /// </summary>
    /// <param name="nationalId">The normalized national identifier value.</param>
    /// <param name="suppliedDateOfBirth">The date of birth supplied by the user, if any.</param>
    /// <returns>
    /// The supplied date of birth when present; otherwise, the date extracted from an EGN-compatible identifier.
    /// Returns <see langword="null" /> when neither value is available.
    /// </returns>
    public static DateOnly? Resolve(string? nationalId, DateOnly? suppliedDateOfBirth)
        => suppliedDateOfBirth ?? CustomerEgnHelper.TryExtractDateOfBirth(nationalId);
}
