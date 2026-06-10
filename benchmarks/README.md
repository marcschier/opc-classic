# Opc.Classic benchmarks

This folder hosts the BenchmarkDotNet performance harness for the
managed OPC Classic stack. The single project — `Opc.Classic.Benchmarks`
— is a console app (not a TUnit test project), so it runs via
`dotnet run -c Release` and writes BenchmarkDotNet artifacts under
the working directory.

## Why these specific benchmarks

Each benchmark class targets a hot path that shows up under load when
real OPC clients hammer the managed stack — typically a wire codec, a
COM marshaling primitive, or the call-channel dispatch loop. The
benchmarks are intentionally narrow and self-contained so regressions
can be attributed to a single code change.

| Class | Hot path | Why it matters |
| --- | --- | --- |
| [`CodecRegistryBenchmarks`](Opc.Classic.Benchmarks/Benchmarks/CodecRegistryBenchmarks.cs) | `OpcCodecRegistry` primitive + struct codec lookup (cold vs warm cache) | Every NDR encode/decode resolves a codec via `OpcCodecRegistry.GetCodec<T>()`; a regression in the cache fast-path slows down EVERY OPC call. |
| [`NdrReaderBenchmarks`](Opc.Classic.Benchmarks/Benchmarks/NdrReaderBenchmarks.cs) | `NdrReader.Read{UInt32,String,ByteArray,Double,FileTime}` vs naive implementations | The reader runs on every inbound response; UInt32 + String dominate (header fields + payload strings). The "Naive" variants document the speedup vs an unoptimized reference impl. |
| [`NdrWriterBenchmarks`](Opc.Classic.Benchmarks/Benchmarks/NdrWriterBenchmarks.cs) | `NdrWriter.Write{UInt32,String,ByteArray,Double,FileTime}` vs naive implementations | The writer runs on every outbound request; same UInt32 + String dominance as the reader. |
| [`OpcVariantBenchmarks`](Opc.Classic.Benchmarks/Benchmarks/OpcVariantBenchmarks.cs) | `OpcVariant` wire-format encode / decode / round-trip | Every DA item read/write goes through `OpcVariant`; the type-dispatch table is on the critical path of `IOPCSyncIO::Read`/`Write`. |
| [`OpcSafeArrayBenchmarks`](Opc.Classic.Benchmarks/Benchmarks/OpcSafeArrayBenchmarks.cs) | `OpcSafeArray` wire-format encode / decode / round-trip | Multi-item batches (e.g., 1000-item reads) marshal via `OpcSafeArray`; element-loop efficiency directly drives subscription throughput. |
| [`DcomCallChannelBenchmarks`](Opc.Classic.Benchmarks/Benchmarks/DcomCallChannelBenchmarks.cs) | Full `DcomCallChannel.InvokeAsync` round-trip through an in-memory loopback channel (`IOPCServer::GetStatus`) | End-to-end allocations per RPC call. This is the integration-level number that surfaces regressions in the bind/alter/request PDU codec path or in the loopback transport's allocator. |

## Running

From the repo root:

```powershell
# All benchmark classes
dotnet run -c Release --project benchmarks/Opc.Classic.Benchmarks -- --filter "*"

# A specific class
dotnet run -c Release --project benchmarks/Opc.Classic.Benchmarks -- --filter "*NdrWriter*"

# A specific method within a class
dotnet run -c Release --project benchmarks/Opc.Classic.Benchmarks -- --filter "*NdrWriter*.WriteString"

# CI-friendly: pin runtime + emit machine-readable results
dotnet run -c Release --project benchmarks/Opc.Classic.Benchmarks -- --runtimes net10.0 --exporters json
```

BenchmarkDotNet drops reports, logs, and exported data under
`BenchmarkDotNet.Artifacts/` in the working directory. The generated
markdown is the most useful for side-by-side comparison; the JSON
export is what CI consumes for regression-checking against historical
baselines.

## Project anatomy

```
benchmarks/
├── README.md                              <- you are here
└── Opc.Classic.Benchmarks/
    ├── Opc.Classic.Benchmarks.csproj      Net 10 console app; BenchmarkDotNet + project refs to Core/Dcom/Generators
    ├── Program.cs                         BenchmarkSwitcher.FromAssembly(...).Run(args) entry point
    ├── README.md                          Per-project quick-reference (also re-linked from this README)
    └── Benchmarks/
        ├── CodecRegistryBenchmarks.cs
        ├── DcomCallChannelBenchmarks.cs
        ├── NdrReaderBenchmarks.cs
        ├── NdrWriterBenchmarks.cs
        ├── OpcSafeArrayBenchmarks.cs
        └── OpcVariantBenchmarks.cs
```

## Adding a new benchmark

1. Add a new `.cs` file under `Opc.Classic.Benchmarks/Benchmarks/`.
2. Decorate the class with `[MemoryDiagnoser]` (default) and methods with
   `[Benchmark]` (or `[Benchmark(Baseline = true)]` for the
   comparison anchor).
3. Keep each class focused on ONE hot path. If you want
   "before vs after a refactor" comparisons, use `[Arguments]` or the
   `BaselineColumn` rather than splitting across classes.
4. Update the table above with a one-liner explaining why the new hot
   path matters.

## Known limitations

- **Process isolation overhead**: each benchmark class runs in a
  fresh process by default (BenchmarkDotNet's `InProcessNoEmitToolchain`
  is opt-in). For very fast benchmarks (sub-microsecond), prefer
  filtering to a single class to amortize the startup cost.
- **No baseline JSON committed**: there's no historical
  `BenchmarkDotNet.Artifacts/` snapshot in-repo yet -- runs are
  reproducible but absolute numbers depend on the canonical hardware
  used by whoever is comparing. Establishing a baseline is a TODO.
- **No CI gate yet**: benchmarks are not wired into
  `.github/workflows/build.yml`. Running them in CI is
  cost-prohibitive for a per-push gate; the right shape is probably a
  `workflow_dispatch` scheduled job (analogous to
  `.github/workflows/fuzz-deep.yml`).
