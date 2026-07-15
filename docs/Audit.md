# SuiteCase Audit Logging

## Status

- Not implemented.
- Phase 1 scope is defined for the Customer slice.
- Authentication, actor identity, retention, and audit-view permissions are still pending.

## Purpose

Audit events provide an append-only record of important staff actions and sensitive-data access:

```text
who performed which action, on which record, and when
```

Audit logging does not replace:

- authorization
- operational logs
- exception logs
- business snapshots

## Security Rules

Audit rows must never contain raw or protected sensitive values.

Prohibited audit content:

- `NationalId`
- `PassportNumber`
- sensitive-value hashes
- encrypted values
- passport scans or document contents
- request or response bodies containing personal data

Allowed detail examples:

```text
Changed fields: Email, PhoneNumber
Sensitive fields changed: NationalId, PassportNumber
Customer sensitive details viewed
```

## Planned Event Model

Table: `AuditEvents`

| Field | Rule |
|---|---|
| `Id` | `long` identity |
| `Action` | Required stable dot-style action name |
| `EntityType` | Required, e.g. `Customer`, `Booking`, `Payment` |
| `EntityId` | Required string to support different identifier types |
| `ActorId` | Nullable until authentication is implemented |
| `CorrelationId` | Nullable request `traceId` used to correlate responses, logs, and audit |
| `Details` | Optional short metadata; no sensitive values |
| `OccurredAt` | Required UTC `DateTimeOffset` |

Recommended indexes:

- `(EntityType, EntityId, OccurredAt)`
- `OccurredAt`
- `Action`
- `ActorId`
- `CorrelationId`

## Architecture

```text
SuiteCase.Core
  -> audit contract and model

SuiteCase.Server
  -> EF Core persistence
  -> authenticated actor resolution
  -> HTTP correlation integration
```

Phase 1 direction:

- Use an `IAuditLogger` boundary.
- Use explicit audit calls while Customer is the only active API slice.
- Mutation audit writes must participate in the same database transaction as the business change.
- Reconsider a `SaveChanges` interceptor after multiple slices repeat mutation-audit mechanics.
- Sensitive read/export actions still require explicit logging because an EF interceptor cannot observe reads.

The final `IAuditLogger` method shape is intentionally deferred until the transaction behavior is designed.

## Customer Events

| Action | Trigger | Phase |
|---|---|---|
| `customer.created` | Customer creation succeeds | Phase 1 |
| `customer.updated` | Customer update succeeds | Phase 1 |
| `customer.soft-deleted` | Customer soft delete succeeds | Phase 1 |
| `customer.details-viewed` | A successful details response exposes decrypted identifiers | Phase 1 |
| `customer.exported` | Customer data export succeeds | Future export feature |

`GET /api/customers` is not audited because the directory response does not contain decrypted identifiers.

## Atomicity and Failure Policy

Mutation events (`created`, `updated`, `soft-deleted`) must be atomic with the related customer change:

- commit both the business change and audit event
- or roll back both

An outbox is not required for Phase 1 while audit rows use the same SQL Server database. Use a shared EF transaction or one `SaveChangesAsync` where the generated identifier permits it.

Pending decision before implementation:

- If the audit write for `customer.details-viewed` fails, decide whether the endpoint fails closed instead of returning decrypted details. Failing closed is the recommended compliance-safe behavior.

## Retention and Access

Required before production:

- retention period
- roles allowed to view audit records
- archival/deletion policy
- audit export policy
- database enforcement of append-only access

Audit rows are personal/operational data because they may identify staff through `ActorId`.

## Required Tests

Integration tests must verify:

- create writes `customer.created`
- update writes `customer.updated`
- delete writes `customer.soft-deleted`
- successful details view writes `customer.details-viewed`
- missing customer details do not write a view event
- action, entity type, entity id, timestamp, and correlation id are correct
- raw sensitive values never appear in `Details`
- failed mutation audit rolls back the business change
- `ActorId` is populated after authentication is introduced

## Future Scope

- booking create/update/cancel
- booking final-price changes
- payment create/update/refund/delete
- document upload/download/delete
- sign-in and security-relevant authorization failures

