#!/usr/bin/env node

import { existsSync, readdirSync, readFileSync, statSync } from 'node:fs';
import { dirname, extname, join, normalize, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { parse } from 'parse5';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const dist = join(root, 'dist');
const errors = [];
const excludedElements = new Set(['pre', 'script', 'style', 'textarea']);

if (!existsSync(dist)) {
  console.error('dist/ does not exist. Run npm run build before npm run check:links.');
  process.exit(1);
}

function walk(directory, extension) {
  const results = [];
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) results.push(...walk(path, extension));
    else if (!extension || extname(entry.name) === extension) results.push(path);
  }
  return results;
}

function candidatesFor(path) {
  if (extname(path)) return [path];
  return [path, `${path}.html`, join(path, 'index.html')];
}

function collectUrlAttributes(html) {
  const values = [];

  function visit(node) {
    if (excludedElements.has(node.tagName)) return;
    for (const attribute of node.attrs ?? []) {
      if (attribute.name === 'href' || attribute.name === 'src') values.push(attribute.value);
    }
    for (const child of node.childNodes ?? []) visit(child);
  }

  visit(parse(html));
  return values;
}

const basePath = (process.env.BASE_PATH ?? '').replace(/^\/+|\/+$/g, '');
const htmlFiles = walk(dist, '.html');

for (const htmlFile of htmlFiles) {
  const html = readFileSync(htmlFile, 'utf8');

  for (const original of collectUrlAttributes(html)) {
    if (/^(?:https?:|mailto:|tel:|data:|blob:|javascript:|#)/.test(original)) continue;

    let value = original.split('#')[0].split('?')[0];
    if (!value) continue;
    try {
      value = decodeURIComponent(value);
    } catch {
      errors.push(`${htmlFile}: invalid encoded URL ${original}`);
      continue;
    }

    if (basePath && value.startsWith(`/${basePath}/`)) value = value.slice(basePath.length + 1);
    const target = value.startsWith('/') ? resolve(dist, `.${value}`) : resolve(dirname(htmlFile), value);
    const normalizedDist = `${normalize(dist)}${sep}`;
    if (
      !`${normalize(target)}${target.endsWith(sep) ? '' : sep}`.startsWith(normalizedDist) &&
      normalize(target) !== normalize(dist)
    ) {
      errors.push(`${htmlFile}: link escapes dist: ${original}`);
      continue;
    }

    const exists = candidatesFor(target).some((candidate) => {
      if (!existsSync(candidate)) return false;
      return statSync(candidate).isFile() || existsSync(join(candidate, 'index.html'));
    });
    if (!exists) errors.push(`${htmlFile}: missing target ${original}`);
  }
}

if (errors.length > 0) {
  console.error(`Link validation failed with ${errors.length} issue(s):`);
  for (const error of errors) console.error(`- ${error}`);
  process.exit(1);
}

console.log(`Link validation passed across ${htmlFiles.length} HTML files.`);
