import { expect, test } from '@playwright/test';

const budgets = {
  totalBytes: 2_500_000,
  scriptBytes: 900_000,
  styleBytes: 300_000,
  fontBytes: 1_500_000,
  domNodes: 1_500,
  domContentLoadedMs: 2_500,
};

test('home stays within the production performance budget', async ({ page }) => {
  await page.goto('./');
  await page.waitForLoadState('networkidle');

  const metrics = await page.evaluate(() => {
    const resources = performance.getEntriesByType('resource') as PerformanceResourceTiming[];
    const bytes = (entry: PerformanceResourceTiming) => entry.transferSize || entry.encodedBodySize || 0;
    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
    return {
      totalBytes: resources.reduce((total, entry) => total + bytes(entry), 0),
      scriptBytes: resources
        .filter((entry) => entry.initiatorType === 'script')
        .reduce((total, entry) => total + bytes(entry), 0),
      styleBytes: resources
        .filter((entry) => entry.initiatorType === 'css' || entry.initiatorType === 'link')
        .reduce((total, entry) => total + bytes(entry), 0),
      fontBytes: resources
        .filter((entry) => /\.(woff2?|ttf|otf)(\?|$)/i.test(entry.name))
        .reduce((total, entry) => total + bytes(entry), 0),
      domNodes: document.querySelectorAll('*').length,
      domContentLoadedMs: navigation.domContentLoadedEventEnd,
    };
  });

  expect(metrics.totalBytes).toBeLessThanOrEqual(budgets.totalBytes);
  expect(metrics.scriptBytes).toBeLessThanOrEqual(budgets.scriptBytes);
  expect(metrics.styleBytes).toBeLessThanOrEqual(budgets.styleBytes);
  expect(metrics.fontBytes).toBeLessThanOrEqual(budgets.fontBytes);
  expect(metrics.domNodes).toBeLessThanOrEqual(budgets.domNodes);
  expect(metrics.domContentLoadedMs).toBeLessThanOrEqual(budgets.domContentLoadedMs);
});
