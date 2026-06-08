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
| `IRemoteSCMActivator::RemoteGetClassObject` over TCP | ✅ Client + server | `src\Opc.Classic.Dcom\Activation\` + `src\Opc.Classic.Discovery\OpcEnumClient.cs` |
| `IRemoteSCMActivator::RemoteCreateInstance` over TCP | ✅ Client + server | same |
| `IActivation::RemoteActivation` over TCP | ✅ Client + server | `src\Opc.Classic.Dcom\Activation\ActivationClient.cs` + `IActivationCodec.cs` + `ActivationServer.cs` |
| `IActivation::RemoteActivation` over SMB (ncacn_np) | ✅ Client + server | channel-pluggable `ActivationClient`/`ActivationServer` over `src\Opc.Classic.Dcom\Transport\NcacnNpTransport.cs` |
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
`OpcObjectRegistry` for per-IPID object routing.

## When to use which

| Scenario | Recommended path |
| --- | --- |
| Modern Windows server (Vista+, Server 2008+) | `IRemoteSCMActivator` over TCP — what we implement today |
| Windows XP / Server 2003 / older Samba-DCE-RPC bridge | `IActivation` over TCP if available, otherwise `IActivation` over SMB |
| Server with TCP port 135 blocked but SMB (445) open | `IActivation` over SMB via `ncacn_np` / `NcacnNpTransport` (the typical "firewall-friendly" DCOM scenario described in Microsoft's MSDN archives) |
| Linux/macOS client talking to Windows | TCP-based modern path (works today; no platform OS dependency in our code) |
| Linux/macOS client talking to legacy Windows (XP / 2003) with TCP blocked | Now supported via the `ncacn_np` transport (`src\Opc.Classic.Dcom\Transport\NcacnNpTransport.cs`) + the legacy `IActivation` client/server in `src\Opc.Classic.Dcom\Activation\` |

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
`src\Opc.Classic.Dcom\Activation\`. The legacy entry point is mostly an
additional opnum dispatcher and a wrapper that adapts the legacy wire shape
(slightly different field ordering per [MS-DCOM] §8200-8240) to the modern
`ClassFactoryRegistry` + `IClassFactory.CreateInstance` plumbing.

## Status matrix

| Goal | Status |
| --- | --- |
| Talk to a legacy XP/Server-2003 server over TCP | ✅ Shipped via `ActivationClient` and the shared activation NDR codec. |
| Talk to a legacy server over SMB (firewall scenario) | ✅ Shipped via the SMB2 client, `ncacn_np` transport, and channel-pluggable `ActivationClient`; real-world SMB activation captures remain useful validation inputs. |
| Accept legacy clients into our managed server | ✅ Shipped via `ActivationServer`, which adapts legacy activation to the same class-factory registry as `RemoteSCMActivatorServer`. |
| Cross-platform WINREG discovery | ✅ Shipped via WINREG opnums, `ncacn_np`, fixture replay, and Samba smoke coverage. |

## References

- `MS-DCOM.md` (vendored Microsoft spec) — DCOM Remote Protocol
- `MS-RPCE.md` (vendored Microsoft spec) — RPC Protocol Extensions
- `src\Opc.Classic.Dcom\Activation\IRemoteSCMActivator.cs` — modern activator definition
- `src\Opc.Classic.Dcom\Activation\RemoteSCMActivatorServer.cs` — modern activator server
- `src\Opc.Classic.Dcom\Activation\ActivationProperties.cs` — shared activation-property carrier
- `src\Opc.Classic.Hosting\Windows\ComClassObjectRegistrar.cs` — Windows SCM `IClassFactory` registration
- `src\Opc.Classic.Da\Hosting\Windows\OpcDaServerCcw.cs` — DA root server CCW returned by SCM activation
- `src\Opc.Classic.Dcom\Transport\DcomCallChannelFactory.cs` and `TcpClientTransport.cs` — direct TCP client transport
- `src\Opc.Classic.Dcom\Transport\OpcServerListener.cs`, `RpcServerConnectionProcessor.cs`, and `OpcObjectRegistry.cs` — managed DCOM-over-IP listener path
