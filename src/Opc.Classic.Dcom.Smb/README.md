# Opc.Classic.Dcom.Smb

Minimal, MIT-licensed, AOT-clean SMB2 client tightly scoped to the named-pipe operations required by OPC Classic's `ncacn_np` (RPC over SMB) transport — see `docs\architecture\smb-transport.md` for the project-wide rationale.

## Scope

| Component | Status |
| --- | --- |
| SMB2 packet header (synchronous form) — `Smb2PacketHeader` | ✅ Read + Write |
| NetBIOS-over-TCP framing (4-byte length prefix) — `TcpSmb2Transport` | ✅ Read + Write |
| SMB2 NEGOTIATE (request + response) | ✅ |
| SMB2 SESSION_SETUP (request + response, NTLMSSP blob carrier) | ✅ |
| SMB2 TREE_CONNECT (request + response) | ✅ |
| SMB2 CREATE / CLOSE (named-pipe handle lifecycle) | ✅ |
| SMB2 READ / WRITE (named-pipe I/O) | ✅ |
| SMB2 IOCTL with FSCTL_PIPE_TRANSCEIVE (per `[MS-RPCE] §2.1.1.2`) | ✅ |
| SMB2 TREE_DISCONNECT / LOGOFF (teardown) | ✅ |
| Connection state machine — `Smb2Connection` | ✅ |
| Named-pipe handle — `Smb2NamedPipe` (`TransceiveAsync`) | ✅ |
| RPC adapter/builder — `Smb2RpcTransportAdapter`, `Smb2RpcTransportBuilder`, `SmbRpcAddress` | ✅ sync bridge for legacy `ITransport` callers |
| NTLMSSP blob threading into SESSION_SETUP | ✅ Carrier API (`NtlmsspBlobProvider`) — actual NTLM Type 1/2/3 generation comes from `Auth` |
| WINREG replay validation | ✅ captured request/response fixtures under Winreg tests |
| Samba WINREG smoke | ✅ `samba` fixture + `.github\workflows\samba-smoke.yml` gated by `OPC_CLASSIC_INTEGRATION_SAMBA=1` |
| SMB2 signing (HMAC-SHA256 for SMB 2.0.2/2.1; AES-CMAC for SMB 3.x) | ✅ signs outgoing PDUs and verifies signed responses when SessionKey is supplied |
| SMB3 encryption (AES-128-CCM/GCM with `SMB2 TRANSFORM_HEADER`) | ✅ negotiates ciphers, derives directional keys, encrypts requests, and authenticates/decrypts responses when the session or share requires encryption |
| `ncacn_np` transport wire-up | ✅ `NcacnNpTransport` carries remote named-pipe RPC over this SMB stack |
| WINREG and activation wiring | ✅ remote registry and legacy activation bindings can select `ncacn_np`; local named pipes use `LocalNamedPipeTransport` |

## Implementation boundaries

- This is a named-pipe RPC client, not a general SMB file-share client.
- The client honors server-required session/share encryption but does not expose a client-side “require encryption” policy when the server permits plaintext.
- Named-pipe RPC services beyond the OPC-related WINREG and activation scenarios are outside scope.

## External validation limitations

- The opt-in Samba WINREG smoke covers a live server, but Windows Server SMB/WINREG and encryption-policy combinations are not continuously exercised in the default test run.
- The PCAP replay harness is shipped; additional redacted real-world SMB3 encrypted captures are still needed for broader implementation-to-implementation evidence.

## Public surface

```csharp
using Opc.Classic.Dcom.Smb;
using Opc.Classic.Dcom.Smb.Rpc;

// 1. Establish a TCP connection on port 445
await using var tcp = await TcpSmb2Transport.ConnectAsync("server-host");
await using var conn = new Smb2Connection(new Smb2ConnectionOptions("server-host"), tcp);

// 2. Negotiate the SMB2 dialect
var negotiate = await conn.NegotiateAsync();

// 3. Authenticate via NTLMSSP (the exported SessionKey enables signing/encryption)
await conn.SessionSetupAsync(
    serverBlob =>
    {
        // First call: produce Type 1 message; subsequent calls: produce Type 3
        return YourNtlmsspProvider.Next(serverBlob);
    },
    () => YourNtlmsspProvider.SessionKey);

// 4. Open the IPC$ tree
await conn.TreeConnectIpcAsync();

// 5. Open a named pipe
await using var pipe = await conn.OpenNamedPipeAsync("winreg");

// 6. Transact: send DCE/RPC PDU, receive response, in one round-trip
ReadOnlyMemory<byte> response = await pipe.TransceiveAsync(rpcPduBytes);

// Or build the sync-over-async adapter expected by legacy RPC transports.
var address = SmbRpcAddress.Parse("smb://server-host/IPC$/winreg");
var builder = new Smb2RpcTransportBuilder(address, YourNtlmsspProvider.Next, () => YourNtlmsspProvider.SessionKey);
using Smb2RpcTransportAdapter adapter = await builder.BuildAsync();
ReadOnlyMemory<byte> rpcResponse = adapter.Transceive(rpcPduBytes);
```

## Specifications referenced

All section references in source-file comments target the vendored Microsoft Open Specifications:

- `MS-SMB2.md` — SMB 2.0/2.1/3.0/3.1.1 wire format
- `MS-CIFS.md` — NetBIOS-over-TCP framing
- `MS-RPCE.md §2.1.1.2` — RPC over SMB framing rules
- `MS-FSCC.md` — FSCTL codes
- `MS-NLMP.md` — NTLMSSP
- `MS-ERREF.md` — NTSTATUS values

## Testing and fixtures

- Opc.Classic.Dcom.Smb tests — codec round-trip, state-machine, address parser, adapter, and mock-transport tests.
- Winreg tests — captured WINREG bind/open/enumerate request-response fixtures replayed by `WinregFixtureReplayTests`.
- `WinRegSambaSmokeTests` — opt-in Samba real-server smoke used by `.github\workflows\samba-smoke.yml`.
- `FIXTURES.md` — fixture capture/redaction guidance and current fixture inventory.

## License

MIT, same as the rest of the repository. No LGPL or other reciprocal-license dependencies.
