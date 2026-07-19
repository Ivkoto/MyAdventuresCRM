# SuiteCase API Error Handling

## Status

Implemented:

- Minimal API validation through `AddValidation()`
- `ProblemDetails` registration through `AddProblemDetails()`
- safe non-Development `500 ProblemDetails` through `UseExceptionHandler()`
- Customer `404` and `409` problem responses
- stable Customer error codes
- `traceId` correlation
- source-generated Customer endpoint logging
- Customer audit correlation through `AuditEvent.CorrelationId`

Not implemented:

- authentication and final `401` / `403` contracts
- durable centralized log storage
- generic cross-slice problem factory

## Response Contracts

| Status | Response | Usage |
|---|---|---|
| `400` | `ValidationProblemDetails` | Input and business validation |
| `401` | Pending | Unauthenticated request |
| `403` | Pending | Authenticated but unauthorized request |
| `404` | `ProblemDetails` | Missing or soft-deleted resource |
| `409` | `ProblemDetails` | Business/data conflict |
| `500` | `ProblemDetails` | Unexpected server failure |

Normal API errors use `application/problem+json`. Do not return plain string errors.

The framework-generated `type` value links to the corresponding RFC 9110 status section. It is response metadata, not an application error code.

## Validation Errors

Validation failures return field-level errors:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "firstName": ["The FirstName field is required."]
  }
}
```

Rules:

- React uses `errors` for per-field messages.
- Validation responses do not currently include Customer business `code` values.
- Customer-specific validation currently covers unsupported residence country codes.

## Customer Business Errors

Customer HTTP error construction is centralized in:

- `Features/Customers/ErrorHandling/CustomerHttpResultProblems.cs`
- `Features/Customers/ErrorHandling/CustomerValidationProblem.cs`

Sensitive-identifier conflict detection is implemented in:

- `Features/Customers/Queries/CustomerQueries.cs`
- `Features/Customers/Queries/SensitiveIdentifierConflict.cs`
- `Features/Customers/Queries/SensitiveIdentifierConflictKind.cs`

Stable Customer codes live in the `CustomerErrorCodes` type beside `CustomerHttpResultProblems`.

| Code | Status | Trigger | Additional extension |
|---|---:|---|---|
| `customer.not_found` | `404` | GET/PUT/DELETE cannot find an active customer | none |
| `customer.duplicate_national_id` | `409` | Pre-check or recognized post-save race finds an active National ID duplicate | `existingCustomerId` |
| `customer.duplicate_passport_number` | `409` | Pre-check or recognized post-save race finds an active passport-number duplicate | `existingCustomerId` |

React must branch and localize on `code`, not parse `title` or `detail`.

`existingCustomerId` is included when either the pre-check or the post-save race check identifies the conflicting customer.

No generic fallback error code is exposed for sensitive-identifier conflicts. A recognized National ID or passport race returns the corresponding specific code. An unrecognized unique-constraint violation is rethrown and handled by the global exception pipeline instead of being reported as an unrelated Customer conflict.

Current limitation: authentication is not implemented. Exposing `existingCustomerId` assumes the API will be restricted to authorized CRM staff before production use.

## Global Exception Handling

Registration:

```csharp
builder.Services.AddProblemDetails();
```

Pipeline:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}
```

Behavior:

- Development keeps detailed developer diagnostics.
- Non-Development returns a generic `500 ProblemDetails` response.
- Stack traces and infrastructure details stay out of responses.
- The framework adds `traceId` to global exception responses.

Example failures covered by this path include Data Protection key-ring mismatch during `Unprotect`.

## Ownership Boundaries

`CustomerHttpResultProblems` owns Customer `404` and `409` response contracts:

- status
- title and detail
- stable error code
- Customer-specific extensions
- endpoint-local `traceId`

`CustomerValidationProblem` owns Customer-specific `400 ValidationProblem` responses.

`SqlServerExceptionClassifier` identifies SQL Server unique-constraint violations. It does not identify the conflicting Customer field or construct HTTP responses.

After a unique-constraint failure, `CustomerQueries.FindSensitiveIdentifierConflictAsync` queries active Customers by the attempted hashes. It returns a `SensitiveIdentifierConflict` containing:

- `Kind`: `NationalId` or `PassportNumber`
- `ExistingCustomerId`: the conflicting active Customer id

For updates, the current Customer id is excluded from this query. `CustomerHttpResultProblems.FromSensitiveIdentifierConflict` maps the structured result to the specific `409 ProblemDetails` response.

If the unique constraint cannot be attributed to National ID or passport number, the endpoint rethrows the original exception. This prevents future unrelated unique constraints from being misreported as sensitive-identifier conflicts.

`CustomerEndpointLogger` owns source-generated operational log messages for Customer endpoint outcomes and failures.

Do not add a generic cross-slice problem abstraction until another vertical slice repeats the same response mechanics. Expected business outcomes can remain endpoint-local typed results. Reassess central exception translation when reusable application services introduce repeated domain/application failures.

## Sensitive Data Rules

Never include these values in API errors or operational logs:

- raw `NationalId`
- raw `PassportNumber`
- HMAC hashes
- encrypted values
- full request bodies containing personal data

Human-readable `title` and `detail` describe the error without echoing submitted identifiers.

## Logging and Correlation

`code` and `traceId` serve different purposes:

- `code`: UI branching and localization
- `traceId`: support correlation across responses, operational logs, and audit events

Endpoint-local Customer problems add:

```csharp
Activity.Current?.Id ?? httpContext.TraceIdentifier
```

Recognized unique-index race logs include only the structured conflict kind:

```text
NationalId
PassportNumber
```

The raw identifier, its hash, and its protected value are never logged.

Operational logs are currently emitted through the default ASP.NET Core logging providers. No durable production log sink or retention policy is configured yet.

## Current Test Coverage

Integration tests verify:

- field-level validation `400`
- duplicate National ID/passport `409` with stable code and existing id
- Customer `404` with `code` and `traceId`
- safe non-Development `500` with `traceId`
- date-of-birth derivation when the supplied value is missing
- residence-country validation
- SQL Server unique-constraint classification through a real unique-index violation
- safe `500 ProblemDetails` when an audit write fails

## Pending Work

- define `401` and `403` response contracts with authentication
- configure durable production log storage and retention
- add a focused concurrent integration test that forces two endpoint requests through the unique-constraint race window
- decide whether shared problem mechanics are justified after the next API slice
