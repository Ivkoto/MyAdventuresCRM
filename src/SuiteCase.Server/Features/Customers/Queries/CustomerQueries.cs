using Microsoft.EntityFrameworkCore;
using SuiteCase.Core.Entities;
using SuiteCase.Core.Security;
using SuiteCase.Server.Data;
using SuiteCase.Server.Security;

namespace SuiteCase.Server.Features.Customers.Queries;

/// <summary>
/// Provides database queries used by the customer endpoints.
/// </summary>
internal static class CustomerQueries
{
    /// <summary>
    /// Finds an existing customer whose national ID or passport number conflicts with the supplied hashes.
    /// </summary>
    /// <param name="db">The customer database context.</param>
    /// <param name="nationalIdHash">The national ID hash to check, if supplied.</param>
    /// <param name="passportNumberHash">The passport number hash to check, if supplied.</param>
    /// <param name="excludedCustomerId">The customer ID to exclude when checking an update.</param>
    /// <param name="ct">The cancellation token for the database operation.</param>
    /// <returns>The detected sensitive identifier conflict, or <see langword="null" /> when no conflict exists.</returns>
    internal static async Task<SensitiveIdentifierConflict?> FindSensitiveIdentifierConflictAsync(
        SuiteCaseDbContext db, string? nationalIdHash, string? passportNumberHash, int? excludedCustomerId, CancellationToken ct)
    {
        IQueryable<Customer> customers = db.Customers.AsNoTracking();

        if (excludedCustomerId is not null)
            customers = customers.Where(customer => customer.Id != excludedCustomerId.Value);

        if (nationalIdHash is not null)
        {
            var existingCustomerId = await customers
                .Where(customer => customer.NationalIdHash == nationalIdHash)
                .Select(customer => (int?)customer.Id)
                .SingleOrDefaultAsync(ct);

            if (existingCustomerId is not null)
                return new SensitiveIdentifierConflict(SensitiveIdentifierConflictKind.NationalId, existingCustomerId.Value);
        }

        if (passportNumberHash is not null)
        {
            var existingCustomerId = await customers
                .Where(customer => customer.PassportNumberHash == passportNumberHash)
                .Select(customer => (int?)customer.Id)
                .SingleOrDefaultAsync(ct);

            if (existingCustomerId is not null)
                return new SensitiveIdentifierConflict(SensitiveIdentifierConflictKind.PassportNumber, existingCustomerId.Value);
        }

        return null;
    }

    /// <summary>
    /// Applies the customer directory search across names, phone number, national ID, and passport number.
    /// </summary>
    /// <param name="customers">The customer query to filter.</param>
    /// <param name="search">The search value supplied by the customer directory.</param>
    /// <param name="dataProtector">The sensitive data protector used for exact identifier matching.</param>
    /// <returns>The filtered customer query.</returns>
    internal static IQueryable<Customer> ApplySearch(IQueryable<Customer> customers, string? search, ISensitiveDataProtector dataProtector)
    {
        if (string.IsNullOrWhiteSpace(search))
            return customers;

        var searchTerm = search.Trim();
        var normalizedPhoneSearchTerm = NormalizePhoneSearchTerm(searchTerm);
        var normalizedSensitiveSearchTerm = searchTerm.NormalizeSensitiveValue();
        var sensitiveValueHash = normalizedSensitiveSearchTerm is null
            ? null
            : dataProtector.Hash(normalizedSensitiveSearchTerm);

        return customers.Where(
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
            (sensitiveValueHash != null && (c.NationalIdHash == sensitiveValueHash || c.PassportNumberHash == sensitiveValueHash)));
    }

    private static string NormalizePhoneSearchTerm(string value)
        => value.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
}
