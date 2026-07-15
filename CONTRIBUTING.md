# Contributing to Liturgy

Thank you for helping improve Liturgy. Contributions may include code, tests, design, documentation, issue triage, accessibility feedback, and product insight.

By participating, you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md). For support or security concerns, use the channels in [SUPPORT.md](SUPPORT.md) and [SECURITY.md](SECURITY.md).

## Before contributing

- Search existing issues and pull requests before starting work.
- Open an issue for large features, database schema changes, or changes to the enforcement engine — the rules that gate 4D phases and 5R completion are the heart of the product.
- Never include real user information, credentials, signing keys, or connection strings in issues, fixtures, screenshots, commits, or logs. Use seeded or synthetic data.

## Development setup

```bash
git clone https://github.com/QuinntyneBrown/Liturgy.git
cd Liturgy

# Backend
cd backend
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5099 \
  dotnet run --project src/Liturgy.Api

# Frontend (second terminal)
cd frontend
npm install
npm start
```

See the [README](README.md) for prerequisites, the seeded sign-in, and how the frontend locates the API.

## Development workflow

1. Create a branch from the latest `main` using `feat/`, `fix/`, `docs/`, or `chore/` followed by a short description.
2. Keep the change focused and avoid unrelated formatting or refactoring.
3. For behavior changes, work test-first: add or update a test, run it and confirm it fails for the expected reason, implement the smallest passing change, then refactor while green.
4. Enforcement rules belong in the domain and application layers and must be covered by unit tests. A client-only "fix" that lets the UI skip a gate is a defect, not a feature.
5. Documentation-only, repository metadata, configuration, and template changes do not require a test unless they alter runtime behavior.
6. Update architecture, decision, or changelog documentation when public behavior or the contributor workflow changes.
7. Open a pull request using the repository template and respond to review feedback.

Do not commit directly to `main`. Maintainers squash-merge approved pull requests after required checks pass.

## Quality gates

For changes to application behavior, run the relevant checks before opening a pull request:

```bash
# Backend
cd backend
dotnet build
dotnet test tests/Liturgy.UnitTests
dotnet test tests/Liturgy.IntegrationTests   # requires SQL Server

# Frontend
cd frontend
npm test          # Jest
npm run build
npm run e2e       # Playwright (backend faked)
```

For non-behavior changes, run only the checks relevant to the files changed.

## Architecture boundaries

Read [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) before changing module boundaries or data flow. Key conventions:

- The dependency direction is `Api → Application → Domain`, with `Infrastructure` implementing application abstractions. Nothing in `Domain` or `Application` references EF Core, ASP.NET, or SignalR directly.
- One public type per file, matching the existing layout (request DTOs, exceptions, and handlers each get their own file).
- Commands and queries flow through MediatR handlers; controllers stay thin.
- The enforcement engine is server-authoritative. State transitions are validated on the server and broadcast through the `IRealtimeNotifier` abstraction, never trusted from the client.
- Database changes require a forward EF Core migration and, where relevant, an update to the architecture documentation.

## Frontend contributions

- Shared services (HTTP and SignalR) live in `@liturgy/api`; shared UI in `@liturgy/components`; shared types in `@liturgy/domain`. Application shell and routes live in `liturgy-app`.
- The API base URL is injected through the `API_BASE_URL` token; do not hard-code hostnames inside library code.
- Test loading, error, and blocked (gate-locked) states — the locked affordance is a core part of the experience.

## Commits and pull requests

Write concise imperative commit subjects, for example `enforcement: block Done until five movements logged` or `docs: clarify seeded sign-in`. A pull request should:

- Explain the user or contributor problem and the resulting behavior
- Link related issues and architecture decisions
- Include test evidence appropriate to the change
- Call out security, migration, and rollback implications
- Keep generated files and unrelated changes out of the diff

At least one maintainer approval is required.

## Recognition

Merged human contributions are recognized in [CONTRIBUTORS.md](CONTRIBUTORS.md) and the repository's contributors graph. Notable user-facing changes should also be added under `Unreleased` in [CHANGELOG.md](CHANGELOG.md).
