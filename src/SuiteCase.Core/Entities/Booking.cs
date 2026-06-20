using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProgramId { get; set; }
    public int GroupId { get; set; }
    public DateTimeOffset BookedOn { get; set; }
    public BookingStatus Status { get; set; }

    
    public required string ProgramName { get; set; }
    public required string GroupName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal BasePriceAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal FinalPriceAmount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;

    
    public int? AppliedLoyaltyRuleId { get; set; }
    public string? AppliedLoyaltyRuleName { get; set; }
    public decimal AppliedLoyaltyDiscountAmount { get; set; }

    
    public TicketSentStatus TicketSentStatus { get; set; }
    public ContractStatus ContractStatus { get; set; }
    public AnnexStatus AnnexStatus { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
