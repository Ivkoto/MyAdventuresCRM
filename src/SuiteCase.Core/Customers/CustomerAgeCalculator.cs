namespace SuiteCase.Core.Customers;

/// <summary>
/// Calculates a customer's age on a specified reference date.
/// </summary>
public static class CustomerAgeCalculator
{
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
}
