# OPC Foundation TestServer interop

A minimal OPC DA 2.05a test server (and matching native client) used as a
deterministic interop target for byte-exact wire-format validation. Both
are MIT-licensed and source-traceable — when our managed proxy fails a
call against TestServer we can debug into the MIDL-generated stub to see
exactly which byte rejected it.

## What's in `ext/CoreComponents/`

The entire OPC Foundation
[OPC-Classic-CoreComponents](https://github.com/OPCFoundation/OPC-Classic-CoreComponents)
repository is vendored verbatim into `ext/CoreComponents/` so the
native TestServer + proxy/stub DLLs can be built without an external
clone. Key paths:

| Path | Purpose |
|------|---------|
| `Source/Test/TestServer/COpcTestServer.{h,cpp}` | Server class derived from `COpcDaServer` (in `Source/Shared/SampleServerClasses/`). Exposes `IOPCServer` / `IOPCBrowseServerAddressSpace` / `IOPCItemProperties` on the server object and `IOPCItemMgt` / `IOPCSyncIO` / `IOPCAsyncIO2` / `IOPCGroupStateMgt` on the group object. |
| `Source/Test/TestServer/COpcTestGroup.h` | Group class derived from `COpcDaGroup`. |
| `Source/Test/TestServer/OpcTestServer.{cpp,idl,rc}` | Local-server entry point (`_tWinMain`), per-bitness CLSIDs, MIDL IDL for the empty server interface. |
| `Source/Test/TestServer/OpcTestServer.config.xml` | 3-item address space: `Test.Int32` (=42), `Test.Float` (=3.14159), `Test.String` ("OPC Test"), each carrying property 6 (Item Quality) = 100. |
| `Source/Test/TestClient/OpcTestClient.cpp` | 179-line console exerciser — enumerates DA 2.0 servers via OpcEnum, calls `GetStatus` on each. |
| `Source/Shared/` | Sample server scaffolding TestServer derives from (`OpcUtilityClasses`, `SampleServerClasses`, `SampleDevice`, `SampleServer205`). |
| `Source/Common/`, `Source/DataAccess/`, `Source/Security/` | Per-spec IDLs + proxy/stub builds for `opccomn_ps.dll`, `opcproxy.dll`, `opcsec_ps.dll` (the DLLs the TestServer's COM activation needs for marshalling). |
| `Source/Include/` | Shared headers (CATID GUIDs, error codes). |
| `CMakeLists.txt`, `cmake/` | Upstream CMake harness — invoked by `tools\build-testserver.ps1`. |
| `docker/` | Windows Server Core 2022 + VS 2022 build-tools docker harness for reproducible builds without a local VS install. |
| `build.ps1`, `docker-build.ps1` | Upstream one-shot build entry points (native + docker). |
| `WiX/` | MSI installer scaffolding (optional). |

See `ext/CoreComponents/VENDORED.md` for the snapshot provenance and
re-sync workflow. The OPC Foundation MIT License 1.00 grant lives in
the file headers; `LICENSE.md` at the vendor root is the umbrella
OPC Foundation license that governs the broader specification suite.

## CLSIDs

- **x64**: `F8582CF9-88FB-11DA-A5ED-0060B0692061`
- **x86**: `F8582CF4-88FB-11DA-A5ED-0060B0692061`

Both register under the `OPC DA 2.05a Test Server` ProgID prefix.

## Building

### Option 1 — `tools\build-testserver.ps1` (recommended)

```powershell
.\tools\build-testserver.ps1            # Release x64
.\tools\build-testserver.ps1 -Clean     # wipe build\ first
```

The script discovers VS's bundled CMake (or any cmake.exe on PATH),
configures `ext\CoreComponents\build\x64`, and builds the
`OpcTestServer`, `OpcTestClient`, `OpcCategoryManager`, `opccomn_ps`
and `opcproxy` targets. Output lands in
`ext\CoreComponents\build\x64\Release\`.

Prerequisites: Visual Studio 2022 17.14+ (Desktop development with
C++ + ATL + Win11 SDK + MSVC v14.44 or later), CMake 3.20+ (bundled
with VS).

### Option 2 — upstream `build.ps1` directly

```powershell
cd ext\CoreComponents
.\build.ps1
```

Builds **all** targets including the WiX MSI installers (requires
`dotnet tool install --global wix`). Slower but produces a
distributable MSI.

### Option 3 — Docker (fully reproducible, no local VS)

```powershell
cd ext\CoreComponents
.\docker-build.ps1
```

Requires Docker Desktop with **Windows containers** enabled. The
image (~10 GB) builds VS 2022 build tools inside
`mcr.microsoft.com/windows/servercore:ltsc2022` and emits the same
artifacts as the native build to the `out/` directory.

## Installation / registration

After building, you have two options to register the TestServer
with DCOM so the managed Opc.Classic client can activate it:

### Option A — full MSI install (canonical Core Components install)

`ext\CoreComponents\build.ps1` (or option 3 above) produces an MSI
under `out\x64\`. Install it:

```powershell
msiexec /i ext\CoreComponents\out\x64\OpcClassicCoreComponents-x64-*.msi /qn
```

This registers OpcEnum, the proxy/stub DLLs, the x64 category manager,
and both TestServer flavors. Use this path when validating OpcEnum,
discovery, or mixed x86/x64 installs.

### Option B — `tools\register-testserver.ps1` (local x64, no MSI)

```powershell
# From an ELEVATED 64-bit PowerShell window:
.\tools\register-testserver.ps1
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
`ext\CoreComponents\build\x64\Release\`; pass `-ExePath` to override.

To remove the TestServer entries and the System32 proxy/stub DLLs
copied by this script: `.\tools\register-testserver.ps1 -Unregister`.
Missing files are skipped; mismatched files without this script's install
marker are left in place to avoid removing an unrelated MSI install.

Validate from a fresh elevated 64-bit PowerShell, then probe from a
non-elevated shell:

```powershell
# Elevated
.\tools\build-testserver.ps1
.\tools\register-testserver.ps1
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
`ext/CoreComponents/Source/DataAccess/ProxyStub/` are the authoritative
reference — they're literally the bytes the OPC proxy/stub DLLs
marshal.
