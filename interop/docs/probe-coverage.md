# Per-call MCP probe results (DA, HDA, AE)

See interop/docs/probe-matrikon.json + probe-testserver.json.
- Matrikon OPC Simulation 1: CLSID F8582CF2-88FB-11D0-B850-00C0F0104305 (vendor MSI install).
- OPC Foundation TestServer x64: CLSID F8582CF9-88FB-11DA-A5ED-0060B0692061 (built via interop/tools/build-testserver.ps1; no MSI).

## Headline numbers

- Matrikon: **25/95 tools OK**; 70 FAIL. ALL DA tools pass against live Matrikon Simulation Server when using `--da-clsid F8582CF2-88FB-11D0-B850-00C0F0104305` (direct activation). The `--da-progid` path currently fails because OPCEnum's data port rejects `IOPCServerList2` and `IOPCServerList` binds with `PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED` (see Issue D below).
- **TestServer: 104/104 MATCH, 0 REGRESSION, 0 UNEXPECTED_PASS, 0 MISSING** (commit `a18f8c29`). Full cross-impl matrix green end-to-end via `tools/run-cross-impl-matrix.ps1 -Profile testserver`. Foundation `OpcTestClient_x64.exe` also activates successfully and runs the full DA 2.x lifecycle exerciser (GetStatus, AddGroup, AddItems, read/write). See [Issue E](#issue-e-testserver-config-xml-clsid-bug-fixed-a18f8c29) for the root cause + fix.

**Track AY+ (commit `6a8f32ce` + follow-up) closed da.get_properties by fixing three stacking NDR VARIANT codec bugs (embedded VARIANT is a `[unique]` pointer, missing 4-byte ULONG discriminator before union body, FLAGGED_WORD_BLOB missing max_count).**

**Track AY++ (commit `7fce8b45`) closed da.read_sync and da.poll_subscription by refactoring `OPCITEMSTATE` decode to the deferred-pile model for the same `[unique] VARIANT` pattern.**

**Track DR3 round 6 (commit `a18f8c29`) closed the TestServer profile by identifying + working around the upstream OPC Foundation TestServer source bug where `__uuidof(OpcTestServer_x64)` (F8582CF8 from IDL coclass) ≠ `OPC_IMPLEMENT_LOCAL_SERVER` GUID (F8582CF9 the runtime CLSID). The fix runs in `interop/tools/register-testserver.ps1` and patches the `<CLSID>` element of TestServer's auto-generated `OpcTestServer_x64.config.xml`. See Issue E below.**

## Issue E: TestServer config XML CLSID bug (FIXED `a18f8c29`)

The OPC Foundation TestServer source has a long-standing inconsistency
between two CLSIDs that should be identical but aren't:

| Source | UUID | Where |
| --- | --- | --- |
| IDL `coclass OpcTestServer_x64` | `F8582CF8-...` | vendored OPC Foundation TestServer `OpcTestServer.idl:45` |
| `OPC_IMPLEMENT_LOCAL_SERVER` GUID | `F8582CF9-...` | vendored OPC Foundation TestServer `OpcTestServer.cpp:53` |

The class table macro `OPC_CLASS_TABLE_ENTRY(COpcTestServer, OpcTestServer_x64, ...)`
expands to `__uuidof(OpcTestServer_x64)` which resolves to the IDL coclass UUID (F8582CF8).
But the AppID, HKLM `CLSID` registration, and Windows DCOM SCM activation all use the
`OPC_IMPLEMENT_LOCAL_SERVER` GUID (F8582CF9).

On the first `OpcTestServer_x64.exe /regserver`, the EXE writes a
`<SelfRegInfo>` block into `OpcTestServer_x64.config.xml` populating
`<CLSID>` with the value from `pClasses[0].pClsid` (which is the IDL
coclass UUID F8582CF8). On every subsequent activation,
`RegisterFromFiles` reads `<CLSID>=F8582CF8` and registers the class
factory under F8582CF8. SCM waits for F8582CF9. They never meet,
SCM times out, returns `CO_E_SERVER_EXEC_FAILURE` (0x80080005).

**Fix**: `interop/tools/register-testserver.ps1` now patches the `<CLSID>`
element of `OpcTestServer_x64.config.xml` after copying it alongside
the EXE. After patching, `pClasses[0].pClsid` becomes F8582CF9 and
the class factory registers under the SCM-expected CLSID.

Validated via:

- Foundation `OpcTestClient_x64.exe` successfully `CoCreateInstance` +
  `GetStatus` + AddGroup + AddItem.
- `tools/run-cross-impl-matrix.ps1 -Profile testserver` reports
  104 MATCH / 0 REGRESSION / 0 UNEXPECTED_PASS / 0 MISSING.

## TestServer (DA 2.05a + Track AB5 DA 3.0): per-tool outcome

After applying `interop/tools/register-testserver.ps1` (Issue E fix) and the
BH3 ACL grant (`interop/tools/grant-testserver-acl.ps1`), every applicable
MCP tool passes against TestServer. The matrix invocation is:

```powershell
.\tools\run-cross-impl-matrix.ps1 -Profile testserver
# expected: testserver  da  104  0  0  0
```

The full per-tool outcome list mirrors the Matrikon table below; key
TestServer-specific notes:

- `da.read_items_by_id` (IOPCItemIO, DA 3.0) **PASS** — TestServer
  advertises CATID_OPCDAServer30 (Track AB5 divergence vs. upstream
  OPC-Classic-CoreComponents). Returns `hr=0xC0040007 (OPC_E_UNKNOWNITEMID)`
  for unknown items, which proves the interface marshalling is healthy
  end-to-end.
- `cpx.get_type_system` **PASS** — TestServer implements `IOPCTypeSystem`;
  returns the supported-type-system list (typically just OPCBinary).
- All `hda.*`, `ae.*`, `batch.*`, `commands.*`, `dx.*`, `xmlda.*`
  NOT_APPLICABLE — TestServer is DA-only.
- All `security.*` EXPECTED_FAIL (`E_NOINTERFACE`) — TestServer doesn't
  implement `IOPCSecurityNT`/`IOPCSecurityPrivate` (only the
  `Opc.Classic.Samples.OpcSecurityServer` `security-da` profile does).

## Matrikon DA: per-tool outcome (with `--da-clsid` direct activation)

Tool | Result | Notes
---|---|---
opcclassic.session.create | OK | session bootstrap
opcclassic.session.list | OK | enumerates active sessions
opcclassic.session.close | OK | tears down session at end
opcclassic.da.connect | OK | CLSID-direct activation via IActivation::RemoteActivation (opnum 0)
opcclassic.da.disconnect | OK | clean disconnect after activation
opcclassic.da.get_status | OK | IOPCServer::GetStatus opnum 6; state=Running, version=2.0.0, vendor=Matrikon Inc.
opcclassic.da.browse (root) | OK | 5 root entries (#MonitorACLFile, @ClientCount, Configured Aliases, Simulation Items)
opcclassic.da.browse (Random leaves) | OK | 17 leaves found
opcclassic.da.read_items_by_id | OK | DA 3.0 stateless IOPCItemIO::Read; Random.Int4 hr=0x0
opcclassic.da.add_group | OK | returns serverGroupHandle
opcclassic.da.remove_group | OK | clean tear-down after add
opcclassic.da.get_error_string | OK | round-trips HRESULT to message text
opcclassic.da.get_properties | OK | **fixed by Track AY+ commit `6a8f32ce`** — all 14 standard properties decode (Item Quality=192, Item Access Rights=1, Server Scan Rate=100, etc.)
opcclassic.da.add_items | OK | OPCITEMDEF[] payload accepted; returns 2 server handles with hr=0x00000000
opcclassic.da.read_sync | OK | **fixed by Track AY++ commit `7fce8b45`** — Bucket Brigade.Int4 = 0 hr=0x00000000
opcclassic.da.write_sync | OK | reaches server; hr=0xC0040004 (OPC_E_BADTYPE) is server-side validation (probe writes string into Int4 item)
opcclassic.da.subscribe | OK | AddItems → Advise round-trip via IOPCAsyncIO2 + IConnectionPoint; subscribe cookie returned
opcclassic.da.poll_subscription | OK | **fixed by Track AY++** — drains queued callbacks from the subscription sink
opcclassic.cpx.get_type_system | OK | reports namespace/supported state

## TestServer: per-tool outcome

opcclassic.session.create / list / close | OK | session lifecycle
opcclassic.da.connect | FAIL | tools/call timed out (15s) - DCOM SCM activation blocked by CO_E_SERVER_EXEC_FAILURE; root cause documented in interop/docs/testserver.md (suspected missing DCOM AppID entry or COM categories — Track BH audits the WiX spec to confirm)
all other tools | FAIL | downstream of connect failure: session not connected
opcclassic.*.disconnect | OK | returns "client was not connected" hr=0x1; no actual call to server

## HDA / AE / Batch / Commands / DX / XML-DA on Matrikon

Matrikon OPC Simulation Server only implements OPC DA. Probing the HDA/AE/Batch/Commands/DX connect tools requires server-specific CLSIDs that the simulation server does not register. The probe consequently:

- opcclassic.{hda,ae,batch,commands,dx}.connect -> FAIL with "Provide an OPC server ProgID, CLSID, or connectionString." (probe sent no spec-specific clsid).
- All read/write/browse tools downstream of those connects -> FAIL ("session is not connected").
- opcclassic.{hda,ae,batch,commands,dx}.disconnect -> OK (no-op success).
opcclassic.discovery.enumerate_servers | FAIL | OPCEnum data-port bind for IOPCServerList2/IOPCServerList rejected with PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED (Issue D below; same root cause that breaks `--da-progid` activation).

## Root causes (4 distinct issues)

### Issue A: AlterContext PROVIDER_REJECTION on per-spec sub-IIDs — **CLOSED by Track AC** (`2d96d8f9`)

Original symptom: `src/Opc.Classic.Dcom/Transport/DcomCallChannel.cs::AlterContextAsync` sent a
single-IID `alter_context_req` with the new interface IID (`IOPCItemProperties` / `IOPCItemMgt` /
`IOPCSyncIO` / `IOPCAsyncIO2`). Matrikon (and most production OPC servers) rejected because their
RPC endpoint did not advertise those IIDs during `bind_ack`.

Fix: Track AC introduced `OpcSpecCatalog.Da` and the `preBindIids` channel parameter so the full DA
IID set is declared in the initial bind PDU. After the catalog landed, all DA AlterContext
rejections went away.

Residual gap: Non-DA specs (`Cpx`, `Security`, `Discovery`, and the HDA/AE/Batch/Commands/DX
surfaces) do not yet have catalog entries, so the same rejection class persists for their
non-DA IIDs. **Track BG** extends the catalog to close that residual.

### Issue B: TestServer activation fails (CO_E_SERVER_EXEC_FAILURE)

Locally built TestServer EXE activation fails with HRESULT `0x80080005` and DCOM event log
10010 ("server did not register with DCOM within the required timeout") even after
`interop/tools/register-testserver.ps1` runs. Suspected cause: the ad-hoc registration script is
missing one or more entries that the legacy installer writes (most likely the DCOM
AppID entry for `OpcTestServer_x64.exe`, COM Implemented/Required Categories under each
CLSID, or wrong proxy-stub registration order). **Track BH** audits the upstream
`OPC-Classic-CoreComponents` installer manifests (`Installer.wxs`, `MergeModule.wxs`,
`MergeModuleSdk.wxs`) as the canonical registration spec and fixes the script in-place
(NO `msiexec`-driven install path).

### Issue C: OPCEnum **activation** fails (rpc_s_access_denied 0x05)

`opcclassic.discovery.enumerate_servers` and ProgID-based connect paths use
`IRemoteSCMActivator::RemoteCreateInstance`. The activator path reuses the same OPC connection
credentials used for the target server and upgrades weak activation protection to
`RPC_C_AUTHN_LEVEL_PKT_INTEGRITY`. Operators must still grant the calling identity DCOM
Launch/Activation and Access permissions on the OPCEnum AppID
`{13486D44-4821-11D2-A494-3CB306C10000}` (note: AppID is distinct from the CLSID
`{13486D51-4821-11D2-A494-3CB306C10000}`, differing in one hex digit). The helper
`interop/tools/grant-opcenum-acl.ps1` (Track AN) automates this once per host.

### Issue D: OPCEnum **data-port bind** rejects IOPCServerList(2) (NEW finding, 2026-06-04 probe)

Independent of Issue C: even when the activation succeeds (`IActivation::RemoteActivation`
opnum 0 returns a valid OBJREF with `IPID` + `DUALSTRINGARRAY` bindings for the OPCEnum data
port), the subsequent DCE bind to the data port for `IOPCServerList2` (or after downgrade,
`IOPCServerList`) returns `bind_ack` with `PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED`
for BOTH IIDs.

This is the **same root-cause class** as Issue A, but for OPCEnum's `IOPCServerList` /
`IOPCServerList2` IIDs which are not in any pre-bind catalog today
(`DcomOpcEnumCallChannelFactory.CreateObjectChannelAsync` calls
`DcomCallChannelFactory.ConnectAsync` with `preBindIids: null`).

The wire trace shows our client sends bind for `IOPCServerList2` alone — and OPCEnum responds
PROVIDER_REJECTION. After the existing downgrade fallback, bind for `IOPCServerList` alone is
also rejected. The single-IID bind shape is correct DCE; what is unusual is that OPCEnum
rejects both IIDs that it is responsible for advertising. The likely cause is that our bind
PDU is missing a presentation-context attribute OPCEnum requires (e.g., the OPC-Common
type-library reference, or a specific transfer-syntax alternative beyond the default NDR
8a885d04-1ceb-11c9-9fe8-08002b104860 v2.0).

**Workaround:** pass `--da-clsid <CLSID>` to the probe driver (bypasses OPCEnum and dials the
target server's IRemoteActivation directly) — this is what the current 25/95 OK baseline uses.

**Fix surface:** **Track BG** (initial-bind catalog extension to Discovery / Cpx / Security
specs) will add an `OpcSpecCatalog.Discovery` collection that the
`DcomOpcEnumCallChannelFactory` passes through. If the issue persists after adding the IID
to the catalog, Track BG also covers per-tower-syntax presentation-context experiments
(adding the OPC-Common type-library 64-bit transfer-syntax alternative).

## What works today (Matrikon DA)

ALL DA tools work end-to-end against live Matrikon Simulation Server when activated via
`--da-clsid F8582CF2-88FB-11D0-B850-00C0F0104305`. Tracks AY+ and AY++ closed the last decode
issues (GetProperties + ItemState wireVARIANT). All write/read/subscribe/poll/properties
paths validated against the live server.

## What is blocked today

- **Non-DA AlterContext on Matrikon** (CPX `get_complex_type` / `get_dictionary`, Security):
  same Issue A class for non-DA IIDs. Track BG adds `OpcSpecCatalog` entries per spec to
  declare the full IID set in the initial bind.
- **`--da-progid` activation** (Issue D): OPCEnum data-port rejects `IOPCServerList`(`2`) bind.
  Track BG (catalog extension) is the suspect-fix; if catalog alone doesn't close it, also
  covers presentation-context attribute experiments.
- **`discovery.enumerate_servers`**: same Issue D root cause.
- **TestServer end-to-end** (Issue B): Track BH audits the upstream WiX spec and fixes
  `interop/tools/register-testserver.ps1` in-place.
- **Non-DA specs (HDA / AE / Batch / Commands / DX / XML-DA)**: Matrikon Simulation does not
  implement them. Track BH adds the Foundation `OpcTestServer` as a probe target via the
  `opc-classic/testserver` Docker container — TestServer + bundled spec plugins cover all
  those specs.

## Post-fix delta (commit 2d96d8f9, 2026-06-02)

After Tracks AC + AD + AE landed, re-ran Matrikon probe (full JSON: interop/docs/probe-matrikon-post-ac.json).

Matrikon: 21/95 OK (was 19/95). Newly working: opcclassic.da.write_sync, opcclassic.da.subscribe.

Still failing (residual follow-ups):
- opcclassic.da.add_items: server fault 0x80010105 (RPC_E_SERVERFAULT) - wire-format issue in OPCITEMDEF array encoding
- opcclassic.da.read_sync: dependent on add_items
- opcclassic.da.get_properties: LPWSTR/VARIANT decode offset issue revealed after AlterContext was unblocked
- opcclassic.da.poll_subscription: depends on item handles from add_items
- opcclassic.cpx.get_complex_type / get_dictionary: shared codec path with get_properties
- opcclassic.discovery.enumerate_servers: OPCEnum activation still rejected (0x000006F7) under default Windows-SSO; needs DCOM AppID ACL tweak per interop/docs/opcenum-auth.md

## Post-fix delta (Tracks AF + AG + AH + AI shipped)

After NDR wire-format completion shipped, re-ran Matrikon probe (full JSON: interop/docs/probe-matrikon-final.json).

Matrikon: **22/95 OK** (was 21/95 headline). The headline count only moved by one but the underlying state changed materially:

**New verified end-to-end against Matrikon (now hitting the server and getting valid responses):**
- `opcclassic.da.add_items` — Matrikon AddItems accepts the OPCITEMDEF[] payload, returns 2 server handles (e.g. `78333568, 78333928`) with `hr=0x00000000`. RPC_E_SERVERFAULT (0x80010105) eliminated.
- `opcclassic.da.write_sync` — Now actually transmits the values to Matrikon (previously failed silently on empty arrays). Response `hr=0xC0040004` is `OPC_E_BADTYPE` (Bucket Brigade.Int4 doesn't accept the probe's string write), confirming the request reached the server and the server validated it.

**Residual failures (post-AF/AG/AH):**
- `opcclassic.da.read_sync` — request now reaches the server (with correct dwCount + handles), but the response array decode still drops bytes for the OPCITEMSTATE+VARIANT pile when VARIANT bodies have unusual padding. Needs live wire capture to diagnose.
- `opcclassic.da.poll_subscription` — depends on `read_sync` succeeding.
- `opcclassic.da.get_properties` — LPWSTR/VARIANT response decode offset issue persists. Likely the per-element VARIANT alignment in `[OpcVariantElements]` decode is misaligned for the specific Matrikon wire shape.
- `opcclassic.cpx.get_complex_type` / `get_dictionary` — share the `get_properties` codec path.
- `opcclassic.discovery.enumerate_servers` — environmental DCOM AppID ACL issue on OPCEnum AppID `{13486D44-4821-11D2-A494-3CB306C10000}` (CLSID `{13486D51-...}`). Also surfaced a code gap: `IOPCServerList` enum/details methods are stubbed in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:887,902` (Track AJ2 follow-up).
- HDA / AE / Batch / Commands / DX / XML-DA — Matrikon Simulation does not implement these specs; no in-tree server available.

**What shipped under Track AF/AG/AH:**
- AF1: `[OpcEmitArrayCount]` added to every DA / CPX method matching `[in] DWORD dwCount, [in, size_is(dwCount)] T*` (covers IOPCItemMgt, IOPCSyncIO/2, IOPCAsyncIO2/3, IOPCItemProperties, IOPCItemDeadbandMgt, IOPCItemSamplingMgt, IOPCItemIO).
- AF2: `[OpcUniquePointer]` added to every `out T[]` parameter and `[return: OpcUniquePointer]` to every `Task<T[]>` return that maps to IDL `[out, size_is(,N)] T**`.
- AF4: `[OpcVariantElements]` added to VARIANT-array outputs (e.g. `IOPCItemProperties.GetItemPropertiesAsync.data`, `IOPCSyncIO2.ReadMaxAgeAsync.values`).
- AG1: `NdrOpcItemResultCodec` rewritten with the correct DCE/RPC 1.1 §14.3.12.3 deferred-pointer-pile layout (inline 20 bytes + deferred conformant blob). Added self-contained `WriteConformantArray` / `ReadConformantArray` helpers that handle the outer unique-pointer referent and null-as-empty-array semantics.
- AG2: `NdrOpcItemAttributesCodec` rewritten symmetrically for the `IEnumOPCItemAttributes::Next` response shape.
- AG3: Registered both new helpers in `OpcProxyGenerator.TryGetDeferredPileArrayHelper` (response decode), `OpcProxyGenerator.TryGetDeferredPileArrayHelperWrite` (server-side write), and `OpcServerDispatchGenerator.TryGetDeferredPileArrayHelperWrite/Read`.
- AG4: Null-referent decode safety. Proxy generator now branches on the outer unique-pointer referent for `[out, size_is(,N)] T**` parameters — a null referent yields `Array.Empty<T>()` instead of consuming `max_count` and corrupting the wire cursor for subsequent out parameters.
- AH: Hand-written `IOPCSyncIOClientProxy.cs` fixed to emit `dwCount` before serverHandles in `ReadAsync` request, and to consume the outer unique-pointer referents on both response arrays (`OPCITEMSTATE**` + `HRESULT**`).
- AI2: Full TUnit suite green (1+ pre-existing flaky TCP test outside scope).

**Outstanding work (tracked as follow-up todos):**
- AJ1: TestServer activation timeout (CO_E_SERVER_EXEC_FAILURE). Even after `interop/tools/register-testserver.ps1` registers the proxy/stub DLLs into System32, DCOM SCM times out activating the local EXE. Requires either upstream WiX MSI build or AppID/DCOM ACL investigation.
- AJ2: Implement `IOPCServerList::EnumClassesOfCategories` + `GetClassDetails` (currently TODO comments) to unblock `discovery.enumerate_servers` once OPCEnum ACLs are granted.
- Live wire decode fixes for `read_sync` / `get_properties` / `cpx.*`: needs Wireshark capture of an equivalent Windows DCOM call to disambiguate the actual VARIANT-array padding Matrikon emits. The proxy code paths are correct per DCE/MIDL spec; Matrikon may use a non-spec layout for these specific fields that requires server-specific accommodation.
