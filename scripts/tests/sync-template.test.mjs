import assert from 'node:assert/strict';
import { appendFileSync, mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';
import { createBook } from '../create-book.mjs';
import { syncTemplate } from '../sync-template.mjs';

const templateRoot = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
const templateMetadata = JSON.parse(readFileSync(join(templateRoot, '.book-template.json'), 'utf8'));
const isSourceTemplate = Object.keys(templateMetadata.managedFiles ?? {}).length === 0;

test(
  'syncTemplate checks the source template without an explicit path',
  { skip: !isSourceTemplate && 'Only the source template resolves itself as an update source.' },
  () => {
    const result = syncTemplate({ mode: 'check', force: false }, templateRoot);

    assert.equal(result.updateAvailable, false);
    assert.deepEqual(result.drift, []);
  }
);

test('syncTemplate refuses to overwrite a locally modified managed file', { skip: !isSourceTemplate }, () => {
  const directory = mkdtempSync(join(tmpdir(), 'astro-book-sync-'));
  const target = join(directory, 'managed-book');
  try {
    createBook({
      sourceRoot: templateRoot,
      target,
      title: 'Managed Book',
      slug: 'managed-book',
      description: 'A generated book used to verify conflict-aware template updates.',
    });
    appendFileSync(join(target, 'src', 'styles', 'global.css'), '\n/* local change */\n');

    assert.throws(
      () => syncTemplate({ mode: 'apply', source: templateRoot, force: false }, target),
      /Refusing to overwrite locally modified managed files/
    );
  } finally {
    rmSync(directory, { recursive: true, force: true });
  }
});
