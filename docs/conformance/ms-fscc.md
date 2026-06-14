# MS-FSCC (File System Control Codes) conformance review

**Spec:** `opc-classic-docs/MS-FSCC.md` (File System Control Codes).

**Scope:** MS-FSCC defines the FSCTL control codes + FileInformation classes used by SMB CREATE / IOCTL / QUERY_INFO operations. Opc.Classic consumes a **very narrow subset** of MS-FSCC — only the FSCTL identifiers required to negotiate per-RPC pipe semantics over SMB2 (`FSCTL_PIPE_TRANSCEIVE`, `FSCTL_PIPE_WAIT`, etc.). The broader file-system information classes (FileBasicInformation, FileStandardInformation, FileDirectoryInformation, etc.) are not used.

**Implementing assemblies:** `Opc.Classic.Dcom.Smb` (`Smb2Constants.cs` FSCTL constants + SMB2 IOCTL message handling in `Smb2ReadWriteIoctlMessages.cs`).

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| `FSCTL_PIPE_TRANSCEIVE` (0x0011C017) | §2.3 + §2.3.18 | ✅ `Smb2Constants` FSCTL constants + `Smb2ReadWriteIoctlMessages` | ✅ `Smb2WireFixtureTests` | conformant |
| `FSCTL_PIPE_WAIT` (0x00110018) | §2.3.19 | ⚠️ not actively used | n/a | deferred-by-design |
| Other FSCTL control codes (file system operations, encryption, sparse files, reparse points, etc.) | §2.3.x | ❌ not used | n/a | deferred-by-design |
| FileInformation classes (`FileBasicInformation`, `FileStandardInformation`, `FileInternalInformation`, etc.) | §2.4 | ❌ not used | n/a | deferred-by-design |
| FileSystemInformation classes | §2.5 | ❌ not used | n/a | deferred-by-design |
| ReparsePoint structures | §2.1 | ❌ not used | n/a | deferred-by-design |

---

## 1 Surface-by-surface coverage matrix

### 1.1 FSCTL pipe-control codes (spec §2.3.18 - §2.3.19)

`FSCTL_PIPE_TRANSCEIVE` is the SMB2 IOCTL control code used to perform
a write-then-read on a named pipe in a single round trip — the
standard mechanism for RPC-over-named-pipe transport.

| Surface | Source | Tests |
|---|---|---|
| FSCTL constant declarations | `src/Opc.Classic.Dcom.Smb/Smb2Constants.cs` line 192+ | covered by `Smb2WireFixtureTests` |
| IOCTL message build / parse | `src/Opc.Classic.Dcom.Smb/Smb2ReadWriteIoctlMessages.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/Smb2WireFixtureTests.cs` |
| RPC transport over pipe IOCTL | `src/Opc.Classic.Dcom.Smb/Smb2RpcTransportAdapter.cs` | `tests/Opc.Classic.Dcom.Smb.Tests/Fixtures/Winreg/WinregFixtureReplayTests.cs` |

### 1.2 SMB2 CREATE Disposition + Options

SMB2 CREATE (see [`ms-smb2.md`](ms-smb2.md) §1.2) references MS-FSCC
constants for `CreateDisposition` (`FILE_SUPERSEDE` = 0, `FILE_OPEN` =
1, `FILE_CREATE` = 2, `FILE_OPEN_IF` = 3, `FILE_OVERWRITE` = 4,
`FILE_OVERWRITE_IF` = 5) and `CreateOptions` flag bits
(`FILE_DIRECTORY_FILE`, `FILE_NON_DIRECTORY_FILE`, etc.). These values
are declared in `Smb2Constants.cs` per spec.

### 1.3 What's NOT used

Opc.Classic does not consume:

- File-system information classes (§2.4 - 2.5) — would only be relevant if we were a file-share client.
- Reparse-point structures (§2.1) — irrelevant for pipe access.
- Encryption FSCTLs (`FSCTL_ENCRYPTION_FSCTL_IO`, etc.) — encryption is handled at the SMB2 layer.
- Sparse-file FSCTLs (`FSCTL_SET_SPARSE`, etc.) — irrelevant for pipes.
- Compression FSCTLs (`FSCTL_GET_COMPRESSION`, etc.) — irrelevant.
- Quota / object-id / volume-info classes — irrelevant.

---

## 2 Normative-clause checklist

MS-FSCC contains **564 MUST/SHALL clauses** per Phase 0 inventory.
Only the small subset relating to pipe FSCTLs (~15 clauses) is in
scope for Opc.Classic. §-range summary:

| § range | Topic | Clause count | Status | Evidence |
|---|---|---|---|---|
| §1 | Introduction | 12 | ✅ informative | n/a |
| §2.1 | Reparse points | 41 | n/a — not used | §1.3 |
| §2.3.18 - 2.3.19 | Pipe FSCTLs | ~15 | ✅ conformant for `FSCTL_PIPE_TRANSCEIVE` | §1.1 |
| §2.3 (other FSCTLs) | File system control codes | ~287 | n/a — not used | §1.3 |
| §2.4 | File information classes | 124 | n/a — not used | §1.3 |
| §2.5 | File system information classes | 47 | n/a — not used | §1.3 |
| §5 | Security | 38 | ✅ via SMB2 layer | n/a |

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 Full file-system surface not implemented

Status: **WAIVED** (deferred-by-design) — Opc.Classic uses SMB2
strictly for `\PIPE\` access, not for file-share operations.

### 3.2 Hard gaps

None at present. The single FSCTL constant we use
(`FSCTL_PIPE_TRANSCEIVE`) is correct, wire-byte fixture tested via
`Smb2WireFixtureTests`, and exercised via the `\PIPE\winreg`
end-to-end path.

---

## 4 Cross-references

- Related spec: [`docs/conformance/ms-smb2.md`](ms-smb2.md) — SMB2 IOCTL carries FSCTL codes.
- Related spec: [`docs/conformance/ms-rrp.md`](ms-rrp.md) — Remote Registry over `\PIPE\winreg` is the only consumer of `FSCTL_PIPE_TRANSCEIVE`.
- ROADMAP open items: [`docs/ROADMAP.md`](../ROADMAP.md)

---

## 5 Citation footer

Source: vendored `opc-classic-docs/MS-FSCC.md` (Microsoft Open
Specifications MS-FSCC: File System Control Codes).

Phase 0 inventory:

- `files/conformance/inventory/ms-fscc-headings.csv` (265 entries)
- `files/conformance/inventory/ms-fscc-clauses.csv` (564 normative entries — only ~15 apply to Opc.Classic; remainder deferred-by-design)
