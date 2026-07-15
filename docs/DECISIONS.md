# Architecture Decisions

A running log of technical and product decisions worth recording. Each entry states the decision, why it was made, and its consequences. Newer decisions may supersede older ones; superseded entries are kept for history.

## 1. Enforcement is server-authoritative

**Decision.** The rules that gate 4D phases and require the 5R loop live in the server-side enforcement engine, not the client. The API rejects illegal transitions with typed exceptions.

**Why.** Enforcing the FaithTech Playbook is the product's reason to exist. A client-side check can be skipped, so process could be bypassed. Locking states in the UI are a reflection of server truth.

**Consequences.** Enforcement changes require backend tests. Frontend "locked" affordances are presentation only and must match server behavior.

## 2. .NET 9 with Clean Architecture and MediatR / CQS

**Decision.** Build the backend as four layers — Domain, Application, Infrastructure, Api — with MediatR commands and queries and thin controllers, on .NET 9 pinned via `global.json`.

**Why.** The layering keeps the domain and enforcement rules free of framework concerns and testable in isolation. MediatR/CQS matches proven reference patterns and gives each use case a single handler. .NET 9 aligns with the installed `dotnet-ef` tooling.

**Consequences.** One public type per file. Adding a use case means a command/query, a handler, a validator, and a controller action.

## 3. SQL Server via EF Core

**Decision.** Persist through Entity Framework Core against SQL Server, defaulting to a local `.\SQLEXPRESS` instance, with forward migrations checked in.

**Why.** Relational integrity fits the project → phase → gate → requirement and card → movement structure, and EF Core migrations give a reproducible schema. Integration tests run against a real database for fidelity.

**Consequences.** Contributors need a reachable SQL Server instance; the test instance is overridable with `LITURGY_TEST_SQLSERVER`. Schema changes require a migration.

## 4. JWT bearer authentication

**Decision.** Authenticate with signed JWT access tokens issued by the API.

**Why.** Stateless bearer tokens are simple for a single-page client and a REST API, and they carry the identity the enforcement engine needs.

**Consequences.** The signing key must be a strong secret in any non-local environment. The development key in `appsettings.json` is a placeholder only (see [SECURITY.md](../SECURITY.md)).

## 5. SignalR for real-time collaboration

**Decision.** Broadcast board, loop, and checklist changes over a SignalR hub at `/hubs/board`, behind the `IRealtimeNotifier` abstraction.

**Why.** Multiple members edit the same board and loop concurrently, so allowed transitions must appear to collaborators without a refresh. The abstraction keeps the Application layer free of SignalR.

**Consequences.** Handlers notify after persisting. The frontend merges pushed updates into its views.

## 6. Angular library workspace

**Decision.** Structure the frontend as one application (`liturgy-app`) and three libraries (`@liturgy/api`, `@liturgy/components`, `@liturgy/domain`), with the API origin injected through `API_BASE_URL`.

**Why.** Separating services, UI, and types clarifies boundaries and keeps hostnames out of library code, which also makes the libraries testable and reusable.

**Consequences.** The application composes the libraries and provides configuration; libraries do not read global configuration directly.

## 7. Playwright against a faked backend

**Decision.** Run the default end-to-end suite with a faked backend using the Page Object Model.

**Why.** Deterministic, fast UI journeys that do not require the API or a database run anywhere, including CI, without external services.

**Consequences.** The faked backend must stay faithful to the real API contract. Full-stack verification against the running API is a separate, optional step.

## 8. Vertical slice first

**Decision.** Build one end-to-end path through the core — Auth → Project → 4D phases and gates → Develop board → 5R loop → enforcement → real time → tests — before broadening surface area.

**Why.** Proving the enforcement engine end to end de-risks the product's differentiator before investing in the marketing cover, dashboard aggregation, the standalone Discern screen, and the Demonstrate surfaces.

**Consequences.** Some screens shown in `docs/mocks` are intentionally deferred; see the [implementation plan](implementation-plan.md).
