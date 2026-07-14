# TestServer registration spec (no-MSI local registration)

This document enumerates every file, registry entry, COM CLSID, AppID,
category, and self-registration step needed when installing the OPC
Foundation `OpcTestServer_x64.exe` and the proxy/stub DLLs required for
DCOM marshalling. It is derived from the upstream installer manifests
(local WiX/MSI packaging is not vendored). It is the source-of-truth
reference for auditing register-testserver and the
suspected `CO_E_SERVER_EXEC_FAILURE` root cause (Issue B in
probe-coverage).

> **Status (June 2026)**: TestServer activates end-to-end from the
> in-repo MCP matrix after the standard build + register + ACL flow.
> Activation uses LRPC (`ncacn_np`, protocol ID `0x0F`) via
> `src/Opc.Classic.Dcom/Transport/LocalNamedPipeTransport.cs`
> (kernel `NamedPipeClientStream`, bypasses SMB2). `DaClientTools`,
> `AeClientTools`, and `HdaClientTools` request both `ncacn_ip_tcp`
> and `ncacn_np` from `IActivation::RemoteActivation`; the local SCM
> picks the matching protocol and the
> `TransportFactoryDispatcher` (also in
> `src/Opc.Classic.Dcom/Transport/`) routes the per-OBJREF
> resolver-binding to the right transport (TCP for sockets,
> `LocalNamedPipeTransport` for local pipes,
> `NcacnNpTransport` for remote pipes via SMB2). The DCOM cross-impl
> matrix profile `testserver` is enabled by default and reports a
> green end-to-end result with no regressions.

## Source artifacts audited

- Legacy upstream installer manifests (not vendored in this tree)
- `OpcTestServer.cpp` (vendored OPC Foundation TestServer)
- `OpcTestServer.idl` (vendored OPC Foundation TestServer)
- `OpcTestServer.config.xml` (vendored OPC Foundation TestServer)
- `OpcUtilityClasses` (vendored OPC Foundation shared utilities)

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
| TypeLib UUID | `F8582CF7-88FB-11DA-A5ED-0060B0692061`           | `OpcTestServer`         |
| coclass UUID | `F8582CF8-88FB-11DA-A5ED-0060B0692061`           | `OpcTestServer`         |
| **CLSID**    | **`F8582CF9-88FB-11DA-A5ED-0060B0692061`**       | `OpcTestServer` (`OPC_IMPLEMENT_LOCAL_SERVER`) |
| Category     | `CATID_OPCDAServer20` (`63D5F432-CFE4-11D1-B2C8-0060083BA1FB`) | `OpcTestServer` (`OPC_CATEGORY_TABLE_ENTRY`) |
| ProgID       | **`OpcTestServer_x64.1`** (verified against `HKLM\SOFTWARE\Classes\CLSID\{F8582CF9-...}\ProgID` after `/RegServer`; macro expansion in `OpcUtilityClasses` does NOT prepend `OPC.`) | macros expand at compile time  |
| Description  | `"OPC DA 2.05a Test Server (x64)"`               | `OpcTestServer`         |

The corresponding x86 build uses `F8582CF4-...` (CLSID) and
`F8582CF3-...` (coclass UUID).

> ⚠️ **`probe-coverage.md` correction note:** the headline says
> `OPC Foundation TestServer x64: CLSID F8582CF9-...` — confirmed correct
> against `OpcTestServer`. (The IDL coclass UUID `F8582CF8` is the
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

## `register-testserver.ps1` behavior

The no-MSI script performs the following registration work:

1. Copies the full canonical proxy/stub set to `%SystemRoot%\System32` and
   registers it in dependency order:
   `opccomn_ps.dll` → `opcproxy.dll` → `opc_aeps.dll` → `opcbc_ps.dll` →
   `OpcCmdPs.dll` → `OpcDxPs.dll` → `opchda_ps.dll` → `opcsec_ps.dll`.
   Missing artifacts produce warnings; DA requires the first two.
2. Records the installed path and SHA-256 for each copied DLL so
   unregistration removes only files installed by this script and leaves
   replaced or externally managed copies intact.
3. Copies `OpcTestServer_x64.config.xml` beside the EXE. It patches the
   upstream generated `<CLSID>` from the IDL coclass UUID (`F8582CF8-...`)
   to the runtime local-server CLSID (`F8582CF9-...`) before activation.
4. Runs `OpcCategoryManager.exe /RegServer` when the artifact is present.
5. Runs `OpcTestServer_x64.exe /regserver` from the native System32 working
   directory.
6. Writes compatibility HKLM entries for CLSID, LocalServer32, AppID,
   versioned/version-independent ProgIDs (including the upstream `OPC.`
   aliases), and DA 1.0/2.0/3.0 Implemented Categories.
7. On `-Unregister`, runs the TestServer unregister action, removes the
   compatibility keys, unregisters proxy/stubs in reverse order, unregisters
   `OpcCategoryManager`, and removes the script's install markers.

This developer path registers native x64 proxy/stubs from System32. Official
machine-wide installation uses `Common Files\OPC Foundation\Bin`. The script
requires elevated 64-bit PowerShell so SCM and native COM registration see the
same HKLM/System32 view.

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

`register-testserver.ps1` is a no-MSI shortcut for developer machines. It
installs every proxy/stub artifact produced by the build.

## Current validation status

The standard build + register + ACL flow activates TestServer end-to-end in
the in-repo MCP matrix. The required installation state includes:

- full proxy/stub registration in canonical order;
- deploying and correcting `OpcTestServer_x64.config.xml`;
- registering `OpcCategoryManager`;
- writing the stable HKLM compatibility identities and DA categories;
- removing stale per-user registrations that can shadow HKLM through HKCR;
- applying TestServer launch/access ACLs on restrictive hosts; and
- using packet integrity or stronger authentication under current Windows
  DCOM hardening.

If a clean machine still reports `CO_E_SERVER_EXEC_FAILURE` or
DistributedCOM event 10010, compare the installed state against the steps
above before investigating the RPC client. The `testserver` matrix profile
is the acceptance reference.

### Diagnostic helper: `OPC_CLASSIC_DCOM_WIRE_DUMP=1`

`DcomCallChannel` honors the
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
