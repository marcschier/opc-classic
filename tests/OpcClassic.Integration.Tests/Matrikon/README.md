# Matrikon OPC Simulation conformance (Phase 14C)

Tests under this folder target the Matrikon OPC Simulation Server -
the canonical Windows DCOM OPC reference implementation used by every
OPC client developer for interop validation.

## Tag tree

The Matrikon Simulation Server exposes a well-known tag tree:

- `Bucket Brigade.*` - writable tags whose value is whatever the client wrote
- `Random.*` - read-only tags returning a random value of the appropriate type
- `Saw-toothed Waves.*`, `Square Waves.*`, `Triangle Waves.*` - periodic-signal tags
- `Read Error.*`, `Write Error.*` - error-simulator tags (return OPC_E_BADRIGHTS etc.)

See https://www.matrikonopc.com/products/opc-desktop-tools/opc-simulation-server.aspx

## Running

Matrikon Simulation Server is distributed by Matrikon (now Honeywell).
Free download requires registration at matrikonopc.com.

The Phase 14A `windows-conformance` CI job does NOT install Matrikon
(would require a CI secret holding the installer URL). To enable
matrikon-conformance in CI, add a `matrikon-installer-url` secret and
update the workflow to download + install before running the
matrikon-conformance test filter.

Locally on Windows, install Matrikon, then:
```bash
dotnet test --filter "Category=MatrikonConformance"
```

Tests soft-skip when Matrikon isn't installed.

## Status

Scaffold-only today. Prerequisites are the same as Phase 14B (real
DCOM CallChannel via Phase 4 + per-method generator bodies via Phase
6B follow-up). The well-known tag tree means tests will need minimal
parameterisation once the prerequisites land.
