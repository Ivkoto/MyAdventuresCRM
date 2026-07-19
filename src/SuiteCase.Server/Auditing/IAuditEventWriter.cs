namespace SuiteCase.Server.Auditing;

/// <summary>
/// Records audit events in the current persistence unit of work.
/// </summary>
public interface IAuditEventWriter
{
    /// <summary>
    /// Stages an audit event so it is persisted with the current database operation.
    /// </summary>
    /// <param name="operationId">The stable identifier used to verify the outcome of a retried database operation.</param>
    /// <param name="action">The stable action code.</param>
    /// <param name="entityType">The stable audited entity type.</param>
    /// <param name="entityId">The identifier of the audited entity.</param>
    /// <param name="correlationId">The request or operation trace identifier, when available.</param>
    /// <param name="details">Optional non-sensitive metadata.</param>
    void Record(Guid operationId, string action, string entityType, string entityId, string? correlationId, string? details = null);
}
