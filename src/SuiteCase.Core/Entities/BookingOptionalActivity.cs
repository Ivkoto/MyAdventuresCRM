using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class BookingOptionalActivity
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int GroupOptionalActivityId { get; set; }
    public required string Name { get; set; }
    public decimal PriceAmount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public int Quantity { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
