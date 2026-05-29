# SMB transport — current state and cross-platform plan

OPC Classic clients written for Microsoft Windows use two RPC transport
sequences interchangeably depending on the era of the client and the available
server:

| Sequence | Carrier | Endpoint | Used by |
| --- | --- | --- | --- |
| `ncacn_ip_tcp` | TCP/IP | Port 135 (endpoint mapper) + dynamic | Modern `IRemoteSCMActivator` (DCOM v5.6+, XP SP2+) |
| `ncacn_np` | SMB | `\PIPE\<name>` | Legacy `IActivation`, all `[MS-RRP]` WINREG (`\PIPE\winreg`), event-log / print-spooler / SAM / workstation named-pipe RPC services, etc. |

This repository implements `ncacn_ip_tcp` end-to-end and now ships the focused
`ncacn_np` client path required by OPC Classic discovery and legacy activation.
The legacy `src\Opc.Classic.Dcom\rpc\ncacn_np\RpcTransport.cs` and local
SharpCifs compatibility shims still exist for older call sites, but the active
wire path is `src\Opc.Classic.Dcom.Smb\` plus
`src\Opc.Classic.Dcom\Transport\NcacnNpTransport.cs`.

The SMB path includes `Smb2Connection`, `TcpSmb2Transport`, `Smb2NamedPipe`,
`Smb2RpcTransportAdapter`, SMB2 signing (`Smb2Signer`), SMB 3.x encryption
(`Smb2Crypter`), WINREG client coverage, and the `ncacn_np` transport wire-up.
WINREG round-trips against a Samba container are covered end-to-end in
`tests\Opc.Classic.Integration.Tests\Winreg\`, and byte-level fixture replay
lives under `tests\Opc.Classic.Dcom.Smb.Tests\Fixtures\Winreg\`.

Consumers still handle remote-registry failures gracefully:
`src\Opc.Classic.Discovery\RemoteRegistryEnum.cs` logs `"Remote-registry
enumeration failed for host {Host}; returning no OPC servers. Consider using
OpcEnumClient (OPC.ServerList.1) instead."` and returns an empty list.

## Remaining limits

- OPC Classic uses WINREG and legacy activation; other named-pipe RPC services
  (eventlog, print spooler, workstation, SAM, etc.) are not modeled.
- The PCAP replay harness is shipped, but real-world redacted PCAP captures are
  still pending.
- This is a named-pipe RPC client surface, not a general SMB file-share client.

## Microsoft specifications driving the design

The repository vendors the relevant Microsoft Open Specifications in
`ext/private/docs/`:

| Spec | Relevance |
| --- | --- |
| `MS-CIFS.md` | SMB v1 wire format (mostly informational; we target SMB2+) |
| `MS-SMB2.md` | SMB 2.0.2 / 2.1 / 3.0 / 3.1.1 wire format — the implementation target |
| `MS-RPCE.md §2.1.1.2` | RPC over SMB framing: same DCE/RPC PDUs sent as named-pipe writes / received as reads, with optional transact for synchronous calls |
| `MS-DCOM.md §3.1.2.5.2.3` | Legacy `IActivation::RemoteActivation` opnum 0 |
| `MS-RRP.md §2.1.2` | WINREG client guidance: SHOULD use `ncacn_np` on `\PIPE\winreg` |
| `MS-NLMP.md` | NTLMSSP — already implemented in `Opc.Classic.Dcom\rpc\Auth\` for the TCP path, can be reused inside the SMB2 SESSION_SETUP security blob |

## Cross-platform implementation options

The hard problem is the SMB protocol itself. The options below were considered:

| Option | License | AOT-clean | Lines of code | SMB versions | Recommendation |
| --- | --- | --- | --- | --- | --- |
| **(A) SMBLibrary** (TalAloni/SMBLibrary on NuGet) | LGPL-3.0 | likely (pure C#) | 0 (consume) | SMB1/2/3 | Subject to license review; consult counsel. If approved, fastest to ship but creates dynamic-link / disclosure obligations atypical for an MIT repo. |
| **(B) Hand-roll SMB2 client** ⭐ recommended | MIT (ours) | ✅ | ~4000-5000 | SMB 2.0.2 + 2.1 + 3.0 / 3.1.1 with negotiation | Modern Windows (Win10+, Server 2016+) disables SMB1 by default; SMB2 is sufficient for all targets we care about. NTLMSSP wire-up reuses existing managed NTLM impl. |
| (C) Hand-roll SMB1 client | MIT | ✅ | ~3000 | SMB1 only | Does NOT work against default Windows 10+/Server 2016+. Useful only against legacy XP / Server 2003 / old Samba. |
| (D) Hybrid SMB1 + SMB2 | MIT | ✅ | ~7000-8000 | SMB1/2/3 | Doubles surface area for marginal benefit; only matters if we need to talk to pre-Windows-Vista servers. |
| (E) Windows-only `System.IO.Pipes.NamedPipeClientStream` | MIT | ✅ | ~50 | N/A (OS-level) | Trivial but loses the cross-platform goal entirely. Could ship as a Windows-fast-path alongside option B. |
| (F) P/Invoke into `cifs-utils` on Linux / native SMB client on macOS | varies | ❌ | ~500 + native deps | varies | AOT-hostile, fragile, ships native deps. Out of repo conventions. |

The recommended path is **option B** — a hand-rolled, AOT-clean, MIT-licensed
SMB2 client tightly scoped to the named-pipe operations OPC Classic needs.
The SMB2 client surface, `ncacn_np` wire-up, signing, encryption, and Samba
end-to-end smoke are all implemented in `src\Opc.Classic.Dcom.Smb\` and
`src\Opc.Classic.Dcom\Transport\NcacnNpTransport.cs`.

## Sub-protocol surface required from SMB2

Only the connection / session / file / pipe primitives are needed:

| `[MS-SMB2]` § | Command | Use |
| --- | --- | --- |
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
| §3.1.5.1 | Signing (HMAC-SHA256 for SMB 2.0.2/2.1; AES-128-CMAC for SMB 3.x) | Implemented for signed request/response verification when the NTLMSSP/Kerberos SessionKey is provided |
| §3.1.4.3 | Encryption (AES-128-CCM/GCM) | Implemented with SMB2 TRANSFORM_HEADER, AES-128-CCM for SMB 3.0/3.0.2, and negotiated AES-128-CCM/GCM for SMB 3.1.1. |

## Implementation status

| Component | Status | Output |
| --- | --- | --- |
| Architecture docs | ✅ Done | this document |
| SMB2 client | ✅ Landed | `src\Opc.Classic.Dcom.Smb\` with SMB2 negotiate/session/tree/pipe primitives, `TcpSmb2Transport`, and `Smb2RpcTransportAdapter` |
| SMB signing + encryption | ✅ Landed | SMB2 signing (HMAC-SHA256/AES-CMAC) and SMB3 encryption (AES-128-CCM/GCM transforms) are implemented for encryption-required server smoke |
| `ncacn_np` transport | ✅ Landed | functional `ncacn_np` transport backed by `src\Opc.Classic.Dcom\Transport\NcacnNpTransport.cs` |
| WINREG end-to-end smoke | ✅ Landed | Samba container fixture, no-env soft-skip TUnit smoke (`WinRegSambaSmokeTests`), fixture replay (`WinregFixtureReplayTests`), and CI coverage prove SMB ↔ ncacn_np ↔ WINREG RPC |
| Legacy `IActivation` interface | ✅ Landed | client + server-side dispatcher for pre-XP-SP2 interop in `src\Opc.Classic.Dcom\Activation\` |
| Cross-platform CI matrix | ✅ Landed | Ubuntu + macOS + Windows restore/build/test matrix in `.github\workflows\build.yml` |
| PCAP-based wire fixtures | ⚠️ Harness landed | replay harness shipped; real-world redacted PCAP captures are still pending at `tests\Opc.Classic.Dcom.Smb.Tests\Pcap\Fixtures\` |

## See also

- `docs\architecture\activation-transports.md` — TCP vs SMB activation paths
- `src\Opc.Classic.Dcom.Smb\README.md` — SMB2 project public surface
- `samples\Opc.Classic.Samples.CttServer\README.md` — current Windows-only registration cookbook
- `ext\private\docs\MS-SMB2.md` etc. — vendored Microsoft specs
