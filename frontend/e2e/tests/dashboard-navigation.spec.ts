// Traces to: L2-UX-011
import { test, expect, authenticate } from '../fixtures/test';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('every authenticated screen links back to the dashboard from the topbar', async ({ page }) => {
  const routes = [
    '/projects',
    '/members',
    '/projects/p-lantern',
    '/discern/p-lantern',
    '/board/p-lantern',
    '/loop/card-24',
    '/demonstrate/p-lantern',
  ];

  for (const route of routes) {
    await page.goto(route);

    const workspaceLink = page.locator('.topbar__crumbs').getByRole('link', { name: 'Workspace' });
    await expect(workspaceLink).toBeVisible();
    await expect(workspaceLink).toHaveAttribute('href', '/dashboard');

    await workspaceLink.click();
    await expect(page).toHaveURL(/\/dashboard$/);
  }
});

test('the desktop project rail links back to the dashboard', async ({ page }) => {
  await page.goto('/projects/p-lantern');

  const dashboardLink = page
    .getByRole('navigation', { name: 'Project rhythm' })
    .getByRole('link', { name: /Dashboard/ });
  await expect(dashboardLink).toBeVisible();

  await dashboardLink.click();
  await expect(page).toHaveURL(/\/dashboard$/);
});

test('the dashboard link remains available on a narrow project screen', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto('/board/p-lantern');

  await expect(page.locator('.rail__foot')).toBeHidden();
  const workspaceLink = page.locator('.topbar__crumbs').getByRole('link', { name: 'Workspace' });
  await expect(workspaceLink).toBeVisible();

  await workspaceLink.click();
  await expect(page).toHaveURL(/\/dashboard$/);
});
