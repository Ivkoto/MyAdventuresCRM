using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class LoyaltyDiscountRule
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public int TripCountFrom { get; set; }
    public int? TripCountTo { get; set; }
    public decimal? ProgramPriceMinAmount { get; set; }
    public decimal? ProgramPriceMaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public int Priority { get; set; }
    public LoyaltyDestinationMode DestinationMode { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
