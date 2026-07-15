# SuiteCase Customer Document Storage

## Status

- Not implemented.
- Planned as a separate vertical feature after the current Customer slice.
- pCloud is the intended external storage provider, subject to technical and legal verification.

## Architecture Decisions

- File contents are stored outside SQL Server.
- SQL Server stores document metadata and the external provider file identifier.
- Customer documents use a separate entity/table; do not add document columns to `Customer`.
- The frontend communicates only with the SuiteCase backend.
- pCloud credentials and API calls remain in a Server infrastructure adapter.
- Customer directory queries never call pCloud.
- Document lists are served from SQL metadata.
- File access is requested from pCloud only when a user opens or downloads a document.

## Planned Data Boundary

Expected SQL metadata:

- `CustomerId`
- external provider/file identifier
- original file name
- content type
- file size
- document category
- uploaded timestamp
- uploaded-by actor when authentication exists
- soft-delete timestamp/status

The final entity and column names will be decided with the feature implementation.

Do not store:

- file contents in the Customer row
- pCloud credentials
- permanent public URLs
- raw document contents in logs or audit details

## Access Flow

```text
Browser
  -> SuiteCase Server authorization
  -> SQL document metadata lookup
  -> pCloud API request when file access is needed
  -> short-lived/backend-mediated file response
```

Public links and upload links are disabled by policy unless a specific workflow is reviewed and approved.

## pCloud Requirements

- Use a server-side OAuth/API flow suitable for backend applications.
- Use the EU data region.
- Confirm the actual business-account region before go-live.
- Treat region migration as an operational/cost decision, not an application feature.
- Do not rely on marketing/help-page statements as contractual GDPR coverage.

## Compliance Prerequisites

Required before production:

- verify GDPR Article 28 processor agreement coverage in the signed contract
- verify data-region commitments in contractual terms
- define document retention and deletion policy
- define roles allowed to upload, view, download, and delete documents
- define incident and access-review procedures

## Audit Scope

Future audit actions:

- document uploaded
- document viewed/downloaded
- document metadata changed
- document soft-deleted

Audit records may contain document identifiers and safe metadata, but never file contents or sensitive document values.

## Required Tests

- metadata is persisted when upload succeeds
- provider failure does not create a completed document record
- authorization is required for file access
- customer directory loading does not call the storage provider
- file links are not stored permanently
- delete behavior updates SQL metadata and provider state consistently
- audit events contain no raw document content

## Pending Decisions

- exact pCloud API/OAuth integration
- upload size and file-type limits
- malware scanning strategy
- document categories
- transaction/recovery behavior when SQL and pCloud operations partially fail
- retention and hard-delete workflow

