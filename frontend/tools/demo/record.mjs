import { chromium, expect } from '@playwright/test';
import { spawn, execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { mkdir, readFile, writeFile, cp, readdir, stat } from 'node:fs/promises';
import { createWriteStream } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import net from 'node:net';
import crypto from 'node:crypto';
import assert from 'node:assert/strict';
import { narration } from './narration.mjs';
import { marketing, workspace, apiStory } from './stories.mjs';

const execute = promisify(execFile);
const cancellation = new AbortController();
let activeBrowser;
for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => {
    cancellation.abort(new Error(`Recording interrupted by ${signal}`));
    void activeBrowser?.close();
  });
}
const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, '../../..');
const frontend = path.join(root, 'frontend');
const chosen = process.argv[2] ?? 'all';
assert.ok(
  ['all', ...Object.keys(narration)].includes(chosen),
  'Usage: node tools/demo/record.mjs [all|marketing|workspace|api]',
);
const selection = chosen === 'all' ? Object.keys(narration) : [chosen];
const runId = crypto.randomBytes(8).toString('hex');
const run = path.join(root, 'artifacts', 'demo-runs', runId);
await mkdir(run, { recursive: true });
console.log(`Run directory: ${run}`);

async function command(file, args, options = {}) {
  return execute(file, args, {
    windowsHide: true,
    timeout: 180000,
    maxBuffer: 16 * 1024 * 1024,
    signal: cancellation.signal,
    ...options,
  });
}
async function findFile(folder, name) {
  for (const item of await readdir(folder, { withFileTypes: true }).catch(() => [])) {
    const target = path.join(folder, item.name);
    if (item.isFile() && item.name === name) return target;
    if (item.isDirectory()) {
      const found = await findFile(target, name);
      if (found) return found;
    }
  }
}
const dotnet = process.env.LITURGY_DEMO_DOTNET ?? 'dotnet';
const ffmpeg =
  process.env.LITURGY_DEMO_FFMPEG ??
  (await findFile(path.join(root, 'artifacts/demo-tools/ffmpeg'), 'ffmpeg.exe')) ??
  'ffmpeg';
const ffprobe =
  process.env.LITURGY_DEMO_FFPROBE ??
  (path.isAbsolute(ffmpeg) ? path.join(path.dirname(ffmpeg), 'ffprobe.exe') : 'ffprobe');
const sqlcmd = process.env.LITURGY_DEMO_SQLCMD ?? 'sqlcmd';
const sqlServer = process.env.LITURGY_DEMO_SQLSERVER ?? '.\\SQLEXPRESS';
const versions = {};
for (const [label, file, args] of [
  ['dotnet', dotnet, ['--version']],
  ['ffmpeg', ffmpeg, ['-version']],
  ['ffprobe', ffprobe, ['-version']],
]) {
  versions[label] = (await command(file, args, { cwd: path.join(root, 'backend') })).stdout.split(
    /\r?\n/,
  )[0];
}
versions.node = process.version;
versions.playwright = JSON.parse(
  await readFile(path.join(frontend, 'node_modules/@playwright/test/package.json')),
).version;
const revision = (await command('git', ['rev-parse', 'HEAD'], { cwd: root })).stdout.trim();
const browserCheck = await chromium.launch({ headless: true });
await browserCheck.close();

// Stage a private bundle. Existing wwwroot and development servers are not reused.
const app = path.join(run, 'app');
console.log('Building the isolated application bundle...');
await command(process.execPath, ['node_modules/@angular/cli/bin/ng.js', 'build', 'liturgy-app'], {
  cwd: frontend,
  timeout: 300000,
});
await command(dotnet, ['publish', 'src/Liturgy.Api', '-c', 'Release', '-o', app], {
  cwd: path.join(root, 'backend'),
  timeout: 300000,
});
await mkdir(path.join(app, 'wwwroot'), { recursive: true });
await cp(path.join(frontend, 'dist/liturgy-app/browser'), path.join(app, 'wwwroot'), {
  recursive: true,
});
await cp(path.join(root, 'marketing'), path.join(app, 'wwwroot'), { recursive: true });

const segments = selection.flatMap((kind) =>
  narration[kind].map(([id, title, text]) => ({ id: `${kind}-${id}`, title, text })),
);
const manifest = path.join(run, 'speech.json');
const speech = path.join(run, 'speech');
await writeFile(manifest, JSON.stringify(segments, null, 2));
console.log('Generating Microsoft David narration locally...');
await command(
  'powershell.exe',
  [
    '-NoProfile',
    '-ExecutionPolicy',
    'Bypass',
    '-File',
    path.join(here, 'speak.ps1'),
    '-Manifest',
    manifest,
    '-OutputDirectory',
    speech,
  ],
  { timeout: 180000 },
);
async function probe(file) {
  return JSON.parse(
    (await command(ffprobe, ['-v', 'error', '-show_format', '-show_streams', '-of', 'json', file]))
      .stdout,
  );
}
const durations = new Map();
for (const segment of segments)
  durations.set(
    segment.id,
    Number((await probe(path.join(speech, `${segment.id}.wav`))).format.duration),
  );
await writeFile(
  path.join(run, 'provenance.json'),
  JSON.stringify(
    { runId, revision, versions, voice: 'Microsoft David Desktop', rate: -1 },
    null,
    2,
  ),
);

async function freePort() {
  const server = net.createServer();
  await new Promise((resolve, reject) => {
    server.once('error', reject);
    server.listen(0, '127.0.0.1', resolve);
  });
  const port = server.address().port;
  await new Promise((resolve) => server.close(resolve));
  return port;
}
async function sql(query) {
  return command(
    sqlcmd,
    ['-S', sqlServer, '-E', '-C', '-d', 'master', '-b', '-l', '15', '-t', '30', '-Q', query],
    { timeout: 45000, signal: undefined },
  );
}
async function pause(ms) {
  const { setTimeout } = await import('node:timers/promises');
  await setTimeout(ms, undefined, { signal: cancellation.signal });
}
function stamp(seconds) {
  return new Date(Math.max(0, seconds) * 1000).toISOString().slice(11, 23);
}
const failures = [];
for (const kind of selection) {
  if (cancellation.signal.aborted) break;
  const directory = path.join(run, kind);
  await mkdir(directory, { recursive: true });
  const database = `LiturgyDemo_${runId}_${kind}`;
  assert.match(database, /^LiturgyDemo_[a-f0-9]{16}_(marketing|workspace|api)$/);
  let databaseOwned = false;
  let service;
  let browser;
  let context;
  let serviceLog;
  let deadline;
  try {
    await sql(`CREATE DATABASE [${database}]`);
    databaseOwned = true;
    cancellation.signal.throwIfAborted();
    const port = await freePort();
    const base = `http://127.0.0.1:${port}`;
    serviceLog = createWriteStream(path.join(directory, 'service.log'));
    service = spawn(dotnet, ['Liturgy.Api.dll'], {
      cwd: app,
      windowsHide: true,
      stdio: ['ignore', 'pipe', 'pipe'],
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: 'Development',
        ASPNETCORE_URLS: base,
        ConnectionStrings__DefaultConnection: `Server=${sqlServer};Database=${database};Integrated Security=True;TrustServerCertificate=True`,
        Jwt__SigningKey: crypto.randomBytes(48).toString('base64'),
      },
    });
    service.stdout.pipe(serviceLog);
    service.stderr.pipe(serviceLog);
    let serviceError;
    service.on('error', (error) => {
      serviceError = error;
    });
    const readiness = Date.now() + 120000;
    while (true) {
      cancellation.signal.throwIfAborted();
      if (serviceError) throw serviceError;
      if (service.exitCode !== null)
        throw new Error(`API exited ${service.exitCode}; inspect ${directory}/service.log`);
      const response = await fetch(`${base}/health`, { signal: AbortSignal.timeout(2000) }).catch(
        () => null,
      );
      if (response?.ok) break;
      if (Date.now() > readiness) throw new Error('API readiness deadline exceeded');
      await pause(300);
    }
    async function api(method, route, body, token) {
      const response = await fetch(`${base}${route}`, {
        method,
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: body === undefined ? undefined : JSON.stringify(body),
        signal: AbortSignal.timeout(15000),
      });
      return { status: response.status, body: await response.json() };
    }
    const signedIn = await api('POST', '/api/auth/sign-in', {
      email: 'quinn@newhope.dev',
      password: 'Liturgy!2026',
    });
    assert.equal(signedIn.status, 200);
    const auth = signedIn.body.accessToken;
    browser = await chromium.launch({ headless: true });
    activeBrowser = browser;
    const options = { viewport: { width: 1280, height: 720 }, deviceScaleFactor: 1, baseURL: base };
    let storageState;
    if (kind === 'workspace') {
      const setup = await browser.newContext(options);
      try {
        const login = await setup.newPage();
        await login.goto(`${base}/sign-in`);
        await login.locator('#email').fill('quinn@newhope.dev');
        await login.locator('#password').fill('Liturgy!2026');
        await login.getByRole('button', { name: 'Sign in', exact: true }).click();
        await expect(login).toHaveURL(/\/dashboard$/);
        storageState = await setup.storageState();
      } finally {
        await setup.close();
      }
    }
    context = await browser.newContext({
      ...options,
      storageState,
      recordVideo: { dir: directory, size: options.viewport },
    });
    context.setDefaultTimeout(30000);
    context.setDefaultNavigationTimeout(30000);
    const page = await context.newPage();
    deadline = setTimeout(() => {
      void context?.close();
    }, 12 * 60000);
    const errors = [];
    page.on('pageerror', (e) => errors.push(e.message));
    await page.setContent(
      '<body style="margin:0;background:#16241f;color:#c6fb50;font:40px system-ui;display:grid;place-items:center;height:100vh">LITURGY<div id="calibrate" style="position:fixed;inset:0;background:#ff00ff"></div></body>',
    );
    await pause(650);
    await page.evaluate(() => document.querySelector('#calibrate').remove());
    const epoch = performance.now();
    const events = [];
    async function say(id) {
      const segment = narration[kind].find((s) => s[0] === id);
      assert.ok(segment, `Missing narration ${kind}/${id}`);
      const [, title, text] = segment;
      // A single sentence per caption avoids large blocks obscuring the product.
      const sentences = text.match(/[^.!?]+[.!?]+|[^.!?]+$/g).map((s) => s.trim());
      const seconds = durations.get(`${kind}-${id}`);
      const start = (performance.now() - epoch) / 1000;
      const event = { id, title, text, start, duration: seconds, captions: [] };
      console.log(`${kind}: ${title}`);
      for (const sentence of sentences) {
        const duration =
          (seconds * sentence.length) / sentences.reduce((total, s) => total + s.length, 0);
        await page.evaluate(
          ({ title, sentence }) => {
            document.querySelector('#demo-caption')?.remove();
            const overlay = document.createElement('aside');
            overlay.id = 'demo-caption';
            Object.assign(overlay.style, {
              position: 'fixed',
              bottom: '12px',
              left: '24px',
              right: '24px',
              zIndex: '2147483647',
              background: 'rgba(15,29,23,.97)',
              color: '#fff',
              borderLeft: '4px solid #c6fb50',
              padding: '12px 20px',
              font: '20px/1.35 system-ui',
              boxShadow: '0 4px 18px #0005',
              pointerEvents: 'none',
            });
            const heading = document.createElement('div');
            heading.textContent = title;
            Object.assign(heading.style, {
              color: '#c6fb50',
              font: '600 13px/1.5 system-ui',
              marginBottom: '3px',
              textTransform: 'uppercase',
              letterSpacing: '1px',
            });
            const caption = document.createElement('div');
            caption.textContent = sentence;
            overlay.append(heading, caption);
            document.body.append(overlay);
          },
          { title, sentence },
        );
        event.captions.push({
          text: sentence,
          start: (performance.now() - epoch) / 1000,
          duration,
        });
        await pause(duration * 1000);
      }
      events.push(event);
      await pause(500);
      await page.evaluate(() => document.querySelector('#demo-caption')?.remove());
    }
    console.log(`Recording ${kind} against a fresh database...`);
    await { marketing, workspace, api: apiStory }[kind]({ page, say, base, directory, api, auth });
    assert.deepEqual(errors, [], 'Unexpected browser errors');
    await pause(800);
    const video = page.video();
    await context.close();
    context = null;
    clearTimeout(deadline);
    const raw = path.join(directory, 'continuous.webm');
    await video.saveAs(raw);
    await browser.close();
    browser = null;
    // Locate the calibration prefix in encoded frames; remove setup only, never story scenes.
    const pixels = (
      await command(
        ffmpeg,
        [
          '-v',
          'error',
          '-i',
          raw,
          '-t',
          '4',
          '-vf',
          'fps=25,scale=1:1',
          '-pix_fmt',
          'rgb24',
          '-f',
          'rawvideo',
          '-',
        ],
        { encoding: 'buffer' },
      )
    ).stdout;
    let markerEnd = -1;
    for (let i = 0; i < pixels.length; i += 3) {
      if (pixels[i] > 220 && pixels[i + 1] < 35 && pixels[i + 2] > 220)
        markerEnd = (i / 3 + 1) / 25;
    }
    assert.ok(markerEnd > 0, 'Video calibration marker missing');
    await writeFile(
      path.join(directory, 'recording.json'),
      JSON.stringify({ kind, events, markerEnd, revision, versions, runId }, null, 2),
    );
    const staging = path.join(directory, 'staging');
    await mkdir(staging);
    const slug = `liturgy-${kind}`;
    const output = path.join(staging, `${slug}.webm`);
    const rawInfo = await probe(raw);
    const duration = Number(rawInfo.format.duration) - markerEnd;
    const inputs = events.flatMap((e) => ['-i', path.join(speech, `${kind}-${e.id}.wav`)]);
    const filters = events.map(
      (e, i) => `[${i + 1}:a]adelay=${Math.round(e.start * 1000)}:all=1[a${i}]`,
    );
    filters.push(
      `${events.map((_, i) => `[a${i}]`).join('')}amix=inputs=${events.length}:normalize=0,alimiter=limit=0.75:level=false,apad[audio]`,
    );
    await command(
      ffmpeg,
      [
        '-y',
        '-v',
        'error',
        '-ss',
        String(markerEnd),
        '-i',
        raw,
        ...inputs,
        '-filter_complex',
        filters.join(';'),
        '-map',
        '0:v:0',
        '-map',
        '[audio]',
        '-c:v',
        'libvpx',
        '-deadline',
        'realtime',
        '-b:v',
        '1800k',
        '-cpu-used',
        '8',
        '-c:a',
        'libopus',
        '-b:a',
        '96k',
        '-t',
        String(duration),
        output,
      ],
      { timeout: 300000 },
    );
    const info = await probe(output);
    assert.equal(info.streams.find((s) => s.codec_type === 'video').width, 1280);
    assert.equal(info.streams.find((s) => s.codec_type === 'video').height, 720);
    assert.equal(info.streams.find((s) => s.codec_type === 'audio').codec_name, 'opus');
    assert.ok(Number(info.format.duration) > events.at(-1).start + events.at(-1).duration);
    await command(ffmpeg, ['-v', 'error', '-i', output, '-f', 'null', '-'], { timeout: 120000 });
    await command(ffmpeg, [
      '-y',
      '-v',
      'error',
      '-ss',
      String(events[2].start + 1),
      '-i',
      output,
      '-frames:v',
      '1',
      path.join(staging, `${slug}-poster.png`),
    ]);
    const cues = events
      .flatMap((e) => e.captions)
      .map((c) => `${stamp(c.start)} --> ${stamp(c.start + c.duration)}\n${c.text}`)
      .join('\n\n');
    await writeFile(path.join(staging, `${slug}.vtt`), `WEBVTT\n\n${cues}\n`);
    await writeFile(
      path.join(staging, `${slug}-transcript.md`),
      `# ${slug}\n\nNarrator: Microsoft David Desktop (Windows SAPI), rate -1.\n\n` +
        events
          .map((e) => `## ${stamp(e.start).slice(0, 8)} — ${e.title}\n\n${e.text}`)
          .join('\n\n') +
        '\n',
    );
    await writeFile(
      path.join(staging, `${slug}-chapters.json`),
      JSON.stringify(
        {
          slug,
          revision,
          runId,
          versions,
          voice: 'Microsoft David Desktop',
          duration: Number(info.format.duration),
          width: 1280,
          height: 720,
          bytes: (await stat(output)).size,
          calibrationTrimSeconds: markerEnd,
          chapters: events,
          assertions: 'passed',
          review: 'pending encoded playback review',
        },
        null,
        2,
      ),
    );
    console.log(`Assertions and decode checks passed: ${output}`);
  } catch (error) {
    failures.push({ kind, error: error.message });
    await writeFile(path.join(directory, 'failure.txt'), error.stack ?? String(error));
    console.error(`${kind} blocked: ${error.message}`);
  } finally {
    clearTimeout(deadline);
    await context?.close().catch((e) => console.error(`Context cleanup: ${e.message}`));
    await browser?.close().catch((e) => console.error(`Browser cleanup: ${e.message}`));
    activeBrowser = null;
    if (service?.pid && service.exitCode === null) {
      try {
        await command('taskkill.exe', ['/PID', String(service.pid), '/T', '/F'], {
          timeout: 15000,
          signal: undefined,
        });
      } catch (error) {
        if (service.exitCode === null) {
          failures.push({ kind, error: `Process cleanup failed: ${service.pid}` });
          console.error(error.message);
        }
      }
    }
    serviceLog?.end();
    if (databaseOwned) {
      try {
        await sql(
          `ALTER DATABASE [${database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${database}]`,
        );
      } catch (error) {
        failures.push({ kind, error: `Database cleanup failed: ${database}` });
        console.error(error.message);
      }
    }
  }
}
await writeFile(
  path.join(run, 'result.json'),
  JSON.stringify({ runId, selection, failures }, null, 2),
);
console.log(`Recordings staged for review in ${run}`);
if (failures.length) process.exitCode = 1;
