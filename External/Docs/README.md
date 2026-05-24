# External Protocol Specifications (auto-generated markdown)

This directory contains markdown conversions of the protocol specification PDFs in `External/Spec/`, produced by `markitdown` (Microsoft) with a `pypdf` fallback for PDFs that markitdown could not handle. Source PDFs remain in `External/Spec/` as the authoritative reference.

**Conversion fidelity is approximate.** Tables, figures, and complex layout may render imperfectly. For exact text and figures, refer to the source PDFs.

## OPC Foundation specifications

The 9 OPC Foundation specs implemented by Opc.Classic, plus the opc-common-1.10 specification extracted from its archive:

| Spec | Markdown | Notes |
|---|---|---|
| OPC DA 2.05a | [opc-da-2.05a-specification.md](opc-da-2.05a-specification.md) | |
| OPC DA 3.00 | [opc-da-3.00-specification.md](opc-da-3.00-specification.md) | Errata applied inline (5 entries from dataaccess-3.00-errata.zip) |
| OPC HDA 1.20 | [opc-hda-1.20-specification.md](opc-hda-1.20-specification.md) | Errata: see [opc-hda-1.20-errata-notes.md](opc-hda-1.20-errata-notes.md) — too table-heavy for inline application |
| OPC AE 1.10 | [opc-ae-1.10-specification.md](opc-ae-1.10-specification.md) | |
| OPC Common 1.10 | [opc-common-1.10-specification.md](opc-common-1.10-specification.md) | Extracted from `opc-common-1.10-specification-20151201.zip` (Word document) |
| OPC Batch 2.00 | [opc-batch-2.00-specification.md](opc-batch-2.00-specification.md) | |
| OPC Commands (Cpx) 1.00 | [opc-cpx-1.00-specification.md](opc-cpx-1.00-specification.md) | |
| OPC Data eXchange (DX) 1.00 | [opc-dx-1.00-specification.md](opc-dx-1.00-specification.md) | |
| OPC Security 1.00 | [opc-security-1.00-specification.md](opc-security-1.00-specification.md) | |
| OPC XML-DA 1.01 | [opc-xmlda-1.01-specification.md](opc-xmlda-1.01-specification.md) | |

## Microsoft Open Specifications

The `Win/` subdirectory contains markdown conversions of 424 Microsoft Open Specifications (`MS-*`) PDFs. These are reference material for protocol implementation. Each `[MS-*].md` mirrors `External/Spec/Win/[MS-*].pdf`.

Key specs referenced by Opc.Classic.Dcom:

| Spec | Markdown |
|---|---|
| MS-DCOM (Distributed COM) | [Win/[MS-DCOM].md](Win/[MS-DCOM].md) |
| MS-RPCE (RPC Extensions) | [Win/[MS-RPCE].md](Win/[MS-RPCE].md) |
| MS-OAUT (OLE Automation) | [Win/[MS-OAUT].md](Win/[MS-OAUT].md) |
| MS-NLMP (NTLM Authentication) | [Win/[MS-NLMP].md](Win/[MS-NLMP].md) |
| MS-KILE (Kerberos Extensions) | [Win/[MS-KILE].md](Win/[MS-KILE].md) |
| MS-SPNG (SPNEGO) | [Win/[MS-SPNG].md](Win/[MS-SPNG].md) |
| MS-CIFS (Common Internet File System) | [Win/[MS-CIFS].md](Win/[MS-CIFS].md) |

Full alphabetical listing of all 424 MS-* specs is in the `Win/` directory; see `External/Spec/Win/` for original PDFs. The companion `MC-*` PDFs in `External/Spec/Win/` are outside this generated MS-* markdown set.

## Conversion details

- Tool: `markitdown` (`pip install markitdown[all]`); fallback `pypdf` for PDFs markitdown couldn't process
- Errata sources:
  - `External/Spec/dataaccess-3.00-errata.zip` → applied inline to opc-da-3.00
  - `External/Spec/historical-dataaccess-1.20-errata.zip` → extracted to errata-notes file
  - `External/Spec/opc-common-1.10-specification-20151201.zip` → extracted spec itself
- Conversion produces text-only markdown; embedded images are NOT extracted by default. For images, refer to source PDFs.

## Re-running the conversion

To regenerate any individual file:

```powershell
python -m markitdown 'External/Spec/<input>.pdf' -o 'External/Docs/<output>.md'
```

For PDFs markitdown fails to parse (output < 1KB), use the pypdf fallback:

```powershell
pip install --user pypdf
python -c "from pypdf import PdfReader; r = PdfReader('External/Spec/<input>.pdf'); open('External/Docs/<output>.md', 'w', encoding='utf-8').write('\n\n'.join(p.extract_text() for p in r.pages))"
```
