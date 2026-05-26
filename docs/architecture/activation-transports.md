# DCOM activation transports

OPC Classic clients activate a server by calling `CoCreateInstance` (or the
in-process equivalent) on a registered CLSID. On the wire this translates to a
DCOM activation RPC against one of two interfaces:

| Interface | UUID | Opnum | Modern? | RPC seq |
|---|---|---|---|---|
| `IActivation` (legacy) | `4d9f4ab8-7d1c-11cf-861e-0020af6e7c57` | 0 `RemoteActivation` | Pre-XP-SP2 | `ncacn_np` (SMB) on `\PIPE\epmapper` OR `ncacn_ip_tcp` on `[135]` |
| `IRemoteSCMActivator` (modern) | `000001A0-0000-0000-C000-000000000046` | 3 `RemoteGetClassObject`, 4 `RemoteCreateInstance` | XP SP2+ / Vista+ / 2008+ | `ncacn_ip_tcp` on `[135]` |

Reference: `External/Docs/Win/[MS-DCOM].md` sections 3.1.2.5.2.3 (legacy) and
3.1.2.5.2.4-5 (modern).

## What this codebase supports today

| Path | Status | Implemented in |
|---|---|---|
| `IRemoteSCMActivator::RemoteGetClassObject` over TCP | ✅ Client + server | `src/Opc.Classic.Dcom/Activation/` + `src/Opc.Classic.Discovery/OpcEnumClient.cs` |
| `IRemoteSCMActivator::RemoteCreateInstance` over TCP | ✅ Client + server | same |
| `IActivation::RemoteActivation` over TCP | ❌ Not implemented | (would be in `src/Opc.Classic.Dcom/Activation/IActivation*.cs`) |
| `IActivation::RemoteActivation` over SMB (ncacn_np) | ❌ Not implemented | requires SMB transport (see `docs/architecture/smb-transport.md`) |
| `IRemoteSCMActivator` over SMB | ❌ Not implemented + not normally used | per [MS-DCOM] the modern activator is registered with `ncacn_ip_tcp` only |

## When to use which

| Scenario | Recommended path |
|---|---|
| Modern Windows server (Vista+, Server 2008+) | `IRemoteSCMActivator` over TCP — what we implement today |
| Windows XP / Server 2003 / older Samba-DCE-RPC bridge | `IActivation` over TCP if available, otherwise `IActivation` over SMB |
| Server with TCP port 135 blocked but SMB (445) open | `IActivation` over SMB (the typical "firewall-friendly" DCOM scenario described in Microsoft's MSDN archives) |
| Linux/macOS client talking to Windows | TCP-based modern path (works today; no platform OS dependency in our code) |
| Linux/macOS client talking to legacy Windows (XP / 2003) with TCP blocked | Requires Phase 1-4 of `docs/architecture/smb-transport.md` |

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

Because the two interfaces share so much structure, a future Phase 4 (`IActivation`
legacy support) can largely reuse the existing `ActivationProperties` / NDR
codec under `src/Opc.Classic.Dcom/Activation/`. The work is mostly an
additional opnum dispatcher and a wrapper that adapts the legacy wire shape
(slightly different field ordering per [MS-DCOM] §8200-8240) to the modern
`ClassFactoryRegistry` + `IClassFactory.CreateInstance` plumbing.

## Decision matrix for future work

| Goal | Required work |
|---|---|
| Talk to a legacy XP/Server-2003 server over TCP | `IActivation` client implementation (~1 day; reuse activation NDR codec) |
| Talk to a legacy server over SMB (firewall scenario) | SMB transport (Phase 1-2 in `smb-transport.md`) + `IActivation` client |
| Accept legacy clients into our managed server | `IActivation` server-side dispatcher (mirror of `RemoteSCMActivatorServer.cs`) |
| Cross-platform WINREG discovery | SMB transport (Phase 1-3 in `smb-transport.md`); WINREG opnums already implemented |

## References

- `External/Docs/Win/[MS-DCOM].md` — DCOM Remote Protocol
- `External/Docs/Win/[MS-RPCE].md` — RPC Protocol Extensions
- `src/Opc.Classic.Dcom/Activation/IRemoteSCMActivator.cs` — modern activator definition
- `src/Opc.Classic.Dcom/Activation/RemoteSCMActivatorServer.cs` — modern activator server
- `src/Opc.Classic.Dcom/Activation/ActivationProperties.cs` — shared activation-property carrier
