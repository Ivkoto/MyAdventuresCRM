namespace SuiteCase.Server.Security;

/// <summary>
/// Provides normalization helpers for sensitive identifier values before protection or hashing.
/// </summary>
public static class SensitiveValueExtensions
{
    /// <summary>
    /// Normalizes a sensitive identifier value before it is protected or hashed.
    /// </summary>
    /// <param name="value">The sensitive identifier value to normalize.</param>
    /// <returns>
    /// The trimmed uppercase value when supplied; otherwise, <see langword="null" />.
    /// </returns>
    public static string? NormalizeSensitiveValue(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
