# Opc.Classic.Samples.DaServer

Managed OPC DA sample server that mirrors the well-known Matrikon Simulation-style tag tree and the preserved `COM\Sample Server\Da\` reference.

## Tags

- `Random.{Real4,Real8,Int1,Int2,Int4,UInt1,UInt2,UInt4,Boolean,String}`: read-only random values refreshed on each read.
- `Bucket Brigade.{Real4,Real8,Int1,Int2,Int4,UInt1,UInt2,UInt4,Boolean,String}`: read/write tags that return the last successfully written value.
- `Saw-toothed Waves.{Real4,Real8}`: periodic ramp values.
- `Square Waves.{Real4,Real8}`: periodic low/high values.
- `Triangle Waves.{Real4,Real8}`: periodic triangle values.
- `Read Error.{Int1,Int2,Int4}`: throw `OPC_E_BADRIGHTS` on read.
- `Write Error.Int1`: throws `OPC_E_BADRIGHTS` on write.

## Run

```powershell
dotnet run --project .\samples\Opc.Classic.Samples.DaServer\Opc.Classic.Samples.DaServer.csproj
```

The sample registers as ProgID `Opc.Classic.Samples.DaServer.1` with CLSID `B3AE5D6F-2A91-4F8B-9D2C-7E5B0C8F1A3E`. It is intentionally buildable by direct project path and is not listed in `Opc.Classic.slnx` yet.
