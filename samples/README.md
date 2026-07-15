# Opc.Classic samples

This folder contains runnable .NET 10 sample projects demonstrating the `Opc.Classic.*` stack: managed DA/AE/HDA servers and clients, OPC Complex Data, OPC Security, CTT-oriented DA coverage, an in-process loopback, a full-feature simulation server, and the NativeAOT canary.

Most sample folders ship their own `README.md` with run instructions. DA/AE/HDA/CttServer/Security sample servers bind `OPC_CLASSIC_SAMPLE_PORT` on `0.0.0.0` by default, and the DA/AE/HDA sample clients dial TCP when `OPC_CLASSIC_SERVER_HOST` + `OPC_CLASSIC_SERVER_PORT` are set (otherwise they fall back to the in-process channel for local dev).

## Sample map

| Folder | What it demonstrates |
| --- | --- |
| [Opc.Classic.Samples.DaServer/](Opc.Classic.Samples.DaServer/README.md) | Managed DA server with browse, reads, writes, and data-change publishing. |
| [Opc.Classic.Samples.OpcSecurityServer/](Opc.Classic.Samples.OpcSecurityServer/README.md) | Managed DA reference server exposing OPC Security 1.00 `IOPCSecurityNT` and `IOPCSecurityPrivate`. |
| [Opc.Classic.Samples.AeServer/](Opc.Classic.Samples.AeServer/README.md) | Managed AE server with areas, sources, conditions, and event metadata. |
| [Opc.Classic.Samples.HdaServer/](Opc.Classic.Samples.HdaServer/README.md) | Managed HDA server with historical values, aggregates, and annotations. |
| [Opc.Classic.Samples.DaClient/](Opc.Classic.Samples.DaClient/README.md) | DA client bootstrap: browse, read, write, subscribe. |
| [Opc.Classic.Samples.AeClient/](Opc.Classic.Samples.AeClient/README.md) | AE client subscription + event acknowledgement. |
| [Opc.Classic.Samples.HdaClient/](Opc.Classic.Samples.HdaClient/README.md) | HDA client reads, aggregates, annotations, updates. |
| [Opc.Classic.Samples.CpxServer/](Opc.Classic.Samples.CpxServer/README.md) | Managed DA-backed CPX address space with deterministic OPCBinary, XMLSchema, invalid, and vendor items. |
| [Opc.Classic.Samples.CpxClient/](Opc.Classic.Samples.CpxClient/README.md) | CPX browse/property discovery, OPCBinary/XML decode, bounded conversion/filter behavior. |
| [Opc.Classic.Samples.LoopbackDemo/](Opc.Classic.Samples.LoopbackDemo/README.md) | In-process DA client/server loopback through the managed channel stack. |
| [Opc.Classic.Samples.CttServer/](Opc.Classic.Samples.CttServer/README.md) | Additional managed DA sample registered as `Opc.Classic.DaSample.1` (different CLSID from `samples-da`). |
| [Opc.Classic.Samples.SimulationServer/](Opc.Classic.Samples.SimulationServer/README.md) | Full-feature simulation server: one deterministic plant model projected through every OPC Classic spec, including an engine-backed, persistable DX DA-to-DA bridge, with default in-memory MCP hosting and optional DA/AE/HDA TCP listeners. |
| [Opc.Classic.Samples.AotCanary/](Opc.Classic.Samples.AotCanary/) | NativeAOT publish verification used in CI. |

## Sample container deployment

The DA/AE/HDA sample server/client pairs are designed to interop over DCOM-over-IP between containers. `OpcSecurityServer` uses the same server env-var contract at port 51304 and can be run from source today. See [README.docker.md](README.docker.md) for the multi-service Compose deployment and the NoOpAuthContext caveat for the demo path.

## Environment variables

| Variable | Default | Used by |
| --- | --- | --- |
| `OPC_CLASSIC_SAMPLE_PORT` | DA=51300, AE=51301, HDA=51302, CttServer=51303, Security=51304 | Sample servers — port to bind on `0.0.0.0`. |
| `OPC_CLASSIC_LISTEN_ADDRESS` | (unset) | Optional explicit `host:port` bind for sample servers (overrides the sample-port default). |
| `OPC_CLASSIC_SERVER_HOST` | (unset) | Sample clients — when set with `OPC_CLASSIC_SERVER_PORT`, dial TCP. |
| `OPC_CLASSIC_SERVER_PORT` | (unset) | Sample clients — TCP port of the remote sample server. |
| `OPC_CLASSIC_SIM_DX_CONFIG` | (unset) | Optional SimulationServer DX JSON configuration path; enables atomic restart recovery. |

When the client env vars are unset, sample clients fall back to an in-process `InMemoryCallChannel` + `Loopback*Server` for local `dotnet run`. Each client logs which path is active at startup.
