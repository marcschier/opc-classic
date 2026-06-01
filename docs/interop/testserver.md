# OPC Foundation TestServer interop

A minimal OPC DA 2.05a test server (and matching native client) used as a
deterministic interop target for byte-exact wire-format validation. Both
are MIT-licensed and source-traceable — when our managed proxy fails a
call against TestServer we can debug into the MIDL-generated stub to see
exactly which byte rejected it.

## What's in `ext/samples/Test/`

| Path | Origin | Purpose |
|------|--------|---------|
| `TestServer/COpcTestServer.{h,cpp}` | Upstream `Source/Test/TestServer/` | Class derived from `COpcDaServer` (in `ext/samples/Sample Server/Da/Core/`). Exposes IOPCServer / IOPCBrowseServerAddressSpace / IOPCItemProperties on the server object and IOPCItemMgt / IOPCSyncIO / IOPCAsyncIO2 / IOPCGroupStateMgt on the group object. |
| `TestServer/COpcTestGroup.h` | Upstream | Group class derived from `COpcDaGroup`. |
| `TestServer/OpcTestServer.{cpp,idl,rc}` | Upstream | Local-server entry point (`_tWinMain`), per-bitness CLSIDs, MIDL IDL for the empty server interface. |
| `TestServer/OpcTestServer.config.xml` | Upstream | 3-item address space: `Test.Int32` (=42), `Test.Float` (=3.14159), `Test.String` ("OPC Test"), each carrying property 6 (Item Quality) = 100. |
| `TestServer/StdAfx.{h,cpp}` | Upstream | Precompiled-header stub. |
| `TestClient/OpcTestClient.cpp` | Upstream | 179-line console exerciser — enumerates DA 2.0 servers via OpcEnum, calls `GetStatus` on each. |
| `docker/Dockerfile` | Upstream | Windows server-core 2022 base + VS 2022 build tools + CMake. |
| `docker/entrypoint.cmd` | Upstream | Runs `build.ps1` inside the container. |

## CLSIDs

- **x64**: `F8582CF9-88FB-11DA-A5ED-0060B0692061`
- **x86**: `F8582CF4-88FB-11DA-A5ED-0060B0692061`

Both register under the `OPC DA 2.05a Test Server` ProgID prefix.

## Building

The TestServer sources are intended to be built from the upstream
[OPC Classic Core Components](https://github.com/OPCFoundation/OPC-Classic-CoreComponents)
repository's CMake harness, which produces the proxy/stub DLLs and
installer in addition to the TestServer/TestClient binaries.

### Option 1 — native MSVC (recommended for local debugging)

```powershell
git clone https://github.com/OPCFoundation/OPC-Classic-CoreComponents.git
cd OPC-Classic-CoreComponents
.\build.ps1
```

Prerequisites: Visual Studio 2022 (C++ Desktop + ATL + Win11 SDK +
MSVC v14.44+), CMake 3.20+, .NET SDK (for WiX), WiX v4+ as a global
tool (`dotnet tool install --global wix`).

Output: `out/x64/OpcTestServer.exe`, `out/x86/OpcTestServer.exe`, plus
the proxy/stub DLLs and MSIs the TestServer depends on.

### Option 2 — Docker (reproducible)

```powershell
cd OPC-Classic-CoreComponents
.\docker-build.ps1
```

Requires Docker Desktop with **Windows containers** enabled. The
image (~10 GB) builds VS 2022 build tools inside `mcr.microsoft.com/
windows/servercore:ltsc2022` and emits the same artifacts as the
native build to the `out/` directory.

### Installation

After building, install the MSI from `out/x64/`:

```powershell
msiexec /i out\x64\OpcClassicCoreComponents-x64-*.msi /qn
```

This registers OpcEnum, the proxy/stub DLLs, and both TestServer
flavors. The x64 MSI bundles all x86 components — only one installer
is required on 64-bit systems.

### DCOM ACL

`OpcTestServer.config.xml` ships with `AllowEveryoneAccess="true"`,
so launch/access permissions are permissive by default. If your
machine's DCOM defaults are more restrictive, grant the
`Distributed COM Users` group launch + access on the TestServer
AppID via `dcomcnfg.exe`.

## Running against the managed Opc.Classic stack

Once the upstream MSI is installed, the same `mcp/mcp_driver.py`
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
|            | IOPCBrowseServerAddressSpace, IOPCItemProperties) with         |
|            | source-level wire-byte debugging via the MIDL stubs            |
| Matrikon   | DA 3.0 (IOPCBrowse, IOPCItemIO); large item-set fuzzing;       |
|            | broad vendor-quirk coverage                                    |

When TestServer and Matrikon disagree on the wire format for a
particular method, the MIDL-generated format strings in
`ext/inc/opcda_p.c` are the authoritative reference — they're
literally the bytes the OPC proxy/stub DLLs mar