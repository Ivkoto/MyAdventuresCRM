using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class BookingOption
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int ProgramOptionId { get; set; }
    public required string OptionName { get; set; }
    public decimal PriceAmount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
