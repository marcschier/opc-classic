# OPC Classic generator diagnostics

The `Opc.Classic.Generators` package reports these diagnostics while validating `[OpcInterface]`, `[OpcMethod]`, and `[GenerateOpcProxy]` declarations.

| ID | Severity | Meaning |
|---|---|---|
| OPCGEN001 | Error | `[OpcInterface]` contains an invalid GUID literal. |
| OPCGEN002 | Error | `[OpcInterface]` is applied to a non-partial interface. |
| OPCGEN003 | Error | Two `[OpcMethod]` members on one interface share an opnum. |
| OPCGEN004 | Error | `[GenerateOpcProxy]` is applied to a non-partial interface. |
| OPCGEN005 | Warning | `[GenerateOpcProxy]` is missing the companion `[OpcInterface]` attribute. |
| OPCGEN006 | Warning | A `ref`/`out` parameter type has no registered request/response codec. |
| OPCGEN007 | Warning | An `[OpcMethod]` signature is unsupported, such as a generic method or non-`Task` return. |
| OPCGEN008 | Info | A method falls back to an empty-payload placeholder because a type is unsupported. |
| OPCGEN009 | Warning | A method return type has no registered codec in the generator codec registry. |
| OPCGEN010 | Warning | A method parameter type is unsupported by the generator codec registry. |
