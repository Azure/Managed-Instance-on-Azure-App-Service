# Security policy

## Reporting a vulnerability

Please report vulnerabilities privately through GitHub Security Advisories for this repository. Do not open a public issue for an unpatched vulnerability.

Include affected versions, reproduction steps, impact, and any suggested remediation. Maintainers will acknowledge reports as soon as practical and coordinate disclosure after a fix is available.

## Supported versions

Only the latest commit on the default branch is supported.

## Secrets

Do not commit credentials, deployment parameter files, publish profiles, database files, or production resource identifiers. Configure `ApiKey`, database connections, and MCP settings through protected deployment settings or a secret store.
