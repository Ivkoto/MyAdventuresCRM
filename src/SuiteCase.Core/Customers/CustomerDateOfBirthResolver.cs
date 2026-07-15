namespace SuiteCase.Core.Customers;

/// <summary>
/// Resolves the customer's date of birth from supplied customer data.
/// </summary>
public static class CustomerDateOfBirthResolver
{
    /// <summary>
    /// Resolves the date of birth from the explicitly supplied value or from a valid Bulgarian EGN.
    /// </summary>
    /// <param name="nationalId">The normalized national identifier value.</param>
    /// <param name="suppliedDateOfBirth">The date of birth supplied by the user, if any.</param>
    /// <returns>
    /// The supplied date of birth when present; otherwise, the date extracted from a valid Bulgarian EGN.
    /// Returns <see langword="null" /> when neither value is available.
    /// </returns>
    /// <exception cref="CustomerDateOfBirthMismatchException">
    /// Thrown when both a date of birth and a valid Bulgarian EGN are supplied, but they resolve to different dates.
    /// </exception>
    public static DateOnly? Resolve(string? nationalId, DateOnly? suppliedDateOfBirth)
    {
        var nationalIdDateOfBirth = CustomerEgnHelper.TryExtractDateOfBirth(nationalId);

        if (suppliedDateOfBirth is not null && nationalIdDateOfBirth is not null
            && suppliedDateOfBirth != nationalIdDateOfBirth)
        {
            throw new CustomerDateOfBirthMismatchException();
        }

        return suppliedDateOfBirth ?? nationalIdDateOfBirth;
    }
}
