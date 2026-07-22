---
name: tutorial-book-auditor
description: Audit, score, and improve technical tutorial books and companion examples with evidence-backed teaching, implementation, and release checks. Use for chapter or full-book reviews, 10/10 scoring, curriculum and documentation audits, progressive/final example synchronization, production-readiness, validation reports, API contracts, and browser, accessibility, security, or release QA.
---

# Tutorial Book Auditor

## Required References

Read references according to the task:

- `references/teaching-principles.md`: always read before reviewing or rewriting a chapter
- `references/automated-rules.md`: read before interpreting CLI findings or changing audit thresholds
- `references/scoring-policy.md`: read before assigning a score or release decision
- `references/report-contract.md`: read before returning a scored audit or fix report

When the task touches planning, structure, examples, behavior, compatibility, or release quality, also inspect the relevant files in `docs/internal/`:

- `README.md`
- `book-plan.md`
- `api-contract.md`
- `final-project-structure.md`
- `style-guide.md`
- `release-checklist.md`
- `validation-report.md`

## Workflow

1. Resolve the exact scope: repository, part, chapter, example project, report, QA checklist, or release gate.
2. Inspect source files with `rg`/file reads. Do not rely on memory when files are available.
3. Compare the chapter against the progressive example at the same learning point and the final example when relevant.
4. Check that the work follows `docs/internal/README.md` and the book-specific planning docs.
5. Run `node skills/tutorial-book-auditor/scripts/audit-book.mjs --root .` when the script exists. Treat its findings as evidence, not a substitute for manual review.
6. Classify findings with the shared severity policy and lead with concrete blockers.
7. Score each rubric category from direct evidence, then apply every relevant hard cap.
8. If the user asks to fix, edit the smallest coherent set of files that resolves the behavior and documentation together.
9. Verify with commands that match the touched scope and record commands that fail or cannot run.
10. Return the report shape defined in `references/report-contract.md`.

## Editing Rules

- Explain a new concept before asking the learner to use it.
- Split code into small, teachable blocks instead of replacing whole files with long code.
- Include folder/file creation commands before a lesson creates new files.
- Include Windows PowerShell and macOS/Linux Bash commands when the action differs by OS.
- Keep ports, routes, project names, filenames, commands, expected responses, and expected screens aligned with the actual example project.
- Preserve the progressive learning path. Do not introduce future-chapter files, helpers, fields, packages, routes, or security behavior without an explanation.
- Make chapters self-contained. A learner should not need to open example source files to understand what to write, where to write it, or how to verify it.
- Treat example projects as source of truth for verification and optional reference only.
- Update docs, examples, README, validation report, API contract notes, QA notes, and release checklist together when behavior changes.
- Never award `10/10` while a required command is unverified or a Critical, High, or Medium finding remains open.

## Book Standard Checks

- `docs/internal/README.md` defines the standard structure.
- `book-plan.md` defines audience, scope, chapter map, and final outcome.
- `api-contract.md` defines API behavior when the book uses an API; write `Not applicable` only when the book truly has no API dependency.
- `final-project-structure.md` must match the final example project.
- `style-guide.md` controls language, code block shape, commands, and naming.
- `validation-report.md` must record the latest meaningful build/test/review result.
- `qa/` checklists must be updated when browser, accessibility, or security behavior changes.

## Verification

Run only commands relevant to the files touched:

- Automated book audit: `npm run audit:book` or the direct skill script.
- Book/docs changes in a scaffolded site: `npm run build`.
- Example project changes: run the build/test command defined by that example.
- Browser/UI changes: run available browser/e2e checks, or inspect the rendered app when available.
- Docker/Compose changes: run `docker compose config` in the touched compose directory.
- Security/dependency changes: run the relevant audit command when the package manager supports it.

If a command cannot be run because the copied book has not been scaffolded yet, report that directly instead of pretending validation happened.

## Reporting

Follow `references/report-contract.md`. Keep findings ordered by severity, cite file locations, distinguish automated findings from manual judgment, and state remaining risk. Do not average scores in a way that hides a blocking chapter or failed example.
