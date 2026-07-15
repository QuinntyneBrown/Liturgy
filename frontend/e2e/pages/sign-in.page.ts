import { Page } from '@playwright/test';
import { BasePage } from './base.page';

export class SignInPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async open(): Promise<void> {
    await this.goto('/sign-in');
  }

  async signIn(email: string, password: string): Promise<void> {
    await this.page.fill('#email', email);
    await this.page.fill('#password', password);
    await this.page.getByRole('button', { name: 'Sign in' }).click();
  }
}
