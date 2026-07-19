using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SuiteCase.IntegrationTests.Auditing;

/// <summary>
/// Simulates a transient connection failure after SQL Server has committed a transaction.
/// </summary>
internal sealed class CommitAcknowledgementFailureInterceptor : DbTransactionInterceptor
{
    private int _failNextCommit;
    private int _failureCount;

    public int FailureCount => Volatile.Read(ref _failureCount);

    public void FailNextCommit()
        => Interlocked.Exchange(ref _failNextCommit, 1);

    public override Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _failNextCommit, 0) == 1)
        {
            Interlocked.Increment(ref _failureCount);
            throw new TimeoutException("Simulated loss of the SQL commit acknowledgement.");
        }

        return Task.CompletedTask;
    }
}
