# Spec coverage reviews

This directory contains per-spec gap-analysis reviews comparing each OPC specification's protocol surface against the `Opc.Classic.*` implementation. Each review:

- Reads the full spec markdown in `External/Docs/`
- Cross-references interfaces, methods, structs, error codes, and behavioral requirements against `src/Opc.Classic.*`
- Separates cross-platform DCOM/source-generated coverage from Windows CCW/native-hosting coverage
- Lists source and test file references that exist in the current tree

## Reviews

| Spec | Doc | Current implementation coverage | Remaining status notes |
|---|---|---|---|
| [OPC AE 1.10](ae-1.10.md) | Alarms & Events | DCOM declarations, proxies, and dispatchers cover the AE interfaces; CCW has real `GetStatus`, `QueryAvailableFilters`, `GetState`, `SetState`, `Refresh`, and `CancelRefresh` paths. | CCW subscription/browser creation and array-heavy AE query methods still return `E_NOTIMPL`. |
| [OPC Batch 2.00](batch-2.00.md) | Batch | 4/4 interfaces and 11/11 methods projected; batch summary/filter codecs and Batch error constants are present. | Server namespace/property semantics remain implementation work for Batch servers. |
| [OPC Common 1.10](common-1.10.md) | Common (locale, shutdown, server-list) | `IOPCCommon`, `IOPCShutdown`, `IOPCServerList(2)`, and `IOPCEnumGUID` are covered by generated or hand-written DCOM projections. | Only public convenience helpers such as a reusable LIKE string-filter utility remain. |
| [OPC CPX 1.00](cpx-1.00.md) | Complex Data | Interface projections, CPX property IDs, CPX HRESULTs, OPCBinary/XMLSchema parsers, XML serializer, OPCBinary encoder/decoder, and namespace helpers are implemented. | DA server runtime integration, type conversion, data-filter execution, and BitString support remain. |
| [OPC DA 2.05a](da-2.05a.md) | DA (V20 back-compat + modern DCOM) | V20 remains a minimal compatibility shim; the modern DCOM surface covers DA 2.05a including `IOPCServer`, `IOPCCommon`, group/item management, sync/async I/O, browsing, properties, callbacks, and connection points. | Remaining caveats are mostly V20-scope and CTT/integration coverage, not missing modern DCOM methods. |
| [OPC DA 3.00](da-3.00.md) | DA (flagship) | DA 3.0 DCOM projections and default hosting helpers cover browse, item I/O, group keep-alive, sync/async VQT and max-age I/O, deadband, sampling, callbacks, and item enumeration. | Default deadband/sampling helpers deliberately report per-handle unsupported/not-set results until a server supplies policy. |
| [OPC DX 1.00](dx-1.00.md) | Data eXchange | `IOPCConfiguration` has a complete hand-written client proxy backed by DX structure codecs, status records, enums, namespace helpers, and error constants. | DX server runtime/DA bridge, persistence, and live data-transfer state machine are not implemented. |
| [OPC HDA 1.20](hda-1.20.md) | Historical Data Access | 56/56 methods and 5/5 codecs are declared; CCW implements core `IOPCHDA_Server` metadata/status/handle methods. | CCW `CreateBrowse` and read bodies are still `E_NOTIMPL` pending browser CCW and OPCHDA item/VARIANT allocation support. |
| [OPC Security 1.00](security-1.00.md) | Security | 6/6 methods across `IOPCSecurityNT` and `IOPCSecurityPrivate` are projected and tested. | Optional server ACL/sample guidance only. |
| [OPC XML-DA 1.01](xmlda-1.01.md) | XML-DA (SOAP transport) | Client supports all 8 operations, SOAP 1.1, scalar/extended scalar values, array values, base64Binary, quality, errors, and polled subscriptions. | XML-DA server hosting and SOAP 1.2 are not implemented. |

## Cross-cutting themes

### Generated and hand-written projections now coexist

Most OPC Classic DCOM interfaces use `[GenerateOpcProxy]` and `[OpcGenerateServerDispatch]`. A few interface-pointer-heavy surfaces still use hand-written proxies or dispatchers, for example Batch enumerators and DX configuration calls with compound structures.

### Cross-platform vs Windows CCW coverage differs

The cross-platform managed DCOM path usually has broader interface coverage than Windows CCW native hosting. DA has the most complete CCW surface; AE and HDA still expose some native vtable slots as `E_NOTIMPL` where returning interface pointers or native arrays requires more CCW infrastructure.

### Runtime semantics are server-specific

Several specs define server behavior beyond wire projection: Batch namespace models, CPX type-conversion/data filters, DX runtime transfer state, HDA aggregate calculations, and XML-DA server hosting. The docs mark those separately from client proxy/dispatcher coverage.

### Error constants and codecs have moved forward

Earlier reviews flagged missing CPX, DX, HDA, Batch, XML-DA array, and DA VARIANT/OPCITEM codecs. Those are now implemented where noted in each document; do not carry forward old “codec-blocked” caveats without checking the source.

## Read order

1. **[da-3.00.md](da-3.00.md)** — flagship spec and broadest runtime surface
2. **[da-2.05a.md](da-2.05a.md)** — modern DA 2.x coverage plus V20 compatibility scope
3. **[hda-1.20.md](hda-1.20.md)** — full DCOM declarations with targeted CCW gaps
4. **[ae-1.10.md](ae-1.10.md)** — DCOM complete, CCW still partial
5. **[common-1.10.md](common-1.10.md)** — shared locale, shutdown, and discovery support
6. **[batch-2.00.md](batch-2.00.md)** — projections complete; server semantics remain
7. **[cpx-1.00.md](cpx-1.00.md)** — codecs/types complete; runtime integration remains
8. **[dx-1.00.md](dx-1.00.md)** — configuration client complete; runtime not implemented
9. **[security-1.00.md](security-1.00.md)** — optional security API coverage
10. **[xmlda-1.01.md](xmlda-1.01.md)** — XML/SOAP client coverage
