# Audit Report Contract

## Required Sections

Use this structure for every scored review:

1. **Scope**: repository, parts, chapters, examples, contracts, and commit or working-tree state reviewed
2. **Evidence**: files inspected and commands run, including commands that could not run
3. **Score**: rubric category scores, raw total, applicable hard cap, and final score
4. **Findings**: severity, stable rule ID when available, location, impact, and required correction
5. **Verification**: command, result, environment, and relevant observed output
6. **Decision**: release-ready, needs revision, or blocked, with remaining risk

## Score Table

```markdown
| Category                         | Score | Maximum | Evidence            |
| -------------------------------- | ----: | ------: | ------------------- |
| Outcome and context              |       |     1.0 | `path:line`         |
| Concepts before use              |       |     2.0 | `path:line`         |
| Complete steps                   |       |     2.0 | `path:line`         |
| Readable, located code           |       |     1.0 | `path:line`         |
| Verification and expected result |       |     1.5 | command/output      |
| Failures and checkpoint          |       |     1.0 | `path:line`         |
| Example synchronization          |       |     1.5 | build/test evidence |
```

State the calculation explicitly:

```text
Raw score: 9.50
Hard cap: 8.00 - required verification could not be run
Final score: 8.00/10
```

## Finding Shape

Each finding must include:

- severity: Critical, High, Medium, or Low
- rule ID: use the automated rule ID when one exists; otherwise use `MANUAL`
- location: a clickable file and line reference when possible
- evidence: the exact mismatch or missing requirement
- impact: what the learner, maintainer, or production system risks
- correction: the smallest concrete change that resolves the issue

Do not report a vague finding such as "needs more explanation" without naming the concept, location, and missing explanation.

## Verification Status

Use only these statuses:

- `Passed`: the command ran successfully for the reviewed state
- `Failed`: the command ran and failed
- `Not run`: the command was available but outside the approved scope
- `Blocked`: the command could not run because of a stated environmental or repository condition
- `Not applicable`: the check does not apply, with a reason

Never convert `Not run` or `Blocked` into `Passed`. Apply the scoring hard cap when required evidence is missing.

## Fix Report

After editing, report:

- files changed and behavior affected
- findings resolved and findings still open
- verification commands and results
- final score only when the full scored scope was re-audited

Do not claim `10/10` after a partial fix unless every release condition in the scoring policy has been rechecked.
