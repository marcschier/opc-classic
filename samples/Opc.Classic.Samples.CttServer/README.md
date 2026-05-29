# Opc.Classic CTT Sample Server

Managed DA server used by the OPC Compliance Test Tool workflow.

## Run

```powershell
dotnet run --project samples\Opc.Classic.Samples.CttServer
```

The server registers as `Opc.Classic.DaSample.1` (CLSID `8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A`) and listens on `0.0.0.0:51303` by default.

Set `OPC_CLASSIC_SAMPLE_PORT` to change the default port or `OPC_CLASSIC_LISTEN_ADDRESS` to override the full bind address.

The `CttDaServer` surface includes:

- `GetStatus` -> Running, vendor "Opc.Classic .NET CTT Sample"
- `AddGroup` -> creates a tracked `OpcDaGroup`, returns a server handle, and registers group dispatchers/IPIDs for follow-up calls
- `RemoveGroup` -> unregisters tracked group state
- `GetGroupByName` -> resolves a tracked group by name
- `GetErrorString` -> formatted HRESULT string
- group-level `IOPCGroupStateMgt(2)`, `IOPCItemMgt`, `IOPCSyncIO(2)`, `IOPCAsyncIO2/3`, connection-point, deadband, and sampling dispatch through `OpcDaGroup`

## Windows COM registration

The server EXE supports a Windows-only COM registration CLI for use with OPC clients that activate servers via standard `CoCreateInstance` / SCM:

```powershell
# Publish a Release build first
dotnet publish samples\Opc.Classic.Samples.CttServer -c Release

# Register HKLM (system-wide; requires elevation)
samples\Opc.Classic.Samples.CttServer\bin\Release\net10.0\publish\Opc.Classic.Samples.CttServer.exe --register

# Register HKCU (per-user; no admin needed)
.\Opc.Classic.Samples.CttServer.exe --register --registry-hive=hkcu

# Restrict to a single registry view
.\Opc.Classic.Samples.CttServer.exe --register --registry-view=64

# Remove the registration
.\Opc.Classic.Samples.CttServer.exe --unregister --registry-hive=hkcu
```

Default registration:

- `--registry-hive=hklm` — system-wide under `HKLM\Software\Classes` (admin required)
- `--registry-view=both` — writes to both `Registry32` and `Registry64` views so that 32-bit OPC clients (including OPC CTT v2.0.15) can discover the server on a 64-bit OS

Registration writes the standard out-of-process COM keys:

- `CLSID\{guid}` (default = friendly name; `AppID` named value = `{guid}`)
- `CLSID\{guid}\LocalServer32` (default = quoted exe path)
- `CLSID\{guid}\ProgID`, `CLSID\{guid}\VersionIndependentProgID`
- `CLSID\{guid}\Implemented Categories\{CATID}` — DA 2.0 + DA 3.0
- `AppID\{guid}` (default = friendly name)
- `Opc.Classic.DaSample.1\CLSID` (default = `{guid}`)
- `Opc.Classic.DaSample\CLSID`, `Opc.Classic.DaSample\CurVer`
- `Component Categories\{CATID}\409` — LCID 409 (en-US) description

Unregister removes the per-server tree but leaves the shared `Component Categories` description subtree intact.

## CI and Docker integration

`.github\workflows\opc-ctt.yml` invokes this sample via `dotnet run` before the CTT runs. The Windows-container test fleet publishes this project into `opc-classic/managed`; see `samples\README.docker.md` and `docker\README.md` for the compose workflow.

## COM SCM activation (-Embedding)

When launched by Windows COM SCM via the `LocalServer32` registration, the EXE is invoked with the `-Embedding` (or `/Embedding`) flag. In that path the EXE:

1. Calls `CoInitializeEx(STA)` on the main thread
2. Registers an `IClassFactory` class object with `CoRegisterClassObject`
3. Uses `ComClassObjectRegistrar.RegisterClassObject(..., createInstanceCallback: requestedIid => OpcDaServerCcw.Create(serverImpl, requestedIid))` so `CreateInstance` can return a NativeAOT-friendly `IOPCServer` CCW
4. Calls `CoResumeClassObjects` to allow SCM to dispatch activations
5. Runs the hosted service
6. Revokes the class object on shutdown via `CoRevokeClassObject` + `CoUninitialize`

The Windows CCW path now returns real `IOPCServer` pointers for supported IIDs and routes `AddGroup`, `GetStatus`, `GetErrorString`, `GetGroupByName`, and `RemoveGroup` into the managed server. `CreateGroupEnumerator` and broader CTT coverage remain incremental follow-ups.

## Source files

- `Program.cs` — listen-address selection, COM registration CLI, and SCM `-Embedding` integration.
- `CttDaServer.cs` — managed DA server and group registry used by the CTT workflow.
