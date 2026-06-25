# Host an authenticated managed OPC DA server consumed by a Windows COM client

## What this covers

Run a managed OPC DA server on Linux, macOS, or Windows while native Windows OPC clients connect through Classic DCOM. The authenticated native-client reference is the full-feature simulation activation host: `samples\Opc.Classic.Samples.SimulationServer\Transports\SimulationActivationHost.cs`.

For an operator runbook with Matrikon OPC Explorer, see [Connect Matrikon OPC Explorer to the Linux simulation server](01-connect-to-matrikon-from-linux.md). For a focused native-client deployment checklist, see [Authenticated DCOM server for native OPC clients](09-authenticated-dcom-server-for-native-clients.md).

## Hosting shape

`SimulationActivationHost` composes the pieces a native Windows client expects:

- `EndpointMapperDispatcher` for EPM `ept_map` on TCP 135 when `endpointMapperListenAddress` is set.
- `RemoteSCMActivatorDispatcher` and `ActivationServer` for activation.
- `SimulationActivationServer` for DA, AE, HDA, and OpcEnum class activation.
- `IObjectExporterDispatcher` plus `RemUnknownServerDispatcher` registration through `OpcObjectRegistry` for OXID and `IRemUnknown` routing.
- `RpcServerConnectionProcessor` with an optional `ConfiguredAuthenticationSource` for server-side NTLMv2 and per-PDU integrity/privacy after authentication.

A host for native clients should create the activation host over its shared server model:

```csharp
using Microsoft.Extensions.Logging;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;

var model = new SimulatedPlantModel();
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

ConfiguredAuthenticationSource? auth = ConfiguredAuthenticationSource.FromEnvironment();
if (auth is null)
{
    throw new InvalidOperationException(
        "Set OPC_CLASSIC_DCOM_USER and OPC_CLASSIC_DCOM_PASSWORD before exposing native DCOM.");
}

await using SimulationActivationHost host = SimulationActivationHost.Create(
    model,
    daClsid: new Guid("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001"),
    listenAddress: "0.0.0.0:51300",
    loggerFactory,
    endpointMapperListenAddress: "0.0.0.0:135",
    authenticationSource: auth);

await host.StartAsync();
await Task.Delay(Timeout.InfiniteTimeSpan);
```

`OPC_CLASSIC_DCOM_DOMAIN` is optional; `ConfiguredAuthenticationSource.FromEnvironment()` treats a missing domain as empty. The DA/AE/HDA simulation ProgIDs are `Opc.Classic.Simulation.DA.1`, `Opc.Classic.Simulation.AE.1`, and `Opc.Classic.Simulation.HDA.1`.

## Running the sample topology

The SimulationServer README contains the Matrikon topology and should stay the step-by-step sample runbook:

```bash
export OPC_CLASSIC_DCOM_USER=opcuser
export OPC_CLASSIC_DCOM_PASSWORD='change-me'
export OPC_CLASSIC_DCOM_DOMAIN=OPC

dotnet run --project samples/Opc.Classic.Samples.SimulationServer -- --opc-only --listen
```

Use the process output to verify which transport mode is active. Native Windows DCOM consumption requires the `SimulationActivationHost` endpoint mapper plus activation/object listener. Separate `DA`, `AE`, and `HDA` `tcp://` endpoints are useful for managed direct transports, but they are not by themselves the EPM-driven native activation path.

## Windows client side

The Windows client discovers OpcEnum on the Linux host through TCP 135, authenticates to the activation listener with the configured NTLMv2 credential, activates the selected CLSID with `IRemoteSCMActivator::RemoteCreateInstance`, resolves OXID bindings, queries interfaces through `IRemUnknown`, and then calls DA group/item/read/write interfaces.

For DA subscriptions, the client advises an `IOPCDataCallback` sink through `IConnectionPoint::Advise`; the managed server sends `OnDataChange` back to that sink. AE and HDA activation use the same authenticated activation/OXID path; AE subscriptions deliver event callbacks and HDA supports raw history reads in the verified full-stack path.

## Network requirements

Open TCP 135 and the activation/object listener port from the Windows client to the managed server. On Linux, binding TCP 135 requires root or a capability grant on the published executable:

```bash
sudo setcap cap_net_bind_service=+ep ./Opc.Classic.Samples.SimulationServer
```

Keep the ProgID and CLSID stable. Legacy clients often store `Opc.Classic.Simulation.DA.1` or a production ProgID in project files, while the wire activation uses the CLSID.

## Validation aids

- `ManagedDcomFullStackE2ETests` verifies the EPM, authenticated activation, OpcEnum, DA group/read/write/callback, AE event, and HDA raw-read path.
- `SimulationActivationHost` is the reusable authenticated native-client host composition.
- [DCOM activation transports](../architecture/activation-transports.md) documents the expected activation and OXID sequence.
