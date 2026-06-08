# Cross-implementation interop matrix

This document is the authoritative reference for which Opc.Classic
client-implementation × server-implementation pairs are validated, what
features each pair exercises, and the spec-mandated reason behind any
EXPECTED-FAIL / NOT-APPLICABLE cell.

Cells are filled in by running [`tools/run-cross-impl-matrix.ps1`](../../tools/run-cross-impl-matrix.ps1)
(see [Track DV1](#track-dv1)) or by manual `tools/probe_servers.py`
runs. The PowerShell wrapper auto-registers every Opc.Classic sample
server under HKCU (no elevation required) before launching the Python
[`tools/run_cross_impl_matrix.py`](../../tools/run_cross_impl_matrix.py)
driver against each server profile defined in
[`tools/probe_matrix.py`](../../tools/probe_matrix.py).

## Profile inventory

| Profile | Server | Spec(s) implemented | Default ProgID |
| --- | --- | --- | --- |
| `testserver` | OPC Foundation `OpcTestServer_x64.exe` | DA 2.05a | `OPC.TestServer.1` |
| `matrikon` | Matrikon OPC Simulation Server | DA 1.0 + 2.05a + 3.0 | `Matrikon.OPC.Simulation.1` |
| `samples-da` | `samples/Opc.Classic.Samples.DaServer` | DA 2.05a + 3.0 | `Opc.Classic.Samples.DaServer.1` |
| `ctt-da` | `samples/Opc.Classic.Samples.CttServer` | DA 2.05a + 3.0 | `Opc.Classic.DaSample.1` |
| `samples-hda` | `samples/Opc.Classic.Samples.HdaServer` | HDA 1.0 | `Opc.Classic.Samples.HdaServer.1` |
| `samples-ae` | `samples/Opc.Classic.Samples.AeServer` | AE 1.0 | `Opc.Classic.Samples.AeServer.1` |
| `security-da` | `samples/Opc.Classic.Samples.OpcSecurityServer` | DA 2.05a + IOPCSecurityNT + IOPCSecurityPrivate | `Opc.Classic.Samples.OpcSecurityServer.1` |

## Client inventory

| Client | What it exercises |
| --- | --- |
| MCP probe (`tools/probe_servers.py`) | Every `opcclassic.*` MCP tool. Curated probe specs per tool. Mirrors what end-user LLMs see through the MCP server. |
| OPC Foundation `OpcTestClient_x64.exe` | Minimal DA enumerator: `IOPCServerList::EnumClassesOfCategories(CATID_OPCDAServer20)` + per-server `IOPCServer::GetStatus`. Does NOT browse / add groups / read / write / subscribe. |
| Managed `samples/Opc.Classic.Samples.DaClient` | Demonstrates full DA flow: connect, browse, add group, add items, sync read, write, subscribe + callback. |
| Managed `samples/Opc.Classic.Samples.AeClient` | Full AE flow: connect, browse areas, create subscription, refresh, poll events, ack condition. |
| Managed `samples/Opc.Classic.Samples.HdaClient` | Full HDA flow: connect, browse, get item handles, read raw / processed / at time. |

## Cross-implementation matrix

Cell legend:

- ✅ **PASS** — every applicable tool succeeds (DA 2.05a tools for a
  DA 2.05a server, plus DA 3.0 tools for a DA 3.0 server).
- 🚧 **TODO** — not yet validated in this matrix. Run
  `tools/run-cross-impl-matrix.ps1` to update.
- ❌ **EXPECTED-FAIL (NOINTERFACE)** — tool calls a higher-spec
  interface (e.g. `opcclassic.da.read_items_by_id` uses DA 3.0
  `IOPCItemIO` against a DA 2.05a server).
- ⛔ **NOT APPLICABLE** — wrong spec entirely (HDA tool against a DA
  server).
- 🔒 **PERMISSION** — server requires admin elevation or explicit DCOM
  ACL grant (see `external/tools/grant-testserver-acl.ps1`).

| Client ↓ \ Server → | `testserver` | `matrikon` | `samples-da` | `ctt-da` | `samples-hda` | `samples-ae` | `security-da` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| MCP probe (DA tools, DA 2.05a subset) | ✅ **104/0** (a18f8c29) | ✅ (26/95 OK, 0 DA failures) | 🚧 TODO | 🚧 TODO | ⛔ wrong spec | ⛔ wrong spec | 🚧 TODO 🔒 |
| MCP probe (DA tools, DA 3.0 IOPCItemIO) | ✅ (TestServer Track AB5 divergence) | ✅ | 🚧 TODO | 🚧 TODO | ⛔ | ⛔ | ❌ NOINTERFACE |
| MCP probe (HDA tools) | ⛔ | ⛔ | ⛔ | ⛔ | 🚧 TODO | ⛔ | ⛔ |
| MCP probe (AE tools) | ⛔ | ⛔ | ⛔ | ⛔ | ⛔ | 🚧 TODO | ⛔ |
| MCP probe (security tools) | ❌ | ❌ | ❌ | ❌ | ⛔ | ⛔ | 🚧 TODO 🔒 |
| MCP probe (capture tools) | ✅ local | ✅ local | ✅ local | ✅ local | ✅ local | ✅ local | ✅ local |
| OPC Foundation `OpcTestClient_x64.exe` | ✅ (a18f8c29) | ✅ (verified GetStatus + AddGroup + AddItems) | 🚧 TODO | 🚧 TODO | ⛔ DA-only client | ⛔ DA-only client | 🚧 TODO |
| Managed `DaClient` sample | 🚧 TODO | 🚧 TODO | 🚧 TODO | 🚧 TODO | ⛔ | ⛔ | 🚧 TODO |
| Managed `AeClient` sample | ⛔ | ⛔ | ⛔ | ⛔ | ⛔ | 🚧 TODO | ⛔ |
| Managed `HdaClient` sample | ⛔ | ⛔ | ⛔ | ⛔ | 🚧 TODO | ⛔ | ⛔ |

Each PASS cell will eventually link to the run log + wire capture that
validated it; see [Validation procedure](#validation-procedure) below.

## Validation procedure

### MCP probe row

The Python driver pair `tools/run_cross_impl_matrix.py` +
`tools/probe_servers.py --expect-matrix <profile>` automates this. The
`tools/run-cross-impl-matrix.ps1` wrapper handles HKCU registration so a
non-elevated dev machine can run the full matrix end-to-end except for
the `testserver` profile (which needs the BH3 ACL grant).

```powershell
# Build samples once
dotnet build samples\Opc.Classic.Samples.DaServer\Opc.Classic.Samples.DaServer.csproj
dotnet build samples\Opc.Classic.Samples.CttServer\Opc.Classic.Samples.CttServer.csproj
dotnet build samples\Opc.Classic.Samples.AeServer\Opc.Classic.Samples.AeServer.csproj
dotnet build samples\Opc.Classic.Samples.HdaServer\Opc.Classic.Samples.HdaServer.csproj
dotnet build samples\Opc.Classic.Samples.OpcSecurityServer\Opc.Classic.Samples.OpcSecurityServer.csproj

# Run the full matrix end-to-end (samples + Matrikon + TestServer)
.\tools\run-cross-impl-matrix.ps1

# Or restrict to a specific profile
.\tools\run-cross-impl-matrix.ps1 -Profile samples-da -Profile samples-hda

# Wire capture per profile (writes .pcap under matrix-out/wire-captures/)
.\tools\run-cross-impl-matrix.ps1 -WireCapture
```

Each profile run produces:

- `matrix-out/<profile>.json` — full per-tool result list (status, error
  text, expected outcome, verdict).
- `matrix-out/matrix.json` — aggregate summary with per-profile MATCH /
  REGRESSION / UNEXPECTED_PASS / MISSING_CLASSIFICATION counts.
- `matrix-out/wire-captures/<profile>/...` — when `-WireCapture`, the
  per-tool `.hex` wire-capture dumps (same shape as
  `docs/interop/wire-captures/`).

The exit code is **0** iff every profile completed with zero REGRESSION
rows. Exit **2** when any profile has a regression; **3** when the
probe driver failed catastrophically (server didn't launch, etc.).

### `OpcTestClient.exe` row

After registering the sample servers (via the same
`tools/run-cross-impl-matrix.ps1` or the per-sample
`--register --registry-hive=hklm` elevated command), invoke the
Foundation TestClient:

```powershell
& external\redist\build\x64\Release\OpcTestClient_x64.exe
```

The TestClient walks `IOPCServerList::EnumClassesOfCategories(CATID_OPCDAServer20)`
and `IOPCServerList::EnumClassesOfCategories(CATID_OPCDAServer30)`, then
for each enumerated CLSID does `CoCreateInstance` + `IOPCServer::GetStatus`.
Expected output includes our registered samples with `state=Running`.
Archive the stdout under `docs/interop/testclient-runs/<profile>-<timestamp>.log`.

### Managed `DaClient` / `AeClient` / `HdaClient` rows

The managed sample clients run from `samples/Opc.Classic.Samples.*Client/`
and target the matching sample servers. These rows validate the OPC
Classic specs end-to-end against our own implementation.

## Expected-fail catalog

These cells are spec-mandated and will never be PASS for the given pair:

| Cell | Reason |
| --- | --- |
| `opcclassic.da.read_items_by_id` against `testserver` / `security-da` | `IOPCItemIO` is DA 3.0; pure DA 2.05a server returns `E_NOINTERFACE`. |
| `opcclassic.security.*` against non-`security-da` profiles | Only `Opc.Classic.Samples.OpcSecurityServer` registers `IOPCSecurityNT`/`IOPCSecurityPrivate`. |
| `opcclassic.cpx.*` against profiles without OPCBinary type system | CPX needs an OPCBinary `IOPCComplexType` registered; sample servers don't. |
| `opcclassic.hda.*` / `opcclassic.ae.*` / `opcclassic.batch.*` / `opcclassic.commands.*` / `opcclassic.dx.*` / `opcclassic.xmlda.*` against DA-only profiles | Wrong spec entirely. |
| All non-DA tools against DA-only `OpcTestClient.exe` | TestClient only exercises DA enumerate + GetStatus. |

## Track DV1

The `tools/run-cross-impl-matrix.ps1` automation script (DV1 in the
project plan) wraps the Python driver with HKCU auto-registration and a
PowerShell-native parameter surface. See `Get-Help .\tools\run-cross-impl-matrix.ps1 -Full`.

## Related docs

- [`docs/interop/probe-coverage.md`](probe-coverage.md) — per-tool
  results from Matrikon-only probes (historical baseline).
- [`docs/interop/testserver-registration-spec.md`](testserver-registration-spec.md)
  — WiX-derived TestServer registration reference.
- [`docs/interop/network-capture.md`](network-capture.md) — Track CA
  capture engine cookbook.
- [`docs/interop/wire-captures/README.md`](wire-captures/README.md) —
  `.hex` wire-capture file format used by `-WireCapture`.
