using SuiteCase.Core.Entities;
using SuiteCase.Core.Helpers;
using SuiteCase.Core.Security;
using SuiteCase.Server.Features.Customers.DTO;

namespace SuiteCase.Server.Features.Customers.Helpers;

public static class CustomerMapping
{
    public static CustomerDetailsResponse ToCustomerDetailsResponse(this Customer customer, ISensitiveDataProtector dataProtector)
        => new(
            customer.Id,
            customer.FirstName,
            customer.MiddleName,
            customer.LastName,
            customer.FirstNameLatin,
            customer.MiddleNameLatin,
            customer.LastNameLatin,
            customer.NationalIdEncrypted is null ? null : dataProtector.Unprotect(customer.NationalIdEncrypted),
            customer.DateOfBirth,
            customer.PassportNumberEncrypted is null ? null : dataProtector.Unprotect(customer.PassportNumberEncrypted),
            customer.PassportExpiresOn,
            customer.Email,
            customer.PhoneNumber,
            customer.ResidenceCountryCode,
            SuiteCase.Core.Countries.Countries.GetName(customer.ResidenceCountryCode),
            customer.Notes
        );

    public static Customer ToCustomer(
        this CreateCustomerRequest request,
        DateTimeOffset createdAt,
        string? rawNationalId,
        string residenceCountryCode)
        => new()
        {
            FirstName = request.FirstName.Trim(),
            MiddleName = NormalizeOptionalText(request.MiddleName),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth ?? EgnHelper.TryExtractDateOfBirth(rawNationalId),
            PassportExpiresOn = request.PassportExpiresOn,
            Email = NormalizeOptionalText(request.Email),
            PhoneNumber = NormalizeOptionalText(request.PhoneNumber),
            ResidenceCountryCode = residenceCountryCode,
            Notes = NormalizeOptionalText(request.Notes),
            CreatedAt = createdAt
        };

    public static void UpdateFrom(
        this Customer customer,
        UpdateCustomerRequest request,
        DateTimeOffset updatedAt,
        string? rawNationalId,
        string residenceCountryCode)
    {
        customer.FirstName = request.FirstName.Trim();
        customer.MiddleName = NormalizeOptionalText(request.MiddleName);
        customer.LastName = request.LastName.Trim();
        customer.FirstNameLatin = NormalizeOptionalText(request.FirstNameLatin);
        customer.MiddleNameLatin = NormalizeOptionalText(request.MiddleNameLatin);
        customer.LastNameLatin = NormalizeOptionalText(request.LastNameLatin);
        customer.DateOfBirth = request.DateOfBirth ?? EgnHelper.TryExtractDateOfBirth(rawNationalId);
        customer.PassportExpiresOn = request.PassportExpiresOn;
        customer.Email = NormalizeOptionalText(request.Email);
        customer.PhoneNumber = NormalizeOptionalText(request.PhoneNumber);
        customer.ResidenceCountryCode = residenceCountryCode;
        customer.Notes = NormalizeOptionalText(request.Notes);
        customer.UpdatedAt = updatedAt;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
