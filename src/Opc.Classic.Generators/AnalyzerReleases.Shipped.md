; Shipped analyzer releases.
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 0.1.0

### New Rules

Rule ID  | Category                  | Severity | Notes
---------|---------------------------|----------|------------------------------------------------------------------
OPCGEN001| Opc.Classic.Generators     | Error    | OpcInterface attribute carries an invalid GUID.
OPCGEN002| Opc.Classic.Generators     | Error    | OpcInterface target must be partial.
OPCGEN003| Opc.Classic.Generators     | Error    | Duplicate OpcMethod opnum on the same interface.
OPCGEN004| Opc.Classic.Generators     | Error    | OpcProxy target must be partial.
OPCGEN005| Opc.Classic.Generators     | Warning  | OpcProxy target should be decorated with [OpcInterface].
OPCGEN006| Opc.Classic.Generators     | Warning  | OpcMethod has unsupported ref/out parameter type without a registered codec.
OPCGEN007| Opc.Classic.Generators     | Warning  | OpcMethod has an unsupported method signature.
OPCGEN008| Opc.Classic.Generators     | Info     | OpcMethod has unsupported parameter/return type; emitting empty-payload placeholder body.
OPCGEN009| Opc.Classic.Generators     | Warning  | OpcMethod return type is missing a registered codec.
OPCGEN010| Opc.Classic.Generators     | Warning  | OpcMethod has an unsupported parameter type.
