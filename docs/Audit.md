# SuiteCase Audit Logging

## Status

Implemented for customer operations:

- Creating, updating, and soft-deleting a customer records a corresponding audit event.
- Reading customers through `GET /api/customers` or `GET /api/customers/{id}` is not audited. Access is controlled through authorization; future sensitive exports and document downloads will be audited separately.
- Each audit event records what happened, which customer was affected, when it happened, and the request correlation ID. The acting user will be recorded after authentication is implemented.
- Audit events never contain National ID, passport number, encrypted values, hashes, document contents, or complete request/response bodies.
- Customer changes and their audit events are saved in the same SQL Server transaction. Either both are saved or neither is saved.
- Temporary database and network failures are retried. A unique operation ID prevents duplicate customer or audit records when SQL Server commits successfully but its confirmation is lost.
- `IAuditEventWriter` provides the application boundary for recording events, while EF Core stores them in the `AuditEvents` table.
- The EF Core migration creates the audit table and its indexes.
- SQL Server integration tests cover all implemented customer audit actions, rollback behavior, sensitive-data protection, and retry scenarios.

Pending before production:

- ASP.NET Core Identity implementation and actor attribution
- authorization for audit access
- retention, archival, and deletion policy
- audit read/export API
- database-level append-only permissions

## Purpose

Audit events record security-relevant and business-significant actions:

```text
who performed which action, on which record, and when
```

Until authentication is implemented, the actor is unknown and `ActorId` remains `null`.

Audit logging does not replace authorization, operational logging, exception logging, or business snapshots.

## Event Model

Table: `AuditEvents`

| Field | Rule |
|---|---|
| `Id` | `long` identity |
| `OperationId` | Required unique UUIDv7 used to verify a database operation after an ambiguous commit |
| `Action` | Required stable dot-style action code; maximum 100 characters |
| `EntityType` | Required stable entity type; maximum 100 characters |
| `EntityId` | Required string identifier; maximum 100 characters |
| `ActorId` | Nullable until authentication is implemented; maximum 450 characters |
| `CorrelationId` | Nullable request activity/trace identifier; maximum 100 characters |
| `Details` | Optional non-sensitive metadata; maximum 2000 characters |
| `OccurredAt` | Required UTC `DateTimeOffset` |

Indexes:

- `(EntityType, EntityId, OccurredAt)`
- `OccurredAt`
- unique `OperationId`
- `Action`
- `ActorId`
- `CorrelationId`

Audit events do not have foreign keys to audited records. Their history must remain available independently of later soft deletion or schema-specific relationships.

## Architecture

```text
SuiteCase.Core/Entities/AuditEvent.cs
    -> persistence model

SuiteCase.Server/Auditing/IAuditEventWriter.cs
    -> application boundary

SuiteCase.Server/Auditing/EfCoreAuditEventWriter.cs
    -> stages AuditEvent in the current scoped SuiteCaseDbContext

SuiteCase.Server/Auditing/AuditTransaction.cs
    -> executes audited writes with SQL retry and commit-outcome verification

SuiteCase.Server/Data/Configurations/AuditEventConfiguration.cs
    -> SQL Server schema and indexes
```

`IAuditEventWriter.Record` stages an event with a stable `OperationId` in the current EF Core unit of work. `AuditTransaction` owns the retry-aware transaction and checks that marker when SQL Server may have committed but the acknowledgement was lost.

Explicit endpoint calls are intentional while Customer is the only implemented API slice. A `SaveChanges` interceptor is not currently justified.

## Customer Events

| Action | Trigger | Status |
|---|---|---|
| `customer.created` | Customer creation commits successfully | Implemented |
| `customer.updated` | Customer update commits successfully | Implemented |
| `customer.soft-deleted` | Customer soft delete commits successfully | Implemented |
| `customer.exported` | Customer export succeeds | Future export feature |

Customer list and details reads are not audited by product decision. Authorization must restrict access to customer details; future exports and document downloads remain auditable actions.

## Atomicity and Failure Policy

- SQL Server `EnableRetryOnFailure` is enabled for transient database and network failures.
- Create uses `AuditTransaction.ExecuteAsync` because the identity `Customer.Id` is available only after the customer insert. The customer insert and audit insert use two saves inside one retry-aware transaction.
- Create clears and rebuilds its tracked entities at the start of every retry attempt so a rolled-back identity insert is never reused from stale EF state.
- Update and soft delete use `SaveChangesAsync(acceptAllChangesOnSuccess: false)` inside `AuditTransaction`; tracked state is accepted only after commit or successful outcome verification.
- Every audited operation has a unique `OperationId`. If commit acknowledgement is lost, the persisted audit row proves that the complete SQL transaction committed and prevents a duplicate retry.
- `OperationId` protects retries within one server operation. It is not an HTTP `Idempotency-Key` contract for separately submitted requests.
- A failed mutation audit write rolls back the customer change.
- No outbox is required while audit events and business data use the same SQL Server database.

## Correlation and Actor Identity

Audited Customer endpoints resolve the correlation identifier at the request boundary:

```text
Activity.Current.Id ?? HttpContext.TraceIdentifier
```

They pass the resulting string explicitly to `IAuditEventWriter`. The persistence adapter does not depend on `IHttpContextAccessor` or ambient ASP.NET Core request state. This links audit rows with request traces and operational logs while keeping the writer usable and testable outside HTTP requests.

ASP.NET Core Identity has been selected but is not implemented. The current implementation therefore stores `ActorId = null`.

After Identity is implemented, `EfCoreAuditEventWriter` must store the authenticated `ApplicationUser.Id` from the name-identifier claim. `ActorId` intentionally remains free of a foreign key to the Identity user table so historical events survive account disablement or deletion. Authorization policies for audit access are still pending.

See `docs/Authentication.md` for the selected authentication boundary and implementation requirements.

## Security Rules

Audit rows must never contain:

- raw National ID or passport number
- sensitive-value hashes
- encrypted values
- passport scans or document content
- request or response bodies containing personal data

Current Customer events store `Details = null`. Future metadata may contain field names such as `Email` or `PhoneNumber`, but never old/new sensitive values.

## Integration Tests

Current SQL Server integration tests verify:

- all three implemented Customer actions write audit events
- operation ID, action, entity type, entity ID, timestamp, correlation ID, and current null actor are correct
- successful customer details reads do not add audit events
- raw sensitive values do not appear in audit details
- audit failure rolls back customer creation
- a lost commit acknowledgement preserves successful create and update results without duplicate writes
- migrations successfully create each isolated SQL Server test database

After authentication is implemented, add tests proving that `ActorId` contains the authenticated `ApplicationUser.Id` and that an expected authenticated audit write cannot silently persist a null actor.

## Production Readiness

The following must be completed or explicitly decided before production:

- Define which security-relevant and business-significant actions are audited. Routine list, details, pagination, search, refresh, and internal retry operations do not create audit events. Sensitive exports and document operations are audited separately when implemented.
- Define a retention, archival, and deletion policy based on business, legal, and compliance requirements. Audit rows must not be deleted using an arbitrary technical retention period.
- Keep recent events available in the primary database and define how older events are archived and retrieved when the retained volume justifies it.
- Delete or archive expired rows in bounded batches to avoid long locks and excessive transaction-log growth.
- Review indexes against the implemented audit query patterns. The unique `OperationId` index is required for commit verification; the remaining indexes must be justified by entity-history, time-range, actor, action, or correlation searches.
- Monitor audit row count, table and index size, insert latency, database storage, transaction-log growth, and backup/restore duration.
- Consider partitioning by `OccurredAt` only after measured volume and maintenance costs justify the additional complexity.
- Keep audit events in the same SQL Server database as business data while atomic writes are required. Moving them to separate storage requires an outbox or another reliable delivery mechanism.

## Future Scope

- booking create/update/cancel
- booking final-price changes
- payment create/update/refund/delete
- document upload/download/delete
- customer export
- sign-in and security-relevant authorization failures
