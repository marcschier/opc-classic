# Spec coverage reviews

This directory contains per-spec gap-analysis reviews comparing each OPC specification's protocol surface against the `Opc.Classic.*` implementation. Each review:

- Reads the full spec markdown in `External/Docs/`
- Cross-references every interface, method, struct, error code, and behavioral requirement against the corresponding `src/Opc.Classic.<spec>/` implementation and `tests/Opc.Classic.<spec>.Tests/` coverage
- Reports gaps with severity (BLOCKER / HIGH / MEDIUM / LOW), spec section citations, and source file:line references
- Where the implementation is complete, recommends integration test scenarios to harden coverage

## Reviews

| Spec | Doc | Method coverage | Severity breakdown |
|---|---|---|---|
| [OPC AE 1.10](ae-1.10.md) | Alarms & Events | 26/37 declared (70%); only 16/37 with correct opnums (43%) | **10 BLOCKER** / 7 HIGH / 4 MEDIUM / 0 LOW |
| [OPC Batch 2.00](batch-2.00.md) | Batch | 6/11 declared (55%); 2/2 struct codecs | 1 BLOCKER / 3 HIGH / 1 MEDIUM / 1 LOW |
| [OPC Common 1.10](common-1.10.md) | Common (locale, shutdown, server-list) | ~92% (5/5 codecs, 48/52 elements) | 0 BLOCKER / 0 HIGH / 1 MEDIUM / 5 LOW |
| [OPC CPX 1.00](cpx-1.00.md) | Complex Data | 11/11 method projections; 0/2 codec systems | **2 BLOCKER** / 2 HIGH / 1 MEDIUM / 1 LOW |
| [OPC DA 2.05a](da-2.05a.md) | DA (V20 back-compat) | V20: 3/19 (intentionally minimal); Dcom: 43+ methods | 0 BLOCKER / 1 HIGH / 3 MEDIUM / 0 LOW |
| [OPC DA 3.00](da-3.00.md) | DA (flagship) | 47 methods declared (post gap-9) | 0 BLOCKER / 8 HIGH / 5 MEDIUM / 3 LOW |
| [OPC DX 1.00](dx-1.00.md) | Data eXchange | 3/14 methods (codec-blocked); 0/16 structs | 0 BLOCKER / 4 HIGH / 1 MEDIUM / 0 LOW |
| [OPC HDA 1.20](hda-1.20.md) | Historical Data Access | 56/56 declared (post gap-10); 5/5 codecs | 0 BLOCKER / 0 HIGH / 1 MEDIUM / 0 LOW |
| [OPC Security 1.00](security-1.00.md) | Security | 6/6 methods (100%); 2/2 interfaces | 0 BLOCKER / 0 HIGH / 0 MEDIUM / 2 LOW |
| [OPC XML-DA 1.01](xmlda-1.01.md) | XML-DA (SOAP transport) | 8/8 operations; scalar types only | 0 BLOCKER / 1 HIGH / 3 MEDIUM / 0 LOW |

## Cross-cutting themes

### AE opnum mismatch epidemic

The OPC AE review surfaced an `[OpcMethod(opnum)]` mismatch pattern similar to the DA opnum bugs fixed in commit `aaa3ad5`. Of 26 declared AE methods, **only 16 have correct opnums** (43%). Recommendation: dedicated AE opnum-fix wave mirroring gap-15-getstatus-opnum.

### Interface-pointer return methods

Multiple specs (AE `CreateEventSubscription`, Batch `CreateEnumerator`, DA `AddGroup`/`CreateGroupEnumerator`, HDA `IOPCHDA_Browser`) defer methods that return COM interface pointers. The proxy generator gained STDOBJREF/MEOW handle support in commit `cda87ac` (M1); the deferred methods now CAN be implemented but haven't been migrated from hand-written shims yet.

### Multi-out parameter shapes

DA methods like `IOPCSyncIO::Read`, `IOPCAsyncIO2::Read/Write`, `IOPCBrowse::Browse`, and HDA `IOPCHDA_SyncRead::ReadRaw` use multi-out + continuation-point patterns. The generator gained `[OpcGenerateMultiOutRecord]` support in M1. Several methods remain stubbed with `NotSupportedException` pending migration.

### Codec registration gaps

- CPX: XML Schema and OPCBinary codecs entirely absent (BLOCKER for any real CPX server interop)
- DX: 0 of 16 DX-specific structs have codecs registered (proxies use empty-payload placeholders)

### Error code coverage

Most specs define spec-specific HRESULT constants (OPCAE_E_*, OPCB_E_NOT_MEANINGFUL, OPCCPX_E_TYPE_CHANGED, etc.). The reviews flagged 100+ missing error constants across the assembly. These are trivial to add but improve adopter ergonomics.

### XML-DA array types

The XML-DA review flagged that scalar values work but **array values are unsupported** (0/10 array data types). This is a real limitation for industrial process-control workloads where multi-sample reads are common.

## Read order

For the most actionable findings, read in this order:

1. **[da-3.00.md](da-3.00.md)** — flagship spec; biggest implementation surface
2. **[hda-1.20.md](hda-1.20.md)** — 100% declared post-gap-10; mostly coverage gaps
3. **[ae-1.10.md](ae-1.10.md)** — opnum bugs need a dedicated fix wave
4. **[common-1.10.md](common-1.10.md)** — foundational; high coverage already
5. **[security-1.00.md](security-1.00.md)** — small spec, near-complete
6. **[batch-2.00.md](batch-2.00.md)** — moderate coverage; codec-ready follow-ups
7. **[cpx-1.00.md](cpx-1.00.md)** — codec registration is the bottleneck
8. **[dx-1.00.md](dx-1.00.md)** — deferred by design; codec layer needed for activation
9. **[da-2.05a.md](da-2.05a.md)** — back-compat shim; minor gaps
10. **[xmlda-1.01.md](xmlda-1.01.md)** — separate SOAP transport; scalar-only

## How to use these reviews

- **Adopters**: skim the "Gaps in implementation" sections of the specs you care about to understand current capabilities
- **Contributors**: each gap is sized (effort estimate) and prioritized (severity) — pick a BLOCKER/HIGH from your area of interest
- **Release-planning**: the cross-cutting themes above shape M11+ work items toward 1.0.0
