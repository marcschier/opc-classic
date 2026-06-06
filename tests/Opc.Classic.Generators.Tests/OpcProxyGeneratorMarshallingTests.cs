//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Opc.Classic.Ae;
using Opc.Classic.Da;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Generators.Tests;

[OpcInterface("33333333-4444-5555-6666-777777777778")]
[GenerateOpcProxy]
public partial interface IMarshalRoundTrip
{
    [OpcMethod(8)]
    Task<int> SomeMethodAsync(int id, string name, CancellationToken ct);
}

[OpcInterface("33333333-4444-5555-6666-777777777780")]
[GenerateOpcProxy]
public partial interface IRefOutRoundTrip
{
    [OpcMethod(9)]
    Task<int> AdjustAsync(int id, ref int current, out string name, CancellationToken ct);
}

public sealed class OpcProxyGeneratorMarshallingTests
{
    private const string SampleSource = """
        using Opc.Classic;
        using Opc.Classic.Ae;
        using Opc.Classic.Da;
        using Opc.Classic.Generators;
        using System;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Test;

        public sealed class UnknownPayload { }

        [OpcInterface("33333333-4444-5555-6666-777777777771")]
        [GenerateOpcProxy]
        public partial interface IChildObject
        {
        }

        [OpcInterface("33333333-4444-5555-6666-777777777777")]
        [GenerateOpcProxy]
        public partial interface IMarshalTest
        {
            [OpcMethod(3)] Task<int> ReadCountAsync(int id, CancellationToken ct);
            [OpcMethod(4)] Task<string> GetNameAsync(int handle, CancellationToken ct);
            [OpcMethod(5)] Task WriteAsync(int handle, double value);
            [OpcMethod(6)] Task<Guid> GetServerIdAsync();
            [OpcMethod(7)] Task<OpcVariant> EchoVariantAsync(OpcVariant variant);
            [OpcMethod(8)] Task<int> WriteItemStateAsync(OpcItemState state);
            [OpcMethod(9)] Task<OpcConditionState> GetConditionStateAsync();
            [OpcMethod(10)] Task<UnknownPayload> WithUnknownAsync(UnknownPayload payload);
            [OpcMethod(11)] Task<OpcItemDef> MixedAsync(int id, OpcVariant value);
            [OpcMethod(12)] Task<OpcSafeArray> EchoSafeArrayAsync(OpcSafeArray value);
            [OpcMethod(13)] Task<int> AdjustAsync(int id, ref int current, out string name);
            [OpcMethod(14)] Task<IChildObject> GetChildAsync();
            [OpcMethod(15)] [OpcGenerateMultiOutRecord] Task GetPairAsync(out int count, out string name);
        }
        """;

    private const string AeStatusSource = """
        using Opc.Classic;
        using Opc.Classic.Generators;
        using System.Threading.Tasks;

        namespace Opc.Classic.Ae;

        [OpcInterface("33333333-4444-5555-6666-777777777779")]
        [GenerateOpcProxy]
        public partial interface IAeMarshalTest
        {
            [OpcMethod(1)] Task<OpcServerStatus> GetStatusAsync();
        }
        """;

    [Test]
    public async Task ReadCountAsync_emits_int32_request_and_response_marshalling()
    {
        string method = GeneratedMethodSection("ReadCountAsync");

        await Assert.That(method).Contains("global::System.Buffers.ArrayPool<byte>.Shared.Rent");
        await Assert.That(method).Contains("WriteInt32(id)");
        await Assert.That(method).Contains("ReadInt32()");
    }

    [Test]
    public async Task GetNameAsync_emits_int32_request_and_lpwstr_response_marshalling()
    {
        string method = GeneratedMethodSection("GetNameAsync");

        await Assert.That(method).Contains("WriteInt32(handle)");
        await Assert.That(method).Contains("ReadUnicodeStringPtr");
    }

    [Test]
    public async Task WriteAsync_emits_arguments_without_response_reader()
    {
        string method = GeneratedMethodSection("WriteAsync");

        await Assert.That(method).Contains("WriteInt32(handle)");
        await Assert.That(method).Contains("WriteDouble(value)");
        await Assert.That(method.Contains("NdrReader", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task GetServerIdAsync_emits_guid_response_marshalling()
    {
        string method = GeneratedMethodSection("GetServerIdAsync");

        await Assert.That(method).Contains("ReadGuid()");
    }

    [Test]
    public async Task EchoVariantAsync_emits_variant_request_and_response_marshalling()
    {
        string method = GeneratedMethodSection("EchoVariantAsync");

        await Assert.That(method).Contains("global::Opc.Classic.Ndr.NdrVariantExtensions.WriteVariant(ref __opcWriter, variant)");
        await Assert.That(method).Contains("global::Opc.Classic.Ndr.NdrVariantExtensions.ReadVariant(ref __opcReader)");
    }

    [Test]
    public async Task WriteItemStateAsync_emits_item_state_codec_and_int32_response_marshalling()
    {
        string method = GeneratedMethodSection("WriteItemStateAsync");

        await Assert.That(method).Contains("global::Opc.Classic.Da.Ndr.NdrOpcItemStateCodec.Write(ref __opcWriter, state)");
        await Assert.That(method).Contains("ReadInt32()");
    }

    [Test]
    public async Task GetConditionStateAsync_emits_condition_state_response_codec()
    {
        string method = GeneratedMethodSection("GetConditionStateAsync");

        await Assert.That(method).Contains("global::Opc.Classic.Ae.Ndr.NdrOpcConditionStateCodec.Read(ref __opcReader)");
    }

    [Test]
    public async Task WithUnknownAsync_falls_back_to_empty_payload_placeholder()
    {
        string method = GeneratedMethodSection("WithUnknownAsync");

        await Assert.That(method).Contains("global::System.ReadOnlyMemory<byte>.Empty");
        await Assert.That(method).Contains("return default!;");
        await Assert.That(method.Contains("NdrVariantExtensions", StringComparison.Ordinal)).IsFalse();
        await Assert.That(method.Contains("NdrOpcItemStateCodec", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task MixedAsync_emits_primitive_variant_and_item_def_marshalling()
    {
        string method = GeneratedMethodSection("MixedAsync");

        await Assert.That(method).Contains("WriteInt32(id)");
        await Assert.That(method).Contains("global::Opc.Classic.Ndr.NdrVariantExtensions.WriteVariant(ref __opcWriter, value)");
        await Assert.That(method).Contains("global::Opc.Classic.Da.Ndr.NdrOpcItemDefCodec.Read(ref __opcReader)");
    }

    [Test]
    public async Task EchoSafeArrayAsync_emits_safe_array_request_and_response_marshalling()
    {
        string method = GeneratedMethodSection("EchoSafeArrayAsync");

        await Assert.That(method).Contains("global::Opc.Classic.Ndr.NdrSafeArrayExtensions.WriteSafeArray(ref __opcWriter, value)");
        await Assert.That(method).Contains("global::Opc.Classic.Ndr.NdrSafeArrayExtensions.ReadSafeArray(ref __opcReader)");
    }

    [Test]
    public async Task AdjustAsync_emits_ref_request_and_out_response_marshalling()
    {
        string method = GeneratedMethodSection("AdjustAsync");

        await Assert.That(method).Contains("WriteInt32(id)");
        await Assert.That(method).Contains("WriteInt32(current)");
        await Assert.That(method).Contains("ReadInt32()");
        await Assert.That(method).Contains("name = __opcDecoded.Name;");
        await Assert.That(method.Contains("WriteUnicodeStringPtr(name)", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task InterfaceReturn_emits_objref_decode_handle()
    {
        string method = GeneratedMethodSection("GetChildAsync");

        // DR10 changed the proxy generator to wrap an out IOpcInterface return
        // value in a generated sub-proxy (e.g. IChildObjectClientProxy) so the
        // caller can immediately invoke methods on it. The wire decode still
        // routes through OpcMInterfacePointerCodec which yields the OBJREF
        // (DCOM MInterfacePointer wrapping), and the proxy then constructs
        // an IChildObjectClientProxy bound to the registered IPID.
        await Assert.That(method).Contains("global::Opc.Classic.Dcom.OpcMInterfacePointerCodec.Read(ref __opcReader)");
        await Assert.That(method).Contains("global::Test.IChildObjectClientProxy");
    }

    [Test]
    public async Task MultiOutMethod_emits_generated_record_and_assignments()
    {
        GeneratorDriverRunResult result = RunGenerator(SampleSource, out Compilation outputCompilation, out _);
        ThrowIfCompilationHasErrors(outputCompilation);
        string generated = GeneratedProxySource(result);
        string method = MethodSection(generated, "GetPairAsync");

        await Assert.That(generated).Contains("public sealed record GetPairAsyncResult(int Count, string Name);");
        await Assert.That(method).Contains("return new GetPairAsyncResult(__opcResponseValue0, __opcResponseValue1);");
        await Assert.That(method).Contains("count = __opcDecoded.Count;");
        await Assert.That(method).Contains("name = __opcDecoded.Name;");
    }

    [Test]
    public async Task AeStatusMethod_emits_event_server_status_codec()
    {
        string method = GeneratedMethodSection("GetStatusAsync", AeStatusSource, "IAeMarshalTest");

        await Assert.That(method).Contains("global::Opc.Classic.Ae.Ndr.NdrOpcEventServerStatusCodec.Read(ref __opcReader)");
    }

    [Test]
    public async Task Proxy_round_trip_encodes_request_payload_and_decodes_response_payload()
    {
        byte[] expectedPayload = EncodeRequest(42, "hello");
        ReadOnlyMemory<byte> responsePayload = EncodeInt32(7);
        byte[]? observedPayload = null;
        Guid observedInterfaceId = Guid.Empty;
        int observedOpnum = -1;
        CancellationToken observedCancellationToken = default;
        var channel = new InMemoryCallChannel((interfaceId, opnum, requestPayload, cancellationToken) =>
        {
            observedInterfaceId = interfaceId;
            observedOpnum = opnum;
            observedCancellationToken = cancellationToken;
            observedPayload = requestPayload.ToArray();
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });
        var proxy = new IMarshalRoundTripClientProxy(channel);

        int result = await proxy.SomeMethodAsync(42, "hello", CancellationToken.None);

        await Assert.That(result).IsEqualTo(7);
        await Assert.That(observedInterfaceId).IsEqualTo(IMarshalRoundTrip.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IMarshalRoundTrip.Opnums.SomeMethodAsync);
        await Assert.That(observedCancellationToken).IsEqualTo(CancellationToken.None);
        await Assert.That(observedPayload is not null).IsTrue();
        await Assert.That(Convert.ToHexString(observedPayload!)).IsEqualTo(Convert.ToHexString(expectedPayload));
    }

    [Test]
    public async Task Proxy_round_trip_assigns_ref_and_out_response_values()
    {
        byte[] expectedPayload = EncodeRefOutRequest(5, 41);
        ReadOnlyMemory<byte> responsePayload = EncodeRefOutResponse(7, 42, "updated");
        byte[]? observedPayload = null;
        var channel = new InMemoryCallChannel((_, _, requestPayload, _) =>
        {
            observedPayload = requestPayload.ToArray();
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });
        var proxy = new IRefOutRoundTripClientProxy(channel);
        int current = 41;

        int result = await proxy.AdjustAsync(5, ref current, out string name, CancellationToken.None);

        await Assert.That(result).IsEqualTo(7);
        await Assert.That(current).IsEqualTo(42);
        await Assert.That(name).IsEqualTo("updated");
        await Assert.That(observedPayload is not null).IsTrue();
        await Assert.That(Convert.ToHexString(observedPayload!)).IsEqualTo(Convert.ToHexString(expectedPayload));
    }

    private static string GeneratedMethodSection(string methodName, string? source = null, string interfaceTypeName = "IMarshalTest")
    {
        GeneratorDriverRunResult result = RunGenerator(source ?? SampleSource, out Compilation outputCompilation, out _);
        ThrowIfCompilationHasErrors(outputCompilation);
        return MethodSection(GeneratedProxySource(result, interfaceTypeName), methodName);
    }

    private static string GeneratedProxySource(GeneratorDriverRunResult result, string interfaceTypeName = "IMarshalTest") =>
        result.Results.SelectMany(static generator => generator.GeneratedSources)
            // Pick the requested interface's proxy specifically: adding
            // [GenerateOpcProxy] to the test's IChildObject (so out IChildObject
            // params can construct a sub-proxy per DR10) made multiple
            // .OpcProxy.g.cs files match.
            .Single(generated =>
                generated.HintName.EndsWith(".OpcProxy.g.cs", StringComparison.Ordinal)
                && generated.HintName.Contains(interfaceTypeName, StringComparison.Ordinal))
            .SourceText.ToString();

    private static void ThrowIfCompilationHasErrors(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        }
    }

    private static string MethodSection(string generated, string methodName)
    {
        int methodNameIndex = generated.IndexOf(methodName + "(", StringComparison.Ordinal);
        if (methodNameIndex < 0)
        {
            throw new InvalidOperationException($"Generated method '{methodName}' was not found.");
        }

        int methodStart = generated.LastIndexOf("\n", methodNameIndex, StringComparison.Ordinal);
        methodStart = methodStart < 0 ? 0 : methodStart + 1;

        int nextMethod = generated.IndexOf("\n        public ", methodNameIndex + methodName.Length, StringComparison.Ordinal);
        int methodEnd = nextMethod >= 0
            ? nextMethod
            : generated.IndexOf("\n    }", methodNameIndex, StringComparison.Ordinal);

        if (methodEnd < 0)
        {
            methodEnd = generated.Length;
        }

        return generated.Substring(methodStart, methodEnd - methodStart);
    }

    private static GeneratorDriverRunResult RunGenerator(string source, out Compilation outputCompilation, out ImmutableArray<Diagnostic> driverDiagnostics)
    {
        var compilation = CreateCompilation(source);
        ISourceGenerator[] generators =
        [
            new OpcInterfaceGenerator().AsSourceGenerator(),
            new OpcProxyGenerator().AsSourceGenerator(),
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators, parseOptions: ParseOptions());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out driverDiagnostics);
        return driver.GetRunResult();
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create(
            assemblyName: "OpcProxyGeneratorMarshallingTestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions())],
            references: References(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static CSharpParseOptions ParseOptions() => CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

    private static IEnumerable<MetadataReference> References()
    {
        string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrEmpty(trustedPlatformAssemblies))
        {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator))
            {
                yield return MetadataReference.CreateFromFile(path);
            }
        }

        yield return MetadataReference.CreateFromFile(typeof(ICallChannel).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(OpcConditionState).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(OpcItemState).Assembly.Location);
    }

    private static byte[] EncodeRequest(int id, string name)
    {
        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(id);
        writer.WriteUnicodeStringPtr(name);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static byte[] EncodeRefOutRequest(int id, int current)
    {
        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(id);
        writer.WriteInt32(current);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static ReadOnlyMemory<byte> EncodeRefOutResponse(int result, int current, string name)
    {
        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(result);
        writer.WriteInt32(current);
        writer.WriteUnicodeStringPtr(name);
        return buffer.AsMemory(0, writer.Position).ToArray();
    }

    private static ReadOnlyMemory<byte> EncodeInt32(int value)
    {
        var buffer = new byte[16];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(value);
        return buffer.AsMemory(0, writer.Position).ToArray();
    }
}
