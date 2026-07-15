# Security Policy

## Supported versions

This project is under active development. Security fixes are applied to the latest commit on the `main` branch; older commits, forks, and deployments are not supported.

## Reporting a vulnerability

Do not disclose a suspected vulnerability in a public issue, discussion, pull request, or social channel.

Use [GitHub private vulnerability reporting](https://github.com/QuinntyneBrown/Liturgy/security/advisories/new) to send the maintainers:

- A description of the vulnerability and its potential impact
- The affected endpoint, handler, dependency, or configuration
- Reproduction steps or a minimal proof of concept
- Any suggested mitigation, if known

Do not include real user information, credentials, signing keys, or connection strings in a report. Use synthetic data and redact sensitive logs.

Maintainers will acknowledge a report as soon as practical, investigate it, and coordinate remediation and disclosure with the reporter. Response times are best effort because this is a volunteer-maintained project.

## Security considerations

Liturgy is an early-stage project and has not completed an independent production security or privacy assessment. In particular:

- The development `Jwt:SigningKey` in `appsettings.json` is a placeholder and must be replaced with a strong secret supplied through user secrets or environment configuration before any non-local use.
- The development CORS policy and seeded demo credentials are intended for local development only.

Deployers are responsible for reviewing authentication, authorization, secret management, dependency risk, transport security, data residency, logging, and backups before handling real data.
