# Opc.Classic.Dcom.Smb

Minimal, MIT-licensed, AOT-clean SMB2 client tightly scoped to the named-pipe
operations required by OPC Classic's `ncacn_np` (RPC over SMB) transport — see
`docs/architecture/smb-transport.md` and `docs/decisions/2026-05-smb-implementation.md`
for the project-wide rationale.

## Scope (Phase 1 — what ships in commit `*`)

| Component | Status |
|---|---|
| SMB2 packet header (synchronous form) — `Smb2PacketHeader` | ✅ Read + Write |
| NetBIOS-over-TCP framing (4-byte length prefix) — `NetBiosFraming` | ✅ Read + Write |
| SMB2 NEGOTIATE (request + response) | ✅ |
| SMB2 SESSION_SETUP (request + response, NTLMSSP blob carrier) | ✅ |
| SMB2 TREE_CONNECT (request + response) | ✅ |
| SMB2 CREATE / CLOSE (named-pipe handle lifecycle) | ✅ |
| SMB2 READ / WRITE (named-pipe I/O) | ✅ |
| SMB2 IOCTL with FSCTL_PIPE_TRANSCEIVE (per `[MS-RPCE] §2.1.1.2`) | ✅ |
| SMB2 TREE_DISCONNECT / LOGOFF (teardown) | ✅ |
| Connection state machine — `Smb2Connection` | ✅ |
| TCP transport (`TcpSmb2Transport`) — port 445 + NetBIOS framing | ✅ |
| Named-pipe handle — `Smb2NamedPipe` (TransceiveAsync API) | ✅ |
| SMB2 signing (HMAC-SHA256 for SMB 3.x / AES-CMAC for 2.x) | ⏳ stubbed (no MAC on outgoing PDUs, ignored on incoming) |
| SMB2 encryption (AES-128-CCM/GCM for SMB 3.x) | ⏳ deferred to Phase 1.5 |
| NTLMSSP blob threading into SESSION_SETUP | ✅ Carrier API (`NtlmsspBlobProvider`) — actual NTLM Type 1/2/3 generation comes from `src/Opc.Classic.Dcom/rpc/Auth/` |

## Not yet shipping

- SMB 3.0 signing/encryption (will fail against servers with `RequireSecuritySignature=1` until Phase 1.5)
- End-to-end smoke against real Samba / Windows servers (Phase 3)
- Wire-up into `Opc.Classic.Dcom.Rpc.Ncacn_Np.RpcTransport` (Phase 2 — `smb-2-ncacn-np-wireup` todo)
- WINREG client validation (Phase 3 — `smb-3-winreg-e2e` todo)
- Legacy `IActivation::RemoteActivation` interface (Phase 4 — `smb-4-iactivation-legacy` todo)
- Cross-platform CI (Phase 5 — `smb-5-ci-matrix` todo)
- PCAP-recorded wire fixtures (Phase 6 — `smb-6-wire-fixtures` todo)

## Public surface (consumed by Phase 2)

```csharp
using Opc.Classic.Dcom.Smb;

// 1. Establish a TCP connection on port 445
await using var tcp = await TcpSmb2Transport.ConnectAsync("server-host");
await using var conn = new Smb2Connection(new Smb2ConnectionOptions("server-host"), tcp);

// 2. Negotiate the SMB2 dialect
var negotiate = await conn.NegotiateAsync();

// 3. Authenticate via NTLMSSP (caller supplies the blob iterator)
await conn.SessionSetupAsync(serverBlob =>
{
    // First call: produce Type 1 message; subsequent calls: produce Type 3
    return YourNtlmsspProvider.Next(serverBlob);
});

// 4. Open the IPC$ tree
await conn.TreeConnectIpcAsync();

// 5. Open a named pipe
await using var pipe = await conn.OpenNamedPipeAsync("winreg");

// 6. Transact: send DCE/RPC PDU, receive response, in one round-trip
ReadOnlyMemory<byte> response = await pipe.TransceiveAsync(rpcPduBytes);
```

## Specifications referenced

All section references in source-file comments target the vendored Microsoft
Open Specifications under `External/Docs/Win/`:

- `[MS-SMB2].md` — SMB 2.0/2.1/3.0/3.1.1 wire format
- `[MS-CIFS].md` — NetBIOS-over-TCP framing
- `[MS-RPCE].md §2.1.1.2` — RPC over SMB framing rules
- `[MS-FSCC].md` — FSCTL codes
- `[MS-NLMP].md` — NTLMSSP (consumed indirectly via `src/Opc.Classic.Dcom/rpc/Auth/`)
- `[MS-ERREF].md` — NTSTATUS values

## Testing

- `tests/Opc.Classic.Dcom.Smb.Tests/` — codec round-trip + state-machine tests using a hand-built mock transport (no network I/O).
- Real-server smoke tests land in Phase 3 (Samba container in CI + scheduled Windows VM run).

## License

MIT, same as the rest of the repository. No LGPL or other reciprocal-license
dependencies; see `docs/decisions/2026-05-smb-implementation.md` for the
licensing rationale.
