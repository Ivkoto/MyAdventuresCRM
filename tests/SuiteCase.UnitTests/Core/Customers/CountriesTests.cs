using SuiteCaseCountries = SuiteCase.Core.Countries.Countries;

namespace SuiteCase.UnitTests.Core.Customers;

public sealed class CountriesTests
{
    [Fact]
    public void NormalizeCodeOrDefault_WhenCodeIsMissing_ReturnsBulgaria()
    {
        var code = SuiteCaseCountries.NormalizeCodeOrDefault(null);

        Assert.Equal(SuiteCaseCountries.DefaultCode, code);
    }

    [Fact]
    public void NormalizeCodeOrDefault_WhenCodeHasWhitespaceAndLowercase_ReturnsTrimmedUppercaseCode()
    {
        var code = SuiteCaseCountries.NormalizeCodeOrDefault(" gb ");

        Assert.Equal("GB", code);
    }

    [Theory]
    [InlineData("AL")]
    [InlineData("AT")]
    [InlineData("BY")]
    [InlineData("BE")]
    [InlineData("BA")]
    [InlineData("BG")]
    [InlineData("HR")]
    [InlineData("CY")]
    [InlineData("CZ")]
    [InlineData("DK")]
    [InlineData("EE")]
    [InlineData("FI")]
    [InlineData("FR")]
    [InlineData("DE")]
    [InlineData("GR")]
    [InlineData("HU")]
    [InlineData("IS")]
    [InlineData("IE")]
    [InlineData("IT")]
    [InlineData("LV")]
    [InlineData("LT")]
    [InlineData("LU")]
    [InlineData("MD")]
    [InlineData("ME")]
    [InlineData("NL")]
    [InlineData("MK")]
    [InlineData("NO")]
    [InlineData("PL")]
    [InlineData("PT")]
    [InlineData("RO")]
    [InlineData("RU")]
    [InlineData("RS")]
    [InlineData("SK")]
    [InlineData("SI")]
    [InlineData("ES")]
    [InlineData("SE")]
    [InlineData("CH")]
    [InlineData("TR")]
    [InlineData("UA")]
    [InlineData("GB")]
    public void IsSupportedCode_WhenCodeIsConfiguredEuropeanCountry_ReturnsTrue(string code)
    {
        var isSupported = SuiteCaseCountries.IsSupportedCode(code);

        Assert.True(isSupported);
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("SS")]
    public void IsSupportedCode_WhenCodeIsUnsupported_ReturnsFalse(string code)
    {
        var isSupported = SuiteCaseCountries.IsSupportedCode(code);

        Assert.False(isSupported);
    }

    [Theory]
    [InlineData("AD")]
    [InlineData("LI")]
    [InlineData("MC")]
    [InlineData("SM")]
    [InlineData("VA")]
    public void IsSupportedCode_WhenCodeIsMicrostate_ReturnsFalse(string code)
    {
        var isSupported = SuiteCaseCountries.IsSupportedCode(code);

        Assert.False(isSupported);
    }

    [Fact]
    public void GetName_WhenCodeIsKnown_ReturnsCountryName()
    {
        var name = SuiteCaseCountries.GetName("BG");

        Assert.Equal("Bulgaria", name);
    }

    [Fact]
    public void GetName_WhenCodeHasWhitespaceAndLowercase_ReturnsCountryName()
    {
        var name = SuiteCaseCountries.GetName(" gb ");

        Assert.Equal("United Kingdom", name);
    }
}
