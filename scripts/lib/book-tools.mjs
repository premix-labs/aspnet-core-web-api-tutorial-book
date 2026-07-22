import { existsSync, readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

export function slugifyTitle(value) {
  return value
    .normalize('NFKC')
    .toLocaleLowerCase('en-US')
    .trim()
    .replace(/[^\p{Letter}\p{Mark}\p{Number}]+/gu, '-')
    .replace(/(^-|-$)/g, '');
}

export function parseFrontmatter(content) {
  const block = content.match(/^---\r?\n([\s\S]*?)\r?\n---/);
  if (!block) return null;

  const value = (key) => {
    const match = block[1].match(new RegExp(`^${key}:\\s*(.+)$`, 'm'));
    if (!match) return undefined;
    const raw = match[1].trim();
    if ((raw.startsWith('"') && raw.endsWith('"')) || (raw.startsWith("'") && raw.endsWith("'"))) {
      return raw.slice(1, -1);
    }
    return raw;
  };

  const arrayValue = (key) => {
    const raw = value(key);
    if (raw === undefined) return undefined;
    if (raw === '[]') return [];
    if (!raw.startsWith('[') || !raw.endsWith(']')) return undefined;
    return raw
      .slice(1, -1)
      .split(',')
      .map((item) => item.trim().replace(/^['"]|['"]$/g, ''))
      .filter(Boolean);
  };

  const chapterValue = value('chapter');
  return {
    title: value('title'),
    description: value('description'),
    chapter: chapterValue === undefined ? undefined : Number(chapterValue),
    part: value('part'),
    difficulty: value('difficulty'),
    prerequisites: arrayValue('prerequisites'),
    learningOutcomes: arrayValue('learningOutcomes'),
    testedWith: arrayValue('testedWith'),
    lastVerified: value('lastVerified'),
    status: value('status'),
  };
}

export function readChapterMetadata(chaptersDir) {
  return readdirSync(chaptersDir)
    .filter((file) => file.endsWith('.mdx'))
    .map((file) => {
      const content = readFileSync(join(chaptersDir, file), 'utf8');
      return { file, content, metadata: parseFrontmatter(content) };
    });
}

export function parseChapterArgs(args) {
  const positionals = [];
  const options = {};

  for (let index = 0; index < args.length; index += 1) {
    const argument = args[index];
    if (argument === '--slug' || argument === '--part') {
      const value = args[index + 1];
      if (!value || value.startsWith('--')) throw new Error(`${argument} requires a value.`);
      options[argument.slice(2)] = value;
      index += 1;
      continue;
    }
    if (argument.startsWith('--')) throw new Error(`Unknown option: ${argument}`);
    positionals.push(argument);
  }

  return { title: positionals.join(' ').trim(), ...options };
}

function quoteYaml(value) {
  return JSON.stringify(value);
}

export function createChapter({ chaptersDir, title, requestedSlug, part }) {
  if (!title) throw new Error('A chapter title is required.');

  const chapters = readChapterMetadata(chaptersDir);
  const chapterNumbers = chapters
    .map(({ metadata }) => metadata?.chapter)
    .filter((chapter) => Number.isInteger(chapter));
  const nextChapter = (chapterNumbers.length > 0 ? Math.max(...chapterNumbers) : -1) + 1;
  const titleSlug = requestedSlug ? slugifyTitle(requestedSlug) : slugifyTitle(title);
  const safeSlug = titleSlug || `chapter-${String(nextChapter).padStart(2, '0')}`;
  const slug = `${String(nextChapter).padStart(2, '0')}-${safeSlug}`;
  const filePath = join(chaptersDir, `${slug}.mdx`);

  if (existsSync(filePath)) throw new Error(`File already exists: src/content/chapters/${slug}.mdx`);

  const partLine = part ? `part: ${quoteYaml(part)}\n` : '';
  const frontmatter = `---
title: ${quoteYaml(title)}
description: "TODO: replace with one sentence shown on the home page and chapter header."
chapter: ${nextChapter}
${partLine}icon: book-open
tags: []
difficulty: beginner
prerequisites: []
learningOutcomes: ["TODO: add one observable learner outcome."]
testedWith: ["TODO: record the runtime and tool versions used for verification."]
lastVerified: ${new Date().toISOString().slice(0, 10)}
status: draft
---

> Status: chapter scaffold. Replace this note while writing the lesson.

## Goal

Explain what the learner will be able to do after this chapter.

## Checkpoint

Add a result the learner can verify before continuing.
`;

  writeFileSync(filePath, frontmatter, 'utf8');
  return { filePath, nextChapter, slug };
}
