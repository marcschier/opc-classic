# Opc.Classic Sample HDA Server

Managed OPC HDA sample server mirroring the native `COM\Sample Server\Hda` historian shape with a small synthetic tag tree.

## Tags

- `Sensor.Temperature` - sinusoidal temperature, seeded for one day at 1-second intervals.
- `Sensor.Pressure` - cosine pressure, seeded for one day at 1-second intervals.
- `Sensor.FlowRate` - faster sinusoidal flow rate, seeded for one day at 1-second intervals.

## Run

```powershell
dotnet run --project samples\Opc.Classic.Samples.HdaServer
```

The server registers as `Opc.Classic.Samples.HdaServer.1` (CLSID `A2BBEA4E-F1C6-469B-8D71-89767DCD2D48`) and listens on `0.0.0.0:51302` by default.

Set `OPC_CLASSIC_SAMPLE_PORT` to change the default port or `OPC_CLASSIC_LISTEN_ADDRESS` to override the full bind address.

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

## Source files

- `Program.cs` — host setup, registry options, and listen-address selection.
- `SampleHdaServer.cs` — managed HDA server status, validation, raw read, and processed read implementation.
- `HistoricalDataStore.cs` — seeded one-day historian data set.

For the compose-orchestrated container demo, see `samples\README.docker.md`.
