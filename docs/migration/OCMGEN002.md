# OCMGEN002 — Manual VARIANT conversion

`OCMGEN002` reports manual `VariantValue` construction and `Marshal.GetVariant*` conversion patterns. `Opc.Classic.Core.OpcVariant` centralizes OPC VARIANT handling and keeps applications away from platform-specific COM marshaling helpers.

## Before

```csharp
var value = new VariantValue(rawValue);
```

## After

```csharp
var value = OpcVariant.FromObject(rawValue);
```

For native buffers, use the closest `OpcVariant.FromXxx(...)` factory provided by `Opc.Classic.Core` and remove direct `Marshal.GetObjectForNativeVariant` or `Marshal.GetNativeVariantForObject` calls.
