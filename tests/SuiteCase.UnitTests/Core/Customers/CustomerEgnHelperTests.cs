using SuiteCase.Core.Customers;

namespace SuiteCase.UnitTests.Core.Customers;

public sealed class CustomerEgnHelperTests
{
    [Fact]
    public void TryExtractDateOfBirth_ReturnsCorrectDate_ForValid1900sEgn()
    {
        // EGN 8501014017: born 1985-01-01
        // Checksum: 8*2+5*4+0*8+1*5+0*10+1*9+4*7+0*3+1*6 = 84, 84%11=7
        var result = CustomerEgnHelper.TryExtractDateOfBirth("8501014017");

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(1985, 1, 1), result.Value);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsCorrectDate_ForValid2000sEgn()
    {
        // 2000s EGN: month encoded as month + 40
        // Born 2001-05-10, digits: 01 45 10 001 0
        // Checksum: 0*2+1*4+4*8+5*5+1*10+0*9+0*7+0*3+1*6 = 77, 77%11=0
        var result = CustomerEgnHelper.TryExtractDateOfBirth("0145100010");

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2001, 5, 10), result.Value);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsCorrectDate_ForValid1800sEgn()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("9932310011");

        Assert.Equal(new DateOnly(1899, 12, 31), result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsCorrectDate_WhenChecksumRemainderIsTen()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("8501010500");

        Assert.Equal(new DateOnly(1985, 1, 1), result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForNullInput()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth(null);
        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForEmptyString()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("");
        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForNonNumericInput()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("ABCDEFGHIJ");

        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForNonAsciiDigits()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("\u0661\u0662\u0663\u0664\u0665\u0666\u0667\u0668\u0669\u0660");

        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForWrongLength()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("123456789");

        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForSpecificForeignPlaceholderWithInvalidChecksum()
    {
        // This specific mmddyy0000 placeholder does not pass EGN checksum validation.
        var result = CustomerEgnHelper.TryExtractDateOfBirth("0115850000");

        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForInvalidChecksum()
    {
        // Valid EGN 8501014017 with last digit changed
        var result = CustomerEgnHelper.TryExtractDateOfBirth("8501014018");

        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForInvalidMonth()
    {
        // Month 13 is not in any valid range (1-12, 21-32, 41-52)
        var result = CustomerEgnHelper.TryExtractDateOfBirth("8513010017");

        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForInvalidDay()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("8502310010");

        Assert.Null(result);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForWhitespaceInput()
    {
        var result = CustomerEgnHelper.TryExtractDateOfBirth("   ");

        Assert.Null(result);
    }
}
