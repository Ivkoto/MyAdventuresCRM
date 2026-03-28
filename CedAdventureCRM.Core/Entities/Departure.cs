namespace CedAdventureCRM.Core.Entities;

internal class Departure
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public required string DepartureLocation { get; set; }
    public required string ReturnLocation { get; set; }
    public string? CustomerContactName { get; set; }
    public string? GuideName { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}
