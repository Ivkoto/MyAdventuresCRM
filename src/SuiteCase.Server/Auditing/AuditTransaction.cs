using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SuiteCase.Server.Data;

namespace SuiteCase.Server.Auditing;

/// <summary>
/// Executes audited database operations with transient-failure retries and commit-outcome verification.
/// </summary>
internal static class AuditTransaction
{
    /// <summary>
    /// Executes an audited operation in a retry-aware transaction and verifies an ambiguous commit by its audit marker.
    /// </summary>
    public static Task<TResult> ExecuteAsync<TResult>(SuiteCaseDbContext db, Guid operationId,
        Func<CancellationToken, Task<TResult>> operation, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        return strategy.ExecuteInTransactionAsync(
            operation,
            verificationCt => db.AuditEvents
                .AsNoTracking()
                .AnyAsync(auditEvent => auditEvent.OperationId == operationId, verificationCt),
            IsolationLevel.ReadCommitted, ct);
    }

    /// <summary>
    /// Commits the currently tracked business and audit changes without accepting their state before commit is confirmed.
    /// </summary>
    public static async Task CommitTrackedChangesAsync(SuiteCaseDbContext db, Guid operationId, CancellationToken ct)
    {
        await ExecuteAsync(db, operationId, operationCt
            => db.SaveChangesAsync(acceptAllChangesOnSuccess: false, operationCt), ct);

        db.ChangeTracker.AcceptAllChanges();
    }
}
