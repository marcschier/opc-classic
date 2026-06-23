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

## Exposing real transports (`--listen`)

By default the server is only reachable in-process via `inmemory://`. Pass `--listen` to
also start the real cross-platform **DA, AE, and HDA** listeners (managed `ncacn_ip_tcp`), so
external OPC clients — the Opc.Classic MCP server over `tcp://` / `dcom://`, or (on Windows)
Matrikon OPC Explorer over native DCOM — can connect:

```powershell
dotnet run --project samples\Opc.Classic.Samples.SimulationServer -- --listen
```

On startup it prints the bound endpoints, for example:

```
DA  transport listening: tcp://0.0.0.0:51760 (ProgID Opc.Classic.Simulation.DA.1).
AE  transport listening: tcp://0.0.0.0:51761 (ProgID Opc.Classic.Simulation.AE.1).
HDA transport listening: tcp://0.0.0.0:51762 (ProgID Opc.Classic.Simulation.HDA.1).
```

- Override the DA bind address/port with `OPC_CLASSIC_SIM_DA_LISTEN` (e.g. `0.0.0.0:51300`).
- The listeners serve the **same deterministic plant model** as the in-memory channels (DA via
  the DA 3.0 stateless `IOPCItemIO` read/write surface; AE/HDA via their status + metadata
  root calls).

## Connecting

### a) From the Opc.Classic MCP server

Point an MCP session's connect tool at the simulation using a connection string:

| Transport | `connectionString` | Notes |
| --- | --- | --- |
| In-memory (default) | `inmemory://sim-da` | Same process only; used by the integration tests. |
| Managed TCP | `tcp://<host>:<port>` | The `--listen` endpoint above; cross-platform, no DCOM. |
| DCOM | `dcom://<host>/Opc.Classic.Simulation.DA.1` | Activated over the managed DCOM stack (or native DCOM on Windows). |

Example (managed TCP):

```jsonc
// opcclassic.da.connect
{ "sessionId": "<id>", "connectionString": "tcp://127.0.0.1:51300" }
```

Then `opcclassic.da.browse`, `opcclassic.da.read_sync`, etc. operate against the live
simulated address space over the wire.

### b) From a Windows OPC Classic explorer (Matrikon) — two deployment topologies

A Windows OPC explorer (Matrikon OPC Explorer, the OPC Foundation client, etc.) speaks native
DCOM and discovers servers via OpcEnum. Two topologies are supported:

#### Topology 1 — explorer and simulation both on Windows (native DCOM)

1. **Register** the simulation DA server (writes CLSID/ProgID + component categories + the
   OpcEnum entry; needs an elevated prompt):

   ```powershell
   dotnet run --project samples\Opc.Classic.Samples.SimulationServer -- --register
   ```

2. Let the explorer **activate** it: in **Matrikon OPC Explorer** → *Connect* → browse the local
   OPC server list (OpcEnum) and select **`Opc.Classic.Simulation.DA.1`** (the **AE** and **HDA**
   servers, `Opc.Classic.Simulation.AE.1` / `.HDA.1`, are registered too and appear under the AE /
   HDA categories). Windows SCM launches the sample with `-Embedding`, which runs in
   **OPC-servers-only** mode (no MCP stdio) and registers the DA COM class object (CCW). You can
   also pre-start it explicitly with `-- --opc-only`.

3. **Fully interact**: browse the address space (`Plant.Reactor1.Temperature`, `Random.Real8`,
   `Bucket Brigade.Int4`, …), add items to a group, and watch **live values** update (the server's
   value ticker refreshes the model every 250 ms); sync-read, write to `Bucket Brigade.*`, and
   remove the group — the exact flow `DaLifecycleTransportTests` exercises programmatically.

4. **Unregister** when done: `dotnet run --project … -- --unregister`.

#### Topology 2 — simulation on Linux, explorer on Windows (remote, over TCP)

Run the simulation on Linux exposing the managed `ncacn_ip_tcp` listeners (no Windows needed on
the server side):

```bash
# On the Linux host (open/forward the chosen DA port):
OPC_CLASSIC_SIM_DA_LISTEN=0.0.0.0:51300 \
  dotnet run --project samples/Opc.Classic.Samples.SimulationServer -- --opc-only --listen
```

A managed Opc.Classic client (including **the Opc.Classic MCP server** via
`tcp://<linux-host>:51300` / `dcom://<linux-host>/Opc.Classic.Simulation.DA.1`) connects straight
to that listener cross-platform and gets the full browse/group/read/write/subscribe surface —
this is the path verified end-to-end by `DaLifecycleTransportTests` and the `interop/docker`
native↔managed fleet.

> Note on **native** Windows explorers → Linux: classic Matrikon activation uses the Windows
> RPC runtime against the remote SCM (TCP 135) + the OXID resolver, with NTLM. The managed Linux
> host exposes the ORPC object endpoints used by managed clients today; surfacing the full remote
> **activation** endpoint (so an unmodified native explorer cold-activates a Linux-hosted server)
> is the remaining cross-platform item, tracked in the plan. Until then, reach a Linux-hosted
> simulation from Windows via the Opc.Classic managed client / MCP server over `tcp://`/`dcom://`,
> or run Topology 1 for native-explorer interaction.

> Status: **DA full group lifecycle** (browse, AddGroup, AddItems, live sync read, write with
> persistence, remove) works over the real transport and is covered by `DaLifecycleTransportTests`;
> AE/HDA expose status + metadata over TCP (`TransportSmokeTests`). Windows native hosting
> (`-Embedding`/CCW for DA) + OpcEnum registration for **DA, AE, and HDA** (`--register`) are wired.
> Server-side LRPC, the XML-DA HTTP endpoint, DX-over-DCOM, AE/HDA SCM activation, full AE event /
> HDA history delivery over transport, and native remote activation to a Linux host are incremental
> follow-ups tracked in the project plan.

## Integration testing

`tests/Opc.Classic.Mcp.Integration.Tests` boots the MCP server in-process against a fresh
simulation instance (via the `SimulationMcpHost` fixture) and drives every MCP tool family
end-to-end over the **in-memory** channels. `TransportSmokeTests` additionally connects a
managed OPC DA client to the `SimulationTransportHost` over a real TCP listener. See that
project for usage of `SimulationServerRegistration.RegisterAll`, the
`SimulationServerHandle.ConnectionStrings` map, and `SimulationTransportHost`.
