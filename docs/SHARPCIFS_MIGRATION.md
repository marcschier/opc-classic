# SharpCifs.Std migration retrospective

## Status: COMPLETE 2026-05-23

This document is now a historical retrospective. The migration is complete, N7.6 FINAL drop is confirmed, and the distributable repository is license-clean under MIT with no external `SharpCifs.Std` LGPL runtime dependency. Any remaining `SharpCifs.*` compatibility namespaces are in-tree Opc.Classic code, not the old package.

Final outcome:

- ✅ ALL six phases, 2D.1 through 2D.6, are complete.
- ✅ `SharpCifs.Std` package reference and central package version were removed.
- ✅ NTLMSSP messages are self-contained in `Opc.Classic.Dcom.Internal.Ntlm`.
- ✅ Legacy NDR and compatibility surfaces are in-tree and MIT-licensed with the rest of Opc.Classic.
- ✅ The migration record below is preserved for auditability only; it is not an active work plan.

## Why the migration existed

`SharpCifs.Std` was licensed under LGPL-2.1 while this repository now ships under MIT. That transitive licensing mismatch blocked distribution. The package was also dormant and pulled Java-portability shims, NTLM helpers, SMB/NetBIOS code, and NDR helpers into the DCOM assembly even when only small slices were used.

The migration goal was to remove the package reference without changing wire behavior. Each step was mechanical, tested, and independently buildable so regressions could be isolated to one surface area at a time.

## Original inventory

The original `src\Opc.Classic.Dcom` SharpCifs usage, grouped by namespace, was:

| SharpCifs namespace | Using count | Primary role |
| --- | ---: | --- |
| `SharpCifs.Dcerpc.Ndr` | 74 | NDR buffer/codecs used by the legacy DCOM marshalling layer. |
| `SharpCifs.Util.Sharpen` | 52 | Java-portability shims such as `Properties`, `Iterator`, `IOException`, `Collections`, and thread helpers. |
| `SharpCifs.Util` | 5 | Utility helpers such as hex formatting. |
| `SharpCifs.Ntlmssp` | 4 | NTLM Type1/Type2/Type3 message types and flags. |
| `SharpCifs.Smb` | 4 | `NtlmPasswordAuthentication` credential carrier. |
| `SharpCifs` root | 2 | `Config` access used by NTLM defaults. |
| `SharpCifs.Netbios` | 1 | NetBIOS name helper. |

The larger-than-expected `SharpCifs.Dcerpc.Ndr` dependency made the final package drop impossible until the NDR buffer/object surface was replaced with managed in-repo code.

## Final phase ledger

| Phase | Final status | Result |
| --- | --- | --- |
| 2D.1 | ✅ Complete | Replaced `SharpCifs.Util.Sharpen.Properties` with `Opc.Classic.Dcom.Internal.PropertyBag`; updated DCOM call sites and focused tests. |
| 2D.2 | ✅ Complete | Replaced `SharpCifs.Util` helpers with BCL/local equivalents such as `Convert.ToHexString`, `HMACMD5`, and `BinaryPrimitives`. |
| 2D.3 | ✅ Complete | Replaced `SharpCifs.Smb.NtlmPasswordAuthentication` with `System.Net.NetworkCredential` and corrected constructor/accessor semantics. |
| 2D.4 | ✅ Complete | Moved NTLMSSP message/flag call sites behind local `Opc.Classic.Dcom.Internal.Ntlm` wrappers. |
| 2D.5 | ✅ Complete | Moved the DCE/RPC NDR surface into `Opc.Classic.Dcom.Internal.LegacyNdr` and cleared direct package-backed NDR dependencies. |
| 2D.6 / N7.6 FINAL | ✅ Complete | Reimplemented `NtlmMessage`, `Type1Message`, `Type2Message`, and `Type3Message` directly per MS-NLMP §2.2.1.1-3 and removed `SharpCifs.Std` from production dependencies. |

## Retrospective notes

- The Phase 2D.4 forwarding shim intentionally preserved Type1/Type2/Type3 wire bytes before N7.6 swapped in the self-contained implementation.
- The Phase 2D.5 compatibility-shaped NDR surface let the DCOM marshalling layer keep existing call patterns while later milestones move more internals to span-based `Opc.Classic.Ndr` primitives.
- Phase 2C's async transport work remains a separate modernization thread; it is no longer blocked by the SharpCifs package drop.

## Validation record

The migration was accepted only after full solution build/test gates and NTLM vector coverage stayed green in the owning sessions. The release docs now treat 1.0.0 Gate 2 (clear LGPL dependency) as met because the old package is gone and the repository license is MIT.