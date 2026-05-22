//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OpcClassic.Ndr;
using OpcClassic.Testing;
using TUnit.Core;

namespace OpcClassic.Generators.Tests;

[OpcInterface("33333333-4444-5555-6666-777777777778")]
[GenerateOpcProxy]
public partial interface IMarshalRoundTrip
{
    [OpcMethod(8)]
    Task<int> SomeMethodAsync(int id, string name, CancellationToken ct);
}

public sealed class OpcProxyGeneratorMarshallingTests
{
    private const string SampleSource = """
        using OpcClassic;
        using OpcClassic.Generators;
        using System;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Test;

        [OpcInterface("33333333-4444-5555-6666-777777777777")]
        [GenerateOpcProxy]
        public partial interface IMarshalTest
        {
            [OpcMethod(3)] Task<int> ReadCountAsync(int id, CancellationToken ct);
            [OpcMethod(4)] Task<string> GetNameAsync(int handle, CancellationToken ct);
            [OpcMethod(5)] Task WriteAsync(int handle, double value);
            [OpcMethod(6)] Task<Guid> GetServerIdAsync();
            [OpcMethod(7)] Task<int> WithComplexParamAsync(OpcVariant variant);
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
    public async Task WithComplexParamAsync_falls_back_to_empty_payload_placeholder()
    {
        string method = GeneratedMethodSection("WithComplexParamAsync");

        await Assert.That(method.Contains("WriteInt32", StringComparison.Ordinal)).IsFalse();
        await Assert.That(method.Contains("ReadInt32", StringComparison.Ordinal)).IsFalse();
        await Assert.That(method).Contains("global::System.ReadOnlyMemory<byte>.Empty");
        await Assert.That(method).Contains("return default!;");
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
        var proxy = new IMarshalRoundTrip_ClientProxy(channel);

        int result = await proxy.SomeMethodAsync(42, "hello", CancellationToken.None);

        await Assert.That(result).IsEqualTo(7);
        await Assert.That(observedInterfaceId).IsEqualTo(IMarshalRoundTrip.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IMarshalRoundTrip.Opnums.SomeMethodAsync);
        await Assert.That(observedCancellationToken).IsEqualTo(CancellationToken.None);
        await Assert.That(observedPayload is not null).IsTrue();
        await Assert.That(Convert.ToHexString(observedPayload!)).IsEqualTo(Convert.ToHexString(expectedPayload));
    }

    private static string GeneratedMethodSection(string methodName)
    {
        GeneratorDriverRunResult result = RunGenerator(SampleSource, out Compilation outputCompilation, out _);
        ThrowIfCompilationHasErrors(outputCompilation);
        return MethodSection(GeneratedProxySource(result), methodName);
    }

    private static string GeneratedProxySource(GeneratorDriverRunResult result) =>
        result.Results.SelectMany(static generator => generator.GeneratedSources)
            .Single(static generated => generated.HintName.EndsWith(".OpcProxy.g.cs", StringComparison.Ordinal))
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
    }

    private static byte[] EncodeRequest(int id, string name)
    {
        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(id);
        writer.WriteUnicodeStringPtr(name);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static ReadOnlyMemory<byte> EncodeInt32(int value)
    {
        var buffer = new byte[16];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(value);
        return buffer.AsMemory(0, writer.Position).ToArray();
    }
}
