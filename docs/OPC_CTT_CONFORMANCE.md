# OPC Compliance Test Tool (CTT) conformance

The OPC Compliance Test Tool is the OPC Foundation conformance suite for OPC Classic servers. For this repository, the CTT gate validates the managed OPC DA sample server before a release is promoted.

## Scope

- Target server: `samples\Opc.Classic.Samples.CttServer\`.
- Target ProgID: `Opc.Classic.DaSample.1`.
- Target CLSID: `{8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A}`.
- CI workflow: `.github\workflows\opc-ctt.yml`.
- Acceptance artifact: the CTT XML report uploaded as `opc-ctt-results`.

## Prerequisites

1. **OPC Foundation membership** to download the CTT installer.
2. **Installer URL** supplied either through the `OPC_CTT_INSTALLER_URL` repository secret or the workflow dispatch input.
3. **Windows runner** capable of installing CTT, building the sample server, and running the registered server process.
4. **Release build** of `Opc.Classic.slnx` and the managed CTT sample server.

## Running in CI

- Manual run: GitHub -> Actions -> "OPC CTT conformance" -> Run workflow.
- Scheduled run: weekly on Sunday at 03:00 UTC.
- The workflow resolves the installer URL, builds the solution, starts `Opc.Classic.Samples.CttServer`, and invokes CTT against `Opc.Classic.DaSample.1`.
- If no installer URL is available, the workflow records a warning and skips the conformance run; a skipped run is not a passing CTT result.

## Results and release gate

CTT pass/fail is evaluated per OPC Foundation test in `ctt-results.xml`. A release candidate requires a completed CTT run with no failing conformance tests and no infrastructure failure. Keep the XML report with the release artifacts for auditability.

Local runs should target the same ProgID and CLSID pair as CI so reports remain comparable.

CTT is a required gate for `1.0.0-rc.1` and later stable releases.
