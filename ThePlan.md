# CedAdventures CRM Plan (React + ASP.NET Core)

## 1) Product and Technical Direction

- Staff-facing back-office CRM.
- React + TypeScript frontend.
- ASP.NET Core Web API backend.
- One repo, one solution, one database, one deployment unit.
- Clean API boundary between client and server.
- In development:
  - Vite dev server for React.
  - ASP.NET Core SPA proxy integration.
- In production:
  - React build assets served by ASP.NET Core as static files.
  - API and UI under the same host/domain.

## 2) Solution Shape

- `CedAdventureCRM.Server` (ASP.NET Core Web API host + static file hosting for client build).
- `cedadventurecrm.client` (React + TypeScript + Vite).
- Add these class library projects:
  - `CedAdventureCRM.Domain`
  - `CedAdventureCRM.Application`
  - `CedAdventureCRM.Infrastructure`

### References

- `CedAdventureCRM.Server` references:
  - `CedAdventureCRM.Application`
  - `CedAdventureCRM.Infrastructure`
- `CedAdventureCRM.Application` references:
  - `CedAdventureCRM.Domain`
- `CedAdventureCRM.Infrastructure` references:
  - `CedAdventureCRM.Application`
  - `CedAdventureCRM.Domain`
- `cedadventurecrm.client` calls backend through HTTP API only.

## 3) Layer Responsibilities

### Domain

- Entities, value objects, business rules, and core calculations.
- No framework, no persistence, no UI logic.

### Application

- Use cases (commands/queries), validation, authorization checks, DTO contracts.
- Search/filter query models.
- Service interfaces:
  - current user context
  - document storage
  - encryption/protection services
  - clock/date providers

### Infrastructure

- EF Core + SQL Server.
- ASP.NET Core Identity integration.
- SaveChanges interceptor for audit fields.
- Document storage implementations.
- Security/encryption implementations.

### Server (Web API)

- API endpoints.
- Auth and role policies.
- Middleware, OpenAPI, static-file/fallback hosting.
- Composition root for DI.

### ClientApp (React)

- Screens, forms, list filtering, routing, API client calls.
- No business calculations that must be authoritative.
- UI only consumes server-calculated values for critical totals/rules.

## 4) Authentication and Authorization

- ASP.NET Core Identity for employees.
- Initial roles:
  - `Admin`
  - `Agent`
  - `Accountant`
  - `ReadOnly`
- Same-domain app: cookie auth as default starting point.
- Role-based policy checks on API endpoints and document access.

## 5) Data and Security Core Decisions

- SQL Server as primary DB.
- Sensitive identity fields protected at rest:
  - `NationalId` (EGN)
  - `PassportNumber`
- Keep normalized hash fields for exact-match lookup on protected values.
- No passport scans/files in relational DB.
- Store file metadata in DB and files in controlled external/private storage.

## 6) Currency and Pricing Decisions

- BGN is deprecated operationally.
- EUR is the main operational currency.
- Keep `Currency` columns on money tables for rare non-EUR edge cases.
- Phase 1 has no automatic FX engine.
- Booking is a snapshot:
  - `FinalPriceAmount` is persisted.
  - `TotalDiscountAmount` is persisted as booking-level summary.
- `PaidAmount` and `RemainingAmount` are computed read-model values from `Payment` rows.

## 7) Domain Model (Phase 1)

### Customer

- Fields:
  - `Id`
  - names in Cyrillic + Latin
  - `NationalId` (optional, sensitive)
  - `DateOfBirth`
  - `PassportNumber`
  - `PassportExpiresOn`
  - `Email`
  - `Phone`
  - `ResidenceCountry`
  - `ResidenceCity`
  - `AddressLine`
  - `Notes`
  - audit fields
- Not stored (computed):
  - `Age`
  - `PassportValidityToday`
  - `PassportValidityForSelectedDeparture`
  - `PastTrips`
  - `UpcomingTrips`
  - `TotalTrips`
  - `ApplicableDiscount`
- Constraints:
  - `Email` not unique.
  - `PassportNumber` not globally unique.
  - `NationalId` can be missing/wrong; avoid hard unique enforcement in v1.

### Program (base product)

- `Id`, `Name`, `BaseStartDate`, `BaseEndDate`, `Destination`, `BasePriceAmount`, `BasePriceCurrency`, `OrganizerName`, `Description`, `Notes`, audit fields.

### ProgramOption

- `Id`, `ProgramId`, `Name`, `Code`, `Description`, `Notes`, audit fields.
- `Code` is stable manual uppercase snake case, never auto-regenerated.
- Unique: `(ProgramId, Code)`.

### Departure (real selectable group)

- `Id`, `ProgramId`, `Name`, `StartDate`, `EndDate`, `DepartureLocation`, `ReturnLocation`, `CustomerContactName`, `GuideName`, `Description`, `Notes`, audit fields.

### DepartureOptionPrice

- `Id`, `DepartureId`, `ProgramOptionId`, `PriceAmount`, `PriceCurrency`, `Notes`, audit fields.
- Unique: `(DepartureId, ProgramOptionId)`.
- Total display price:
  - `Program.BasePriceAmount + Sum(DepartureOptionPrice.PriceAmount)`.

### ProgramPricingRule (defaults catalog)

- `Id`, `ProgramId`, `DepartureId?`, `Kind` (`Discount|Surcharge`), `Code`, `Name`, `Amount`, `Currency`, `AppliesTo` (`PerBooking|PerPerson|PerNight`), `IsOptional`, `Notes`, audit fields.
- Used to prefill booking items.
- Final customer-specific financial truth lives in `BookingItem`.

### LoyaltyDiscountRule

- Supports trip-count based discounts, date changes, and destination-sensitive differences.
- Fields:
  - `Id`
  - `Name`
  - `EffectiveFrom`
  - `EffectiveTo?`
  - `MinCompletedTrips`
  - `MaxCompletedTrips?`
  - `DestinationScope?`
  - `ProgramPriceMinAmount?`
  - `ProgramPriceMaxAmount?`
  - `DiscountAmount`
  - `Currency`
  - `Priority`
  - `Notes`
  - audit fields

### Booking (snapshot root)

- `Id`, `CustomerId`, `DepartureId`, `BookedOn`, `Status` (`Reserved|PartiallyPaid|Paid|Cancelled`), `ParticipantType` (`Adult|Child|Infant`), `BasePriceAmount`, `Currency`, `TotalDiscountAmount`, `FinalPriceAmount`, `TicketSentStatus`, `ContractStatus`, `AnnexStatus`, `RoomType?`, `Notes`, audit fields.

### BookingOption (snapshot)

- `Id`, `BookingId`, `ProgramOptionId`, `PriceAmountAtBooking`, `Currency`, `OptionNameAtBooking`.
- Unique: `(BookingId, ProgramOptionId)`.

### BookingTravelLeg

- `Id`, `BookingId`, `Direction` (`Outbound|Return`), `Location`, `TravelDateTime`, `Notes`, audit fields.
- Unique: `(BookingId, Direction)`.

### BookingItem (snapshot adjustments)

- `Id`, `BookingId`, `ProgramPricingRuleId?`, `Type`, `Description`, `Kind` (`Charge|Discount`), `Amount`, `Currency`, `IsIncludedInTotal`, `Notes`, audit fields.

### PaymentMilestone (plan)

- `Id`, `DepartureId`, `Sequence`, `Name`, `DueBy`, `Amount`, `Currency`, audit fields.
- Unique: `(DepartureId, Sequence)`.
- Represents expected plan, not actual money movement.

### Payment (actual money)

- `Id`, `BookingId`, `Amount`, `Currency`, `PaidOn`, `PaymentMethod`, `Source`, `ExternalReference`, `Direction` (`Payment|Refund`), `Notes`, `CreatedAt`, `CreatedByUserId`.
- Direction + positive amount model.

### CustomerDocument

- `Id`, `CustomerId`, `DocumentType`, `StorageProvider`, `StorageKey`, `AccessPolicy`, `OriginalFileName`, `MimeType`, `FileSizeBytes`, `UploadedAt`, `UploadedByUserId`, optional delete fields.

### DocumentAccessLog

- `Id`, `CustomerDocumentId`, `Action`, `PerformedByUserId`, `PerformedAt`, `Notes`.

## 8) API Design Direction

- Use versioned REST-like endpoints under `/api`.
- Keep endpoint contracts explicit (DTOs), not entity exposure.
- Return server-calculated fields for critical pricing/discount outputs.
- Keep filtering/paging/sorting on API endpoints, not on client-side large datasets.
- Keep OpenAPI enabled for contract visibility and future typed client generation.

## 9) Search and Filtering (Early Requirement)

- Server-side paging/sorting/filtering from day 1.
- Query logic in `Application` query handlers/services.
- Add indexes for frequently searched fields:
  - customer names
  - phone/email
  - trip dates
  - booking status/date
  - hashes for sensitive exact-match lookups
- Start with standard indexed search; introduce full-text search only if needed.

## 10) Audit Strategy

### v1 (required)

- `CreatedAt/CreatedBy`, `UpdatedAt/UpdatedBy`, optional delete audit fields on major entities.
- SaveChanges interceptor populates user-linked audit fields automatically.

### v2 (optional)

- Full `AuditLog` with `ChangesJson` diffs per entity operation.

## 11) Delivery Plan (Execution Order)

1. Reshape solution into Domain/Application/Infrastructure + Server + ClientApp.
2. Wire DI, EF Core, SQL Server, Identity, and base auditing infrastructure.
3. Implement customer module with secure sensitive-data handling and search.
4. Implement program/program option/departure/departure option price.
5. Implement pricing-rule and loyalty-discount modules.
6. Implement booking snapshot flow:
   - booking root
   - booking options
   - booking travel legs
   - booking items
7. Implement payment milestones and payments.
8. Implement document metadata, storage abstraction, and document access logging.
9. Add role-based admin capabilities and audit visibility.
10. Harden production config, retention, backup, and operational checks.

## 12) Test Strategy

- Backend:
  - domain unit tests for formulas and discount matching.
  - application tests for handlers/use cases and validation.
  - infrastructure tests for mappings, interceptors, and storage adapters.
- Frontend:
  - component tests for critical forms and list behavior.
  - integration-level UI tests for booking creation/edit and payment flows.
- API tests:
  - auth policy checks.
  - pagination/filtering correctness.
  - snapshot immutability behavior after catalog changes.

## 13) Acceptance Criteria (Phase 1)

- Staff can authenticate and work according to role permissions.
- Customer module supports reliable search/filter and secure sensitive field handling.
- Staff can maintain program catalog, options, departures, and departure option prices.
- Staff can create bookings that persist frozen commercial snapshots.
- Staff can register payments and view computed paid/remaining balances.
- Staff can upload/access customer documents via controlled storage with access logging.
- System does not depend on Excel import, public portals, or separate microservice deployment.

## 14) Deferred Items

- Public portal/customer self-service.
- Offline/PWA features.
- Automatic FX conversion engine.
- Full historical field-diff audit log (if needed beyond v1 audit fields).
- External integration API surface beyond internal app needs.
