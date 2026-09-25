# Security Policy

Sannr provides model validation without runtime reflection. This document describes how to report a vulnerability
and what response to expect.

## Reporting a vulnerability

**Do not report security vulnerabilities through public GitHub issues, discussions, or pull requests.**

Report vulnerabilities privately through either of the following channels:

- [GitHub private vulnerability reporting](https://github.com/Digvijay/Sannr/security/advisories/new) (preferred)
- Email the maintainer at `security@digvijay.dev`

Please include as much of the following as you can provide:

- The type of issue and the affected component or package
- Full paths of the source files related to the issue
- The affected version, commit, or package version
- Step-by-step instructions to reproduce
- Proof-of-concept or exploit code, if available
- The impact of the issue, including how an attacker might exploit it

### If this project is transferred to Microsoft

This repository is prepared for review by the Microsoft Open Source Programs Office.
If ownership transfers to Microsoft, security reporting moves to the Microsoft Security
Response Center (MSRC) and this policy will be replaced by the standard MSRC policy:

- Report at [https://msrc.microsoft.com/create-report](https://aka.ms/opensource/security/create-report)
- Email [secure@microsoft.com](mailto:secure@microsoft.com), optionally encrypted with the
  [MSRC PGP key](https://aka.ms/opensource/security/pgpkey)
- See the [Microsoft vulnerability disclosure policy](https://aka.ms/opensource/security/cvd)

Until such a transfer occurs, use the maintainer channels above. Do not send reports for this
project to MSRC, because MSRC does not currently own this code.

## Response targets

| Stage | Target |
| --- | --- |
| Acknowledgement of report | 3 business days |
| Initial assessment and severity triage | 10 business days |
| Fix or documented mitigation for High/Critical | 90 days from triage |

These are best-effort targets for an independently maintained project, not a contractual
service-level agreement. See [SUPPORT.md](SUPPORT.md) for the support model.

## Supported versions

Security fixes are applied to the latest released minor version. Older versions are not
patched. See [SUPPORT.md](SUPPORT.md) for the full support and lifecycle statement.

## Disclosure policy

This project follows coordinated disclosure. Issues are disclosed publicly through a GitHub
Security Advisory once a fix or documented mitigation is available. Reporters are credited
unless they ask not to be.

## Security considerations for users

Sannr performs validation and sanitization of externally supplied input. Treat validation failures as untrusted-input events, avoid logging raw rejected payloads that may contain personal data, and review the sanitization attributes applied to security-relevant fields.

Build-time code generation is part of this project's design. Source generators execute inside
the compiler process during build. Only build code you trust, and review generated output when
it participates in a security decision.