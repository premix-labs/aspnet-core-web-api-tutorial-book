#!/usr/bin/env node

import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createChapter, parseChapterArgs } from './lib/book-tools.mjs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const chaptersDir = join(__dirname, '..', 'src', 'content', 'chapters');

try {
  const { title, slug, part } = parseChapterArgs(process.argv.slice(2));
  if (!title) {
    console.error('Usage: npm run new-chapter -- "Chapter title" [--slug chapter-slug] [--part "Part 1: Foundations"]');
    process.exit(1);
  }

  const result = createChapter({ chaptersDir, title, requestedSlug: slug, part });
  console.log(`Created src/content/chapters/${result.slug}.mdx (chapter ${result.nextChapter})`);
} catch (error) {
  console.error(error instanceof Error ? error.message : error);
  process.exit(1);
}
