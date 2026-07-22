# Operations Runbook

## Purpose

Use this runbook when publishing, diagnosing or restoring the production book site. Record the commit, workflow run and result in `validation-report.md`.

## Ownership

| Role                 | Responsibility                                                   |
| -------------------- | ---------------------------------------------------------------- |
| Incident lead        | classifies severity, coordinates recovery and owns the timeline  |
| Technical responder  | diagnoses the build, dependency, configuration or Pages failure  |
| Communications owner | posts status updates and records user impact                     |
| Release approver     | confirms recovery evidence and authorizes production restoration |

One person may hold multiple roles in a small project, but the incident record must name the acting owner for each role.

## Severity And Recovery Targets

| Severity | Definition                                                                          | Restore target (RTO) | Update cadence      |
| -------- | ----------------------------------------------------------------------------------- | -------------------- | ------------------- |
| SEV-1    | production is unavailable, compromised or materially unsafe                         | 60 minutes           | every 30 minutes    |
| SEV-2    | a core reading, search or navigation path is broken without a reasonable workaround | 4 hours              | every 2 hours       |
| SEV-3    | limited defect with a viable workaround and no security impact                      | next planned release | when status changes |

The recovery point objective (RPO) is the latest immutable tag or commit that passed the recorded enterprise release gate. The site is static and stores no reader data, so rollback must not invent or reconstruct mutable production state.

## Normal Deployment

1. Merge a reviewed pull request after all required checks pass.
2. Confirm **Deploy to GitHub Pages** completes for the merged commit.
3. Open the home page, one direct chapter URL and search from the deployed site.
4. For a release, create the matching `vX.Y.Z` tag and confirm the GitHub release contains `site.tar.gz`, `sbom.cdx.json`, `checksums.sha256` and provenance attestations.

## Incident Triage

1. Check GitHub Actions for the first failing job and preserve its logs.
2. Determine whether the fault is content, build, dependency, Pages configuration or availability.
3. Stop concurrent production changes until the failure is understood.
4. If the current deployment is unusable, restore the last known-good tag using the rollback procedure.
5. Assign the incident roles, severity, next update time and recovery target in the incident record.

## Rollback

1. Identify a previously validated tag or immutable commit SHA.
2. Open **Actions > Roll Back GitHub Pages > Run workflow**.
3. Enter the approved tag or commit in `ref` and run the workflow.
4. Confirm the workflow rebuilds and verifies that exact revision before deployment.
5. Smoke-test home, direct chapter navigation, search and assets under the repository base path.
6. Record the incident, restored revision and follow-up fix in the validation report and changelog.

## Recovery Verification

- GitHub Pages returns HTTP 200 for home and a chapter URL.
- Search opens and returns indexed content.
- CSS, fonts and icons load under the configured base path.
- The deployed footer/version identifies the expected release where applicable.
- A forward-fix pull request includes a regression test for the incident when practical.

## Escalation And Ownership

Repository CODEOWNERS approve platform changes. Security reports follow `SECURITY.md`. If GitHub Pages itself is unavailable, preserve evidence and monitor GitHub Status; do not bypass verification by uploading unreviewed artifacts.

## Recovery Drill

Run a non-production rollback drill at least quarterly and after material deployment-workflow changes. Use an immutable known-good tag, exercise the rollback workflow without replacing a healthy production deployment where possible, verify the recovery checklist, and record elapsed time, gaps, owners and follow-up dates in `validation-report.md`.

## Incident Record

Record at minimum: start and detection time, impact, severity, acting owners, affected revision, evidence links, decisions, status updates, recovery revision, RTO result, root cause, corrective actions and due dates. Do not place credentials, private reports or personal data in a public record.
