; Unshipped analyzer release.
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID  | Category                  | Severity | Notes
---------|---------------------------|----------|------------------------------------------------------------------
OCMDA001 | Opc.Classic.Migration     | Info     | Flags legacy OPC DA server construction.
OCMDA002 | Opc.Classic.Migration     | Info     | Flags legacy OPC DA synchronous browse calls.
OCMDA003 | Opc.Classic.Migration     | Info     | Flags legacy OPC DA synchronous read calls.
OCMAE001 | Opc.Classic.Migration     | Info     | Flags legacy OPC AE callback subscription patterns.
OCMHDA001| Opc.Classic.Migration     | Info     | Flags legacy OPC HDA SyncReadRaw calls.
OCMGEN001| Opc.Classic.Migration     | Info     | Flags OpcRcw namespace usage.
OCMGEN002| Opc.Classic.Migration     | Info     | Flags manual VARIANT conversion patterns.
