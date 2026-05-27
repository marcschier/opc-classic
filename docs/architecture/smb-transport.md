# SMB transport — current state and cross-platform plan

OPC Classic clients written for Microsoft Windows use two RPC transport
sequences interchangeably depending on the era of the client and the available
server:

| Sequence | Carrier | Endpoint | Used by |
|---|---|---|---|
| `ncacn_ip_tcp` | TCP/IP | Port 135 (endpoint mapper) + dynamic | Modern `IRemoteSCMActivator` (DCOM v5.6+, XP SP2+) |
| `ncacn_np` | SMB | `\PIPE\<name>` | Legacy `IActivation`, all `[MS-RRP]` WINREG (`\PIPE\winreg`), `[MS-EVEN]` event log (`\PIPE\eventlog`), etc. |

This repository implements `ncacn_ip_tcp` end-to-end. The legacy
`ncacn_np` path under `src\Opc.Classic.Dcom\rpc\ncacn_np\RpcTransport.cs`
remains **scaffolded but inert** on every platform:

- It parses `ncacn_np:host[\PIPE\name]` addresses correctly.
- `src\Opc.Classic.Dcom\Common\Ntlm\SmbNamedPipe.cs` is a stub: it wraps two
  `MemoryStream`s and performs no network I/O.
- `src\Opc.Classic.Dcom\Common\Ntlm\SmbSession.cs::Logon` is a no-op.
- The local `SharpCifs.*` namespace types are minimal compile-time shims left
  over from the original JCIFS/SharpCifs.Std port. The protocol layer was
  intentionally removed during the AOT migration (see
  `src\Opc.Classic.Dcom\Common\SharpCifsBoundary.md`).

In parallel, Phase 1 of the chosen replacement now exists in
`src\Opc.Classic.Dcom.Smb\`: `Smb2Connection`, `TcpSmb2Transport`,
`Smb2NamedPipe`, and `Smb2RpcTransportAdapter` implement the focused SMB2
client surface and the sync bridge needed by the legacy RPC transport. That
project is not yet wired into `RpcTransport`, and SMB signing/encryption plus
real-server WINREG smoke coverage remain follow-up work.

Consumers handle the stub gracefully: `src\Opc.Classic.Discovery\RemoteRegistryEnum.cs`
logs `"Remote-registry enumeration failed for host {Host}; returning no OPC
servers. Consider using OpcEnumClient (OPC.ServerList.1) instead."` and returns
an empty list.

## What is NOT supported today

- **WINREG-based discovery** (browsing a remote host's
  `HKLM\SOFTWARE\Classes` to find OPC servers, used by `RemoteRegistryEnum`).
- **Legacy DCOM activation** through `IActivation::RemoteActivation`
  (UUID `4d9f4ab8-7d1c-11cf-861e-0020af6e7c57`) over `ncacn_np`. The codebase
  does not implement the `IActivation` interface even on the TCP path.
- Any other named-pipe RPC service (`[MS-EVEN]`, `[MS-RPRN]`, `[MS-WKST]`,
  `[MS-SAMR]`, etc.). None of these are required by OPC Classic but a wired
  `ncacn_np` transport would make them addressable.

## Microsoft specifications driving the design

The repository vendors the relevant Microsoft Open Specifications in
`External/Docs/Win/`:

| Spec | Relevance |
|---|---|
| `[MS-CIFS].md` | SMB v1 wire format (mostly informational; we target SMB2+) |
| `[MS-SMB2].md` | SMB 2.0.2 / 2.1 / 3.0 / 3.1.1 wire format — the implementation target |
| `[MS-RPCE].md §2.1.1.2` | RPC over SMB framing: same DCE/RPC PDUs sent as named-pipe writes / received as reads, with optional transact for synchronous calls |
| `[MS-DCOM].md §3.1.2.5.2.3` | Legacy `IActivation::RemoteActivation` opnum 0 |
| `[MS-RRP].md §2.1.2` | WINREG client guidance: SHOULD use `ncacn_np` on `\PIPE\winreg` |
| `[MS-NLMP].md` | NTLMSSP — already implemented in `Opc.Classic.Dcom\rpc\Auth\` for the TCP path, can be reused inside the SMB2 SESSION_SETUP security blob |

## Cross-platform implementation options

The hard problem is the SMB protocol itself. The options below were considered:

| Option | License | AOT-clean | Lines of code | SMB versions | Recommendation |
|---|---|---|---|---|---|
| **(A) SMBLibrary** (TalAloni/SMBLibrary on NuGet) | LGPL-3.0 | likely (pure C#) | 0 (consume) | SMB1/2/3 | Subject to license review; consult counsel. If approved, fastest to ship but creates dynamic-link / disclosure obligations atypical for an MIT repo. |
| **(B) Hand-roll SMB2 client** ⭐ recommended | MIT (ours) | ✅ | ~4000-5000 | SMB 2.0.2 + 2.1 + 3.0 / 3.1.1 with negotiation | Modern Windows (Win10+, Server 2016+) disables SMB1 by default; SMB2 is sufficient for all targets we care about. NTLMSSP wire-up reuses existing managed NTLM impl. |
| (C) Hand-roll SMB1 client | MIT | ✅ | ~3000 | SMB1 only | Does NOT work against default Windows 10+/Server 2016+. Useful only against legacy XP / Server 2003 / old Samba. |
| (D) Hybrid SMB1 + SMB2 | MIT | ✅ | ~7000-8000 | SMB1/2/3 | Doubles surface area for marginal benefit; only matters if we need to talk to pre-Windows-Vista servers. |
| (E) Windows-only `System.IO.Pipes.NamedPipeClientStream` | MIT | ✅ | ~50 | N/A (OS-level) | Trivial but loses the cross-platform goal entirely. Could ship as a Windows-fast-path alongside option B. |
| (F) P/Invoke into `cifs-utils` on Linux / native SMB client on macOS | varies | ❌ | ~500 + native deps | varies | AOT-hostile, fragile, ships native deps. Out of repo conventions. |

The recommended path is **option B** — a hand-rolled, AOT-clean, MIT-licensed
SMB2 client tightly scoped to the named-pipe operations OPC Classic needs.
Phase 1 of that path has landed in `src\Opc.Classic.Dcom.Smb\`; the remaining
work is integration, signing/encryption hardening, and end-to-end validation.

## Sub-protocol surface required from SMB2

Only the connection / session / file / pipe primitives are needed:

| `[MS-SMB2]` § | Command | Use |
|---|---|---|
| §3.2.4.2 / §2.2.3-4 | SMB2 NEGOTIATE | Pick dialect (0x0202 / 0x0210 / 0x0300 / 0x0311) |
| §3.2.4.3 / §2.2.5-6 | SMB2 SESSION_SETUP | Carry the NTLMSSP Type 1 / 2 / 3 GSS-API blobs (already implemented in `Opc.Classic.Dcom/rpc/Auth/`) |
| §3.2.4.4 / §2.2.9-10 | SMB2 TREE_CONNECT | Open `\\host\IPC$` |
| §3.2.4.5 / §2.2.13-14 | SMB2 CREATE | Open `\\host\IPC$\<pipename>` with `FILE_OPEN` + `FILE_NON_DIRECTORY_FILE` + `FILE_OPEN_NO_RECALL` |
| §3.2.4.7 / §2.2.21-22 | SMB2 WRITE | Send DCE/RPC PDU (`bind`, `request`, `alter_context`) |
| §3.2.4.6 / §2.2.19-20 | SMB2 READ | Receive DCE/RPC PDU |
| §3.2.4.10 / §2.2.31-32 | SMB2 IOCTL with `FSCTL_PIPE_TRANSCEIVE` (0x0011C017) | Synchronous transact (write+read in one round-trip); preferred per `[MS-RPCE] §2.1.1.2` for bind / alter_context / last-fragment-of-request + first-fragment-of-response |
| §3.2.4.11 / §2.2.15-16 | SMB2 CLOSE | Close the pipe handle |
| §3.2.4.8 / §2.2.11-12 | SMB2 TREE_DISCONNECT | Close the tree connection |
| §3.2.4.9 / §2.2.7-8 | SMB2 LOGOFF | Tear down session |
| §3.1.5.1 | Signing (HMAC-SHA256 for SMB 3.0 / AES-128-CMAC for SMB 2.0-2.1) | Required when server requires signing (default since Server 2008 R2) |
| §3.1.4.3 | Encryption (AES-128-CCM/GCM) | Required by Server 2022 default config (`SMB Encryption Required = 1`). Phase 1.5; not blocking initial WINREG smoke. |

## Implementation status

| Phase | Status | Output |
|---|---|---|
| 0 — Architecture docs + ADR | ✅ Done | this document + `docs\decisions\2026-05-smb-implementation.md` |
| 1 — SMB2 client | ✅ Landed | `src\Opc.Classic.Dcom.Smb\` with SMB2 negotiate/session/tree/pipe primitives, `TcpSmb2Transport`, and `Smb2RpcTransportAdapter` |
| 1.5 — SMB signing/encryption hardening | ⏳ Pending | signing currently stubbed; encryption deferred before real-server WINREG/activation smoke |
| 2 — Wire SMB into `Ncacn_Np.RpcTransport` | ⏳ Pending | functional `ncacn_np` transport backed by the SMB2 project |
| 3 — WINREG end-to-end smoke | ⏳ Pending | green CI against Samba/Windows container or recorded PCAP fixtures |
| 4 — Legacy `IActivation` interface | ⏳ Pending | client + optional server side for pre-XP-SP2 interop |
| 5 — Cross-platform CI matrix | ⏳ Pending | Ubuntu + macOS + Windows entries in `.github\workflows\build.yml` |
| 6 — PCAP-based wire fixtures | ⏳ Pending | unit-test gold files captured from real Windows exchanges |

## See also

- `docs\architecture\activation-transports.md` — TCP vs SMB activation paths
- `docs\decisions\2026-05-smb-implementation.md` — ADR: chose option B
- `src\Opc.Classic.Dcom.Smb\README.md` — current SMB2 project status and public surface
- `samples\Opc.Classic.Samples.CttServer\README.md` — current Windows-only registration cookbook
- `External\Docs\Win\[MS-SMB2].md` etc. — vendored Microsoft specs
