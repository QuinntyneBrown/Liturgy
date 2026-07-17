# Liturgy — Implementation Plan (Vertical Slice)

## Context

`C:\projects\Liturgy` currently holds two things: a set of static HTML mockups in
`docs/mocks` and a **bare, application-less Angular 21.2 workspace** in `frontend/`
(empty `projects: {}`, no `src/`, no test runner, no CDK/SignalR/Playwright). There
is no backend at all.

**What Liturgy is:** an agile workspace that *enforces* the FaithTech Playbook.
The product's differentiator is an **enforcement engine** — a server-authoritative
state machine where a project moves through the **4D cycle**
(Discover → Discern → Develop → Demonstrate) behind **gates**, and every work item
inside Develop must complete the **5R loop** (Request → Receive → Review → Render →
Rejoice) before it can reach Done. Gates stay *locked* until their requirement
checklist is complete and *auto-unlock* when it is. Multiple members edit the same
board, loop, and checklists concurrently, so state changes must broadcast in real
time.

**This plan builds a full-stack vertical slice** through that core (per the chosen
scope): Auth → Workspace/Project → 4D phases + gates → Develop Kanban board → the 5R
loop → the enforcement engine → real-time updates → tests, end to end. The surfaces
originally deferred to a follow-up — the marketing cover, design-system page, workspace
dashboard aggregation, the dedicated Discern decision screen, and the Demonstrate/Impact
(stories, gratitude wall, metrics) surfaces — were subsequently built in the 2026-07-15
audit remediation (see "Audit remediation" below).

**Decisions locked with the user:** MediatR/CQS + controllers; hand-built following
the Forge reference patterns (`C:\projects\forge`); JWT auth included; vertical slice
first. Target **.NET 9** (pinned via `global.json`) to match the proven Forge/mvp
patterns and the installed `dotnet-ef` tooling. SQL Server via **LocalDB**
(`(localdb)\MSSQLLocalDB`, confirmed installed).

---

## Backend — `backend/` (Clean Architecture, .NET 9)

Mirror the Forge layout exactly: `backend/Liturgy.sln`, `global.json` pinning the 9.x
SDK, `Directory.Build.props` (Nullable enable, ImplicitUsings, LangVersion), four
projects under `src/` and two test projects under `tests/`. **One type per file**
throughout (matches Forge — request DTOs, exceptions, handlers each get their own
file).

### `Liturgy.Domain` — entities (POCOs, one per file)
Plain classes with `Id: Guid` + `CreatedAt: DateTimeOffset`, following
`Forge.Domain/WorkoutSession.cs` style. Enforcement state lives in enums.

- `User`, `RefreshToken`, `SignInAttempt` — **ported verbatim from Forge** (auth).
- `Workspace` (Name, Slug) — the root tenant, surfaced in-product as the "Account".
- `Membership` (WorkspaceId, UserId, Role, Initials) — links a User to a Workspace.
- `Invitation` (WorkspaceId, Email, Role, Token, Status: `InvitationStatus`, InvitedByUserId) —
  an in-app invite to join an account; no email delivery, the token/URL is shared out of band.
- `Project` (WorkspaceId, Name, Tag, CurrentPhase: `PhaseKind`, Status: `ProjectStatus`
  default `Active`).
- `Phase` (ProjectId, Kind: `PhaseKind`, State: `PhaseState`, Order).
- `Gate` (PhaseId, Title, State: `GateState`).
- `Requirement` (GateId, Label, Meta, State: `RequirementState`, Order).
- `Sprint` (ProjectId, Number, EndsAt).
- `Card` (ProjectId, SprintId, Code e.g. "LAN-24", Title, Description?, Points?, AssigneeId?,
  Column: `BoardColumn`, CurrentR: `RKind?`, IsBlocked, Status: `CardStatus` default `Open`).
- `RMovement` (CardId, Kind: `RKind`, Order 1-5, State: `MovementState`, plus
  content: `Ask`, `Received`, `Synthesis`, `ArtifactUrl`, `WhatChanged`,
  `Thanksgiving`).
- Enums (one file each): `PhaseKind {Discover,Discern,Develop,Demonstrate}`,
  `PhaseState {Locked,Current,Done}`, `GateState {Blocked,Open}`,
  `RequirementState {Todo,Done}`, `BoardColumn {Backlog,InLoop,Review,Done}`,
  `RKind {Request,Receive,Review,Render,Rejoice}`, `MovementState {Locked,Current,Done}`,
  `CardStatus {Open,Closed,Cancelled}`, `ProjectStatus {Active,Closed}`,
  `InvitationStatus {Pending,Accepted,Revoked}`.

### `Liturgy.Application` — MediatR/CQS
Copy the structure of `Forge.Application`:
- `DependencyInjection.AddApplication()` — `AddMediatR`, `AddValidatorsFromAssembly`,
  register `ValidationBehavior<,>` pipeline (copy `Behaviors/ValidationBehavior.cs`).
- `Abstractions/`: `IAppDbContext` (exposes the DbSets), `IClock`, `ICurrentUser`,
  `IJwtTokenIssuer`, `IPasswordHasher`, `IRefreshTokenStore` — ported from Forge.
  **New:** `IRealtimeNotifier` — the seam the Infrastructure/SignalR layer implements
  so handlers stay transport-agnostic (methods like `CardMovedAsync`,
  `MovementLoggedAsync`, `GateChangedAsync`).
- `Auth/` — port `RegisterCommand(+Handler+Validator)`, `SignInCommand(...)`,
  `AuthResult`, `GetCurrentUserQuery`, and the auth exceptions from Forge.
- Feature folders, each command = `Command.cs` + `CommandHandler.cs` +
  `CommandValidator.cs` + result/DTO files (one per file, per the chosen preview):
  - `Projects/` — `GetProjectQuery` (returns full 4D journey DTO: phases, gates,
    requirements), `ListProjectsQuery`.
  - `Gates/` — `ToggleRequirementCommand` → recompute gate + phase state via the
    engine, notify.
  - `Board/` — `GetBoardQuery`, `MoveCardCommand`, `CreateCardCommand`,
    `AssignCardCommand`.
  - `Loop/` — `GetCardLoopQuery`, `LogMovementCommand` (advances the current R,
    fills content, recomputes card state), `MarkCardDoneCommand`.
- **`Enforcement/EnforcementEngine.cs`** — the heart. Pure, unit-testable domain
  service (no EF): given a Card + its RMovements returns whether Done is permitted
  and the next `CurrentR`; given a Gate + Requirements returns `GateState` and
  whether the downstream Phase unlocks. Handlers call it, then persist, then notify.
  Guard rules throw `GateLockedException` / `MovementsIncompleteException`
  (→ 409/400 in middleware).
- DTOs return **enum names as strings** (JSON string enum converter, as Forge does)
  so the Angular contract is string-typed.

### `Liturgy.Infrastructure`
- `AppDbContext : DbContext, IAppDbContext` with `OnModelCreating` config per entity
  (follow `Forge.Infrastructure/AppDbContext.cs`: max lengths, `HasConversion<int>()`
  wait — use string enum conversions or int; keep int in DB, string on wire).
- `DependencyInjection.AddInfrastructure(config)` — `AddDbContext<AppDbContext>`
  (SqlServer), register `IAppDbContext`, JWT/`IJwtTokenIssuer`, `BCryptPasswordHasher`,
  `SystemClock`, `RefreshTokenStore`, `HttpContextCurrentUser`, `DevDataSeeder`, and
  **`SignalRRealtimeNotifier : IRealtimeNotifier`** (wraps `IHubContext<BoardHub>`).
- `JwtOptions` / `JwtTokenIssuer` / `BCryptPasswordHasher` / `SystemClock` /
  `HttpContextCurrentUser` / `RefreshTokenStore` — ported from Forge.
- `DevDataSeeder` — seeds the mock's data (workspace "New Hope Collective", members
  QB/AM/JP/SD, project "Lantern" with its 4 phases, the Discern→Develop gate + its
  requirements, Sprint 6, and cards LAN-24/31/33/28/19/12/08 with their 5R movements)
  so the running app matches `docs/mocks` on first load.
- EF migrations generated with `dotnet-ef` into `Infrastructure/Migrations/`.

### `Liturgy.Api`
- `Program.cs` — copy Forge's: `AddApplication` + `AddInfrastructure`, JWT bearer,
  CORS `web` policy for `http://localhost:4200` **with `AllowCredentials`** (required
  for SignalR), controllers with `JsonStringEnumConverter`, migrate-on-startup +
  dev seed, exception middleware, `MapControllers()`. **Add** `AddSignalR()` and
  `app.MapHub<BoardHub>("/hubs/board")` (inside the CORS/auth pipeline; hub carries
  `[Authorize]`). Keep `public partial class Program {}` for the test factory.
- `Hubs/BoardHub.cs` — `[Authorize]` hub; clients call `JoinProject(projectId)` /
  `LeaveProject` to join a SignalR group per project. Server pushes
  `CardMoved`, `CardCreated`, `CardAssigned`, `MovementLogged`, `RequirementToggled`,
  `GateChanged`, `PhaseUnlocked` to the group. `IRealtimeNotifier` impl targets
  `Clients.Group($"project:{id}")`.
- `Controllers/` — thin, `[ApiController]`/`[Authorize]`, inject `IMediator`
  (pattern = `Forge SessionsController`). One controller per feature:
  `AuthController` (register/sign-in), `MeController`, `ProjectsController`,
  `GatesController`, `BoardController`, `LoopController`. Request DTOs one-per-file.
- `Middleware/ExceptionHandlingMiddleware.cs` — port Forge's, add cases for the new
  enforcement exceptions (`GateLockedException`→409, `MovementsIncompleteException`
  →400, `CardNotFoundException`→404).
- `appsettings.json` / `appsettings.Development.json` — `ConnectionStrings.DefaultConnection`
  = `Server=(localdb)\MSSQLLocalDB;Database=Liturgy;Trusted_Connection=True;TrustServerCertificate=True`,
  `Jwt` section (placeholder signing key), `Cors:AllowedOrigins`.

### Tests — `tests/`
- **`Liturgy.UnitTests`** (xUnit) — pure unit tests for `EnforcementEngine` (all the
  gate/loop transition rules: card can't go Done with <5 movements, gate opens only
  when every requirement done, phase auto-unlocks, next-R computation) and for
  individual handlers using in-memory fakes (`IAppDbContext` over EF InMemory or hand
  fakes, fake `IClock`/`ICurrentUser`/`IRealtimeNotifier`). This is where the
  differentiating logic is proven cheaply.
- **`Liturgy.IntegrationTests`** (xUnit + `Microsoft.AspNetCore.Mvc.Testing`) —
  **copy the `Forge.Acceptance` pattern verbatim**: `Support/AcceptanceSqlServer.cs`
  (default `(localdb)\MSSQLLocalDB`, env override), each test class `IAsyncLifetime`
  spinning up `WebApplicationFactory<Program>` against a **per-test real SQL Server
  DB** (`Liturgy_IntegrationTests_{Guid:N}`), `MigrateAsync` in `InitializeAsync`,
  drop DB in `DisposeAsync`, `ConfigureTestServices` swaps the `DbContextOptions`.
  Cover the **major behaviours over real HTTP**: register→sign-in→GET /me;
  toggling the last gate requirement flips the gate to Open and unlocks Develop;
  moving a card to Done is **rejected (409)** until all 5 movements are logged, then
  **accepted**; logging movements advances `CurrentR`. csproj packages match Forge's
  (`Mvc.Testing`, `xunit`, `Microsoft.NET.Test.Sdk`, `Microsoft.Data.SqlClient`).

> **MediatR licensing note:** Forge references **MediatR 12.2.0** (MIT-licensed).
> Match that (or pin ≤ 12.4.1, the last MIT release) to avoid the commercial license
> on 13+.

---

## Frontend — `frontend/` (Angular 21, into the existing workspace)

The workspace exists but is empty. Generate the application + segmented libraries
(mirrors the Forge/mvp shape the user prefers and satisfies "one type per file"):

```
frontend/
  projects/
    liturgy-app/        # the application (standalone, zoneless — Angular 21 default)
    api/                # @liturgy/api   — HTTP services, models, realtime client
    components/         # @liturgy/components — pure presentational BEM components
    domain/             # @liturgy/domain — feature components (depend on api)
    liturgy-app-e2e/    # Playwright POM suite (or top-level e2e/)
```
Add TS path aliases `@liturgy/api|components|domain` in `tsconfig.json`.

### Tooling to add (none present today)
- **Jest** (unit) — `jest`, `jest-preset-angular`, `@types/jest`; `jest.config.ts`
  per project; wire `ng test` to Jest via a builder (`@angular-builders/jest` or a
  direct `test` npm script). Remove the stale Vitest/Karma expectation from the
  README and the `.vscode/launch.json` Karma (9876) config.
- **`@angular/cdk`** — `DragDropModule` for the Kanban board columns, plus `a11y`
  (FocusTrap/LiveAnnouncer) and `OverlayModule` where needed.
- **`@microsoft/signalr`** — realtime client.
- **Playwright** (`@playwright/test`) — E2E with **backend faked** (see below).

### Application structure (BEM, separate template/style files)
"No single-file components" → **every component has separate `.ts`, `.html`, and
`.scss` files** (no inline `template`/`styles`); **one type per file** (models,
services, guards, interceptors each in their own file). Port `docs/mocks/assets/liturgy.css`
into `projects/liturgy-app/src/styles.scss` as the design-token + global BEM layer
(`:root` custom properties, `.btn`, `.badge`, `.card`, `.rail`, `.gate`, `.dial`,
`.board`, `.col`, `.wcard`, `.movement`, etc.) and reuse the **exact BEM class names**
already established in the mocks — components render markup matching the mock HTML.

- **api lib** — `models/` (one interface per file: `Project`, `Phase`, `Gate`,
  `Requirement`, `Board`, `Card`, `Movement`, DTOs, enums mirrored as string-literal
  unions), `services/` (`AuthService`, `ProjectsService`, `BoardService`,
  `LoopService`, `GatesService` — one per file), `realtime/` (`BoardRealtimeService`
  wrapping a `HubConnection`, exposing observables per event; behind an
  injection token `REALTIME` so it can be faked in tests), `auth/`
  (`AuthStateService` with signals + localStorage, `authGuard`, `authInterceptor` —
  Forge/mvp pattern).
- **components lib** — presentational, `@Input`/`@Output` only, BEM: `RhythmRailComponent`
  (the signature 4D spine), `DialComponent` (the 5R canonical-hours dial — port the
  `buildDial` SVG logic from `liturgy.js` into a real component), `PipStripComponent`,
  `GateComponent`, `ChecklistComponent`, `BadgeComponent`, `WorkCardComponent`,
  `MovementComponent`, `StatComponent`.
- **domain lib / app pages** — routed feature components wired to services + realtime:
  - `ShellComponent` (rail + topbar layout).
  - `SignInComponent` / `SignUpComponent` (auth screens).
  - `ProjectsComponent` (list / landing).
  - `ProjectJourneyComponent` → `project-4d.html`: the 4-phase spine with gates +
    checklists; toggling a requirement calls `GatesService` and reflects the
    server-recomputed gate/phase state (live via realtime).
  - `DevelopBoardComponent` → `develop-board.html`: CDK drag-drop columns
    (Backlog / In the 5R loop / Review / **locked Done**); dropping into Done is
    blocked client-side *and* server-side until the 5R loop is complete; card moves
    broadcast to collaborators.
  - `LoopComponent` → `5r-loop.html`: the five stacked movements, edit the current R
    (artifact link, notes, thanksgiving), "Log & continue" calls `LogMovementCommand`,
    dial/pips advance from server truth; "Mark Done" enabled only when all 5 logged.

### Unit tests (Jest)
Per component/service: `EnforcementEngine` mirror on the client is thin, so focus
Jest on `DialComponent` (segment math), `BoardService`/`LoopService` (HTTP contract
via `HttpTestingController`), guards/interceptor, and the board drag-drop guard
(cannot drop into Done when loop incomplete).

### E2E (Playwright, Page Object Model, backend faked)
- `e2e/pages/` — one POM per screen (`base.page.ts`, `sign-in.page.ts`,
  `project-journey.page.ts`, `develop-board.page.ts`, `loop.page.ts`) following the
  Forge/mvp POM style.
- **Faked backend:** intercept all `**/api/**` calls with `page.route(...)` and serve
  JSON fixtures (auth, project journey, board, loop) — no real server. Model the
  enforcement rules in the fixtures so the specs prove the *UI* behaviour: gate stays
  blocked until the last requirement is checked; Done column rejects a card with an
  incomplete loop and accepts it once movements are logged. Realtime is behind the
  `REALTIME` token → provide a **fake realtime** in the E2E build (no live socket;
  the negotiate route is stubbed so the app degrades to REST cleanly and the POMs
  assert REST-driven state). `tests/` specs drive each POM.

---

## Build order (suggested execution sequence)

1. **Backend skeleton**: sln, `global.json`, `Directory.Build.props`, 4 src csprojs +
   references, port Auth (Domain/Application/Infrastructure/Api) from Forge, get
   register/sign-in/`/me` green. First integration test (auth round-trip) passing.
2. **Liturgy domain + enforcement engine** (Domain entities, `EnforcementEngine`,
   `IAppDbContext`, `AppDbContext` + first migration, `DevDataSeeder`). Unit-test the
   engine.
3. **Projects/Gates/Board/Loop** commands, queries, controllers; integration tests
   for the gate-unlock and 5R-Done enforcement over HTTP.
4. **SignalR** hub + `IRealtimeNotifier` impl; wire notifications into the Board/Loop/
   Gates handlers.
5. **Frontend tooling** (generate app + 3 libs, add Jest/CDK/SignalR/Playwright,
   port `liturgy.css` → `styles.scss`).
6. **api lib** (models/services/realtime/auth) + auth screens + guard/interceptor.
7. **components lib** (rail, dial, gate, checklist, pipstrip, work-card, movement).
8. **Pages**: Projects → Project 4D journey → Develop board (CDK drag-drop) → 5R loop,
   each wired to services + realtime. Jest specs alongside.
9. **Playwright POM E2E** with faked backend.

---

## Verification (end-to-end, run the code)

**Backend**
- `dotnet build backend/Liturgy.sln` — clean.
- `dotnet test backend/tests/Liturgy.UnitTests` — engine + handler rules green.
- `dotnet test backend/tests/Liturgy.IntegrationTests` — requires LocalDB
  (`sqllocaldb info` → `MSSQLLocalDB`, confirmed); spins real per-test DBs. Confirms:
  auth round-trip; gate opens + Develop unlocks on last requirement; **card→Done is
  409 until all 5 movements logged, then 2xx**.
- `dotnet run --project backend/src/Liturgy.Api` → open Swagger; exercise
  `/api/auth/register`, `/api/projects/{id}`, `/api/board/...`, `/api/loop/...`;
  confirm dev seed matches the Lantern mock.

**Realtime smoke** — two browser tabs (or a REST call + one tab): move a card / log a
movement in one, confirm the other updates live via the `board` hub.

**Frontend**
- `npm --prefix frontend test` (Jest) — component/service specs green.
- `npm --prefix frontend start` + `dotnet run` backend → click through Sign-in →
  Projects → 4D journey (check the last Discern→Develop requirement, watch the gate
  unlock) → Develop board (drag a card; Done rejects an incomplete-loop card) → 5R
  loop (log the five movements, watch the dial fill, then Mark Done succeeds). Visually
  compare against `docs/mocks/*.html`.
- `npx --prefix frontend playwright test` — POM E2E with faked backend green,
  including the gate-unlock and 5R-Done UI flows.

---

## Audit remediation (2026-07-15) — the follow-up, delivered

A completeness + mock-fidelity audit closed the originally-deferred gaps:

- **New screens** (all mock-faithful, against real endpoints): marketing landing
  (`index.html`; superseded — the marketing landing ships as a static page, not an
  Angular screen, see [marketing-deployment.md](marketing-deployment.md)),
  design-system page, workspace **dashboard** (momentum stats + "gates
  that need attention" feed + 4-lane 4D board), the dedicated **Discern** decision screen
  (four paths + rationale, backed by a `Decision` entity), and **Demonstrate/Impact**
  (relationship metrics, week-ordered stories, gratitude wall — `ImpactMetric`/`Story`/
  `Gratitude` entities). New endpoints: `GET/PUT /api/projects/{id}/decision`,
  `GET /api/projects/{id}/impact`, `GET /api/dashboard`, `GET /api/members`,
  `POST /api/projects`; `CardLoopDto` gained `projectId`.
- **Workspace scoping** (`IWorkspaceAccess`): project-scoped handlers require caller
  membership (non-member → 404); `ListProjects` filters by membership; registration
  creates a personal workspace.
- **Signature Rhythm Rail** built as a real contextual component (4D spine + gate-latches
  + Develop-nested 5R dial/rlist); brand webfonts loaded; create/assign-card UI wired;
  fidelity bug fixes (Demonstrate badge slug, on-paper dial, 5R poetic titles, gate
  advance affordance, `aria-disabled` locked-action language, skip links).
- **Seed** expanded to Lantern, Wellspring, Bread & Fish, Refuge Finder, Sabbath, Common
  Table (all four 4D phases represented). Refresh dev demo data by dropping the `Liturgy`
  database (the seeder is globally idempotent).

---

## Build notes & deviations from the plan (as implemented)

- **SQL Server: SQL Express, not LocalDB.** This is a Windows-on-ARM (arm64) machine;
  LocalDB's native `SQLUserInstance.dll` is x64-only and cannot load in the arm64
  test/runtime host. Switched to the installed `.\SQLEXPRESS` instance (reachable via
  arm64 SNI over TCP/named pipes). Connection strings in `appsettings.json` and the
  integration tests' `AcceptanceSqlServer` default to `.\SQLEXPRESS`; override tests
  with `LITURGY_TEST_SQLSERVER`.
- **Lean auth.** Access-token-only JWT (no refresh tokens, throttle, audit, or password
  reset) to keep the slice focused. `AuthResult` carries the token + identity + derived
  initials. MediatR pinned to **12.2.0** (matches Forge; MIT-licensed).
- **Frontend test stack.** `jest-preset-angular@17` + `jest@30` (v15/16 don't support
  Angular 21). Jest config is `jest.config.js` (CommonJS) with the zoneless setup env.
  Lib paths resolve to source (`projects/**/src/public-api.ts`) so no lib build is
  needed for dev/test.
- **Pages live in the app**, not `@liturgy/domain`; the domain lib is a reserved
  placeholder. Presentational components (dial, pip strip, work card, gate) are in
  `@liturgy/components`.
- **SignalR hub + notifier live in `Liturgy.Infrastructure`** (which has the ASP.NET
  framework reference), avoiding a circular Api → Infrastructure dependency; `Program`
  maps the hub at `/hubs/board`.
- **E2E realtime** is aborted in the faked suite (`**/hubs/**` → abort); the app
  degrades to REST cleanly. A `LIVE=1` opt-in suite (`e2e/live`) drives the real API.

### Verified green (post-audit, 2026-07-15)
- Backend: `dotnet build` clean; `Liturgy.UnitTests` 15/15; `Liturgy.IntegrationTests`
  12/12 (real SQL Express, per-test DBs; added scoping/decision/impact/dashboard/
  create-project/members acceptance tests).
- Frontend: `ng build liturgy-app` clean; Jest 16/16; Playwright 11/11 (faked) + live
  opt-in suite. Live contract + visual pass confirmed against the running API.
- Note: `ng build` needs the project name in this multi-project workspace
  (`ng build liturgy-app`); bare `ng build` errors on project ambiguity.
