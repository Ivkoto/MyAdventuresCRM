using CedAdventureCRM.Core.Enums;

namespace CedAdventureCRM.Core.Entities;

enum Kind
{
    Discount = 0,
    Surcharge = 1
}

internal class ProgramPricingRule
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public int? DepartureId { get; set; }

    public required Kind Kind { get; set; }

    public required string Name { get; set; }
    public Decimal PriceAmount { get; set; }

    public Currency PriceCurrency { get; set; } = Currency.EUR;    
    public string? AppliesTo { get; set; } //PerBooking | PerPerson | PerNight
    public bool IsOptional { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}
