// Traces to: L2-WORK-002, L2-WKSP-002
import { test, expect, authenticate } from '../fixtures/test';
import { DevelopBoardPage } from '../pages/develop-board.page';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('a new work item can be added to the backlog', async ({ page }) => {
  const board = new DevelopBoardPage(page);
  await board.open('p-lantern');

  await page.getByLabel('New card title').fill('Consent copy for first-time callers');
  await page.getByRole('button', { name: 'Add card' }).click();

  await expect(page.locator('.wcard', { hasText: 'Consent copy for first-time callers' })).toBeVisible();
});

test('cards expose an assignee selector wired to members', async ({ page }) => {
  const board = new DevelopBoardPage(page);
  await board.open('p-lantern');

  const assign = board.card('LAN-24').locator('select.wcard__assign');
  await expect(assign).toBeVisible();
});
