# Liturgy — HTML mocks

Static, high-fidelity design mocks for **Liturgy**, an agile-development workspace that
*enforces* the [FaithTech Playbook](https://www.faithtech.com/playbook): the **4D Cycle** as
its spine and the **5R co-creation loop** inside every build.

Open `index.html` in a browser — it's the cover and gallery for the whole set. No build step;
the only external dependency is Google Fonts.

## The framework these screens model

- **4D Cycle** (sequential) — `01 Discover → 02 Discern → 03 Develop → 04 Demonstrate`
  - **Discern** offers four paths: **Reject · Receive · Reimagine · Create**
- **5R co-creation loop** (inside Develop, repeats per work item) —
  `Request → Receive → Review → Render → Rejoice`

Enforcement is a hard gate: a phase stays **locked** until its required artifacts are complete,
and a work item cannot reach **Done** until all five R's are logged.

## Screens

| File | Screen |
|------|--------|
| `index.html` | Cover + gallery |
| `design-system.html` | Style guide (tokens, type, components) |
| `dashboard.html` | Workspace home — projects plotted on the 4D cycle |
| `project-4d.html` | A project's 4D journey with enforced phase gates |
| `discern-gate.html` | The Discern decision (Reject/Receive/Reimagine/Create) |
| `develop-board.html` | Agile Kanban with 5R pips and a locked Done column |
| `5r-loop.html` | The 5R co-creation ritual for one work item |
| `demonstrate.html` | Impact & Rejoice — "friendship compounded by time" |

## Design system

Colors and type are taken **directly from FaithTech's live design system**
(`faithtech-v2.webflow.shared.css`) — nothing is invented. The single source of truth is
`assets/liturgy.css`; see `design-system.html` for the documented tokens.

- **Ink** `#16160c` · **Paper** `#ffffff` · **Signature lime** `#c6fb50`
- Warm sand greys `#f5f0f0` → `#e4e0d8` → `#c6c5bb`
- Phase coding — Discover `#1d8fb9`, Discern `#f05228`, Develop `#32a432`, Demonstrate `#ffb300`
- Type — Hanken Grotesk (display, standing in for FaithTech's Noi Grotesk) · Inter (body, exact) ·
  Newsreader italic (scripture accents, echoing Avril/Palatino)

The **Rhythm Rail** — the vertical 4D spine with gate-latches, expanding into a
canonical-hours 5R dial during Develop — is the signature element carrying the "liturgy" idea.
