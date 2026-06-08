# OPC Foundation TestServer interop

A minimal OPC DA 2.05a test server (and matching native client) used as a
deterministic interop target for byte-exact wire-format validation. Both
are MIT-licensed and source-traceable — when our managed proxy fails a
call against TestServer we can debug into the MIDL-generated stub to see
exactly which byte rejected it.

## What's in `external/redist/`

The entire OPC Foundation
[OPC-Classic-CoreComponents](https://github.com/OPCFoundation/OPC-Classic-CoreComponents)
repository is vendored in pruned/restructured form under
`external/redist/` so the native TestServer + proxy/stub DLLs can be
built without an external clone. Key paths:

| Path | Purpose |
|------|---------|
| Vendored OPC Foundation TestServer `COpcTestServer.{h,cpp}` | Server class derived from `COpcDaServer` (in `src/Shared/SampleServerClasses/`). Exposes `IOPCServer` / `IOPCBrowseServerAddressSpace` / `IOPCItemProperties` on the server object and `IOPCItemMgt` / `IOPCSyncIO` / `IOPCAsyncIO2` / `IOPCGroupStateMgt` on the group object. |
| Vendored OPC Foundation TestServer `COpcTestGroup.h` | Group class derived from `COpcDaGroup`. |
| Vendored OPC Foundation TestServer `OpcTestServer.{cpp,idl,rc}` | Local-server entry point (`_tWinMain`), per-bitness CLSIDs, MIDL IDL for the empty server interface. |
| Vendored OPC Foundation TestServer `OpcTestServer.config.xml` | 3-item address space: `Test.Int32` (=42), `Test.Float` (=3.14159), `Test.String` ("OPC Test"), each carrying property 6 (Item Quality) = 100. |
| Vendored OPC Foundation TestClient `OpcTestClient.cpp` | 179-line console exerciser — enumerates DA 2.0 servers via OpcEnum, calls `GetStatus` on each. |
| `src/Shared/` | Sample server scaffolding TestServer derives from (`OpcUtilityClasses`, `SampleServerClasses`, `SampleDevice`, `SampleServer205`). |
| `src/Common/`, `src/DataAccess/`, `src/Security/` | Per-spec IDLs + proxy/stub builds for `opccomn_ps.dll`, `opcproxy.dll`, `opcsec_ps.dll` (the DLLs the TestServer's COM activation needs for marshalling). |
| `src/Include/` | Shared headers (CATID GUIDs, error codes). |
| `CMakeLists.txt`, `cmake/` | Upstream CMake harness — invoked by `external\tools\build-testserver.ps1`. |
| `external/docker/` | Windows Server Core 2022 + VS 2022 build-tools docker harness for reproducible builds without a local VS install. |
| `build.ps1`, `docker-build.ps1` | One-shot build entry points (native + docker). |

See `external/redist/VENDORED.md` for the snapshot provenance and
re-sync workflow. The OPC Foundation MIT License 1.00 grant lives in
the file headers; `LICENSE.md` at the vendor root is the umbrella
OPC Foundation license that governs the broader specification suite.

## CLSIDs

- **x64**: `F8582CF9-88FB-11DA-A5ED-0060B0692061`
- **x86**: `F8582CF4-88FB-11DA-A5ED-0060B0692061`

Both register under the `OPC DA 2.05a Test Server` ProgID prefix.

## Building

### Option 1 — `external\tools\build-testserver.ps1` (recommended)

```powershell
.\external\tools\build-testserver.ps1            # Release x64
.\external\tools\build-testserver.ps1 -Clean     # wipe build\ first
```

The script discovers VS's bundled CMake (or any cmake.exe on PATH),
configures `external\redist\build\x64`, and builds the
`OpcTestServer`, `OpcTestClient`, `OpcCategoryManager`, `opccomn_ps`
and `opcproxy` targets. Output lands in
`external\redist\build\x64\Release\`.

Prerequisites: Visual Studio 2022 17.14+ (Desktop development with
C++ + ATL + Win11 SDK + MSVC v14.44 or later), CMake 3.20+ (bundled
with VS).

### Option 2 — upstream `build.ps1` directly

```powershell
cd external\redist
.\build.ps1
```

Builds **all** native targets for both platforms and installs outputs under
`out\`. MSI packaging was removed from the vendored tree.

### Option 3 — Docker (fully reproducible, no local VS)

```powershell
cd external\redist
.\docker-build.ps1
```

Requires Docker Desktop with **Windows containers** enabled. The
image (~10 GB) builds VS 2022 build tools inside
`mcr.microsoft.com/windows/servercore:ltsc2022` and emits the same
artifacts as the native build to the `out/` directory.

## Installation / registration

After building, register the TestServer with DCOM so the managed Opc.Classic
client can activate it. The redistributable installers are no longer vendored;
use the local no-MSI registration path below, or install the official OPC
Foundation Core Components package externally when validating a machine-wide
deployment.

### `external\tools\register-testserver.ps1` (local x64, no MSI)

```powershell
# From an ELEVATED 64-bit PowerShell window:
.\external\tools\register-testserver.ps1
```

The script performs the no-MSI setup needed for x64 DCOM activation:

1. Copies `opccomn_ps.dll` and `opcproxy.dll` from the build output to
   `%SystemRoot%\System32`.
2. Registers those System32 copies with the native `regsvr32.exe`.
3. Runs `OpcTestServer_x64.exe /regserver` with `%SystemRoot%\System32`
   as the working directory.
4. Writes compatibility HKLM entries (CLSID + LocalServer32 + ProgIDs +
   AppID + Implemented Categories for DA 1.0, DA 2.0, and DA 3.0).

Defaults to looking for the EXE and sibling proxy/stub DLLs under
`external\redist\build\x64\Release\`; pass `-ExePath` to override.

To remove the TestServer entries and the System32 proxy/stub DLLs
copied by this script: `.\external\tools\register-testserver.ps1 -Unregister`.
Missing files are skipped; mismatched files without this script's install
marker are left in place to avoid removing an unrelated external install.

Validate from a fresh elevated 64-bit PowerShell, then probe from a
non-elevated shell:

```powershell
# Elevated
.\external\tools\build-testserver.ps1
.\external\tools\register-testserver.ps1
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
that the elevated registration actually completed and that both DLLs are
present in `%SystemRoot%\System32`. If an elevation prompt was canceled
(or the script was run from non-elevated PowerShell), the HKLM entries may
exist while the required System32 proxy/stub DLLs are still missing.

### DCOM ACL

`OpcTestServer.config.xml` ships with `AllowEveryoneAccess="true"`,
so launch/access permissions are permissive by default. If your
machine's DCOM defaults are more restrictive, grant the
`Distributed COM Users` group launch + access on the TestServer
AppID via `dcomcnfg.exe`.

### Known residual blocker: `CO_E_SERVER_EXEC_FAILURE` after no-MSI registration

Even when `external/tools/register-testserver.ps1` completes successfully — proxy/stub
DLLs are copied into `%SystemRoot%\System32`, `regsvr32` reports no error,
`OpcTestServer_x64.exe /regserver` exits 0, and the HKLM CLSID/AppID/Implemented
Categories entries exist — DCOM SCM can still time out with
`CO_E_SERVER_EXEC_FAILURE (0x80080005)` plus event log entry **DistributedCOM
10010** when the managed `opcclassic.da.connect` activates the local CLSID.

This is the OPC Foundation's canonical "registered but the SCM cannot launch
it" failure mode. It happens because the SCM runs as `SYSTEM`, launches the
EXE as the calling user's interactive desktop process (per the AppID's
RunAs/InteractiveUser semantics), but the launched EXE has 60 seconds to
call `CoRegisterClassObject` for the CLSID before the SCM gives up. If the
EXE crashes or returns before registering (because of a missing proxy/stub
DLL, a hung COM initialization, or a service-side ACL that blocks the
process from registering its class object), the timeout fires.

Investigation paths (in increasing complexity):

1. **Watch the EXE launch interactively.** Run `OpcTestServer_x64.exe` (no
   args, no `/regserver`) from an elevated shell to see if it opens its
   own console / window without crashing. If it crashes immediately, the
   stub-resolution path is broken — re-verify the System32 proxy/stub
   copies match the EXE build flavor and bitness.

2. **Add the calling identity to the AppID's RunAs.** Use `dcomcnfg.exe`
   → Component Services → My Computer → DCOM Config → find the TestServer
   AppID `{F8582CF9-88FB-11DA-A5ED-0060B0692061}` → Identity tab → set
   "The interactive user". If the AppID is set to "The launching user"
   (default) and the launching user is the calling SSO identity, this
   should already work — but explicit "interactive user" can resolve
   service-context launch failures.

3. **Compare against an official OPC Foundation Core Components install.** The
   redistributable installer is no longer vendored here, but an external
   official install can confirm AppID Launch/Activation/Access ACLs, RunAs
   identity, and canonical proxy/stub registration independently of this
   no-MSI path.

4. **Capture the EXE's stderr at SCM launch.** Use
   `procmon.exe` filtered to `Process Name = OpcTestServer_x64.exe` to
   see exactly which file/registry access fails during startup.

Once one of these resolves the activation, all of the AddItems / SyncIO /
WriteSync / Subscribe flows that work against Matrikon should also work
against TestServer because the wire format is identical (both are MIDL-
generated DCOM endpoints implementing the same OPC IDL).

## Running against the managed Opc.Classic stack

Once the TestServer is registered, the same `mcp/mcp_driver.py`
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
