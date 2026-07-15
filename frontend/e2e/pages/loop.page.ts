import { Locator, Page } from '@playwright/test';
import { BasePage } from './base.page';

export class LoopPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async open(cardId: string): Promise<void> {
    await this.goto(`/loop/${cardId}`);
  }

  currentMovement(): Locator {
    return this.page.locator('.movement.is-current');
  }

  logButton(): Locator {
    return this.page.getByRole('button', { name: /Log & continue|Logging/ });
  }

  markDoneButton(): Locator {
    return this.page.getByRole('button', { name: 'Mark done' });
  }

  dialCount(): Locator {
    return this.page.locator('.dial__count');
  }

  async logCurrent(): Promise<void> {
    const current = this.currentMovement();
    const textarea = current.locator('textarea, input').first();
    if (await textarea.count()) {
      await textarea.fill('Logged in the ritual.');
    }
    await current.getByRole('button', { name: /Log & continue|Logging/ }).click();
  }
}
