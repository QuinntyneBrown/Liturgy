import { test, expect, authenticate } from '../fixtures/test';
import { ProjectJourneyPage } from '../pages/project-journey.page';

test.beforeEach(async ({ page }) => {
  await authenticate(page);
});

test('completing the last requirement opens the gate and unlocks Demonstrate', async ({ page }) => {
  const journey = new ProjectJourneyPage(page);
  await journey.open('p-lantern');

  const gate = journey.gate('Develop → Demonstrate');
  await expect(gate).toBeVisible();
  await expect(gate.locator('.badge--blocked')).toBeVisible();
  await expect(journey.phaseRow('Demonstrate').locator('.badge')).toHaveText('Locked');

  await journey.requirementCheckbox('Demo prepared for the community').click();

  await expect(gate.locator('.badge--done')).toHaveText('Open');
  await expect(journey.phaseRow('Demonstrate').locator('.badge')).toHaveText('Current');
});
