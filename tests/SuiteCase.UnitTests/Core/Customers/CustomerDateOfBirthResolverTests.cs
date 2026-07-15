using SuiteCase.Core.Customers;

namespace SuiteCase.UnitTests.Core.Customers;

public sealed class CustomerDateOfBirthResolverTests
{
    [Fact]
    public void Resolve_ReturnsDateOfBirthFromNationalId_WhenSuppliedDateOfBirthIsMissing()
    {
        var result = CustomerDateOfBirthResolver.Resolve("8501014017", null);

        Assert.Equal(new DateOnly(1985, 1, 1), result);
    }

    [Fact]
    public void Resolve_ReturnsSuppliedDateOfBirth_WhenNationalIdIsMissing()
    {
        var suppliedDateOfBirth = new DateOnly(1990, 6, 15);

        var result = CustomerDateOfBirthResolver.Resolve(null, suppliedDateOfBirth);

        Assert.Equal(suppliedDateOfBirth, result);
    }

    [Fact]
    public void Resolve_ReturnsSuppliedDateOfBirth_WhenNationalIdDoesNotContainValidEgn()
    {
        var suppliedDateOfBirth = new DateOnly(1985, 1, 15);

        var result = CustomerDateOfBirthResolver.Resolve("0115850000", suppliedDateOfBirth);

        Assert.Equal(suppliedDateOfBirth, result);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenOnlyNationalIdIsForeignPlaceholder()
    {
        var result = CustomerDateOfBirthResolver.Resolve("0115850000", null);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_ReturnsSuppliedDateOfBirth_WhenItMatchesValidNationalId()
    {
        var suppliedDateOfBirth = new DateOnly(1985, 1, 1);

        var result = CustomerDateOfBirthResolver.Resolve("8501014017", suppliedDateOfBirth);

        Assert.Equal(suppliedDateOfBirth, result);
    }

    [Fact]
    public void Resolve_ForeignIdentifierPassesEgnChecksum_ReturnsSuppliedDateOfBirth()
    {
        const string nationalId = "0101050000";
        var suppliedDateOfBirth = new DateOnly(2005, 1, 1);

        Assert.Equal(
            new DateOnly(1901, 1, 5),
            CustomerEgnHelper.TryExtractDateOfBirth(nationalId));

        var result = CustomerDateOfBirthResolver.Resolve(nationalId, suppliedDateOfBirth);

        Assert.Equal(suppliedDateOfBirth, result);
    }
}
