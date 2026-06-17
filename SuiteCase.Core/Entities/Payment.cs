using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public PaymentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public DateOnly PaidOn { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Bank;
    public string? ExternalReference { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
}
