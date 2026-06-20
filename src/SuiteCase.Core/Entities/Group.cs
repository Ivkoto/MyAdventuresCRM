using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class Group
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public int? ParentGroupId { get; set; }
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? DepartureLocation { get; set; }
    public string? ReturnLocation { get; set; }
    public CapacityMode CapacityMode { get; set; }
    public int? Capacity { get; set; }
    public string? CustomerContactName { get; set; }
    public string? GuideName { get; set; }
    public TicketType TicketType { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
