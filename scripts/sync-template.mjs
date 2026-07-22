#!/usr/bin/env node

import { createHash } from 'node:crypto';
import { copyFileSync, existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const root = resolve(import.meta.dirname, '..');

function parseOptions(args) {
  const options = { mode: 'status', force: false };
  for (let index = 0; index < args.length; index += 1) {
    const argument = args[index];
    if (argument === '--status') options.mode = 'status';
    else if (argument === '--check') options.mode = 'check';
    else if (argument === '--apply') options.mode = 'apply';
    else if (argument === '--force') options.force = true;
    else if (argument === '--from') {
      const value = args[index + 1];
      if (!value || value.startsWith('--')) throw new Error('--from requires a template directory.');
      options.source = resolve(value);
      index += 1;
    } else throw new Error(`Unknown option: ${argument}`);
  }
  return options;
}

function readJson(path) {
  return JSON.parse(readFileSync(path, 'utf8'));
}

function sha256(path) {
  return createHash('sha256').update(readFileSync(path)).digest('hex');
}

function compareVersions(left, right) {
  const a = left.split('.').map(Number);
  const b = right.split('.').map(Number);
  for (let index = 0; index < 3; index += 1) {
    if (a[index] !== b[index]) return a[index] - b[index];
  }
  return 0;
}

function managedFileState(targetRoot, metadata) {
  const drift = [];
  for (const [file, expectedHash] of Object.entries(metadata.managedFiles ?? {})) {
    const target = join(targetRoot, file);
    if (!existsSync(target)) drift.push({ file, state: 'missing' });
    else if (sha256(target) !== expectedHash) drift.push({ file, state: 'modified' });
  }
  return drift;
}

function sourceRequired(options, targetRoot, current) {
  const value = options.source ?? process.env.BOOK_TEMPLATE_PATH;
  if (value) return resolve(value);

  const isSourceTemplate =
    existsSync(join(targetRoot, '.template-manifest.json')) && Object.keys(current.managedFiles ?? {}).length === 0;
  if (isSourceTemplate) return targetRoot;

  throw new Error('Set BOOK_TEMPLATE_PATH or pass --from <template-directory>.');
}

function updatePackage(targetRoot, sourceRoot, manifest) {
  const targetPath = join(targetRoot, 'package.json');
  const sourcePackage = readJson(join(sourceRoot, 'package.json'));
  const targetPackage = readJson(targetPath);

  targetPackage.scripts ??= {};
  for (const name of manifest.managedScripts ?? []) {
    if (sourcePackage.scripts?.[name]) targetPackage.scripts[name] = sourcePackage.scripts[name];
  }

  for (const name of manifest.managedDependencies ?? []) {
    for (const section of ['dependencies', 'devDependencies']) {
      if (!sourcePackage[section]?.[name]) continue;
      targetPackage.dependencies && delete targetPackage.dependencies[name];
      targetPackage.devDependencies && delete targetPackage.devDependencies[name];
      targetPackage[section] ??= {};
      targetPackage[section][name] = sourcePackage[section][name];
    }
  }

  writeFileSync(targetPath, `${JSON.stringify(targetPackage, null, 2)}\n`, 'utf8');
}

function applyUpdate(targetRoot, sourceRoot, current, manifest, force) {
  const conflicts = [];
  const nextFiles = new Set(manifest.managedFiles);

  for (const file of manifest.managedFiles) {
    const target = join(targetRoot, file);
    const baseline = current.managedFiles?.[file];
    const source = join(sourceRoot, file);
    if (!existsSync(source)) throw new Error(`Template manifest references a missing file: ${file}`);
    if (existsSync(target) && baseline && sha256(target) !== baseline && sha256(target) !== sha256(source))
      conflicts.push(file);
    if (existsSync(target) && !baseline && sha256(target) !== sha256(source)) conflicts.push(file);
  }

  for (const file of Object.keys(current.managedFiles ?? {})) {
    if (nextFiles.has(file)) continue;
    const target = join(targetRoot, file);
    if (existsSync(target) && sha256(target) !== current.managedFiles[file]) conflicts.push(file);
  }

  if (conflicts.length > 0 && !force) {
    throw new Error(
      `Refusing to overwrite locally modified managed files:\n${conflicts.map((file) => `- ${file}`).join('\n')}`
    );
  }

  for (const file of manifest.managedFiles) {
    const source = join(sourceRoot, file);
    const target = join(targetRoot, file);
    mkdirSync(dirname(target), { recursive: true });
    copyFileSync(source, target);
  }

  for (const file of Object.keys(current.managedFiles ?? {})) {
    if (!nextFiles.has(file)) rmSync(join(targetRoot, file), { force: true });
  }

  updatePackage(targetRoot, sourceRoot, manifest);
  const managedFiles = Object.fromEntries(manifest.managedFiles.map((file) => [file, sha256(join(targetRoot, file))]));
  const nextMetadata = {
    ...current,
    schemaVersion: manifest.schemaVersion,
    templateVersion: manifest.templateVersion,
    generatedAt: new Date().toISOString().slice(0, 10),
    managedFiles,
  };
  writeFileSync(join(targetRoot, '.book-template.json'), `${JSON.stringify(nextMetadata, null, 2)}\n`, 'utf8');
  console.log(`Updated ${relative(dirname(targetRoot), targetRoot)} to template ${manifest.templateVersion}.`);
  console.log('Run npm install and npm run verify:enterprise before committing.');
}

export function syncTemplate(options, targetRoot = root) {
  const metadataPath = join(targetRoot, '.book-template.json');
  if (!existsSync(metadataPath))
    throw new Error('Missing .book-template.json. This repository is not version-tracked.');
  const current = readJson(metadataPath);

  if (options.mode === 'status') {
    const drift = managedFileState(targetRoot, current);
    console.log(`Template version: ${current.templateVersion}`);
    console.log(`Managed files: ${Object.keys(current.managedFiles ?? {}).length}`);
    if (
      existsSync(join(targetRoot, '.template-manifest.json')) &&
      Object.keys(current.managedFiles ?? {}).length === 0
    ) {
      const manifest = readJson(join(targetRoot, '.template-manifest.json'));
      console.log(`Mode: source template (${manifest.managedFiles.length} files in release manifest)`);
    }
    console.log(`Local drift: ${drift.length}`);
    for (const item of drift) console.log(`- ${item.state}: ${item.file}`);
    return { drift };
  }

  const sourceRoot = sourceRequired(options, targetRoot, current);
  const manifest = readJson(join(sourceRoot, '.template-manifest.json'));
  const versionDelta = compareVersions(manifest.templateVersion, current.templateVersion);

  if (options.mode === 'check') {
    const drift = managedFileState(targetRoot, current);
    const updateAvailable = versionDelta > 0;
    console.log(`Installed template: ${current.templateVersion}`);
    console.log(`Available template: ${manifest.templateVersion}`);
    console.log(`Update available: ${updateAvailable ? 'yes' : 'no'}`);
    console.log(`Local managed-file drift: ${drift.length}`);
    if (drift.length > 0) process.exitCode = 1;
    return { drift, updateAvailable };
  }

  if (versionDelta < 0 && !options.force) throw new Error('Refusing to downgrade the template without --force.');
  applyUpdate(targetRoot, sourceRoot, current, manifest, options.force);
  return { updated: true };
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  try {
    syncTemplate(parseOptions(process.argv.slice(2)));
  } catch (error) {
    console.error(error instanceof Error ? error.message : error);
    process.exit(1);
  }
}
