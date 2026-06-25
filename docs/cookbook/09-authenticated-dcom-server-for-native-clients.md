# Authenticated DCOM server for native OPC clients

## Purpose

Use this guide when a native Windows OPC client must consume a managed Opc.Classic server hosted on Linux or another non-Windows platform. The verified reference path is the simulation activation host consumed by a native-style DCOM client: EPM discovery, NTLMv2 authentication, modern activation, OpcEnum browse, DA group/item operations, and reverse callbacks.

For hands-on Matrikon steps, start with [01-connect-to-matrikon-from-linux.md](01-connect-to-matrikon-from-linux.md). For host composition details, see [02-host-managed-da-server-consumed-by-windows-client.md](02-host-managed-da-server-consumed-by-windows-client.md). For protocol background, see [DCOM activation transports](../architecture/activation-transports.md).

## Credential model

`ConfiguredAuthenticationSource` accepts one configured NTLMv2 credential. From environment, it reads:

| Variable | Required | Meaning |
| --- | --- | --- |
| `OPC_CLASSIC_DCOM_USER` | yes | Username the native client must present. |
| `OPC_CLASSIC_DCOM_PASSWORD` | yes | Password used to validate the NTLMv2 proof. |
| `OPC_CLASSIC_DCOM_DOMAIN` | no | Domain/workgroup string; missing means empty. |

When a `ConfiguredAuthenticationSource` is supplied to `RpcServerConnectionProcessor`, requests are rejected until NTLM authentication is established. After authentication, the established context supplies per-PDU signing/sealing for integrity/privacy levels negotiated by the client.

## Server pieces

A native-client server needs these listeners and dispatchers:

1. Endpoint mapper listener on TCP 135 using `EndpointMapperDispatcher`.
2. Activation/object listener using `RpcServerConnectionProcessor` with `ConfiguredAuthenticationSource`.
3. `RemoteSCMActivatorDispatcher` for `IRemoteSCMActivator::RemoteCreateInstance`.
4. `SimulationActivationServer` or your own activator to register the requested OPC objects in `OpcObjectRegistry`.
5. `IObjectExporterDispatcher` and `IRemUnknown` routing so clients can call `ResolveOxid2` and `RemQueryInterface`.
6. OPC server dispatchers for the interfaces you expose, such as DA `IOPCServer`, `IOPCItemMgt`, `IOPCSyncIO`, and `IConnectionPoint`.

The reference simulation host advertises:

- `Opc.Classic.Simulation.DA.1` (`D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001`)
- `Opc.Classic.Simulation.AE.1` (`D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0002`)
- `Opc.Classic.Simulation.HDA.1` (`D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0003`)

## Operator startup checklist

1. Export `OPC_CLASSIC_DCOM_USER`, `OPC_CLASSIC_DCOM_PASSWORD`, and optionally `OPC_CLASSIC_DCOM_DOMAIN` on the server.
2. Start the host that creates `SimulationActivationHost` with an endpoint mapper address of `0.0.0.0:135` and a stable activation/object listener address.
3. If binding TCP 135 as non-root on Linux, grant the published executable `cap_net_bind_service`:

   ```bash
   sudo setcap cap_net_bind_service=+ep ./Opc.Classic.Samples.SimulationServer
   ```

4. Open TCP 135 and the activation/object listener port from the Windows client to the server.
5. In the Windows OPC client, add the remote host and use the same credential values.
6. Browse OpcEnum, select the server ProgID, then test DA read/write and subscriptions.

## Full activation to subscription path

```text
EPM ept_map
  -> NTLMv2 authenticated bind to activation/object listener
  -> IRemoteSCMActivator::RemoteCreateInstance for OpcEnum or server CLSID
  -> OBJREF_STANDARD + pipidRemUnknown
  -> IObjectExporter::ResolveOxid2
  -> IRemUnknown::RemQueryInterface
  -> DA AddGroup/AddItems/SyncRead/Write
  -> IConnectionPoint::Advise
  -> IOPCDataCallback::OnDataChange back to the Windows client
```

The same activation/OXID/`IRemUnknown` foundation is used for AE and HDA in the full-stack test path.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Remote server list is empty or unavailable | TCP 135 must reach the endpoint mapper listener; verify the process is using `EndpointMapperDispatcher`, not only direct `tcp://` DA/AE/HDA listeners. |
| Authentication is rejected | Confirm the username, password, and domain exactly match the server environment variables. Check that the client is using NTLM and packet integrity/privacy settings accepted by the server. |
| Activation succeeds but method calls fail | Open the activation/object listener port returned by EPM and verify the server address in OXID bindings is routable from Windows. |
| DA reads work but subscriptions do not | Reverse callbacks require the Windows client callback sink to be reachable and accepted by its local DCOM/firewall policy. |
| TCP 135 will not bind on Linux | Run as root for testing or set `cap_net_bind_service` on the published executable. |

Use `ManagedDcomFullStackE2ETests` as the source-level acceptance reference for the complete authenticated native-style sequence.
