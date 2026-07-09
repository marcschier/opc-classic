# Opc.Classic migration diagnostics

The `Opc.Classic.MigrationAnalyzer` package identifies OPC Foundation .NET API usage and offers starter code fixes that move callers toward `Opc.Classic.*`. Diagnostics are informational by default so existing projects keep compiling; raise severity in `.editorconfig` when you are ready to enforce migration work.

The current diagnostic descriptors are in `MigrationDiagnosticDescriptors`. The release tracker currently lists these IDs in `AnalyzerReleases.Unshipped`; `AnalyzerReleases.Shipped.md` has no shipped entries yet.

| ID | Severity | Area | Summary |
| --- | --- | --- | --- |
| [OCMDA001](OCMDA001.md) | Info | DA | Replaces `OpcCom.Da.Server`/`OpcCom.Server` construction with injected or factory-created `IDaServer` instances and `await using` disposal. |
| [OCMDA002](OCMDA002.md) | Info | DA | Replaces synchronous `server.Browse(...)` calls with `IDaServer.BrowseAsync(..., ct)` async enumeration. |
| [OCMDA003](OCMDA003.md) | Info | DA | Replaces synchronous `group.Read(...)` calls with `IDaServer.ReadAsync(..., ct)` or `IDaSubscription.ReadAsync(..., ct)`. |
| [OCMAE001](OCMAE001.md) | Info | AE | Replaces `IOPCEventSubscription` callback plumbing with `IAeSubscription.Events` async stream consumption. |
| [OCMHDA001](OCMHDA001.md) | Info | HDA | Replaces synchronous `SyncReadRaw(...)` history reads with `IHdaServer.ReadRawAsync(..., ct)`. |
| [OCMGEN001](OCMGEN001.md) | Info | General | Rewrites `OpcRcw.*` namespaces toward `Opc.Classic.*` package namespaces. |
| [OCMGEN002](OCMGEN002.md) | Info | General | Replaces manual VARIANT wrappers with `OpcVariantConverter.FromObject` or typed `OpcVariant.From*` factories. |

Apply code fixes in small batches, then adapt generated placeholder names such as `options`, `ct`, and `handler` to your application's dependency injection and cancellation model. Prefer injecting `Opc.Classic.*` interfaces into services instead of recreating COM-style global server objects.
