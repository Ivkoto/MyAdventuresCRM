using MyAdventuresCRM.Core.Enums;

namespace MyAdventuresCRM.Core.Entities;

internal class Program
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public DateOnly BaseStartDate {get; set;}
    public DateOnly BaseEndDate {get; set;}
    public string? Destination {get; set;}
    public decimal BasePriceAmount {get; set;}
    public Currency BasePriceCurrency {get; set;} = Currency.EUR;
    public string? OrganizerName {get; set;}
    public string? Description {get; set;}
    public string? Notes {get; set;}

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}
