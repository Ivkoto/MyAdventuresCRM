using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class TravelProgramPricingRule
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public int? GroupId { get; set; }
    public BookingItemKind Kind { get; set; }
    public required string Name { get; set; }
    public decimal PriceAmount { get; set; }
    public Currency PriceCurrency { get; set; } = Currency.EUR;
    public string? AppliesTo { get; set; }
    public bool IsOptional { get; set; }
    public int? AgeMin { get; set; }
    public int? AgeMax { get; set; }
    public bool IsDefaultSuggestion { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
