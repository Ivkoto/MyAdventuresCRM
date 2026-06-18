using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using SuiteCase.Core.Security;

namespace SuiteCase.Server.Security;

/// <inheritdoc cref="ISensitiveDataProtector"/>
/// <remarks>
/// Uses ASP.NET Core Data Protection for reversible encryption and keyed HMAC-SHA256 for hashing.
/// <list type="bullet">
///   <item><description>ASP.NET Core Data Protection: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction</description></item>
///   <item><description>Configure ASP.NET Core Data Protection: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview</description></item>
/// </list>
/// </remarks>
public sealed class SensitiveDataProtector : ISensitiveDataProtector
{
    private readonly IDataProtector _protector;
    private readonly byte[] _hashKey;

    /// <summary>
    /// Initializes a new instance of <see cref="SensitiveDataProtector"/>.
    /// </summary>
    /// <param name="dataProtectionProvider">The ASP.NET Core Data Protection provider used for reversible encryption.</param>
    /// <param name="configuration">Application configuration; must contain <c>Security:SensitiveDataHashKey</c>.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>Security:SensitiveDataHashKey</c> is missing, not valid Base64, or shorter than 32 bytes.
    /// </exception>
    public SensitiveDataProtector(IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
    {
        _protector = dataProtectionProvider.CreateProtector("SuiteCase.CustomerSensitiveData.v1");

        var hashKey = configuration["Security:SensitiveDataHashKey"];

        if (string.IsNullOrWhiteSpace(hashKey))
        {
            //TODO: try not to throw unless absolutely necessary. Add log!
            throw new InvalidOperationException("Missing Security:SensitiveDataHashKey configuration.");
        }

        try
        {
            _hashKey = Convert.FromBase64String(hashKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Invalid Security:SensitiveDataHashKey configuration. Expected a Base64-encoded key. Configure it with user-secrets for development or environment variables/secret manager for deployed environments.",
                ex);
        }

        if (_hashKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Invalid Security:SensitiveDataHashKey configuration. The decoded key must be at least 32 bytes for HMAC-SHA256.");
        }
    }

    /// <inheritdoc/>
    public string Protect(string value) => _protector.Protect(value);

    /// <inheritdoc/>
    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);

    /// <inheritdoc/>
    public string Hash(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        var bytes = Encoding.UTF8.GetBytes(normalized);

        using var hmac = new HMACSHA256(_hashKey);
        return Convert.ToHexString(hmac.ComputeHash(bytes));
    }
}
