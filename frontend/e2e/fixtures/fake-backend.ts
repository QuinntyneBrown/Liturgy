import { Page, Route } from '@playwright/test';

/**
 * A stateful in-memory fake of the Liturgy API for E2E. It models just enough of the
 * enforcement engine — the Develop → Demonstrate gate and the 5R loop — so the specs
 * prove the UI's behaviour without a real backend. SignalR hub requests are aborted so
 * the app degrades cleanly to REST.
 */
export interface FakeState {
  journey: any;
  board: any;
  loops: Record<string, any>;
  decision: any;
  impact: any;
}

const PROJECT_ID = 'p-lantern';

function initialState(): FakeState {
  const journey = {
    id: PROJECT_ID,
    name: 'Lantern',
    tag: 'After-hours crisis line',
    currentPhase: 'Develop',
    phases: [
      { id: 'ph-0', kind: 'Discover', state: 'Done', order: 0, gate: null },
      { id: 'ph-1', kind: 'Discern', state: 'Done', order: 1, gate: null },
      {
        id: 'ph-2',
        kind: 'Develop',
        state: 'Current',
        order: 2,
        gate: {
          id: 'gate-dev',
          phaseId: 'ph-2',
          title: 'Develop → Demonstrate',
          state: 'Blocked',
          requirements: [
            { id: 'req-1', label: 'Every work item has completed the 5R loop', meta: '5 of 7', state: 'Done', order: 0 },
            { id: 'req-2', label: 'Impact reframed as friendship compounded by time', meta: 'draft', state: 'Done', order: 1 },
            { id: 'req-3', label: 'Demo prepared for the community', meta: 'required', state: 'Todo', order: 2 },
          ],
        },
      },
      { id: 'ph-3', kind: 'Demonstrate', state: 'Locked', order: 3, gate: null },
    ],
  };

  const card = (id: string, code: string, title: string, column: string, logged: number, currentR: string | null) => ({
    id,
    projectId: PROJECT_ID,
    sprintId: 'sprint-6',
    code,
    title,
    assigneeId: null,
    assigneeInitials: 'JP',
    column,
    currentR,
    isBlocked: false,
    loggedCount: logged,
  });

  const board = {
    projectId: PROJECT_ID,
    sprintId: 'sprint-6',
    sprintNumber: 6,
    cards: [
      card('card-24', 'LAN-24', 'Warm-handoff script when a caller is in danger', 'InLoop', 3, 'Render'),
      card('card-33', 'LAN-33', 'Crisis resource directory by region', 'Backlog', 0, 'Request'),
      card('card-12', 'LAN-12', 'Quiet-hours routing rules', 'Done', 5, null),
    ],
  };

  const R = ['Request', 'Receive', 'Review', 'Render', 'Rejoice'];
  const makeLoop = (cardId: string, code: string, title: string, logged: number) => ({
    cardId,
    projectId: PROJECT_ID,
    code,
    title,
    column: logged === 0 ? 'Backlog' : 'InLoop',
    currentR: logged >= 5 ? null : R[logged],
    loggedCount: logged,
    canMarkDone: logged >= 5,
    movements: R.map((kind, i) => ({
      id: `${cardId}-m${i}`,
      kind,
      order: i + 1,
      state: i < logged ? 'Done' : i === logged ? 'Current' : 'Locked',
      ask: kind === 'Request' && i < logged ? 'Invite the Spirit.' : null,
      received: kind === 'Receive' && i < logged ? 'Captured notes.' : null,
      synthesis: kind === 'Review' && i < logged ? 'A buildable direction.' : null,
      artifactUrl: kind === 'Render' && i < logged ? 'prs/lantern/handoff' : null,
      whatChanged: kind === 'Render' && i < logged ? 'Simplified the path.' : null,
      thanksgiving: kind === 'Rejoice' && i < logged ? 'Grateful.' : null,
      loggedAt: i < logged ? '2026-07-14T00:00:00Z' : null,
    })),
  });

  return {
    journey,
    board,
    loops: {
      'card-24': makeLoop('card-24', 'LAN-24', 'Warm-handoff script when a caller is in danger', 3),
      'card-33': makeLoop('card-33', 'LAN-33', 'Crisis resource directory by region', 0),
    },
    decision: { chosenPath: null, rationale: '', prayedOverWith: '', decidedAt: null },
    impact: {
      headline: 'Impact is friendship, compounded by time.',
      metrics: [
        { value: '17', unit: ' wks', label: 'Weeks walked alongside the same 6 families', highlight: false },
        { value: '4', unit: null, label: 'Stories of change, in their own words', highlight: true },
      ],
      stories: [
        { week: 2, text: 'Maria signed up for a meal. She knew no one on her street.' },
        { week: 17, text: 'Maria is training two others to host.' },
      ],
      gratitude: [{ quote: 'He gave us a table, not a feature.', attribution: 'Team retro, week 12' }],
    },
  };
}

function json(route: Route, body: unknown, status = 200): Promise<void> {
  return route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(body) });
}

export async function installFakeBackend(page: Page): Promise<FakeState> {
  const state = initialState();

  await page.route('**/hubs/**', (route) => route.abort());

  await page.route('**/api/**', async (route) => {
    const request = route.request();
    const method = request.method();
    const url = new URL(request.url());
    const path = url.pathname;

    // --- Auth ---
    if (path === '/api/auth/sign-in' || path === '/api/auth/register') {
      return json(route, {
        accessToken: 'fake-token',
        userId: 'u1',
        email: 'quinn@newhope.dev',
        role: 'Member',
        firstName: 'Quinn',
        lastName: 'Brown',
        initials: 'QB',
      });
    }
    if (path === '/api/me') {
      return json(route, { id: 'u1', email: 'quinn@newhope.dev', firstName: 'Quinn', lastName: 'Brown', role: 'Member', initials: 'QB' });
    }

    // --- Members ---
    if (path === '/api/members' && method === 'GET') {
      return json(route, [
        { id: 'u1', name: 'Quinn Brown', initials: 'QB', role: 'Lead' },
        { id: 'u2', name: 'Jonah Park', initials: 'JP', role: 'Developer' },
      ]);
    }

    // --- Dashboard ---
    if (path === '/api/dashboard' && method === 'GET') {
      return json(route, {
        momentum: { activeProjects: 1, movementsThisWeek: 3, gatesBlocked: 1, weeksCompounded: 17 },
        attention: [
          {
            projectId: PROJECT_ID,
            projectName: 'Lantern',
            title: 'Lantern · Develop → Demonstrate',
            reason: '1 requirement remaining before this gate opens.',
            actionKind: 'gate',
            actionTargetId: PROJECT_ID,
          },
        ],
        projects: [
          { id: PROJECT_ID, name: 'Lantern', currentPhase: state.journey.currentPhase, meta: 'Sprint 6 · 3.0/5 R avg', blocked: true },
        ],
      });
    }

    // --- Projects / journey ---
    if (path === '/api/projects' && method === 'GET') {
      return json(route, [{ id: PROJECT_ID, name: 'Lantern', tag: 'After-hours crisis line', currentPhase: state.journey.currentPhase }]);
    }
    if (path === '/api/projects' && method === 'POST') {
      const body = request.postDataJSON() as { name: string; tag: string };
      return json(route, { id: 'p-new', name: body.name, tag: body.tag, currentPhase: 'Discover' });
    }
    if (path === `/api/projects/${PROJECT_ID}` && method === 'GET') {
      return json(route, state.journey);
    }

    // --- Decision (Discern) ---
    const decisionMatch = path.match(/^\/api\/projects\/(.+)\/decision$/);
    if (decisionMatch) {
      if (method === 'GET') {
        return json(route, state.decision);
      }
      if (method === 'PUT') {
        const body = request.postDataJSON() as { chosenPath: string; rationale: string; prayedOverWith: string };
        state.decision = { ...body, decidedAt: '2026-07-15T00:00:00Z' };
        return json(route, state.decision);
      }
    }

    // --- Impact (Demonstrate) ---
    const impactMatch = path.match(/^\/api\/projects\/(.+)\/impact$/);
    if (impactMatch && method === 'GET') {
      return json(route, state.impact);
    }

    // --- Gate requirement toggle (recompute + unlock next phase) ---
    const toggleMatch = path.match(/^\/api\/gates\/requirements\/(.+)\/toggle$/);
    if (toggleMatch && method === 'POST') {
      const requirementId = toggleMatch[1];
      const body = request.postDataJSON() as { done: boolean };
      const develop = state.journey.phases.find((p: any) => p.kind === 'Develop');
      const gate = develop.gate;
      const req = gate.requirements.find((r: any) => r.id === requirementId);
      req.state = body.done ? 'Done' : 'Todo';
      const allDone = gate.requirements.every((r: any) => r.state === 'Done');
      gate.state = allDone ? 'Open' : 'Blocked';
      if (allDone) {
        develop.state = 'Done';
        const demo = state.journey.phases.find((p: any) => p.kind === 'Demonstrate');
        demo.state = 'Current';
        state.journey.currentPhase = 'Demonstrate';
      }
      return json(route, gate);
    }

    // --- Board ---
    if (path === `/api/board/${PROJECT_ID}` && method === 'GET') {
      return json(route, state.board);
    }

    if (path === '/api/board/cards' && method === 'POST') {
      const body = request.postDataJSON() as { title: string };
      const newCard = {
        id: `card-${state.board.cards.length + 1}`,
        projectId: PROJECT_ID,
        sprintId: 'sprint-6',
        code: `LAN-${90 + state.board.cards.length}`,
        title: body.title,
        assigneeId: null,
        assigneeInitials: null,
        column: 'Backlog',
        currentR: 'Request',
        isBlocked: false,
        loggedCount: 0,
      };
      state.board.cards.push(newCard);
      return json(route, newCard);
    }

    const assignMatch = path.match(/^\/api\/board\/cards\/(.+)\/assign$/);
    if (assignMatch && method === 'POST') {
      const body = request.postDataJSON() as { assigneeId: string | null };
      const card = state.board.cards.find((c: any) => c.id === assignMatch[1]);
      if (card) {
        card.assigneeId = body.assigneeId;
        card.assigneeInitials = body.assigneeId ? 'JP' : null;
      }
      return json(route, card ?? {});
    }

    const moveMatch = path.match(/^\/api\/board\/cards\/(.+)\/move$/);
    if (moveMatch && method === 'POST') {
      const cardId = moveMatch[1];
      const body = request.postDataJSON() as { column: string };
      const card = state.board.cards.find((c: any) => c.id === cardId);
      if (body.column === 'Done' && card.loggedCount < 5) {
        return json(route, { title: 'The 5R loop is incomplete.' }, 409);
      }
      card.column = body.column;
      if (body.column === 'Done') {
        card.currentR = null;
      }
      return json(route, card);
    }

    // --- Loop ---
    const loopGet = path.match(/^\/api\/loop\/cards\/(.+)$/);
    if (loopGet && method === 'GET') {
      return json(route, state.loops[loopGet[1]]);
    }

    const logMatch = path.match(/^\/api\/loop\/cards\/(.+)\/movements$/);
    if (logMatch && method === 'POST') {
      const cardId = logMatch[1];
      const loop = state.loops[cardId];
      loop.loggedCount += 1;
      const R = ['Request', 'Receive', 'Review', 'Render', 'Rejoice'];
      loop.movements.forEach((m: any, i: number) => {
        m.state = i < loop.loggedCount ? 'Done' : i === loop.loggedCount ? 'Current' : 'Locked';
      });
      loop.currentR = loop.loggedCount >= 5 ? null : R[loop.loggedCount];
      loop.canMarkDone = loop.loggedCount >= 5;
      loop.column = 'InLoop';
      return json(route, loop);
    }

    const doneMatch = path.match(/^\/api\/loop\/cards\/(.+)\/done$/);
    if (doneMatch && method === 'POST') {
      const cardId = doneMatch[1];
      const card = state.board.cards.find((c: any) => c.id === cardId) ?? { id: cardId, projectId: PROJECT_ID };
      card.column = 'Done';
      return json(route, card);
    }

    return json(route, { title: `Unhandled ${method} ${path}` }, 404);
  });

  return state;
}
