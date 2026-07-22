import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';
import {
  auditBook,
  auditChapterContent,
  parseArgs,
  shouldFail,
} from '../../skills/tutorial-book-auditor/scripts/audit-book.mjs';

const fixtures = join(
  dirname(fileURLToPath(import.meta.url)),
  '..',
  '..',
  'skills',
  'tutorial-book-auditor',
  'scripts',
  'fixtures'
);

test('enterprise fixture has no automated audit findings', () => {
  const content = readFileSync(join(fixtures, 'complete-chapter.mdx'), 'utf8');
  assert.deepEqual(auditChapterContent(content, 'complete-chapter.mdx'), []);
});

test('high-risk fixture produces stable severity and rule IDs', () => {
  const content = readFileSync(join(fixtures, 'high-risk-chapter.txt'), 'utf8');
  const findings = auditChapterContent(content, 'high-risk-chapter.txt');
  const ruleIds = new Set(findings.map((finding) => finding.ruleId));

  assert.ok(ruleIds.has('TB001'));
  assert.ok(ruleIds.has('TB002'));
  assert.ok(ruleIds.has('TB004'));
  assert.ok(ruleIds.has('TB005'));
  assert.ok(ruleIds.has('TB006'));
  assert.equal(findings.find((finding) => finding.ruleId === 'TB006')?.severity, 'critical');
  assert.equal(shouldFail({ findings }, 'high'), true);
});

test('CLI arguments support JSON output and configurable failure thresholds', () => {
  const options = parseArgs(['--path', 'chapters', '--root', '.', '--format', 'json', '--fail-on', 'medium']);
  assert.equal(options.format, 'json');
  assert.equal(options.failOn, 'medium');
  assert.match(options.target, /chapters$/);
});

test('audit report exposes a stable JSON schema', () => {
  const target = join(fixtures, 'complete-chapter.mdx');
  const report = auditBook({ root: fixtures, target });
  const serialized = JSON.parse(JSON.stringify(report));

  assert.equal(serialized.schemaVersion, 1);
  assert.equal(serialized.tool, 'tutorial-book-auditor');
  assert.equal(serialized.filesAudited, 1);
  assert.deepEqual(serialized.counts, { critical: 0, high: 0, medium: 0, low: 0 });
});

test('Unicode word segmentation does not penalize Thai prose', () => {
  const prose = 'บทเรียนนี้อธิบายแนวคิดและขั้นตอนอย่างละเอียดเพื่อให้ผู้อ่านเข้าใจเหตุผลและตรวจสอบผลลัพธ์ได้';
  const content = `---\ntitle: บททดสอบ\ndescription: ทดสอบการนับคำภาษาไทย\nchapter: 1\nstatus: published\n---\n\n## เนื้อหา\n\n${prose.repeat(12)}`;
  const findings = auditChapterContent(content, 'thai-chapter.mdx');

  assert.equal(
    findings.some((finding) => finding.ruleId === 'TB008'),
    false
  );
});
