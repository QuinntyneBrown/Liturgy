// Traces to: L2-PLAY-010, L2-PLAY-011
import { test, expect, authenticate } from '../fixtures/test';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('choosing a discernment path marks exactly one choice', async ({ page }) => {
  await page.goto('/discern/p-lantern');

  const choices = page.locator('.choice');
  await expect(choices).toHaveCount(4);

  const reimagine = choices.filter({ hasText: 'Reimagine' });
  await reimagine.click();
  await expect(reimagine).toHaveAttribute('aria-pressed', 'true');
  await expect(page.locator('.choice.is-chosen')).toHaveCount(1);
});
