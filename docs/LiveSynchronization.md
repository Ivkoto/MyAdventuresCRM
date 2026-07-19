# SuiteCase Live Synchronization

## Status

Architecture decision accepted; implementation is deferred until the Travel Board feature is implemented.

- ASP.NET Core SignalR is the selected real-time transport.
- SQL Server and the HTTP API remain the source of truth.
- EF Core optimistic concurrency protects writes.
- SignalR updates connected clients after a successful commit.
- Full board reload is a recovery mechanism, not the normal update path.

## Scope

The first use case is multiple authenticated agents working on the same Travel Board.

Goals:

- show committed changes to other connected agents quickly
- update only the affected item in normal operation
- prevent one agent from silently overwriting another agent's work
- recover to a correct state after disconnects or missed messages

SignalR is a transport and notification mechanism. It does not replace persistence, domain validation, authorization, transactions, or concurrency control.

## Selected Architecture

```text
React client
    -> HTTP command with concurrency token
    -> Server validates and commits to SQL Server
    -> Server publishes canonical response DTO through SignalR
    -> Connected clients update the affected local item
```

Business writes remain HTTP API operations. The initial SignalR hub owns connection and group coordination, not duplicate create/update/delete business logic.

After a successful commit, endpoint/application code publishes through a typed `IHubContext`. Do not publish an uncommitted EF entity or trust a client-supplied representation as the final state.

## Client Update Model

Normal operation uses small server-authored events, for example:

```text
TravelBoardItemCreated(item DTO)
TravelBoardItemUpdated(item DTO)
TravelBoardItemDeleted(boardId, itemId)
```

The final event names and DTOs belong to the Travel Board slice and will be fixed with that implementation.

Client behavior:

- create/update event: upsert the item by ID
- delete event: remove the item by ID
- ignore events for boards that are not currently open
- never treat optimistic UI state as a committed server result
- use the response/event DTO as the canonical representation

Do not send the entire board after every change unless the feature's actual data shape proves that this is cheaper and simpler.

## Recovery Synchronization

The client reloads the current board in the background when:

- the SignalR connection reconnects
- the application returns from an extended background/suspended state
- an HTTP mutation returns `409 Conflict`
- an event cannot be applied safely to the current local state
- the user explicitly refreshes

This reload is a consistency fallback. Normal committed changes are applied directly from SignalR events without a full page or board reload.

SignalR messages are not the durable history of a board. A client that was disconnected must recover from the API/database snapshot.

## Concurrency

Use SQL Server `rowversion` through an EF Core concurrency token on mutable Travel Board records.

Expected flow:

1. The API returns an item and its opaque version token.
2. The client sends that token with an update/delete command.
3. EF Core includes the token in the SQL write condition.
4. If the record changed after it was read, EF Core reports a concurrency conflict.
5. The API returns `409 Conflict` without overwriting the newer value.
6. The client retrieves the current state and presents or reapplies the user's change according to the feature UX.

Prefer item-level concurrency where agents can independently edit different rows. A board-wide token would create unnecessary conflicts between unrelated item changes.

SignalR reduces how often agents edit stale state, but optimistic concurrency remains required because simultaneous writes can occur before either client receives an event.

## Hub and Group Design

Use one logical group per open board:

```text
travel-board:{boardId}
```

Rules:

- the hub requires authentication
- joining a group verifies authorization for that board
- group membership is only a delivery mechanism, not permission proof
- every HTTP mutation performs its own authorization check
- disconnect removes transient presence; reconnect performs authorization and joins again
- never accept an actor/user ID supplied by the client

ASP.NET Core Identity is the selected authentication source. See `docs/Authentication.md`.

## Publish Timing and Failure Policy

- Publish only after the database transaction commits.
- Do not roll back a committed business change merely because a live notification fails afterward.
- Log publication failures without including sensitive board/customer data.
- Clients recover through reconnect, visibility refresh, or explicit reload.

There is a small commit-to-publish failure window in this first design. A transactional outbox is not justified for the initial single-server Travel Board unless missed notifications prove operationally unacceptable. Add an outbox later if delivery must survive process crashes between commit and publication.

## Payload Rules

SignalR events use response DTOs, never EF entities.

Payloads should contain only what connected clients need:

- board and item identifiers
- canonical display/state fields
- concurrency token
- safe update metadata when required by the UX

Do not include:

- National ID or passport number
- protected values or sensitive hashes
- authentication tokens
- document contents or provider credentials
- fields the receiving user is not authorized to view

## Scaling

Initial deployment:

- one SuiteCase Server instance
- built-in SignalR connection management
- no Redis backplane or managed SignalR service

If the Server is later scaled to multiple instances, messages must reach clients connected to every instance. Reassess:

- Redis backplane for self-hosted infrastructure in the same data center
- Azure SignalR Service for an Azure-hosted deployment
- sticky-session requirements for the selected topology

Do not add scale-out infrastructure before multiple application instances are required.

## Observability and Audit

Operational logs should cover:

- connection/reconnection failures
- rejected group joins
- publish failures
- concurrency conflicts

Connection IDs are transient diagnostics and are not staff identities.

SignalR delivery is not itself a business audit event. The underlying successful Travel Board mutation should write the appropriate audit event in the same database transaction as the business change. Presence, reconnects, and routine delivery acknowledgements do not require audit rows.

## Required Tests

Server integration tests must verify:

- unauthenticated hub connections are rejected
- unauthorized users cannot join a board group
- an authorized connection receives a committed item event
- failed database writes publish no event
- a stale concurrency token returns `409` and preserves the newer value
- agents updating different items do not conflict unnecessarily
- event DTOs do not expose restricted data

Client tests must verify:

- create/update/delete events modify only the affected local item
- reconnect triggers a board snapshot reload
- `409 Conflict` triggers the selected conflict/reload UX
- duplicate or unusable events do not corrupt local state

## Deferred Scope

- live cursor or field-level editing presence
- pessimistic board/item locks
- chat or comments
- offline mutation queues
- CRDT/operational-transform editing for simultaneous free-text collaboration
- transactional outbox and durable event replay
- multi-instance SignalR scale-out

## Official References

- [ASP.NET Core SignalR overview](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0)
- [SignalR authentication and authorization](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0)
- [EF Core optimistic concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [SignalR production hosting and scaling](https://learn.microsoft.com/en-us/aspnet/core/signalr/scale?view=aspnetcore-10.0)
