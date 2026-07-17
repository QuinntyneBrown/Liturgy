// Traces to: L2-AUTH-011, L2-AUTH-012, L2-WKSP-005
import { test, expect } from '../fixtures/test';
import { SignInPage } from '../pages/sign-in.page';

test('signing in lands on the dashboard', async ({ page }) => {
  const signIn = new SignInPage(page);
  await signIn.open();
  await signIn.signIn('quinn@newhope.dev', 'Liturgy!2026');

  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole('heading', { name: /Good (morning|afternoon|evening)/ })).toBeVisible();
});

test('an unauthenticated visit redirects to sign-in', async ({ page }) => {
  await page.goto('/projects');
  await expect(page).toHaveURL(/\/sign-in$/);
});
