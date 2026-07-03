# Agent Instructions

## Scope

These instructions apply to this repository.

## Book Workflow

- Use `skills/tutorial-book-auditor` for any request that reviews or edits tutorial content, example projects, README files, validation reports, API contracts, internal planning docs, QA checklists, or teaching quality.
- Read `skills/tutorial-book-auditor/SKILL.md` first. Load `skills/tutorial-book-auditor/references/teaching-principles.md` before scoring or rewriting chapters.
- Keep lessons step by step: explain new ASP.NET Core concepts, attributes, methods, packages, commands, and configuration before using them.
- Keep progressive examples aligned with the chapter state and final examples aligned with production-grade reference behavior.
- Keep book chapters self-contained. Learners should be able to understand and complete a chapter from the book without opening example source files.
- Treat example projects as source of truth for verification and optional reference, not as required reading for the learner.
- If a chapter references an example file, include the relevant explanation, commands, code shape, and checks in the chapter itself.
- Avoid long pasted code blocks in chapters when a smaller sequence with explanation is possible.
- When creating folders or files in lessons, include copyable Windows PowerShell and macOS/Linux Bash commands when useful.

## Documentation Standard

- Keep internal planning and quality documents under `docs/internal`.
- Follow `docs/internal/README.md` for the shared `Book Documentation Standard v1`.
- Keep `docs/internal/api-contract.md`, `docs/internal/final-project-structure.md`, and `docs/internal/validation-report.md` in sync when API behavior or examples change.
- Update `docs/internal/validation-report.md` after validation or when a validation blocker is discovered.

## Verification

- Run `npm run build` after docs, navigation, or frontmatter changes.
- Run relevant `dotnet test` projects when ASP.NET example code changes.
- Run `dotnet publish` for touched runnable APIs when publishing or deployment behavior changes.
- Run `docker compose config` in touched compose directories when Docker Compose changes.
- Report commands that were run and any command that could not be run.

## Git

- Do not commit or push unless explicitly asked.
- Do not revert user changes unless explicitly asked.
