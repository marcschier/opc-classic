# OPC + MS-\* protocol conformance index

Per-spec coverage matrices for the 22 specifications Opc.Classic
implements. The vendored spec corpus lives at
`D:\git\marcschier\opc-classic-docs\` (sibling repository); each doc
below pins the spec sections, interfaces, methods, structures, and
error codes to source files + tests in this repository, and surfaces
any conformance gaps.

For the historical aggregate write-up see
[`docs/CONFORMANCE.md`](../CONFORMANCE.md).

For known open follow-up gaps see
[`docs/ROADMAP.md` § Open conformance follow-ups](../ROADMAP.md#open-conformance-follow-ups).

## Status snapshot

- Build: warning-free.
- Solution tests: all .NET test projects green.
- Cross-impl matrix: all configured profiles green
  (ctt-da, matrikon, samples-ae, samples-ae-managed, samples-da,
  samples-hda, security-da, testserver).

## OPC Foundation specifications (10)

| Spec | Per-spec doc | Existing review |
|---|---|---|
| OPC DA 2.05a | [`opc-da-2-05a.md`](opc-da-2-05a.md) | [`CONFORMANCE.md#opc-da-205a`](../CONFORMANCE.md#opc-da-205a) |
| OPC DA 3.00 | [`opc-da-3-00.md`](opc-da-3-00.md) | [`CONFORMANCE.md#opc-da-300`](../CONFORMANCE.md#opc-da-300) |
| OPC HDA 1.20 | [`opc-hda-1-20.md`](opc-hda-1-20.md) | [`CONFORMANCE.md#opc-hda-120`](../CONFORMANCE.md#opc-hda-120) |
| OPC AE 1.10 | [`opc-ae-1-10.md`](opc-ae-1-10.md) + [`ae-wire-format.md`](ae-wire-format.md) | [`CONFORMANCE.md#opc-ae-110`](../CONFORMANCE.md#opc-ae-110) |
| OPC Common 1.10 | [`opc-common-1-10.md`](opc-common-1-10.md) | [`CONFORMANCE.md#opc-common-110`](../CONFORMANCE.md#opc-common-110) |
| OPC Batch 2.00 | [`opc-batch-2-00.md`](opc-batch-2-00.md) | [`CONFORMANCE.md#opc-batch-200`](../CONFORMANCE.md#opc-batch-200) |
| OPC Complex Data 1.00 | [`opc-cpx-1-00.md`](opc-cpx-1-00.md) | [`CONFORMANCE.md#opc-complex-data-100`](../CONFORMANCE.md#opc-complex-data-100) |
| OPC DX 1.00 | [`opc-dx-1-00.md`](opc-dx-1-00.md) | [`CONFORMANCE.md#opc-dx-100`](../CONFORMANCE.md#opc-dx-100) |
| OPC Security 1.00 | [`opc-security-1-00.md`](opc-security-1-00.md) | [`CONFORMANCE.md#opc-security-100`](../CONFORMANCE.md#opc-security-100) |
| OPC XML-DA 1.01 | [`opc-xmlda-1-01.md`](opc-xmlda-1-01.md) | [`CONFORMANCE.md#opc-xml-da-101`](../CONFORMANCE.md#opc-xml-da-101) |

## Microsoft Open Specifications (12 directly-cited)

These are the protocols our managed DCOM / RPC / SMB / authentication
stack implements. The remaining 17 supporting MS-\* specs (Kerberos /
SMB / RPC transitives, auth-supporting specs) are out of scope for
this review — see [`docs/CONFORMANCE.md`](../CONFORMANCE.md) for context.

| Spec | Per-spec doc |
|---|---|
| MS-DCOM (Distributed COM) | [`ms-dcom.md`](ms-dcom.md) |
| MS-RPCE (RPC Extensions) | [`ms-rpce.md`](ms-rpce.md) |
| MS-NLMP (NTLM Authentication) | [`ms-nlmp.md`](ms-nlmp.md) |
| MS-KILE (Kerberos Extensions) | [`ms-kile.md`](ms-kile.md) |
| MS-SPNG (SPNEGO) | [`ms-spng.md`](ms-spng.md) |
| MS-SMB2 (SMB 2/3) | [`ms-smb2.md`](ms-smb2.md) |
| MS-CIFS (CIFS / SMB1 legacy) | [`ms-cifs.md`](ms-cifs.md) |
| MS-OAUT (OLE Automation) | [`ms-oaut.md`](ms-oaut.md) |
| MS-RRP (Remote Registry) | [`ms-rrp.md`](ms-rrp.md) |
| MS-FSCC (File System Control Codes) | [`ms-fscc.md`](ms-fscc.md) |
| MS-CSSP (Credential Security SP) | [`ms-cssp.md`](ms-cssp.md) |
| MS-ERREF (Windows Error Codes) | [`ms-erref.md`](ms-erref.md) |

## Conformance summary by category

| Category | OPC specs | MS-\* specs | Notes |
|---|---|---|---|
| Wire-format codecs | ✅ all 10 | ✅ all 12 | Source-generated NDR codecs + hand-written tearoff codecs; wire-byte fixture tests across DCOM / RPCE / NLMP / SMB2 surfaces. |
| Interface projections (DCOM) | ✅ 36+ OPC interfaces | n/a | Generator-emitted client proxies + server dispatchers. |
| Authentication | n/a | ✅ NLMP + KILE + SPNG | Self-contained NTLMv2 + Kerberos + SPNEGO stacks; channel binding via RFC 5929 + MS-NLMP §3.1.5.1.2. |
| Activation + OXID + ORPC | n/a | ✅ DCOM + RPCE | `IRemoteSCMActivator`, `IObjectExporter`, `IRemUnknown(2)`, OBJREF variants, DUALSTRINGARRAY (TCP + named-pipe towers). |
| Transport | n/a | ✅ ncacn_ip_tcp + ncacn_np | ALPC (ncalrpc) deferred-by-design; cross-impl matrix validates both. |
| Error mapping | ✅ all OPC HRESULT ranges | ✅ ERREF facility / severity / customer / N-bit | `OpcResultId.FromWin32` / `FromNtStatus` + `OpcFacility.*` constants per MS-ERREF §2.1. |
| Variant / SafeArray / BSTR | n/a | ✅ OAUT | Comprehensive snapshot + property + fuzz tests for all primary VT_* + array shapes. |

## Validation evidence

- **Build:** `dotnet build Opc.Classic.slnx -c Release` — warning-free.
- **Tests:** `dotnet test Opc.Classic.slnx -c Release --no-build` — all .NET test projects green.
- **Matrix:** `tools/run-cross-impl-matrix.ps1` — all configured profiles green.
- **Phase 0 inventory tooling:** [`tools/conformance_extract.py`](../../tools/conformance_extract.py) + [`tools/conformance_xref.py`](../../tools/conformance_xref.py).
- **Phase 0 inventory artifacts:** CSV files at `files/conformance/inventory/<spec>.csv` (per-spec headings + clauses + interfaces) and `files/conformance/citations.csv` (363 code citations across 1317 scanned files).

## Phase 1 + Phase 2 + Phase 3 outcomes

- **Phase 1 — Coverage matrices.** All 22 specs have a per-spec
  `docs/conformance/<spec>.md` file mapping every implemented surface
  to source files + tests.
- **Phase 2 — Deep validation.** §-range summaries in each per-spec
  doc account for all normative clauses bucketed by surface; the most
  critical spec (MS-DCOM, 874 clauses) is decomposed into 12 §-ranges,
  each cross-referenced to source + tests. Per-clause checklists are
  tracked at the granularity of §-range bucket + implementing source
  file; further per-clause line-by-line drill-down would be backlog
  work, not a release-blocking gap.
- **Phase 3 — Hard-gap remediation.** 6 hard gaps closed inline
  (MS-ERREF: `FromWin32` + `FromNtStatus` helpers, 4 standard HRESULT
  constants, `OpcFacility` class; MS-CSSP: 2 spec-citation comment
  fixes). 13 remaining hard gaps (OPC AE / DA / HDA / DX
  CCW-completion + interface-shape work) are documented as soft-gap
  follow-ups in [`ROADMAP.md` § Open conformance follow-ups](../ROADMAP.md#open-conformance-follow-ups)
  pending dedicated implementation PRs.
