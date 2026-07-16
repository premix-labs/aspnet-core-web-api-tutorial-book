#!/usr/bin/env node
// Scaffolds a new chapter .mdx file with valid frontmatter (see src/content.config.ts
// for the schema it must satisfy) and the next sequential chapter number.
//
// Usage: npm run new-chapter -- "My Chapter Title"

import { readdirSync, readFileSync, writeFileSync, existsSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const chaptersDir = join(__dirname, '..', 'src', 'content', 'chapters');

const title = process.argv.slice(2).join(' ').trim();
if (!title) {
  console.error('Usage: npm run new-chapter -- "My Chapter Title"');
  process.exit(1);
}

const slugify = (text) =>
  text
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');

const files = readdirSync(chaptersDir).filter((f) => f.endsWith('.mdx'));
let maxChapter = -1;
for (const file of files) {
  const content = readFileSync(join(chaptersDir, file), 'utf-8');
  const match = content.match(/^chapter:\s*(\d+)/m);
  if (match) maxChapter = Math.max(maxChapter, Number(match[1]));
}
const nextChapter = maxChapter + 1;

const slug = `${String(nextChapter).padStart(2, '0')}-${slugify(title)}`;
const filePath = join(chaptersDir, `${slug}.mdx`);

if (existsSync(filePath)) {
  console.error(`File already exists: src/content/chapters/${slug}.mdx`);
  process.exit(1);
}

const frontmatter = `---
title: ${title}
description: TODO — one sentence shown on the landing page card and chapter header.
chapter: ${nextChapter}
icon: book-open
tags: []
---

# ${title}

Write your chapter here.

<div class="callout callout--tip">
  <div class="callout-body">
    <div class="callout-title">Tip</div>
    <div class="callout-content">
      <p>Use <code>##</code> and <code>###</code> headings — they populate the "On this page" table of contents automatically.</p>
    </div>
  </div>
</div>
`;

writeFileSync(filePath, frontmatter, 'utf-8');
console.log(`Created src/content/chapters/${slug}.mdx (chapter ${nextChapter})`);
