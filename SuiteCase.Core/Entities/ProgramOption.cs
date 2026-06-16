namespace SuiteCase.Core.Entities;

public class ProgramOption
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
