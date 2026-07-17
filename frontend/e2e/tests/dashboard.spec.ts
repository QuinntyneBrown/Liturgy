// Traces to: L2-WKSP-005
import { test, expect, authenticate } from '../fixtures/test';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('the dashboard surfaces momentum, attention, and the 4D board', async ({ page }) => {
  await page.clock.setFixedTime(new Date('2026-07-16T09:00:00'));
  await page.goto('/dashboard');

  await expect(page.getByRole('heading', { name: /Good morning/ })).toBeVisible();
  await expect(page.locator('.stat', { hasText: 'Active projects' })).toBeVisible();
  await expect(page.locator('.stat', { hasText: 'Gates blocked' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Gates that need attention' })).toBeVisible();
  await expect(page.locator('.attn').first()).toContainText('Lantern');
  await expect(page.locator('.pcard', { hasText: 'Lantern' })).toBeVisible();
});

const greetings: Array<[string, string]> = [
  ['09:00', 'Good morning'],
  ['14:30', 'Good afternoon'],
  ['21:00', 'Good evening'],
  ['02:00', 'Good evening'],
];

for (const [time, expected] of greetings) {
  test(`the greeting at ${time} is "${expected}"`, async ({ page }) => {
    await page.clock.setFixedTime(new Date(`2026-07-16T${time}:00`));
    await page.goto('/dashboard');

    await expect(page.getByRole('heading', { name: `${expected}, Quinn.` })).toBeVisible();
  });
}
