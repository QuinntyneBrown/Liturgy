// Traces to: L2-PLAY-005, L2-PLAY-006, L2-WORK-012, L2-WORK-013
import { test, expect } from '@playwright/test';
import { SignInPage } from '../pages/sign-in.page';
import { ProjectsPage } from '../pages/projects.page';
import { ProjectJourneyPage } from '../pages/project-journey.page';
import { LoopPage } from '../pages/loop.page';

/**
 * End-to-end against the REAL backend (no faked network). Opt-in via LIVE=1 so the
 * default suite (faked backend) stays hermetic. Requires the API running on :5099
 * with the Lantern demo seeded, and mutates that dev database.
 */
test.describe('live', () => {
  test.skip(!process.env['LIVE'], 'set LIVE=1 and run the API to exercise the real backend');

  test('sign in, unlock a gate, and complete a 5R loop against the real API', async ({ page }) => {
    const signIn = new SignInPage(page);
    await signIn.open();
    await signIn.signIn('quinn@newhope.dev', 'Liturgy!2026');

    await expect(page).toHaveURL(/\/dashboard$/);
    const projects = new ProjectsPage(page);
    await projects.open();
    await projects.openProject('Lantern');

    const journey = new ProjectJourneyPage(page);
    await expect(page).toHaveURL(/\/projects\//);
    const gate = journey.gate('Develop → Demonstrate');
    await expect(gate).toBeVisible();

    // Toggle the outstanding requirement → gate opens (server-recomputed).
    await journey.requirementCheckbox('Demo prepared for the community').click();
    await expect(gate.locator('.badge--done')).toHaveText('Open');

    // Into the board, open a card, and drive its 5R loop to completion.
    await page.getByRole('link', { name: /Open board/ }).click();
    await expect(page).toHaveURL(/\/board\//);
    await page.locator('.wcard', { hasText: 'LAN-24' }).locator('.wcard__title').click();

    const loop = new LoopPage(page);
    await expect(page).toHaveURL(/\/loop\//);
    await expect(loop.markDoneButton()).toBeDisabled();

    // LAN-24 seeds with 3 of 5 logged; log the remaining two.
    for (let i = 0; i < 2; i++) {
      await loop.logCurrent();
    }
    await expect(loop.dialCount()).toHaveText('5/5');
    await expect(loop.markDoneButton()).toBeEnabled();

    await loop.markDoneButton().click();
    await expect(page).toHaveURL(/\/board\//);
  });
});
