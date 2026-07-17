# Marketing pages: production deployment model

**Status:** Accepted — Phase 1 implemented (2026-07-16)
**Date:** 2026-07-16
**Decision record:** [DECISIONS.md #11](DECISIONS.md)

**The answer up front:** No — the marketing pages should not be Angular SPA routes.
The production-quality marketing site already exists as static HTML (the redesigned
splash and the downloadable brochure PDF under `docs/mocks/`). It should be promoted
to a first-class static site and served **beside** the SPA from the same App Service
(Phase 1, zero new infrastructure), with a documented path to a split-origin
`www.` / `app.` model when the product needs it (Phase 2). The in-app Angular
`LandingComponent` retires.

---

## 1. The question and scope

Where do the marketing pages live in production, and should they be routes inside
the Angular app?

**In scope:** the deployment and serving model, the source-of-truth location for
marketing content, the link contract between marketing CTAs and app routes, and the
operational concerns (caching, SEO, the PDF workflow).

**Out of scope:** the visual design (already done — `docs/mocks/index.html` is the
finished splash). Phase 1 of the model described here was implemented alongside
this document (see §6 and §9 for what changed).

## 2. State before this change

Production today is a **single-artifact, same-origin** deployment:

```
frontend (ng build, production)
      │  frontend/dist/liturgy-app/browser/
      ▼
CI copy step ──► backend/src/Liturgy.Api/wwwroot/   (gitignored; exists only in CI)
      │
      ▼
dotnet publish ──► one bundle ──► Azure App Service (Linux, DOTNETCORE:9.0)
                                   deployed via OIDC; gated on AZURE_WEBAPP_NAME
```

The API serves everything from one origin (`backend/src/Liturgy.Api/Program.cs`):

| Request | Served by |
| --- | --- |
| `/api/*`, `/health`, Swagger | ASP.NET Core controllers / endpoints |
| `/hubs/board` | SignalR hub |
| Any existing static file | `UseDefaultFiles()` + `UseStaticFiles()` over `wwwroot` |
| Everything else | `MapFallbackToFile("index.html")` — the SPA shell, so deep links like `/board/123` survive a refresh |

The SPA's route table (`frontend/projects/liturgy-app/src/app/app.routes.ts`)
serves the **marketing landing at route `''`** (public, alongside `design-system`,
`sign-in`, `sign-up`), with the authenticated shell (dashboard, projects, members,
board, loop, discern, demonstrate) behind `authGuard`, and `**` redirecting to `''`.

The API origin is resolved at runtime
(`frontend/projects/liturgy-app/src/app/api-origin.ts`): when the bundle is served
by the API it returns `''`, so every request is relative (`/api/...`,
`/hubs/board`). There are no Angular environment files, no CORS in production, and
auth is a JWT in the `Authorization` header — no cookies (DECISIONS #4, #6).

**The gap this document resolved:** there were two landing pages. The *deployed* one
was the Angular `LandingComponent`
(`frontend/projects/liturgy-app/src/app/pages/landing/`), which still carries the
old design. The *good* one — the redesigned professional splash — is static HTML at
`docs/mocks/index.html` (with `docs/mocks/assets/liturgy.css`, `liturgy.js`, and the
committed brochure PDF `docs/mocks/assets/liturgy-overview.pdf`), and is deployed
nowhere: CI never touches `docs/`.

## 3. What we need from a marketing surface

The criteria the options are judged against:

- **Crawlability.** Search engines and social unfurlers should get real HTML, not
  an empty SPA shell that renders client-side.
- **First-paint payload.** An anonymous visitor bouncing off the splash should not
  download the application bundle (the production budget allows up to 1 MB initial,
  warning at 700 kB).
- **Change cadence.** Copy edits should not require an Angular build, unit tests,
  and an app release to ship.
- **Design-system source of truth.** The FaithTech tokens exist twice today:
  `docs/mocks/assets/liturgy.css` (the origin) and the app's `styles.scss` (the
  port). Any model should be honest about which copy the marketing site uses.
- **Build & infra complexity.** Fewer moving parts wins at this stage.
- **Auth / CORS impact.** The same-origin, relative-`/api`, no-CORS contract is a
  simplifying asset; breaking it needs a reason.

## 4. Options

### A. Status quo — marketing pages as Angular routes

One framework, one deploy, one styling system. But crawlers see client-side-rendered
markup, bounce traffic pays for the full app bundle, marketing edits ride the app
release train — and the finished static splash would have to be *ported into*
Angular components, duplicating completed work into a heavier delivery vehicle.

### B. Angular prerender / SSG of the public routes

Keeps one framework and gives crawlers real HTML. But it adopts SSR/prerender build
infrastructure for what is currently a single page, still couples marketing cadence
to app releases, and still requires the port from static HTML into Angular. A
legitimate future option if marketing pages ever need live app data.

### C. Static marketing at `/`, SPA behind the fallback — same origin *(recommended now)*

The static site is promoted to a first-class folder and copied into `wwwroot`
beside the SPA. Zero new infrastructure, no CORS change, and the finished artifact
ships as-is. One collision must be solved: both the marketing page and the SPA
entry want to be `wwwroot/index.html` (§6).

### D. Split origin — `www.` static host/CDN + `app.` App Service *(recommended later)*

The professional end-state for a real marketing presence: independent deploys, CDN
caching, marketing tooling freedom. Costs a second deployment target, DNS/TLS
management, and absolute app URLs in CTAs. Nothing about the current architecture
blocks it — which is exactly why it can wait for a trigger (§5).

| Criterion | A. SPA routes | B. Prerender | C. Static, same origin | D. Static, split origin |
| --- | --- | --- | --- | --- |
| Crawlable HTML | ✗ (CSR) | ✓ | ✓ | ✓ |
| First-paint payload | app bundle | small | small | small (+CDN) |
| Copy-edit cadence | app release | app release | app deploy, no app code | independent |
| Design-system source | `styles.scss` | `styles.scss` | `liturgy.css` | `liturgy.css` |
| Port of finished splash needed | yes | yes | **no** | **no** |
| New build/infra | none | SSR build | none | static host, DNS, TLS |
| Auth/CORS impact | none | none | none | CORS only if marketing calls the API (it doesn't) |

**A variant rejected explicitly:** moving the SPA to `/app/*` (base-href change) so
marketing can own `/`. It reaches the same end state as C but breaks every
bookmarked deep link (`/dashboard`, `/board/:id`), every E2E page-object path, and
the `<base href="/">` assumption — strictly more invasive than renaming one output
file.

## 5. Decision

- **Phase 1 (next implementation task):** Option C. The static marketing site is
  served at `/` from the same App Service; the SPA keeps every app route via the
  fallback; `LandingComponent` retires.
- **Phase 2 (documented, not scheduled):** Option D, when a real trigger appears —
  a marketing owner who isn't an engineer, a custom domain / CDN requirement, or
  the marketing site growing beyond a splash + brochure.

Marketing pages are **not** Angular routes in either phase.

## 6. Target model — Phase 1 (same origin)

Implemented 2026-07-16; this section describes the shipped state.

### Source promotion

A new top-level `marketing/` folder becomes the canonical, deployable marketing
site:

```
marketing/
  index.html                    ← promoted from docs/mocks/index.html
  assets/
    liturgy.css                 ← promoted copy (live)
    liturgy.js
    liturgy-overview.pdf        ← the committed brochure PDF
```

`docs/mocks/` stays as the internal design reference — `gallery.html`, the screen
mocks, the design-system page, and the brochure *source*
(`docs/mocks/brochure/liturgy-overview.html` + `brochure.css`) remain there and are
never deployed.

Two deliberate consequences: `liturgy.css` will exist in both places (`docs/mocks`
as the frozen design reference, `marketing/` as the live copy), and the PDF
generator (`frontend/tools/generate-brochure.mjs`, run via `npm run brochure:pdf`)
must write its output to `marketing/assets/` instead of `docs/mocks/assets/`.

### Resolving the index collision

Both the marketing page and the Angular entry file want to be `wwwroot/index.html`.
Resolution: rename the SPA's emitted entry file.

- `frontend/angular.json` sets the `@angular/build:application` builder's `index`
  option (previously absent — the builder defaulted to `src/index.html`, emitted
  as `index.html`):

  ```json
  "index": { "input": "projects/liturgy-app/src/index.html", "output": "app.html" }
  ```

- `Program.cs`: `MapFallbackToFile("app.html")` (was `"index.html"`).
- `UseDefaultFiles()` then serves `marketing/index.html` at `/`.

**Validated:** `ng build` emits `app.html`, and `ng serve` (the Vite dev server)
still handles deep-link fallback correctly with the renamed index output (`/`,
`/sign-in`, and `/board/123` all serve the SPA shell in dev).

**One discovery from implementation:** the minimal-hosting `WebApplication`
injects `UseRouting` at the *start* of the pipeline, where the SPA fallback
endpoint (`{*path:nonfile}`) matches `/` — and `UseDefaultFiles` skips requests
that already have an endpoint, so `/` served `app.html` instead of the marketing
page. The fix is an explicit `app.UseRouting()` **after** the static-file
middleware in `Program.cs` (this had gone unnoticed before only because the
fallback file used to *be* `index.html`).

There is no other collision: the Angular build emits `favicon.ico`, content-hashed
bundles, and `media/`; the marketing site owns `assets/`.

### CI change

One additional copy step after "Copy SPA into API wwwroot" in
`.github/workflows/ci-cd.yml`:

```yaml
- name: Copy marketing site into API wwwroot
  run: cp -r marketing/. backend/src/Liturgy.Api/wwwroot/
```

### Link contract

| Marketing element | Today (mock-relative) | Phase 1 target |
| --- | --- | --- |
| "Sign in" (nav, footer) | `dashboard.html` | `/sign-in` |
| "Get started" (nav, hero, final CTA) | `dashboard.html` | `/sign-up` |
| "Download the product overview (PDF)" | `assets/liturgy-overview.pdf` | `/assets/liturgy-overview.pdf` (unchanged) |
| In-page anchors (`#rhythm`, `#board`, `#capabilities`) | — | unchanged |
| SPA sign-out | — | `/sign-in` (unchanged — stays in the SPA) |

### Angular cleanup

- Remove the `''` route and delete
  `frontend/projects/liturgy-app/src/app/pages/landing/` entirely.
- Retarget the wildcard: `** → /dashboard` (unauthenticated visitors bounce to
  `/sign-in` via `authGuard`).
- The `design-system` route stays — it is an internal tool, not a marketing page.
- **Test impact:** no fake-backend E2E test visits `/`. The only test touching the
  landing is the live screenshot spec
  (`frontend/e2e/live/screenshots.spec.ts`), whose `/` capture will show the static
  marketing page — but only when run against the published `wwwroot` (full-stack),
  not against `ng serve`, which never serves the marketing site.

### Local development

`ng serve` continues to serve only the app (now entered via `app.html`). To view
the marketing site locally, open `marketing/index.html` directly (or any static
file server). Full-stack parity check: build the SPA, copy SPA + marketing into
`wwwroot`, and `dotnet run` — the same layout production uses.

Request routing in the end state:

```
request ──► matches a file in wwwroot?          ──► serve it (marketing page, PDF, JS/CSS bundle)
        ──► starts with /api, /hubs, /health?   ──► backend endpoint
        ──► anything else                       ──► app.html (SPA router takes over)
```

## 7. Phase 2 — split origin (documented, not scheduled)

When a trigger from §5 appears: `www.<domain>` on a static host (Azure Static Web
Apps, or Storage + Front Door for CDN) and `app.<domain>` on the existing App
Service, unchanged.

Implications checklist:

- **CORS:** only needed if the marketing site ever calls the API — it currently
  makes no API calls, so likely none.
- **CTAs** become absolute: `https://app.<domain>/sign-up`, `/sign-in`.
- **Auth is unaffected:** JWT lives in the `Authorization` header, not cookies, so
  the app works identically on its own subdomain (DECISIONS #4).
- **SignalR** stays app-origin-only; no negotiate CORS unless marketing ever embeds
  live data (not planned).
- **Per-host `robots.txt` / `sitemap.xml`;** `app.` root (`/`) should redirect to
  `www.`.
- **DNS, TLS certificates, and a second deploy workflow** become real operational
  surface — the actual cost of this phase.

## 8. Operational notes

- **Caching (implemented).** Angular bundles are content-hashed
  (`outputHashing: all`) and served with
  `Cache-Control: public, max-age=31536000, immutable`; the unhashed files —
  `app.html`, `marketing/index.html`, `assets/liturgy.css`, `assets/liturgy.js`,
  the PDF — get `no-cache` (revalidate on every request). Implemented via
  `StaticFileOptions.OnPrepareResponse` in `Program.cs`, matching the
  `-XXXXXXXX.ext` hash suffix.
- **PDF workflow.** `npm run brochure:pdf` (Playwright script
  `frontend/tools/generate-brochure.mjs`) regenerates the brochure from
  `docs/mocks/brochure/liturgy-overview.html`. The PDF is a **committed artifact**,
  deliberately not built in CI — that would drag Playwright browser installs into
  the deploy pipeline for a file that changes rarely. Regeneration is a manual
  step whenever the brochure source changes.
- **Google Fonts** is the marketing site's only external runtime dependency
  (Hanken Grotesk, Inter, Newsreader). Self-hosting the fonts is a reasonable
  Phase 2 / privacy hardening step.
- **SEO.** Shipped with Phase 1: Open Graph / Twitter card meta, a social card
  image (`marketing/assets/social-card.png`), a favicon link, and
  `marketing/robots.txt` (allows all; disallows `/api/` and `/hubs/`). Still
  waiting on a domain: `sitemap.xml` (needs absolute URLs) and a canonical URL.
- **Unchanged:** the `/health` endpoint, the deploy gate on the
  `AZURE_WEBAPP_NAME` repo variable, and the OIDC deploy flow (see
  [ci-cd.md](ci-cd.md)).

## 9. Consequences and non-goals

Phase 1 shipped as one change set touching:

- `marketing/` (new folder: promoted splash with app-route CTAs, SEO meta, social
  card, `robots.txt`, assets, PDF)
- `frontend/angular.json` (index rename to `app.html`)
- `backend/src/Liturgy.Api/Program.cs` (fallback → `app.html`, explicit
  `UseRouting()` after static files, cache-control policy)
- `.github/workflows/ci-cd.yml` (marketing copy step)
- `frontend/projects/liturgy-app/src/app/app.routes.ts` (dropped `''`, wildcard →
  `/dashboard`)
- `frontend/projects/liturgy-app/src/app/pages/landing/` (deleted)
- `frontend/e2e/live/screenshots.spec.ts` (landing capture removed — the static
  page never renders through `ng serve`)
- `frontend/tools/generate-brochure.mjs` (writes to `marketing/assets/`)

What is deliberately given up: the marketing pages no longer use Angular components
or `styles.scss`. The FaithTech tokens in `liturgy.css` — which were the origin of
the design system that `styles.scss` ported (see
[mocks/README.md](mocks/README.md)) — become the live styling source for marketing,
maintained by hand alongside the app's copy.

## 10. References

- [ci-cd.md](ci-cd.md) — pipeline and Azure setup runbook
- [ARCHITECTURE.md](ARCHITECTURE.md) — system overview
- [DECISIONS.md](DECISIONS.md) — #4 (JWT), #6 (API_BASE_URL), #7 (Playwright fake
  backend), #8 (vertical slice; marketing cover deferred), #11 (this decision)
- [implementation-plan.md](implementation-plan.md) — history of the landing surface
- [mocks/README.md](mocks/README.md) — the mock set and design tokens
