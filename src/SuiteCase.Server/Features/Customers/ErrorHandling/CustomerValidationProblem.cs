using Microsoft.AspNetCore.Http.HttpResults;
using SuiteCase.Server.Features.Customers.DTO;

namespace SuiteCase.Server.Features.Customers.ErrorHandling;

internal static class CustomerValidationProblem
{
    internal static ValidationProblem InvalidResidenceCountryCode()
        => TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(CreateCustomerRequest.ResidenceCountryCode)] =
            ["Residence country code must be a supported European ISO alpha-2 country code."]
        });
}
