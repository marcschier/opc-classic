# OCMGEN001 — OpcRcw namespace usage

`OCMGEN001` reports `using OpcRcw.*` directives and qualified `OpcRcw.Da`, `OpcRcw.Hda`, `OpcRcw.Ae`, or `OpcRcw.Comn` references. These assemblies expose raw COM interop surfaces; `Opc.Classic.*` provides cross-platform managed APIs.

## Before

```csharp
using OpcRcw.Da;
```

## After

```csharp
using Opc.Classic.Da;
```

Add the appropriate `Opc.Classic.*` NuGet package to the consuming project, then resolve type-level API differences with the DA, AE, HDA, and Core migration diagnostics.
