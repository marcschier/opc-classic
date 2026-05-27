# Architecture diagram suite

This directory contains Mermaid diagrams that support the main architecture narrative in [`docs\ARCHITECTURE.md`](../ARCHITECTURE.md) and the adopter-facing terminology in [`docs\ADOPTION.md`](../ADOPTION.md). The diagrams describe the current `Opc.Classic.*` architecture: source-generated client proxies and server dispatchers, `ICallChannel` with in-memory and DCOM implementations, channel-level NTLM/Kerberos/SPNEGO/CBT, NativeAOT-compatible libraries, and coverage across DA, AE, HDA, Batch, Commands, Security, DX, Cpx, and Discovery.

GitHub renders Mermaid fenced blocks directly in Markdown. Keep diagrams small, prefer short labels, use `<br/>` for label line breaks, and use standard Mermaid arrows such as `-->`, `->>`, and `-->>`.

## Diagrams

1. [High-level architecture](01-high-level-architecture.md) — top-level client, generated proxy, `ICallChannel`, DCOM/in-memory channels, NDR, `TcpClientTransport`, and managed listener shape.
2. [Call shim flow](02-call-shim-flow.md) — outbound generated proxy call sequence for `IOPCServer::GetStatus`.
3. [Server dispatch flow](03-server-dispatch-flow.md) — inbound TCP listener, `RpcServerConnectionProcessor`, optional `OpcObjectRegistry`, `OpcDaServerDispatcher`, and `IOpcDaServer` routing.
4. [NTLM handshake](04-ntlm-handshake.md) — NTLMSSP NEGOTIATE, CHALLENGE, AUTHENTICATE, and CBT computation.
5. [Kerberos handshake](05-kerberos-handshake.md) — Kerberos AP-REQ/AP-REP mutual authentication and GSS-API protection seam.
6. [SPNEGO negotiation](06-spnego-negotiation.md) — NegTokenInit, NegTokenResp, mechanism selection, and MIC handling.
7. [Discovery flow](07-discovery-flow.md) — OPCEnum / `IOPCServerList` and remote-registry discovery strategies.
8. [Source generator pipeline](08-source-generator-pipeline.md) — attributes, Roslyn generators, codec table, and emitted proxies and dispatchers.
9. [Subscription data flow](09-subscription-data-flow.md) — DA group, item activation, sampling, `IOpcDataCallbackSink`, and callback delivery.
10. [AOT and trimming shape](10-aot-trimming-shape.md) — AOT-visible static code, analyzers, banned APIs, DCOM channel shape, and canary publish.
