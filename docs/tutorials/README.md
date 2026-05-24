# Opc.Classic long-form tutorials

Applies to Opc.Classic 0.6.0-alpha.1 (targeting 1.0.0-rc.1).

These tutorials are deeper than the short recipes in [../cookbook/README.md](../cookbook/README.md). They walk through complete production scenarios, explain the sequence of calls, and call out the platform, security, and diagnostics traps that appear in production OPC Classic deployments. Opc.Classic implements all nine OPC Classic sub-specifications under MIT licensing, uses the `Opc.Classic.*` namespace family, provides self-contained NTLMv2/Kerberos/SPNEGO with channel binding, and keeps proxies and dispatchers source-generated for NativeAOT compatibility.

## Start here: new adopters

1. [Build your first DA client](01-build-your-first-da-client.md) — create a .NET 10 hosted service that browses tags, reads values, subscribes to changes, handles OPC HRESULTs, and shuts down cleanly.
2. [Host an OPC DA server](02-host-an-opc-server.md) — implement a managed `IOpcDaServer`, register it with `Opc.Classic.Hosting`, model a tag tree, and preserve DA group/HRESULT semantics.
3. [Cross-platform deployment](03-cross-platform-deployment.md) — package clients and servers for Linux, macOS, containers, and Kubernetes, including NativeAOT, multi-arch images, health checks, and Kerberos files.

## Security and operations

4. [Security with Kerberos and channel binding](04-security-with-kerberos-and-channel-binding.md) — move from NTLMv2 to Kerberos/SPNEGO, register SPNs, use keytabs, and troubleshoot common Kerberos failures.
5. [Troubleshooting and diagnostics](09-troubleshooting-and-diagnostics.md) — diagnose connection, authentication, NDR, HRESULT, and channel-binding failures with `ILogger` and OpenTelemetry-friendly app instrumentation.

## OPC feature areas

6. [Historical data with HDA](05-historical-data-with-hda.md) — read raw and processed historian data, use built-in aggregates, handle annotations, and compare the HDA client and server samples.
7. [Events and alarms with AE](06-events-and-alarms-with-ae.md) — browse areas, filter simple/tracking/condition events, refresh subscriptions, and acknowledge conditions.

## Migration and advanced topics

8. [Migrate from the OPC Foundation .NET API](07-migrating-from-opc-foundation-net-api.md) — translate namespaces and synchronous patterns into `Opc.Classic.*` async interfaces, with gotchas and migration-script guidance.
9. [Performance tuning](08-performance-tuning.md) — tune NDR hot paths, ArrayPool usage, codec choice, `OpcVariant` boxing, async I/O pipelining, and DA batch sizes.
10. [AOT and trimming](10-aot-and-trimming.md) — publish NativeAOT binaries safely, understand what is trimming-safe, and use the AOT canary sample as a deployment gate.

For architectural background, read [../ARCHITECTURE.md](../ARCHITECTURE.md). For adoption and package guidance, read [../ADOPTION.md](../ADOPTION.md). For compact production recipes, use [../cookbook/README.md](../cookbook/README.md).
