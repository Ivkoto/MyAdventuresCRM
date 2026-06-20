namespace SuiteCase.Core.Entities;

public class LoyaltyDiscountRuleDestination
{
    public int Id { get; set; }
    public int RuleId { get; set; }
    public required string LoyaltyScopeKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
