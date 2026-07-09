# Opc.Classic BenchmarkDotNet suite

This project contains standalone BenchmarkDotNet benchmarks for OPC Classic hot paths. It is a console app, not a test project.

## Running all benchmarks

```powershell
dotnet run -c Release --project benchmarks/Opc.Classic.Benchmarks -- --filter "*"
```

## Running a specific benchmark class

```powershell
dotnet run -c Release --project benchmarks/Opc.Classic.Benchmarks -- --filter "*NdrWriter*"
```

Current benchmark classes:

- `CodecRegistryBenchmarks`
- `DcomCallChannelBenchmarks`
- `NdrReaderBenchmarks`
- `NdrWriterBenchmarks`
- `OpcSafeArrayBenchmarks`
- `OpcVariantBenchmarks`

## Results

BenchmarkDotNet writes reports, logs, and exported data under `BenchmarkDotNet.Artifacts/` in the working directory. Use the generated markdown, CSV, HTML, and JSON exports for comparisons.

## Running in CI

CI should keep runs explicit and export machine-readable output, for example:

```powershell
dotnet run -c Release --project benchmarks/Opc.Classic.Benchmarks -- --runtimes net10.0 --exporters json
```

## Known baselines

Reserved for first-run results on canonical hardware.
