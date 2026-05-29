# AOT and trimming for Opc.Classic applications

Opc.Classic is designed for NativeAOT-compatible libraries. That matters for plant gateways, edge containers, and small service deployments where startup time, image size, and predictable dependencies are important. It also means application code must avoid patterns that only work when the full runtime and reflection metadata are available. This tutorial shows how to publish AOT-trimmed binaries, how to use root descriptors when you really need them, and what is safe or unsafe in the Opc.Classic stack.

The repository's canary is `samples\Opc.Classic.Samples.AotCanary\`. It references `Opc.Classic.Core` and `Opc.Classic.Da`, creates `OpcUrl`, `OpcVariant`, `OpcItemState`, and round-trips `NdrOpcItemStateCodec`. The CI contract is zero AOT/trimming warnings for the canary.

## Prerequisites

- .NET 10 SDK.
- An application that builds in Release.
- Access to the target OS/architecture or a container build that targets it.
- Understanding that trimming warnings are correctness warnings, not style warnings.

## What you'll learn

- How to enable `PublishAot=true` and trimming warnings as errors.
- How to test against the repository AOT canary.
- Which Opc.Classic patterns are trimming-safe.
- Which reflection-heavy patterns are unsafe.
- How to use trimming root descriptors sparingly.
- How to package AOT binaries in containers.

## Run the canary first

From the repository root:

```powershell
dotnet publish samples\Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

The canary project file is intentionally small:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <PublishAot>true</PublishAot>
    <RootNamespace>Opc.Classic.Samples.AotCanary</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Opc.Classic.Core\Opc.Classic.Core.csproj" />
    <ProjectReference Include="..\..\src\Opc.Classic.Da\Opc.Classic.Da.csproj" />
  </ItemGroup>
</Project>
```

The program exercises representative AOT-safe pieces:

```csharp
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

var url = OpcUrl.Parse("opcda://localhost/Matrikon.OPC.Simulation.1");
var variant = OpcVariant.FromInt32(42);

var state = new OpcItemState(
    ClientHandle: 7,
    Timestamp: DateTimeOffset.UtcNow,
    Quality: OpcQuality.Good,
    Value: OpcVariant.FromDouble(3.14));

Span<byte> buffer = stackalloc byte[256];
var writer = new NdrWriter(buffer);
NdrOpcItemStateCodec.Write(ref writer, state);

var reader = new NdrReader(buffer[..writer.Position]);
OpcItemState roundTripped = NdrOpcItemStateCodec.Read(ref reader);
Console.WriteLine(roundTripped.Value.AsDouble());
```

If the canary fails, fix the library or environment before blaming your app.

## Enable AOT in your app

A production worker project can enable AOT like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <PublishAot>true</PublishAot>
    <IsAotCompatible>true</IsAotCompatible>
    <IsTrimmable>true</IsTrimmable>
    <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
    <EnableAotAnalyzer>true</EnableAotAnalyzer>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" />
    <ProjectReference Include="..\src\Opc.Classic.Core\Opc.Classic.Core.csproj" />
    <ProjectReference Include="..\src\Opc.Classic.Da\Opc.Classic.Da.csproj" />
  </ItemGroup>
</Project>
```

Publish for a concrete runtime identifier:

```bash
dotnet publish src/MyOpcGateway/MyOpcGateway.csproj \
  -c Release \
  -r linux-x64 \
  -p:PublishAot=true \
  -p:TreatWarningsAsErrors=true
```

NativeAOT is RID-specific. Build `linux-x64` and `linux-arm64` separately or through a multi-arch Docker build.

## What is safe

### Source-generated proxies

The DCOM projection model uses attributes and source generators at build time. Generated proxies call `ICallChannel` and static codecs. No runtime reflection is needed to discover opnums or marshal parameters.

### Explicit codecs

`NdrWriter`, `NdrReader`, and static codecs such as `NdrOpcItemStateCodec` are trimming-safe. They read and write known fields explicitly.

### Plain DTOs and records

Types such as `Item`, `ItemValueResult`, `SubscriptionState`, `EventNotification`, `HdaReadResult`, and `OpcServerStatus` are normal managed DTOs. They do not require dynamic code generation.

### Microsoft.Extensions.Hosting with explicit registration

DI is safe when constructors are statically visible and types are registered explicitly:

```csharp
builder.Services.AddSingleton<MyWorker>();
builder.Services.AddHostedService(static sp => sp.GetRequiredService<MyWorker>());
```

Per-spec hosting methods use `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]` on generic server types to preserve constructors.

## What is not safe

### Reflection-based configuration

Avoid configuration systems that store arbitrary type names and call `Type.GetType(string)` or `Activator.CreateInstance(Type)` at runtime. Those APIs are banned in `src` and fragile in trimmed apps. Prefer explicit registration:

```csharp
builder.Services.AddSingleton<IDaServer, MyDaServer>();
```

If you must choose among known implementations, use a switch:

```csharp
string mode = builder.Configuration["Opc:Mode"] ?? "Loopback";
switch (mode)
{
    case "Loopback":
        builder.Services.AddSingleton<IDaServer, LoopbackDaServer>();
        break;
    case "Production":
        builder.Services.AddSingleton<IDaServer, ProductionDaServer>();
        break;
    default:
        throw new InvalidOperationException($"Unknown OPC mode '{mode}'.");
}
```

### Runtime code generation

Do not use `Reflection.Emit`, expression compilation, dynamic proxy libraries, or mocking libraries that emit IL in production code. They are incompatible with NativeAOT and banned by the repository rules.

### Unbounded serialization reflection

JSON or XML serializers that discover types dynamically can be trimmed away. Use source-generated serializers or explicit DTOs if your app configuration or API layer needs serialization.

## Root descriptors

A trimming root descriptor keeps specific members even if the trimmer cannot see them statically. Use it as a last resort for application code you cannot rewrite immediately.

`ILLink.Descriptors.xml`:

```xml
<linker>
  <assembly fullname="MyOpcGateway">
    <type fullname="MyOpcGateway.LegacyConfigType" preserve="public-constructors" />
  </assembly>
</linker>
```

Project file:

```xml
<ItemGroup>
  <TrimmerRootDescriptor Include="ILLink.Descriptors.xml" />
</ItemGroup>
```

Root descriptors are not a substitute for design. They preserve code and metadata, increasing size and hiding dynamic patterns. Prefer explicit registration and source generation.

## Validate dependencies

Third-party dependencies can break AOT even if Opc.Classic is clean. Watch for:

- dynamic proxy/mocking libraries in runtime projects;
- serializers without source generation;
- plug-in systems that load assemblies by name;
- Windows-only COM interop packages;
- native libraries not available for your target RID.

Tests and samples are allowed to use more flexible tooling, but production `src` libraries are strict. Keep that distinction in your app too.

## Container packaging

AOT binaries can use runtime-deps images:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/MyOpcGateway/MyOpcGateway.csproj \
    -c Release -r linux-x64 -p:PublishAot=true -p:TreatWarningsAsErrors=true \
    -o /out

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /out/ ./
USER $APP_UID
ENTRYPOINT ["./MyOpcGateway"]
```

Do not copy the SDK into the runtime image. Do not bake Kerberos keytabs into the image; mount them as secrets as described in [03-cross-platform-deployment.md](03-cross-platform-deployment.md). The repository sample Dockerfiles and Compose environment variables are summarized in [../../samples/README.docker.md](../../samples/README.docker.md).

## AOT troubleshooting

### IL2026 or IL3050 warning

A method requires unreferenced code or dynamic code. Find the call path. Replace reflection with explicit code or source generation. If the warning is in a dependency, check for an AOT-compatible package version.

### Missing constructor at runtime

The trimmer removed a constructor used by DI or reflection. Prefer explicit generic registration or add the appropriate `DynamicallyAccessedMembers` annotation in library code. In app code, a root descriptor can unblock you temporarily.

### Works framework-dependent, fails AOT

Look for hidden reflection, dynamic serialization, culture/globalization differences, or native dependencies. Reproduce with `dotnet publish -p:PublishAot=true` locally and keep the output logs.

## Production checklist

- Canary publish passes with zero warnings.
- Application publish passes with zero warnings.
- Release binary runs on the target OS/architecture.
- Health check performs an authenticated OPC status call.
- Logs include version, target URL, auth mode, and protection level.
- Keytabs and certificates are mounted as secrets, not embedded.
- The deployment process builds one artifact per RID.

## Configuration binding in trimmed apps

`Microsoft.Extensions.Configuration` is fine in AOT apps when you bind explicitly or read known keys. Problems appear when configuration contains arbitrary type names or relies on reflection-heavy object graphs. Prefer small option records and manual parsing for the OPC boundary:

```csharp
public sealed record OpcEndpointOptions(
    string Url,
    string AuthMode,
    string ProtectionLevel,
    int TimeoutSeconds);

static OpcEndpointOptions ReadOpcOptions(IConfiguration configuration) => new(
    Url: configuration["Opc:Url"] ?? throw new InvalidOperationException("Missing Opc:Url"),
    AuthMode: configuration["Opc:AuthMode"] ?? "NtlmV2",
    ProtectionLevel: configuration["Opc:ProtectionLevel"] ?? "Integrity",
    TimeoutSeconds: int.TryParse(configuration["Opc:TimeoutSeconds"], out int seconds) ? seconds : 30);
```

This code is boring and trimming-safe. It also makes validation errors explicit. If you use source-generated configuration binding or options validation, keep the generator output in the build and treat warnings as errors.

## Native libraries and globalization

NativeAOT links differently from framework-dependent apps. If your application depends on native drivers, vendor SDKs, or platform libraries outside Opc.Classic, verify they exist for every target RID. A binary that publishes for `linux-x64` can still fail at startup if a native dependency is missing from the image.

Globalization can also matter for OPC. LCIDs appear in DA group state and error text. Keep server protocol values culture-invariant, and convert localized text at the UI boundary. Do not parse numbers from OPC item values using current culture unless the server explicitly returns strings intended for humans.

## Size and startup measurement

AOT is not automatically better for every workload. Measure binary size, container size, cold start, steady-state memory, and throughput. For long-running gateways, startup may matter less than diagnostics and maintainability. For serverless-style bridges or short-lived tools, NativeAOT startup can be a major win. Keep both framework-dependent and AOT publish profiles available until production data proves the right default.

## Release gate

Make AOT publish a release gate, not a best-effort command. A practical pipeline stage publishes the canary, publishes your application for each RID, runs `--version` or a smoke command, and starts the container long enough for a health endpoint to answer. Store publish logs as artifacts so trimming warnings can be reviewed even when they fail the build.

If a new dependency introduces warnings, stop and evaluate it. Adding a root descriptor may be acceptable for an application-level compatibility bridge, but it should be tracked with an issue and removed when a trimming-safe alternative exists.

## Library-author guidance for application teams

If your team adds internal libraries on top of Opc.Classic, hold them to the same AOT expectations. Avoid APIs that accept `Type` and construct objects dynamically. Prefer generic methods, explicit factories, and source-generated registries. If a library must preserve constructors for DI, annotate the generic parameter the same way the hosting extensions do with `DynamicallyAccessedMembers`.

Keep reflection-heavy convenience helpers out of shared runtime libraries. It is acceptable for test projects, migration tools, or design-time generators to use reflection; it is not acceptable for the library that every gateway carries into NativeAOT publish. This separation mirrors the repository: production `src` projects are strict, while tests and samples can be more flexible.

## Comparing publish outputs

Track output size over time. Store the size of the executable, total publish directory, and container image in CI artifacts. A sudden size increase often means a dependency rooted extra metadata or brought in globalization/native assets. Size is not the only goal, but unexplained growth deserves review.

Also measure startup and first OPC call. NativeAOT usually improves startup, but first call can still be dominated by DNS, Kerberos, endpoint mapping, or server activation. Separate process startup metrics from protocol readiness metrics so you know what improved.

## Fallback plan

If a dependency blocks AOT late in a release, you can ship a framework-dependent build while keeping AOT as a tracked requirement. Do not hide the warning and call the AOT build done. Document the blocker, the affected RID, and the plan to remove or replace the dependency. The worst outcome is a build that appears AOT-safe but fails only after trimming removes required metadata.

## Keeping AOT healthy over time

AOT compatibility can regress quietly when a team adds a package for an unrelated feature. Keep a small AOT smoke test in the application repository, just as Opc.Classic keeps `Opc.Classic.Samples.AotCanary`. The smoke test should parse configuration, construct the OPC connection options, encode/decode at least one representative DTO, and start the host enough to validate DI. It does not need to connect to a real server to catch most trimming problems.

Review dependencies during upgrades. Read release notes for trimming annotations, NativeAOT support, and source-generated alternatives. Prefer packages that test AOT themselves. If a dependency is used only for a convenience feature, consider keeping it out of the gateway process and doing that work in a separate service.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Debug versus release behavior

Always reproduce AOT issues from a published Release output, not from `dotnet run`. Debug builds keep metadata and runtime behavior that NativeAOT removes. If a bug appears only in the published binary, compare configuration files, current directory, environment variables, globalization settings, and file permissions before changing protocol code. Many AOT "bugs" are actually deployment differences exposed by a smaller runtime image.

## Smoke-test command design

Add a command-line switch such as `--smoke-test` that starts configuration, validates DI, constructs representative OPC DTOs, and exits without connecting to production. This gives CI and container platforms a fast way to prove the published binary is coherent before deployment. Rerun the smoke test after every SDK, package, base-image, or compiler upgrade, and keep the expected output in release documentation.

## Next steps

- Tune hot paths with [08-performance-tuning.md](08-performance-tuning.md).
- Deploy images with [03-cross-platform-deployment.md](03-cross-platform-deployment.md).
- Diagnose publish/runtime failures with [09-troubleshooting-and-diagnostics.md](09-troubleshooting-and-diagnostics.md).

## References

- Repository canary: `samples\Opc.Classic.Samples.AotCanary\`.
- Repository rules: `src\Directory.Build.props` and `src\BannedSymbols.txt`.
- [../ARCHITECTURE.md](../ARCHITECTURE.md) for source-generated proxy and explicit codec design.

