# MS-CIFS (Common Internet File System / SMB1) conformance review

**Spec:** `opc-classic-docs/MS-CIFS.md` (Common Internet File System (CIFS) Protocol — the legacy SMB1 specification).

**Scope:** Opc.Classic does **NOT** implement SMB1 / CIFS as a wire protocol. The only consumer relationship with MS-CIFS is the **NetBIOS-over-TCP framing** (§2.2.1) that SMB2 inherits — a 4-byte length-prefix on TCP-445 / TCP-139. That single framing layer is honoured; the rest of the CIFS protocol (SMB1 commands, SMB1 transactions, SMB1 signing, etc.) is intentionally not implemented.

**Implementing assemblies:** `Opc.Classic.Dcom.Smb` (the NetBIOS framing layer in `TcpSmb2Transport.cs` is the entire MS-CIFS touchpoint).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| NetBIOS-over-TCP framing (4-byte length prefix) | §2.2.1 | ✅ `TcpSmb2Transport`, `Smb2Constants.MaxFrameLength` | ✅ `NetBiosFramingTests` | conformant |
| SMB1 packet header (`0xFF SMB`) | §2.2.3.1 | ❌ not implemented (SMB2-only) | n/a | deferred-by-design |
| SMB1 commands (NEGOTIATE / SESSION_SETUP / TREE_CONNECT / CREATE / READ / WRITE / TRANS / TRANS2 / NT_TRANS / CLOSE / LOGOFF / etc.) | §2.2.4 | ❌ | n/a | deferred-by-design |
| SMB1 transactions (TRANS / TRANS2 / NT_TRANS) | §2.2.5 | ❌ | n/a | deferred-by-design |
| SMB1 signing (MD5-HMAC) | §3.x | ❌ | n/a | deferred-by-design |
| SMB1 dialect negotiation | §3.2.4.2.1 | ❌ | n/a | deferred-by-design — Opc.Classic only negotiates SMB2 dialects 2.0.2 - 3.1.1 |
| NetBIOS name resolution | §2.2.1 (informative) | ✅ resolved via DNS / `ListenAddressParser` | ✅ via SMB2 connection tests | conformant |

---

## 1 Surface-by-surface coverage matrix

### 1.1 NetBIOS-over-TCP framing (spec §2.2.1)

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| 4-byte length prefix (`SessionMessage`, `SessionRequest`, etc.) | §2.2.1 | `src/Opc.Classic.Dcom.Smb/TcpSmb2Transport.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/NetBiosFramingTests.cs` |
| Max frame length constant | §2.2.1 | `src/Opc.Classic.Dcom.Smb/Smb2Constants.cs` (`MaxFrameLength`) | covered |
| Port 445 (direct TCP/IP) preferred over 139 (NetBIOS) | §2.2.1 | `TcpSmb2Transport.cs` | covered |

### 1.2 SMB1 protocol (out of scope)

All SMB1 command formats (`SMB_COM_NEGOTIATE`, `SMB_COM_SESSION_SETUP_ANDX`,
`SMB_COM_TREE_CONNECT_ANDX`, `SMB_COM_NT_CREATE_ANDX`,
`SMB_COM_READ_ANDX`, `SMB_COM_WRITE_ANDX`, `SMB_COM_TRANSACTION`,
`SMB_COM_TRANSACTION2`, `SMB_COM_NT_TRANSACT`, `SMB_COM_CLOSE`,
`SMB_COM_LOGOFF_ANDX`, `SMB_COM_TREE_DISCONNECT`, `SMB_COM_ECHO`, etc.)
are intentionally not implemented. Opc.Classic only speaks SMB2 dialects
2.0.2 and above; servers that require SMB1 are not supported.

This is a deliberate scope decision: SMB1 was deprecated by Microsoft
in 2016 (Windows 10 v1709) and is disabled by default on modern Windows
systems. Supporting SMB1 would re-introduce a known-vulnerable code
path (WannaCry, EternalBlue, etc.) for no measurable interop benefit.

### 1.3 NetBIOS name resolution

NetBIOS name resolution (`nbns`, `nbtstat`) is deliberately not used —
Opc.Classic relies on DNS for hostname-to-IP resolution. The
`ListenAddressParser` in `Opc.Classic.Dcom/Transport/` parses the
caller-supplied `host` string and resolves it via `Dns.GetHostAddresses`.

---

## 2 Normative-clause checklist

MS-CIFS contains **3751 MUST/SHALL clauses** per Phase 0 inventory.
**Only 1 normative clause applies to Opc.Classic** — the NetBIOS-over-TCP
framing length prefix in §2.2.1. The remaining 3750 clauses describe
SMB1 protocol semantics which are deferred-by-design.

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §2.2.1 | NetBIOS-over-TCP framing | ~5 | ✅ conformant | §1.1 |
| §2.2.2 - 2.2.9 | SMB1 packet header, commands, transactions | ~1200 | n/a — not implemented | §1.2 |
| §3.1 - 3.4 | Common / client / server / state | ~2500 | n/a | §1.2 |
| §5 | Security considerations | 46 | n/a — SMB1 deprecated for security | §1.2 |

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 SMB1 / CIFS protocol intentionally not implemented

Status: **WAIVED** (deferred-by-design) — Microsoft deprecated SMB1 in
2016 for security reasons (WannaCry, EternalBlue exploits). Adding
SMB1 support would re-introduce known-vulnerable code paths for no
interop benefit; modern Windows hosts default to SMB2/3 and refuse
SMB1 connections.

### 3.2 Hard gaps

None at present. The single MS-CIFS touchpoint (NetBIOS-over-TCP
framing) is conformant.

---

## 4 Cross-references

- Related spec: [`docs/conformance/ms-smb2.md`](ms-smb2.md) — SMB2 inherits the NetBIOS-over-TCP framing.
- Architecture: [`docs/architecture/smb-transport.md`](../architecture/smb-transport.md)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-CIFS.md` (Microsoft Open
Specifications MS-CIFS: Common Internet File System (CIFS) Protocol).

Phase 0 inventory:

- `files/conformance/inventory/ms-cifs-headings.csv` (700 entries)
- `files/conformance/inventory/ms-cifs-clauses.csv` (3751 normative entries — only ~5 apply to Opc.Classic; remainder deferred-by-design)
