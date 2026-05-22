; Unshipped analyzer release.
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID  | Category                  | Severity | Notes
---------|---------------------------|----------|------------------------------------------------------------------
OPCGEN001| OpcClassic.Generators     | Error    | OpcInterface attribute carries an invalid GUID.
OPCGEN002| OpcClassic.Generators     | Error    | OpcInterface target must be partial.
OPCGEN003| OpcClassic.Generators     | Error    | Duplicate OpcMethod opnum on the same interface.
OPCGEN004| OpcClassic.Generators     | Error    | OpcProxy target must be partial.
OPCGEN005| OpcClassic.Generators     | Warning  | OpcProxy target should be decorated with [OpcInterface].
OPCGEN006| OpcClassic.Generators     | Warning  | OpcMethod with ref/out parameter cannot have generated body; falling back to NotImplementedException.
