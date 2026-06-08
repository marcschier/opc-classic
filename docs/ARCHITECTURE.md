# Opc.Classic Architecture

Opc.Classic is a cross-platform, NativeAOT-compatible .NET 10 implementation of the OPC Classic protocol family. Runtime packages and namespaces are rooted at `Opc.Classic.*`, and the project is MIT licensed.

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

The portable runtime is pure managed code. Windows-specific features, such as local COM registry writes for native client activation, are isolated behind platform-guarded components. The repository also keeps OPC Foundation C++ sample servers in `external/redist/samples/` and redistributable inputs in `external/redist/` and spec/reference material in `external/private/docs/` as conformance assets; they are not part of the portable runtime libraries.

| Area | Current state |
| --- | --- |
| Framework | .NET 10, SDK pinned by `global.json` |
| License | MIT |
| Public namespace root | `Opc.Classic.*` |
| OPC areas | DA, AE, HDA, Batch, Commands, Cpx, DX, Security, Discovery |
| DCOM stack | Managed MSRPC/DCOM over async transports with v5.6 activation, `OpcServerListener`, per-connection PDU processing, and per-IPID object routing; cross-platform `ncacn_ip_tcp` and `ncacn_np` (RPC over SMB2) |
| Authentication | Self-contained NTLMv2, Kerberos, SPNEGO, channel binding, RFC 5056 / RFC 5929 helpers; SMB2 signing (HMAC-SHA256 / AES-CMAC) and SMB 3.x encryption (AES-128-CCM / GCM) for the SMB transport |
| Generation | Source-generated client proxies and server dispatchers: 47 dispatchers, 127 opnums |
| AOT stance | Runtime libraries are trimmable; `Opc.Classic.Dcom` runs with strict AOT/trimming analyzers enabled |
| Samples | 10 sample apps: DA/AE/HDA server+client, LoopbackDemo, CttServer, OpcSecurityServer, AotCanary; sample containers now exchange DCOM-over-IP |
| Verification | 0 build errors / 0 warnings; rc.10 sweep has 2113 passed / 12 skipped / 0 failed across 23 .NET test projects |

## 2. Assembly layout

Runtime source is organized by protocol boundary rather than by sample scenario. Every project under `src/` is listed below.

| Assembly | Role |
| --- | --- |
| `Opc.Classic.Core` | Common contracts, URLs, connection data, NDR, OAUT, `OpcVariant`, `OpcSafeArray`, HRESULT/result identifiers, testing transports, `IAuthContext`. |
| `Opc.Classic.Dcom` | Managed MSRPC/DCOM channel, activation (`IRemoteSCMActivator` + legacy `IActivation` client/server), OBJREF/OXID handling, NTLMv2, packet protection, ping, server object export, `ncacn_np` transport that wraps `Opc.Classic.Dcom.Smb`. |
| `Opc.Classic.Dcom.Kerberos` | Kerberos/SPNEGO token flow and packet protection integration. |
| `Opc.Classic.Dcom.Smb` | Minimal AOT-clean SMB2 client scoped to the named-pipe operations required by `ncacn_np`: NEGOTIATE/SESSION_SETUP/TREE_CONNECT/CREATE/READ/WRITE/IOCTL/CLOSE; SMB2 signing (HMAC-SHA256, AES-CMAC) and SMB 3.x encryption (AES-128-CCM/GCM with `TRANSFORM_HEADER`). |
| `Opc.Classic.Da` | Data Access managed APIs, DCOM projections, generated proxies/dispatchers, hosting, subscriptions, item/value models, Windows CCWs (`OpcDaServerCcw`, `OpcDaGroupCcw`, `OpcEnumConnectionsCcw`, `OpcEnumConnectionPointsCcw`, `OpcEnumOpcItemAttributesCcw`, `OpcDataCallbackProxy`). |
| `Opc.Classic.Ae` | Alarms & Events managed APIs, event categories, subscriptions, condition/event models, DCOM projections, array-heavy CCW marshaling helpers, Windows CCWs (`OpcAeServerCcw`, `OpcAeSubscriptionCcw`, `OpcAeAreaBrowserCcw`, `OpcAeEventSinkProxy`, `OpcEnumStringCcw`). |
| `Opc.Classic.Hda` | Historical Data Access APIs, time ranges, attributes, aggregates, annotations, playback/update projections, Windows CCWs (`OpcHdaServerCcw`, `OpcHdaBrowserCcw`, `OpcHdaCallbackProxy`, `OpcHdaItemMarshaler`) including sync/async update, advise, annotation, and playback surfaces. |
| `Opc.Classic.Batch` | Batch summary, filtering, enumeration, and batch-state models. |
| `Opc.Classic.Commands` | OPC Commands metadata, state, invocation, and callback surfaces. |
| `Opc.Classic.Cpx` | Complex Data dictionaries, fields, type descriptions, OPC Binary-style values, BitString codecs, and DA address-space/property integration helpers. |
| `Opc.Classic.Dx` | Data eXchange configuration, source server, and connection models. |
| `Opc.Classic.Security` | OPC Security abstractions plus channel-binding helpers and sample-server-facing ACL semantics. |
| `Opc.Classic.Discovery` | Local configuration, Windows registry, remote registry, and OPCEnum discovery strategies. |
| `Opc.Classic.Hosting` | Microsoft.Extensions.Hosting integration, CLSID/ProgID registry abstractions, and Windows COM registration. |
| `Opc.Classic.Xml` | XML-DA HTTP/SOAP DTOs, serializers, and client transport shape. |
| `Opc.Classic.Generators` | Build-time Roslyn incremental generators for `[OpcInterface]`/`[OpcMethod]` metadata, client proxies, server dispatchers, and codec tables. |
| `Opc.Classic.MigrationAnalyzer` | Roslyn analyzer that emits porting diagnostics for legacy `.NET Framework OPC .NET API` consumers migrating to `Opc.Classic.*`. |

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
        |                              |                                |
        v                              v                                v
+--------------------------+  +--------------------------+   +------------------------+
| DCOM ncacn_ip_tcp        |  | DCOM ncacn_np (SMB)      |   | XML-DA HTTP/SOAP       |
| bind / request / resp    |  | SMB2 NEGOTIATE+SESSION+  |   | HttpClient serializers |
|                          |  | TREE+CREATE pipe+R/W     |   |                        |
+--------------------------+  +--------------------------+   +------------------------+
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

`Opc.Classic.Dcom` implements the managed MSRPC/DCOM path for cross-machine Classic endpoints. The transport stack includes:

- `TcpClientTransport` and `DcomCallChannelFactory.ConnectTcpAsync(...)` for outbound TCP clients;
- `OpcServerListener` for TCP/ncacn_ip_tcp server accept loops;
- `RpcServerConnectionProcessor` for bind, alter-context, request, response, shutdown, fragmentation, ORPC envelope, and authentication-trailer PDU handling;
- `OpcObjectRegistry` for per-IPID routing so server-created groups, enumerators, and subscriptions receive calls on the right managed object;
- endpoint mapper and activation flows (`IRemoteSCMActivator` + legacy `IActivation` per MS-DCOM §2.2.18 / §3.1.2.5);
- OBJREF and OXID runtime structures;
- packet signing and sealing according to the negotiated protection level.

Generated proxies do not depend on the concrete channel. The same proxy can target an in-memory loopback channel, a test fixture, or a TCP-backed DCOM channel.

### DCOM over `ncacn_np` (RPC over SMB2)

`Opc.Classic.Dcom.Smb` implements a minimal AOT-clean SMB2 client targeting the named-pipe operations required for RPC-over-SMB transport per [MS-RPCE] §2.1.1.1: NEGOTIATE → SESSION_SETUP → TREE_CONNECT (IPC$) → CREATE (open pipe) → READ/WRITE/IOCTL with `FSCTL_PIPE_TRANSCEIVE` → CLOSE. `Opc.Classic.Dcom/Transport/NcacnNpTransport` wraps this stack behind the standard `IAsyncTransport` so any ncacn_np interface (e.g., WINREG per MS-RRP) is dialable from Linux/macOS.

Wire protection:

- **Signing**: HMAC-SHA256 (SMB 2.0.2 / 2.1) or AES-128-CMAC (SMB 3.x); signing keys derived via SMB3KDF (NIST SP800-108 counter mode).
- **Encryption**: AES-128-CCM or AES-128-GCM (SMB 3.x), negotiated via the `SMB2_ENCRYPTION_CAPABILITIES` context per MS-SMB2 §2.2.3.1.2; the encrypted message uses the 52-byte `SMB2 TRANSFORM_HEADER` per §2.2.41.

Quotas (`MaxSmb2MessageSize`, `MaxNdrPayloadSize`, `MaxNtlmMessageSize`) are exposed by `RpcTransportQuotas` for tunable bounds on inbound parsers.

See [architecture/smb-transport.md](architecture/smb-transport.md) and [architecture/activation-transports.md](architecture/activation-transports.md) for the detailed phase ledger.

### XML-DA over HTTP/SOAP

`Opc.Classic.Xml` implements the XML-DA 1.01 HTTP/SOAP shape. XML-DA is independent of DCOM but shares core concepts such as item IDs, result IDs, quality, timestamps, and value conversion.

## 4. Activation and object lifetime

Managed activation has two complementary paths: portable DCOM activation over the managed MSRPC stack, and Windows SCM activation through raw-vtable COM-callable wrappers (CCWs) for native OPC clients.

| Component | Role |
| --- | --- |
| `IRemoteSCMActivator` | Source-generated DCOM projection for `RemoteGetClassObject` and `RemoteCreateInstance`. |
| `RemoteSCMActivatorServer` | Server-side v5.6 activation implementation for managed class factories and object export. |
| `IActivation` (legacy) | Shipped client + server implementation of MS-DCOM §2.2.18.3 `RemoteActivation` for XP / Server-2003 / pre-W2K3-SP1 interop, sharing the authentication policy of the modern activator. |
| `ClassFactoryRegistry` | Maps CLSIDs and ProgIDs to managed factories. |
| `LocalCoClass` / OXID runtime | Exports managed objects as DCOM object references and maintains object lifetime. |
| `OpcServerListener` + `OpcObjectRegistry` | Accept inbound TCP DCOM calls and route each IPID to the exported server, group, enumerator, or subscription object. |
| Windows CCWs | Raw-vtable CCWs without `[ComImport]` or RCWs. DA covers `IOPCServer`, `IOPCGroupStateMgt(2)`, `IOPCItemMgt`, `IOPCSyncIO(2)`, `IOPCAsyncIO2/3` (including `WriteVqt`), `IConnectionPoint(Container)` with `IEnumConnections` + `IEnumConnectionPoints`, callback + item-attribute enumerators, and VARIANT/SAFEARRAY/BSTR marshaling. AE covers `IOPCEventServer`, `IOPCEventSubscriptionMgt` (filter set/get + state + refresh + returned attributes), `IOPCEventAreaBrowser`, `IOPCEventSink` callback delivery with `ONEVENTSTRUCT[]`, array-heavy query/translate/state/condition operations, and a reusable `IEnumString`. HDA covers `IOPCHDA_Server`, `IOPCHDA_Browser` from `CreateBrowse`, `IOPCHDA_SyncRead` / `IOPCHDA_AsyncRead` read methods, sync/async update, annotation insert, advise, playback, and `IOPCHDA_DataCallback`. |
| Ping support | Implements DCOM keepalive semantics for exported objects and client sessions. |

The portable path is the normal cross-platform route. Windows native clients still require appropriate CLSID/ProgID registration and platform setup, but the implementation behind the CCW remains managed and AOT-oriented.

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
| `OpcDaServerHost` / AE / HDA hosts | Bind managed implementations to generated dispatchers, `OpcServerListener`, and runtime endpoints. |
| `IOpcDaServer` | Managed DA server contract implemented by application code. |
| `IOpcAddressSpace` | DA browse/property abstraction implemented by `FlatHierarchicalNamespace`, `InMemoryAddressSpace`, `DefaultBrowseServerAddressSpace`, `DefaultBrowse`, and `DefaultItemProperties`. |
| `IOpcDataCallbackSink` | Unified outbound callback abstraction used by both cross-platform `IOpcInterfaceRef` sinks and Windows `OpcDataCallbackProxy` sinks. |
| Data-change publishers | Bridge managed subscription updates to `IOPCDataCallback` callbacks. |

AE and HDA hosting use the same pattern with their per-spec server contracts and sample applications.

## 8. Authentication and packet protection

Authentication is behind the `IAuthContext` abstraction so transports and generated code do not depend on a concrete security mechanism. Transports that need the negotiated session key (e.g., SMB2 signing/encryption) consume `IAuthSessionKeyProvider` as an optional capability.

| Mechanism | Current state |
| --- | --- |
| NTLMv2 | Self-contained implementation with MIC handling, extended session security, signing, and sealing. Direct `SIGNATURE_BLOCK` formation and mismatch tests against MS-NLMP §3.4.4 / §3.4.5 vectors. |
| Kerberos | Kerberos AP-REQ/AP-REP and packet protection for supported encryption types. |
| SPNEGO | RFC 4178 / [MS-SPNG] negotiation wrapper for Kerberos and NTLM tokens. |
| Channel binding | RFC 5056 and RFC 5929 helpers, including `tls-server-end-point` binding material. |
| SMB2 signing | HMAC-SHA256 (SMB 2.0.2/2.1) or AES-128-CMAC (SMB 3.x); session keys derived via SMB3KDF per MS-SMB2 §3.1.4.1 + §3.1.5.1. |
| SMB 3.x encryption | AES-128-CCM or AES-128-GCM per MS-SMB2 §3.1.4.3 + §2.2.41 `TRANSFORM_HEADER`. |
| DCOM hardening | Defaults align with KB5004442: NTLMv2 or Kerberos plus packet integrity. |
| Decoder bounds | `RpcTransportQuotas` exposes tunable maximums for NDR / NTLMSSP / SMB2 messages, with fuzz-tested rejection of oversized or malformed input. |
| Secret hygiene | Password-derived intermediate buffers go through `SensitiveBufferPool` with `CryptographicOperations.ZeroMemory` on release. |

`OpcConnectData` expands `OpcProtectionLevel.Default` to `Integrity`. Use `Privacy` when you need confidentiality in addition to integrity. Use `Connect` only for isolated compatibility exceptions where the target cannot accept packet integrity.

The security implementation follows the relevant wire specifications: [MS-DCOM], [MS-RPCE], [MS-NLMP], [MS-KILE], [MS-SPNG], [MS-SMB2], RFC 4178, RFC 5056, RFC 5929, RFC 4121, RFC 4757, RFC 3962, RFC 8009, and NIST SP800-108.

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
| DA | Managed client/server contracts, generated DCOM projections, hosting, subscriptions, `IOpcAddressSpace`-backed browse/properties, read/write, data-change callbacks, Windows CCWs, DA client/server samples, CTT server. |
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

The sample suite contains 10 apps:

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
| `samples/Opc.Classic.Samples.OpcSecurityServer` | OPC Security reference server and ACL semantics. |
| `samples/Opc.Classic.Samples.AotCanary` | NativeAOT publish smoke test for consumer applications. |

## 12. Related architecture documents

The `docs/architecture/` folder contains the diagram suite and topic-deep narratives that support this overview. GitHub renders Mermaid fenced blocks directly in Markdown; keep diagrams small, prefer short labels, use `<br/>` for label line breaks, and use standard Mermaid arrows such as `-->`, `->>`, and `-->>`.

### Topic-deep architecture notes

- [`architecture/activation-transports.md`](architecture/activation-transports.md) — TCP vs SMB activation paths; legacy `IActivation` interop matrix; client/server status per transport.
- [`architecture/smb-transport.md`](architecture/smb-transport.md) — SMB2 connection lifecycle, `ncacn_np` wire-up, signing, encryption, PCAP fixtures, and phase ledger.
- [`architecture/dcom-container-networking.md`](architecture/dcom-container-networking.md) — container-network considerations for DCOM-over-IP between sample servers and clients.

### Diagram suite

The diagrams describe the current `Opc.Classic.*` architecture: source-generated client proxies and server dispatchers, `ICallChannel` with in-memory and DCOM implementations, channel-level NTLM/Kerberos/SPNEGO/CBT, NativeAOT-compatible libraries, and coverage across DA, AE, HDA, Batch, Commands, Security, DX, Cpx, and Discovery.

1. [`architecture/01-high-level-architecture.md`](architecture/01-high-level-architecture.md) — top-level client, generated proxy, `ICallChannel`, DCOM/in-memory channels, NDR, `TcpClientTransport`, and managed listener shape.
2. [`architecture/02-call-shim-flow.md`](architecture/02-call-shim-flow.md) — outbound generated proxy call sequence for `IOPCServer::GetStatus`.
3. [`architecture/03-server-dispatch-flow.md`](architecture/03-server-dispatch-flow.md) — inbound TCP listener, `RpcServerConnectionProcessor`, optional `OpcObjectRegistry`, `OpcDaServerDispatcher`, and `IOpcDaServer` routing.
4. [`architecture/04-ntlm-handshake.md`](architecture/04-ntlm-handshake.md) — NTLMSSP NEGOTIATE, CHALLENGE, AUTHENTICATE, and CBT computation.
5. [`architecture/05-kerberos-handshake.md`](architecture/05-kerberos-handshake.md) — Kerberos AP-REQ/AP-REP mutual authentication and GSS-API protection seam.
6. [`architecture/06-spnego-negotiation.md`](architecture/06-spnego-negotiation.md) — NegTokenInit, NegTokenResp, mechanism selection, and MIC handling.
7. [`architecture/07-discovery-flow.md`](architecture/07-discovery-flow.md) — OPCEnum / `IOPCServerList` and remote-registry discovery strategies.
8. [`architecture/08-source-generator-pipeline.md`](architecture/08-source-generator-pipeline.md) — attributes, Roslyn generators, codec table, and emitted proxies and dispatchers.
9. [`architecture/09-subscription-data-flow.md`](architecture/09-subscription-data-flow.md) — DA group, item activation, sampling, `IOpcDataCallbackSink`, and callback delivery.
10. [`architecture/10-aot-trimming-shape.md`](architecture/10-aot-trimming-shape.md) — AOT-visible static code, analyzers, banned APIs, DCOM channel shape, and canary publish.
11. [`architecture/ndr-pointer-marshaling.md`](architecture/ndr-pointer-marshaling.md) — NDR unique-pointer shape model, `[OpcUniquePointer]` and `[return: OpcUniquePointer]`, real-DCOM wire compatibility.
