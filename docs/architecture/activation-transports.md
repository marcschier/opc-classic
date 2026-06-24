# DCOM activation transports

OPC Classic clients activate a server by calling `CoCreateInstance` (or the
in-process equivalent) on a registered CLSID. On the wire this translates to a
DCOM activation RPC against one of two interfaces:

| Interface | UUID | Opnum | Modern? | RPC seq |
| --- | --- | --- | --- | --- |
| `IActivation` (legacy) | `4d9f4ab8-7d1c-11cf-861e-0020af6e7c57` | 0 `RemoteActivation` | Pre-XP-SP2 | `ncacn_np` (SMB) on `\PIPE\epmapper` OR `ncacn_ip_tcp` on `[135]` |
| `IRemoteSCMActivator` (modern) | `000001A0-0000-0000-C000-000000000046` | 3 `RemoteGetClassObject`, 4 `RemoteCreateInstance` | XP SP2+ / Vista+ / 2008+ | `ncacn_ip_tcp` on `[135]` |

Reference: the vendored `MS-DCOM.md` spec sections 3.1.2.5.2.3 (legacy) and
3.1.2.5.2.4-5 (modern).

## What this codebase supports today

| Path | Status | Implemented in |
| --- | --- | --- |
| `IRemoteSCMActivator::RemoteGetClassObject` over TCP | ✅ Client + server | source-generated `IRemoteSCMActivator` + `RemoteSCMActivatorServer` |
| `IRemoteSCMActivator::RemoteCreateInstance` over TCP | ✅ Client + server | source-generated `IRemoteSCMActivator` + `RemoteSCMActivatorServer` |
| `IActivation::RemoteActivation` over TCP | ✅ Client + server | `ActivationClient` + `IActivationCodec.cs` + `ActivationServer` / `LegacyActivationServer` |
| `IActivation::RemoteActivation` over SMB (ncacn_np) | ✅ Client + server | channel-pluggable `ActivationClient` / `ActivationServer` over `NcacnNpTransport` |
| Simulation DA cold-activation over managed TCP | ⚠️ Handler + listener test path | `SimulationActivationHost` hosts `ActivationServer` + `SimulationActivationServer`, registers DA dispatchers in `OpcObjectRegistry`, and can optionally host the MS-RPCE Endpoint Mapper (`ept` on TCP 135) via `EndpointMapperDispatcher`; full authenticated cold-activation waits on server-side NTLM bind handling in `RpcServerConnectionProcessor` |
| `IRemoteSCMActivator` over SMB | ❌ Not implemented + not normally used | per [MS-DCOM] the modern activator is registered with `ncacn_ip_tcp` only |

The Windows SCM path is also wired for local/native client activation.
`ComClassObjectRegistrar` registers an AOT-friendly `IClassFactory`; DA, AE,
and HDA hosting expose explicit CCWs such as `OpcDaServerCcw`,
`OpcDaGroupCcw`, `OpcAeServerCcw`, `OpcHdaServerCcw`,
`OpcDataCallbackProxy`, and `OpcEnumOpcItemAttributesCcw`. This path avoids
`[ComImport]` and the Windows COM runtime marshaler while still satisfying
`CoCreateInstance` / SCM clients.

For container and cross-platform samples that already know a host and port,
clients can skip endpoint-mapper activation and dial the managed listener
directly. `DcomCallChannelFactory.ConnectTcpAsync` opens a public
`TcpClientTransport`; the server side accepts with `OpcServerListener`,
processes PDUs through `RpcServerConnectionProcessor`, and uses
`OpcObjectRegistry` for per-IPID object routing. The full-feature
`Opc.Classic.Samples.SimulationServer --listen` uses this for DA/AE/HDA real
transport hosting. Its separate `SimulationActivationHost` exercises the
modern cold-activation shape for DA by serving `IActivation::RemoteActivation`
and the activated object on one listener.

`SimulationActivationHost.Create(..., endpointMapperListenAddress: "0.0.0.0:135")`
starts a managed Endpoint Mapper beside the activation/object listener. Its
`ept_map` responder maps `IRemoteSCMActivator` / `IObjectExporter` towers to the
actual bound activation endpoint. On Linux, binding TCP 135 requires root or a
capability grant such as `setcap cap_net_bind_service=+ep <published-binary>`;
tests and unprivileged development can pass a non-privileged override port.

## When to use which

| Scenario | Recommended path |
| --- | --- |
| Modern Windows server (Vista+, Server 2008+) | `IRemoteSCMActivator` over TCP — what we implement today |
| Windows XP / Server 2003 / older Samba-DCE-RPC bridge | `IActivation` over TCP if available, otherwise `IActivation` over SMB |
| Server with TCP port 135 blocked but SMB (445) open | `IActivation` over SMB via `ncacn_np` / `NcacnNpTransport` (the typical "firewall-friendly" DCOM scenario described in Microsoft's MSDN archives) |
| Linux/macOS client talking to Windows | TCP-based modern path (works for direct known endpoints; native SCM/EPM flows still depend on the target server and network policy) |
| Linux/macOS client talking to legacy Windows (XP / 2003) with TCP blocked | Now supported via the `ncacn_np` transport + the legacy `IActivation` client/server in `Activation` |

## Activation property surface

Both `IActivation::RemoteActivation` and `IRemoteSCMActivator::RemoteGetClassObject`
share the same activation-property carrier (per `[MS-DCOM] §2.2.22` /
`§2.2.18` for v5.6). The shared fields:

- `ORPCTHIS` / `ORPCTHAT` — call context envelope
- `CLSID` — class to activate
- `pwszObjectName` (Mode-dependent, mostly NULL for OPC Classic)
- `pObjectStorage` (NULL for OPC Classic)
- `ClientImpLevel` (`RPC_C_IMP_LEVEL_IDENTIFY` / `_IMPERSONATE` / `_DELEGATE`)
- `Mode` (`MODE_GET_CLASS_OBJECT` etc.)
- `Interfaces[]` array of requested IIDs (IUnknown + IOPCServer + etc.)
- `cRequestedProtseqs` + `aRequestedProtseqs[]` — protocol-sequence preferences for the activated object

The response carries `OXID`, `DUALSTRINGARRAY` (object bindings), `IPID`,
`pAuthnHint`, `COMVERSION`, and the per-IID activation results
(`MInterfacePointer[]` + HRESULT[]).

Because the two interfaces share so much structure, the legacy `IActivation`
support reuses the existing `ActivationProperties` / NDR codec under
`Activation`. The legacy entry point is mostly an
additional opnum dispatcher and a wrapper that adapts the legacy wire shape
(slightly different field ordering per [MS-DCOM] §8200-8240) to the modern
`ClassFactoryRegistry` + `IClassFactory.CreateInstance` plumbing.

## Status matrix

| Goal | Status |
| --- | --- |
| Talk to a legacy XP/Server-2003 server over TCP | ✅ Shipped via `ActivationClient` and the shared activation NDR codec. |
| Talk to a legacy server over SMB (firewall scenario) | ✅ Shipped via the SMB2 client, `ncacn_np` transport, and channel-pluggable `ActivationClient`; real-world SMB activation captures remain useful validation inputs. |
| Accept legacy clients into our managed server | ✅ Shipped via `ActivationServer`, which adapts legacy activation to `LegacyActivationServer` / `RemoteSCMActivatorServer` when backed by class factories. |
| Simulation DA cold-activation returns routable IPID | ⚠️ Handler shipped and covered by MCP integration tests: `SimulationActivationServer` returns a spec-conformant `OBJREF_STANDARD` and registers the activated DA object in `OpcObjectRegistry`; `EndpointMapperDispatcher` answers `ept_map` for activation and OXID resolver endpoints; anonymous network activation is denied, and full authenticated cold-activation still needs server-side NTLM bind handling on the managed listener. |
| Cross-platform WINREG discovery | ✅ Shipped via WINREG opnums, `ncacn_np`, fixture replay, and Samba smoke coverage. |

## References

- `MS-DCOM.md` (vendored Microsoft spec) — DCOM Remote Protocol
- `MS-RPCE.md` (vendored Microsoft spec) — RPC Protocol Extensions
- `IRemoteSCMActivator` — modern activator definition
- `RemoteSCMActivatorServer` — modern activator server
- `ActivationServer` / `LegacyActivationServer` — legacy `IActivation` dispatcher and adapter
- `EndpointMapperDispatcher` / `EndpointMapperTower` — managed MS-RPCE `ept_map` responder and TCP tower codec
- `SimulationActivationServer` / `SimulationActivationHost` — simulation DA cold-activation host and handler
- `ActivationProperties` — shared activation-property carrier
- `ComClassObjectRegistrar` — Windows SCM `IClassFactory` registration
- `OpcDaServerCcw` — DA root server CCW returned by SCM activation
- `DcomCallChannelFactory` and `TcpClientTransport.cs` — direct TCP client transport
- `OpcServerListener`, `RpcServerConnectionProcessor.cs`, and `OpcObjectRegistry.cs` — managed DCOM-over-IP listener path
