namespace SuiteCase.Core.Security;

/// <summary>
/// Contract for protecting and hashing sensitive customer data (e.g., NationalId, PassportNumber).
/// </summary>
/// <remarks>
/// <para>
/// Protect/Unprotect provide reversible encryption so values can be displayed to authorized staff.
/// </para>
/// <para>
/// Hash provides a deterministic one-way value used for duplicate checks and uniqueness constraints
/// without storing the raw plaintext in the database.
/// </para>
/// </remarks>
public interface ISensitiveDataProtector
{
    /// <summary>
    /// Encrypts a plaintext value using reversible protection.
    /// The result can be stored in the database and later decrypted with <see cref="Unprotect"/>.
    /// </summary>
    /// <param name="value">The plaintext value to protect.</param>
    /// <returns>The protected (encrypted) string suitable for database storage.</returns>
    string Protect(string value);

    /// <summary>
    /// Decrypts a previously protected value back to plaintext.
    /// </summary>
    /// <param name="protectedValue">The protected string previously returned by <see cref="Protect"/>.</param>
    /// <returns>The original plaintext value.</returns>
    string Unprotect(string protectedValue);

    /// <summary>
    /// Produces a deterministic, normalized hash of the given value.
    /// Used for duplicate checks and uniqueness constraints without storing the raw value.
    /// </summary>
    /// <remarks>
    /// Implementations should normalize the input (trim, uppercase) before hashing
    /// to ensure consistent matching regardless of casing or surrounding whitespace.
    /// </remarks>
    /// <param name="value">The plaintext value to hash (e.g., NationalId or PassportNumber).</param>
    /// <returns>A hash string suitable for database indexing and equality comparison.</returns>
    string Hash(string value);
}
