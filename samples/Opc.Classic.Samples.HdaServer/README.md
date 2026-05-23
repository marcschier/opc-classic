# Opc.Classic Sample HDA Server

Managed in-process OPC HDA sample server mirroring the native
`COM/Sample Server/Hda` historian shape with a small synthetic tag tree.

## Tags

- `Sensor.Temperature` - sinusoidal temperature, seeded for one day at 1-second intervals.
- `Sensor.Pressure` - cosine pressure, seeded for one day at 1-second intervals.
- `Sensor.FlowRate` - faster sinusoidal flow rate, seeded for one day at 1-second intervals.

## Run

```bash
dotnet run --project samples/Opc.Classic.Samples.HdaServer
```

The server registers as `Opc.Classic.Samples.HdaServer.1` (CLSID
`A2BBEA4E-F1C6-469B-8D71-89767DCD2D48`) and listens on `127.0.0.1:0`.

## Raw read example

```csharp
OpcHdaItem[] items = await server.ReadRawAsync(
    ["Sensor.Temperature", "Sensor.Pressure"],
    OpcHdaTime.FromString("NOW-5M"),
    OpcHdaTime.FromString("NOW"),
    maxValues: 100,
    ct);
```

## Processed read example

```csharp
OpcHdaItem[] averages = await server.ReadProcessedAsync(
    ["Sensor.FlowRate"],
    OpcHdaTime.FromString("NOW-1H"),
    OpcHdaTime.FromString("NOW"),
    TimeSpan.FromMinutes(5),
    HdaAggregate.Average,
    ct);
```
