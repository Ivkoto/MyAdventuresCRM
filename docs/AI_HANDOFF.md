# SuiteCase AI Handoff

## Purpose

Use this file to start a clean engineering session. It describes the current implementation, fixed technical decisions, and pending work.

## Product and Architecture

- Internal staff CRM for a travel agency.
- Phase 1 targets one real agency; multi-tenancy is not required.
- Solution structure: `SuiteCase.Core`, `SuiteCase.Server`, React client, unit tests, integration tests.
- Server uses vertical feature slices without full Clean Architecture.
- SQL Server and EF Core migrations are the database source of truth.
- .NET NuGet versions are centralized in the root `Directory.Packages.props`; React/npm dependencies remain separate.
- Customer is the only implemented API slice.
- Travel Programs, Groups, Bookings, Payments, and Loyalty have database models but no API workflows yet.

## Implementation Status

| Area | Status |
|---|---|
| Customer CRUD | Implemented |
| Customer pagination and search | Implemented |
| Sensitive identifier protection/hash | Implemented |
| Customer error handling and operational logging | Implemented |
| Travel-domain EF model and migrations | Implemented |
| Travel-domain API workflows | Not implemented |
| Authentication/authorization | ASP.NET Core Identity selected; not implemented |
| Customer audit events / `IAuditEventWriter` | Implemented |
| Travel Board live synchronization | SignalR design selected; not implemented |
| Customer document storage / pCloud | Not implemented |
| Durable production log sink | Not configured |

## Repository Layout

```text
SuiteCase.slnx
src/
  SuiteCase.Core/
  SuiteCase.Server/
tests/
  SuiteCase.UnitTests/
  SuiteCase.IntegrationTests/
ui/
  SuiteCase.Client/
docs/
```

## Architecture Rules

- `Core` owns entities, enums, domain helpers, contracts, and business rules.
- `Server` owns Minimal APIs, EF Core, SQL Server, security implementations, error handling, and infrastructure adapters.
- `Client` owns UX; financial, security, and compliance rules remain server-side.
- API responses use DTOs, not EF entities.
- Keep changes vertical-slice based and avoid speculative shared abstractions.
- Do not add multi-tenant infrastructure before a real second-agency requirement.

## Customer API

| Method | Route | Success response |
|---|---|---|
| GET | `/api/customers` | `PagedResponse<CustomerShortDetailsResponse>` |
| GET | `/api/customers/{id:int}` | `CustomerDetailsResponse` |
| POST | `/api/customers` | `CreatedAtRoute<CustomerDetailsResponse>` |
| PUT | `/api/customers/{id:int}` | `CustomerDetailsResponse` |
| DELETE | `/api/customers/{id:int}` | `204 No Content` soft delete |

Key implementation paths:

```text
src/SuiteCase.Server/Features/Customers/CustomerEndpoints.cs
src/SuiteCase.Server/Features/Customers/CustomerFactory.cs
src/SuiteCase.Server/Features/Customers/DTO/
src/SuiteCase.Server/Features/Customers/ErrorHandling/
src/SuiteCase.Server/Features/Customers/Logging/
src/SuiteCase.Server/Features/Customers/Mapping/
src/SuiteCase.Server/Features/Customers/Queries/
src/SuiteCase.Server/Features/Customers/Validation/
src/SuiteCase.Server/Auditing/
src/SuiteCase.Core/Customers/
src/SuiteCase.Core/Countries/Countries.cs
src/SuiteCase.Core/Entities/AuditEvent.cs
```

### Directory Pagination and Search

Query parameters:

- `Page`: default `1`, range `1..1_000_000`
- `PageSize`: default `13`, range `1..100`
- `Search`: optional, maximum `100` characters

Stable ordering:

```text
FirstName -> LastName -> Id
```

Search behavior:

- partial match: Bulgarian names, Latin names, normalized phone number
- exact match only: National ID and passport number through HMAC hash columns
- search is applied before pagination
- current implementation uses EF Core/SQL Server; global cross-entity search is a future architecture decision

### Customer Business Rules

- `FirstName` and `LastName` are required.
- Residence country uses ISO alpha-2 codes.
- Default country is `BG`.
- Supported values are commonly recognized European countries; microstates are excluded for now.
- Unsupported country code returns validation `400`.
- Customer details include `ResidenceCountryCode` and `ResidenceCountryName`.
- Age is calculated at response time.
- Passport validity requires expiry on or after `today.AddMonths(6)`.
- Soft delete sets `DeletedAt`; global query filters hide deleted customers.

Date of birth:

- use the supplied value when present; it is authoritative because `NationalId` can contain an untyped foreign identifier
- otherwise derive it from a value that passes Bulgarian EGN structural and checksum validation
- passing EGN validation does not prove the identifier's issuing scheme
- values that fail EGN validation do not derive a date

### Sensitive Identifiers

Sensitive values:

- `NationalId`
- `PassportNumber`

Storage:

```text
Data Protection value -> reversible display/export
HMAC-SHA256 hash       -> exact lookup and active-row uniqueness
raw plaintext          -> never stored
```

Rules:

- callers normalize with `NormalizeSensitiveValue()` before `Protect` or `Hash`
- normalization is `Trim().ToUpperInvariant()`; blank values become `null`
- `Protect` and `Hash` expect already-normalized input
- filtered unique indexes apply only to active rows
- soft delete allows a new active customer to reuse the same identifier
- update clears protected/hash values when identifiers are removed
- HMAC key must remain stable after real data exists
- Data Protection key ring must be persisted and backed up before deployment

Configuration key:

```text
Security:SensitiveDataHashKey
```

### Errors and Logging

- Validation uses `ValidationProblemDetails`.
- Customer `404` and `409` responses use `ProblemDetails` with stable `code` values.
- Duplicate National ID and passport pre-checks return specific `409` codes with `existingCustomerId`.
- After a unique-index save conflict, `CustomerQueries.FindSensitiveIdentifierConflictAsync` rechecks the attempted hashes and returns a structured `NationalId` or `PassportNumber` conflict.
- Recognized save races return the same specific duplicate code and `existingCustomerId`; unrecognized unique constraints are rethrown to the global exception pipeline.
- Endpoint-local Customer problems include `traceId`.
- Non-Development unhandled exceptions return safe `500 ProblemDetails`.
- `CustomerEndpointLogger` uses source-generated logging methods; race logs include only the conflict kind, never the identifier or hash.
- No durable production log sink is configured.
- Authentication is required before the API is production-ready.

See `docs/ErrorHandling.md` for the complete contract.

### Customer Audit

- `IAuditEventWriter.Record` stages an event in the current scoped `SuiteCaseDbContext`.
- Implemented actions: `customer.created`, `customer.updated`, and `customer.soft-deleted`.
- Each audit row has a unique UUIDv7 `OperationId` used to verify ambiguous SQL commit outcomes.
- `AuditTransaction` executes audited writes through the configured EF execution strategy and verifies successful commits by `OperationId`.
- Create uses two saves because `Customer.Id` is database-generated; update and delete preserve tracked state until commit is confirmed.
- Customer list and details reads are not audited by product decision. Future sensitive exports and document downloads remain auditable actions.
- Audit rows store request correlation IDs and no sensitive values.
- ASP.NET Core Identity is selected but not implemented; `ActorId` remains `null` until the authenticated `ApplicationUser.Id` is available.

See `docs/Audit.md` for the complete contract and pending production requirements.

## Database

`SuiteCaseDbContext` contains:

- Customers
- TravelPrograms and TravelProgramOptions
- Groups, GroupOptions, GroupOptionalActivities
- TravelProgramPricingRules
- Bookings, BookingOptions, BookingOptionalActivities, BookingItems, BookingTravelLegs
- PaymentMilestones and Payments
- LoyaltyDiscountRules and LoyaltyDiscountRuleDestinations
- AuditEvents

SQL Server transient retries are enabled with `EnableRetryOnFailure`. Manually grouped database operations must run through the EF execution strategy; current audited writes use `AuditTransaction` for this purpose.

Important migrations:

- `FilterCustomerUniqueHashesByActiveRows`
- `Customer-Fixes`
- `AddTravelDomainModel`
- `EnforceGroupProgramIntegrity`
- `StandardizeTimestampsToDateTimeOffset`
- `RenameProgramTablesToTravelPrograms`
- `UseCustomerResidenceCountryCode`
- `AddAuditEvents`

Travel and booking rules are documented in `docs/Database-blueprint.md`.

## Testing

Customer integration tests are split by behavior:

- `CustomerCreateEndpointTests`
- `CustomerReadEndpointTests`
- `CustomerUpdateEndpointTests`
- `CustomerDeleteEndpointTests`
- `CustomerCrudFlowEndpointTests`
- shared setup in `CustomerEndpointTestBase`
- audit behavior in `CustomerAuditEventTests`

Test infrastructure:

- `WebApplicationFactory<Program>`
- `Testcontainers.MsSql`
- EF migrations against disposable SQL Server
- `FakeSensitiveDataProtector`
- Docker Desktop or another compatible Docker daemon must be running for container-backed integration tests.

Covered behavior includes:

- CRUD and soft delete
- duplicate identifiers and safe ProblemDetails contracts
- real SQL Server unique-constraint classification
- protected/hash storage
- date-of-birth resolution from supplied values and EGN-compatible identifiers
- country default/validation
- pagination, ordering, and search
- exact-only sensitive identifier search
- passport validity and age calculation
- global safe `500 ProblemDetails`
- atomic Customer mutation audit writes

Known test gap:

- no focused concurrent integration test currently forces two endpoint requests through the unique-index race catch path

Verification commands:

```powershell
dotnet build .\SuiteCase.slnx
dotnet test .\tests\SuiteCase.UnitTests\SuiteCase.UnitTests.csproj
dotnet test .\tests\SuiteCase.IntegrationTests\SuiteCase.IntegrationTests.csproj
dotnet ef migrations list --project .\src\SuiteCase.Server --startup-project .\src\SuiteCase.Server
```

## Technical Documentation

- `docs/Database-blueprint.md`: implemented EF/database model and integrity rules
- `docs/ErrorHandling.md`: current API error and logging contract
- `docs/Audit.md`: implemented Customer audit architecture and pending production requirements
- `docs/Authentication.md`: selected ASP.NET Core Identity architecture and pending implementation
- `docs/LiveSynchronization.md`: selected SignalR and EF Core concurrency design for Travel Board collaboration
- `docs/Documents.md`: pending pCloud/customer-document architecture

## Next Work

1. Implement ASP.NET Core Identity and authorization following `docs/Authentication.md`.
2. Populate `AuditEvent.ActorId` from the authenticated `ApplicationUser.Id`.
3. Define audit retention, access, export, and database append-only policies.
4. Configure production Data Protection key-ring persistence.
5. Configure durable production log storage and retention.
6. Implement the next vertical slice: Travel Programs, Groups, and Options.
7. Implement Travel Board concurrency and SignalR synchronization with the Travel Board workflow.
8. Design global cross-entity search before customer/history volume requires it.
9. Implement Customer documents through the backend pCloud adapter after compliance verification.

## Build Health

- `dotnet build .\SuiteCase.slnx` completes with zero warnings.
- NuGet reports no vulnerable or deprecated packages for the .NET projects.
- `npm audit` reports zero vulnerabilities for `SuiteCase.Client`.

## Git Conventions

- Branch prefixes: `feature/`, `fix/`, `test/`, `docs/`, `chore/`, `refactor/`, `codex/`.
- Use Semantic Versioning tags only at meaningful checkpoints.
- Do not branch, commit, push, or tag without explicit instruction.

## Non-Negotiable Constraints

- Never store raw National ID or passport number in SQL, logs, errors, or audit details.
- Never expose EF entities directly from APIs.
- Never expose pCloud credentials or permanent public document links to the frontend.
- Do not use startup `MigrateAsync()` as the staging/production migration strategy.
- Do not introduce broad abstractions or multi-tenancy without a demonstrated requirement.

