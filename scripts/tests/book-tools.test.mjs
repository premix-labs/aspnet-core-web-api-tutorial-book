import assert from 'node:assert/strict';
import { mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import test from 'node:test';
import { createChapter, parseChapterArgs, slugifyTitle } from '../lib/book-tools.mjs';

test('slugifyTitle preserves Thai letters and normalizes separators', () => {
  assert.equal(slugifyTitle('เริ่มต้น ใช้งาน Tailwind CSS'), 'เริ่มต้น-ใช้งาน-tailwind-css');
});

test('parseChapterArgs supports an explicit slug and part', () => {
  assert.deepEqual(parseChapterArgs(['ชื่อบท', '--slug', 'getting-started', '--part', 'Part 1: Foundations']), {
    title: 'ชื่อบท',
    slug: 'getting-started',
    part: 'Part 1: Foundations',
  });
});

test('createChapter creates a valid Unicode chapter file', () => {
  const directory = mkdtempSync(join(tmpdir(), 'astro-book-chapter-'));
  try {
    const result = createChapter({ chaptersDir: directory, title: 'บทภาษาไทย' });
    assert.equal(result.slug, '00-บทภาษาไทย');
    const content = readFileSync(result.filePath, 'utf8');
    assert.match(content, /chapter: 0/);
    assert.match(content, /title: "บทภาษาไทย"/);
    assert.match(content, /learningOutcomes:/);
    assert.match(content, /status: draft/);
  } finally {
    rmSync(directory, { recursive: true, force: true });
  }
});
