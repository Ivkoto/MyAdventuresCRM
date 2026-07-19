namespace SuiteCase.Core.Entities;

/// <summary>
/// Represents an append-only record of a security-relevant or business-significant action.
/// </summary>
public sealed class AuditEvent
{
    public long Id { get; private set; }
    public required Guid OperationId { get; init; }
    public required string Action { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public string? ActorId { get; init; }
    public string? CorrelationId { get; init; }
    public string? Details { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}
