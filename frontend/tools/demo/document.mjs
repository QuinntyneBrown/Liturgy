import { readFile, writeFile, readdir } from 'node:fs/promises';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');
const directory = path.join(root, 'docs/demo');
const sourceDirectory = path.join(root, 'frontend/tools/demo');
const sources = {};
for (const file of (await readdir(sourceDirectory))
  .filter((file) => /\.(mjs|ps1)$/.test(file))
  .sort()) {
  sources[`frontend/tools/demo/${file}`] = crypto
    .createHash('sha256')
    .update(await readFile(path.join(sourceDirectory, file)))
    .digest('hex');
}
await writeFile(
  path.join(directory, 'recording-source-manifest.json'),
  JSON.stringify(
    {
      description:
        'SHA256 hashes of the delivered rerun recipe; application revisions are recorded separately per video. These are not historical capture-script snapshots.',
      sources,
    },
    null,
    2,
  ),
);
const records = [];
const sections = [];
const clock = (seconds) =>
  `${Math.floor(seconds / 60)}:${String(Math.floor(seconds % 60)).padStart(2, '0')}`;
for (const kind of ['marketing', 'workspace', 'api']) {
  const slug = `liturgy-${kind}`;
  const metadata = await readFile(path.join(directory, `${slug}-chapters.json`), 'utf8')
    .then(JSON.parse)
    .catch(() => null);
  if (!metadata) {
    records.push(`| ${kind} | Pending / blocked; no reviewed video delivered | — |`);
    continue;
  }
  records.push(
    `| [${kind}](${slug}.webm) | Recorded and reviewed · ${clock(metadata.duration)} · ${metadata.width} × ${metadata.height} · ${(metadata.bytes / 1024 / 1024).toFixed(2)} MiB | [Poster](${slug}-poster.png) · [Transcript](${slug}-transcript.md) · [Captions](${slug}.vtt) |`,
  );
  sections.push(
    `### ${kind}\n\nApplication revision: \`${metadata.revision}\`. Run: \`${metadata.runId}\`.\n\n` +
      metadata.chapters.map((c) => `- **${clock(c.start)}** ${c.title}`).join('\n'),
  );
}
const readme = path.join(directory, 'README.md');
const text = await readFile(readme, 'utf8');
const generated = `| Application | Status and measured media | Supporting files |\n| --- | --- | --- |\n${records.join('\n')}\n\n${sections.join('\n\n')}`;
await writeFile(
  readme,
  text.replace(
    /<!-- generated-demo-index:start -->[\s\S]*?<!-- generated-demo-index:end -->/,
    `<!-- generated-demo-index:start -->\n${generated}\n<!-- generated-demo-index:end -->`,
  ),
);
const cards = [];
for (const kind of ['marketing', 'workspace', 'api']) {
  const slug = `liturgy-${kind}`;
  if (!(await readFile(path.join(directory, `${slug}-chapters.json`)).catch(() => null))) continue;
  cards.push(
    `<section><h2>${{ marketing: 'The public introduction', workspace: 'The team workspace', api: 'The API and its rules' }[kind]}</h2><video controls preload="metadata" poster="${slug}-poster.png" src="${slug}.webm"><track kind="captions" srclang="en" label="English" src="${slug}.vtt"></video><p><a href="${slug}.webm" download>Download video</a> · <a href="${slug}-transcript.md">Read transcript</a></p></section>`,
  );
}
await writeFile(
  path.join(directory, 'index.html'),
  `<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Liturgy — narrated demos</title><style>body{margin:0;background:#14271e;color:#f2f3e8;font:18px/1.6 system-ui}main{max-width:1100px;margin:auto;padding:40px 24px}h1{font-size:42px;margin:0}h2{font-weight:500}a{color:#c6fb50}video{width:100%;border:1px solid #425d4b;border-radius:8px}section{margin:48px 0}p{color:#c6d1c9}.eyebrow{color:#c6fb50;letter-spacing:3px;font-size:14px}</style></head><body><main><div class="eyebrow">LITURGY / DEMONSTRATIONS</div><h1>Build in rhythm.</h1><p>Real application workflows, with warm Christian narration using Microsoft David on Windows.</p><p><a href="README.md">Recording details and rerun instructions</a></p>${cards.join('')}</main></body></html>`,
);
console.log('Updated the demo index and video gallery.');
