#!/usr/bin/env node

import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { extname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const severityRank = { low: 1, medium: 2, high: 3, critical: 4 };
const supportedExtensions = new Set(['.md', '.mdx']);

function lineNumberAt(content, index) {
  return content.slice(0, index).split('\n').length;
}

function addFinding(findings, file, ruleId, severity, line, message, remediation) {
  findings.push({ ruleId, severity, file, line, message, remediation });
}

function parseFrontmatter(content) {
  if (!content.startsWith('---\n') && !content.startsWith('---\r\n')) {
    return { body: content, status: undefined, chapter: undefined, bodyOffset: 0 };
  }

  const normalized = content.replaceAll('\r\n', '\n');
  const closing = normalized.indexOf('\n---\n', 4);
  if (closing === -1) {
    return { body: normalized, status: undefined, chapter: undefined, bodyOffset: 0 };
  }

  const frontmatter = normalized.slice(4, closing);
  const status = frontmatter.match(/^status:\s*['"]?([^'"\s]+)['"]?\s*$/m)?.[1];
  const chapterValue = frontmatter.match(/^chapter:\s*(\d+)\s*$/m)?.[1];
  const chapter = chapterValue === undefined ? undefined : Number(chapterValue);
  const bodyOffset = closing + 5;
  return { body: normalized.slice(bodyOffset), status, chapter, bodyOffset };
}

function auditCodeFences(content, file, findings) {
  const lines = content.replaceAll('\r\n', '\n').split('\n');
  let openFence;

  for (let index = 0; index < lines.length; index += 1) {
    const match = lines[index].match(/^\s*```([^`]*)$/);
    if (!match) continue;

    if (!openFence) {
      const language = match[1].trim();
      openFence = { line: index + 1, language };
      if (!language) {
        addFinding(
          findings,
          file,
          'TB002',
          'medium',
          index + 1,
          'Fenced code block has no language identifier.',
          'Add the appropriate language after the opening fence.'
        );
      }
      continue;
    }

    const blockLength = index - openFence.line;
    if (blockLength > 50) {
      addFinding(
        findings,
        file,
        'TB003',
        'medium',
        openFence.line,
        `Code block contains ${blockLength} lines.`,
        'Split the block into focused steps with explanations, or document why the complete file is necessary.'
      );
    }
    openFence = undefined;
  }

  if (openFence) {
    addFinding(
      findings,
      file,
      'TB004',
      'high',
      openFence.line,
      'Fenced code block is not closed.',
      'Add the closing fence and rebuild the book.'
    );
  }
}

export function auditChapterContent(content, file = 'chapter.mdx') {
  const findings = [];
  const normalized = content.replaceAll('\r\n', '\n');
  const { body, status, chapter, bodyOffset } = parseFrontmatter(normalized);
  const bodyStartLine = lineNumberAt(normalized, bodyOffset);
  const published = status === 'published';

  for (const match of normalized.matchAll(/\b(?:TODO|FIXME|TBD)\b/gi)) {
    addFinding(
      findings,
      file,
      'TB001',
      published ? 'high' : 'low',
      lineNumberAt(normalized, match.index),
      `Unresolved ${match[0].toUpperCase()} marker found${published ? ' in published content' : ''}.`,
      'Replace the marker with complete content or keep the chapter in draft status.'
    );
  }

  auditCodeFences(normalized, file, findings);

  for (const match of normalized.matchAll(/(?:[A-Za-z]:\\(?:Users|code)\\|\/(?:Users|home)\/[^/\s]+\/)/g)) {
    addFinding(
      findings,
      file,
      'TB005',
      'high',
      lineNumberAt(normalized, match.index),
      'Machine-specific absolute path appears in learner content.',
      'Use a repository-relative path or clearly named placeholder.'
    );
  }

  const secretPattern =
    /\b(?:password|api[_-]?key|access[_-]?token|client[_-]?secret)\b\s*[:=]\s*['"]([^'"\n]{8,})['"]/gi;
  for (const match of normalized.matchAll(secretPattern)) {
    const value = match[1].trim();
    if (/^(?:your-|example-|placeholder|change-me|<|\$\{)/i.test(value)) continue;
    addFinding(
      findings,
      file,
      'TB006',
      'critical',
      lineNumberAt(normalized, match.index),
      'A credential-like literal appears in learner content.',
      'Replace it with a descriptive placeholder and remove the real value from repository history.'
    );
  }

  if (published && !/^##\s+\S+/m.test(body)) {
    addFinding(
      findings,
      file,
      'TB007',
      'medium',
      bodyStartLine,
      'Published chapter has no second-level section headings.',
      'Organize the chapter into scannable sections with descriptive headings.'
    );
  }

  const prose = body
    .replace(/```[\s\S]*?```/g, ' ')
    .replace(/<[^>]+>/g, ' ')
    .replace(/[`*_#[\]()>|{}-]/g, ' ');
  const wordCount = [...new Intl.Segmenter(undefined, { granularity: 'word' }).segment(prose)].filter(
    (segment) => segment.isWordLike
  ).length;
  if (published && wordCount < 80) {
    addFinding(
      findings,
      file,
      'TB008',
      'medium',
      bodyStartLine,
      `Published chapter contains only ${wordCount} prose words.`,
      'Confirm the chapter fully explains context, steps, expected results, and a checkpoint.'
    );
  }

  const handsOn = /```|<Steps\b|\b(?:npm|pnpm|yarn|dotnet|mvnw?|gradlew?|docker|pytest)\b/i.test(body);
  if (
    published &&
    chapter !== 0 &&
    handsOn &&
    !/\b(?:run|test|build|verify|check|expected|result|output|response|status code)\b|(?:ทดสอบ|ตรวจสอบ|ผลลัพธ์|ควรเห็น)/i.test(
      body
    )
  ) {
    addFinding(
      findings,
      file,
      'TB009',
      'medium',
      bodyStartLine,
      'Hands-on chapter has no detectable verification command or expected-result guidance.',
      'Add a runnable command and describe the observable successful result.'
    );
  }

  if (
    published &&
    chapter !== 0 &&
    handsOn &&
    !/\b(?:checkpoint|definition of done|before the next chapter|ready to continue)\b|(?:ก่อนบทถัดไป|จุดตรวจ)/i.test(
      body
    )
  ) {
    addFinding(
      findings,
      file,
      'TB010',
      'low',
      bodyStartLine,
      'Hands-on chapter has no detectable learner checkpoint.',
      'Add a checkpoint that readers can verify before continuing.'
    );
  }

  return findings;
}

function collectMarkdownFiles(target) {
  if (!existsSync(target)) return [];
  if (statSync(target).isFile()) return supportedExtensions.has(extname(target)) ? [target] : [];

  const files = [];
  for (const entry of readdirSync(target, { withFileTypes: true })) {
    const path = join(target, entry.name);
    if (entry.isDirectory()) files.push(...collectMarkdownFiles(path));
    else if (supportedExtensions.has(extname(entry.name))) files.push(path);
  }
  return files.sort();
}

export function auditBook({ root, target = join(root, 'src', 'content', 'chapters') }) {
  const files = collectMarkdownFiles(target);
  const findings = files.flatMap((file) =>
    auditChapterContent(readFileSync(file, 'utf8'), relative(root, file).replaceAll('\\', '/'))
  );
  const counts = { critical: 0, high: 0, medium: 0, low: 0 };
  for (const finding of findings) counts[finding.severity] += 1;

  return {
    schemaVersion: 1,
    tool: 'tutorial-book-auditor',
    root,
    target: relative(root, target).replaceAll('\\', '/') || '.',
    filesAudited: files.length,
    counts,
    findings,
  };
}

export function parseArgs(argv) {
  const raw = { root: process.cwd(), target: undefined, format: 'text', failOn: 'high' };
  for (let index = 0; index < argv.length; index += 1) {
    const value = argv[index];
    if (value === '--root') raw.root = argv[++index];
    else if (value === '--path') raw.target = argv[++index];
    else if (value === '--format') raw.format = argv[++index];
    else if (value === '--fail-on') raw.failOn = argv[++index];
    else throw new Error(`Unknown option: ${value}`);
  }
  if (!['text', 'json'].includes(raw.format)) throw new Error('--format must be text or json.');
  if (!['critical', 'high', 'medium', 'low', 'none'].includes(raw.failOn)) {
    throw new Error('--fail-on must be critical, high, medium, low or none.');
  }
  const root = resolve(raw.root);
  return {
    root,
    target: raw.target ? resolve(root, raw.target) : join(root, 'src', 'content', 'chapters'),
    format: raw.format,
    failOn: raw.failOn,
  };
}

function printText(report) {
  console.log(`Tutorial book automated audit: ${report.filesAudited} file(s)`);
  console.log(
    `Findings: ${report.counts.critical} critical, ${report.counts.high} high, ${report.counts.medium} medium, ${report.counts.low} low`
  );
  for (const finding of report.findings) {
    console.log(
      `[${finding.severity.toUpperCase()}] ${finding.ruleId} ${finding.file}:${finding.line} - ${finding.message}`
    );
  }
}

export function shouldFail(report, failOn) {
  if (failOn === 'none') return false;
  const threshold = severityRank[failOn];
  return report.findings.some((finding) => severityRank[finding.severity] >= threshold);
}

const isCli = process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url);
if (isCli) {
  try {
    const options = parseArgs(process.argv.slice(2));
    const report = auditBook(options);
    if (options.format === 'json') console.log(JSON.stringify(report, null, 2));
    else printText(report);
    if (shouldFail(report, options.failOn)) process.exitCode = 1;
  } catch (error) {
    console.error(error instanceof Error ? error.message : String(error));
    process.exitCode = 2;
  }
}
