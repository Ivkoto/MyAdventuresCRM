namespace SuiteCase.Core.Countries;

public static class Countries
{
    public const string DefaultCode = "BG";

    private static readonly Dictionary<string, string> NamesByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AL"] = "Albania",
        ["AT"] = "Austria",
        ["BY"] = "Belarus",
        ["BE"] = "Belgium",
        ["BA"] = "Bosnia and Herzegovina",
        ["BG"] = "Bulgaria",
        ["HR"] = "Croatia",
        ["CY"] = "Cyprus",
        ["CZ"] = "Czechia",
        ["DK"] = "Denmark",
        ["EE"] = "Estonia",
        ["FI"] = "Finland",
        ["FR"] = "France",
        ["DE"] = "Germany",
        ["GR"] = "Greece",
        ["HU"] = "Hungary",
        ["IS"] = "Iceland",
        ["IE"] = "Ireland",
        ["IT"] = "Italy",
        ["LV"] = "Latvia",
        ["LT"] = "Lithuania",
        ["LU"] = "Luxembourg",
        ["MD"] = "Moldova",
        ["ME"] = "Montenegro",
        ["NL"] = "Netherlands",
        ["MK"] = "North Macedonia",
        ["NO"] = "Norway",
        ["PL"] = "Poland",
        ["PT"] = "Portugal",
        ["RO"] = "Romania",
        ["RU"] = "Russia",
        ["RS"] = "Serbia",
        ["SK"] = "Slovakia",
        ["SI"] = "Slovenia",
        ["ES"] = "Spain",
        ["SE"] = "Sweden",
        ["CH"] = "Switzerland",
        ["TR"] = "Turkey",
        ["UA"] = "Ukraine",
        ["GB"] = "United Kingdom"
    };

    public static string NormalizeCodeOrDefault(string? code)
        => string.IsNullOrWhiteSpace(code) ? DefaultCode : code.Trim().ToUpperInvariant();

    public static bool IsSupportedCode(string? code)
        => !string.IsNullOrWhiteSpace(code) && NamesByCode.ContainsKey(code.Trim());

    public static string GetName(string code) => NamesByCode[code.Trim()];
}
