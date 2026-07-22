import assert from 'node:assert/strict';
import { existsSync, mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { spawnSync } from 'node:child_process';
import test from 'node:test';
import { fileURLToPath } from 'node:url';
import { check as checkFormatting } from 'prettier';
import { createBook } from '../create-book.mjs';

const templateRoot = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
const sourceTemplateMetadata = JSON.parse(readFileSync(join(templateRoot, '.book-template.json'), 'utf8'));
const isSourceTemplate = Object.keys(sourceTemplateMetadata.managedFiles ?? {}).length === 0;

test('createBook produces a clean localized scaffold', { skip: !isSourceTemplate }, async () => {
  const directory = mkdtempSync(join(tmpdir(), 'astro-book-create-'));
  const target = join(directory, 'thai-book');
  try {
    createBook({
      sourceRoot: templateRoot,
      target,
      title: 'หนังสือทดสอบ',
      slug: 'thai-test-book',
      description: 'หนังสือสำหรับทดสอบ template',
      lang: 'th',
    });

    const packageJson = JSON.parse(readFileSync(join(target, 'package.json'), 'utf8'));
    const templateMetadata = JSON.parse(readFileSync(join(target, '.book-template.json'), 'utf8'));
    assert.equal(packageJson.name, 'thai-test-book');
    assert.equal(templateMetadata.templateVersion, sourceTemplateMetadata.templateVersion);
    assert.ok(Object.keys(templateMetadata.managedFiles).length > 20);
    assert.match(readFileSync(join(target, 'src', 'site.config.ts'), 'utf8'), /หนังสือทดสอบ/);
    assert.match(readFileSync(join(target, 'src', 'content', 'chapters', 'intro.mdx'), 'utf8'), /เริ่มต้นหนังสือ/);
    assert.match(
      readFileSync(join(target, 'src', 'content', 'chapters', 'intro.mdx'), 'utf8'),
      /lastVerified: \d{4}-\d{2}-\d{2}/
    );
    const plan = readFileSync(join(target, 'docs', 'internal', 'book-plan.md'), 'utf8');
    assert.match(plan, /Working title: หนังสือทดสอบ/);
    assert.match(plan, /Repository name: thai-test-book/);
    assert.match(plan, /Content language: th/);
    assert.match(plan, /Target reader: Not decided/);
    assert.doesNotMatch(plan, /Astro Technical Book Template/);
    assert.equal(existsSync(join(target, 'tests', 'visual', 'visual.spec.ts-snapshots')), false);
    assert.equal(existsSync(join(target, 'scripts', 'shims', 'picomatch.mjs')), true);
    assert.equal(existsSync(join(target, 'node_modules')), false);
    assert.equal(existsSync(join(target, '.git')), false);
    assert.equal(existsSync(join(target, 'dist')), false);

    for (const file of [
      'README.md',
      'CHANGELOG.md',
      'docs/internal/book-plan.md',
      'docs/internal/manuscript-status.md',
      'docs/internal/validation-report.md',
    ]) {
      const path = join(target, file);
      assert.equal(await checkFormatting(readFileSync(path, 'utf8'), { filepath: path }), true, file);
    }

    const visualCheck = spawnSync(process.execPath, ['scripts/check-visual-baselines.mjs'], {
      cwd: target,
      encoding: 'utf8',
    });
    assert.notEqual(visualCheck.status, 0);
    assert.match(visualCheck.stderr, /Visual baselines are missing/);

    const validation = spawnSync(process.execPath, ['scripts/validate-book.mjs'], {
      cwd: target,
      encoding: 'utf8',
    });
    assert.equal(validation.status, 0, validation.stderr || validation.stdout);
  } finally {
    rmSync(directory, { recursive: true, force: true });
  }
});

test('createBook refuses to treat a generated book as a template source', { skip: !isSourceTemplate }, () => {
  const directory = mkdtempSync(join(tmpdir(), 'astro-book-source-contract-'));
  const generated = join(directory, 'generated');
  try {
    createBook({
      sourceRoot: templateRoot,
      target: generated,
      title: 'Generated Book',
      slug: 'generated-book',
      description: 'A generated book used to verify the source-template contract.',
    });

    assert.throws(
      () =>
        createBook({
          sourceRoot: generated,
          target: join(directory, 'nested'),
          title: 'Nested Book',
          slug: 'nested-book',
        }),
      /must be run from the astro-book-template source repository/
    );
  } finally {
    rmSync(directory, { recursive: true, force: true });
  }
});
