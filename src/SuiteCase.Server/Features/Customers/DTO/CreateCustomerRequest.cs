using System.ComponentModel.DataAnnotations;

namespace SuiteCase.Server.Features.Customers.DTO;

public sealed record CreateCustomerRequest(
    [Required, MaxLength(100), MinLength(2)]
    string FirstName,

    [MaxLength(100), MinLength(2)]
    string? MiddleName,

    [Required, MaxLength(100), MinLength(2)]
    string LastName,

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

    [Length(2, 2)]
    string? ResidenceCountryCode,

    string? Notes
);
