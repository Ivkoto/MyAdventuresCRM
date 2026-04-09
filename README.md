# SuiteCase - Travel Agency CRM

SuiteCase is a staff-facing CRM for travel agencies, focused on customers, departures, bookings, payments, and documents.

> Naming note: the repository/projects currently still use `SuiteCase` in technical names and namespaces. Product name is now **SuiteCase**.

## Why This Project
Travel agencies often operate with spreadsheets and disconnected tools.
SuiteCase is designed to centralize daily operations into one clear workflow with production-oriented architecture decisions.

## Why SuiteCase Is Different

SuiteCase is not designed as a generic sales CRM with leads and deal pipelines.
It is designed as a **travel agency operations CRM** focused on executing trips correctly, not just selling them.

### Core Differentiators

- Travel-operations-first model:
  - Customers, programs, departures, bookings, payments, and documents are first-class workflows.
- Booking snapshot logic:
  - Price, selected options, and discount context are preserved at booking time for auditability and consistency.
- Operational readiness tracking:
  - Contract status, annex status, ticket status, and document completeness are part of daily operations.
- Payment-pressure visibility:
  - Payment milestones and alerts support proactive follow-up before departures.
- Built for controlled flexibility:
  - The system is being designed to support agency-specific rules and settings without turning into one-off chaos.

### Product Vision

SuiteCase starts as a focused solution for real agency workflows and evolves toward a reusable B2B/SaaS product.
The goal is not “another CRM,” but a **back-office operating system for travel agencies**.

## Current Status
Early-stage demo/prototype.
The UI and domain flow are actively shaped, but this is not production-ready yet.

## Scope (Phase 1)
- Customer directory and customer detail workflows
- Programs and departures
- Booking studio and quote snapshot logic
- Payment tracking and alerts
- Document workflow placeholders

## Architecture Direction
- Current: `Client + Server + Core` (pragmatic start)
- Goal: evolve safely to full Clean Architecture only when complexity requires it
- Delivery approach: single-agency runtime now, tenant-ready foundations for future SaaS

## Tech Stack
- React + TypeScript + Vite
- ASP.NET Core
- SQL Server (planned operational DB)

## What Is Implemented So Far
- Presentation-grade CRM UI flows
- Demo data model for customers/programs/departures/bookings
- Booking and customer detail screens with iterative UX refinements

## Screenshots (WIP)
Current UI screenshots are being curated and will be added in this section.

## Run Locally
### Prerequisites
- .NET SDK 10
- Node.js 20+ and npm

### Setup
```bash
dotnet restore SuiteCase.slnx
cd SuiteCase.Client
npm install
cd ..
```

### Start the app
```bash
dotnet run --project SuiteCase.Server
```

Notes:
- Server runs on `https://localhost:7295` (and `http://localhost:5245`).
- The client is served through ASP.NET Core SPA proxy in development.

## Contact
- Author: Ivaylo Kostov
- GitHub: https://github.com/IvayloKostov
- LinkedIn: https://www.linkedin.com/in/ikostov87/
- Email: ikostov87@gmail.com
- Phone: +359885986062

## License
This project is proprietary software. All rights reserved.

Public access to this repository does not grant permission to use, copy, modify, distribute, or reuse any part of the software without prior written consent from the copyright holder.

Copyright (c) 2026 Ivaylo Kostov. All rights reserved.




