# Security Policy

The opc-classic project implements protocol, authentication, and marshalling code that may run in industrial environments. Security reports are handled privately first so maintainers can assess impact before public disclosure.

## Supported Versions

This project is still pre-1.0 and has no stable released version line. During the pre-1.0 period, security support is best-effort for the default branch and any current prerelease packages derived from it.

| Version | Supported |
| --- | --- |
| Default branch pre-1.0 commits | Yes |
| Current pre-1.0 prerelease packages | Yes, best effort |
| Older pre-1.0 snapshots | Best effort |
| 1.0.0 and later | Not released yet |

When 1.0.0 is released, this table will be updated with explicit supported release lines.

## Reporting a Vulnerability

Please report suspected vulnerabilities privately through GitHub Security Advisories:

<https://github.com/marcschier/opc-classic/security/advisories/new>

Do not use public GitHub issues, pull requests, discussions, or social media to report a vulnerability before maintainers have triaged it. Public reports may put downstream users at risk before a fix or mitigation is available.

A useful report includes:

- Affected commit, branch, or package version.
- A clear description of the issue and affected component.
- Reproduction steps or a minimal proof of concept, when safe to share.
- Expected impact, including confidentiality, integrity, availability, or authentication consequences.
- Any known mitigations or configuration workarounds.

Please avoid destructive proof-of-concept payloads, attacks against third-party systems, or disclosure of data that you are not authorized to access.

## Security-sensitive Surface

The project is a pure-managed OPC Classic stack with DCOM and MSRPC components under `src\Opc.Classic.Dcom\`. Its security-sensitive surface includes authentication, authorization, channel protection, NDR marshalling, and server callback dispatch.

The active roadmap includes NTLMv2, Kerberos/SPNEGO, and DCOM authentication hardening across Phase 2 and Phase 3. Findings against those areas are welcome, including protocol downgrade issues, replay weaknesses, signing or sealing mistakes, credential handling bugs, and interoperability behavior that weakens expected DCOM security.

Cryptanalysis findings against the in-tree authentication implementations are especially valuable. Please include test vectors, traces, or references to protocol specifications when available.

## In-Tree Cryptographic Implementations

The project intentionally contains in-tree cryptographic code for OPC Classic interoperability. NTLMv2, RC4, and MD4 are hand-rolled in `src\Opc.Classic.Dcom\Crypto\` as part of Phase 2E rather than delegated entirely to BCL-backed implementations.

This code exists to support protocol compatibility, not to introduce new cryptographic designs. Responsible disclosure is particularly important for:

- Incorrect MD4 or RC4 test-vector behavior.
- NTLMv2 response, key exchange, signing, or sealing mistakes.
- Nonce, timestamp, session key, or challenge handling errors.
- Differences from Windows, MSRPC, DCOM, or NTLM protocol behavior that reduce security.
- Timing, allocation, or state-reuse issues that could expose secrets.

Where possible, include deterministic vectors that can become regression tests under `tests\`.

## Response SLA

opc-classic is a pre-1.0 project maintained on a best-effort basis. The maintainers will try to acknowledge private reports promptly, but no fixed response or remediation SLA is currently guaranteed.

After triage, maintainers may coordinate with the reporter on severity, affected versions, mitigations, fix timing, and public advisory text. Credit will be offered when appropriate and when the reporter wants it.
