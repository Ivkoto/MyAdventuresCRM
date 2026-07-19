# SuiteCase Authentication and Authorization

## Status

Architecture decision accepted; implementation is pending.

- ASP.NET Core Identity is the selected authentication system.
- Identity data will use EF Core and the existing SQL Server database.
- The React browser client will use secure cookie authentication.
- Microsoft Entra ID and other external identity providers are outside the current scope.
- Public self-registration will not be available.

This document defines the implementation boundary. It does not mean that the current API is protected yet.

## Scope

Authentication applies to travel-agency staff accounts. A staff identity is not a Customer and must remain a separate model.

```text
ApplicationUser
    -> authenticates a CRM staff member

Customer
    -> represents a travel-agency customer
```

No relationship between these types is required merely because a staff member creates or edits a Customer.

## Selected Architecture

- Use ASP.NET Core Identity for users, password hashing, lockout, reset tokens, roles, claims, and MFA support.
- Use `Microsoft.AspNetCore.Identity.EntityFrameworkCore` with the version matching the ASP.NET Core/EF Core runtime.
- Keep the Identity persistence model in `SuiteCase.Server`; it is infrastructure, not a Customer domain entity.
- Store Identity tables in the SuiteCase SQL Server database and manage them through EF Core migrations.
- Use a minimal `ApplicationUser` type. Add profile fields only when a real staff-account requirement exists.
- Use policy-based authorization at API and SignalR boundaries. Roles or claims may satisfy policies, but endpoint code must not spread direct role-name checks.

The simplest implementation can extend the existing context:

```text
SuiteCaseDbContext : IdentityDbContext<ApplicationUser>
```

A separate authentication database or DbContext is not justified for the current single-application deployment.

## Account Lifecycle

- Staff accounts are provisioned by an authorized administrator or a controlled bootstrap process.
- Do not expose an anonymous registration endpoint.
- Login, logout, password reset, account disablement, and MFA recovery require explicit workflows.
- Account deletion or disablement must not delete historical audit events.
- The initial administrator bootstrap must be repeatable, secret-safe, and disabled after setup.

Exact role names, the role-to-policy matrix, password-reset delivery, session duration, and MFA enforcement scope must be agreed during implementation. They are not silently fixed by this document.

## Browser Authentication

Use the ASP.NET Core Identity application cookie for the React client.

Required rules:

- production cookies use `HttpOnly` and `Secure`
- use an appropriate `SameSite` policy for the final deployment topology
- prefer serving the SPA and API from the same origin
- do not store bearer or refresh tokens in `localStorage` or `sessionStorage`
- protect cookie-authenticated state-changing requests against CSRF
- use HTTPS outside local development
- persist and back up the ASP.NET Core Data Protection key ring before production

Identity API endpoints may be used where their exposed behavior matches the CRM requirements. Do not map the default anonymous registration flow without restricting or replacing it.

## API Pipeline

Authentication and authorization middleware must run in this order:

```text
UseAuthentication
UseAuthorization
Map protected API endpoints and SignalR hubs
```

Customer and future business APIs must require an authenticated user. Anonymous access should be limited to explicitly selected authentication/recovery endpoints and public application assets.

Expected HTTP behavior:

| Situation | Status |
|---|---:|
| Missing or invalid authentication | `401 Unauthorized` |
| Authenticated user lacks the required policy | `403 Forbidden` |
| Authenticated and authorized request | Endpoint-specific result |

Final `401` and `403` ProblemDetails contracts must be synchronized with `docs/ErrorHandling.md` during implementation.

## Audit Integration

`ApplicationUser.Id` is the stable staff identifier selected for `AuditEvent.ActorId`.

After Identity is implemented:

- protected Customer endpoints require authentication
- `EfCoreAuditEventWriter` reads the authenticated name-identifier claim
- successful audited actions store the `ApplicationUser.Id` value in `ActorId`
- an expected authenticated audited request must not silently produce a null actor
- `AuditEvent.ActorId` remains a scalar historical value and has no foreign key to the Identity user table

The missing foreign key is intentional: disabling or deleting a staff account must not remove or invalidate its audit history. See `docs/Audit.md`.

## SignalR Integration

The selected Identity cookie also authenticates the browser's SignalR connection when the SPA and hub use the same application origin.

- SignalR hubs require authorization.
- Hub group membership is not an authorization boundary.
- Joining a Travel Board group must verify the user's policy and resource access.
- The server derives user identity from the authenticated connection, never from a client-supplied actor ID.

See `docs/LiveSynchronization.md`.

## Security Requirements

- Never log passwords, reset tokens, MFA secrets, recovery codes, authentication cookies, or complete request bodies.
- Configure lockout protection and rate limits for authentication endpoints.
- Use TOTP/authenticator-app MFA rather than SMS when MFA is enabled.
- Rotate compromised credentials and invalidate existing sessions through Identity security-stamp behavior.
- Keep development bootstrap credentials out of source control and migrations.
- Review cookie lifetime, sliding expiration, password policy, account recovery, and MFA policy before production.

## Required Tests

Integration tests must cover:

- valid login establishes the expected cookie session
- invalid credentials do not authenticate
- anonymous business API requests return `401`
- authenticated users without a required policy return `403`
- authorized users reach the endpoint
- anonymous public registration is unavailable
- disabled/locked accounts cannot sign in
- state-changing requests enforce the selected CSRF protection
- an authenticated Customer action writes the Identity user ID to `AuditEvent.ActorId`
- SignalR rejects unauthenticated connections and unauthorized board access

Test account creation must use Identity APIs such as `UserManager`; tests must not insert password hashes directly.

## Implementation Sequence

1. Add the Identity EF Core package and minimal `ApplicationUser`.
2. Integrate Identity persistence with `SuiteCaseDbContext` and add a reviewed migration.
3. Register Identity, cookie options, authentication, authorization, and CSRF protection.
4. Add the controlled account provisioning and login/logout flows.
5. Protect Customer endpoints and define the first authorization policies.
6. Populate `AuditEvent.ActorId` from the authenticated Identity user.
7. Add integration tests for authentication, authorization, audit attribution, and safe failures.
8. Persist the Data Protection key ring and finalize production account-recovery/MFA operations.

## Deferred Decisions

- exact roles and policy matrix
- whether MFA is mandatory for all staff or selected privileged roles
- email provider and password-reset delivery
- session lifetime and sliding-expiration values
- permanent account-disablement representation
- future Microsoft Entra ID or another external login provider

## Official References

- [Introduction to ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0)
- [Identity for a Web API backend used by an SPA](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
- [ASP.NET Core authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-10.0)
- [SignalR authentication and authorization](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0)
