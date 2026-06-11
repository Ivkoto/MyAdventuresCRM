using System.ComponentModel.DataAnnotations;

namespace SuiteCase.Server.Features.Customers;

public sealed record CustomerDetailsResponse(
    int Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? FirstNameLatin,
    string? MiddleNameLatin,
    string? LastNameLatin,
    string? NationalId,
    DateOnly? DateOfBirth,
    string? PassportNumber,
    DateOnly? PassportExpiresOn,
    string? Email,
    string? PhoneNumber,
    string? ResidenceCountry,
    string? Notes
);

public sealed record CustomerListResponse(
    int Id,
    string FirstName,
    string LastName,
    string? FirstNameLatin,
    string? LastNameLatin,
    DateOnly? DateOfBirth,
    DateOnly? PassportExpiresOn
);


public sealed record CreateCustomerRequest(
    [Required, MaxLength(100)]
    string FirstName,

    [MaxLength(100)]
    string? MiddleName,

    [Required, MaxLength(100)]
    string LastName,
    
    [MaxLength(100)]
    string? FirstNameLatin,

    [MaxLength(100)]
    string? MiddleNameLatin,

    [MaxLength(100)]
    string? LastNameLatin,

    [MaxLength(50)]
    string? NationalId,

    DateOnly? DateOfBirth,

    [MaxLength(50)]
    string? PassportNumber,

    DateOnly? PassportExpiresOn,

    [EmailAddress, MaxLength(254)]
    string? Email,

    [MaxLength(20)]
    string? PhoneNumber,

    [MaxLength(50)]
    string? ResidenceCountry,
    
    string? Notes
);

public sealed record UpdateCustomerRequest(
    [Required, MaxLength(100)]
    string FirstName,

    [MaxLength(100)]
    string? MiddleName,

    [Required, MaxLength(100)]
    string LastName,

    [MaxLength(100)]
    string? FirstNameLatin,

    [MaxLength(100)]
    string? MiddleNameLatin,

    [MaxLength(100)]
    string? LastNameLatin,

    [MaxLength(50)]
    string? NationalId,

    DateOnly? DateOfBirth,

    [MaxLength(50)]
    string? PassportNumber,

    DateOnly? PassportExpiresOn,

    [EmailAddress, MaxLength(254)]
    string? Email,

    [MaxLength(20)]
    string? PhoneNumber,

    [MaxLength(50)]
    string? ResidenceCountry,

    string? Notes
);
