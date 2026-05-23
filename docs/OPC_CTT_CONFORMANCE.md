# OPC Compliance Test Tool (CTT) conformance — Phase 14E

The OPC Compliance Test Tool is the OPC Foundation's official conformance test suite. Running it against a managed server is the gold-standard acceptance test before claiming OPC DA / AE / HDA spec compliance.

## Prerequisites

1. **OPC Foundation membership** — required to download the CTT.
   https://opcfoundation.org/membership/

2. **CTT installer URL** — added as the `OPC_CTT_INSTALLER_URL` secret on the repo. Workflow dispatch can also override with a one-shot URL.

3. **CTT-compliant managed server** — `samples\Opc.Classic.Samples.CttServer\` exposes a managed `OpcDaServerHost`-hosted DA server. The renamed CttServer registers the ProgID `Opc.Classic.DaSample.1` and CLSID `{8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A}`.

## Triggering

- **Manual**: GitHub → Actions → "OPC CTT conformance" → Run workflow
- **Scheduled**: weekly on Sunday at 03:00 UTC (only runs if secret is set)

## Results

CTT produces an XML report; the workflow uploads it as the `opc-ctt-results` artifact. Pass/fail per-test is the per-OPC-spec acceptance criterion.

## Status

The workflow and managed sample-server scaffold are present, but the CTT gate is **externally blocked** today by OPC Foundation membership and the `OPC_CTT_INSTALLER_URL` secret. The current target ProgID for the CTT server is `Opc.Classic.DaSample.1`.

When this turns green, it gates the 1.0.0 release: a fully CTT-passing managed server is the necessary (though not sufficient) condition for releasing.