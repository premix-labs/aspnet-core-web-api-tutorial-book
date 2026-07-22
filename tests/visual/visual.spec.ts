import { expect, test } from '@playwright/test';

async function stabilizePage(page: import('@playwright/test').Page) {
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await page.addStyleTag({
    content: `
      *, *::before, *::after {
        animation-duration: 0s !important;
        caret-color: transparent !important;
        transition-duration: 0s !important;
      }
    `,
  });
  await page.evaluate(() => document.fonts.ready);
}

for (const scenario of [
  { name: 'home', path: './' },
  { name: 'reader', path: 'intro/' },
]) {
  test(`${scenario.name} matches its reviewed visual baseline`, async ({ page }) => {
    await page.goto(scenario.path);
    await stabilizePage(page);

    await expect(page).toHaveScreenshot(`${scenario.name}.png`, {
      animations: 'disabled',
      fullPage: true,
      maxDiffPixelRatio: 0.01,
      threshold: 0.3,
    });
  });
}
