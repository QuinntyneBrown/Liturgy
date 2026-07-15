import { test, expect, authenticate } from '../fixtures/test';
import { LoopPage } from '../pages/loop.page';
import { DevelopBoardPage } from '../pages/develop-board.page';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('the Done column is locked behind the 5R loop', async ({ page }) => {
  const board = new DevelopBoardPage(page);
  await board.open('p-lantern');

  await expect(board.column('Done')).toBeVisible();
  await expect(board.lockedNote()).toBeVisible();
  await expect(board.card('LAN-24')).toBeVisible();
});

test('Mark done stays disabled until every movement is logged, then completes the card', async ({ page }) => {
  const loop = new LoopPage(page);
  await loop.open('card-24');

  // Card starts with 3 of 5 logged.
  await expect(loop.dialCount()).toHaveText('3/5');
  await expect(loop.markDoneButton()).toBeDisabled();

  // Log Render, then Rejoice.
  await loop.logCurrent();
  await expect(loop.dialCount()).toHaveText('4/5');
  await expect(loop.markDoneButton()).toBeDisabled();

  await loop.logCurrent();
  await expect(loop.dialCount()).toHaveText('5/5');
  await expect(loop.markDoneButton()).toBeEnabled();

  // Completing returns to the board.
  await loop.markDoneButton().click();
  await expect(page).toHaveURL(/\/board\/p-lantern$/);
});
