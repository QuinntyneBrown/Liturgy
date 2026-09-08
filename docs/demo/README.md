# Narrated Liturgy demos

Three real application demonstrations, narrated locally with **Microsoft David Desktop**, the installed Windows male English voice, at SAPI rate `-1`. The script uses warm Christian language about prayer, discernment, service, and thanksgiving. Windows voice metadata does not establish a Black narrator identity. There is no music or external speech service.

Open [the video gallery](index.html) or download the WebM files below. Captions are visible in the footage; matching WebVTT files and narration transcripts are also provided.

<!-- generated-demo-index:start -->
| Application | Status and measured media | Supporting files |
| --- | --- | --- |
| [marketing](liturgy-marketing.webm) | Recorded and reviewed · 1:40 · 1280 × 720 · 3.46 MiB | [Poster](liturgy-marketing-poster.png) · [Transcript](liturgy-marketing-transcript.md) · [Captions](liturgy-marketing.vtt) |
| [workspace](liturgy-workspace.webm) | Recorded and reviewed · 3:46 · 1280 × 720 · 39.90 MiB | [Poster](liturgy-workspace-poster.png) · [Transcript](liturgy-workspace-transcript.md) · [Captions](liturgy-workspace.vtt) |
| [api](liturgy-api.webm) | Recorded and reviewed · 2:16 · 1280 × 720 · 17.75 MiB | [Poster](liturgy-api-poster.png) · [Transcript](liturgy-api-transcript.md) · [Captions](liturgy-api.vtt) |

### marketing

Application revision: `e5fa9a25b8b22f8dff477f98978e30b7f01f0e64`. Run: `ba46afa46a26783c`.

- **0:04** Build in rhythm
- **0:13** A shared way of working
- **0:25** Discover · Discern · Develop · Demonstrate
- **0:39** Keep the ritual
- **0:59** Take the overview with you
- **1:14** Enter the workspace
- **1:26** Build with care

### workspace

Application revision: `e5fa9a25b8b22f8dff477f98978e30b7f01f0e64`. Run: `879804db65068f5b`.

- **0:00** Liturgy · Team workspace
- **0:12** Begin with the team
- **0:28** Choose a project
- **0:45** The four D journey
- **0:58** A visible boundary
- **1:16** Develop together
- **1:27** Three of five movements complete
- **1:42** Render · Give the work form
- **1:53** Record the result
- **2:07** Render recorded
- **2:18** Rejoice · Give thanks
- **2:30** Five of five movements complete
- **2:49** Done, and saved
- **3:01** Prepare to demonstrate
- **3:18** The checklist gate opens
- **3:31** A rhythm of faithful work

### api

Application revision: `e5fa9a25b8b22f8dff477f98978e30b7f01f0e64`. Run: `689303739a845bc4`.

- **0:00** Liturgy · API enforcement
- **0:14** Read the workspace
- **0:30** Inspect the loop
- **0:43** An incomplete loop is rejected
- **0:57** Record Render
- **1:10** Record Rejoice
- **1:21** Complete and read back
- **1:35** Inspect the gate checklist
- **1:49** Save and verify the gate
- **2:01** Shared rules, faithful practice
<!-- generated-demo-index:end -->

## Applications and local setup

| Application    | Purpose and audience                                       | Entrypoint / surface                                       | Dependencies and authentication                       |
| -------------- | ---------------------------------------------------------- | ---------------------------------------------------------- | ----------------------------------------------------- |
| Marketing site | Public introduction for prospective FaithTech teams        | `marketing/index.html`, served at `/` by the API host      | Static assets, Google Fonts; public                   |
| Team workspace | Project journeys, board, and 5R workflows for team members | Angular `liturgy-app`, `/sign-in` and authenticated routes | API, SQL Server, SignalR; real email/password sign-in |
| API            | Server enforcement and persistence for client developers   | `backend/src/Liturgy.Api/Program.cs`, `/api/*`             | .NET 9, SQL Server; JWT bearer authentication         |

These are all first-party application surfaces. The three Angular libraries, backend class libraries, tests, brochure generator, and infrastructure are not separate application demos. There is no independently runnable worker.

The recorder builds Angular and publishes the API into a private directory under `artifacts/demo-runs/<run-id>/app`, then copies the marketing assets beside the Angular `app.html`. It launches `dotnet Liturgy.Api.dll` from that bundle on an unused `127.0.0.1` port. This follows the existing deployment arrangement, exercises same-origin requests, and does not reuse a development server or modify an existing `wwwroot`.

Prerequisites: Windows with Microsoft David Desktop available through SAPI, Node 22, the .NET SDK selected by `backend/global.json` (9.0.308 in this recording), SQL Express or another authorized SQL Server with Windows authentication and create/drop database permissions, ODBC `sqlcmd`, Playwright Chromium matching the lockfile, and FFmpeg/ffprobe. The recording command uses finite build, startup, request, take, encoding, and cleanup deadlines.

Run from `C:\projects\Liturgy\frontend`:

```powershell
npm ci
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/demo/setup-tools.ps1

# This machine has the required SDK in the user installation:
$env:LITURGY_DEMO_DOTNET = 'C:\Users\quinn\.dotnet\dotnet.exe'
node tools/demo/record.mjs all
```

`setup-tools.ps1` downloads the pinned FFmpeg 9.0.1 essentials build, verifies its SHA256, expands it under ignored `artifacts/demo-tools`, and installs the matching Chromium. Sources: [Windows FFmpeg builds](https://www.gyan.dev/ffmpeg/builds/) and [Playwright browser installation](https://playwright.dev/docs/browsers). If these tools are already installed, set the overrides below instead of downloading them again.

| Optional variable        | Default / purpose                                                                |
| ------------------------ | -------------------------------------------------------------------------------- |
| `LITURGY_DEMO_DOTNET`    | `dotnet`; executable with the pinned SDK available                               |
| `LITURGY_DEMO_SQLSERVER` | `.\SQLEXPRESS`; dedicated demo connection uses integrated Windows authentication |
| `LITURGY_DEMO_SQLCMD`    | `sqlcmd`; executable path override                                               |
| `LITURGY_DEMO_FFMPEG`    | Discover under `artifacts/demo-tools/ffmpeg`, otherwise `ffmpeg`                 |
| `LITURGY_DEMO_FFPROBE`   | Beside an absolute FFmpeg path, otherwise `ffprobe`                              |

The harness supplies `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`, `ConnectionStrings__DefaultConnection`, and a generated `Jwt__SigningKey` only to its child process. Credentials and tokens are omitted from the recordings and deliverables. Workspace authentication is performed through the actual UI before recording, and the authenticated browser state stays in memory.

## Rerun, review, and promote

Each command prepares its own bundle, speech, database, and application prerequisites. To record one application, replace `all` with `marketing`, `workspace`, or `api`:

```powershell
node tools/demo/record.mjs workspace
```

The full order is marketing, workspace, API. Every story gets a separate database, so no video depends on mutations from an earlier take. The existing seeder supplies synthetic Lantern and team records. Each database is explicitly created with a unique `LiturgyDemo_<run-id>_<application>` name; only a database successfully created by that invocation is eligible for deletion.

The recorder prints its run directory. Review the encoded files at normal speed, inspect the chapter images and caption placement, then promote each verified set:

```powershell
# Set this to the exact run directory printed by record.mjs.
$demoRun = 'C:\projects\Liturgy\artifacts\demo-runs\<run-id>'
node tools/demo/review.mjs $demoRun

# After inspecting the generated review images and playback:
node tools/demo/promote.mjs $demoRun marketing --visuals-reviewed
node tools/demo/promote.mjs $demoRun workspace --visuals-reviewed
node tools/demo/promote.mjs $demoRun api --visuals-reviewed
node tools/demo/document.mjs
```

For a single application, also pass its name to `review.mjs`. Review plays the entire encoded file at 1×, checks media dimensions, duration, progress, end-of-playback, and decoding, and captures each chapter. Promotion verifies the reviewed video hash, backs up previous application files, and restores those files if promotion fails. The documentation command refreshes only the generated index above and the generated gallery, preserving the rest of this README.

## What is verified

- Marketing: real page navigation, a successful brochure download with a PDF header and substantive contents, and the sign-in route.
- Workspace: blocked checklist, three initially completed movements on LAN-24, Render and Rejoice persisted through UI actions, Done enabled only after 5/5, and card/gate state surviving reloads.
- API: real authenticated HTTP calls, HTTP 409 for premature Done, successful Render and Rejoice responses, and fresh reads confirming Done and an open checklist gate.
- Media: complete continuous successful takes, calibrated chapter timing, readable captions, 1280 × 720 video, Opus narration, full decode, normal-speed playback, and extracted footage posters.

The API video uses a browser presentation of actual HTTP responses, displaying selected response fields. It is not Swagger footage or a mocked terminal. A short colour calibration prefix is removed before the story; no story scenes are spliced or reordered. Windows SAPI generates speech segments before capture; they are mixed at the observed scene times into the continuous video. The static image examples on the marketing site illustrate the product; the workspace and API videos execute the product itself.

The checklist records team declarations: the server does not independently judge the quality of their artifacts. In this seeded workflow, completing the checklist also automatically advances Lantern into Demonstrate; this is visible in the journey and in the API's project response. The narration concentrates on the verified saved gate state. LAN-24 starts with three movements complete. Product impact examples are seeded data, and Liturgy remains the repository's demonstration system rather than a production system of record. The videos do not claim to verify concurrent editors or independent security/accessibility review.

Application revisions identify the unchanged tracked product source. The newly added recording tools are delivered alongside the videos, with their current recipe hashes in [recording-source-manifest.json](recording-source-manifest.json); they are not claimed to be committed at the application revision. Take durations, seeded dates, ordering, generated identifiers, and encoded bytes may vary on rerun.

The final audio mixes use a limiter at `0.75` with automatic gain disabled to leave headroom below clipping. To rebuild a mix from the original PCM segments without editing the continuous footage, set `LITURGY_DEMO_FFMPEG` and run `node tools/demo/remix.mjs $demoRun <application>`, then repeat playback review and promotion. The hash check rejects promotion against a review of an earlier mix.

Owned services are launched hidden and stopped in cleanup, including partial failures. Owned demo databases are dropped after each take. Transient footage, synthetic downloads, speech segments, diagnostics, and failure evidence remain under ignored `artifacts/demo-runs`; downloaded tools remain under ignored `artifacts/demo-tools`. Failed takes are never promoted. Cleanup failures are printed and recorded in the run's `result.json` with the resource requiring attention.

Recording sources: [orchestrator](../../frontend/tools/demo/record.mjs), [stories and assertions](../../frontend/tools/demo/stories.mjs), [narration](../../frontend/tools/demo/narration.mjs). Existing references: [live workflow test](../../frontend/e2e/live/real-flow.spec.ts), [marketing deployment](../marketing-deployment.md), [API controllers](../../backend/src/Liturgy.Api/Controllers), and [demo seeder](../../backend/src/Liturgy.Infrastructure/DevDataSeeder.cs).
