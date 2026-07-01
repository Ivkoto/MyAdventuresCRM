using System.ComponentModel.DataAnnotations;

namespace SuiteCase.Server.Features.Customers.DTO;

public sealed record CustomerQueryParameters(
    [property: Range(1, 1_000_000)]
    int Page = 1,

    [property: Range(1, 100)]
    int PageSize = 13,

    [property: MaxLength(100)]
    string? Search = null
);
