# TestServer registration spec (no-MSI local registration)

**Track BH1 audit output.** This document enumerates every file, registry
entry, COM CLSID, AppID, category, and self-registration step needed when
installing the OPC Foundation `OpcTestServer_x64.exe` and the proxy/stub DLLs
required for DCOM marshalling. It was originally derived from the upstream
installer manifests, but local WiX/MSI packaging is no longer vendored. It is
the source-of-truth reference for auditing `tools/register-testserver.ps1` and
the suspected `CO_E_SERVER_EXEC_FAILURE` root cause (Issue B in
`docs/interop/probe-coverage.md`).

## Source artifacts audited

- Legacy upstream installer manifests (not vendored in this tree)
- `samples\OpcTestServer\OpcTestServer.cpp`
- `samples\OpcTestServer\OpcTestServer.idl`
- `samples\OpcTestServer\OpcTestServer.config.xml`
- `ext\redist\CoreComponents\src\Shared\OpcUtilityClasses`

## Canonical install layout (x64)

All files install under `[CommonFiles64Folder]\OPC Foundation\Bin\`. On a
default Windows install this resolves to `C:\Program Files\Common Files\OPC Foundation\Bin\`
for 64-bit binaries. **None of the proxy/stub DLLs or the TestServer EXE
install into `System32`.**

| File                         | Source                              | Self-registration                                 |
| ---------------------------- | ----------------------------------- | ------------------------------------------------- |
| `opccomn_ps.dll`             | `MergeModule.wxs:comp_opccomn_ps`   | `SelfRegCost=1` — MSI calls `DllRegisterServer`   |
| `opcproxy.dll`               | `MergeModule.wxs:comp_opcproxy`     | `SelfRegCost=1`                                   |
| `opc_aeps.dll`               | `MergeModule.wxs:comp_opc_aeps`     | `SelfRegCost=1`                                   |
| `opcbc_ps.dll`               | `MergeModule.wxs:comp_opcbc_ps`     | `SelfRegCost=1`                                   |
| `OpcCmdPs.dll`               | `MergeModule.wxs:comp_OpcCmdPs`     | `SelfRegCost=1`                                   |
| `OpcDxPs.dll`                | `MergeModule.wxs:comp_OpcDxPs`      | `SelfRegCost=1`                                   |
| `opchda_ps.dll`              | `MergeModule.wxs:comp_opchda_ps`    | `SelfRegCost=1`                                   |
| `opcsec_ps.dll`              | `MergeModule.wxs:comp_opcsec_ps`    | `SelfRegCost=1`                                   |
| `OpcCategoryManager.exe`     | `MergeModule.wxs:comp_OpcCategoryManager` | CustomAction `RegisterOpcCategoryManager` runs `OpcCategoryManager.exe /RegServer` (deferred, no impersonation = SYSTEM) |
| `OpcTestServer_x64.exe`      | `Installer.wxs:comp_OpcTestServer`  | CustomAction `RegisterTestServer` runs `OpcTestServer_x64.exe /RegServer` (deferred, SYSTEM) — gated on `TestServer` feature being selected |
| `OpcTestServer_x64.config.xml` | `Installer.wxs:comp_OpcTestServerConfig` | None (loaded by the EXE at runtime) |
| `OpcTestClient_x64.exe`      | `Installer.wxs:comp_OpcTestClient`  | None (standalone client app)                      |

> **Important:** the legacy installer also includes the x86 components when the
> x64 package is selected — `MergeModuleX86` reference inside `Installer.wxs`
> brings in the x86 DLLs to `[SystemFolder]` (= `SysWOW64`). That gives
> 32-bit COM clients access to the proxy/stub DLLs even on a 64-bit box.

## TestServer COM identity (from IDL + cpp)

`OpcTestServer.idl` and `OpcTestServer.cpp` agree on the x64 identity:

| Field        | Value                                            | Source                         |
| ------------ | ------------------------------------------------ | ------------------------------ |
| TypeLib UUID | `F8582CF7-88FB-11DA-A5ED-0060B0692061`           | `OpcTestServer.idl:36`         |
| coclass UUID | `F8582CF8-88FB-11DA-A5ED-0060B0692061`           | `OpcTestServer.idl:45`         |
| **CLSID**    | **`F8582CF9-88FB-11DA-A5ED-0060B0692061`**       | `OpcTestServer.cpp:50` (`OPC_IMPLEMENT_LOCAL_SERVER`) |
| Category     | `CATID_OPCDAServer20` (`63D5F432-CFE4-11D1-B2C8-0060083BA1FB`) | `OpcTestServer.cpp:46` (`OPC_CATEGORY_TABLE_ENTRY`) |
| ProgID       | **`OpcTestServer_x64.1`** (verified against `HKLM\SOFTWARE\Classes\CLSID\{F8582CF9-...}\ProgID` after `/RegServer`; macro expansion in `OpcUtilityClasses` does NOT prepend `OPC.`) | macros expand at compile time  |
| Description  | `"OPC DA 2.05a Test Server (x64)"`               | `OpcTestServer.cpp:42`         |

The corresponding x86 build uses `F8582CF4-...` (CLSID) and
`F8582CF3-...` (coclass UUID).

> ⚠️ **`probe-coverage.md` correction note:** the headline says
> `OPC Foundation TestServer x64: CLSID F8582CF9-...` — confirmed correct
> against `OpcTestServer.cpp:50`. (The IDL coclass UUID `F8582CF8` is the
> TypeLib-side identifier, NOT the runtime CLSID; do not confuse the two.)

## What `/RegServer` actually writes (inferred from the OPC_ macros)

The `OPC_IMPLEMENT_LOCAL_SERVER` macro from
`D:\git\marcschier\OPC-Classic-CoreComponents\src\Shared\OpcUtilityClasses`
expands into a class-factory-with-self-registration pattern equivalent to
ATL's `CAtlExeModuleT`. When invoked with `/RegServer`, the EXE writes the
following keys under `HKEY_CLASSES_ROOT` (which is the merged view of
`HKLM\SOFTWARE\Classes` + the per-user `HKCU\Software\Classes`):

```
HKCR\CLSID\{F8582CF9-88FB-11DA-A5ED-0060B0692061}
  (Default) = "OPC DA 2.05a Test Server (x64)"
  AppID = "{F8582CF9-88FB-11DA-A5ED-0060B0692061}"     -- self-referenced AppID
HKCR\CLSID\{F8582CF9-88FB-11DA-A5ED-0060B0692061}\LocalServer32
  (Default) = "<full path to OpcTestServer_x64.exe>"
HKCR\CLSID\{F8582CF9-88FB-11DA-A5ED-0060B0692061}\ProgID
  (Default) = "OpcTestServer_x64.1"                     -- verified
HKCR\CLSID\{F8582CF9-88FB-11DA-A5ED-0060B0692061}\VersionIndependentProgID
  (Default) = "OpcTestServer_x64"
HKCR\CLSID\{F8582CF9-88FB-11DA-A5ED-0060B0692061}\TypeLib
  (Default) = "{F8582CF7-88FB-11DA-A5ED-0060B0692061}"
HKCR\CLSID\{F8582CF9-88FB-11DA-A5ED-0060B0692061}\Implemented Categories\{63D5F432-CFE4-11D1-B2C8-0060083BA1FB}
  (no value — category-presence entry only; CATID_OPCDAServer20)

HKCR\OpcTestServer_x64.1
  (Default) = "OPC DA 2.05a Test Server (x64)"
HKCR\OpcTestServer_x64.1\CLSID
  (Default) = "{F8582CF9-88FB-11DA-A5ED-0060B0692061}"
HKCR\OpcTestServer_x64\CurVer
  (Default) = "OpcTestServer_x64.1"

HKCR\AppID\{F8582CF9-88FB-11DA-A5ED-0060B0692061}
  (Default) = "OPC DA 2.05a Test Server (x64)"
HKCR\AppID\OpcTestServer_x64.exe
  AppID = "{F8582CF9-88FB-11DA-A5ED-0060B0692061}"     -- AppID name lookup

HKCR\TypeLib\{F8582CF7-88FB-11DA-A5ED-0060B0692061}\1.0
  (Default) = "OPC DA 2.05a Test Server (x64)"
HKCR\TypeLib\{F8582CF7-88FB-11DA-A5ED-0060B0692061}\1.0\0\win64
  (Default) = "<full path to OpcTestServer_x64.exe>"   -- TypeLib points at the EXE resource
HKCR\TypeLib\{F8582CF7-88FB-11DA-A5ED-0060B0692061}\1.0\FLAGS
  (Default) = "0"
HKCR\TypeLib\{F8582CF7-88FB-11DA-A5ED-0060B0692061}\1.0\HELPDIR
  (Default) = ""
```

> **Verification action**: ship a probe build that dumps the entire
> registry tree under `HKCR\CLSID\{F8582CF9-...}` and `HKCR\AppID\{...}`
> immediately after `/RegServer` runs, on a clean machine. The above is
> the inferred set from how `OPC_IMPLEMENT_LOCAL_SERVER` interacts with
> the ATL-equivalent local server module; replace with a captured set
> once verified.

**Importantly, the macro DOES NOT write:**
- `HKCR\AppID\{...}\LaunchPermission` (DCOM ACL — REG_BINARY)
- `HKCR\AppID\{...}\AccessPermission`
- `HKCR\AppID\{...}\AuthenticationLevel`
- `HKCR\AppID\{...}\RunAs`
- `HKCR\CLSID\{...}\Elevation\Enabled`

These are **DCOM SCM defaults** when the AppID has no explicit settings:
- Authentication: per-host DefaultAuthenticationLevel (typically CONNECT).
- Launch/Access: per-host DefaultLaunchPermission / DefaultAccessPermission
  (typically `Administrators` only on hardened Windows).
- RunAs: interactive user when launched interactively, otherwise the
  user that activated.

## Proxy/stub DLL registration (what each `DllRegisterServer` writes)

Each of the 8 proxy/stub DLLs (`opccomn_ps`, `opcproxy`, `opc_aeps`,
`opcbc_ps`, `OpcCmdPs`, `OpcDxPs`, `opchda_ps`, `opcsec_ps`) is a MIDL-
generated COM proxy. When `regsvr32` (or MSI's SelfReg) invokes
`DllRegisterServer`, the DLL writes one entry per supported IID:

```
HKCR\Interface\{<IID>}
  (Default) = "<interface friendly name>"
HKCR\Interface\{<IID>}\ProxyStubClsid32
  (Default) = "{<DLL-CLSID>}"                          -- the DLL's own CLSID
HKCR\Interface\{<IID>}\NumMethods
  (Default) = "<count>"
HKCR\Interface\{<IID>}\TypeLib                          -- optional
  (Default) = "{<TLB-GUID>}"
```

And one CLSID entry for the proxy/stub DLL itself:

```
HKCR\CLSID\{<DLL-CLSID>}\InprocServer32
  (Default) = "<full path to the DLL>"
  ThreadingModel = "Both"
```

**Registration order matters**: `opccomn_ps.dll` carries `IOPCCommon` and
`IOPCShutdown` interface IIDs. The other proxy/stub DLLs reference these
IIDs (via TypeLib imports) — so `opccomn_ps.dll` must be registered FIRST.
Otherwise the dependent DLLs will fail to load their type library
references when registering.

## What our `tools/register-testserver.ps1` does TODAY (script-only path)

From `D:\git\marcschier\opc-classic\tools\register-testserver.ps1`:

1. Copies `opccomn_ps.dll` and `opcproxy.dll` to `%SystemRoot%\System32`.
2. Runs `regsvr32 %SystemRoot%\System32\opccomn_ps.dll`.
3. Runs `regsvr32 %SystemRoot%\System32\opcproxy.dll`.
4. Runs `OpcTestServer_x64.exe /RegServer` with working directory =
   `%SystemRoot%\System32`.
5. (Writes some HKLM CLSID/ProgID/LocalServer32/category entries
   directly, per the doc-string at the top of the script.)

## Diff: script vs. canonical (BH2 fix-list)

| # | Item                                       | Installer canonical                       | Script today                          | Verdict       |
| - | ------------------------------------------ | ----------------------------------------- | ------------------------------------- | ------------- |
| 1 | Install path for proxy/stub DLLs            | `[CommonFiles64Folder]\OPC Foundation\Bin\` | `%SystemRoot%\System32`                | Diverges (script's choice still works — System32 is on the DLL search path — but inconsistent with vendor install) |
| 2 | Number of proxy/stub DLLs registered        | All 8 (`opccomn_ps`, `opcproxy`, `opc_aeps`, `opcbc_ps`, `OpcCmdPs`, `OpcDxPs`, `opchda_ps`, `opcsec_ps`) | 2 (`opccomn_ps`, `opcproxy`)          | **Gap — DA-only works; AE/HDA/Batch/Commands/DX/Security marshalling will fail without the missing PS DLLs** |
| 3 | `OpcCategoryManager.exe /RegServer`         | Yes (deferred CustomAction, SYSTEM)       | Not run                               | **Gap — x64 category enumeration needs it (used by the category-resolution helpers in OPCEnum)** |
| 4 | `OpcTestServer_x64.exe /RegServer`          | Yes (deferred CustomAction, SYSTEM)       | Yes                                   | ✅ Match     |
| 5 | `OpcTestServer_x64.config.xml` deployed alongside the EXE | Yes (`comp_OpcTestServerConfig`)          | Not copied                            | **Likely gap — the EXE may load this on startup; without it the EXE could fail to initialize and never register its class factory (could cause `CO_E_SERVER_EXEC_FAILURE`)** |
| 6 | `OpcTestServer_x64.exe` path stability      | Stable (`Common Files\OPC Foundation\Bin\`) | Build directory (`ext\redist\CoreComponents\build\x64\Release\`) | **Risk — if the path contains characters DCOM SCM can't handle, or if SYSTEM lacks read access, activation fails. Build dir is typically OK but worth verifying.** |
| 7 | Registry entries written by `/RegServer`    | CLSID + LocalServer32 + ProgID + AppID + TypeLib + Implemented Categories | Same (the EXE writes them itself)     | ✅ Match (the EXE does the work; script just invokes /RegServer) |
| 8 | DCOM AppID `LaunchPermission` / `AccessPermission` | None written (SCM defaults apply)         | None written                          | ✅ Match     |
| 9 | DCOM AppID `RunAs`                          | None written                              | None written                          | ✅ Match (means activation uses default "Launching User") |
| 10 | Working directory for `/RegServer`         | None set (CustomAction runs with default) | `%SystemRoot%\System32`                | Probably OK — the EXE uses its own image path for LocalServer32, not the working dir. |

## Most-likely root causes for `CO_E_SERVER_EXEC_FAILURE`

Based on the diff above + live verification on the dev box (2026-06-04),
the suspected causes ranked by likelihood:

1. **DCOM AppID has no explicit `LaunchPermission`/`AccessPermission` set.**
   Without these, DCOM SCM falls back to the per-host
   `DefaultLaunchPermission` ACL — which on a typical Windows install
   grants `Administrators` + `INTERACTIVE` only. A non-admin caller
   (for example `REDMOND\mschier` running the probe) gets DCOM access
   denied, and SCM cannot communicate the activation to the launched
   EXE. The symptom from the calling side is a tools/call timeout
   because the SCM keeps re-trying until the channel times out.
   **Verified**: launching `OpcTestServer_x64.exe -Embedding` manually
   (no DCOM involved) succeeds — the EXE stays alive and registers
   its class factory normally. So the EXE itself is healthy; the
   failure is in the SCM activation path.
   **Fix surface**: a new helper `tools/grant-testserver-acl.ps1`
   modeled on `tools/grant-opcenum-acl.ps1` that writes a permissive
   `LaunchPermission` + `AccessPermission` SD on the TestServer AppID
   (CLSID `{F8582CF9-...}`).
2. **Missing `OpcTestServer_x64.config.xml`** alongside the EXE. BH2's
   fix copies it; live verification confirmed the absence alone does
   NOT cause activation timeout (the EXE runs without the XML). Keep
   the copy because the config file controls the TestServer's runtime
   tag list — without it the server is empty and DA browse/read tests
   would return no items.
3. **EXE crashes for other reasons** (missing DLL dependency). Ruled
   out: manual launch succeeds with the EXE staying alive.
4. **DCOM SCM cannot read the EXE path**. Ruled out for the dev box
   path; may matter on hardened Windows installs with strict NTFS ACLs.
5. **Missing proxy/stub registration** for one of the DA-marshalled
   interfaces. Only matters AFTER activation succeeds (DCOM uses the
   proxy/stub DLLs for the call marshalling); not the cause of
   activation timeout itself. BH2's full-DLL registration is still
   correct for ensuring AE/HDA/Batch/Commands/DX/Security call paths
   work once activation succeeds.

## BH2 actionable script changes (next-track fix list)

1. **Copy `OpcTestServer_x64.config.xml`** to the same directory as the
   EXE before running `/RegServer`. (Cited from the legacy
   `Installer.wxs:85-90` `comp_OpcTestServerConfig` component.)
2. **Register all 8 proxy/stub DLLs**, in the order:
   `opccomn_ps.dll` → `opcproxy.dll` → `opc_aeps.dll` → `opcbc_ps.dll`
   → `OpcCmdPs.dll` → `OpcDxPs.dll` → `opchda_ps.dll` → `opcsec_ps.dll`.
   (Cited from `MergeModule.wxs:156-219`.)
3. **Register `OpcCategoryManager.exe /RegServer`** before the
   TestServer. (Cited from `MergeModule.wxs:233-253`.)
4. **Mirror unregistration order** on `-Unregister` (reverse of
   registration; TestServer `/UnRegServer` first, then EXEs, then DLLs
   from `opcsec_ps` back to `opccomn_ps`).
5. Document the divergence from the installer `Common Files\OPC Foundation\Bin\`
   install path (System32 is acceptable but non-standard).
6. Add a unit test under `tests/Opc.Classic.Tools.Tests/` (new project
   if needed) that parses the script and asserts each installer-cited entry
   from the table above is covered.

## What canonical install would do for non-Matrikon-cwd-style test

If a user installs the upstream `OPC-Classic-CoreComponents` MSI via
`msiexec /i OpcCoreComponents_x64.msi /qn ADDLOCAL=Complete,TestServer`,
the post-install state is:

- All 8 proxy/stub DLLs registered in `Common Files\OPC Foundation\Bin\`.
- `OpcCategoryManager.exe` registered.
- `OpcTestServer_x64.exe` registered with CLSID
  `F8582CF9-88FB-11DA-A5ED-0060B0692061` and accompanied by its config
  XML.
- `OpcEnum.exe` is NOT installed by the x64 path (it's x86-only per
  `MergeModule.wxs:88-94`); the x64 MSI installs the x86 merge module
  which provides OpcEnum on the SysWOW64 side.

`tools/register-testserver.ps1` is a no-MSI shortcut for developer
machines. The BH2 fixes above bring it to functional parity with the
MSI for the **TestServer-only DA case**; full multi-spec marshalling
would also require the additional proxy/stub registrations from the
table.

## Verified live findings (2026-06-04, post-DR1)

Live diagnostic instrumentation (`OPC_CLASSIC_DCOM_WIRE_DUMP=1`,
`DcomCallChannel.cs` bind+invoke tracing) against a partially-registered
TestServer revealed the actual symptom chain:

1. **MCP `da.connect` opens TCP to localhost:135** (RPCSS). ✅
2. **NTLM SPNEGO bind succeeds**: BIND PDU written, BIND_ACK received
   (238-byte auth body), AUTH3 written, IActivation presentation
   context accepted. ✅
3. **IActivation::RemoteActivation REQUEST written** (opnum 0,
   150-byte payload, 182-byte signed stub). ✅
4. **SCM launches TestServer** — process appears in Task Manager
   listening for the SCM's class-factory registration callback. ✅
5. **TestServer never calls `CoRegisterClassObject`** within the
   SCM timeout window (~2 minutes). The Windows Event Log shows
   recurring `Microsoft-Windows-DistributedCOM` event:
   > `The server {F8582CF9-88FB-11DA-A5ED-0060B0692061} did not
   > register with DCOM within the required timeout.`
6. **SCM eventually returns `CO_E_SERVER_EXEC_FAILURE` (0x80080005)**
   to our managed activation client (~180s after the request was
   sent). Our MCP probe times out before this response arrives
   (default 60s).

### Why TestServer fails to register — DEEP-DIVE (DR3 round 3-5)

After extensive bisection:

1. **HKCU shadow registration**: an earlier non-elevated `OpcTestServer_x64.exe /regserver`
   run from a DIFFERENT build path (`D:\git\marcschier\OPC-Classic-CoreComponents\build\...`)
   left HKCU entries that override HKLM via HKCR merge. Removing them
   via `Remove-Item HKCU:\SOFTWARE\Classes\CLSID\{F8582CF9-...}` etc.
   restored HKLM as the source of truth. **Did NOT fix the activation**.
2. **DCOM hardening (KB5004442)**: TestServer's `COpcComModule::Run` and
   `COpcComModule::RegisterFromFiles` called `CoInitializeSecurity` with
   `RPC_C_AUTHN_LEVEL_PKT` (level 4). Microsoft's June-2021 DCOM
   hardening REQUIRES `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY` (level 5) for
   servers. **Fixed in `ext/redist/CoreComponents/src/Shared/OpcUtilityClasses/COpcComModule.cpp`**
   (both call sites). **Did NOT fix the activation** either — the
   fundamental issue is elsewhere.
3. **Comparison with Matrikon (working baseline)**: Foundation TestClient
   successfully activates Matrikon's CoCreateInstance. Matrikon is
   registered as a **Windows Service** (`MatrikonOPC Server for
   Simulation and Testing`, x86 LocalServer process under WoW64). SCM
   activates services via a different code path than EXE-launch.
4. **TestServer activation fails for ALL DCOM clients**: Foundation
   `OpcTestClient_x64.exe` (Microsoft's native DCOM client) gets the
   identical `0x80080005` failure. Our managed MCP client gets the same.
   This rules out our managed DCOM stack as the cause.

### Conclusion

The TestServer C++ EXE has a fundamental activation-time problem that
prevents it from successfully registering its class factory with SCM
within the required timeout window, regardless of which DCOM client is
attempting activation. The issue persists across:

- Fresh `tools/build-testserver.ps1 -Clean` builds.
- Both binary builds present on the dev box.
- HKLM-only registration (after HKCU shadow removal).
- Both Microsoft's native DCOM client and our managed client.

Root cause is **TestServer-side** (C++ source or DCOM/Windows
configuration), NOT in our codebase. Fixing it requires either:

- Source-level debug of TestServer's `COpcComModule::Run` to determine
  why `CoRegisterClassObject` or its surrounding plumbing aborts.
- Upstream fix from OPC Foundation `OPC-Classic-CoreComponents`.
- Switch to a different reference server (Matrikon, which is the only
  one currently demonstrating end-to-end DA activation).

### Recommended workaround

Use **Matrikon OPC Simulation Server** as the primary DA 2.05a reference
server in the cross-impl matrix. Foundation `OpcTestClient_x64.exe` and
our managed MCP probe both successfully activate Matrikon end-to-end.
Matrikon is the only DA server currently demonstrating full activation
on this dev box. The `testserver` profile in
[`tools/probe_matrix.py`](../../tools/probe_matrix.py) remains the
"strict" reference for when TestServer activation is fixed upstream,
but the `matrikon` profile is the practical baseline.

### Diagnostic helper: `OPC_CLASSIC_DCOM_WIRE_DUMP=1`

`src/Opc.Classic.Dcom/Transport/DcomCallChannel.cs` honors the
`OPC_CLASSIC_DCOM_WIRE_DUMP=1` environment variable. When set on the
MCP process, every DCE/RPC BIND, ALTER_CONTEXT, and REQUEST/RESPONSE
exchange writes a `[bind-trace]` or `[wire]` line to stderr, including
HRESULTs and full request/response hex. Use this when diagnosing
opaque DCOM hangs — it surfaces the exact opnum + IID + byte counts
the channel saw, which is otherwise invisible to the MCP framework's
own log.

```powershell
$env:OPC_CLASSIC_DCOM_WIRE_DUMP = '1'
.\tools\run-cross-impl-matrix.ps1 -Profile testserver
```
