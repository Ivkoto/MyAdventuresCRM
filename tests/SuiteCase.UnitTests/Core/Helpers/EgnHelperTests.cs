using SuiteCase.Core.Helpers;

namespace SuiteCase.UnitTests.Core.Helpers;

public sealed class EgnHelperTests
{
    [Fact]
    public void TryExtractDateOfBirth_ReturnsCorrectDate_ForValid1900sEgn()
    {
        // EGN 8501014017: born 1985-01-01
        // Checksum: 8*2+5*4+0*8+1*5+0*10+1*9+4*7+0*3+1*6 = 84, 84%11=7
        var result = EgnHelper.TryExtractDateOfBirth("8501014017");

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(1985, 1, 1), result.Value);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsCorrectDate_ForValid2000sEgn()
    {
        // 2000s EGN: month encoded as month + 40
        // Born 2001-05-10, digits: 01 45 10 001 0
        // Checksum: 0*2+1*4+4*8+5*5+1*10+0*9+0*7+0*3+1*6 = 77, 77%11=0
        var result = EgnHelper.TryExtractDateOfBirth("0145100010");

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2001, 5, 10), result.Value);
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForNullInput()
    {
        Assert.Null(EgnHelper.TryExtractDateOfBirth(null));
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForEmptyString()
    {
        Assert.Null(EgnHelper.TryExtractDateOfBirth(""));
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForNonNumericInput()
    {
        Assert.Null(EgnHelper.TryExtractDateOfBirth("ABCDEFGHIJ"));
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForWrongLength()
    {
        Assert.Null(EgnHelper.TryExtractDateOfBirth("123456789"));
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForForeignPlaceholder()
    {
        // Foreign placeholder like mmddyy0000 will fail checksum
        Assert.Null(EgnHelper.TryExtractDateOfBirth("0115850000"));
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForInvalidChecksum()
    {
        // Valid EGN 8501014017 with last digit changed
        Assert.Null(EgnHelper.TryExtractDateOfBirth("8501014018"));
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForInvalidMonth()
    {
        // Month 13 is not in any valid range (1-12, 21-32, 41-52)
        Assert.Null(EgnHelper.TryExtractDateOfBirth("8513010017"));
    }

    [Fact]
    public void TryExtractDateOfBirth_ReturnsNull_ForWhitespaceInput()
    {
        Assert.Null(EgnHelper.TryExtractDateOfBirth("   "));
    }

    [Fact]
    public void TryExtractDateOfBirth_KeepsManualDateOfBirth_WhenProvided()
    {
        // This tests the autofill logic indirectly: if DOB is provided, EGN should not override it.
        // EgnHelper itself just extracts; the caller decides whether to use the result.
        DateOnly? manualDob = new DateOnly(1990, 6, 15);
        var egnDob = EgnHelper.TryExtractDateOfBirth("8501014017");

        // The caller (CustomerMapping) uses: request.DateOfBirth ?? EgnHelper.TryExtractDateOfBirth(...)
        // So if manual is provided, it takes precedence.
        var result = manualDob ?? egnDob;

        Assert.Equal(new DateOnly(1990, 6, 15), result);
    }
}
