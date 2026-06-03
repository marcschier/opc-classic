# OPC DA `IOPCDataCallback` push delivery (Track AP)

`opcclassic.da.subscribe` creates poll-style subscriptions. The
[`IOPCDataCallback`](../../src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs)
interface (IID `39C13A70-011E-11D0-9675-0020AFD8ADB3`) gives a real OPC DA
server a way to push `OnDataChange`, `OnReadComplete`, `OnWriteComplete`,
and `OnCancelComplete` notifications back to the client. This document
describes how that path is wired in the MCP server today, what is still
deferred, and what the production callback-bind path requires.

## Architecture

```
                ┌──────────────────────────────────────────────┐
                │  MCP host process                            │
                │  ┌────────────────────────────────────────┐  │
   Matrikon     │  │  OpcServerListener (callback path)     │  │
   pushes ────▶ │  │  bound to a TCP port on a routable IP  │  │
   ORPC         │  └─────────────┬──────────────────────────┘  │
                │                ▼                             │
                │  ┌────────────────────────────────────────┐  │
                │  │  RpcServerConnectionProcessor          │  │
                │  │   (existing — proven by                │  │
                │  │    OutboundCallbackOverTransportTests) │  │
                │  └─────────────┬──────────────────────────┘  │
                │                ▼                             │
                │  ┌────────────────────────────────────────┐  │
                │  │  IOPCDataCallbackServerDispatcher      │  │
                │  │   (auto-generated from                 │  │
                │  │    [OpcGenerateServerDispatch])        │  │
                │  └─────────────┬──────────────────────────┘  │
                │                ▼                             │
                │  ┌────────────────────────────────────────┐  │
                │  │  DaDataCallbackSink                    │  │
                │  │   (this file: bounded Channel<T> +     │  │
                │  │    OnDataChange/OnReadComplete enqueue)│  │
                │  └─────────────┬──────────────────────────┘  │
                │                ▼                             │
                │  DaSubscriptionContext.Sink.DrainItems(N)   │
                │                ▼                             │
                │  opcclassic.da.poll_subscription returns    │
                │  push notifications, falling back to a       │
                │  synchronous pull when the queue is empty.   │
                └──────────────────────────────────────────────┘
```

## Status

| Subtrack | Description | Status |
|---------|-------------|--------|
| AP1 | `OpcServerListener` startup on MCP host bound to dynamic TCP port + IOPCDataCallback dispatcher registration | **Deferred** — requires live Matrikon verification |
| AP2 | Construct `IOpcInterfaceRef` for sink (TCP string binding + fresh IPID/OXID/OID) + pass to `IConnectionPointClientProxy.Advise(sink)` + track cookie | **Deferred** — requires live Matrikon verification |
| AP3 | `DataChangeNotification` queue + bounded `Channel<T>` sink + `poll_subscription` drain-first-then-pull | **Done** |
| AP4 | Accept Matrikon callback-bind PDU auth via existing `RpcServerConnectionProcessor` + `Spnego` | **Mostly done** — the loopback test proves the auth path; production OBJREF routing still needs AP1+AP2 |
| AP5 | Synthetic in-process test of sink + queue + drain mapping | **Done** (`tests/Opc.Classic.Mcp.Tests/DaDataCallbackSinkTests.cs`) |
| AP6 | Documentation | **This document** |

`poll_subscription` already prefers push notifications when the sink has
data and falls back to a synchronous `IOPCSyncIO::Read` when the queue is
empty. Until AP1/AP2 land, no production code feeds the sink, so the
fallback path matches existing pull behavior bit-for-bit.

## Sink contract

The sink ([`DaDataCallbackSink`](../../mcp/Opc.Classic.Mcp/Tools/DaDataCallbackSink.cs))
is bounded with drop-oldest semantics so a stalled MCP client cannot
unbound the queue. The default capacity is **1024 notification batches**
(`DataChangeNotification.DefaultCapacity`).

`poll_subscription` resolves each `DataChangeItem.ClientHandle` back to
its `DaItemBindingContext` via a one-shot reverse index over
`DaGroupContext.Items`. This matches the OPC DA wire contract — the
server delivers values keyed by **client handle**, not server handle.

`OnWriteCompleteAsync` and `OnCancelCompleteAsync` are accepted (counters
increment) but not enqueued because `poll_subscription` does not surface
async-IO completion to MCP today. A future MCP tool may drain them
separately.

`DrainItems(maxItems)`:

- `maxItems <= 0` drains everything currently queued.
- `maxItems > 0` returns at most that many flattened items; if a batch
  would be split it requeues the trailing items at the head of the
  channel so the next poll picks up where this one stopped.

## Production callback-bind path (AP1 / AP2 / AP4)

For production push delivery the MCP host needs:

1. **Inbound listener**: an `OpcServerListener` bound to a callback TCP
   port. Default to **loopback only**; require an explicit
   `--allow-remote-callbacks` flag for non-loopback bind addresses.
2. **Sink OBJREF**: an `IOpcInterfaceRef` whose IID is
   `IOPCDataCallback`, OXID/OID/IPID are fresh GUIDs, and whose
   `ResolverBindings` include a TCP string binding for the listener's
   `IP:port`. See
   [`OpcInterfaceRefCodec`](../../src/Opc.Classic.Core/Dcom/OpcInterfaceRefCodec.cs)
   for the wire encoding.
3. **Advise**: pass the OBJREF to `IConnectionPointClientProxy.AdviseAsync(sink)`
   and store the returned cookie on the subscription so `Unadvise(cookie)`
   detaches cleanly at remove-group/dispose time.
4. **Object routing**: register the sink dispatcher in `OpcObjectRegistry`
   under the IPID chosen for the sink OBJREF so the inbound bind PDU
   resolves to the right dispatcher.
5. **Authentication**: `RpcServerConnectionProcessor` already handles
   inbound bind PDU authentication using the same `Spnego` machinery as
   the outbound client. The MCP host just needs to publish appropriate
   `NoOpAuthContext` or `SpnegoAuthContext` to the listener.

### Wire-level proof

The loopback test
[`OutboundCallbackOverTransportTests.cs`](../../tests/Opc.Classic.Integration.Tests/CompatMatrix/OutboundCallbackOverTransportTests.cs)
demonstrates a managed outbound proxy calling `OnCancelComplete`,
`OnWriteComplete`, and arbitrary repeated callbacks against a managed
in-process sink listener using `OpcServerListener` +
`RpcServerConnectionProcessor` + `IOPCDataCallbackServerDispatcher`. The
sample
[`samples/Opc.Classic.Samples.LoopbackDemo/LoopbackDaClient.cs`](../../samples/Opc.Classic.Samples.LoopbackDemo/LoopbackDaClient.cs)
demonstrates the full Advise → OnDataChange → channel-drain pattern with
`InMemoryCallChannel`. AP1/AP2/AP4 connect these proven primitives to the
real MCP DA flow.

## Firewall / DCOM ACL prerequisites on the Matrikon side

For Matrikon Simulation Server to reach the MCP host's listener:

1. **Outbound from Matrikon host**: Matrikon must allow outbound DCOM
   calls to the MCP host's listener port (and the dynamic RPC range
   135 / 49152-65535 unless the MCP listener publishes a fixed port).
2. **Inbound on MCP host**: a firewall rule allowing inbound TCP from the
   Matrikon host on the chosen callback port.
3. **DCOM ACLs on MCP host**: if MCP runs as a service identity, that
   identity needs to be granted COM Activation / Access permissions on
   the callback CLSID's AppID. See
   [`opcenum-auth.md`](opcenum-auth.md) for the equivalent OPCEnum-side
   recipe and the
   [`grant-opcenum-acl.ps1`](../../tools/grant-opcenum-acl.ps1) helper.
4. **Authentication compatibility**: Matrikon's outbound callback uses
   the auth level it was Advised under. The MCP listener must accept the
   same auth level (PKT_INTEGRITY by default).

## Probe

A future `tools/probe_servers.py --use-callbacks` flag will:

1. Turn off pull-mode polling.
2. Subscribe with the callback sink wired up.
3. Wait up to N seconds for an `OnDataChange` notification.
4. Compare the notification payload against a `read_sync` baseline.

The probe flag is gated on AP1/AP2 production wiring.

## Testing notes

The synthetic test suite at
[`DaDataCallbackSinkTests.cs`](../../tests/Opc.Classic.Mcp.Tests/DaDataCallbackSinkTests.cs)
covers:

- `OnDataChangeAsync` and `OnReadCompleteAsync` enqueue per-item batches.
- `OnWriteCompleteAsync` / `OnCancelCompleteAsync` increment counters but
  do not enqueue.
- Mismatched array lengths reject with `ArgumentException`.
- `DrainItems(maxItems)` respects the cap and requeues remainders.
- Multi-batch FIFO order is preserved.
- Bounded queue at capacity drops oldest with a counter increment.
- Drain after Dispose throws; enqueue after Dispose is a silent no-op
  (the call is still observed by the counter).
- `DateTimeOffset.FromFileTime` correctly maps the wire `long`
  timestamp.
