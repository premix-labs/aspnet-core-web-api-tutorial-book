# Governance

## Roles

- Maintainers own repository policy, releases, security response and template architecture.
- CODEOWNERS review changes in their assigned paths.
- Contributors propose focused changes with reproducible evidence.

## Decision Process

Routine fixes use a reviewed pull request. Changes to managed-file ownership, compatibility, security controls, curriculum contracts or release behavior require a decision record and migration notes. Unresolved design decisions remain open rather than being hidden in implementation.

## Merge Policy

Protected `main` requires passing checks, resolved conversations, an approving review and CODEOWNER review for owned paths. The author of the latest material change cannot provide the final approval. Emergency changes use the same checks and receive a documented retrospective.

## Releases

Template releases follow Semantic Versioning. The tag, package, manifest and changelog versions must agree. Release artifacts include the built site, CycloneDX SBOM, SHA-256 checksums and GitHub build-provenance attestations.

## Security And Conduct

Security reports follow `SECURITY.md`. Participation follows `CODE_OF_CONDUCT.md`. Maintainers disclose conflicts of interest and recuse themselves where impartial review is not possible.
