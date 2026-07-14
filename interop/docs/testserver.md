# OPC Foundation TestServer interop

A minimal OPC DA 2.05a test server (and matching native client) used as a
deterministic interop target for byte-exact wire-format validation. Both
are MIT-licensed and source-traceable — when our managed proxy fails a
call against TestServer we can debug into the MIDL-generated stub to see
exactly which byte rejected it.

## What's in `interop/`

The entire OPC Foundation
[OPC-Classic-CoreComponents](https://github.com/OPCFoundation/OPC-Classic-CoreComponents)
repository is vendored in pruned/restructured form under
`interop/` so the native TestServer + proxy/stub DLLs can be
built without an external clone. Key paths:

| Path | Purpose |
|------|---------|
| Vendored OPC Foundation TestServer `COpcTestServer.{h,cpp}` | Server class derived from `COpcDaServer`. Exposes `IOPCServer` / `IOPCBrowseServerAddressSpace` / `IOPCItemProperties` on the server object and `IOPCItemMgt` / `IOPCSyncIO` / `IOPCAsyncIO2` / `IOPCGroupStateMgt` on the group object. |
| Vendored OPC Foundation TestServer `COpcTestGroup.h` | Group class derived from `COpcDaGroup`. |
| Vendored OPC Foundation TestServer `OpcTestServer.{cpp,idl,rc}` | Local-server entry point (`_tWinMain`), per-bitness CLSIDs, MIDL IDL for the empty server interface. |
| Vendored OPC Foundation TestServer `OpcTestServer.config.xml` | 3-item address space: `Test.Int32` (=42), `Test.Float` (=3.14159), `Test.String` ("OPC Test"), each carrying property 6 (Item Quality) = 100. |
| Vendored OPC Foundation TestClient `OpcTestClient.cpp` | Native console exerciser — enumerates DA 2.0 servers via OpcEnum, calls `GetStatus`, then runs the repo lifecycle extension (`AddGroup`, `AddItems`, sync read/write, cleanup). |
| `Shared` | Sample server scaffolding TestServer derives from (`OpcUtilityClasses`, `SampleServerClasses`, `SampleDevice`, `SampleServer205`). |
| `Common`, `DataAccess`, `Security` | Per-spec IDLs + proxy/stub builds for `opccomn_ps.dll`, `opcproxy.dll`, `opcsec_ps.dll` (the DLLs the TestServer's COM activation needs for marshalling). |
| `Include` | Shared headers (CATID GUIDs, error codes). |
| `CMakeLists.txt`, `cmake/` | Upstream CMake harness — invoked by build-testserver. |
| `docker` | Windows Server Core 2022 + VS 2022 build-tools docker harness for reproducible builds without a local VS install. |
| `build.ps1` | One-shot build entry point. |

See `interop` for the folder layout, local divergences, and vendoring rationale. The OPC Foundation MIT License 1.00 grant lives in
the file headers; `LICENSE.md` at the vendor root is the umbrella
OPC Foundation license that governs the broader specification suite.

## CLSIDs

- **x64**: `F8582CF9-88FB-11DA-A5ED-0060B0692061`
- **x86**: `F8582CF4-88FB-11DA-A5ED-0060B0692061`

Both register under the `OPC DA 2.05a Test Server` ProgID prefix.

## Building

### Option 1 — build-testserver (recommended)

```powershell
.\interop\tools\build-testserver.ps1            # Release x64
.\interop\tools\build-testserver.ps1 -Clean     # wipe build\ first
```

The script discovers VS's bundled CMake (or any cmake.exe on PATH),
configures `x64`, and builds the
`OpcTestServer`, `OpcTestClient`, `OpcCategoryManager`, and the proxy/stub
targets. Output lands in
`interop\build\x64\Release`.

Prerequisites: Visual Studio 2022 17.14+ (Desktop development with
C++ + ATL + Win11 SDK + MSVC v14.44 or later), CMake 3.20+ (bundled
with VS).

### Option 2 — upstream `build.ps1` directly

```powershell
cd interop
.\build.ps1
```

Builds **all** native targets for both platforms and installs outputs under
`out\`. The vendored build does not produce MSI packages.

## Installation / registration

After building, register the TestServer with DCOM so the managed Opc.Classic
client can activate it. Use the local no-MSI registration path below, or install the official OPC
Foundation Core Components package externally when validating a machine-wide
deployment.

### register-testserver (local x64, no MSI)

```powershell
# From an ELEVATED 64-bit PowerShell window:
.\interop\tools\register-testserver.ps1
.\interop\tools\grant-testserver-acl.ps1
```

The script performs the no-MSI setup needed for x64 DCOM activation:

1. Copies and registers the full proxy/stub set in canonical dependency
   order: `opccomn_ps.dll`, `opcproxy.dll`, `opc_aeps.dll`, `opcbc_ps.dll`,
   `OpcCmdPs.dll`, `OpcDxPs.dll`, `opchda_ps.dll`, and `opcsec_ps.dll`.
   Missing optional artifacts are reported; DA requires at least
   `opccomn_ps.dll` and `opcproxy.dll`.
2. Copies `OpcTestServer_x64.config.xml` alongside the EXE and corrects the
   upstream generated `<CLSID>` value to the runtime TestServer CLSID.
3. Runs `OpcCategoryManager.exe /RegServer` when present so x64 OPC category
   enumeration is registered.
4. Runs `OpcTestServer_x64.exe /regserver` with `%SystemRoot%\System32`
   as the working directory.
5. Writes compatibility HKLM entries for the CLSID, LocalServer32, AppID,
   versioned and version-independent ProgIDs, and Implemented Categories for
   DA 1.0, DA 2.0, and DA 3.0.

Defaults to looking for the EXE and sibling proxy/stub DLLs under
`interop\build\x64\Release`; pass `-ExePath` to override.

To remove the TestServer entries and the System32 proxy/stub DLLs
copied by this script: `.\interop\tools\register-testserver.ps1 -Unregister`.
Missing files are skipped; mismatched files without this script's install
marker are left in place to avoid removing an unrelated external install.

Validate from a fresh elevated 64-bit PowerShell, then probe from a
non-elevated shell:

```powershell
# Elevated
.\interop\tools\build-testserver.ps1
.\interop\tools\register-testserver.ps1
.\interop\tools\grant-testserver-acl.ps1
Test-Path "$env:SystemRoot\System32\opccomn_ps.dll"
Test-Path "$env:SystemRoot\System32\opcproxy.dll"

# Non-elevated
python tools\probe_servers.py --da-clsid F8582CF9-88FB-11DA-A5ED-0060B0692061 --request-timeout 30 > .\testsvr.json
python tools\probe_report.py .\testsvr.json | Select-Object -First 30
```

Expected probe results: `opcclassic.da.connect` succeeds and
`opcclassic.da.get_status` reports `Running`.

If `opcclassic.da.connect` times out and the System event log contains
DistributedCOM event 10010 (`The server {F8582CF9-88FB-11DA-A5ED-0060B0692061}
did not register with DCOM within the required timeout.`), first verify
that both elevated setup scripts completed and that the proxy/stub DLLs are
present in `%SystemRoot%\System32`. If an elevation prompt was canceled,
the HKLM entries may exist while the required System32 files or AppID
Launch/Access descriptors are still missing.

### DCOM ACL

`AllowEveryoneAccess` controls the TestServer's application-level OPC access
checks after activation. It does not write the AppID `LaunchPermission` or
`AccessPermission` security descriptors used by DCOM SCM, so it cannot make a
fresh-machine SCM activation permissive.

Run `grant-testserver-acl.ps1` after registration to grant the current caller
Launch, Activation, and Access rights. To grant a group instead:

```powershell
.\interop\tools\grant-testserver-acl.ps1 -Account "BUILTIN\Distributed COM Users"
```

The helper is idempotent and requires elevated 64-bit PowerShell. Use
`-Unregister` to remove the selected account's ACEs.

### Registration troubleshooting

The standard build + register + ACL flow is the current green TestServer
path. If activation returns `CO_E_SERVER_EXEC_FAILURE (0x80080005)` or logs
DistributedCOM event 10010, verify:

1. the config file exists beside `OpcTestServer_x64.exe` and contains CLSID
   `F8582CF9-88FB-11DA-A5ED-0060B0692061`;
2. all proxy/stub DLLs produced by the build are present and registered in
   `%SystemRoot%\System32`;
3. `OpcCategoryManager.exe /RegServer` and
   `OpcTestServer_x64.exe /RegServer` completed from elevated 64-bit
   PowerShell;
4. no stale HKCU COM registration shadows the HKLM TestServer entries; and
5. the TestServer AppID launch/access ACL permits the calling identity.

The registration spec documents the exact files, ordering, compatibility
keys, SCM ACL distinction, and unregistration behavior.

## Running against the managed Opc.Classic stack

Once the TestServer is registered, the same `mcp_driver`
script that targets Matrikon works against TestServer — just point at
the x64 CLSID:

```powershell
python mcp/mcp_driver.py --clsid F8582CF9-88FB-11DA-A5ED-0060B0692061
```

Or use the convenience `--testserver` switch:

```powershell
python mcp/mcp_driver.py --testserver
```

Expected output (read step):

```
Test.Int32   value=42     type=VT_I4    q=0x00C0  hr=0x00000000
Test.Float   value=3.14159 type=VT_R4   q=0x00C0  hr=0x00000000
Test.String  value='OPC Test' type=VT_BSTR q=0x00C0  hr=0x00000000
```

## Why dual-target (TestServer + Matrikon)

| Server     | Primary use cases                                              |
|------------|----------------------------------------------------------------|
| TestServer | DA 2.x lifecycle (AddGroup, AddItems, SyncIO Read/Write,       |
|            | `IOPCBrowseServerAddressSpace`, `IOPCItemProperties`) with     |
|            | source-level wire-byte debugging via the MIDL stubs            |
| Matrikon   | DA 3.0 (`IOPCBrowse`, `IOPCItemIO`); large item-set fuzzing;   |
|            | broad vendor-quirk coverage                                    |

When TestServer and Matrikon disagree on the wire format for a
particular method, the MIDL-generated format strings under
the vendored OPC Foundation DataAccess proxy/stub sources are the authoritative
reference — they're literally the bytes the OPC proxy/stub DLLs
marshal.
