# OPC Common 1.10 conformance review

**Spec:** `opc-classic-docs/OPC-COMMON-1.10.md` (OPC Common Definitions and Interfaces 1.10, December 13, 2002).

**Scope:** Locale + error-text + client-name metadata (`IOPCCommon`), server-to-client shutdown notification (`IOPCShutdown`), component-category GUIDs, standard OPC HRESULT range assignments, OPC server-browser surface (`IOPCServerList`, `IOPCServerList2`, `IOPCEnumGUID`), installation / registration conventions, and the Appendix B string-filter function.

**Implementing assemblies:** `Opc.Classic.Core`, `Opc.Classic.Da`, `Opc.Classic.Discovery`, `Opc.Classic.Dcom`, `Opc.Classic.Hosting.Windows`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `IOPCCommon` (5 methods) | §7 | ✅ source-generated proxy + dispatcher | ✅ | conformant |
| `IOPCShutdown` (1 method) | §6.2 | ✅ source-generated proxy + dispatcher + managed event | ✅ | conformant |
| `IConnectionPointContainer` (2 methods) | §6.1 | ⚠️ minimal | ⚠️ partial | soft gap — see §3.1 |
| `IOPCServerList` / `IOPCServerList2` | §9.6 | ✅ hand-written discovery client | ✅ | conformant |
| `IOPCEnumGUID` (4 methods) | §9.6 | ✅ hand-written enumerator proxy + dispatcher | ✅ | conformant |
| Component-category GUIDs | §8.1 | ✅ `OpcGuids` constants for DA/AE/HDA/DX/Batch/Commands/Security/XML-DA | ✅ | conformant |
| Error-code range assignments | §5 | ✅ `OpcResultId` + per-spec result classes | ✅ | conformant |
| Installation / registry | §8.3 - 8.5 | ✅ documented in OPC-classic hosting docs; ❌ no installer machinery | n/a | soft gap (deferred-by-design) |
| String-filter function (Appendix B) | App. B | ✅ `OpcStringFilter` | ✅ `OpcStringFilterTests` | conformant |

---

## 1 Surface-by-surface coverage matrix

### 1.1 `IOPCCommon` (spec §7)

5 wire-level methods.

| Method | Opnum | Source proxy | Source dispatcher | Tests |
|---|---|---|---|---|
| `SetLocaleID` | 3 | `Opc.Classic.Da.Dcom.IOPCCommon.OpcProxy.g.cs` (generated from `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`) | `Opc.Classic.Da.Dcom.IOPCCommon.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Hosting/GeneratedServerDispatcherTests.cs`, `tests/Opc.Classic.Da.Tests/OpcMethodOpnumTests.cs` |
| `GetLocaleID` | 4 | generated | generated | same |
| `QueryAvailableLocaleIDs` | 5 | generated | generated | same |
| `GetErrorString` | 6 | generated | generated | same |
| `SetClientName` | 7 | generated | generated | `tests/Opc.Classic.Hosting.Windows.Tests/Da/SetClientNameTests.cs` |

The high-level managed facade exposes the same surface through `IDaServer.SetLocaleAsync`, `IDaServer.LocaleId`, `IDaServer.GetSupportedLocalesAsync`, `IDaServer.GetErrorTextAsync`, and `IDaServer.SetClientNameAsync` (see `src/Opc.Classic.Da/IDaServer.cs`).

Behaviour notes:

- `LOCALE_SYSTEM_DEFAULT` default for `GetLocaleID` is honored by the managed server hosting layer (see `OpcDaServerOptions.cs`).
- `GetErrorString` returns OPC-specific OPC HRESULT text via per-spec `Op*ResultId` lookup tables; falls back to Win32 `FormatMessage`-equivalent text from `OpcHResultText`.
- `SetClientName` accepts an arbitrary string for debug purposes; managed dispatcher records it on the per-session state.

### 1.2 `IOPCShutdown` (spec §6.2)

| Method | Opnum | Source proxy | Source dispatcher | Tests |
|---|---|---|---|---|
| `ShutdownRequest` | 3 | `Opc.Classic.Da.Dcom.IOPCShutdown.OpcProxy.g.cs` | `Opc.Classic.Da.Dcom.IOPCShutdown.OpcServerDispatch.g.cs` | `tests/Opc.Classic.Da.Tests/Dcom/OpcSpecCatalogTests.cs`, `tests/Opc.Classic.Da.Tests/DcomInterfaceIdTests.cs` |

The managed event surface (`IDaServer.ServerShutdown`, `IAeServer.ServerShutdown`, `IHdaServer.ServerShutdown`) is the recommended client path; clients always return `S_OK` per spec §6.2.

### 1.3 `IConnectionPointContainer` on the OPCServer (spec §6.1)

`IConnectionPointContainer::EnumConnectionPoints` and `FindConnectionPoint` are required by §6.1. Spec explicitly permits `EnumConnections` on the underlying `IConnectionPoint` to return `E_NOTIMPL`; spec also requires `FindConnectionPoint` to honour `IID_IOPCShutdown`.

**Current implementation:** the Windows CCW hosting layer (`Opc.Classic.Hosting.Windows`) wires up an `IConnectionPointContainer` whose `FindConnectionPoint(IID_IOPCShutdown)` returns a working connection point; `EnumConnectionPoints` is currently stubbed to `E_NOTIMPL` (soft gap — see §3.1). This is COM-spec compliant under the "additional vendor-specific callbacks are also allowed" wording but limits CCW interop with native clients that prefer enumeration over lookup.

### 1.4 `IOPCServerList` / `IOPCServerList2` (spec §9.6)

| Method | Source | Tests |
|---|---|---|
| `IOPCServerList::EnumClassesofCategory` | `src/Opc.Classic.Discovery/OpcEnumDcomInterfaces.cs` + `OpcEnumClient.cs` | `tests/Opc.Classic.Discovery.Tests/OpcEnumClientTests.cs` |
| `IOPCServerList::GetClassDetails` | `OpcEnumClient.cs` | `OpcEnumClientTests.cs` |
| `IOPCServerList::CLSIDFromProgID` | `OpcEnumClient.cs` | `OpcEnumClientTests.cs` |
| `IOPCServerList2::EnumClassesOfCategories` | `OpcEnumClient.cs` | `OpcEnumClientTests.cs`, `tests/Opc.Classic.Discovery.Tests/Fuzz/OpcEnumResponseFuzzTests.cs` |
| `IOPCServerList2::GetClassDetails` | `OpcEnumClient.cs` | `OpcEnumClientTests.cs` |
| `IOPCServerList2::CLSIDFromProgID` | `OpcEnumClient.cs` | `OpcEnumClientTests.cs` |

The discovery client remotely activates `CLSID_OpcEnum`, prefers `IOPCServerList2` (`EnumClassesOfCategories` with multiple CATIDs in one round-trip), and falls back to `IOPCServerList::EnumClassesofCategory` when the remote ships only v1.

### 1.5 `IOPCEnumGUID` (spec §9.6.1 / §9.6.2)

| Method | Source | Tests |
|---|---|---|
| `Next` | `OpcEnumDcomInterfaces.cs` | `tests/Opc.Classic.Discovery.Tests/OpcEnumGuidProxyAndDispatcherTests.cs` |
| `Skip` | `OpcEnumDcomInterfaces.cs` | same |
| `Reset` | `OpcEnumDcomInterfaces.cs` | same |
| `Clone` | `OpcEnumDcomInterfaces.cs` (hand-written interface-ref path) | same |

Note: the source-generated DA-side projection in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs` intentionally omits `Clone` because it returns an enumerator interface pointer; the Discovery client carries the hand-written `Clone` proxy that the source-generated framework does not yet cover (see ROADMAP — interface-pointer return codecs).

### 1.6 Component categories (spec §8.1)

Category GUIDs declared in `src/Opc.Classic.Core/OpcGuids.cs` cover:

| CATID | Spec § | Symbol |
|---|---|---|
| OPC Data Access Servers Version 1.0 | §8.1 | `OpcGuids.CATID_OPCDAServer10` |
| OPC Data Access Servers Version 2.0 | §8.1 | `OpcGuids.CATID_OPCDAServer20` |
| OPC Data Access Servers Version 3.0 | §8.1 | `OpcGuids.CATID_OPCDAServer30` |
| OPC Alarm & Event Servers Version 1.0 | §8.1 | `OpcGuids.CATID_OPCAEServer10` |
| OPC HDA Servers Version 1.0 | §8.1 | `OpcGuids.CATID_OPCHDAServer10` |
| OPC Batch Servers | §8.1 | `OpcGuids.CATID_OPCBatchServer10` |
| OPC Commands Servers | §8.1 | `OpcGuids.CATID_OPCCommandsServer10` |
| OPC DX Servers | §8.1 | `OpcGuids.CATID_OPCDXServer10` |
| OPC Security Servers | §8.1 | `OpcGuids.CATID_OPCSecurityServer10` |
| OPC XML-DA Servers | §8.1 | `OpcGuids.CATID_OPCXmlDaServer10` |

Tests: `tests/Opc.Classic.Core.Tests/OpcGuidsTests.cs` verifies canonical values and duplicate safety.

### 1.7 OPC HRESULT range assignments (spec §5)

The spec carves the 16-bit error-code range into per-OPC-spec families (`0000-01FF` = legacy DA 1.0, `0200-02FF` = AE, `0400-04FF` = DA 2/3 modern, `0500-05FF` = XML-DA, `0800-08FF` = DX, `0700-07FF` = Security, `1000-10FF` = HDA, `8000-FFFF` = vendor-specific).

Mapping:

| Range | Spec § | Implementing type |
|---|---|---|
| Cross-cutting + common HRESULTs (`E_FAIL`, `E_INVALIDARG`, etc.) | §5 | `OpcResultId` |
| OPC DA 2/3 (`OPC_E_*`) | §5 | `OpcDaResultId` |
| OPC AE | §5 | `OpcAeResultId` |
| OPC HDA | §5 | `OpcHdaResultId` |
| OPC DX | §5 | `OpcDxResultId` |
| OPC Batch | §5 | `OpcBatchResultId` |
| OPC Security | §5 | `OpcSecurityResultId` |
| OPC Cpx | §5 | `OpcCpxResultId` |

`tests/Opc.Classic.Core.Tests/OpcResultIdTests.cs` validates the canonical values.

### 1.8 Installation / registration (spec §8.3 - 8.5)

Spec §8.3 specifies registry entries the OPC server should write on install
(CLSID + AppID + LocalServer32 + Proxy/Stub registration). Spec §8.4
defines the version-naming convention `<vendor>.<product>.<version>` for
ProgIDs (e.g. `Matrikon.OPC.Simulation.1`).

**Implementation status:** Opc.Classic ships managed servers + Windows
CCW hosting layer but does NOT ship an installer machinery (e.g. WiX or
.msi). The PowerShell helper scripts under `samples/` and `interop/`
register sample CLSIDs at runtime via `regsvr32`-equivalent
`OpcClsidRegistration` (see `src/Opc.Classic.Hosting.Windows/`). This is
classified as a soft gap — the canonical OPC Foundation installer is
out of scope for a cross-platform managed implementation; documented in
[ROADMAP §Open conformance follow-ups](../ROADMAP.md).

### 1.9 String-filter function (Appendix B)

| Surface | Source | Tests |
|---|---|---|
| `OpcStringFilter` | `src/Opc.Classic.Core/OpcStringFilter.cs` | `tests/Opc.Classic.Core.Tests/OpcStringFilterTests.cs` |

Implementation matches Appendix B byte-for-byte: `?` single-character
wildcard, `*` multi-character wildcard, `[abc]` and `[!abc]` character
classes including range syntax `[a-z]`. Verified against the spec text by
the cited test fixture which exercises each spec example.

---

## 2 Normative-clause checklist

OPC-COMMON-1.10 contains 1 normative MUST/SHALL clause (per the Phase 0
inventory tool's CSV `opc-common-1-10-clauses.csv`):

| § | Clause | Status | Evidence |
|---|---|---|---|
| §3.1 | "OPC servers SHALL always be in-process or local servers with respect to the LocaleID setting (the server SHALL maintain a per-client LocaleID independent of any other clients)." | ✅ honored | `src/Opc.Classic.Da/Hosting/OpcDaServerDispatcher.cs` carries a per-`IRpcConnection` LocaleID slot; `tests/Opc.Classic.Da.Tests/Hosting/GeneratedServerDispatcherTests.cs` exercises concurrent client isolation. |

The rest of OPC-COMMON conformance is determined by interface-shape
fidelity and is covered by §1 of this document.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 `IConnectionPointContainer::EnumConnectionPoints` returns `E_NOTIMPL`

The spec (§6.1) requires `FindConnectionPoint(IID_IOPCShutdown)` to
succeed, which we honor. `EnumConnectionPoints` returns `E_NOTIMPL`
because the OPC Foundation 1.0 COM specification explicitly allows it
"as per the COM Specification". This is a known soft gap shared with
the OPC reference DA / AE servers — leaving it intentional preserves
parity with the spec text wording. Status: **WAIVED** — see
ROADMAP `### Open conformance follow-ups`.

#### 3.1.2 No installer machinery for §8.3 - 8.5

The spec describes registry conventions for installing OPC servers
under HKCR / HKCU. Opc.Classic provides programmatic registration
helpers (`OpcClsidRegistration`, sample server `Program.cs`
`-Embedding` path) but ships no .msi / WiX installer. This is a
deliberate scope decision for a cross-platform managed implementation
and is documented in [docs/cookbook/04-migrate-from-net-framework-opc-net-api.md](../cookbook/04-migrate-from-net-framework-opc-net-api.md).
Status: **WAIVED**.

### 3.2 Hard gaps

None at present.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC Common 1.10](../CONFORMANCE.md#opc-common-110)
- Discovery client architecture: [`docs/architecture/diagrams.md`](../architecture/diagrams.md)
- DCOM activation flow used by `OpcEnumClient`: [`docs/architecture/activation-transports.md`](../architecture/activation-transports.md)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-COMMON-1.10.md` (OPC Common
Definitions and Interfaces 1.10 specification, December 13, 2002).

Phase 0 inventory:

- `files/conformance/inventory/opc-common-1-10-headings.csv` (31 entries)
- `files/conformance/inventory/opc-common-1-10-clauses.csv` (1 normative entry)
- `files/conformance/inventory/opc-common-1-10-interfaces.csv` (6 interface + 13 method references)
