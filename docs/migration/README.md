# Opc.Classic migration diagnostics

The `Opc.Classic.MigrationAnalyzer` package identifies OPC Foundation .NET API usage and offers starter code fixes that move callers toward `Opc.Classic.*`. Diagnostics are informational by default so existing projects keep compiling; raise severity in `.editorconfig` when you are ready to enforce migration work.

The current diagnostic descriptors are in `src/Opc.Classic.MigrationAnalyzer/MigrationDiagnosticDescriptors.cs`. The release tracker currently lists these IDs in `AnalyzerReleases.Unshipped.md`; `AnalyzerReleases.Shipped.md` has no shipped entries yet.

| ID | Severity | Area | Summary |
| --- | --- | --- | --- |
| [OCMDA001](OCMDA001.md) | Info | DA | Replaces `OpcCom.Da.Server`/`OpcCom.Server` construction with async `OpcDaClient.ConnectAsync` and `await using` disposal. |
| [OCMDA002](OCMDA002.md) | Info | DA | Replaces synchronous `server.Browse(...)` calls with `BrowseAsync(..., ct)` on browse abstractions. |
| [OCMDA003](OCMDA003.md) | Info | DA | Replaces synchronous `group.Read(...)` calls with `ReadAsync(..., ct)` and async containing methods. |
| [OCMAE001](OCMAE001.md) | Info | AE | Replaces `IOPCEventSubscription` callback plumbing with an `await foreach` event stream pattern. |
| [OCMHDA001](OCMHDA001.md) | Info | HDA | Replaces synchronous `SyncReadRaw(...)` history reads with `ReadRawAsync(..., ct)`. |
| [OCMGEN001](OCMGEN001.md) | Info | General | Rewrites `OpcRcw.*` namespaces toward `Opc.Classic.*` package namespaces. |
| [OCMGEN002](OCMGEN002.md) | Info | General | Replaces manual VARIANT wrappers and native VARIANT conversion calls with `OpcVariant` factories. |

Apply code fixes in small batches, then adapt generated placeholder names such as `options`, `ct`, and `handler` to your application's dependency injection and cancellation model. Prefer injecting `Opc.Classic.*` interfaces into services instead of recreating COM-style global server objects.
