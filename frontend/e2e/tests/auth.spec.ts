import { test, expect } from '../fixtures/test';
import { SignInPage } from '../pages/sign-in.page';
import { ProjectsPage } from '../pages/projects.page';

test('signing in lands on the projects list', async ({ page }) => {
  const signIn = new SignInPage(page);
  await signIn.open();
  await signIn.signIn('quinn@newhope.dev', 'Liturgy!2026');

  await expect(page).toHaveURL(/\/projects$/);
  const projects = new ProjectsPage(page);
  await expect(projects.project('Lantern')).toBeVisible();
});

test('an unauthenticated visit redirects to sign-in', async ({ page }) => {
  await page.goto('/projects');
  await expect(page).toHaveURL(/\/sign-in$/);
});
