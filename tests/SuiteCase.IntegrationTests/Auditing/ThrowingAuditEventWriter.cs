using SuiteCase.Server.Auditing;

namespace SuiteCase.IntegrationTests.Auditing;

internal sealed class ThrowingAuditEventWriter : IAuditEventWriter
{
    public void Record(
        Guid operationId,
        string action,
        string entityType,
        string entityId,
        string? correlationId,
        string? details = null)
        => throw new InvalidOperationException("Deliberate audit event write failure.");
}
