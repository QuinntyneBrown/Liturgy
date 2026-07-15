import { test as base, Page } from '@playwright/test';
import { installFakeBackend } from './fake-backend';

/** Base test with the fake backend installed on every page. */
export const test = base.extend({
  page: async ({ page }, use) => {
    await installFakeBackend(page);
    await use(page);
  },
});

export { expect } from '@playwright/test';

/** Seeds an authenticated session in localStorage so guarded routes are reachable. */
export async function authenticate(page: Page): Promise<void> {
  await page.addInitScript(() => {
    localStorage.setItem(
      'liturgy.auth',
      JSON.stringify({
        token: 'fake-token',
        user: {
          accessToken: 'fake-token',
          userId: 'u1',
          email: 'quinn@newhope.dev',
          role: 'Member',
          firstName: 'Quinn',
          lastName: 'Brown',
          initials: 'QB',
        },
      }),
    );
  });
}
