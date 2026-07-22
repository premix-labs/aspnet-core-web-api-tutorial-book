#!/usr/bin/env node

import { existsSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const snapshots = join(root, 'tests', 'visual', 'visual.spec.ts-snapshots');
const platform = process.env.PLAYWRIGHT_SNAPSHOT_PLATFORM ?? process.platform;
const required = ['home', 'reader'].flatMap((page) =>
  ['desktop', 'mobile'].map((viewport) => `${page}-chromium-${viewport}-${platform}.png`)
);
const missing = required.filter((file) => !existsSync(join(snapshots, file)));

if (missing.length > 0) {
  console.error(`Visual baselines are missing for ${platform}:`);
  for (const file of missing) console.error(`- tests/visual/visual.spec.ts-snapshots/${file}`);
  console.error('Run npm run test:visual:update, review the images, and commit the approved baselines.');
  if (platform !== 'linux') {
    console.error('Use the Generate Linux Visual Baselines workflow to prepare the Linux baselines required by CI.');
  }
  process.exit(1);
}

console.log(`Visual baseline validation passed for ${platform}: ${required.length} files.`);
