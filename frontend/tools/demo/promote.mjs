import { readFile, writeFile, mkdir, cp, rename, rm, stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import assert from 'node:assert/strict';
import crypto from 'node:crypto';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');
const run = path.resolve(process.argv[2] ?? '');
const kind = process.argv[3];
assert.match(path.basename(run), /^[a-f0-9]{16}$/);
assert.ok(['marketing', 'workspace', 'api'].includes(kind));
assert.ok(
  process.argv.includes('--visuals-reviewed'),
  'Inspect chapter images and playback before passing --visuals-reviewed',
);
const slug = `liturgy-${kind}`;
const staging = path.join(run, kind, 'staging');
const chapters = JSON.parse(await readFile(path.join(staging, `${slug}-chapters.json`)));
const result = JSON.parse(await readFile(path.join(run, 'result.json')));
assert.ok(
  !result.failures.some((failure) => failure.kind === kind),
  'The selected take has recording or cleanup failures',
);
const review = JSON.parse(await readFile(path.join(run, kind, 'review/playback.json')));
assert.equal(chapters.assertions, 'passed');
assert.equal(review.state.ended, true);
assert.equal(review.samples.length, chapters.chapters.length);
assert.equal(
  review.sha256,
  crypto
    .createHash('sha256')
    .update(await readFile(path.join(staging, `${slug}.webm`)))
    .digest('hex'),
);
// Verify the exact reviewed file again, including any replacement narration mix.
const ffmpeg =
  process.env.LITURGY_DEMO_FFMPEG ??
  path.join(root, 'artifacts/demo-tools/ffmpeg/ffmpeg-9.0.1-essentials_build/bin/ffmpeg.exe');
const ffprobe = process.env.LITURGY_DEMO_FFPROBE ?? path.join(path.dirname(ffmpeg), 'ffprobe.exe');
const execute = promisify(execFile);
const movie = path.join(staging, `${slug}.webm`);
const measured = JSON.parse(
  (
    await execute(ffprobe, ['-v', 'error', '-show_format', '-show_streams', '-of', 'json', movie], {
      windowsHide: true,
      timeout: 30000,
    })
  ).stdout,
);
assert.equal(measured.streams.find((s) => s.codec_type === 'video').width, 1280);
assert.equal(measured.streams.find((s) => s.codec_type === 'video').height, 720);
assert.equal(measured.streams.find((s) => s.codec_type === 'audio').codec_name, 'opus');
await execute(ffmpeg, ['-nostdin', '-v', 'error', '-i', movie, '-f', 'null', '-'], {
  windowsHide: true,
  timeout: 120000,
});
const volume = (
  await execute(
    ffmpeg,
    ['-nostdin', '-hide_banner', '-i', movie, '-vn', '-af', 'volumedetect', '-f', 'null', '-'],
    { windowsHide: true, timeout: 30000 },
  )
).stderr;
const peak = Number(/max_volume: ([-\d.]+) dB/.exec(volume)?.[1]);
assert.ok(
  Number.isFinite(peak) && peak > -40 && peak < -0.1,
  'Narration must be audible with headroom below clipping',
);
chapters.duration = Number(measured.format.duration);
chapters.bytes = Number(measured.format.size);
chapters.audioPeakDb = peak;
chapters.review =
  'Assertions, full decode, normal-speed browser playback, and visual chapter inspection passed';
await writeFile(path.join(staging, `${slug}-chapters.json`), JSON.stringify(chapters, null, 2));
const destination = path.join(root, 'docs/demo');
const backup = path.join(run, kind, 'previous-deliverables');
await mkdir(destination, { recursive: true });
await mkdir(backup, { recursive: true });
const names = [
  `${slug}.webm`,
  `${slug}-poster.png`,
  `${slug}.vtt`,
  `${slug}-transcript.md`,
  `${slug}-chapters.json`,
];
const previous = [];
const promoted = [];
let temporary;
try {
  for (const name of names) {
    const target = path.join(destination, name);
    if (
      await stat(target).catch((error) => {
        if (error.code === 'ENOENT') return null;
        throw error;
      })
    ) {
      await cp(target, path.join(backup, name));
      previous.push(name);
    }
    temporary = path.join(destination, `.${name}.${path.basename(run)}.tmp`);
    await cp(path.join(staging, name), temporary);
    await rename(temporary, target);
    temporary = undefined;
    promoted.push(name);
  }
} catch (error) {
  if (temporary) await rm(temporary, { force: true });
  for (const name of promoted) {
    if (previous.includes(name)) await cp(path.join(backup, name), path.join(destination, name));
    else await rm(path.join(destination, name));
  }
  throw error;
}
console.log(`Promoted verified ${slug} artifacts to ${destination}`);
