// Traces to: L2-WORK-014, L2-WORK-015, L2-WORK-019, L2-WKSP-011, L2-WKSP-012, L2-WKSP-015
import { test, expect } from '@playwright/test';
import { SignInPage } from '../pages/sign-in.page';

/**
 * End-to-end against the REAL backend (no faked network). Opt-in via LIVE=1. Requires the
 * API running on :5099 with the Lantern demo seeded. Self-cleaning: every card/invitation it
 * creates is deleted or revoked before the test ends, so the demo database is left pristine.
 */
test.describe('live lifecycle & invitations', () => {
  test.skip(!process.env['LIVE'], 'set LIVE=1 and run the API to exercise the real backend');

  test('card description/points render; create+point+delete a card; invite+revoke a member', async ({ page }) => {
    page.on('dialog', (d) => d.accept()); // auto-accept the delete confirmation

    const signIn = new SignInPage(page);
    await signIn.open();
    await signIn.signIn('quinn@newhope.dev', 'Liturgy!2026');
    await expect(page).toHaveURL(/\/dashboard$/);

    // --- Board: navigate to Lantern's Develop board ---
    await page.goto('/projects');
    await page.locator('.project-tile', { hasText: 'Lantern' }).getByRole('link').first().click();
    await expect(page).toHaveURL(/\/projects\//);
    await page.getByRole('link', { name: /Open board/ }).click();
    await expect(page).toHaveURL(/\/board\//);

    // Seeded card renders the new fields from the real backend.
    const seeded = page.locator('.wcard', { hasText: 'LAN-24' });
    await expect(seeded.getByText('5 pt')).toBeVisible();
    await expect(seeded.locator('.wcard__desc')).toContainText('escalate');

    // Create a card with a description, point it, then delete it (cleanup).
    const title = `E2E verify ${Date.now()}`;
    await page.getByLabel('New card title').fill(title);
    await page.getByLabel('New card description').fill('A described work item.');
    await page.getByRole('button', { name: 'Add card' }).click();

    const created = page.locator('.wcard', { hasText: title });
    await expect(created).toBeVisible();
    await expect(created.locator('.wcard__desc')).toHaveText('A described work item.');

    await created.getByLabel('Card actions').click();
    await created.getByLabel('Story points').fill('8');
    await created.getByLabel('Story points').blur();
    await expect(created.getByText('8 pt')).toBeVisible();

    await created.getByRole('menuitem', { name: 'Delete' }).click();
    await expect(page.locator('.wcard', { hasText: title })).toHaveCount(0);

    // --- Members: invite an email, see it pending, then revoke it (cleanup) ---
    await page.goto('/members');
    const email = `invitee-${Date.now()}@example.com`;
    await page.getByLabel('Invitee email').fill(email);
    await page.getByRole('button', { name: 'Send invite' }).click();

    const pendingRow = page.locator('.member', { hasText: email });
    await expect(pendingRow).toBeVisible();
    await expect(page.getByText(`Share this link with ${email}`)).toBeVisible();

    await pendingRow.getByRole('button', { name: 'Revoke' }).click();
    await expect(page.locator('.member', { hasText: email })).toHaveCount(0);
  });
});
