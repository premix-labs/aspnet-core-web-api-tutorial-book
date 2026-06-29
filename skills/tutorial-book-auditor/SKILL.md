---
name: tutorial-book-auditor
description: Audit and improve programming tutorial books and companion example projects for step-by-step teaching quality. Use when reviewing, scoring, or editing chapters, README/docs, validation reports, progressive/final examples, or when asked to make ASP.NET Core, Spring Boot, Web API, admin, auth, Docker, testing, deployment, production-grade, or 10/10 tutorial content clearer and easier to follow.
---

# Tutorial Book Auditor

## Required Reference

Read `references/teaching-principles.md` before scoring or rewriting chapters. It contains the rubric, chapter checklist, common failure patterns, and verification gates.

## Workflow

1. Resolve the exact scope: repository, part, chapter, example project, or report.
2. Inspect source files with `rg`/file reads. Do not rely on memory when files are available.
3. Compare the chapter against the progressive example at the same learning point and the final example when relevant.
4. Score with the 10-point rubric from the reference file.
5. Lead with concrete blockers: missing explanation, skipped folder/file setup, long code blocks, wrong ports/routes, missing expected results, or unsynced examples.
6. If the user asks to fix, edit the smallest set of files that makes the lesson easier and correct.
7. Verify with commands that match the touched scope.

## Editing Rules

- Explain a new concept before asking the learner to use it.
- Split code into small, teachable blocks instead of replacing whole files with long code.
- Include folder/file creation commands before a lesson creates new files.
- Include Windows PowerShell and macOS/Linux Bash commands when the action differs by OS.
- Keep ports, routes, project names, filenames, and expected responses aligned with the actual example project.
- Update docs, examples, README, validation report, and `.http`/Postman files together when behavior changes.
- Preserve the progressive learning path. Do not introduce future-chapter fields, classes, packages, or security behavior without an explanation.
- Make chapters self-contained. A learner should not need to open example source files to understand what to write, where to write it, or how to verify it.
- Use example projects as source of truth for correctness and optional reference only. If an example path is mentioned, the chapter still needs to include the relevant teaching content.

## Language Notes

- For ASP.NET Core lessons, check attributes, `using` statements, dependency injection registration, DTOs, `.http` files, `dotnet` commands, launch ports, and test projects.
- For Spring Boot lessons, check annotations, imports, packages, Gradle commands, profiles, request examples, and test projects.
- For production-grade claims, check secrets, auth/session behavior, migrations, tests, Docker, health checks, logging, and deployment notes.

## Verification

Run only commands relevant to the files touched:

- Book/docs changes: `npm run build`.
- ASP.NET example changes: relevant `dotnet test`, and `dotnet publish` when deployment/runtime behavior changes.
- Spring Boot example changes: relevant Gradle tests.
- Docker changes: `docker compose config` in the touched compose directory.

If a command cannot be run, report the reason directly.

## Response Shape

For reviews, return score, findings by chapter/file, and the next edits needed. For fixes, return changed files and verification results. Keep the answer concise and grounded in file paths.
