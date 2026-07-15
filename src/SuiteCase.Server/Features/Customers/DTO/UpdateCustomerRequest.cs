using System.ComponentModel.DataAnnotations;

namespace SuiteCase.Server.Features.Customers.DTO;

public sealed record UpdateCustomerRequest(
    [Required, MaxLength(100), MinLength(2)]
    string FirstName,

    [MaxLength(100), MinLength(2)]
    string? MiddleName,

    [Required, MaxLength(100), MinLength(2)]
    string LastName,

    [MaxLength(100), MinLength(2)]
    string? FirstNameLatin,

    [MaxLength(100), MinLength(2)]
    string? MiddleNameLatin,

    [MaxLength(100), MinLength(2)]
    string? LastNameLatin,

    [Length(10, 10)]
    string? NationalId,

    DateOnly? DateOfBirth,

    [Length(5, 20)]
    string? PassportNumber,

    DateOnly? PassportExpiresOn,

    [EmailAddress, MaxLength(254)]
    string? Email,

    [MaxLength(20)]
    string? PhoneNumber,

    string? ResidenceCountryCode,

    string? Notes
);
