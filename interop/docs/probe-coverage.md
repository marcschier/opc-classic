# Per-call MCP probe results (DA, HDA, AE)

See [probe-matrikon.json](probe-matrikon.json) +
[probe-testserver.json](probe-testserver.json) for the raw probe artifacts.

Server profiles exercised:

- **Matrikon OPC Simulation 1**: CLSID `F8582CF2-88FB-11D0-B850-00C0F0104305`
  (vendor MSI install).
- **OPC Foundation TestServer x64**: CLSID
  `F8582CF9-88FB-11DA-A5ED-0060B0692061` (built via
  build-testserver; no MSI).

## Headline numbers

| Profile | Result | Notes |
| --- | --- | --- |
| Matrikon | 25/95 OK (70 FAIL) | All DA tools pass with `--da-clsid F8582CF2-88FB-11D0-B850-00C0F0104305` (direct activation). The `--da-progid` path fails because OPCEnum's data port rejects `IOPCServerList2` and `IOPCServerList` binds with `PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED` ([Issue D](#issue-d-opcenum-data-port-bind-rejects-iopcserverlist2)). |
| TestServer | 104/104 MATCH, 0 REGRESSION, 0 UNEXPECTED_PASS, 0 MISSING | Full cross-impl matrix green end-to-end via run-cross-impl-matrix `-Profile testserver`. Foundation `OpcTestClient_x64.exe` also activates and runs the full DA 2.x lifecycle exerciser (GetStatus, AddGroup, AddItems, read/write). |

DA `get_properties`, `read_sync`, and `poll_subscription` work end-to-end
against Matrikon. The NDR VARIANT decoder handles the embedded `[unique]`
VARIANT, the 4-byte ULONG discriminator before union bodies, and the
FLAGGED_WORD_BLOB `max_count`. The deferred-pile model for `OPCITEMSTATE`
handles the same `[unique] VARIANT` pattern symmetrically.

## Issue E: TestServer config XML CLSID inconsistency (worked around in registration)

The OPC Foundation TestServer source has a long-standing inconsistency
between two CLSIDs that should be identical but aren't:

| Source | UUID | Where |
| --- | --- | --- |
| IDL `coclass OpcTestServer_x64` | `F8582CF8-...` | vendored OPC Foundation TestServer `OpcTestServer` |
| `OPC_IMPLEMENT_LOCAL_SERVER` GUID | `F8582CF9-...` | vendored OPC Foundation TestServer `OpcTestServer` |

The class table macro `OPC_CLASS_TABLE_ENTRY(COpcTestServer, OpcTestServer_x64, ...)`
expands to `__uuidof(OpcTestServer_x64)`, which resolves to the IDL coclass UUID
(F8582CF8). The AppID, HKLM `CLSID` registration, and Windows DCOM SCM activation
all use the `OPC_IMPLEMENT_LOCAL_SERVER` GUID (F8582CF9).

On the first `OpcTestServer_x64.exe /regserver`, the EXE writes a `<SelfRegInfo>`
block into `OpcTestServer_x64.config.xml` populating `<CLSID>` with the value
from `pClasses[0].pClsid` (which is the IDL coclass UUID F8582CF8). On every
subsequent activation, `RegisterFromFiles` reads `<CLSID>=F8582CF8` and
registers the class factory under F8582CF8. SCM waits for F8582CF9. They
never meet, SCM times out, returns `CO_E_SERVER_EXEC_FAILURE` (0x80080005).

**Workaround**: register-testserver
patches the `<CLSID>` element of `OpcTestServer_x64.config.xml` after copying
it alongside the EXE. After patching, `pClasses[0].pClsid` becomes F8582CF9
and the class factory registers under the SCM-expected CLSID.

Validated by:

- Foundation `OpcTestClient_x64.exe` successfully `CoCreateInstance` +
  `GetStatus` + `AddGroup` + `AddItem`.
- run-cross-impl-matrix
  `-Profile testserver` reports 104 MATCH / 0 REGRESSION /
  0 UNEXPECTED_PASS / 0 MISSING.

## TestServer (DA 2.05a + DA 3.0): per-tool outcome

After applying register-testserver
(Issue E workaround) and the DCOM ACL grant via
grant-testserver-acl, every
applicable MCP tool passes against TestServer. The matrix invocation is:

```powershell
.\tools\run-cross-impl-matrix.ps1 -Profile testserver
# expected: testserver  da  104  0  0  0
```

The full per-tool outcome list mirrors the Matrikon table below; key
TestServer-specific notes:

- `da.read_items_by_id` (IOPCItemIO, DA 3.0) **PASS** — TestServer
  advertises CATID_OPCDAServer30 (a divergence vs. upstream
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
opcclassic.da.get_properties | OK | all 14 standard properties decode (Item Quality=192, Item Access Rights=1, Server Scan Rate=100, etc.)
opcclassic.da.add_items | OK | OPCITEMDEF[] payload accepted; returns 2 server handles with hr=0x00000000
opcclassic.da.read_sync | OK | Bucket Brigade.Int4 = 0 hr=0x00000000
opcclassic.da.write_sync | OK | reaches server; hr=0xC0040004 (OPC_E_BADTYPE) is server-side validation (probe writes string into Int4 item)
opcclassic.da.subscribe | OK | AddItems → Advise round-trip via IOPCAsyncIO2 + IConnectionPoint; subscribe cookie returned
opcclassic.da.poll_subscription | OK | drains queued callbacks from the subscription sink
opcclassic.cpx.get_type_system | OK | reports namespace/supported state

## TestServer: per-tool outcome

opcclassic.session.create / list / close | OK | session lifecycle
opcclassic.da.connect | OK | succeeds after the Issue E config-XML patch and the DCOM ACL grant from `grant-testserver-acl.ps1`
all other DA tools | OK | full DA 2.x lifecycle works end-to-end (see headline 104/104)
opcclassic.{hda,ae,batch,commands,dx,xmlda}.* | NOT_APPLICABLE | TestServer is DA-only

## HDA / AE / Batch / Commands / DX / XML-DA on Matrikon

Matrikon OPC Simulation Server only implements OPC DA. Probing the
HDA/AE/Batch/Commands/DX connect tools requires server-specific CLSIDs
that the simulation server does not register. The probe consequently:

- `opcclassic.{hda,ae,batch,commands,dx}.connect` → FAIL with
  "Provide an OPC server ProgID, CLSID, or connectionString." (probe
  sent no spec-specific CLSID).
- All read/write/browse tools downstream of those connects → FAIL
  ("session is not connected").
- `opcclassic.{hda,ae,batch,commands,dx}.disconnect` → OK (no-op success).
- `opcclassic.discovery.enumerate_servers` → FAIL: OPCEnum data-port bind
  for `IOPCServerList2`/`IOPCServerList` rejected with
  `PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED` ([Issue D](#issue-d-opcenum-data-port-bind-rejects-iopcserverlist2);
  same root cause that breaks `--da-progid` activation).

## Root causes

### Issue A: AlterContext PROVIDER_REJECTION on per-spec sub-IIDs (resolved)

`DcomCallChannel` used
to send a single-IID `alter_context_req` with the new interface IID
(`IOPCItemProperties` / `IOPCItemMgt` / `IOPCSyncIO` / `IOPCAsyncIO2`).
Matrikon (and most production OPC servers) rejected those rebinds because
their RPC endpoint did not advertise the IIDs during `bind_ack`.

`OpcSpecCatalog.Da` and the `preBindIids` channel parameter declare the
full DA IID set in the initial bind PDU. With the catalog in place, all
DA AlterContext rejections went away.

Residual gap: non-DA specs (`Cpx`, `Security`, `Discovery`, and the
HDA/AE/Batch/Commands/DX surfaces) do not yet have catalog entries, so the
same rejection class persists for their non-DA IIDs. Extending the
catalog to those specs is the open follow-up.

### Issue B: TestServer activation requires DCOM ACL grant

Locally built TestServer EXE activation fails with HRESULT `0x80080005` and
DCOM event log 10010 ("server did not register with DCOM within the
required timeout") on a fresh install. The fix is to run
grant-testserver-acl once after
the EXE is registered, which writes the DCOM AppID Launch/Access SD for
the TestServer CLSID. Combined with the Issue E config-XML patch above,
the matrix reaches 104/104.

### Issue C: OPCEnum **activation** requires DCOM ACL grant

`opcclassic.discovery.enumerate_servers` and ProgID-based connect paths
use `IRemoteSCMActivator::RemoteCreateInstance`. The activator path
reuses the same OPC connection credentials used for the target server
and upgrades weak activation protection to
`RPC_C_AUTHN_LEVEL_PKT_INTEGRITY`. Operators must grant the calling
identity DCOM Launch/Activation and Access permissions on the OPCEnum
AppID `{13486D44-4821-11D2-A494-3CB306C10000}` (note: AppID is distinct
from the CLSID `{13486D51-4821-11D2-A494-3CB306C10000}`, differing in
one hex digit). grant-opcenum-acl
automates this once per host.

### Issue D: OPCEnum **data-port bind** rejects IOPCServerList(2)

Independent of Issue C: even when activation succeeds
(`IActivation::RemoteActivation` opnum 0 returns a valid OBJREF with
`IPID` + `DUALSTRINGARRAY` bindings for the OPCEnum data port), the
subsequent DCE bind to the data port for `IOPCServerList2` (or after
downgrade, `IOPCServerList`) returns `bind_ack` with
`PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED` for both IIDs.

This is the **same root-cause class** as Issue A, but for OPCEnum's
`IOPCServerList` / `IOPCServerList2` IIDs, which are not in any pre-bind
catalog today (`DcomOpcEnumCallChannelFactory.CreateObjectChannelAsync`
calls `DcomCallChannelFactory.ConnectAsync` with `preBindIids: null`).

The wire trace shows the client sends a bind for `IOPCServerList2` alone
— and OPCEnum responds PROVIDER_REJECTION. After the downgrade fallback,
the bind for `IOPCServerList` alone is also rejected. The single-IID
bind shape is correct DCE; what is unusual is that OPCEnum rejects both
IIDs that it is responsible for advertising. The likely cause is that
the bind PDU is missing a presentation-context attribute OPCEnum
requires (e.g., the OPC-Common type-library reference, or a specific
transfer-syntax alternative beyond the default NDR
`8a885d04-1ceb-11c9-9fe8-08002b104860` v2.0).

**Workaround:** pass `--da-clsid <CLSID>` to the probe driver (bypasses
OPCEnum and dials the target server's `IRemoteActivation` directly) —
this is what the current 25/95 OK baseline uses.

**Fix surface:** an `OpcSpecCatalog.Discovery` collection that the
`DcomOpcEnumCallChannelFactory` passes through is the suspected fix. If
the issue persists after adding the IID to the catalog, per-tower-syntax
presentation-context experiments (adding the OPC-Common type-library
64-bit transfer-syntax alternative) are the next layer.

## What works today (Matrikon DA)

All DA tools work end-to-end against live Matrikon Simulation Server
when activated via `--da-clsid F8582CF2-88FB-11D0-B850-00C0F0104305`.
All write/read/subscribe/poll/properties paths are validated against the
live server.

## What is blocked today

- **Non-DA AlterContext on Matrikon** (CPX `get_complex_type` /
  `get_dictionary`, Security): same Issue A class for non-DA IIDs.
  Extending `OpcSpecCatalog` per spec to declare the full IID set in
  the initial bind is the planned resolution.
- **`--da-progid` activation** ([Issue D](#issue-d-opcenum-data-port-bind-rejects-iopcserverlist2)):
  OPCEnum data-port rejects `IOPCServerList`(`2`) bind. Catalog
  extension is the suspect-fix; if catalog alone doesn't close it,
  presentation-context attribute experiments are the next step.
- **`discovery.enumerate_servers`**: same Issue D root cause.
- **Non-DA specs (HDA / AE / Batch / Commands / DX / XML-DA)**:
  Matrikon Simulation does not implement them. The Foundation
  `OpcTestServer` + bundled spec plugins cover all those specs as the
  alternative probe target.
