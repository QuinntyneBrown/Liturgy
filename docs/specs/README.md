# Liturgy Specifications

Specifications are grouped by **capability** — a general capability of the system (e.g. Authentication). Each capability folder contains:

- `L1.md` — the high-level requirements, numbered `L1-{code}-{#}` (starting at 001 per capability)
- `L2.md` — the detailed requirements, numbered `L2-{code}-{#}`, each tracing to one L1

Reverse-engineered from the `docs/mocks` design set and the implemented full-stack vertical
slice (`backend/` .NET 9 Clean Architecture; `frontend/` Angular 21). Liturgy is an agile
workspace that **enforces the FaithTech Playbook**: projects advance through the **4D Cycle**
(Discover → Discern → Develop → Demonstrate) behind requirement gates, and every Develop work
item must complete the **5R loop** (Request → Receive → Review → Render → Rejoice) before it
can reach Done. Enforcement is server-authoritative; collaborative surfaces update in real time.

Each requirement records a **Source** tag: `implementation` (built and tested in the slice),
`implementation + mocks` (built, and also specified visually), or `mocks (deferred)` (present
in the design mocks, deferred to a follow-up per the implementation plan). Mock-derived
requirements are stated as "must" because they define the intended product; teams building the
deferred surfaces trace to them.

---

Each requirement traces to exactly one capability and carries a **Source** tag (`implementation`,
`implementation + mocks`, or `mocks (deferred)`). Acceptance criteria are in Given/When/Then
form so they translate directly into acceptance tests. Acceptance test files must carry a
`Traces to: L2-<CODE>-###` (e.g. `Traces to: L2-AUTH-001`) comment header identifying the requirement(s) they cover.

Enum values travel over the wire as **strings**: `PhaseKind {Discover, Discern, Develop,
Demonstrate}`, `PhaseState {Locked, Current, Done}`, `GateState {Blocked, Open}`,
`RequirementState {Todo, Done}`, `BoardColumn {Backlog, InLoop, Review, Done}`, `RKind {Request,
Receive, Review, Render, Rejoice}`, `MovementState {Locked, Current, Done}`, `CardStatus {Open,
Closed, Cancelled}`, `ProjectStatus {Active, Closed}`, `InvitationStatus {Pending, Accepted,
Revoked}`.

A `Card` additionally carries `Description` (nullable string) and `Points` (nullable int — a free
story-point value); a `Project` carries `Status` (`ProjectStatus`, default `Active`). Two
product nouns are synonyms for existing domain nouns: an **Account** is a `Workspace`, and a
**Ticket** is a `Card` — the specs keep `Workspace` and `Card` as the canonical names.

## Capabilities

| Code | Capability | L1s | L2s |
|------|------------|-----|-----|
| `AUTH` | [Authentication](./authentication/L1.md) | [1](./authentication/L1.md) | [16](./authentication/L2.md) |
| `WKSP` | [Workspace Management](./workspace-management/L1.md) | [3](./workspace-management/L1.md) | [16](./workspace-management/L2.md) |
| `PLAY` | [Playbook Enforcement](./playbook-enforcement/L1.md) | [4](./playbook-enforcement/L1.md) | [18](./playbook-enforcement/L2.md) |
| `WORK` | [Work Management](./work-management/L1.md) | [2](./work-management/L1.md) | [20](./work-management/L2.md) |
| `COLLAB` | [Collaboration](./collaboration/L1.md) | [1](./collaboration/L1.md) | [4](./collaboration/L2.md) |
| `IMPACT` | [Impact](./impact/L1.md) | [1](./impact/L1.md) | [3](./impact/L2.md) |
| `UX` | [User Experience](./user-experience/L1.md) | [2](./user-experience/L1.md) | [10](./user-experience/L2.md) |
| `SEC` | [Security](./security/L1.md) | [1](./security/L1.md) | [4](./security/L2.md) |
| `PLAT` | [Platform Quality](./platform-quality/L1.md) | [2](./platform-quality/L1.md) | [6](./platform-quality/L2.md) |

## Legacy ID mapping

| Old ID | New ID |
|--------|--------|
| L1-001 | `L1-AUTH-001` |
| L1-002 | `L1-WKSP-001` |
| L1-003 | `L1-WKSP-002` |
| L1-004 | `L1-PLAY-001` |
| L1-005 | `L1-PLAY-002` |
| L1-006 | `L1-PLAY-003` |
| L1-007 | `L1-WORK-001` |
| L1-008 | `L1-WORK-002` |
| L1-009 | `L1-PLAY-004` |
| L1-010 | `L1-COLLAB-001` |
| L1-011 | `L1-IMPACT-001` |
| L1-012 | `L1-UX-001` |
| L1-013 | `L1-UX-002` |
| L1-014 | `L1-SEC-001` |
| L1-015 | `L1-PLAT-001` |
| L1-016 | `L1-PLAT-002` |
| L2-001 | `L2-AUTH-001` |
| L2-002 | `L2-AUTH-002` |
| L2-003 | `L2-AUTH-003` |
| L2-004 | `L2-AUTH-004` |
| L2-005 | `L2-AUTH-005` |
| L2-006 | `L2-AUTH-006` |
| L2-007 | `L2-AUTH-007` |
| L2-008 | `L2-AUTH-008` |
| L2-009 | `L2-AUTH-009` |
| L2-010 | `L2-AUTH-010` |
| L2-011 | `L2-AUTH-011` |
| L2-012 | `L2-AUTH-012` |
| L2-013 | `L2-AUTH-013` |
| L2-014 | `L2-AUTH-014` |
| L2-015 | `L2-WKSP-001` |
| L2-016 | `L2-WKSP-002` |
| L2-017 | `L2-WKSP-003` |
| L2-018 | `L2-WKSP-004` |
| L2-019 | `L2-WKSP-005` |
| L2-020 | `L2-PLAY-001` |
| L2-021 | `L2-PLAY-002` |
| L2-022 | `L2-PLAY-003` |
| L2-023 | `L2-PLAY-004` |
| L2-024 | `L2-PLAY-005` |
| L2-025 | `L2-PLAY-006` |
| L2-026 | `L2-PLAY-007` |
| L2-027 | `L2-PLAY-008` |
| L2-028 | `L2-PLAY-009` |
| L2-029 | `L2-PLAY-010` |
| L2-030 | `L2-PLAY-011` |
| L2-031 | `L2-PLAY-012` |
| L2-032 | `L2-WORK-001` |
| L2-033 | `L2-WORK-002` |
| L2-034 | `L2-WORK-003` |
| L2-035 | `L2-WORK-004` |
| L2-036 | `L2-WORK-005` |
| L2-037 | `L2-WORK-006` |
| L2-038 | `L2-WORK-007` |
| L2-039 | `L2-WORK-008` |
| L2-040 | `L2-WORK-009` |
| L2-041 | `L2-WORK-010` |
| L2-042 | `L2-WORK-011` |
| L2-043 | `L2-WORK-012` |
| L2-044 | `L2-WORK-013` |
| L2-045 | `L2-PLAY-013` |
| L2-046 | `L2-PLAY-014` |
| L2-047 | `L2-PLAY-015` |
| L2-048 | `L2-PLAY-016` |
| L2-049 | `L2-PLAY-017` |
| L2-050 | `L2-PLAY-018` |
| L2-051 | `L2-COLLAB-001` |
| L2-052 | `L2-COLLAB-002` |
| L2-053 | `L2-COLLAB-003` |
| L2-054 | `L2-COLLAB-004` |
| L2-055 | `L2-IMPACT-001` |
| L2-056 | `L2-IMPACT-002` |
| L2-057 | `L2-IMPACT-003` |
| L2-058 | `L2-UX-001` |
| L2-059 | `L2-UX-002` |
| L2-060 | `L2-UX-003` |
| L2-061 | `L2-UX-004` |
| L2-062 | `L2-UX-005` |
| L2-063 | `L2-UX-006` |
| L2-064 | `L2-UX-007` |
| L2-065 | `L2-UX-008` |
| L2-066 | `L2-UX-009` |
| L2-067 | `L2-UX-010` |
| L2-068 | `L2-SEC-001` |
| L2-069 | `L2-SEC-002` |
| L2-070 | `L2-SEC-003` |
| L2-071 | `L2-SEC-004` |
| L2-072 | `L2-PLAT-001` |
| L2-073 | `L2-PLAT-002` |
| L2-074 | `L2-PLAT-003` |
| L2-075 | `L2-PLAT-004` |
| L2-076 | `L2-PLAT-005` |
| L2-077 | `L2-PLAT-006` |
