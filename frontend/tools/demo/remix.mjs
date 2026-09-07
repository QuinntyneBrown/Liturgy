// Rebuild narration from original PCM without cutting or re-encoding the footage.
// Any changed mix invalidates playback review and must be reviewed again.
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { readFile, writeFile, stat, rename } from 'node:fs/promises';
import path from 'node:path';
import assert from 'node:assert/strict';

const run = path.resolve(process.argv[2] ?? '');
const kind = process.argv[3];
assert.match(path.basename(run), /^[a-f0-9]{16}$/);
assert.ok(['marketing', 'workspace', 'api'].includes(kind));
const ffmpeg = process.env.LITURGY_DEMO_FFMPEG ?? 'ffmpeg';
const ffprobe =
  process.env.LITURGY_DEMO_FFPROBE ??
  (path.isAbsolute(ffmpeg) ? path.join(path.dirname(ffmpeg), 'ffprobe.exe') : 'ffprobe');
const execute = promisify(execFile);
const staging = path.join(run, kind, 'staging');
const slug = `liturgy-${kind}`;
const chapterFile = path.join(staging, `${slug}-chapters.json`);
const metadata = JSON.parse(await readFile(chapterFile));
const video = path.join(staging, `${slug}.webm`);
const temporary = path.join(staging, `${slug}-remix.webm`);
const inputs = metadata.chapters.flatMap((c) => [
  '-i',
  path.join(run, 'speech', `${kind}-${c.id}.wav`),
]);
const filters = metadata.chapters.map(
  (c, i) => `[${i + 1}:a]adelay=${Math.round(c.start * 1000)}:all=1[a${i}]`,
);
filters.push(
  `${metadata.chapters.map((_, i) => `[a${i}]`).join('')}amix=inputs=${metadata.chapters.length}:normalize=0,alimiter=limit=0.75:level=false,apad[audio]`,
);
await execute(
  ffmpeg,
  [
    '-y',
    '-v',
    'error',
    '-i',
    video,
    ...inputs,
    '-filter_complex',
    filters.join(';'),
    '-map',
    '0:v:0',
    '-map',
    '[audio]',
    '-c:v',
    'copy',
    '-c:a',
    'libopus',
    '-b:a',
    '96k',
    '-t',
    String(metadata.duration),
    temporary,
  ],
  { windowsHide: true, timeout: 120000 },
);
await rename(temporary, video);
metadata.bytes = (await stat(video)).size;
const measured = JSON.parse(
  (
    await execute(ffprobe, ['-v', 'error', '-show_format', '-of', 'json', video], {
      windowsHide: true,
      timeout: 30000,
    })
  ).stdout,
);
metadata.duration = Number(measured.format.duration);
metadata.audioMix = 'Original Windows SAPI PCM; limiter at 0.75 with automatic gain disabled';
metadata.review = 'Audio remixed; encoded playback review required';
await writeFile(chapterFile, JSON.stringify(metadata, null, 2));
console.log(`Remixed ${slug}; rerun review.mjs before promotion.`);
