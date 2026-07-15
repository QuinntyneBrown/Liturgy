// Traces to: L2-UX-005, L2-UX-008, L2-UX-009
import { test, expect, authenticate } from '../fixtures/test';
import { LoopPage } from '../pages/loop.page';

test('the sign-in page exposes a skip-to-content link', async ({ page }) => {
  await page.goto('/sign-in');
  await expect(page.locator('a.skip-link')).toHaveAttribute('href', '#main');
});

test.describe('authenticated', () => {
  test.beforeEach(async ({ page }) => {
    await authenticate(page);
  });

  test('a locked Mark done control is aria-disabled', async ({ page }) => {
    const loop = new LoopPage(page);
    await loop.open('card-24'); // 3 of 5 logged → locked
    await expect(loop.markDoneButton()).toHaveAttribute('aria-disabled', 'true');
  });
});
