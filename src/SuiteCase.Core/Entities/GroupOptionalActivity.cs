using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class GroupOptionalActivity
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal PriceAmount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
