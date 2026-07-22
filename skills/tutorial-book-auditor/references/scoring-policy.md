# Scoring And Severity Policy

## Purpose

Use this policy to keep scores repeatable across chapters, parts, complete books, and reviewers. A score is an evidence-backed release decision, not an impression of writing quality.

## Severity

| Severity | Meaning                                                                                                 | Examples                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| Critical | The lesson can cause a security incident, data loss, or a materially unsafe production decision.        | Real credentials, destructive commands without safeguards, broken authorization guidance              |
| High     | A learner cannot complete or verify the promised outcome, or the lesson contradicts the implementation. | Build failure, missing required file, incorrect route, future code leaking into a progressive example |
| Medium   | The outcome remains possible but requires meaningful inference or teaches an incomplete mental model.   | Concept used before explanation, missing expected result, unexplained long code block                 |
| Low      | Clarity, consistency, or maintainability issue with limited effect on completion.                       | Naming inconsistency, weak transition, minor formatting problem                                       |

Report findings in severity order. Do not reduce severity to protect a target score.

## Ten-Point Rubric

Score every category independently and cite evidence.

| Category                                                      | Maximum |
| ------------------------------------------------------------- | ------: |
| Outcome and context are explicit                              |     1.0 |
| Concepts, methods, and configuration are explained before use |     2.0 |
| Steps are complete and do not make unexplained jumps          |     2.0 |
| Code is divided into readable, located sections               |     1.0 |
| Verification commands and expected results are explicit       |     1.5 |
| Common failures and checkpoints support self-diagnosis        |     1.0 |
| Examples match the chapter and genuinely build or test        |     1.5 |

Use increments of `0.25`. Do not award points without file, line, command, or observed-behavior evidence.

## Hard Caps

Apply the lowest applicable cap after calculating the rubric total:

| Condition                                                        | Maximum score |
| ---------------------------------------------------------------- | ------------: |
| Open Critical finding                                            |           3.0 |
| Open High finding                                                |           6.0 |
| Required build or test fails                                     |           6.0 |
| Promised behavior is not implemented or examples are out of sync |           6.0 |
| No runnable verification evidence for a hands-on chapter         |           7.0 |
| Missing expected result or learner checkpoint                    |           8.0 |
| Required verification could not be run                           |           8.0 |
| Open Medium finding                                              |           9.0 |

A chapter may receive `10.0` only when:

- there are no open Critical, High, or Medium findings
- every rubric category has direct evidence
- relevant build, test, or behavior checks pass
- the progressive example matches the chapter state
- remaining manual checks and Low findings are disclosed

## Release Decisions

| Score     | Decision                                        |
| --------- | ----------------------------------------------- |
| 10.0      | Release-ready for the reviewed scope            |
| 9.0-9.75  | Strong, but revision or evidence remains        |
| 7.0-8.75  | Usable with material teaching gaps              |
| 5.0-6.75  | Not ready; completion or correctness is at risk |
| Below 5.0 | Unsafe or fundamentally incomplete              |

Never average chapter scores to hide a blocking finding. A part or whole-book decision inherits the most severe unresolved issue in its scope.
