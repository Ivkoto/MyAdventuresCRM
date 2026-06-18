using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class PaymentMilestone
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int Sequence { get; set; }
    public required string Name { get; set; }
    public DateOnly DueBy { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
