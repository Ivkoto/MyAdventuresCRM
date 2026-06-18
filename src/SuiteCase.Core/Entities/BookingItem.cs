using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class BookingItem
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int? ProgramPricingRuleId { get; set; }
    public required string Type { get; set; }
    public string? Description { get; set; }
    public BookingItemKind Kind { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public bool IsIncludedInTotal { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
