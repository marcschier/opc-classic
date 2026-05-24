# OPC Classic generator diagnostics

The `Opc.Classic.Generators` package reports these diagnostics while validating `[OpcInterface]`, `[OpcMethod]`, `[GenerateOpcProxy]`, and `[OpcGenerateServerDispatch]` declarations. The diagnostic category is `Opc.Classic.Generators`.

## Interface contract diagnostics

| ID | Severity | Meaning |
|---|---|---|
| OPCGEN001 | Error | `[OpcInterface]` contains an invalid GUID literal. |
| OPCGEN002 | Error | `[OpcInterface]` is applied to an interface that is not `partial`. |
| OPCGEN003 | Error | Two `[OpcMethod]` members on one interface share an opnum. |

## Client proxy diagnostics

| ID | Severity | Meaning |
|---|---|---|
| OPCGEN004 | Error | `[GenerateOpcProxy]` is applied to an interface that is not `partial`. |
| OPCGEN005 | Warning | `[GenerateOpcProxy]` is missing the companion `[OpcInterface]` attribute. |
| OPCGEN006 | Warning | A `ref` or `out` parameter type has no registered request/response codec. |
| OPCGEN007 | Warning | An `[OpcMethod]` signature is unsupported, such as a generic method or non-`Task` return. |
| OPCGEN008 | Info | A method uses an unsupported parameter or return type, so generation falls back to an empty-payload body. |
| OPCGEN009 | Warning | A method return type has no registered codec in the generator codec registry. |
| OPCGEN010 | Warning | A method parameter type is unsupported by the generator codec registry. |

## Server dispatch diagnostics

| ID | Severity | Meaning |
|---|---|---|
| OPCGEN101 | Error | `[OpcGenerateServerDispatch]` is applied to an interface that is not `partial`. |
| OPCGEN102 | Warning | `[OpcGenerateServerDispatch]` is missing the companion `[OpcInterface]` attribute. |
| OPCGEN103 | Warning | An `[OpcMethod]` signature is unsupported for server dispatch generation. |
| OPCGEN104 | Warning | A server dispatch method parameter type is unsupported by the codec registry. |
| OPCGEN105 | Warning | A server dispatch method response type is unsupported by the codec registry. |

## Severity policy

Errors stop the affected generator output. Warnings keep compilation visible while omitting or limiting generated code for the affected method. Info diagnostics describe a generated fallback that should be reviewed before relying on the method in production.
