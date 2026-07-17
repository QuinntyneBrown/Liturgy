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

## 9. Lifecycle operations: keep the domain names, soft-hide via Status, hard-delete separately

**Decision.** Add lifecycle operations to accounts, projects, and cards without renaming the domain. The product's "Account" stays the `Workspace` entity and the "Ticket" stays the `Card`. Projects and cards gain a lifecycle `Status` — `ProjectStatus {Active, Closed}` and `CardStatus {Open, Closed, Cancelled}` — with a **soft-hide** semantics (Close/Cancel retain the record but drop it from the default project list or active board), kept distinct from an explicit, irreversible **hard delete** (`DELETE`, which removes the entity and its children). `CardStatus` is **independent of `BoardColumn`**: Close and Cancel are allowed from any 5R state and do not assert a complete loop, whereas the `Done` column remains reachable only through a complete 5R loop.

**Why.** Keeping `Workspace`/`Card` as the canonical nouns avoids a churny rename across the domain, DTOs, and Angular contract while still letting the UI speak "Account"/"Ticket". Soft-hide preserves history and is reversible (Reopen), which fits the reflective, relationship-oriented product; hard delete stays available for genuine removal. Making `CardStatus` orthogonal to the Done gate keeps the enforcement engine's one hard rule — Done requires a complete loop — intact, while letting teams retire work that will never finish without pretending it did.

**Consequences.** Default project-list and board queries filter by `Status` (an `includeClosed` option reveals closed projects); Reopen restores a soft-hidden item to its list/board with its state intact. Delete cascades to children (phases/gates/requirements/sprints/cards/movements for a project; the five movements for a card). Close/Cancel must not run the Done-gate check.

## 10. Token-based in-app invitations, no email infrastructure

**Decision.** Grow an account by invitation: a workspace `Lead` creates an `Invitation` (email, optional role, a token, and `InvitationStatus {Pending, Accepted, Revoked}`) through `/api/invitations`. There is **no email delivery** — the API returns the token and an invite URL that the Lead shares out of band. An existing authenticated user accepts via `POST /api/invitations/{token}/accept`; a brand-new user joins by passing the token as `RegisterRequest.invitationToken`, which adds them to the inviting account instead of provisioning a personal one. An anonymous `GET /api/invitations/{token}` returns just `{ workspaceName, invitedByName, email }` so the sign-up screen can show invite context.

**Why.** In-app tokens let us prove the full multi-user, multi-account membership flow without standing up (or securing) mail infrastructure in the slice. Supporting both accept-as-existing-user and join-on-registration covers the two real entry points, and the anonymous context lookup lets the sign-up screen explain who invited whom without exposing anything sensitive.

**Consequences.** Invite URLs must be treated as secrets (anyone with the token can view its context and, if the email matches, join on registration). Registration branches on the token: valid + email-matching → join inviting account (role from the invite); otherwise → provision a personal root account. Accept and revoke are guarded — only a Lead invites or revokes — and a `404` is returned for tokens that are missing, already accepted, or revoked. Adding real email delivery later is an additive change behind the same endpoints.

## 11. Marketing pages are static, served beside the SPA — not Angular routes

**Decision.** Production marketing pages are static HTML — the redesigned splash and brochure PDF that live under `docs/mocks` today — promoted to a first-class `marketing/` folder and served from the same App Service at `/`, with the SPA's entry file renamed (`index.html` → `app.html`) so the API fallback serves the app for `/sign-in`, `/dashboard`, and the other app routes. The in-app `LandingComponent` retires. A future split to a separate static-hosting origin (`www.` + `app.`) is documented but not scheduled. See [marketing-deployment.md](marketing-deployment.md).

**Why.** Marketing needs crawlable HTML, a light first paint, and a copy-edit cadence decoupled from app releases; the production-quality static splash already exists, so porting it into Angular would duplicate finished work into a worse delivery vehicle. Serving it same-origin adds zero infrastructure and keeps the no-CORS, relative-`/api` contract intact (see #6).

**Consequences.** The index-file collision is resolved with Angular's `index.output` option (validated against `ng build` and `ng serve`); the wildcard route retargets to `/dashboard`; marketing CTAs link to SPA routes (`/sign-up`, `/sign-in`); `liturgy.css` exists in both `docs/mocks` (frozen design reference) and `marketing/` (live copy). Implementation also required an explicit `UseRouting()` after the static-file middleware — the implicit early route matching let the SPA fallback endpoint claim `/` before `UseDefaultFiles` could rewrite it. Phase 1 shipped 2026-07-16; see [marketing-deployment.md](marketing-deployment.md) §9 for the full change set.
