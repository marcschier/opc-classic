# Opc.Classic.Hosting Windows COM Registration

The `Opc.Classic.Hosting.Windows` namespace contains Windows-only helpers for registering managed OPC Classic servers with the Windows COM Service Control Manager (SCM).

## WindowsComRegistration

Writes the standard Windows COM registry tree for an out-of-process server:

- `HKCR\CLSID\{clsid}` (FriendlyName as default value)
- `HKCR\CLSID\{clsid}` — **named value** `"AppID"` = `"{clsid}"` *(critical: this is a named value, not a subkey — Windows COM uses the named value to resolve the per-application activation policy)*
- `HKCR\CLSID\{clsid}\LocalServer32` (quoted path to the executable)
- `HKCR\CLSID\{clsid}\ProgID` and `\VersionIndependentProgID`
- `HKCR\CLSID\{clsid}\Implemented Categories\{catid}` (one subkey per CATID)
- `HKCR\AppID\{clsid}`
- ProgID aliases: `HKCR\{progId}\CLSID`, `HKCR\{viProgId}\CLSID`, `HKCR\{viProgId}\CurVer`
- `HKCR\Component Categories\{catid}\409` (LCID 409 = en-US description)

### Choosing a hive

| Hive | Subtree | Privilege | Visible to |
| --- | --- | --- | --- |
| `RegistryHive.LocalMachine` | `HKLM\Software\Classes` | Administrator | All users of the machine, including service principals (production) |
| `RegistryHive.CurrentUser` | `HKCU\Software\Classes` | Standard user | The calling user only (tests and developer workstations) |

`HKCU\Software\Classes` is merged into the per-user view of `HKCR` by Windows. Service-hosted COM activation runs as `LocalSystem` (or another service principal) and will NOT see `HKCU` entries written by an interactive user — production deployments must use `HKLM`.

### Choosing a registry view

| View | Use |
| --- | --- |
| `RegistryView.Registry32` | 32-bit (WoW6432Node) clients on 64-bit Windows |
| `RegistryView.Registry64` | 64-bit clients |

The OPC CTT v2.0.15 ships as 32-bit. To make a single managed publish discoverable by both 32-bit and 64-bit clients on a 64-bit OS, the default behaviour (passing `views: null`) writes to both views.

## ComClassObjectRegistrar

P/Invoke wrappers around `ole32.dll` for registering a managed class object with Windows COM SCM:

- `InitializeApartmentThreaded()` → `CoInitializeEx(STA)`
- `RegisterClassObject(clsid, suspended: true)` → registers a stub `IClassFactory` whose `CreateInstance` returns `E_NOINTERFACE`
- `RegisterClassObject(clsid, createInstanceCallback, suspended: true)` → registers an `IClassFactory` whose `CreateInstance` delegates to a NativeAOT-safe CCW factory callback
- `ResumeClassObjects()` → `CoResumeClassObjects()`
- `RevokeClassObject(cookie)` → `CoRevokeClassObject`
- `Uninitialize()` → `CoUninitialize`

The exposed `IClassFactory` is built from raw `[UnmanagedCallersOnly]` vtable entries (no reflection-based COM). The callback overload receives the requested IID and returns a CCW pointer, or `IntPtr.Zero` to surface `E_NOINTERFACE`.

`Opc.Classic.Samples.CttServer` uses the callback overload for SCM activation: `Program.cs` resolves the managed `IOpcDaServer` from DI and passes `requestedIid => OpcDaServerCcw.Create(serverImpl, requestedIid)`. This lets Windows COM clients receive a real `IOPCServer` CCW for supported IIDs while preserving the parameterless overload for registration smoke tests.

## CCW coverage

The rc.10 Windows hosting tests cover DA SCM activation, all AE array-returning server CCW methods, and HDA Sync/Async Update, Playback, Sync/Async Annotations, annotation insert/read, and async advise callback paths.

## Sample integration

See `samples\Opc.Classic.Samples.CttServer\Program.cs` for the `--register` / `--unregister` / `-Embedding` integration pattern.

## Test isolation

The HKCU hive provides isolation for unit tests:

- `tests\Opc.Classic.Hosting.Tests\Windows\WindowsComRegistrationTests.cs` uses unique per-test CLSIDs and ProgIDs and writes only to `HKCU` so the tests run without administrator privileges and clean up after themselves.
