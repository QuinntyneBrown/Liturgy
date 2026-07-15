import { Locator, Page } from '@playwright/test';
import { BasePage } from './base.page';

export class DevelopBoardPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async open(projectId: string): Promise<void> {
    await this.goto(`/board/${projectId}`);
  }

  column(title: string): Locator {
    return this.page.locator('.col', { hasText: title });
  }

  card(code: string): Locator {
    return this.page.locator('.wcard', { hasText: code });
  }

  lockedNote(): Locator {
    return this.page.getByText("Needs all 5 R's to enter");
  }
}
