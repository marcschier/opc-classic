# Opc.Classic migration diagnostics

The `Opc.Classic.MigrationAnalyzer` package flags legacy OPC Foundation .NET API usage and offers safe starter code fixes for migrating to `Opc.Classic.*`. Diagnostics are informational by default so existing projects keep compiling; raise severity in `.editorconfig` when you are ready to enforce migration work.

| ID | Area | Summary |
| --- | --- | --- |
| [OCMDA001](OCMDA001.md) | DA | Replaces `OpcCom.Da.Server`/`OpcCom.Server` construction with async `OpcDaClient.ConnectAsync` and `await using` disposal. |
| [OCMDA002](OCMDA002.md) | DA | Replaces synchronous `server.Browse(...)` calls with `BrowseAsync(..., ct)` on browse abstractions. |
| [OCMDA003](OCMDA003.md) | DA | Replaces synchronous `group.Read(...)` calls with `ReadAsync(..., ct)` and async containing methods. |
| [OCMAE001](OCMAE001.md) | AE | Replaces `IOPCEventSubscription` callback plumbing with an `await foreach` event stream pattern. |
| [OCMHDA001](OCMHDA001.md) | HDA | Replaces synchronous `SyncReadRaw(...)` history reads with `ReadRawAsync(..., ct)`. |
| [OCMGEN001](OCMGEN001.md) | General | Rewrites `OpcRcw.*` namespaces toward `Opc.Classic.*` package namespaces. |
| [OCMGEN002](OCMGEN002.md) | General | Replaces manual VARIANT wrappers and `Marshal.GetVariant*` calls with `OpcVariant` factories. |

Manual migration guidance: apply code fixes in small batches, then adapt generated placeholder names such as `options`, `ct`, and `handler` to your application's dependency injection and cancellation model. Prefer injecting `Opc.Classic.*` interfaces into services instead of recreating COM-style global server objects.
