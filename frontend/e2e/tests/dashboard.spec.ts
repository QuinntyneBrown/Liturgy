// Traces to: L2-WKSP-005
import { test, expect, authenticate } from '../fixtures/test';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('the dashboard surfaces momentum, attention, and the 4D board', async ({ page }) => {
  await page.goto('/dashboard');

  await expect(page.getByRole('heading', { name: /Good morning/ })).toBeVisible();
  await expect(page.locator('.stat', { hasText: 'Active projects' })).toBeVisible();
  await expect(page.locator('.stat', { hasText: 'Gates blocked' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Gates that need attention' })).toBeVisible();
  await expect(page.locator('.attn').first()).toContainText('Lantern');
  await expect(page.locator('.pcard', { hasText: 'Lantern' })).toBeVisible();
});
