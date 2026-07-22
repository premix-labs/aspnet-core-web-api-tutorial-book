# Security Policy

## Supported Versions

Security fixes are provided for the latest released major version of the template. Generated books must record their installed template version in `.book-template.json`.

## Reporting A Vulnerability

Use GitHub's private vulnerability reporting for this repository. Do not open a public issue and do not include working credentials, personal data or production secrets in test cases.

Include:

- affected version and component
- reproducible impact
- minimal proof of concept using non-sensitive data
- suggested mitigation when known

The maintainer targets acknowledgement within two business days, severity assessment within five business days and coordinated status updates at least weekly until resolution. These are open-source response targets, not a contractual SLA.

## Repository Rules

- Secrets are never committed to source, examples, screenshots or generated output.
- Production dependencies fail CI at moderate or higher audit severity.
- Official GitHub Actions are pinned to full commit SHAs.
- Dependency changes receive automated review.
- Security-sensitive curriculum distinguishes demonstration shortcuts from production practice.
