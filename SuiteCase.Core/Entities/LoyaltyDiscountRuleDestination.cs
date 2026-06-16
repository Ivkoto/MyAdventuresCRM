namespace SuiteCase.Core.Entities;

public class LoyaltyDiscountRuleDestination
{
    public int Id { get; set; }
    public int RuleId { get; set; }
    public required string LoyaltyScopeKey { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
