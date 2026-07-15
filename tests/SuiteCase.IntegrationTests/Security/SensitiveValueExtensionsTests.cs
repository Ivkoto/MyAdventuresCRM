using SuiteCase.Server.Security;

namespace SuiteCase.IntegrationTests.Security;

public sealed class SensitiveValueExtensionsTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(" pa1234567 ", "PA1234567")]
    [InlineData("9001154218", "9001154218")]
    public void NormalizeSensitiveValue_NormalizesValueBeforeProtectionOrHashing(
        string? value,
        string? expected)
    {
        var normalizedValue = value.NormalizeSensitiveValue();

        Assert.Equal(expected, normalizedValue);
    }
}
