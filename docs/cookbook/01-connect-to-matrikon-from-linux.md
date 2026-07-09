# Connect Matrikon OPC Explorer to the Linux simulation server

## What this covers

This recipe is for an operator running a native Windows OPC client, such as Matrikon OPC Explorer, against the cross-platform managed simulation server hosted on Linux. The native client uses DCOM discovery and activation; the Linux host validates the client's NTLMv2 credentials with `ConfiguredAuthenticationSource`, advertises the simulation servers through OpcEnum, and serves DA/AE/HDA objects from the managed activation listener.

For the sample-specific Matrikon runbook, use [the SimulationServer README topology 2](../../samples/Opc.Classic.Samples.SimulationServer/README.md#topology-2--simulation-on-linux-matrikon-on-windows-authenticated-dcom). For the wire-level sequence, see [DCOM activation transports](../architecture/activation-transports.md).

## Linux server setup

Choose the credential that the Windows client will present. `ConfiguredAuthenticationSource.FromEnvironment()` reads these names:

```bash
export OPC_CLASSIC_DCOM_USER=opcuser
export OPC_CLASSIC_DCOM_PASSWORD='change-me'
export OPC_CLASSIC_DCOM_DOMAIN=OPC   # optional; omit or leave empty for a workgroup-style credential
```

Start a host that creates `SimulationActivationHost` with:

- `authenticationSource: ConfiguredAuthenticationSource.FromEnvironment()` or an equivalent `new ConfiguredAuthenticationSource(user, password, domain)`;
- `listenAddress` bound to a routable address for the activation/object listener;
- `endpointMapperListenAddress: "0.0.0.0:135"` so native clients can call EPM `ept_map` first.

The sample README shows the runnable simulation-server command line used by the Matrikon runbook:

```bash
dotnet run --project samples/Opc.Classic.Samples.SimulationServer -- --opc-only --listen
```

When validating a native-client deployment, confirm the process has both the endpoint mapper and activation/object listener from `SimulationActivationHost`. If the process only prints separate `DA`, `AE`, and `HDA` `tcp://` transport endpoints, it is in the direct managed-transport mode rather than the native DCOM activation topology.

## Privileged port and firewall

TCP 135 is privileged on Linux. Either run the published server as root, or grant the published executable the bind capability:

```bash
sudo setcap cap_net_bind_service=+ep ./Opc.Classic.Samples.SimulationServer
```

Open TCP 135 from the Windows client to the Linux host and open the activation/object listener port printed by the server. Keep the listener port stable in production so firewall rules and packet captures are repeatable.

## Matrikon OPC Explorer steps

1. In Matrikon OPC Explorer, add the Linux host as a remote OPC host.
2. Enter credentials that exactly match `OPC_CLASSIC_DCOM_USER`, `OPC_CLASSIC_DCOM_PASSWORD`, and, when set, `OPC_CLASSIC_DCOM_DOMAIN`.
3. Browse the remote OPC server list. The managed OpcEnum path advertises:
   - `Opc.Classic.Simulation.DA.1`
   - `Opc.Classic.Simulation.AE.1`
   - `Opc.Classic.Simulation.HDA.1`
4. Select `Opc.Classic.Simulation.DA.1`, connect, add a group, and add items such as `Bucket Brigade.Int4`, `Signals.Sine`, or `Plant.Reactor1.Temperature`.
5. Read a snapshot, write a writable `Bucket Brigade.*` item, then enable subscription updates. The DA path supports reverse `IOPCDataCallback::OnDataChange` callbacks after `IConnectionPoint::Advise`.
6. For AE, create/refresh an event subscription and watch simulated reactor events. For HDA, request raw history such as `Plant.Reactor1.Temperature`.

## Expected DCOM flow

The verified native-style sequence is:

```text
EPM ept_map -> NTLMv2 authenticated bind -> IRemoteSCMActivator::RemoteCreateInstance
-> OBJREF + pipidRemUnknown -> IObjectExporter::ResolveOxid2
-> IRemUnknown::RemQueryInterface -> DA AddGroup/AddItems/SyncRead/Write
-> IConnectionPoint::Advise -> OnDataChange
```

`ManagedDcomFullStackE2ETests` exercises this path with managed primitives, including OpcEnum, DA callbacks, AE events, and HDA raw reads.

## Troubleshooting

- Authentication rejected before activation: verify username, password, and domain match the configured environment variables. A configured authentication source rejects requests until NTLM is established.
- Remote host does not browse: verify TCP 135 reaches the Linux process and that the endpoint mapper listener is actually running.
- Browse succeeds but activation fails: open the activation/object listener port returned by EPM and make sure clients resolve the Linux host to a routable address.
- Subscriptions connect but do not update: verify the Windows client permits reverse DCOM callbacks; the server must call back to the client's advised sink.
