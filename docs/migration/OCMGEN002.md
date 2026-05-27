# OCMGEN002 — Manual VARIANT conversion

`OCMGEN002` reports manual `VariantValue` construction and native `Marshal.GetVariant*` conversion patterns. `Opc.Classic.Core.OpcVariant` centralizes OPC VARIANT handling and keeps applications away from platform-specific COM marshaling helpers.

Default severity: **Info**.

## Before

```csharp
var value = new VariantValue(rawValue);
```

```csharp
var managed = Marshal.GetObjectForNativeVariant(nativeVariant);
```

## After

```csharp
var value = OpcVariant.FromObject(rawValue);
```

```csharp
var managed = OpcVariant.FromNativeVariant(nativeVariant);
```

The code fix maps `Marshal.GetObjectForNativeVariant(...)` to `OpcVariant.FromNativeVariant(...)`. Other manual wrappers and native conversion calls are replaced with `OpcVariant.FromObject(...)` as a safe starting point; review the resulting factory if your value has a more specific type or timestamp/quality semantics.
