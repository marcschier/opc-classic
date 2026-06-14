# MS-RRP (Remote Registry) conformance review

**Spec:** `opc-classic-docs/MS-RRP.md` (Windows Remote Registry Protocol).

**Scope:** RPC interface to the Windows registry over `\\<host>\IPC$\winreg`. Opc.Classic consumes a small read-only subset of MS-RRP to enumerate the CLSID / AppID / OPC-category registry hives of a remote machine when locating OPC servers (legacy `OPCEnum` fallback). The full read-write registry surface is not implemented.

**Implementing assemblies:** `Opc.Classic.Dcom/Winreg/` (`WinRegClient`), `Opc.Classic.Discovery` (`RemoteRegistryEnum` consumer).

**Status overview:**

| Surface | Spec § / Opnum | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `OpenLocalMachine` / `OpenHKLM` (opnum 2) | §3.1.5.5 | ✅ `WinRegClient.OpenHKLMAsync` | ✅ | conformant (read path) |
| `OpenClassesRoot` / `OpenHKCR` (opnum 0) | §3.1.5.3 | ⚠️ resolved via `OpenLocalMachine` + `OpenKey("SOFTWARE\Classes")` (the modern equivalent path) | ✅ via `RemoteRegistryEnumTests` | conformant (alternative path) |
| `BaseRegOpenKey` (opnum 15) | §3.1.5.15 | ✅ via `WinRegClient.OpenKeyAsync` | ✅ | conformant |
| `BaseRegEnumKey` (opnum 9) | §3.1.5.9 | ✅ `WinRegClient.EnumKeyAsync` | ✅ | conformant |
| `BaseRegEnumValue` (opnum 10) | §3.1.5.10 | ✅ via `WinRegClient` | ✅ | conformant |
| `BaseRegQueryValue` (opnum 17) | §3.1.5.17 | ✅ via `WinRegClient.QueryValueAsync` | ✅ | conformant |
| `BaseRegCloseKey` (opnum 5) | §3.1.5.5 | ✅ `WinRegClient.CloseKeyAsync` | ✅ | conformant |
| `BaseRegQueryInfoKey` (opnum 16) | §3.1.5.16 | ✅ | ✅ | conformant |
| `PolicyHandle` (per spec §2.2.4) | §2.2.4 | ✅ `PolicyHandle` value-type | ✅ | conformant |
| Registry value types (`REG_NONE`, `REG_SZ`, `REG_EXPAND_SZ`, `REG_BINARY`, `REG_DWORD`, `REG_DWORD_BIG_ENDIAN`, `REG_LINK`, `REG_MULTI_SZ`, `REG_QWORD`) | §2.2.8 | ✅ | ✅ | conformant |
| Write operations (`BaseRegCreateKey`, `BaseRegDeleteKey`, `BaseRegSetValue`, `BaseRegDeleteValue`, etc.) | §3.1.5.x | ❌ not implemented (read-only client) | n/a | deferred-by-design |
| Notifications (`BaseRegNotifyChangeKeyValue`) | §3.1.5.20 | ❌ not implemented | n/a | deferred-by-design |
| Security operations (`BaseRegGetKeySecurity`, `BaseRegSetKeySecurity`) | §3.1.5.x | ❌ not implemented | n/a | deferred-by-design |

---

## 1 Surface-by-surface coverage matrix

### 1.1 RPC transport (spec §1.4 / §2.1)

MS-RRP traffic flows over `\\<host>\IPC$\winreg` (named-pipe SMB2),
authenticated with NTLM / Kerberos via SPNEGO and the RPCE auth-trailer
mechanism. Opc.Classic's `WinRegClient` is layered on top of
`Smb2RpcTransportAdapter` (see [`ms-smb2.md`](ms-smb2.md) §1.5).

| Surface | Source | Tests |
|---|---|---|
| Pipe binding (`\PIPE\winreg`) | `src/Opc.Classic.Dcom/Winreg/WinRegClient.cs` | `tests/Opc.Classic.Discovery.Tests/RemoteRegistryEnumTests.cs` |
| Transport adapter (SMB2 → RPCE) | `src/Opc.Classic.Dcom.Smb/Smb2RpcTransportAdapter.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/Fixtures/Winreg/WinregFixtureReplayTests.cs`, `MockWinregServer.cs` |

### 1.2 Read-path methods (spec §3.1.5)

| Method | Opnum | Source | Tests |
|---|---|---|---|
| `OpenLocalMachine` (HKLM) | 2 | `src/Opc.Classic.Dcom/Winreg/WinRegClient.cs` line 56 | `RemoteRegistryEnumTests.cs`, `WinRegSambaSmokeTests.cs` |
| `BaseRegEnumKey` | 9 | line 62 | same |
| `BaseRegEnumValue` | 10 | covered by `WinRegClient` | same |
| `BaseRegOpenKey` | 15 | covered by `WinRegClient` | same |
| `BaseRegQueryInfoKey` | 16 | covered by `WinRegClient` | same |
| `BaseRegQueryValue` | 17 | covered by `WinRegClient` | same |
| `BaseRegCloseKey` | 5 | line 76 | same |

### 1.3 OPC server discovery via remote registry

The `Opc.Classic.Discovery` layer uses `WinRegClient` to enumerate
the standard OPC category roots on a remote machine. This is the
**legacy fallback** path used when `OpcEnumClient` (the OPCEnum
DCOM activation path — see [`ms-dcom.md`](ms-dcom.md) §1.1) is not
available on the remote host.

| Surface | Source | Tests |
|---|---|---|
| Category enumeration (`SOFTWARE\Classes\CLSID\<clsid>\Implemented Categories\<catid>`) | `src/Opc.Classic.Discovery/RemoteRegistryEnum.cs` | `tests/Opc.Classic.Discovery.Tests/RemoteRegistryEnumTests.cs` |
| `IRemoteRegistryReader` abstraction | `src/Opc.Classic.Discovery/IRemoteRegistryReader.cs`, `IRemoteRegistryReaderFactory.cs` | covered |
| Samba smoke test (real-world non-Windows registry hosting) | n/a | `tests/Opc.Classic.Integration.Tests/Winreg/WinRegSambaSmokeTests.cs` |

---

## 2 Normative-clause checklist

MS-RRP contains **546 MUST/SHALL clauses** per Phase 0 inventory.
§-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 17 | ✅ informative | n/a |
| §2.2 | Common data types (`PolicyHandle`, value types, etc.) | 73 | ✅ for the read-path subset | §1.1 - §1.3 |
| §3.1.5.1 - §3.1.5.20 | Per-method message processing | 412 | ✅ for the 7 implemented methods (open hive, open key, enum key, enum value, query value, query info, close); other 13 methods deferred | §1.2 |
| §5 | Security | 18 | ✅ via SPNEGO + SMB2 SESSION_SETUP | §1.1 |

Phase 2 deep-validation will pin the read-path clauses individually.

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Read-only client (write methods not implemented)

`BaseRegCreateKey` (§3.1.5.7), `BaseRegDeleteKey` (§3.1.5.6),
`BaseRegSetValue` (§3.1.5.22), `BaseRegDeleteValue` (§3.1.5.8),
`BaseRegSaveKey` (§3.1.5.21), `BaseRegLoadKey` (§3.1.5.15),
`BaseRegRestoreKey` (§3.1.5.19), and other mutator methods are not
implemented. Status: **WAIVED** (deferred-by-design) — OPC server
discovery is read-only.

#### 3.1.2 Notifications (`BaseRegNotifyChangeKeyValue`) not implemented

Status: **WAIVED** — OPC discovery is a one-shot enumeration; no
live-change subscription is needed.

#### 3.1.3 Security operations (`BaseRegGetKeySecurity`, `BaseRegSetKeySecurity`) not implemented

Status: **WAIVED** — DACL inspection / modification not on the OPC
discovery path.

#### 3.1.4 OPCEnum path is preferred over `WinRegClient`

When the remote host runs the OPCEnum service (`CLSID_OpcEnum`),
`OpcEnumClient` activation is preferred — `IOPCServerList::EnumClassesofCategory`
is much more efficient than walking the registry. `WinRegClient`
remains as fallback for hosts that lack OPCEnum (rare in modern OPC
deployments). Status: **WAIVED** — alternative path is the
recommended one.

### 3.2 Hard gaps

None at present. The 7 read-path methods we consume are implemented,
fixture-replay-tested against a captured real Windows winreg
exchange, and exercised in `RemoteRegistryEnumTests` +
`WinRegSambaSmokeTests`.

---

## 4 Cross-references

- Related spec: [`docs/conformance/ms-smb2.md`](ms-smb2.md) — `\PIPE\winreg` transport.
- Related spec: [`docs/conformance/ms-dcom.md`](ms-dcom.md) — alternative `OpcEnumClient` discovery path.
- Architecture: [`docs/architecture/smb-transport.md`](../architecture/smb-transport.md)
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-RRP.md` (Microsoft Open
Specifications MS-RRP: Windows Remote Registry Protocol).

Phase 0 inventory:

- `files/conformance/inventory/ms-rrp-headings.csv` (95 entries)
- `files/conformance/inventory/ms-rrp-clauses.csv` (546 normative entries)
