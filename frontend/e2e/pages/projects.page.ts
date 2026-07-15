import { Locator, Page } from '@playwright/test';
import { BasePage } from './base.page';

export class ProjectsPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async open(): Promise<void> {
    await this.goto('/projects');
  }

  project(name: string): Locator {
    return this.page.locator('.project-tile', { hasText: name });
  }

  async openProject(name: string): Promise<void> {
    await this.project(name).click();
  }
}
