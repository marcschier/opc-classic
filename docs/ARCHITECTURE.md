# OPC Classic .NET Architecture

This document describes the current top-down architecture of the OPC Classic .NET stack. It is based on the repository layout in `Opc.Classic.slnx`, the shared build configuration under `src\`, and the protocol code under `src\Opc.Classic.*`. For the implementation sequence and open migration phases, see the session `plan.md` for the multi-phase roadmap.

## 1. Overview and cross-platform story

The repository is a cross-platform, .NET 10 implementation of OPC Classic: DA, AE, HDA, DX, Complex Data, Batch, Commands, Security, and XML-DA. The design goal is that consumer libraries remain NativeAOT-compatible and portable across Linux, macOS, and Windows. The DCOM path avoids `[ComImport]` and Windows COM activation APIs; XML-DA uses HTTP/SOAP and has no DCOM dependency.

`global.json` pins the .NET SDK to `10.0.100` with `latestFeature` roll-forward. The roadmap refers to a 13-project source target; the current `Opc.Classic.slnx` lists 12 source entries and 14 test entries. Thirteen test projects are active TUnit projects; `tests\Opc.Classic.Dcom.Tests\Opc.Classic.Dcom.Tests.csproj` is a transitional opt-out. The session baseline records 642+ passing argument-expanded tests.

All project names below are directories under `src\`.

| Project | Role |
|---|---|
| `Core` | Cross-spec primitives, NDR helpers, variants, arrays, and `ICallChannel`. |
| `Dcom` | Pure-managed MSRPC/DCOM stack; SharpInterop-derived and transitional. |
| `Da` | DA 2.05a/3.0 API, projections, and wire structs. |
| `Ae` | AE API, subscriptions, and event/status codecs. |
| `Hda` | HDA API, historical interfaces, and codecs. |
| `Dx` | DX server-to-server configuration model. |
| `Cpx` | Complex Data dictionaries and OPCBinary-style values. |
| `Batch` | Batch summaries, filters, and projections. |
| `Commands` | Commands bootstrap projections. |
| `Security` | Security facade and `IOPCSecurity*` projections. |
| `Xml` | XML-DA HTTP/SOAP client, DTOs, and serializers. |
| `Generators` | Roslyn generators for metadata and future shims. |

## 2. Transport layers

The stack separates protocol semantics from transport. Spec assemblies express DA/AE/HDA/etc. concepts in managed types and DCOM projection interfaces. Transport-specific code is isolated below them.

```text
+--------------------------------------------------------------+
| Applications / hosted services / tests                       |
+--------------------------------------------------------------+
| Managed OPC APIs: DA, AE, HDA, DX, Cpx, Batch, Commands      |
+--------------------------------------------------------------+
| Projection layer: [OpcInterface], future call shims, DTOs    |
+----------------------------+---------------------------------+
| DCOM path                  | XML-DA path                     |
| ICallChannel + NDR         | IXmlDaClient + SOAP serializers |
+----------------------------+---------------------------------+
| Opc.Classic.Dcom MSRPC      | System.Net.Http / System.Xml    |
+----------------------------+---------------------------------+
```

### DCOM

`src\Opc.Classic.Dcom\` contains the SharpInterop-derived MSRPC/DCOM implementation and is the long-term replacement for Windows COM interop. Authentication and connection lifecycle remain below generated shims. The boundary is `ICallChannel.cs`: generated code sends an IID, opnum, and NDR payload, then receives HRESULT plus response bytes.

### XML-DA

`src\Opc.Classic.Xml\HttpXmlDaClient.cs` implements `IXmlDaClient` using `HttpClient`, `text/xml`, and SOAPAction headers. `src\Opc.Classic.Xml\IXmlDaClient.cs` is spec-complete for eight operations: `GetStatus`, `Read`, `Write`, `Browse`, `Subscribe`, `SubscriptionPolledRefresh`, `SubscriptionCancel`, and `GetProperties`. Each has a serializer under `src\Opc.Classic.Xml\Serialization\`.

### Generic call channel

`ICallChannel.InvokeAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken)` is the DCOM-independent contract for call shims. It keeps NDR marshaling in generated/spec code and connection/auth concerns in `Opc.Classic.Dcom`.

## 3. NDR, VARIANT, and SAFEARRAY pipeline

NDR code lives primarily in `src\Opc.Classic.Core\Ndr\`. `NdrWriter` and `NdrReader` are `ref struct` types over caller-provided spans. They are forward-only, little-endian, and self-aligning. The pipeline is intentionally explicit: write primitives, compose OAUT values such as VARIANT/SAFEARRAY, then compose OPC structs.

```csharp
Span<byte> buffer = stackalloc byte[256];
var writer = new NdrWriter(buffer);
writer.WriteUInt32(1);
writer.WriteUnicodeStringPtr("Random.Tag");
ReadOnlySpan<byte> payload = buffer[..writer.Position];
```

| Primitive/helper | Writer/reader member | Notes |
|---|---|---|
| Scalars | `WriteInt16`, `WriteUInt32`, `ReadDouble`, etc. | Natural NDR alignment, little-endian. |
| GUID | `WriteGuid`, `ReadGuid` | DCE/NDR layout: `{uint, ushort, ushort, byte[8]}`. |
| FILETIME | `WriteFileTime`, `ReadFileTime` | Two 32-bit halves, low then high. |
| LPWSTR | `WriteUnicodeString`, `ReadUnicodeString` | Conformant + varying Unicode string with trailing NUL. |
| LPWSTR pointer | `WriteUnicodeStringPtr`, `ReadUnicodeStringPtr` | Unique pointer referent plus LPWSTR body. |
| BSTR | `WriteBstr`, `WriteNullBstr`, `ReadBstr` | OAUT `FLAGGED_WORD_BLOB` form. |
| Conformant arrays | `WriteConformant*Array`, `ReadConformant*Array` | Count-prefixed primitive arrays. |
| VARIANT | `NdrVariantExtensions.WriteVariant`, `ReadVariant` | Managed carrier is `src\Opc.Classic.Core\OpcVariant.cs`. |
| SAFEARRAY | `NdrSafeArrayExtensions.WriteSafeArray`, `ReadSafeArray` | Managed carrier is `src\Opc.Classic.Core\OpcSafeArray.cs`. |

Struct codecs are small static classes that encode one spec structure and decode it back to a managed type. Current tracked codec paths are grouped under `src\Opc.Classic.*\Ndr\`: DA has item def/result/state/VQT/attributes/properties/property/server-status codecs; AE has server-status plus `OpcEventNotificationCodec.cs`; HDA has item/time/attribute/annotation/modified-item codecs; Batch has summary and summary-filter codecs.

## 4. Source generators

`src\Opc.Classic.Generators\OpcInterfaceGenerator.cs` is the first functional generator. It injects the internal attributes `OpcInterfaceAttribute` and `OpcMethodAttribute`, scans partial interfaces, validates GUID literals, and emits a sibling partial interface containing:

```csharp
public static Guid InterfaceId { get; } = new("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
```

If methods are annotated with `[OpcMethod(opnum)]`, the same generator emits a nested `Opnums` static class and rejects duplicate opnums. The current production source has 38 `[OpcInterface]` DCOM projections across DA, AE, HDA, DX, Batch, Commands, and Security. No production DCOM method bodies are annotated yet; the generator support is in place for the later call-shim phase.

## 5. Spec coverage matrix

| Sub-spec | Managed surface | DCOM interfaces | NDR codecs | Status |
|---|---|---|---|---|
| DA | `IDaServer`, `IDaSubscription`, item/property/value types | 12 (`IOPCServer`, browse, item, sync/async, callbacks) | 8 under `src\Opc.Classic.Da\Ndr\` | Broadest DCOM/NDR surface; call shims pending. |
| AE | `IAeServer`, subscriptions, notifications, filters | 6 event server/subscription/browser/sink interfaces | Server status and event notification | Managed/event codec coverage present. |
| HDA | `IHdaServer`, time, items, annotations, aggregates | 10 server/browser/sync/async/playback/callback interfaces | 5 under `src\Opc.Classic.Hda\Ndr\` | Core historical structures represented. |
| DX | `IDxServer`, connections, source servers, state enums | `IOPCConfiguration` | None tracked | Managed configuration model and IID projection. |
| Cpx | Type dictionaries, struct fields, complex values | None | None tracked | Managed Complex Data vocabulary. |
| Batch | `OpcBatchSummary`, `OpcBatchSummaryFilter` | 4 Batch/enumeration interfaces | Summary and summary-filter | Bootstrap plus wire structs. |
| Commands | Projection-only bootstrap | 3 command interfaces | None tracked | IID coverage. |
| Security | `IOpcSecurity` | `IOPCSecurityNT`, `IOPCSecurityPrivate` | None tracked | Security facade plus projections. |
| XML-DA | `IXmlDaClient`, `HttpXmlDaClient`, DTOs, serializers | Not DCOM-based | Not NDR-based | 8/8 operations over HTTP/SOAP. |

## 6. Logging

Logging in `Opc.Classic.Dcom` uses Microsoft.Extensions.Logging. `Internal\LogHost.cs` owns the process-wide `ILoggerFactory`; `Internal\Log.cs` keeps the Serilog-shaped `Log.Logger.X(...)` surface while forwarding to `ILogger`. Phase 2G removed Serilog and left `Microsoft.Extensions.Logging.Abstractions`.

## 7. Test infrastructure

Tests inherit from `tests\Directory.Build.props`: TUnit on Microsoft.Testing.Platform, `OutputType=Exe`, warnings as errors, and non-AOT-strict settings for test code. `tests\Directory.Packages.props` centralizes TUnit 0.13.0, CsCheck, Verify.TUnit, coverlet, and Microsoft logging test packages.

| Test project | Focus |
|---|---|
| `Core` | Core primitives, FILETIME, result IDs, URL/quality/variant behavior. |
| `Da`, `Ae`, `Hda` | Managed types, IID projections, and spec codecs. |
| `Xml` | XML-DA serializers and HTTP/SOAP DTO behavior. |
| `Batch`, `Commands`, `Cpx`, `Dx`, `Security` | Per-spec bootstrap and type-model tests. |
| `Dcom.Crypto` | Hand-rolled MD4/RC4 vectors. |
| `Dcom.Logging` | ILogger shim behavior. |
| `PropertyTests` | CsCheck invariants over Core and DCOM crypto. |
| `Dcom` | Transitional legacy integration drivers; `IsTestProject=false`. |

Snapshot-style tests should snapshot byte arrays, DTOs, or formatted strings rather than `ref struct` values directly. `tests\Opc.Classic.PropertyTests\InvariantProperties.cs` uses CsCheck generators for broad invariants such as FILETIME round-trips, quality bit decomposition, URL parsing, and crypto determinism.

## 8. AOT and trimming story

`src\Directory.Build.props` sets `TargetFramework=net10.0`, nullable, warnings-as-errors, `IsAotCompatible=true`, `IsTrimmable=true`, and trim/single-file/AOT analyzers for normal source projects. It wires `src\BannedSymbols.txt` into BannedApiAnalyzers to reject Reflection.Emit, expression-tree compile, runtime string-to-type lookup, `[ComImport]`, and native COM object release APIs.

Two projects intentionally override that default: transitional DCOM and build-time Generators. New runtime code should follow the parent props.

## 9. Build and CI

The solution source of truth is `Opc.Classic.slnx`, the .NET XML solution format. Typical local commands are:

```powershell
dotnet restore Opc.Classic.slnx
dotnet build Opc.Classic.slnx
dotnet test Opc.Classic.slnx
```

`build.yml` defines the intended CI shape: Linux, macOS, and Windows matrix builds in Debug and Release, a format job, a NativeAOT canary job when `AotCanary` exists, and a Windows conformance job for native OPC sample servers when the preserved COM solution is present. The workflow uses `actions/setup-dotnet` with `global.json`.

## 10. Repository layout

| Path | Role |
|---|---|
| `src\` | Production libraries. Parent props enforce .NET 10, analyzers, AOT/trimming defaults, central generator wiring. |
| `tests\` | TUnit, property, logging, crypto, and transitional integration tests. |
| `External\` | Preserved OPC Foundation Core Components and installer artifacts used as conformance references. |
| `COM\` | Preserved native C++ OPC sample servers used by Windows conformance work. |
| `docs\` | Architecture and future DocFX/cookbook/migration content. |
| `.github\workflows\` | CI entry points for matrix builds, formatting, AOT canary, and conformance jobs. |
| `Opc.Classic.slnx` | Single repository solution. |
| `global.json` | .NET SDK pin and roll-forward policy. |

The active architecture is therefore layered but not monolithic: shared primitives in Core, transport in DCOM or XML-DA, source-generated metadata for DCOM projections, spec assemblies above that, and tests that verify DTOs, wire codecs, and invariants independently.