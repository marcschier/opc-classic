# OPC and Microsoft protocol specifications (auto-generated markdown)

This directory contains markdown conversions of protocol specification PDFs from the upstream OPC Foundation and Microsoft bundles.

## OPC Foundation specifications

The 9 OPC Foundation specs implemented by Opc.Classic, plus the OPC Common 1.10 specification :

| Spec | Markdown | Notes |
| --- | --- | --- |
| OPC DA 2.05a | [OPC-DA-2.05A.md](OPC-DA-2.05A.md) | |
| OPC DA 3.00 | [OPC-DA-3.00.md](OPC-DA-3.00.md) | Errata applied inline (5 entries from dataaccess-3.00-errata.zip) |
| OPC HDA 1.20 | [OPC-HDA-1.20.md](OPC-HDA-1.20.md) | Errata applied: structured Errata 2.0-7.0 inline; table-heavy worked-example corrections appended as final appendix. |
| OPC AE 1.10 | [OPC-AE-1.10.md](OPC-AE-1.10.md) | |
| OPC Common 1.10 | [OPC-COMMON-1.10.md](OPC-COMMON-1.10.md) | Extracted from `opc-common-1.10-specification-20151201.zip` (Word document) |
| OPC Batch 2.00 | [OPC-BATCH-2.00.md](OPC-BATCH-2.00.md) | |
| OPC Commands (Cpx) 1.00 | [OPC-CPX-1.00.md](OPC-CPX-1.00.md) | |
| OPC Data eXchange (DX) 1.00 | [OPC-DX-1.00.md](OPC-DX-1.00.md) | |
| OPC Security 1.00 | [OPC-SECURITY-1.00.md](OPC-SECURITY-1.00.md) | |
| OPC XML-DA 1.01 | [OPC-XMLDA-1.01.md](OPC-XMLDA-1.01.md) | |

## Microsoft Open Specifications

The 29 Microsoft Open Specifications (`MS-*`) relevant to the DCOM, RPC, SMB, and authentication chains implemented by Opc.Classic. They are reference material for protocol implementation. Each `MS-*.md` mirrors the same-named upstream Microsoft Open Specifications PDF.

Specs are grouped by role:

**Direct citations in code or docs (12):**

| Spec | Markdown |
| --- | --- |
| MS-DCOM (Distributed COM) | [MS-DCOM.md](MS-DCOM.md) |
| MS-RPCE (RPC Extensions) | [MS-RPCE.md](MS-RPCE.md) |
| MS-NLMP (NTLM Authentication) | [MS-NLMP.md](MS-NLMP.md) |
| MS-KILE (Kerberos Extensions) | [MS-KILE.md](MS-KILE.md) |
| MS-SPNG (SPNEGO) | [MS-SPNG.md](MS-SPNG.md) |
| MS-SMB2 (SMB 2/3) | [MS-SMB2.md](MS-SMB2.md) |
| MS-CIFS (Common Internet File System / SMB1) | [MS-CIFS.md](MS-CIFS.md) |
| MS-OAUT (OLE Automation) | [MS-OAUT.md](MS-OAUT.md) |
| MS-RRP (Remote Registry) | [MS-RRP.md](MS-RRP.md) |
| MS-FSCC (File System Control Codes) | [MS-FSCC.md](MS-FSCC.md) |
| MS-CSSP (Credential Security SP) | [MS-CSSP.md](MS-CSSP.md) |
| MS-ERREF (Windows Error Codes) | [MS-ERREF.md](MS-ERREF.md) |

**Foundational types (3):** [MS-DTYP.md](MS-DTYP.md), [MS-UCODEREF.md](MS-UCODEREF.md), [MS-LCID.md](MS-LCID.md)

**RPC chain transitives (2):** [MS-RPCL.md](MS-RPCL.md), [MS-RPCH.md](MS-RPCH.md)

**SMB chain transitives (4):** [MS-SMB.md](MS-SMB.md), [MS-SMBD.md](MS-SMBD.md), [MS-FSA.md](MS-FSA.md), [MS-DFSC.md](MS-DFSC.md)

**Kerberos chain transitives (5):** [MS-PAC.md](MS-PAC.md), [MS-SFU.md](MS-SFU.md), [MS-KKDCP.md](MS-KKDCP.md), [MS-PKCA.md](MS-PKCA.md), [MS-NEGOEX.md](MS-NEGOEX.md)

**Auth supporting (3):** [MS-LSAD.md](MS-LSAD.md), [MS-NRPC.md](MS-NRPC.md), [MS-APDS.md](MS-APDS.md)

The full upstream MS-* and MC-* catalog (422 + 27 additional specs) is no longer vendored. If a contributor needs a Microsoft Open Specification that is not present here, fetch it on demand from the Microsoft Open Specifications portal.
