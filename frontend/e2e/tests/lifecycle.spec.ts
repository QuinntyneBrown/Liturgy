// Traces to: L2-WORK-015, L2-WORK-016, L2-WORK-020, L2-WKSP-007, L2-WKSP-008, L2-WKSP-010
import { test, expect, authenticate } from '../fixtures/test';
import { DevelopBoardPage } from '../pages/develop-board.page';
import { ProjectsPage } from '../pages/projects.page';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('a card can be pointed and then cancelled off the board', async ({ page }) => {
  const board = new DevelopBoardPage(page);
  await board.open('p-lantern');

  const card = board.card('LAN-33');
  await expect(card).toBeVisible();

  // Open the card's action menu and set story points (blur commits the change event).
  await card.getByLabel('Card actions').click();
  await card.getByLabel('Story points').fill('8');
  await card.getByLabel('Story points').blur();
  await expect(card.getByText('8 pt')).toBeVisible();

  // Cancel it (the menu is still open) — the card leaves the active board.
  await card.getByRole('menuitem', { name: 'Cancel' }).click();
  await expect(board.card('LAN-33')).toHaveCount(0);
});

test('a project can be closed and revealed with the closed filter', async ({ page }) => {
  const projects = new ProjectsPage(page);
  await projects.open();

  await expect(projects.project('Lantern')).toBeVisible();
  await projects.project('Lantern').getByRole('button', { name: 'Close' }).click();

  // Hidden from the default list…
  await expect(projects.project('Lantern')).toHaveCount(0);

  // …and revealed (with a Reopen action) once closed projects are shown.
  await page.getByRole('button', { name: 'Show closed' }).click();
  await expect(projects.project('Lantern')).toBeVisible();
  await expect(projects.project('Lantern').getByRole('button', { name: 'Reopen' })).toBeVisible();
});
