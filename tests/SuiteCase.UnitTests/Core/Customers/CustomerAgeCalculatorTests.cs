using SuiteCase.Core.Customers;

namespace SuiteCase.UnitTests.Core.Customers;

public sealed class CustomerAgeCalculatorTests
{
    [Theory]
    [InlineData(2000, 6, 19, 26)]
    [InlineData(2000, 6, 20, 26)]
    [InlineData(2000, 6, 21, 25)]
    public void CalculateAge_AroundBirthday_ReturnsExpectedAge(
        int birthYear,
        int birthMonth,
        int birthDay,
        int expectedAge)
    {
        var today = new DateOnly(2026, 6, 20);

        var age = CustomerAgeCalculator.CalculateAge(
            new DateOnly(birthYear, birthMonth, birthDay),
            today);

        Assert.Equal(expectedAge, age);
    }

    [Fact]
    public void CalculateAge_WhenDateOfBirthIsMissing_ReturnsNull()
    {
        var age = CustomerAgeCalculator.CalculateAge(null, new DateOnly(2026, 6, 20));

        Assert.Null(age);
    }
}
