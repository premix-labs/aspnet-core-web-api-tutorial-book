# Teaching Principles for 10/10 Tutorial Books

## Contents

- [Goal](#goal)
- [Definition of Done](#definition-of-done)
- [Chapter Structure](#chapter-structure)
- [Code Teaching Method](#code-teaching-method)
- [10-Point Rubric](#10-point-rubric)
- [Review Checklist](#review-checklist)
- [Common Failure Patterns](#common-failure-patterns)
- [Verification Gates](#verification-gates)

## Goal

A strong programming tutorial enables learners to build something that runs without reducing the experience to unexplained copying. Before introducing a new concept such as a route, controller, component, service, repository, DTO, validation rule, authentication mechanism, container, deployment target, or configuration option, explain:

- what problem it solves
- what happens without it
- how it is used in professional work
- which part of it the chapter uses
- what limitations remain in the current implementation

## Definition of Done

A chapter must be complete within the book. Readers should not need to inspect `examples/` to discover what to write, where to write it, or how to verify it. Examples are the source of truth for authors and reviewers, buildable verification targets, and optional references after the lesson.

A chapter is ready to teach only when it meets every requirement:

- It has a clear chapter outcome.
- It states prerequisites before the first step.
- It lists the files that will change.
- It explains new terms, methods, attributes, annotations, hooks, packages, and configuration before use.
- It provides copyable commands when creating folders or files.
- It provides Windows PowerShell and macOS/Linux Bash commands when syntax differs.
- It divides code into steps instead of presenting an unexplained complete file.
- Code blocks remain near 30 lines unless a longer block is necessary and justified.
- It provides run, test, or build commands.
- It states observable expected results such as a status code, response body, log entry, file, or screen.
- It covers common failures or diagnostic guidance.
- It includes a checkpoint before the next chapter.
- It matches the progressive project at that point in the book.
- The example, final, or validation project builds or tests within the chapter's scope.

## Chapter Structure

Hands-on chapters should use this sequence:

1. What the chapter will accomplish
2. Starting prerequisites and state
3. Concepts used in the chapter
4. Files changed by the chapter
5. A flow or diagram when it improves understanding
6. Step 1: create folders or files
7. Step 2: add a small section of code
8. Step 3: connect it to a service, configuration, route, or UI
9. Step 4: verify behavior
10. Expected result
11. Common failures
12. Checkpoint

Concept-only chapters do not need every heading, but still require a clear outcome, examples, and a checkpoint.

## Code Teaching Method

Teach code in layers:

1. Create the folder.
2. Create the file.
3. Add imports or using directives.
4. Add a class, function, component, record, or interface.
5. Add properties, fields, schemas, or types.
6. Add constructors, dependencies, hooks, or helpers.
7. Add behavior incrementally.
8. Connect it to dependency injection, configuration, routing, controllers, UI, or an API client.
9. Verify the result.

Do not ask learners to insert a large file without explanation. Split long code into focused blocks and explain the important decisions between them.

## 10-Point Rubric

| Category                                                  | Score |
| --------------------------------------------------------- | ----: |
| Clear outcome and context                                 |     1 |
| Concepts, methods, and configuration explained before use |     2 |
| Detailed steps without unexplained jumps                  |     2 |
| Code divided into readable sections                       |     1 |
| Explicit verification commands and expected results       |   1.5 |
| Common failures and checkpoints support self-diagnosis    |     1 |
| Examples match the chapter and genuinely build or test    |   1.5 |

Interpret scores consistently:

- 10/10: a new learner can complete the work, understand the reasoning, and verify it independently
- 8/10: the content is correct, but some steps still require inference
- 6/10: code and explanation exist, but the lesson still behaves like a copy exercise
- below 6/10: the chapter risks confusing learners or teaching an incorrect mental model

## Review Checklist

Use this list while reviewing each chapter:

- Is the outcome explicit?
- Does the learner know which file to open?
- Are folder and file creation commands complete?
- Are Windows and macOS/Linux commands included when needed?
- Is every new term explained before use?
- Are any code blocks unnecessarily long?
- When code is removed or changed, is the resulting state clear?
- Do routes, ports, filenames, and project names match the implementation?
- Are expected responses, status codes, screens, and logs accurate?
- Is the checkpoint observable rather than vague?
- Does demo guidance clearly differ from production guidance where security matters?
- Does the progressive project compile or test after the chapter?
- Does the final project agree with the chapter, or document a justified difference?

## Common Failure Patterns

Treat these as issues that require revision:

- presenting a long complete file without explanation
- saying "add this code" without naming the location
- using a class, component, hook, helper, or field before creating it
- using a package without an installation command
- teaching commands for one operating system without stating the limitation
- showing a response or screen that does not match the code
- documenting a port that disagrees with actual development settings
- using a checkpoint that cannot be verified
- omitting common warnings or errors readers are likely to encounter
- omitting production warnings for secrets, permissive CORS, sensitive logging, or token storage

## Verification Gates

Before declaring an audit complete, verify the applicable scope:

- book or documentation changes: `npm run build`
- example changes: run the relevant build or test command
- Docker or Compose changes: `docker compose config`
- dependency changes: run install, build, and audit commands for the package manager
- behavior changes: update the README, validation report, API contract, QA notes, migration notes, and expected responses

Do not call the work complete without evidence appropriate to the change.
