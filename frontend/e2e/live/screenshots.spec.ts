import { test } from '@playwright/test';
import { SignInPage } from '../pages/sign-in.page';

/**
 * Captures full-page screenshots of the key screens against the REAL backend, for a
 * visual fidelity pass. Opt-in via LIVE=1 with the API running on :5099.
 */
const DIR = 'test-results/shots';

test.describe('live screenshots', () => {
  test.skip(!process.env['LIVE'], 'set LIVE=1 and run the API');

  test('capture the key screens', async ({ page }) => {
    await page.setViewportSize({ width: 1320, height: 900 });

    const signIn = new SignInPage(page);
    await signIn.open();
    await signIn.signIn('quinn@newhope.dev', 'Liturgy!2026');
    await page.waitForURL(/\/dashboard$/);
    await page.waitForTimeout(900);
    await page.screenshot({ path: `${DIR}/01-dashboard.png`, fullPage: true });

    await page.goto('/projects');
    await page.locator('.project-tile', { hasText: 'Wellspring' }).click();
    await page.waitForURL(/\/projects\//);
    await page.getByRole('link', { name: /Open Discern/ }).click();
    await page.waitForURL(/\/discern\//);
    await page.waitForTimeout(700);
    await page.screenshot({ path: `${DIR}/02-discern.png`, fullPage: true });

    await page.goto('/projects');
    await page.locator('.project-tile', { hasText: 'Bread' }).click();
    await page.waitForURL(/\/projects\//);
    await page.getByRole('link', { name: /Open Impact/ }).click();
    await page.waitForURL(/\/demonstrate\//);
    await page.waitForTimeout(700);
    await page.screenshot({ path: `${DIR}/03-demonstrate.png`, fullPage: true });

    await page.goto('/projects');
    await page.locator('.project-tile', { hasText: 'Lantern' }).click();
    await page.waitForURL(/\/projects\//);
    await page.getByRole('link', { name: /Open board/ }).click();
    await page.waitForURL(/\/board\//);
    await page.waitForTimeout(900);
    await page.screenshot({ path: `${DIR}/04-board.png`, fullPage: true });

    // The marketing splash at "/" is a static page served by the API from wwwroot
    // (see docs/marketing-deployment.md) — it never renders through ng serve, so it
    // is not captured here. View it via the published wwwroot or marketing/index.html.
  });
});
