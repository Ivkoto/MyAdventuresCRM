using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class TravelProgram
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateOnly BaseStartDate { get; set; }
    public DateOnly BaseEndDate { get; set; }
    public decimal BasePriceAmount { get; set; }
    public Currency BasePriceCurrency { get; set; } = Currency.EUR;
    public string? OrganizerName { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
