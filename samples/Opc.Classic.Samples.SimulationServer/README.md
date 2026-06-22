# Opc.Classic Full-Feature Simulation Server

A single, in-process **OPC Classic simulation server** that simulates every OPC Classic
feature area over one deterministic plant model:

| Area | What it simulates |
| --- | --- |
| **DA** | Browse, item management, sync/async IO, subscriptions (OnDataChange), item properties |
| **AE** | Area/source space, conditions derived from analog alarm limits, acknowledge, event subscriptions |
| **HDA** | Raw/processed/at-time/modified reads, aggregates, insert/replace/delete, annotations |
| **Batch** | Batch summaries, filtering, enumeration sets |
| **Commands** | Command browse, sync/async invoke, state transitions, control/cancel |
| **Complex Data (Cpx)** | Complex type system + type dictionaries over the DA channel |
| **DX** | Source servers and DX connections (query/add/modify/update/delete/reset) |
| **Security** | NT and private authentication availability, logon/logoff |
| **Discovery** | OpcEnum enumeration of the simulated servers across DA/AE/HDA categories |
| **XML-DA** | Status/browse/read/write/subscribe/polled-refresh/get-properties over the same address space |

Everything is **deterministic** (seeded signal generators, fixed epoch
`2026-01-01T00:00:00Z`) so it is ideal for repeatable integration tests.

## Architecture

- `SimulatedPlantModel` — the single source of truth: a rich address space
  (`Random.*`, `Signals.*`, writable `Bucket Brigade.*`, analog `Plant.Reactor1/2.*`
  with alarm limits) plus deterministic value, history, and write-override semantics.
- `ISimulationModule` — one per feature area. Each builds the spec's server (wrapping the
  source-generated server dispatchers in an in-memory call channel) and registers an
  `inmemory://` endpoint with the MCP connection registries, or contributes a DI service
  (Security, Discovery).
- `SimulationServerRegistration.RegisterAll(...)` — assembles all ten modules over one
  model and returns a `SimulationServerHandle` exposing the per-spec connection strings and
  the MCP-host DI contributions.

## Running

```powershell
dotnet run --project samples\Opc.Classic.Samples.SimulationServer
```

This hosts an MCP server over stdio with the full simulation pre-wired. On startup it prints
(to stderr) the `inmemory://` connection string for each feature area, for example:

```
da        -> inmemory://sim-da
ae        -> inmemory://sim-ae
hda       -> inmemory://sim-hda
batch     -> inmemory://sim-batch
commands  -> inmemory://sim-commands
cpx       -> inmemory://sim-cpx
dx        -> inmemory://sim-dx
xmlda     -> inmemory://sim-xmlda
```

Connect an MCP session with the matching connect tool, e.g.
`opcclassic.da.connect` with `connectionString = inmemory://sim-da`. Security and Discovery
are resolved from the host's DI (Discovery answers for host `sim-host`); no connection
string is needed for those two.

## Integration testing

`tests/Opc.Classic.Mcp.Integration.Tests` boots the MCP server in-process against a fresh
simulation instance (via the `SimulationMcpHost` fixture) and drives every MCP tool family
end-to-end. See that project for usage of `SimulationServerRegistration.RegisterAll` and the
`SimulationServerHandle.ConnectionStrings` map.
