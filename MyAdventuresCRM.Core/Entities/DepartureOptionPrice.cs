using MyAdventuresCRM.Core.Enums;

namespace MyAdventuresCRM.Core.Entities;

internal class DepartureOptionPrice
{
    public int Id { get; set; }
    public int DepartureId { get; set; }
    public int ProgramOptionId { get; set; }
    public decimal PriceAmount { get; set; }
    public Currency PriceCurrency { get; set; } = Currency.EUR;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}
