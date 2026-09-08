import { expect } from '@playwright/test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

export async function marketing({ page, say, base, directory }) {
  await page.goto(base);
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Build in');
  await page.evaluate(() => document.fonts.ready);
  await say('welcome');
  await page
    .getByRole('heading', { name: /Your process forms your team/ })
    .scrollIntoViewIfNeeded();
  await say('purpose');
  await page.locator('#rhythm').scrollIntoViewIfNeeded();
  await say('rhythm');
  await page.locator('#board').scrollIntoViewIfNeeded();
  await say('board');
  await page.evaluate(() => window.scrollTo(0, 0));
  const downloadPromise = page.waitForEvent('download');
  await page
    .getByRole('link', { name: 'Download the product overview (PDF)', exact: true })
    .click();
  const download = await downloadPromise;
  assert.equal(await download.failure(), null);
  const pdfPath = `${directory}/brochure.pdf`;
  await download.saveAs(pdfPath);
  const pdf = await readFile(pdfPath);
  assert.equal(pdf.subarray(0, 5).toString(), '%PDF-');
  assert.ok(pdf.length > 10000, 'Brochure must contain a substantial PDF');
  await say('overview');
  await page.getByRole('link', { name: 'Sign in', exact: true }).first().click();
  await expect(page.locator('#email')).toBeVisible();
  await expect(page.locator('#password')).toBeVisible();
  await say('signin');
  await say('close');
}

export async function workspace({ page, say, base, api, auth }) {
  // Real UI authentication happens before footage starts; no token injection.
  await page.goto(`${base}/dashboard`);
  await expect(page).toHaveURL(/\/dashboard$/);
  await say('welcome');
  await say('dashboard');
  await page
    .getByRole('link', { name: /Projects/ })
    .first()
    .click();
  await expect(page.locator('.project-tile', { hasText: 'Lantern' })).toBeVisible();
  await say('projects');
  await page.locator('.project-tile', { hasText: 'Lantern' }).getByRole('link').first().click();
  const projectId = new URL(page.url()).pathname.split('/').at(-1);
  const gate = page.locator('.gate', { hasText: 'Develop → Demonstrate' });
  await expect(gate).toBeVisible();
  await say('journey');
  await gate.scrollIntoViewIfNeeded();
  await expect(gate).toHaveClass(/is-blocked/);
  await expect(gate.getByRole('button', { name: /Advance to Demonstrate/ })).toBeDisabled();
  const initialJourney = await api('GET', `/api/projects/${projectId}`, undefined, auth);
  assert.equal(initialJourney.body.phases.find((p) => p.kind === 'Develop').gate.state, 'Blocked');
  await say('gate');
  await page.getByRole('link', { name: /Open board/ }).click();
  await expect(page.locator('.wcard', { hasText: 'LAN-24' })).toBeVisible();
  await say('board');
  await page.locator('.wcard', { hasText: 'LAN-24' }).locator('.wcard__title').click();
  const cardId = new URL(page.url()).pathname.split('/').at(-1);
  const done = page.getByRole('button', { name: 'Mark done' });
  await expect(done).toBeDisabled();
  await expect(page.locator('#main .dial__count')).toHaveText('3/5');
  await say('loop');
  await page.locator('#artifactUrl').scrollIntoViewIfNeeded();
  await say('render-intro');
  await page.locator('#artifactUrl').fill('demo/lantern/handoff-script-v2');
  await page
    .locator('#whatChanged')
    .fill(
      'Clarified the escalation path and preserved a caring handoff in this synthetic example.',
    );
  await say('render-filled');
  await page.getByRole('button', { name: 'Log & continue', exact: true }).click();
  await expect(page.locator('#thanksgiving')).toBeVisible();
  await expect(page.locator('#main .dial__count')).toHaveText('4/5');
  await say('render-saved');
  await page
    .locator('#thanksgiving')
    .fill(
      'Thank you, God, for neighbours who serve with patience and care, and for the team that shaped this example.',
    );
  await say('rejoice');
  await page.getByRole('button', { name: 'Log & continue', exact: true }).click();
  await expect(page.locator('#main .dial__count')).toHaveText('5/5');
  await expect(done).toBeEnabled();
  await done.scrollIntoViewIfNeeded();
  await say('complete');
  await done.click();
  await expect(page).toHaveURL(/\/board\//);
  await page.reload();
  await expect(page.locator('.wcard', { hasText: 'LAN-24' })).toBeVisible();
  const board = await api('GET', `/api/board/${projectId}`, undefined, auth);
  assert.equal(board.body.cards.find((c) => c.id === cardId).column, 'Done');
  await page.locator('.wcard', { hasText: 'LAN-24' }).scrollIntoViewIfNeeded();
  await say('done');
  await page.goto(`${base}/projects/${projectId}`);
  await gate.scrollIntoViewIfNeeded();
  await say('return-gate');
  await page
    .getByRole('button', { name: 'Toggle: Demo prepared for the community', exact: true })
    .click();
  await expect(gate.locator('.badge--done')).toHaveText('Open');
  await page.reload();
  await expect(gate.locator('.badge--done')).toHaveText('Open');
  await gate.scrollIntoViewIfNeeded();
  await say('gate-open');
  await say('close');
}

export async function apiStory({ page, say, api, auth }) {
  await page.setContent(`<html><head><style>
    *{box-sizing:border-box}body{margin:0;background:#16241f;color:#eef3e7;font:22px system-ui;padding:44px 60px}
    header{display:flex;justify-content:space-between;color:#c6fb50;font-size:16px;letter-spacing:2px}
    h1{font-size:38px;font-weight:500;margin:26px 0}#request{font:21px Consolas;color:#c6fb50;margin-bottom:18px}
    pre{white-space:pre-wrap;font:21px/1.5 Consolas;margin:0;max-height:350px;overflow:hidden}
    #status{color:#a4b7ac;font-size:16px}footer{position:fixed;bottom:108px;font-size:14px;color:#a4b7ac}
  </style></head><body><header><span>LITURGY / API</span><span>REAL LOCAL REQUESTS</span></header>
  <h1 id="heading">Shared rules for the team</h1><div id="request">Authenticated demonstration</div>
  <pre id="response">Fresh seeded data · SQL Server persistence</pre><p id="status"></p>
  <footer>Selected response fields · credentials omitted · synthetic records</footer></body></html>`);
  const show = async (heading, method, path, result, fields, requestBody) => {
    await page.evaluate(
      ({ heading, method, path, result, fields, requestBody }) => {
        document.querySelector('#heading').textContent = heading;
        document.querySelector('#request').textContent = `${method} ${path}`;
        document.querySelector('#response').textContent =
          (requestBody ? `Request: ${JSON.stringify(requestBody)}\n\n` : '') +
          JSON.stringify(fields, null, 2);
        document.querySelector('#status').textContent =
          `HTTP ${result.status} · response received from the running application`;
      },
      { heading, method, path, result: { status: result.status }, fields, requestBody },
    );
    assert.ok(
      await page
        .locator('#response')
        .evaluate((element) => element.scrollHeight <= element.clientHeight),
      'API response display must fit without clipping',
    );
  };
  await say('welcome');
  const projects = await api('GET', '/api/projects', undefined, auth);
  assert.equal(projects.status, 200);
  const lantern = projects.body.find((p) => p.name === 'Lantern');
  assert.ok(lantern);
  await show(
    'Read the workspace',
    'GET',
    '/api/projects',
    projects,
    projects.body.map((p) => `${p.name} — ${p.currentPhase}`),
  );
  await say('projects');
  const board = await api('GET', `/api/board/${lantern.id}`, undefined, auth);
  const card = board.body.cards.find((c) => c.code === 'LAN-24');
  assert.ok(card);
  const loopPath = `/api/loop/cards/${card.id}`;
  const brief = (b) => ({
    code: b.code,
    loggedCount: b.loggedCount,
    currentR: b.currentR,
    canMarkDone: b.canMarkDone,
  });
  let loop = await api('GET', loopPath, undefined, auth);
  assert.equal(loop.body.loggedCount, 3);
  await show('Three movements recorded', 'GET', loopPath, loop, brief(loop.body));
  await say('loop');
  const rejected = await api('POST', `${loopPath}/done`, {}, auth);
  assert.equal(rejected.status, 409);
  assert.match(rejected.body.title, /incomplete/);
  await show('Premature Done is rejected', 'POST', `${loopPath}/done`, rejected, {
    status: rejected.body.status,
    title: rejected.body.title,
  });
  await say('reject');
  loop = await api(
    'POST',
    `${loopPath}/movements`,
    {
      kind: 'Render',
      artifactUrl: 'demo/lantern/handoff-script-v2',
      whatChanged: 'Clarified the handoff in this synthetic example.',
    },
    auth,
  );
  assert.equal(loop.status, 200);
  assert.equal(loop.body.loggedCount, 4);
  assert.equal(loop.body.currentR, 'Rejoice');
  await show('Render saved', 'POST', `${loopPath}/movements`, loop, brief(loop.body), {
    kind: 'Render',
  });
  await say('render');
  loop = await api(
    'POST',
    `${loopPath}/movements`,
    { kind: 'Rejoice', thanksgiving: 'Thank you, God, for neighbours who serve with care.' },
    auth,
  );
  assert.equal(loop.status, 200);
  assert.equal(loop.body.loggedCount, 5);
  assert.equal(loop.body.canMarkDone, true);
  await show('The loop is complete', 'POST', `${loopPath}/movements`, loop, brief(loop.body), {
    kind: 'Rejoice',
  });
  await say('rejoice');
  const done = await api('POST', `${loopPath}/done`, {}, auth);
  assert.equal(done.status, 200);
  const savedBoard = await api('GET', `/api/board/${lantern.id}`, undefined, auth);
  const saved = savedBoard.body.cards.find((c) => c.id === card.id);
  assert.equal(saved.column, 'Done');
  await show('Read back the saved card', 'GET', `/api/board/${lantern.id}`, savedBoard, {
    code: saved.code,
    title: saved.title,
    column: saved.column,
  });
  await say('done');
  const projectPath = `/api/projects/${lantern.id}`;
  const project = await api('GET', projectPath, undefined, auth);
  const gate = project.body.phases.find((p) => p.kind === 'Develop').gate;
  assert.equal(gate.state, 'Blocked');
  const requirement = gate.requirements.find(
    (r) =>
      r.label === 'Demo prepared for the community' ||
      r.title === 'Demo prepared for the community',
  );
  assert.ok(requirement, 'Expected seeded demo requirement');
  await show('A blocked checklist gate', 'GET', projectPath, project, {
    title: gate.title,
    state: gate.state,
    outstanding: requirement.label ?? requirement.title,
  });
  await say('gate-before');
  const toggled = await api(
    'POST',
    `/api/gates/requirements/${requirement.id}/toggle`,
    { done: true },
    auth,
  );
  assert.equal(toggled.status, 200);
  const savedProject = await api('GET', projectPath, undefined, auth);
  const savedGate = savedProject.body.phases.find((p) => p.kind === 'Develop').gate;
  assert.equal(savedGate.state, 'Open');
  await show('Read back the open gate', 'GET', projectPath, savedProject, {
    title: savedGate.title,
    state: savedGate.state,
    currentPhase: savedProject.body.currentPhase,
  });
  await say('gate-after');
  await say('close');
}
