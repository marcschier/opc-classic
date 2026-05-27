# Opc.Classic CTT Sample Server

Minimal in-process managed DA server used by the OPC Compliance 
Test Tool workflow.

## Run

```bash
dotnet run --project samples/Opc.Classic.Samples.CttServer
```

The server registers as `Opc.Classic.DaSample.1` (CLSID
`8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A`) and listens on
`0.0.0.0:51303` by default. The CttDaServer returns minimal responses for the IOpcDaServer surface:

Release note: with no environment variables set, the server listens on all interfaces instead of loopback-only ephemeral binding. Set `OPC_CLASSIC_SAMPLE_PORT` to change the default port or `OPC_CLASSIC_LISTEN_ADDRESS` to override the full bind address.


- GetStatus -> Running, vendor "Opc.Classic .NET CTT Sample"
- AddGroup -> echoes client handle + 1000 as server handle
- RemoveGroup -> no-op success
- GetErrorString -> formatted HRESULT string

## Windows COM registration

The server EXE supports a Windows-only COM registration CLI for use with
OPC clients that activate servers via standard `CoCreateInstance` / SCM:

```pwsh
# Publish a Release build first
dotnet publish samples/Opc.Classic.Samples.CttServer -c Release

# Register HKLM (system-wide; requires elevation)
samples/Opc.Classic.Samples.CttServer/bin/Release/net10.0/publish/Opc.Classic.Samples.CttServer.exe --register

# Register HKCU (per-user; no admin needed)
.\Opc.Classic.Samples.CttServer.exe --register --registry-hive=hkcu

# Restrict to a single registry view
.\Opc.Classic.Samples.CttServer.exe --register --registry-view=64

# Remove the registration
.\Opc.Classic.Samples.CttServer.exe --unregister --registry-hive=hkcu
```

Default registration:

- `--registry-hive=hklm` — system-wide under `HKLM\Software\Classes` (admin required)
- `--registry-view=both` — writes to both `Registry32` and `Registry64` views so
  that 32-bit OPC clients (including OPC CTT v2.0.15) can discover the server
  on a 64-bit OS

Registration writes the standard out-of-process COM keys:

- `CLSID\{guid}` (default = friendly name; `AppID` named value = `{guid}`)
- `CLSID\{guid}\LocalServer32` (default = quoted exe path)
- `CLSID\{guid}\ProgID`, `CLSID\{guid}\VersionIndependentProgID`
- `CLSID\{guid}\Implemented Categories\{CATID}` — DA 2.0 + DA 3.0
- `AppID\{guid}` (default = friendly name)
- `Opc.Classic.DaSample.1\CLSID` (default = `{guid}`)
- `Opc.Classic.DaSample\CLSID`, `Opc.Classic.DaSample\CurVer`
- `Component Categories\{CATID}\409` — LCID 409 (en-US) description

Unregister removes the per-server tree but leaves the shared
`Component Categories` description subtree intact.

## CI integration

`.github/workflows/opc-ctt.yml` invokes this sample via the
`dotnet run` step before the CTT runs. The CTT then connects
through the registered ProgID and exercises the IOPC* surface.

## COM SCM activation (-Embedding)

When launched by Windows COM SCM via the `LocalServer32` registration, the EXE
is invoked with the `-Embedding` (or `/Embedding`) flag. In that path the EXE:

1. Calls `CoInitializeEx(STA)` on the main thread
2. Registers an `IClassFactory` class object with `CoRegisterClassObject`
3. Calls `CoResumeClassObjects` to allow SCM to dispatch activations
4. Runs the hosted service
5. Revokes the class object on shutdown via `CoRevokeClassObject` + `CoUninitialize`

> **Scope note**: the registered `IClassFactory` is currently an `IUnknown`-only
> stub. `CreateInstance` returns `E_NOINTERFACE` for any IID besides `IUnknown`.
> This is enough to satisfy COM SCM that the EXE registered a class object (so
> CTT and other clients can discover and launch the server), but the actual
> `IOPCServer` dispatch via the managed DCOM listener (`OpcDaServerHost`) is a
> separate follow-up. The CTT and OPCEnum will list the server but
> CTT conformance tests will not pass end-to-end until that wire-up lands.

## Status

Scaffold-grade. Real per-method dispatch beyond the 3 methods
above lands in CttDaServer follow-ups as the OpcDaServerDispatcher
(N6) covers more interface methods.
