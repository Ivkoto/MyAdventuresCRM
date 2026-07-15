namespace SuiteCase.Core.Customers;

/// <summary>
/// Represents a customer date of birth that conflicts with the date encoded in a valid Bulgarian EGN.
/// </summary>
public sealed class CustomerDateOfBirthMismatchException() : Exception(DefaultMessage)
{
    private const string DefaultMessage = "The supplied date of birth does not match the date of birth encoded in the national ID.";
}
