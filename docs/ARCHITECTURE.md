# Opc.Classic Architecture

This document describes the current architecture of **Opc.Classic**, a cross-platform .NET 10 implementation of the OPC Classic protocol family. It supersedes the older phase-oriented architecture note and reflects the post-rename `Opc.Classic.*` assembly layout, MIT license, source-generator proxy pipeline, real DCOM call channel, server-side dispatchers, Kerberos/SPNEGO authentication, and license-clean dependency state.

## 1. Overview

Opc.Classic implements the OPC Classic family without requiring Windows COM runtime interop for the portable path. The stack is split into **15 source assemblies**, **17 active test projects**, and **5 sample applications**. Runtime libraries target .NET 10 and are designed for trimming and NativeAOT; the `Opc.Classic.Samples.AotCanary` sample verifies the consumer publish path.

```text
+------------------------------------------------------------------------+
| Applications, hosted services, samples, and conformance tests          |
+------------------------------------------------------------------------+
| Managed OPC APIs: DA | AE | HDA | DX | Cpx | Batch | Commands | Sec   |
+------------------------------------------------------------------------+
| Source-generated DCOM projections: [OpcInterface] + [OpcMethod]       |
+-------------------------------+----------------------------------------+
| DCOM/MSRPC transport          | XML-DA SOAP transport                  |
| Opc.Classic.Dcom              | Opc.Classic.Xml                       |
+-------------------------------+----------------------------------------+
| Core contracts, NDR, VARIANT, SAFEARRAY, FILETIME, result IDs         |
+------------------------------------------------------------------------+
```

The repository keeps the native OPC Foundation C++ sample servers in `COM/` and redistributable inputs in `External/` as conformance references. Those folders are intentionally preserved and are not part of the portable .NET runtime surface.

| Area | Current shape |
| --- | --- |
| Framework | .NET 10, SDK pinned by `global.json` |
| Runtime license | MIT |
| Source assemblies | `Opc.Classic.Core`, `Dcom`, `Dcom.Kerberos`, `Da`, `Ae`, `Hda`, `Dx`, `Cpx`, `Batch`, `Commands`, `Security`, `Xml`, `Discovery`, `Hosting`, `Generators` |
| Tests | TUnit on Microsoft.Testing.Platform, snapshot-first NDR tests, property tests, Testcontainers-ready integration scaffolds |
| Samples | AOT canary, CTT DA server, full DA server, AE server, HDA server |
| AOT stance | Runtime libraries avoid reflection emit, dynamic dispatch, and runtime COM RCWs; generators emit static code |

## 2. Transport layers

Transport is deliberately below the OPC semantic surface. Spec assemblies express DA, AE, HDA, and related concepts as managed async interfaces and wire DTOs. Transport-specific code converts generated call shims into bytes and back.

```text
               +-------------------------------+
               | Per-spec managed interfaces   |
               | IDaServer, IAeServer, ...     |
               +---------------+---------------+
                               |
                 [GenerateOpcProxy] output
                               |
+------------------------------+------------------------------+
| ICallChannel.InvokeAsync(iid, opnum, request, cancellation) |
+------------------------------+------------------------------+
       |                                               |
       v                                               v
+-------------------------+                    +-------------------------+
| DCOM ncacn_ip_tcp       |                    | XML-DA HttpClient       |
| Bind / Request / Resp   |                    | SOAPAction / DTO XML    |
+-------------------------+                    +-------------------------+
```

### DCOM over `ncacn_ip_tcp`

`Opc.Classic.Dcom` contains the pure-managed MSRPC/DCOM path. It originated from early DCOM port code but has been heavily modernized: SharpCifs.Std was removed, NTLM/NDR boundary types were reimplemented or vendored behind internal namespaces, logging was moved to Microsoft.Extensions.Logging, and the new `DcomCallChannel` drives real bind, request, response, fragmentation, and authentication flows over `IAsyncTransport`.

The DCOM path uses `ncacn_ip_tcp` for cross-platform network transport. Activation, object exporting, LocalCoClass hosting, and OXID runtime support sit under this layer. Generated proxies do not know whether the channel is loopback, in-memory, or TCP-backed; they only call `ICallChannel`.

### XML-DA over `HttpClient`

`Opc.Classic.Xml` implements XML-DA 1.01 as SOAP-over-HTTP. `HttpXmlDaClient` uses `HttpClient`, `text/xml`, SOAPAction headers, and AOT-friendly serializers. The eight spec operations are represented:

1. `GetStatus`
2. `Read`
3. `Write`
4. `Browse`
5. `Subscribe`
6. `SubscriptionPolledRefresh`
7. `SubscriptionCancel`
8. `GetProperties`

XML-DA is independent of DCOM, but it shares core concepts such as item IDs, result IDs, quality, and timestamp conversion.

### `ICallChannel`

`Opc.Classic.Core` defines the DCOM-independent call contract:

```csharp
Task<OpcCallResult> InvokeAsync(
    Guid interfaceId,
    int opnum,
    ReadOnlyMemory<byte> requestPayload,
    CancellationToken cancellationToken = default);
```

Generated proxies marshal parameters into NDR, call this method, check HRESULTs, and decode response payloads. Tests use `InMemoryCallChannel` to exercise generated code and server dispatch without opening sockets.

### `IAsyncTransport`

`Opc.Classic.Core.Transport` defines a pipelines-backed `IAsyncTransport` abstraction for bidirectional byte streams. It gives DCOM and test code a common async transport contract while keeping socket, loopback, and in-memory transport concerns out of protocol code.

## 3. NDR, VARIANT, and SAFEARRAY pipeline

NDR support lives in `Opc.Classic.Core.Ndr`. `NdrWriter` and `NdrReader` are `ref struct` types over spans. They are forward-only, little-endian, naturally aligned, and intentionally explicit: callers write primitive fields, then OAUT values, then OPC structs.

```csharp
Span<byte> scratch = stackalloc byte[256];
var writer = new NdrWriter(scratch);

writer.WriteGuid(IOPCServer.InterfaceId);
writer.WriteUnicodeStringPtr("Random.Int1");
writer.WriteFileTime(DateTimeOffset.UtcNow);

ReadOnlyMemory<byte> payload = scratch[..writer.Position].ToArray();
```

| Capability | Managed type / helper | Notes |
| --- | --- | --- |
| Scalars | `NdrWriter.WriteInt32`, `ReadUInt16`, etc. | Standard NDR alignment and little-endian layout. |
| GUID | `WriteGuid`, `ReadGuid` | DCE wire layout: `uint32`, `uint16`, `uint16`, `byte[8]`. |
| FILETIME | `WriteFileTime`, `ReadFileTime` | Two 32-bit halves, low before high. |
| LPWSTR | `WriteUnicodeString`, `ReadUnicodeString` | Conformant/varying string with trailing NUL. |
| LPWSTR pointer | `WriteUnicodeStringPtr`, `ReadUnicodeStringPtr` | Unique pointer referent plus string body. |
| BSTR | `WriteBstr`, `ReadBstr` | OAUT flagged-word blob format. |
| Interface pointer | IFACE pointer helpers | Used by COM activation/proxy paths and deferred multi-return shapes. |
| VARIANT | `OpcVariant` + `NdrVariantExtensions` | Covers scalar values and SAFEARRAY-in-VARIANT. |
| SAFEARRAY | `OpcSafeArray` + `NdrSafeArrayExtensions` | Managed rank/bounds/value carrier plus NDR codec. |

The codec registry used by the proxy generator covers primitives, strings, GUIDs, `OpcVariant`, `OpcSafeArray`, and 21 spec struct codecs across DA, AE, HDA, and Batch. Codec files are small static classes with one responsibility: encode/decode the exact IDL shape of an OPC structure.

Representative struct codecs:

| Spec | Codecs |
| --- | --- |
| DA | server status, group state, item definition, item result, item state, item VQT, item attributes, item properties, item property, browse element |
| AE | server status, event notification, condition state |
| HDA | time, item, attribute, annotation, modified item |
| Batch | batch summary, batch summary filter |

## 4. Source generators (`Opc.Classic.Generators`)

`Opc.Classic.Generators` is build-time only and targets Roslyn's `netstandard2.0` analyzer requirement. Its output is the AOT-critical part: compile-time metadata, opnum tables, and static client proxies instead of runtime reflection or expression tree dispatch.

### `OpcInterfaceGenerator`

`[OpcInterface(iid)]` decorates partial DCOM projection interfaces. The generator emits:

- `InterfaceId` static property
- nested `Opnums` class for `[OpcMethod(opnum)]` methods
- diagnostics for malformed GUIDs, non-partial targets, and duplicate opnums

```csharp
[OpcInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCServer
{
    [OpcMethod(3)]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
```

### `OpcProxyGenerator`

`[GenerateOpcProxy]` emits `<InterfaceName>ClientProxy : <InterfaceName>` partial classes. A generated proxy:

1. rents a scratch buffer,
2. writes method parameters with the codec registry,
3. calls `ICallChannel.InvokeAsync(interfaceId, opnum, payload)`,
4. checks HRESULTs,
5. decodes the response payload,
6. returns buffers even on exceptions.

The current production surface contains 44+ `[OpcInterface]`-decorated partials across all OPC sub-specs and roughly 90 generated method bodies across about 30 interfaces. The codec registry contains 32+ marshalling entries: primitives, arrays, strings, GUIDs, `OpcVariant`, `OpcSafeArray`, and spec codecs.

## 5. Server-side hosting

Server hosting is split between the common host infrastructure in `Opc.Classic.Hosting` and per-spec server packages.

```text
+-----------------------------------------------------------+
| Microsoft.Extensions.Hosting                              |
| ClassicHostedService : IHostedService                     |
+---------------------------+-------------------------------+
                            |
               IOpcServerHost.StartAsync / StopAsync
                            |
+---------------------------+-------------------------------+
| OpcDaServerHost | OpcAeServerHost | OpcHdaServerHost      |
+---------------------------+-------------------------------+
                            |
          RequestCoPdu -> dispatcher -> managed impl
```

| Component | Role |
| --- | --- |
| `IClsidRegistry` | Abstracts registration of CLSID, ProgID, and server metadata. Implementations include configuration-driven lookup and a Windows registry writer for native interoperability. |
| `IOpcServerHost` | Common lifecycle contract for managed OPC servers. |
| `ClassicHostedService` | Bridges `IOpcServerHost` into `Microsoft.Extensions.Hosting` and coordinates start/stop with application lifetime. |
| `OpcDaServerHost` | DA server host for managed implementations of `IOpcDaServer` / `IDaServer` surfaces. |
| `OpcAeServerHost` | AE server host and event-tree wiring for alarms/events implementations. |
| `OpcHdaServerHost` | HDA server host for historical data APIs and query dispatch. |
| `IOpcDaServerDispatcher` | Routes incoming DCOM `RequestCoPdu` payloads by opnum and decodes into calls on the managed DA implementation. AE and HDA follow the same per-method pattern. |
| `OpcDaDataChangePublisher` | Publishes DA data-change callbacks from managed subscriptions. |

Per-method dispatchers are the server-side mirror of generated client proxies. They keep incoming PDU handling table-driven and AOT-safe while preserving OPC IDL opnums and HRESULT semantics.

## 6. Authentication

Authentication is behind the `IAuthContext` abstraction so the transport and generated proxies can negotiate, sign, seal, and verify messages without depending on a specific mechanism.

| Mechanism | Status |
| --- | --- |
| NTLMv2 | Default. The implementation is internal and self-contained after the SharpCifs.Std removal. NTLMv1 is obsolete and gated behind explicit opt-in for legacy interoperability. |
| Kerberos | `Opc.Classic.Dcom.Kerberos` integrates Kerberos.NET for AP-REQ/AP-REP flows. |
| SPNEGO | RFC 4178 encoder/decoder supports negotiation wrappers around Kerberos and NTLM tokens. |
| Channel binding | RFC 5056 / Extended Protection for Authentication helper binds authentication to the transport channel. |
| DCOM hardening | Defaults align with KB5004442: packet integrity, NTLMv2, and NTLM2 session security. |

The DCOM stack signs and validates PDUs according to the selected protection level. Packet integrity is the default because modern Windows DCOM servers require it after the KB5004442 hardening rollout.

## 7. Discovery (`Opc.Classic.Discovery`)

Discovery normalizes the many ways OPC Classic servers are found into one async contract:

```csharp
public interface IOpcDiscovery
{
    IAsyncEnumerable<OpcServerDescriptor> DiscoverAsync(
        OpcDiscoveryQuery query,
        CancellationToken cancellationToken = default);
}
```

| Component | Role |
| --- | --- |
| `LocalEnum` | Fully implemented local discovery from `IConfiguration` and, where available, Windows Registry entries. |
| `RemoteRegistryEnum` | Scaffold for remote registry enumeration over SMB/registry transports. |
| `OpcEnumClient` | Scaffold for OPCEnum/DCOM discovery. |
| `OpcDiscoveryFactory` | Composite that runs configured discovery sources and deduplicates by CLSID/ProgID. |

Discovery is intentionally separate from activation. A consumer can enumerate descriptors, choose policy/authentication, and then create a DCOM channel or XML-DA client through the relevant transport path.

## 8. Spec coverage matrix

| Spec | Managed types | Interface partials | NDR codecs | Generator coverage | Status |
| --- | --- | --- | --- | --- | --- |
| DA | `IDaServer`, `IDaSubscription`, item/value/property/browse/subscription records | DA 2.x, DA 3.0, callbacks, browse, group, item, sync/async, item I/O | Broadest codec set: status, group, item def/result/state/VQT/attributes/properties/property/browse | Highest coverage; generated client proxies and DA dispatcher route core methods | Active client/server stack, CTT sample, full DA sample, data-change publisher |
| AE | `IAeServer`, subscriptions, event notifications, condition state, categories, filters | event server, event subscription, browser, area browser, sink projections | server status, event notification, condition state | Generated proxy/metadata coverage for AE projections | Event tree and AE host/sample paths in progress |
| HDA | `IHdaServer`, time, items, attributes, annotations, aggregates, modified values | server, browser, sync/async read, update, annotations, playback, callbacks | time, item, attribute, annotation, modified item | Generated proxy/metadata coverage for HDA projections | Historical query and host/sample paths in progress |
| DX | connection/source-server/configuration records and enums | `IOPCConfiguration` and related projections | none currently required beyond primitives/arrays | IID/opnum metadata and proxy scaffolding | Configuration model represented; advanced server-to-server flows remain compat-gated |
| Cpx | complex type dictionaries, fields, and OPCBinary-style values | Complex Data projections | uses core primitives/variants; dedicated struct codecs not currently needed | proxy scaffolding for projection interfaces | Managed vocabulary for Complex Data interop |
| Batch | summaries, filters, batch state/types, enumeration records | Batch server/enumerator projections | summary and summary-filter codecs | generated projection metadata and method scaffolding | Bootstrap Batch surface and wire structs represented |
| Commands | command metadata, state, invocation bootstrap records | command manager, command, and callback projections | primitives/arrays through registry | generated projection metadata and method scaffolding | IDL surface bootstrapped for command workflows |
| Security | `IOpcSecurity` facade and security descriptors | `IOPCSecurityNT`, `IOPCSecurityPrivate` projections | primitives/strings/variants through registry | generated projection metadata and method scaffolding | Security facade plus DCOM projection coverage |
| XML-DA | `IXmlDaClient`, DTOs, SOAP serializers, HTTP client | not DCOM-based | not NDR-based | not generator-based | All eight XML-DA operations implemented over HTTP/SOAP |

## 9. Logging

`Opc.Classic.Dcom.Internal.LogHost` owns the process-wide `ILoggerFactory`. The legacy `Log.Logger.X(...)` shape is preserved where needed but now forwards to Microsoft.Extensions.Logging instead of Serilog. This keeps runtime dependencies small, allows application-level logger injection, and makes tests use `FakeLogger` from `Microsoft.Extensions.Diagnostics.Testing`.

## 10. Test infrastructure

Tests use **TUnit** on **Microsoft.Testing.Platform**. Test projects are executable test hosts, not VSTest-only class libraries. Common patterns are:

- snapshot-first assertions for NDR byte streams using Verify.TUnit,
- property tests for codec and crypto invariants,
- `InMemoryCallChannel` for generated proxy tests,
- `InMemoryAsyncTransport` for DCOM PDU and channel tests,
- Testcontainers-ready integration scaffolding,
- `FakeLogger` / `FakeLoggerProvider` for structured logging assertions,
- coverage gates at 70% line and 60% branch with stricter targets planned before 1.0.

`NdrReader` and `NdrWriter` are `ref struct` types and cannot cross `await` boundaries. Tests capture decoded values synchronously, then use async fluent assertions:

```csharp
OpcServerStatus status;

{
    var reader = new NdrReader(payload);
    status = NdrOpcServerStatusCodec.Decode(ref reader);
}

await Assert.That(status.VendorInfo).IsEqualTo("Opc.Classic");
```

## 11. AOT story

AOT compatibility is a cross-cutting design constraint. Runtime code avoids:

- `System.Reflection.Emit`,
- runtime proxy generation,
- `Expression.Compile`,
- `MethodInfo.Invoke` dispatch,
- `[ComImport]` and Windows COM RCWs,
- runtime string-to-type activation.

`src/Directory.Build.props` sets strict .NET 10, analyzer, trimming, package metadata, and AOT properties for source projects. `Opc.Classic.Generators` is the one expected exception because Roslyn generators are build-time tools; their output must be AOT-safe, not the generator assembly itself. The AOT canary publishes a consumer executable with `PublishAot=true` and treats IL2xxx/IL3xxx warnings as regressions.

## 12. Build, CI, and release

Local source-of-truth commands use the XML solution file:

```powershell
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx
dotnet test Opc.Classic.slnx
```

CI and release workflows are split by purpose:

| Workflow | Purpose |
| --- | --- |
| `.github/workflows/build.yml` | Linux/macOS/Windows matrix, Debug/Release coverage, format/analyzer gates, AOT canary publish. |
| Windows conformance job | Builds preserved `COM/` native sample servers, registers them, and runs native conformance subsets where runner prerequisites are present. |
| `.github/workflows/opc-ctt.yml` | OPC Foundation Compliance Test Tool scaffold; gated by membership and installer secret. |
| `.github/workflows/release.yml` | Release packaging on `v*` tags, using central package metadata and MIT license expression. |

The `.NET 10` SDK version is pinned in `global.json` with feature roll-forward. Release notes are maintained in `CHANGELOG.md`, and package metadata is centralized in `src/Directory.Build.props` / `Directory.Packages.props`.

## 13. License and governance

Opc.Classic is MIT licensed as of `0.4.0-alpha.1`. Governance and contributor-facing documents are:

| File | Role |
| --- | --- |
| `LICENSE` | MIT license text. |
| `SECURITY.md` | Vulnerability reporting and supported versions. |
| `CONTRIBUTING.md` | Build/test/style, sample, conformance, and PR guidance. |
| `CHANGELOG.md` | Keep a Changelog release history. |
| `THIRD-PARTY-NOTICES.md` | Preserved notices for third-party and OPC Foundation material. |

Do not remove original notices from `COM/` or `External/`. Those trees are preserved conformance assets, not relicensed project source.

## 14. Forward roadmap

The 1.0.0 gates are now focused on conformance and compatibility, not dependency cleanup. SharpCifs.Std has been fully removed and the runtime tree is license-clean. Remaining gates are:

1. **Compatibility matrix** — verify client/server behavior against native OPC Foundation samples, Matrikon Simulation Server where available, and representative DA/AE/HDA servers.
2. **OPC CTT** — run official Compliance Test Tool workflows for managed DA server coverage once installer access and secrets are available.
3. **Real Native Server Conformance** — keep Windows runner coverage green for preserved `COM/` native servers and expand assertions from soft-skip scaffolds to release-gating tests.
4. **Sample completeness** — keep AotCanary, CttServer, DaServer, AeServer, and HdaServer aligned with the public APIs and documented quick starts.
5. **Public API stabilization** — lock package IDs, namespaces, generated proxy names, and connection bootstrap APIs before the first stable release.
