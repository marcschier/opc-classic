# OPC Compliance Test Tool (CTT) conformance — Phase 14E

The OPC Compliance Test Tool is the OPC Foundation's official conformance
test suite. Running it against a managed server is the gold-standard
acceptance test before claiming OPC DA / AE / HDA spec compliance.

## Prerequisites

1. **OPC Foundation membership** — required to download the CTT.
   https://opcfoundation.org/membership/

2. **CTT installer URL** — added as the `OPC_CTT_INSTALLER_URL` secret on
   the repo. Workflow dispatch can also override with a one-shot URL.

3. **CTT-compliant managed server** — Phase 14E-followup will add
   `samples/Opc.Classic.Samples.CttServer/` exposing a fully-functional
   `OpcDaServerHost`-hosted server. The server's ProgID is
   `Opc.Classic.DaSample.1`.

## Triggering

- **Manual**: GitHub → Actions → "OPC CTT conformance" → Run workflow
- **Scheduled**: weekly on Sunday at 03:00 UTC (only runs if secret is set)

## Results

CTT produces an XML report; the workflow uploads it as the
`opc-ctt-results` artifact. Pass/fail per-test is the per-OPC-spec
acceptance criterion.

## Status

Workflow is wired but **does not run today** — the secret is unset, and
the managed CTT-compliant server (samples/Opc.Classic.Samples.CttServer/) is not
yet built. Both are Phase 14E follow-ups.

When this turns green, it gates the Phase 16E 1.0.0 release: a fully
CTT-passing managed server is the necessary (though not sufficient)
condition for releasing.
