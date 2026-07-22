#!/usr/bin/env node

import { existsSync, readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { readChapterMetadata } from './lib/book-tools.mjs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const chaptersDir = join(root, 'src', 'content', 'chapters');
const errors = [];
const maximumVerificationAgeDays = Number(process.env.MAX_VERIFICATION_AGE_DAYS ?? 180);
const today = new Date();

const requiredFiles = [
  '.gitattributes',
  'README.md',
  'AGENTS.md',
  'CHANGELOG.md',
  'CODE_OF_CONDUCT.md',
  'CONTRIBUTING.md',
  'GOVERNANCE.md',
  'LICENSE',
  'package-lock.json',
  'SECURITY.md',
  'SUPPORT.md',
  '.book-template.json',
  '.template-manifest.json',
  'src/site.config.ts',
  'docs/internal/README.md',
  'docs/internal/book-plan.md',
  'docs/internal/api-contract.md',
  'docs/internal/final-project-structure.md',
  'docs/internal/manuscript-status.md',
  'docs/internal/operations-runbook.md',
  'docs/internal/release-checklist.md',
  'docs/internal/style-guide.md',
  'docs/internal/teaching-principles.md',
  'docs/internal/template-lifecycle.md',
  'docs/internal/validation-report.md',
  'docs/internal/decisions/0001-tech-stack.md',
  'docs/internal/qa/accessibility-checklist.md',
  'docs/internal/qa/browser-test-plan.md',
  'docs/internal/qa/security-review-checklist.md',
  'skills/tutorial-book-auditor/SKILL.md',
  'skills/tutorial-book-auditor/agents/openai.yaml',
  'skills/tutorial-book-auditor/references/automated-rules.md',
  'skills/tutorial-book-auditor/references/report-contract.md',
  'skills/tutorial-book-auditor/references/scoring-policy.md',
  'skills/tutorial-book-auditor/references/teaching-principles.md',
  'skills/tutorial-book-auditor/scripts/audit-book.mjs',
];

for (const file of requiredFiles) {
  if (!existsSync(join(root, file))) errors.push(`Missing required file: ${file}`);
}

const packageJson = JSON.parse(readFileSync(join(root, 'package.json'), 'utf8'));
const packageLock = JSON.parse(readFileSync(join(root, 'package-lock.json'), 'utf8'));
const templateManifest = JSON.parse(readFileSync(join(root, '.template-manifest.json'), 'utf8'));
const templateMetadata = JSON.parse(readFileSync(join(root, '.book-template.json'), 'utf8'));

if (packageJson.version !== templateManifest.templateVersion) {
  errors.push(
    `package.json version ${packageJson.version} does not match template ${templateManifest.templateVersion}.`
  );
}
if (packageLock.name !== packageJson.name || packageLock.version !== packageJson.version) {
  errors.push('package-lock.json name/version must match package.json.');
}
if (
  packageLock.packages?.['']?.name !== packageJson.name ||
  packageLock.packages?.['']?.version !== packageJson.version
) {
  errors.push('package-lock.json root package metadata must match package.json.');
}
if (templateMetadata.templateVersion !== templateManifest.templateVersion) {
  errors.push(
    `.book-template.json version ${templateMetadata.templateVersion} does not match template ${templateManifest.templateVersion}.`
  );
}

const uniqueManagedFiles = new Set(templateManifest.managedFiles);
if (uniqueManagedFiles.size !== templateManifest.managedFiles.length)
  errors.push('.template-manifest.json contains duplicate managed files.');
for (const file of uniqueManagedFiles) {
  if (!existsSync(join(root, file))) errors.push(`Template manifest references a missing managed file: ${file}`);
}
for (const bookOwnedFile of [
  'src/site.config.ts',
  'src/content/chapters',
  'docs/internal/book-plan.md',
  'docs/internal/validation-report.md',
]) {
  if (uniqueManagedFiles.has(bookOwnedFile))
    errors.push(`Book-owned path must not be template-managed: ${bookOwnedFile}`);
}

const workflowDirectory = join(root, '.github', 'workflows');
for (const workflow of [
  'ci.yml',
  'deploy.yml',
  'release.yml',
  'rollback.yml',
  'security.yml',
  'visual-baselines.yml',
]) {
  const content = readFileSync(join(workflowDirectory, workflow), 'utf8');
  for (const match of content.matchAll(/^\s*uses:\s*([^@\s]+)@([^\s#]+)/gm)) {
    if (!/^[a-f0-9]{40}$/.test(match[2])) errors.push(`${workflow}: ${match[1]} must be pinned to a full commit SHA.`);
  }
}

const chapters = readChapterMetadata(chaptersDir);
const seenNumbers = new Map();
let introductions = 0;

for (const chapter of chapters) {
  const { file, metadata } = chapter;
  if (!metadata) {
    errors.push(`${file}: missing frontmatter block.`);
    continue;
  }
  if (!metadata.title) errors.push(`${file}: missing title.`);
  if (!metadata.description) errors.push(`${file}: missing description.`);
  if (!['beginner', 'intermediate', 'advanced'].includes(metadata.difficulty)) {
    errors.push(`${file}: difficulty must be beginner, intermediate or advanced.`);
  }
  if (!Array.isArray(metadata.prerequisites)) errors.push(`${file}: prerequisites must be an inline array.`);
  if (!Array.isArray(metadata.learningOutcomes) || metadata.learningOutcomes.length === 0) {
    errors.push(`${file}: learningOutcomes must contain at least one observable outcome.`);
  }
  if (!Array.isArray(metadata.testedWith) || metadata.testedWith.length === 0) {
    errors.push(`${file}: testedWith must contain at least one verified tool or runtime.`);
  }
  if (!['draft', 'review', 'published', 'deprecated'].includes(metadata.status)) {
    errors.push(`${file}: status must be draft, review, published or deprecated.`);
  }
  if (!/^\d{4}-\d{2}-\d{2}$/.test(metadata.lastVerified ?? '')) {
    errors.push(`${file}: lastVerified must use YYYY-MM-DD.`);
  } else {
    const verifiedAt = new Date(`${metadata.lastVerified}T00:00:00Z`);
    const ageDays = Math.floor((today.getTime() - verifiedAt.getTime()) / 86_400_000);
    if (Number.isNaN(verifiedAt.getTime())) errors.push(`${file}: lastVerified is not a valid date.`);
    else if (ageDays < 0) errors.push(`${file}: lastVerified cannot be in the future.`);
    else if (metadata.status === 'published' && ageDays > maximumVerificationAgeDays) {
      errors.push(
        `${file}: published content was last verified ${ageDays} days ago (limit ${maximumVerificationAgeDays}).`
      );
    }
  }
  if (!Number.isInteger(metadata.chapter) || metadata.chapter < 0) {
    errors.push(`${file}: chapter must be a non-negative integer.`);
  } else {
    if (seenNumbers.has(metadata.chapter)) {
      errors.push(`${file}: duplicate chapter ${metadata.chapter} also used by ${seenNumbers.get(metadata.chapter)}.`);
    }
    seenNumbers.set(metadata.chapter, file);
    if (metadata.chapter === 0) introductions += 1;
  }

  if (/^\d+-$/.test(file.replace(/\.mdx$/, ''))) errors.push(`${file}: filename has an empty slug.`);

  const filenameChapter = file.match(/^(\d+)-/);
  if (filenameChapter && Number(filenameChapter[1]) !== metadata?.chapter) {
    errors.push(
      `${file}: filename chapter ${Number(filenameChapter[1])} does not match frontmatter chapter ${metadata?.chapter}.`
    );
  }
}

if (introductions !== 1) errors.push(`Expected exactly one chapter 0 introduction, found ${introductions}.`);

const orderedNumbers = [...seenNumbers.keys()].sort((left, right) => left - right);
for (let index = 0; index < orderedNumbers.length; index += 1) {
  if (orderedNumbers[index] !== index) {
    errors.push(`Chapter sequence must be contiguous from 0; expected ${index}, found ${orderedNumbers[index]}.`);
    break;
  }
}

function extractChapterRows(file) {
  const content = readFileSync(join(root, file), 'utf8');
  return [...content.matchAll(/^\|\s*(\d+)\s*\|\s*([^|]*?)\s*\|/gm)]
    .map((match) => ({
      chapter: Number(match[1]),
      title: match[2].trim(),
    }))
    .filter((row) => row.title);
}

const planRows = extractChapterRows('docs/internal/book-plan.md');
const statusRows = extractChapterRows('docs/internal/manuscript-status.md');
if (planRows.length > 0 || statusRows.length > 0) {
  if (planRows.length !== statusRows.length) {
    errors.push(`Plan/status chapter counts differ: ${planRows.length} vs ${statusRows.length}.`);
  } else {
    for (let index = 0; index < planRows.length; index += 1) {
      const plan = planRows[index];
      const status = statusRows[index];
      if (plan.chapter !== status.chapter || plan.title !== status.title) {
        errors.push(
          `Plan/status mismatch at row ${index + 1}: "${plan.chapter} ${plan.title}" vs "${status.chapter} ${status.title}".`
        );
      }
    }
  }
}

if (errors.length > 0) {
  console.error(`Book validation failed with ${errors.length} issue(s):`);
  for (const error of errors) console.error(`- ${error}`);
  process.exit(1);
}

console.log(`Book validation passed: ${chapters.length} content files, ${requiredFiles.length} required files.`);
