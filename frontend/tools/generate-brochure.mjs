/**
 * Renders docs/mocks/brochure/liturgy-overview.html to the downloadable PDF
 * at marketing/assets/liturgy-overview.pdf using headless Chromium.
 *
 * Run from frontend/:  npm run brochure:pdf
 * (Chromium must be installed once: npx playwright install chromium)
 * Google Fonts load over the network — generate while online so the
 * brand webfonts embed instead of falling back to Arial.
 */
import { chromium } from 'playwright';
import { fileURLToPath, pathToFileURL } from 'node:url';
import path from 'node:path';

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const src = pathToFileURL(path.join(repoRoot, 'docs', 'mocks', 'brochure', 'liturgy-overview.html')).href;
const out = path.join(repoRoot, 'marketing', 'assets', 'liturgy-overview.pdf');

const browser = await chromium.launch();
try {
  const page = await browser.newPage();
  await page.goto(src, { waitUntil: 'networkidle' });
  await page.evaluate(() => document.fonts.ready);
  await page.pdf({ path: out, format: 'Letter', printBackground: true, preferCSSPageSize: true });
  console.log(`Wrote ${out}`);
} finally {
  await browser.close();
}
