; Unshipped analyzer release.
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID  | Category                  | Severity | Notes
---------|---------------------------|---|---
OPCGEN101| Opc.Classic.Generators     | Error    | Opc server dispatcher target must be partial.
OPCGEN102| Opc.Classic.Generators     | Warning  | Opc server dispatcher target should be decorated with [OpcInterface].
OPCGEN103| Opc.Classic.Generators     | Warning  | Opc server method has an unsupported signature.
OPCGEN104| Opc.Classic.Generators     | Warning  | Opc server method has an unsupported parameter type.
OPCGEN105| Opc.Classic.Generators     | Warning  | Opc server method has an unsupported response type.
