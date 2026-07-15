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
| **DX** | Engine-backed DA-to-DA transfer runtime plus persistent configuration CRUD |
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

## DX reference runtime

The `dx` endpoint is backed by `DxReferenceEngine`; it is not a disconnected configuration
stub. Its deterministic seed configuration bridges two managed, model-backed DA endpoints:

| Connection | Source item | Target item | Initial state | Rate |
| --- | --- | --- | --- | --- |
| `ReactorTemperatureToBucket` | `Plant.Reactor1.Temperature` | `Bucket Brigade.Real8` | enabled | 1000 ms |
| `ReactorPressureDisabled` | `Plant.Reactor1.Pressure` | `Bucket Brigade.Int4` | disabled | 500 ms |

The same configuration and engine are exposed through:

- MCP `opcclassic.dx.*` tools over `inmemory://...-dx`;
- the OPC DX `IOPCConfiguration` NDR surface (`SimDxClient.Channel`), usable with
  `IOPCConfigurationClientProxy`;
- direct sample APIs for status snapshots, diagnostics, deterministic endpoint failures,
  reconnect/backoff, cancellation, and rate tests.

The reference engine is intentionally bounded. Default limits are 256 source
servers, 1,024 connections, 1,024 queued values per connection, and 1,024
retained diagnostics. Update rates are positive and no greater than one hour;
retry delay uses bounded exponential backoff from one second to one minute.

Set `OPC_CLASSIC_SIM_DX_CONFIG` to a JSON file path to enable atomic, versioned persistence:

```powershell
$env:OPC_CLASSIC_SIM_DX_CONFIG = "$PWD\dx-simulation.json"
dotnet run --project samples\Opc.Classic.Samples.SimulationServer
```

On first start the file is seeded. Later starts recover the committed revision and resume
enabled transfers. Configuration add/modify/update/delete/reset operations all mutate the
same engine state; no MCP- or DCOM-specific transfer implementation is duplicated.

### Reference-grade boundary

The scenario demonstrates deterministic polling-based DA source reads, VQT
propagation, target writes, enabled/disabled state, health checks,
reconnect/backoff, cancellation, versioned configuration, and restart recovery.
It is not a complete generic OPC DX server: the standardized DA-visible DX
Database subtree, DirtyFlag/`E_PERSISTING` timing policy, XML-DA sources,
subscription-driven queue rules, conversion policy, and the full section 6
target-write truth table remain outside the sample.

## Exposing real transports (`--listen`)

By default the server is only reachable in-process via `inmemory://`. Pass `--listen` to
also start the real cross-platform **DA, AE, and HDA** listeners (managed `ncacn_ip_tcp`), so
managed Opc.Classic clients — including the Opc.Classic MCP server over `tcp://` / `dcom://`
— can connect. Native Windows DCOM interaction is covered in the topologies below:

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

4. **Unregister** when done: `dotnet run --project samples\Opc.Classic.Samples.SimulationServer -- --unregister`.

#### Topology 2 — simulation on Linux, Matrikon on Windows (authenticated DCOM)

Server-side NTLM authenticated cold-activation is implemented for the managed DCOM simulation host. Run the Linux server with credentials that Matrikon will present:

```bash
export OPC_CLASSIC_DCOM_USER=opcuser
export OPC_CLASSIC_DCOM_PASSWORD='change-me'
export OPC_CLASSIC_DCOM_DOMAIN=OPC
export OPC_CLASSIC_SIM_DA_LISTEN=0.0.0.0:51300
# For TCP 135 either run as root or grant the published binary:
# sudo setcap cap_net_bind_service=+ep ./Opc.Classic.Samples.SimulationServer

dotnet run --project samples/Opc.Classic.Samples.SimulationServer -- --opc-only --listen
```

Expose/forward TCP 135 and the activation/object listener port printed by the server. In Matrikon OPC Explorer, add the Linux host as a remote OPC host, supply the same domain/user/password, browse the remote OpcEnum list, and activate `Opc.Classic.Simulation.DA.1`, `Opc.Classic.Simulation.AE.1`, or `Opc.Classic.Simulation.HDA.1`. DA supports browse, AddGroup, AddItems, sync read/write, live `OnDataChange` subscriptions, Unadvise, and RemoveGroup. AE subscriptions deliver simulated reactor events; HDA `ReadRaw` returns the seeded deterministic history.

The in-sandbox acceptance proxy is `ManagedDcomFullStackE2ETests`, which stands up EPM on an override port plus the authenticated activation/OXID/OpcEnum/DA/AE/HDA host and drives the native-style sequence with managed primitives. The Windows-container native scaffold under `interop/docker/native-matrikon-proxy` is wired for CI review and Windows-runner execution.
## Integration testing

`tests/Opc.Classic.Mcp.Integration.Tests` boots the MCP server in-process against a fresh
simulation instance (via the `SimulationMcpHost` fixture) and drives every MCP tool family
end-to-end over the **in-memory** channels. `TransportSmokeTests` additionally connects a
managed OPC DA client to the `SimulationTransportHost` over a real TCP listener. See that
project for usage of `SimulationServerRegistration.RegisterAll`, the
`SimulationServerHandle.ConnectionStrings` map, and `SimulationTransportHost`.
`SimDxReferenceIntegrationTests` covers DX JSON restart recovery, live DA read/write
transfer, enabled/disabled state, revised rates, quality/error propagation, endpoint
failure and reconnect/backoff, cancellation/reset, and DCOM/MCP configuration round trips.
