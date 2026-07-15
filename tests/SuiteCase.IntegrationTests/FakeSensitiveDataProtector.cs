using System.Security.Cryptography;
using System.Text;
using SuiteCase.Core.Security;

namespace SuiteCase.IntegrationTests;

internal sealed class FakeSensitiveDataProtector : ISensitiveDataProtector
{
    private const string ProtectedPrefix = "protected:";
    private const string HashPrefix = "hash:";

    public string Protect(string value)
        => $"{ProtectedPrefix}{Guid.NewGuid():N}:{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}";

    public string Unprotect(string protectedValue)
    {
        if (!protectedValue.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
            return protectedValue;

        var payload = protectedValue[ProtectedPrefix.Length..];
        var separatorIndex = payload.IndexOf(':');

        if (separatorIndex >= 0)
            payload = payload[(separatorIndex + 1)..];

        var protectedBytes = Convert.FromBase64String(payload);
        return Encoding.UTF8.GetString(protectedBytes);
    }

    public string Hash(string normalizedValue)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedValue));
        return $"{HashPrefix}{Convert.ToHexString(hashBytes)}";
    }
}
