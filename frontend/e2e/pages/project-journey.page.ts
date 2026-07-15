import { Locator, Page } from '@playwright/test';
import { BasePage } from './base.page';

export class ProjectJourneyPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async open(projectId: string): Promise<void> {
    await this.goto(`/projects/${projectId}`);
  }

  gate(title: string): Locator {
    return this.page.locator('.gate', { hasText: title });
  }

  requirementCheckbox(label: string): Locator {
    return this.page.getByRole('button', { name: `Toggle: ${label}` });
  }

  phaseRow(kind: string): Locator {
    // Scope to the phase-name element so a gate title like "Develop → Demonstrate"
    // does not also match the Develop row.
    return this.page.locator('.phase-row', {
      has: this.page.locator('.phase-row__name', { hasText: new RegExp(`^${kind}$`) }),
    });
  }
}
