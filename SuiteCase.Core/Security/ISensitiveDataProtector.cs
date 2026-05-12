namespace SuiteCase.Core.Security;

public interface ISensitiveDataProtector
{
    string Protect(string value);
    string Unprotect(string protectedValue);
    string Hash(string value);
}
