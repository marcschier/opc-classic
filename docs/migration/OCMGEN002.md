# OCMGEN002 — Manual VARIANT conversion

`OCMGEN002` reports manual `VariantValue` construction and native `Marshal.GetVariant*` conversion patterns. `OpcVariant`, `OpcVariantConverter`, and the typed `OpcVariant.From*` factories centralize OPC VARIANT handling while keeping applications away from platform-specific COM marshaling helpers.

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
var value = OpcVariantConverter.FromObject(rawValue);
```

```csharp
object? managed = ReadManagedValueWithoutNativeVariant(nativeValueSource);
var value = OpcVariantConverter.FromObject(managed);
```

The code fix is only a mechanical starting point. Prefer `OpcVariantConverter.FromObject(...)` for managed values, or specific factories such as `OpcVariant.FromInt32(...)` and `OpcVariant.FromSafeArray(...)`; remove native VARIANT pointer handling at the application boundary.
