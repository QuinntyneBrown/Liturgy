# Liturgy — Frontend

Angular 21 workspace for Liturgy. It is split into an application plus three libraries:

- `projects/liturgy-app` — the application (standalone, zoneless).
- `@liturgy/api` — HTTP services, models, auth (state/guard/interceptor), and the
  realtime (SignalR) client behind the `BoardRealtime` token.
- `@liturgy/components` — presentational BEM components (dial, pip strip, work card, gate).
- `@liturgy/domain` — reserved for shared feature-domain building blocks.

The design system in `src/styles.scss` is ported from `docs/mocks/assets/liturgy.css`.

## Development server

```bash
npm start          # ng serve on http://localhost:4200
```

The app calls the API at `http://localhost:5099` (see `src/app/api-origin.ts`). Run the
backend from `../backend` (`dotnet run --project src/Liturgy.Api`).

## Unit tests (Jest)

```bash
npm test           # jest
npm run test:watch
```

Unit tests use **Jest** (`jest-preset-angular`, zoneless test env). Specs live beside
their sources as `*.spec.ts`.

## End-to-end tests (Playwright, Page Object Model)

```bash
npm run e2e        # playwright test — backend faked via network interception
```

The default E2E suite fakes the backend (`e2e/fixtures/fake-backend.ts`), so no API or
database is needed. A live suite (`e2e/live`) exercises the real backend and is opt-in:

```bash
# with the API running on :5099
LIVE=1 npx playwright test e2e/live
```

## Building

```bash
npm run build      # ng build → dist/liturgy-app
```
