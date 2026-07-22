# Automated Audit Rules

The deterministic auditor reports objective signals. Manual review remains responsible for context, teaching quality, rubric scores, and final severity confirmation.

| Rule    | Default severity                  | Signal                                                               |
| ------- | --------------------------------- | -------------------------------------------------------------------- |
| `TB001` | High for published, Low otherwise | Unresolved TODO, FIXME, or TBD marker                                |
| `TB002` | Medium                            | Fenced code block has no language identifier                         |
| `TB003` | Medium                            | Code block exceeds 50 lines                                          |
| `TB004` | High                              | Fenced code block is not closed                                      |
| `TB005` | High                              | Machine-specific absolute path appears in learner content            |
| `TB006` | Critical                          | Credential-like literal appears in learner content                   |
| `TB007` | Medium                            | Published chapter has no second-level headings                       |
| `TB008` | Medium                            | Published chapter has fewer than 80 prose words                      |
| `TB009` | Medium                            | Hands-on chapter has no detectable verification or expected result   |
| `TB010` | Low                               | Hands-on chapter after the introduction has no detectable checkpoint |

## CLI Contract

```bash
node skills/tutorial-book-auditor/scripts/audit-book.mjs \
  --root . \
  --path src/content/chapters \
  --format json \
  --fail-on high
```

- `--root`: repository root; defaults to the current directory
- `--path`: file or directory relative to root; defaults to `src/content/chapters`
- `--format`: `text` or `json`
- `--fail-on`: `critical`, `high`, `medium`, `low`, or `none`; defaults to `high`

JSON output uses `schemaVersion: 1` and includes the audited target, file count, severity counts, and ordered findings. Exit code `1` means findings met the selected threshold. Exit code `2` means the command or arguments were invalid.

## Limitations

The rules use conservative heuristics and do not prove pedagogical correctness. Unicode prose length uses `Intl.Segmenter` so languages without whitespace word boundaries, including Thai, are not penalized by an English-only word counter. Review every finding in context before editing or assigning a score.
