using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SuiteCase.Server.Features.Customers.ErrorHandling;

/// <summary>
/// Defines stable customer API error codes used by clients for predictable error handling.
/// </summary>
internal static class CustomerErrorCodes
{
    public const string NotFound = "customer.not_found";
    public const string DuplicateNationalId = "customer.duplicate_national_id";
    public const string DuplicatePassportNumber = "customer.duplicate_passport_number";
}

/// <summary>
/// Builds customer-specific problem responses with consistent error codes and support correlation extensions.
/// </summary>
internal static class CustomerHttpResultProblems
{
    internal static ProblemHttpResult NotFound(HttpContext httpContext)
        => TypedResults.Problem(
            title: "Customer not found",
            detail: "Customer not found.",
            statusCode: StatusCodes.Status404NotFound,
            extensions: ProblemExtensions(httpContext,
                ("code", CustomerErrorCodes.NotFound)));

    internal static ProblemHttpResult DuplicateNationalId(HttpContext httpContext, int existingCustomerId)
        => TypedResults.Problem(
            title: "Duplicate customer",
            detail: "A customer with this national ID already exists.",
            statusCode: StatusCodes.Status409Conflict,
            extensions: ProblemExtensions(httpContext,
                ("code", CustomerErrorCodes.DuplicateNationalId),
                ("existingCustomerId", existingCustomerId)));

    internal static ProblemHttpResult DuplicatePassportNumber(HttpContext httpContext, int existingCustomerId)
        => TypedResults.Problem(
            title: "Duplicate customer",
            detail: "A customer with this passport number already exists.",
            statusCode: StatusCodes.Status409Conflict,
            extensions: ProblemExtensions(httpContext,
                ("code", CustomerErrorCodes.DuplicatePassportNumber),
                ("existingCustomerId", existingCustomerId)));

    internal static ProblemHttpResult FromSensitiveIdentifierConflict(HttpContext httpContext, SensitiveIdentifierConflict conflict)
        => conflict.Kind switch
        {
            SensitiveIdentifierConflictKind.NationalId =>
                DuplicateNationalId(httpContext, conflict.ExistingCustomerId),

            SensitiveIdentifierConflictKind.PassportNumber =>
                DuplicatePassportNumber(httpContext, conflict.ExistingCustomerId),

            _ => throw new UnreachableException(
                $"Unsupported sensitive identifier conflict kind: {conflict.Kind}.")
        };

    /// <summary>
    /// Adds the current request trace id and customer-specific extension values to endpoint-local problem responses.
    /// </summary>
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
