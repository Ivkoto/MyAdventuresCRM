namespace CedAdventureCRM.Core.Entities;

internal class Customer
{
    public int Id { get; private set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? FirstNameLatin { get; set; }
    public string? LastNameLatin { get; set; }
    public string? NationalId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PassportNumber { get; set; }
    public DateOnly? PassportExpiresOn { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ResidenceCountry { get; set; }
    public string? ResidenceCity { get; set; }
    public string? AddressLine { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}
