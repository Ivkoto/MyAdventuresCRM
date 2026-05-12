using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using SuiteCase.Core.Security;

namespace SuiteCase.Server.Security;

/*
 * ASP.NET Core Data Protection: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-10.0
 * Configure ASP.NET Core Data Protection: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0
 */
public sealed class SensitiveDataProtector : ISensitiveDataProtector
{
    private readonly IDataProtector _protector;
    private readonly byte[] _hashKey;

    public SensitiveDataProtector(IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
    {
        _protector = dataProtectionProvider.CreateProtector("SuiteCase.CustomerSensitiveData.v1");

        var hashKey = configuration["Security:SensitiveDataHashKey"];

        if (string.IsNullOrWhiteSpace(hashKey))
        {
            //TODO: try not to throw unless absolutely necessary. Add log!
            throw new InvalidOperationException("Missing Security:SensitiveDataHashKey configuration.");
        }

        _hashKey = Convert.FromBase64String(hashKey);
    }

    public string Protect(string value) => _protector.Protect(value);

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);

    public string Hash(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        var bytes = Encoding.UTF8.GetBytes(normalized);

        using var hmac = new HMACSHA256(_hashKey);
        return Convert.ToHexString(hmac.ComputeHash(bytes));
    }
}
