// Decode and play staged videos at normal speed. Promotion remains a separate step
// so chapter images and the encoded playback can be inspected before delivery.
import { chromium } from '@playwright/test';
import { createServer } from 'node:http';
import { createReadStream } from 'node:fs';
import { readFile, writeFile, mkdir, stat } from 'node:fs/promises';
import path from 'node:path';
import assert from 'node:assert/strict';
import crypto from 'node:crypto';

const run = path.resolve(process.argv[2] ?? '');
const selection = process.argv[3] ? [process.argv[3]] : ['marketing', 'workspace', 'api'];
assert.match(path.basename(run), /^[a-f0-9]{16}$/);
for (const kind of selection) {
  assert.ok(['marketing', 'workspace', 'api'].includes(kind));
  const staging = path.join(run, kind, 'staging');
  const slug = `liturgy-${kind}`;
  const movie = path.join(staging, `${slug}.webm`);
  const chapterFile = path.join(staging, `${slug}-chapters.json`);
  const metadata = JSON.parse(await readFile(chapterFile));
  const size = (await stat(movie)).size;
  const review = path.join(run, kind, 'review');
  await mkdir(review, { recursive: true });
  const server = createServer((req, res) => {
    if (req.url === '/video.webm') {
      const range = /^bytes=(\d+)-(\d*)$/.exec(req.headers.range ?? '');
      const start = range ? Number(range[1]) : 0;
      const end = range && range[2] ? Math.min(Number(range[2]), size - 1) : size - 1;
      if (start > end || start >= size) {
        res.writeHead(416);
        res.end();
        return;
      }
      res.writeHead(range ? 206 : 200, {
        'Content-Type': 'video/webm',
        'Content-Length': end - start + 1,
        'Accept-Ranges': 'bytes',
        ...(range ? { 'Content-Range': `bytes ${start}-${end}/${size}` } : {}),
      });
      createReadStream(movie, { start, end }).pipe(res);
    } else if (req.url === '/') {
      res.writeHead(200, { 'Content-Type': 'text/html' });
      res.end(
        '<body style="margin:0;background:#000"><video width="1280" height="720" src="/video.webm" playsinline></video></body>',
      );
    } else {
      res.writeHead(404);
      res.end();
    }
  });
  let browser;
  try {
    await new Promise((resolve) => server.listen(0, '127.0.0.1', resolve));
    browser = await chromium.launch({
      headless: true,
      args: ['--autoplay-policy=no-user-gesture-required'],
    });
    const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
    await page.goto(`http://127.0.0.1:${server.address().port}`);
    await page.waitForFunction(() => document.querySelector('video').readyState >= 2);
    const media = await page.evaluate(() => {
      const v = document.querySelector('video');
      return { width: v.videoWidth, height: v.videoHeight, duration: v.duration };
    });
    assert.equal(media.width, 1280);
    assert.equal(media.height, 720);
    assert.ok(Math.abs(media.duration - metadata.duration) < 0.5);
    await page.evaluate(() => document.querySelector('video').play());
    console.log(`Playing ${slug} at 1x for ${media.duration.toFixed(1)} seconds...`);
    const began = Date.now();
    let last = -1;
    let stationary = 0;
    let chapter = 0;
    const samples = [];
    while (true) {
      const state = await page.evaluate(() => {
        const v = document.querySelector('video');
        return {
          time: v.currentTime,
          ended: v.ended,
          error: v.error?.message,
          rate: v.playbackRate,
          muted: v.muted,
          frames: v.getVideoPlaybackQuality().totalVideoFrames,
          dropped: v.getVideoPlaybackQuality().droppedVideoFrames,
        };
      });
      assert.equal(state.error, undefined);
      assert.equal(state.rate, 1);
      assert.equal(state.muted, false);
      stationary = state.time === last ? stationary + 1 : 0;
      assert.ok(stationary < 10, 'Playback stalled for ten seconds');
      last = state.time;
      if (
        chapter < metadata.chapters.length &&
        state.time >= metadata.chapters[chapter].start + 2
      ) {
        const file = `${String(chapter).padStart(2, '0')}-${metadata.chapters[chapter].id}.png`;
        await page.screenshot({ path: path.join(review, file) });
        samples.push({ chapter: metadata.chapters[chapter].title, time: state.time, file });
        chapter++;
      }
      if (state.ended) {
        assert.ok(state.frames > media.duration * 15);
        assert.ok((Date.now() - began) / 1000 >= media.duration - 1);
        await page.screenshot({ path: path.join(review, 'ending.png') });
        const sha256 = crypto
          .createHash('sha256')
          .update(await readFile(movie))
          .digest('hex');
        await writeFile(
          path.join(review, 'playback.json'),
          JSON.stringify(
            { media, state, elapsedSeconds: (Date.now() - began) / 1000, samples, sha256 },
            null,
            2,
          ),
        );
        break;
      }
      assert.ok(Date.now() - began < (media.duration + 45) * 1000, 'Playback deadline exceeded');
      await new Promise((resolve) => setTimeout(resolve, 1000));
    }
    assert.equal(chapter, metadata.chapters.length);
    metadata.review =
      'Normal-speed encoded playback passed; chapter images await visual inspection';
    await writeFile(chapterFile, JSON.stringify(metadata, null, 2));
    console.log(`Playback passed: ${review}`);
  } finally {
    await browser?.close();
    await new Promise((resolve) => server.close(resolve));
  }
}
