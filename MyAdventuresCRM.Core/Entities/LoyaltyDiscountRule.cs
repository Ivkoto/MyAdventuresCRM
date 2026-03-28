using MyAdventuresCRM.Core.Enums;

namespace MyAdventuresCRM.Core.Entities;

internal class LoyaltyDiscountRule
{
    public int Id {get; set; }
    public required string Name {get; set;}
    public DateTime EffectiveFrom {get; set;}
    public DateTime? EffectiveTo {get; set;}
    public int MinCompletedTrips {get; set;}
    public int? MaxCompletedTrips {get; set;}
    public string? DestinationScope {get; set;}
    public decimal? ProgramPriceMinAmount {get; set;}
    public decimal? ProgramPriceMaxAmount {get; set;}
    public decimal DiscountAmount {get; set;}
    public Currency Currency {get; set;} = Currency.EUR;
    public int Priority {get; set;}
    public string? Notes { get; set;}

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}
