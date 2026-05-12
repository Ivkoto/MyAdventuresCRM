namespace SuiteCase.Core.Entities;

public class Customer
{
    public int Id { get; private set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? FirstNameLatin { get; set; }
    public string? MiddleNameLatin { get; set; }
    public string? LastNameLatin { get; set; }
    public string? NationalIdEncrypted { get; private set; }
    //used only for duplicate checks / uniqueness.
    public string? NationalIdHash { get; private set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PassportNumberEncrypted { get; private set; }
    //used only for duplicate checks / uniqueness
    public string? PassportNumberHash { get; private set; }
    public DateOnly? PassportExpiresOn { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ResidenceCountry { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt is not null;

    public void SoftDelete (DateTime deletedAt)
    {
        if (DeletedAt is not null) return;

        DeletedAt = deletedAt;
    }

    public void SetNationalId(string? encryptedValue, string? hash)
    {
        NationalIdEncrypted = encryptedValue;
        NationalIdHash = hash;
    }

    public void SetPassportNumber(string? encryptedValue, string? hash)
    {
        PassportNumberEncrypted = encryptedValue;
        PassportNumberHash = hash;
    }
}
