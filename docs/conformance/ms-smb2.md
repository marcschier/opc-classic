# MS-SMB2 (Server Message Block 2 / 3 Protocol) conformance review

**Spec:** `opc-classic-docs/MS-SMB2.md` (Server Message Block (SMB) Protocol Versions 2 and 3).

**Scope:** SMB2 / SMB3 client surface used by Opc.Classic for two purposes: (1) the legacy `OPCEnum` enumeration path that walks remote registries over `\\<host>\IPC$\winreg`, and (2) cross-machine named-pipe DCOM transport (`\\<host>\IPC$\pipename`). Covers SMB2 NEGOTIATE / SESSION_SETUP / TREE_CONNECT / CREATE / READ / WRITE / IOCTL / CLOSE / LOGOFF, plus header signing (HMAC-SHA-256), encryption (AES-CCM-128 / AES-GCM-128), dialect 2.x / 3.x negotiation, NetBIOS-over-TCP framing, and the `\\PIPE\` access surface.

**Implementing assemblies:** `Opc.Classic.Dcom.Smb` (client-only — server SMB2 hosting is out of scope for DCOM scenarios).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| SMB2 packet header | §2.2.1 | ✅ `Smb2PacketHeader` | ✅ `Smb2WireFixtureTests` | conformant |
| `SMB2 NEGOTIATE` request / response | §2.2.3 / §2.2.4 | ✅ `Smb2NegotiateMessages` | ✅ `Smb2NegotiateMessagesTests`, `Smb2NegotiateReplayTests` | conformant |
| `SMB2 SESSION_SETUP` request / response (GSS-API token blob inside) | §2.2.5 / §2.2.6 | ✅ `Smb2SessionSetupMessages` | ✅ | conformant |
| `SMB2 TREE_CONNECT` request / response | §2.2.9 / §2.2.10 | ✅ `Smb2TreeMessages` | ✅ `Smb2TreeMessagesTests` | conformant |
| `SMB2 TREE_DISCONNECT` request / response | §2.2.11 / §2.2.12 | ✅ same | ✅ | conformant |
| `SMB2 CREATE` request / response (open file or pipe) | §2.2.13 / §2.2.14 | ✅ `Smb2CreateCloseMessages` | ✅ | conformant |
| `SMB2 CLOSE` request / response | §2.2.15 / §2.2.16 | ✅ same | ✅ | conformant |
| `SMB2 READ` request / response | §2.2.19 / §2.2.20 | ✅ `Smb2ReadWriteIoctlMessages` | ✅ | conformant |
| `SMB2 WRITE` request / response | §2.2.21 / §2.2.22 | ✅ same | ✅ | conformant |
| `SMB2 IOCTL` request / response | §2.2.31 / §2.2.32 | ✅ same | ✅ | conformant |
| `SMB2 LOGOFF` request / response | §2.2.7 / §2.2.8 | ✅ `Smb2Connection` | ✅ | conformant |
| HMAC-SHA-256 packet signing (dialect 2.0/2.1) | §3.1.5.1 | ✅ `Smb2Signer` | ✅ `Smb2SignerTests` | conformant |
| AES-CMAC packet signing (dialect 3.0/3.1.1) | §3.1.5.1 | ✅ same | ✅ same | conformant |
| AES-CCM-128 encryption (dialect 3.0/3.0.2) | §3.1.4.3 | ✅ `Smb2Crypter`, `Smb2TransformHeader` | ✅ `Smb2CrypterTests` | conformant |
| AES-GCM-128 encryption (dialect 3.1.1) | §3.1.4.3 | ✅ same | ✅ same | conformant |
| Dialect negotiation (2.0.2, 2.1, 3.0, 3.0.2, 3.1.1) | §3.2.4.2.2 | ✅ `Smb2NegotiateMessages`, `Smb2Constants` | ✅ `Smb2NegotiateMessagesTests` | conformant |
| Pre-auth integrity (dialect 3.1.1) | §3.2.4.2.2.2 | ✅ via negotiate-context | ✅ | conformant |
| Compression (dialect 3.1.1) | §3.1.4.4 | ❌ not implemented | n/a | deferred-by-design |
| NetBIOS-over-TCP framing (4-byte length prefix) | §2.1 | ✅ `TcpSmb2Transport`, NetBIOS framing | ✅ `NetBiosFramingTests` | conformant |
| `\\PIPE\winreg` access (MS-RRP transport) | §2.2.13 + MS-RRP | ✅ via `Smb2RpcTransportAdapter` | ✅ `WinregFixtureReplayTests`, `MockWinregServer` | conformant |
| Server role (`SMB2 server`) | §3.3 | ❌ not implemented | n/a | deferred-by-design |
| RDMA / SMB Direct | MS-SMBD | ❌ not implemented | n/a | deferred-by-design |
| Multi-channel | §3.2.4.20 | ❌ not implemented | n/a | deferred-by-design |
| Persistent / durable handles | §3.2.4.6 | ❌ not implemented | n/a | deferred-by-design |
| Witness service | MS-SWN | ❌ not implemented | n/a | deferred-by-design |

---

## 1 Surface-by-surface coverage matrix

### 1.1 Packet header (spec §2.2.1)

| Surface | Source | Tests |
|---|---|---|
| `SMB2 Packet Header` (`ProtocolId = 0xFE534D42`, `StructureSize = 64`, `CreditCharge`, `(ChannelSequence, Reserved) \| Status`, `Command`, `Credits`, `Flags`, `NextCommand`, `MessageId`, `TreeId`, `SessionId`, `Signature[16]`) | `src/Opc.Classic.Dcom.Smb/Smb2PacketHeader.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/Smb2WireFixtureTests.cs` |
| Async + sync flags (`SMB2_FLAGS_ASYNC_COMMAND`) | same | same |
| Compound chaining (`NextCommand` field) | same | same |
| Signing flag (`SMB2_FLAGS_SIGNED`) | same | `Smb2SignerTests.cs` |

### 1.2 Messages (spec §2.2.x)

| Command | Numeric | Source | Tests |
|---|---|---|---|
| `NEGOTIATE` (0x0000) | §2.2.3 / §2.2.4 | `src/Opc.Classic.Dcom.Smb/Smb2NegotiateMessages.cs` | `Smb2NegotiateMessagesTests.cs`, `Smb2NegotiateReplayTests.cs` |
| `SESSION_SETUP` (0x0001) | §2.2.5 / §2.2.6 | `Smb2SessionSetupMessages.cs` | `Smb2WireFixtureTests` |
| `LOGOFF` (0x0002) | §2.2.7 / §2.2.8 | `Smb2Connection.cs` | covered by connection tests |
| `TREE_CONNECT` (0x0003) | §2.2.9 / §2.2.10 | `Smb2TreeMessages.cs` | `Smb2TreeMessagesTests.cs` |
| `TREE_DISCONNECT` (0x0004) | §2.2.11 / §2.2.12 | same | same |
| `CREATE` (0x0005) | §2.2.13 / §2.2.14 | `Smb2CreateCloseMessages.cs` | `Smb2WireFixtureTests` |
| `CLOSE` (0x0006) | §2.2.15 / §2.2.16 | same | same |
| `READ` (0x0008) | §2.2.19 / §2.2.20 | `Smb2ReadWriteIoctlMessages.cs` | same |
| `WRITE` (0x0009) | §2.2.21 / §2.2.22 | same | same |
| `IOCTL` (0x000B) | §2.2.31 / §2.2.32 | same | same |

### 1.3 Security (spec §3.1.5)

| Surface | Source | Tests |
|---|---|---|
| HMAC-SHA-256 signing (dialect 2.x) | `src/Opc.Classic.Dcom.Smb/Smb2Signer.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/Smb2SignerTests.cs` |
| AES-CMAC signing (dialect 3.x) | same | same |
| AES-CCM-128 encryption | `Smb2Crypter.cs`, `Smb2TransformHeader.cs` | `Smb2CrypterTests.cs` |
| AES-GCM-128 encryption (dialect 3.1.1) | same | same |
| Pre-auth integrity (dialect 3.1.1) | `Smb2NegotiateMessages.cs` (negotiate-context) | `Smb2NegotiateMessagesTests.cs` |
| Session key derivation (`SMB2APP` + dialect-specific KDF) | `Smb2Signer.cs` | `Smb2SignerTests.cs` |

### 1.4 Transport (spec §2.1)

| Surface | Source | Tests |
|---|---|---|
| NetBIOS-over-TCP framing (4-byte length prefix, port 445 / 139) | `src/Opc.Classic.Dcom.Smb/TcpSmb2Transport.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/NetBiosFramingTests.cs` |
| Connection state machine | `Smb2Connection.cs` | `Smb2ConnectionTests.cs` |
| Address resolution (UNC + DNS) | covered by `Smb2RpcTransportAdapter.cs` | `SmbRpcAddressTests.cs` |

### 1.5 RPC-over-named-pipe-over-SMB2 (spec §2.2.13 CREATE on `\PIPE\` + MS-RRP)

| Surface | Source | Tests |
|---|---|---|
| `\\<host>\IPC$\<pipename>` access | `src/Opc.Classic.Dcom.Smb/Smb2RpcTransportAdapter.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/WinregFixtureReplayTests.cs`, `MockWinregServer.cs` |
| MS-RRP (`\PIPE\winreg`) as concrete consumer | `src/Opc.Classic.Discovery/RemoteRegistryEnum.cs` (legacy fallback path) | `WinregFixtureReplayTests.cs` |
| Pcap fixture replay (real-network bytes) | `Smb2PcapReplayer.cs`, `PcapFileReader.cs`, `PcapFixtureBase.cs` | `Smb2WireFixtureTests.cs` |

### 1.6 Fuzz + bounds tests

| Surface | Source |
|---|---|
| Decoder bounds-checking fuzz | `tests/Opc.Classic.Dcom.Smb.Tests/Smb2DecoderBoundsFuzzTests.cs` |
| Decoder structure fuzz | `Smb2DecoderFuzzTests.cs` |
| Message bounds | `src/Opc.Classic.Dcom.Smb/Smb2MessageBounds.cs` |
| Protocol exception model | `Smb2ProtocolException.cs` |

---

## 2 Normative-clause checklist

MS-SMB2 contains **3805 MUST/SHALL clauses** per Phase 0 inventory — the largest spec in our scope. §-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 41 | ✅ informative | n/a |
| §2.1 | Transport | 32 | ✅ NetBIOS-over-TCP conformant; RDMA / multi-channel deferred | §1.4 |
| §2.2 | Message formats (all commands) | 1187 | ✅ for the 10 commands we consume (NEGOTIATE / SESSION_SETUP / TREE_CONNECT / CREATE / READ / WRITE / IOCTL / CLOSE / LOGOFF / TREE_DISCONNECT); other commands (LOCK / OPLOCK_BREAK / NOTIFY / QUERY_DIRECTORY / QUERY_INFO / SET_INFO / FLUSH / ECHO / CANCEL / SERVER_TO_CLIENT_NOTIFICATION) are not implemented | §1.2 |
| §3.1 | Common processing rules | 632 | ✅ for the implemented commands | §1.1 - §1.5 |
| §3.2 | Client (the only role we implement) | 814 | ✅ conformant | §1.1 - §1.5 |
| §3.3 | Server | 1057 | n/a — Opc.Classic does not implement an SMB2 server | n/a |
| §5 | Security | 42 | ✅ HMAC-SHA-256 + AES-CMAC signing + AES-CCM/GCM encryption | §1.3 |

Phase 2 deep-validation will pin the client-side clauses for the
10 implemented commands.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 SMB2 server role not implemented

Opc.Classic is a client of SMB2 for `\\PIPE\` access; no server
hosting is provided. Status: **WAIVED** (deferred-by-design — DCOM
named-pipe servers run on Windows by hosting `svchost.exe \\winreg`
already; we never replace that).

#### 3.1.2 Compression (dialect 3.1.1) not implemented

Per §3.1.4.4 SMB2 3.1.1 supports `SMB2_COMPRESSION_CAPABILITIES`
negotiation. Not implemented. Status: **WAIVED** (deferred —
compression is bandwidth-optimization only).

#### 3.1.3 RDMA / SMB Direct (MS-SMBD) not implemented

Status: **WAIVED** — high-performance niche feature; out of scope.

#### 3.1.4 Multi-channel (§3.2.4.20) not implemented

Status: **WAIVED** — high-availability niche; out of scope.

#### 3.1.5 Persistent / durable handles (§3.2.4.6) not implemented

Status: **WAIVED** — file-server feature; not relevant to `\PIPE\`.

#### 3.1.6 Witness service (MS-SWN) not implemented

Status: **WAIVED** — cluster failover notification; not relevant.

#### 3.1.7 Commands LOCK / OPLOCK_BREAK / NOTIFY / QUERY_DIRECTORY / QUERY_INFO / SET_INFO / FLUSH / ECHO / CANCEL not implemented

These 9 SMB2 commands have no consumer in the `\PIPE\` scenario. They
would be needed for a generic SMB2 file-share client. Status:
**WAIVED** (deferred-by-design).

### 3.2 Hard gaps

None at present. The 10 client-side commands we consume are
implemented, signed/encrypted per spec, wire-byte fixture tested, and
exercised end-to-end via `WinregFixtureReplayTests` against captured
real-network bytes.

---

## 4 Cross-references

- Architecture: [`docs/architecture/smb-transport.md`](../architecture/smb-transport.md)
- Architecture: [`docs/architecture/dcom-container-networking.md`](../architecture/dcom-container-networking.md)
- Related spec: [`docs/conformance/ms-cifs.md`](ms-cifs.md) — SMB1 legacy fallback (forthcoming).
- Related spec: [`docs/conformance/ms-rrp.md`](ms-rrp.md) — Remote Registry over SMB2 named pipe (forthcoming).
- Related spec: [`docs/conformance/ms-spng.md`](ms-spng.md) — `SESSION_SETUP` carries SPNEGO tokens.
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-SMB2.md` (Microsoft Open
Specifications MS-SMB2: Server Message Block (SMB) Protocol Versions
2 and 3).

Phase 0 inventory:

- `files/conformance/inventory/ms-smb2-headings.csv` (330 entries)
- `files/conformance/inventory/ms-smb2-clauses.csv` (3805 normative entries)
