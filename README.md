# Liturgy

An agile workspace that **enforces** the FaithTech Playbook. Projects move through the
**4D cycle** (Discover → Discern → Develop → Demonstrate) behind gates that stay locked
until their requirement checklists are complete, and every work item must complete the
**5R loop** (Request → Receive → Review → Render → Rejoice) before it can reach Done.
The enforcement is server-authoritative, and collaborative surfaces update in real time.

This repository contains a full-stack vertical slice through that core.

## Structure

```
backend/    .NET 9 Clean Architecture API (Domain / Application / Infrastructure / Api)
            MediatR/CQS + controllers, EF Core (SQL Server), JWT auth, SignalR, xUnit tests
frontend/   Angular 21 workspace: liturgy-app + @liturgy/{api,components,domain},
            Jest unit tests, Playwright E2E (Page Object Model, backend faked)
docs/       implementation-plan.md and the original HTML mocks (docs/mocks)
```

## Prerequisites

- .NET SDK 9 (pinned in `backend/global.json`)
- SQL Server — local `.\SQLEXPRESS` by default (see the plan's build notes re: LocalDB
  on ARM64). Override the tests' instance with `LITURGY_TEST_SQLSERVER`.
- Node 20+ / npm

## Run it

```bash
# 1. Backend (migrates + seeds the "Lantern" demo on first run in Development)
cd backend
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Liturgy.Api
# → http://localhost:5099  (Swagger at /swagger)

# 2. Frontend
cd frontend
npm install
npm start
# → http://localhost:4200
```

Sign in with a seeded member — e.g. `quinn@newhope.dev` / `Liturgy!2026`.

## Test

```bash
# Backend
cd backend
dotnet test tests/Liturgy.UnitTests          # enforcement engine + rules
dotnet test tests/Liturgy.IntegrationTests   # WebApplicationFactory + real SQL per test

# Frontend
cd frontend
npm test                                       # Jest unit tests
npm run e2e                                     # Playwright (backend faked)
LIVE=1 npx playwright test e2e/live             # optional: against the running real API
```
