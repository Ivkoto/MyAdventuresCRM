using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using SuiteCase.Server.Features.Customers.DTO;

namespace SuiteCase.Server.Features.Customers.ErrorHandling;

public static class CustomerErrorCodes
{
    public const string NotFound = "customer.not_found";
    public const string DuplicateNationalId = "customer.duplicate_national_id";
    public const string DuplicatePassportNumber = "customer.duplicate_passport_number";
    public const string DuplicateSensitiveIdentifier = "customer.duplicate_sensitive_identifier";
}

public static class CustomerProblems
{
    public static ProblemHttpResult NotFound(HttpContext httpContext)
        => TypedResults.Problem(
            title: "Customer not found",
            detail: "Customer not found.",
            statusCode: StatusCodes.Status404NotFound,
            extensions: ProblemExtensions(httpContext,
                ("code", CustomerErrorCodes.NotFound)));

    public static ProblemHttpResult DuplicateNationalId(HttpContext httpContext, int existingCustomerId)
        => TypedResults.Problem(
            title: "Duplicate customer",
            detail: "A customer with this national ID already exists.",
            statusCode: StatusCodes.Status409Conflict,
            extensions: ProblemExtensions(httpContext,
                ("code", CustomerErrorCodes.DuplicateNationalId),
                ("existingCustomerId", existingCustomerId)));

    public static ProblemHttpResult DuplicatePassportNumber(HttpContext httpContext, int existingCustomerId)
        => TypedResults.Problem(
            title: "Duplicate customer",
            detail: "A customer with this passport number already exists.",
            statusCode: StatusCodes.Status409Conflict,
            extensions: ProblemExtensions(httpContext,
                ("code", CustomerErrorCodes.DuplicatePassportNumber),
                ("existingCustomerId", existingCustomerId)));

    public static ProblemHttpResult DuplicateSensitiveIdentifier(HttpContext httpContext)
        => TypedResults.Problem(
            title: "Duplicate customer",
            detail: "A customer with the same national ID or passport number already exists.",
            statusCode: StatusCodes.Status409Conflict,
            extensions: ProblemExtensions(httpContext,
                ("code", CustomerErrorCodes.DuplicateSensitiveIdentifier)));

    public static ValidationProblem InvalidResidenceCountryCode()
        => TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(CreateCustomerRequest.ResidenceCountryCode)] =
            ["Residence country code must be a supported European ISO alpha-2 country code."]
        });

    private static Dictionary<string, object?> ProblemExtensions(
        HttpContext httpContext,
        params (string Key, object? Value)[] values)
    {
        var extensions = new Dictionary<string, object?>
        {
            ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
        };

        foreach (var (key, value) in values)
            extensions[key] = value;

        return extensions;
    }
}
