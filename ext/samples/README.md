# OPC Foundation Sample Servers (Native C++ Reference)

This folder contains the OPC Foundation's official C++ sample server sources for DA, AE, and HDA, preserved as the GOLD STANDARD reference for conformance testing.

## Structure

- `Sample Server/Da/` — DA sample server projects:
  - `Server/` — DA 3.0 Simple Server (`OPCSample.OpcDaServer.1`)
  - `Server (2.05a Only)/` — DA 2.05a source server
  - `Server (3.00 Only)/` — DA 3.00 source server
  - `Core/`, `Device/`, `Wrapper/` — shared DA server implementation and wrapper projects
- `Sample Server/Ae/` — AE Simple Server (`OPCSample.OPCEventServer.1`)
- `Sample Server/Hda/` — HDA Simple Server (`OPCSample.OpcHdaServer.1`)
- `Sample Client/Da/Simple Client/` — DA test client for Windows COM client → net10 server compatibility
- `Shared/Utils/` — common C++ helpers used by the native sample projects

No legacy `.sln` files are currently preserved under `ext/samples/`; the native
servers are built by invoking the actual `.vcxproj` files directly.

## Build

Requires:

- Windows + Visual Studio 2022 Build Tools (or full VS 2022)
- Windows SDK 10
- OPC Foundation Core Components available on the host (provides
  `opcproxy.dll` for marshalling and OpcEnum). The redistributable installers
  are no longer vendored; build/register the CoreComponents tree under
  `ext\redist\CoreComponents` or install the official OPC Foundation package
  externally.
- C++ desktop workload with ATL/MFC components

Build steps (the projects include OPC headers from `ext\inc`):

```cmd
:: From Developer Command Prompt for VS 2022:
set SOLUTIONDIR=%CD%\ext\samples\
msbuild "ext\samples\Sample Server\Da\Server\OpcDaServer.vcxproj" /p:Configuration=Release /p:Platform=Win32 /p:SolutionDir="%SOLUTIONDIR%" /p:PlatformToolset=v143 /p:WindowsTargetPlatformVersion=10.0 /m
msbuild "ext\samples\Sample Server\Ae\OpcAeServer.vcxproj" /p:Configuration=Release /p:Platform=Win32 /p:SolutionDir="%SOLUTIONDIR%" /p:PlatformToolset=v143 /p:WindowsTargetPlatformVersion=10.0 /m
msbuild "ext\samples\Sample Server\Hda\Server\OpcHdaServer.vcxproj" /p:Configuration=Release /p:Platform=Win32 /p:SolutionDir="%SOLUTIONDIR%" /p:PlatformToolset=v143 /p:WindowsTargetPlatformVersion=10.0 /m
```

The projects emit EXEs under:

```text
ext\samples\BuildOutput\bin\servers\Win32\Release\
```

Register the built EXEs:

```cmd
ext\samples\regserver.cmd
```

or manually:

```cmd
ext\samples\BuildOutput\bin\servers\Win32\Release\OpcDaServer.exe /RegServer
ext\samples\BuildOutput\bin\servers\Win32\Release\OpcAeServer.exe /RegServer
ext\samples\BuildOutput\bin\servers\Win32\Release\OpcHdaServer.exe /RegServer
```

## CLSID/ProgID inventory

After build + registration, the following CLSIDs are registered:

| ProgID | CLSID | Friendly name | Sample |
| --- | --- | --- | --- |
| `OPCSample.OpcDaServer.1` | `{625C49A1-BE1C-45D7-9A8A-14BEDCF5CE6C}` | OPC Data Access 3.0 Sample Server | DA Sample |
| `OPCSample.OPCEventServer.1` | `{65168852-5783-11D1-84A0-00608CB8A7E9}` | OPC Event Server Sample | AE Sample |
| `OPCSample.OpcHdaServer.1` | `{6A5EEDEC-1509-4627-997F-993CCB65AB7C}` | OPC Historical Data Access 1.20 Sample Server | HDA Sample |

DA and HDA derive their ProgIDs in `ext/samples/Shared/Utils/COpcClassFactory.cpp`
from `OPC_DECLARE_APPLICATION(OPCSample, ...)` plus
`OPC_CLASS_TABLE_ENTRY(..., 1, ...)`. Their CLSIDs come from the coclass
`uuid(...)` declarations in `OpcDaServer.idl` and `OpcHdaServer.idl`.
AE registers `OPCSample.OPCEventServer.1` and
`{65168852-5783-11D1-84A0-00608CB8A7E9}` explicitly in
`Sample Server/Ae/OPCEventServer.rgs` and `OpcAeServer.cpp`.

## Test connection

`tests/Opc.Classic.Integration.Tests/Native/` uses these ProgIDs and CLSIDs via `NativeServerProbe.ShouldSkip(progId, clsid, out reason)`. Tests soft-skip
on non-Windows, when the servers aren't registered, or when the registered ProgID maps to an unexpected CLSID.
