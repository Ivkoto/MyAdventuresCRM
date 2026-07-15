using SuiteCase.Core.Customers;

namespace SuiteCase.UnitTests.Core.Customers;

public sealed class CustomerPassportHelperTests
{
    [Fact]
    public void IsValid_ReturnsFalse_WhenExpiresOnIsNull()
    {
        var today = new DateOnly(2026, 6, 20);

        var result = CustomerPassportHelper.IsValid(null, today);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenPassportExpiresBeforeSixMonthsFromToday()
    {
        var today = new DateOnly(2026, 6, 20);

        var result = CustomerPassportHelper.IsValid(new DateOnly(2026, 12, 19), today);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenPassportExpiresExactlySixMonthsFromToday()
    {
        var today = new DateOnly(2026, 6, 20);

        var result = CustomerPassportHelper.IsValid(new DateOnly(2026, 12, 20), today);

        Assert.True(result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenPassportExpiresAfterSixMonthsFromToday()
    {
        var today = new DateOnly(2026, 6, 20);

        var result = CustomerPassportHelper.IsValid(new DateOnly(2026, 12, 21), today);

        Assert.True(result);
    }
}
