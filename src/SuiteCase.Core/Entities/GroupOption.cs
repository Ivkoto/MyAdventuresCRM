using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class GroupOption
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int ProgramOptionId { get; set; }
    public decimal PriceAmount { get; set; }
    public Currency PriceCurrency { get; set; } = Currency.EUR;
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
