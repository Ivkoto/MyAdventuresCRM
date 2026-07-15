namespace SuiteCase.Core.Customers;

/// <summary>
/// Provides passport-related business rules used by SuiteCase.
/// </summary>
public static class CustomerPassportHelper
{
    /// <summary>
    /// Determines whether a passport is valid for at least the next six months from the supplied date.
    /// </summary>
    /// <param name="expiresOn">The passport expiration date, if known.</param>
    /// <param name="today">The date used as the reference point for the six-month validity rule.</param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="expiresOn" /> is present and is on or after
    /// six months from <paramref name="today" />; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsValid(DateOnly? expiresOn, DateOnly today)
        => expiresOn is not null && expiresOn.Value >= today.AddMonths(6);
}
