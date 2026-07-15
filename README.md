<div align="center">
  <img src="docs/assets/liturgy-logo.svg" alt="Liturgy" width="140">

# Liturgy

An agile workspace that **enforces** the FaithTech Playbook — the 4D cycle as its spine and the 5R co-creation loop inside every build.

[![License: MIT](https://img.shields.io/badge/license-MIT-c6fb50.svg)](LICENSE)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular 21](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Code style: Prettier](https://img.shields.io/badge/code_style-Prettier-F7B93E?logo=prettier&logoColor=black)](https://prettier.io/)
[![Contributions welcome](https://img.shields.io/badge/contributions-welcome-1d8fb9.svg)](CONTRIBUTING.md)

[Implementation plan](docs/implementation-plan.md) | [Architecture](docs/ARCHITECTURE.md) | [Design mocks](docs/mocks/README.md) | [Contributing](CONTRIBUTING.md)
</div>

## About the project

Liturgy is a project-management workspace for teams building redemptive technology. Where a typical agile tool treats process as advice, Liturgy makes the [FaithTech Playbook](https://www.faithtech.com/playbook) the mechanism: every project moves through the **4D cycle** — Discover → Discern → Develop → Demonstrate — behind gates that stay locked until their requirement checklists are complete, and every work item inside Develop must complete the **5R co-creation loop** — Request → Receive → Review → Render → Rejoice — before it can reach Done.

The differentiator is an **enforcement engine**: a server-authoritative state machine that owns phase gates and movement completion. Clients cannot skip a gate or mark a card Done out of order, because the rule lives on the server and every allowed transition broadcasts to collaborators in real time.

This repository is a full-stack vertical slice through that core. It uses seeded demo data for development and demonstration and has not completed an independent security, privacy, or accessibility review; it is not a production system of record. The full build plan is in [docs/implementation-plan.md](docs/implementation-plan.md).

## Features

- Email and password authentication with JWT bearer tokens
- Projects that advance through the 4D cycle behind server-enforced gates
- Requirement checklists that lock a gate until complete and auto-unlock when met
- A Develop Kanban board where each card carries its 5R movement status
- The 5R co-creation loop, with Done blocked until all five movements are logged
- Real-time board, loop, and checklist updates over SignalR for concurrent editors
- Seeded "Lantern" demo project so the workspace is populated on first run

See [docs/mocks](docs/mocks/README.md) for the FaithTech-faithful design mocks these surfaces are built from.

## Getting started

### Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/) (pinned in [backend/global.json](backend/global.json))
- [Node.js](https://nodejs.org/) 20 or later and npm
- SQL Server — local `.\SQLEXPRESS` by default (override the test instance with `LITURGY_TEST_SQLSERVER`)

### Local development

```bash
git clone https://github.com/QuinntyneBrown/Liturgy.git
cd Liturgy

# 1. Backend — migrates and seeds the "Lantern" demo on first run in Development
cd backend
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5099 \
  dotnet run --project src/Liturgy.Api
# → http://localhost:5099  (Swagger at /swagger)

# 2. Frontend — in a second terminal
cd frontend
npm install
npm start
# → http://localhost:4200
```

Open [http://localhost:4200](http://localhost:4200) and sign in with a seeded member — `quinn@newhope.dev` / `Liturgy!2026`. The frontend expects the API at `http://localhost:5099` (set in `frontend/projects/liturgy-app/src/app/api-origin.ts`).

Never commit secrets. The development `Jwt:SigningKey` in `appsettings.json` is a placeholder; provide a real key through user secrets or environment configuration before deploying anywhere.

## Technology

| Area | Technologies |
| --- | --- |
| Backend | .NET 9, ASP.NET Core, MediatR / CQS, controllers |
| Persistence | Entity Framework Core, SQL Server |
| Real time | SignalR |
| Authentication | JWT bearer tokens |
| Frontend | Angular 21, RxJS, TypeScript |
| Quality | xUnit, Jest, Playwright, Prettier, EditorConfig |

## Testing

```bash
# Backend
cd backend
dotnet test tests/Liturgy.UnitTests          # enforcement engine and domain rules
dotnet test tests/Liturgy.IntegrationTests   # WebApplicationFactory + real SQL per test

# Frontend
cd frontend
npm test                                      # Jest unit tests
npm run e2e                                   # Playwright journeys (backend faked)
```

The Playwright suite uses a faked backend by default, so it runs without the API or a database. The integration tests provision a real SQL Server database per test; point them at your instance with `LITURGY_TEST_SQLSERVER` if the default `.\SQLEXPRESS` is not available.

## Project structure

```text
backend/
  src/Liturgy.Domain/               Entities, value objects, the 4D/5R domain model
  src/Liturgy.Application/          MediatR handlers, DTOs, and the enforcement engine
  src/Liturgy.Infrastructure/       EF Core, SQL Server, JWT, and the SignalR notifier
  src/Liturgy.Api/                  Controllers, middleware, and the composition root
  tests/Liturgy.UnitTests/          Enforcement rules and domain tests
  tests/Liturgy.IntegrationTests/   Full API tests over a real database
frontend/
  projects/liturgy-app/             Angular application shell and routes
  projects/liturgy/api/             @liturgy/api — HTTP and SignalR services
  projects/liturgy/components/      @liturgy/components — UI building blocks
  projects/liturgy/domain/          @liturgy/domain — shared types
  e2e/                              Playwright journeys with a faked backend
docs/
  implementation-plan.md            Build plan for the vertical slice
  mocks/                            Static FaithTech design mocks
```

## Documentation

| Document | Purpose |
| --- | --- |
| [Implementation plan](docs/implementation-plan.md) | Vertical-slice scope and the backend and frontend build plan |
| [Architecture](docs/ARCHITECTURE.md) | System layers, the enforcement engine, and data flow |
| [Architecture decisions](docs/DECISIONS.md) | Technical and product decisions worth recording |
| [Design mocks](docs/mocks/README.md) | FaithTech-faithful HTML mocks for the 4D + 5R workspace |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) for the test-first workflow, branch conventions, quality gates, and architecture boundaries. Participation is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

The people who help build the project are recognized in [CONTRIBUTORS.md](CONTRIBUTORS.md), and notable changes are recorded in [CHANGELOG.md](CHANGELOG.md).

## Security

Do not report vulnerabilities in a public issue. Follow [SECURITY.md](SECURITY.md) to submit a private report. For usage questions and non-sensitive problems, see [SUPPORT.md](SUPPORT.md).

## Governance

Maintainer responsibilities, decision making, and the path to becoming a maintainer are described in [GOVERNANCE.md](GOVERNANCE.md).

## License

Copyright (c) 2026 Liturgy contributors. Released under the [MIT License](LICENSE).
