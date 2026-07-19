using SuiteCase.Core.Entities;
using SuiteCase.Server.Data;

namespace SuiteCase.Server.Auditing;

/// <summary>
/// Records audit events through the current scoped EF Core database context.
/// </summary>
internal sealed class EfCoreAuditEventWriter(SuiteCaseDbContext db) : IAuditEventWriter
{
    private readonly SuiteCaseDbContext _db = db;

    public void Record(Guid operationId, string action, string entityType,
        string entityId, string? correlationId, string? details = null)
    {
        _db.AuditEvents.Add(new AuditEvent
        {
            OperationId = operationId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            ActorId = null,
            CorrelationId = correlationId,
            Details = details,
            OccurredAt = DateTimeOffset.UtcNow
        });
    }
}
