# Opc.Classic Architecture

Opc.Classic is a cross-platform, NativeAOT-compatible .NET 10 implementation of the OPC Classic protocol family. The current package version is `0.6.0-alpha.1`; the next release train targets `1.0.0-rc.1`. Runtime packages and namespaces are rooted at `Opc.Classic.*`, and the project is MIT licensed.

The architecture is designed around three constraints:

1. OPC Classic wire compatibility with DA, AE, HDA, Batch, Commands, Complex Data, DX, Security, and Discovery.
2. A portable managed DCOM/MSRPC stack that does not require Windows COM runtime APIs for the normal cross-platform path.
3. Trimmable and NativeAOT-safe runtime libraries, including `Opc.Classic.Dcom`.

## 1. System overview

```text
+------------------------------------------------------------------------+
| Applications, services, gateways, samples, and conformance tests       |
+------------------------------------------------------------------------+
| Managed OPC APIs: DA | AE | HDA | Batch | Commands | Cpx | DX | Sec   |
+------------------------------------------------------------------------+
| Discovery, hosting, CLSID/ProgID registry, sample server models        |
+------------------------------------------------------------------------+
| Source-generated DCOM client proxies and server dispatchers            |
+------------------------------------------------------------------------+
| Opc.Classic.Dcom: MSRPC/DCOM activation, bind, request, auth, ping     |
+------------------------------------------------------------------------+
| Opc.Classic.Core: NDR, VARIANT, SAFEARRAY, FILETIME, HRESULT, URLs     |
+------------------------------------------------------------------------+
```

The portable runtime is pure managed code. Windows-specific features, such as local COM registry writes for native client activation, are isolated behind platform-guarded components. The repository also keeps OPC Foundation C++ sample servers in `COM/` and redistributable inputs in `External/` as conformance assets; they are not part of the portable runtime libraries.

| Area | Current state |
| --- | --- |
| Framework | .NET 10, SDK pinned by `global.json` |
| License | MIT |
| Public namespace root | `Opc.Classic.*` |
| OPC areas | DA, AE, HDA, Batch, Commands, Cpx, DX, Security, Discovery |
| DCOM stack | Managed MSRPC/DCOM over async transports with v5.6 activation support |
| Authentication | Self-contained NTLMv2, Kerberos, SPNEGO, channel binding, RFC 5056 / RFC 5929 helpers |
| Generation | Source-generated client proxies and server dispatchers: 47 dispatchers, 127 opnums |
| AOT stance | Runtime libraries are trimmable; `Opc.Classic.Dcom` runs with strict AOT/trimming analyzers enabled |
| Samples | 9 sample apps: DA/AE/HDA server+client, LoopbackDemo, CttServer, AotCanary |
| Verification | 1253 passed / 24 skipped / 0 failed |

## 2. Assembly layout

Runtime source is organized by protocol boundary rather than by sample scenario.

| Assembly | Role |
| --- | --- |
| `Opc.Classic.Core` | Common contracts, URLs, connection data, NDR, OAUT, `OpcVariant`, `OpcSafeArray`, HRESULT/result identifiers, testing transports. |
| `Opc.Classic.Dcom` | Managed MSRPC/DCOM channel, activation, OBJREF/OXID handling, NTLMv2, packet protection, ping, and server object export. |
| `Opc.Classic.Dcom.Kerberos` | Kerberos/SPNEGO token flow and packet protection integration. |
| `Opc.Classic.Da` | Data Access managed APIs, DCOM projections, generated proxies/dispatchers, hosting, subscriptions, item/value models. |
| `Opc.Classic.Ae` | Alarms & Events managed APIs, event categories, subscriptions, condition/event models, DCOM projections. |
| `Opc.Classic.Hda` | Historical Data Access APIs, time ranges, attributes, aggregates, annotations, playback/update projections. |
| `Opc.Classic.Batch` | Batch summary, filtering, enumeration, and batch-state models. |
| `Opc.Classic.Commands` | OPC Commands metadata, state, invocation, and callback surfaces. |
| `Opc.Classic.Cpx` | Complex Data dictionaries, fields, type descriptions, and OPC Binary-style values. |
| `Opc.Classic.Dx` | Data eXchange configuration, source server, and connection models. |
| `Opc.Classic.Security` | OPC Security abstractions plus channel-binding helpers. |
| `Opc.Classic.Discovery` | Local configuration, Windows registry, remote registry, and OPCEnum discovery strategies. |
| `Opc.Classic.Hosting` | Microsoft.Extensions.Hosting integration and CLSID/ProgID registry abstractions. |
| `Opc.Classic.Xml` | XML-DA HTTP/SOAP DTOs, serializers, and client/server transport shape. |
| `Opc.Classic.Generators` | Build-time Roslyn generators for metadata, client proxies, and server dispatchers. |

## 3. Transport model

Transport is below the OPC semantic surface. Spec assemblies expose managed async contracts and wire DTOs. Generated code converts method parameters into NDR payloads and calls a channel abstraction.

```text
                 +------------------------------+
                 | Per-spec managed interfaces  |
                 | IDaServer, IAeServer, ...    |
                 +--------------+---------------+
                                |
                   generated proxy / dispatcher
                                |
+-------------------------------+-------------------------------+
| ICallChannel.InvokeAsync(iid, opnum, request, cancellation)   |
+-------------------------------+-------------------------------+
        |                                                |
        v                                                v
+--------------------------+                    +------------------------+
| DCOM ncacn_ip_tcp        |                    | XML-DA HTTP/SOAP       |
| bind / request / resp    |                    | HttpClient serializers |
+--------------------------+                    +------------------------+
```

### `ICallChannel`

`Opc.Classic.Core` defines the transport-independent DCOM call contract:

```csharp
Task<NdrCallResult> InvokeAsync(
    Guid interfaceId,
    int opnum,
    ReadOnlyMemory<byte> requestPayload,
    CancellationToken cancellationToken = default);
```

Generated client proxies marshal parameters into NDR, call the channel, check HRESULTs, and decode response payloads. Generated server dispatchers perform the inverse operation: select the opnum, decode request payloads, call the managed implementation, and encode HRESULT plus out parameters.

Tests and loopback samples use `InMemoryCallChannel` to exercise the exact generated proxy/dispatcher path without opening sockets.

### `IAsyncTransport`

`Opc.Classic.Core.Transport` defines a pipelines-backed byte-stream abstraction. DCOM code uses it for bind, request, response, fragmentation, authentication trailers, and packet protection. The abstraction keeps TCP, in-memory, loopback, and test transports outside the generated OPC method code.

### DCOM over `ncacn_ip_tcp`

`Opc.Classic.Dcom` implements the managed MSRPC/DCOM path for cross-machine Classic endpoints. The channel handles:

- endpoint mapper and activation flows;
- DCE/RPC bind and alter-context negotiation;
- request/response PDU encoding;
- fragmentation and reassembly;
- ORPC `this` / `that` envelopes;
- OBJREF and OXID runtime structures;
- packet signing and sealing according to the negotiated protection level.

Generated proxies do not depend on the concrete channel. The same proxy can target an in-memory loopback channel, a test fixture, or a TCP-backed DCOM channel.

### XML-DA over HTTP/SOAP

`Opc.Classic.Xml` implements the XML-DA 1.01 HTTP/SOAP shape. XML-DA is independent of DCOM but shares core concepts such as item IDs, result IDs, quality, timestamps, and value conversion.

## 4. Activation and object lifetime

Managed activation is centered on DCOM v5.6 `IRemoteSCMActivator`.

| Component | Role |
| --- | --- |
| `IRemoteSCMActivator` | Source-generated DCOM projection for `RemoteGetClassObject` and `RemoteCreateInstance`. |
| `RemoteSCMActivatorServer` | Server-side v5.6 activation implementation for managed class factories and object export. |
| `ClassFactoryRegistry` | Maps CLSIDs and ProgIDs to managed factories. |
| `LocalCoClass` / OXID runtime | Exports managed objects as DCOM object references and maintains object lifetime. |
| Ping support | Implements DCOM keepalive semantics for exported objects and client sessions. |

This path lets managed processes host DCOM clients. Windows native clients still require appropriate CLSID/ProgID registration and platform setup, while the managed server implementation remains portable.

## 5. NDR, VARIANT, and SAFEARRAY

NDR support lives in `Opc.Classic.Core.Ndr`. `NdrWriter` and `NdrReader` are span-based `ref struct` types. They are forward-only, little-endian, naturally aligned, and explicit about every field written to the wire.

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
| Scalars | `NdrWriter.WriteInt32`, `NdrReader.ReadUInt16`, etc. | Standard NDR alignment and little-endian layout. |
| GUID | `WriteGuid`, `ReadGuid` | DCE wire layout: `uint32`, `uint16`, `uint16`, `byte[8]`. |
| FILETIME | `WriteFileTime`, `ReadFileTime` | Two 32-bit halves, low before high. |
| LPWSTR / BSTR | Unicode string helpers | Conformant/varying string and OAUT BSTR representations. |
| Interface pointers | OBJREF helpers | Used by activation, callbacks, and COM-style out parameters. |
| VARIANT | `OpcVariant` + NDR extensions | Scalar, array, byref, nested, and automation-compatible value shapes. |
| SAFEARRAY | `OpcSafeArray` + NDR extensions | Rank, bounds, feature flags, and element data for OPC payloads. |

The codec registry supports 30+ entries across primitives, arrays, strings, GUIDs, `OpcVariant`, `OpcSafeArray`, and OPC struct codecs. Codec classes are small static components that encode and decode the exact IDL shape for a structure.

Representative struct codecs:

| Spec | Codecs |
| --- | --- |
| DA | server status, group state, item definition/result/state/VQT, attributes, properties, browse elements |
| AE | server status, event notifications, condition state, event attributes |
| HDA | time, item, attribute, annotation, modified item, aggregates |
| Batch | batch summary and summary filter |
| Discovery | server-list descriptors and enumeration payloads |

## 6. Source generators

`Opc.Classic.Generators` emits AOT-safe code at build time. Runtime libraries avoid reflection dispatch, dynamic method creation, expression compilation, and runtime string-to-type activation.

### Interface metadata

`[OpcInterface]` and `[OpcMethod]` describe DCOM projection interfaces and opnums:

```csharp
[OpcInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCServer
{
    [OpcMethod(6)]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
```

The generator emits interface IDs, opnum constants, diagnostics for malformed metadata, client proxy classes, and server dispatcher classes.

### Client proxies

A generated `<InterfaceName>ClientProxy`:

1. rents or allocates an encode buffer;
2. writes input parameters with the codec registry;
3. calls `ICallChannel.InvokeAsync(interfaceId, opnum, payload)`;
4. checks the returned HRESULT;
5. decodes output values;
6. releases buffers on every path.

Example generated type names include `IOPCServerClientProxy`, `IOPCEventServerClientProxy`, and `IOPCHDA_ServerClientProxy`. OPC IDL identifiers keep their spec-defined spelling, including underscores where the OPC IDL requires them.

### Server dispatchers

A generated `<InterfaceName>ServerDispatcher` mirrors the proxy path. It decodes the request, calls the managed server implementation, and encodes the response. The current generated server-dispatch surface covers 47 dispatchers and 127 opnums.

## 7. Server hosting

Server hosting uses `Microsoft.Extensions.Hosting`. Applications register the shared Classic hosted service, one or more per-spec servers, and a CLSID/ProgID registry.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddClassicServer();
builder.Services.AddClassicClsidRegistry(builder.Configuration);
builder.Services.AddOpcDaServer<MyDaServer>(static options =>
{
    options.Clsid = Guid.Parse("7f41b3e9-32ec-40c9-9e42-3e0e0fce5a11");
    options.ProgId = "Contoso.ManagedOpcDa.1";
    options.FriendlyName = "Contoso Managed OPC DA Server";
    options.ListenAddress = "127.0.0.1:0";
});

await builder.Build().RunAsync();
```

| Component | Role |
| --- | --- |
| `IClsidRegistry` | Resolves CLSID, ProgID, friendly name, assembly/type, and registration metadata. |
| `ClassicHostedService` | Starts and stops registered `IOpcServerHost` instances with the application lifetime. |
| `OpcDaServerHost` / AE / HDA hosts | Bind managed implementations to generated dispatchers and runtime endpoints. |
| `IOpcDaServer` | Managed DA server contract implemented by application code. |
| Data-change publishers | Bridge managed subscription updates to `IOPCDataCallback` callbacks. |

AE and HDA hosting use the same pattern with their per-spec server contracts and sample applications.

## 8. Authentication and packet protection

Authentication is behind the `IAuthContext` abstraction so transports and generated code do not depend on a concrete security mechanism.

| Mechanism | Current state |
| --- | --- |
| NTLMv2 | Self-contained implementation with MIC handling, extended session security, signing, and sealing. |
| Kerberos | Kerberos AP-REQ/AP-REP and packet protection for supported encryption types. |
| SPNEGO | RFC 4178 / [MS-SPNG] negotiation wrapper for Kerberos and NTLM tokens. |
| Channel binding | RFC 5056 and RFC 5929 helpers, including `tls-server-end-point` binding material. |
| DCOM hardening | Defaults align with KB5004442: NTLMv2 or Kerberos plus packet integrity. |

`OpcConnectData` expands `OpcProtectionLevel.Default` to `Integrity`. Use `Privacy` when you need confidentiality in addition to integrity. Use `Connect` only for isolated compatibility exceptions where the target cannot accept packet integrity.

The security implementation follows the relevant wire specifications: [MS-DCOM], [MS-RPCE], [MS-NLMP], [MS-KILE], [MS-SPNG], RFC 4178, RFC 5056, RFC 5929, RFC 4121, RFC 4757, RFC 3962, and RFC 8009.

## 9. Discovery

Discovery is modeled as an async enumeration of server descriptors. Consumers can enumerate candidates, apply policy, and then activate or connect through the desired transport.

| Strategy | Role |
| --- | --- |
| Local configuration | Reads configured Classic servers from application configuration. |
| Windows local registry | Enumerates local COM registrations when running on Windows. |
| Remote registry | Enumerates remote OPC registrations over Windows registry protocols. |
| OPCEnum | Uses OPC ServerList / `OPCEnum.exe` via managed DCOM activation and generated proxies. |
| Factory composition | Runs multiple strategies and de-duplicates by CLSID/ProgID. |

Discovery is separate from activation. A gateway can discover through OPCEnum, enforce authentication policy, and then construct an `OpcConnectData` for the selected server.

## 10. Spec coverage

All nine OPC Classic areas targeted by the repository are implemented in the current tree.

| Area | Current support |
| --- | --- |
| DA | Managed client/server contracts, generated DCOM projections, hosting, subscriptions, browse, read/write, data-change callbacks, DA client/server samples, CTT server. |
| AE | Managed event server/client contracts, generated projections, subscriptions, event categories, condition/event models, AE client/server samples. |
| HDA | Managed historical APIs, generated projections, sync/async read/update/annotation/playback surfaces, HDA client/server samples. |
| Batch | Batch summaries, filters, state/type models, enumerations, generated projections. |
| Commands | Command metadata, state, invocation, callback projections, generated surfaces. |
| Cpx | Complex Data dictionaries, field/type descriptions, value models, generated projections. |
| DX | Source server, connection, configuration, and generated DX projections. |
| Security | OPC Security interfaces plus DCOM authentication and channel-binding integration. |
| Discovery | Local registry/configuration, remote registry, and OPCEnum discovery paths. |

XML-DA support is available through the HTTP/SOAP assembly for deployments that expose Classic data through XML-DA endpoints rather than DCOM.

## 11. Samples

The sample suite contains nine apps:

| Sample | Purpose |
| --- | --- |
| `samples/Opc.Classic.Samples.DaServer` | Managed DA server with a tag tree and hosting registration. |
| `samples/Opc.Classic.Samples.DaClient` | DA client flow using generated proxies and the managed DA abstraction. |
| `samples/Opc.Classic.Samples.AeServer` | Managed AE event source and hosting pattern. |
| `samples/Opc.Classic.Samples.AeClient` | AE subscription and event consumption pattern. |
| `samples/Opc.Classic.Samples.HdaServer` | Managed HDA historical data server. |
| `samples/Opc.Classic.Samples.HdaClient` | HDA query and playback client pattern. |
| `samples/Opc.Classic.Samples.LoopbackDemo` | In-memory generated proxy/dispatcher loopback for DA. |
| `samples/Opc.Classic.Samples.CttServer` | DA server shape for OPC CTT workflows. |
| `samples/Opc.Classic.Samples.AotCanary` | NativeAOT publish smoke test for consumer applications. |

## 12. AOT and trimming

AOT compatibility is a source-library contract. Runtime projects use strict .NET 10 analyzer settings, trimming analyzers, AOT analyzers, and warnings-as-errors. `Opc.Classic.Dcom` participates in the same strict mode.

Runtime code avoids:

- `System.Reflection.Emit`;
- runtime proxy generation;
- `Expression<T>.Compile()`;
- reflection invocation for dispatch;
- runtime string-to-type activation;
- Windows COM RCWs in portable paths.

The source-generator model is the primary mechanism for keeping DCOM projections static and trim-safe. The `Opc.Classic.Samples.AotCanary` sample is the consumer-level publish check.

## 13. Build, test, and release references

Local commands use the XML solution file:

```powershell
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx
dotnet test Opc.Classic.slnx
```

Tests use TUnit on Microsoft.Testing.Platform, property-based tests for codec/security invariants, Verify snapshots for wire-format regressions, in-memory generated-proxy tests, integration tests, and conformance-oriented fixtures. The current result set is 1253 passed / 24 skipped / 0 failed.

Release notes live in `CHANGELOG.md`. Package metadata is centralized in the source directory build props files. Public package IDs and namespaces use the `Opc.Classic.*` root.

## 14. License and governance

Opc.Classic is MIT licensed. Contributor-facing files:

| File | Role |
| --- | --- |
| `LICENSE` | MIT license text. |
| `SECURITY.md` | Vulnerability reporting and supported versions. |
| `CONTRIBUTING.md` | Build, test, style, sample, and conformance guidance. |
| `CHANGELOG.md` | Release notes. |
| `THIRD-PARTY-NOTICES.md` | Notices for third-party and OPC Foundation material. |

Preserve original notices in `COM/` and `External/`; those folders are conformance and redistribution assets.

## 15. Roadmap

The remaining 1.0.0 work is release qualification, not an architectural dependency cleanup:

1. keep the Windows compatibility matrix green across native sample servers and representative external DA/AE/HDA servers;
2. run OPC CTT workflows for managed DA server coverage where installer access is available;
3. stabilize public connection bootstrap APIs and package metadata for `1.0.0-rc.1`;
4. keep AOT canary, CTT server, and DA/AE/HDA samples aligned with the public API;
5. complete external security review inputs for authentication and packet-protection code.
