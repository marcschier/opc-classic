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
using Opc.Classic.Generators;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Generators.Tests;

[OpcInterface("44444444-5555-6666-7777-888888888888")]
[OpcGenerateServerDispatch]
public partial interface IServerDispatchRoundTrip
{
    [OpcMethod(3)]
    Task<int> AddAsync(int left, int right, CancellationToken cancellationToken = default);

    [OpcMethod(4)]
    Task<int> AdjustAsync(int id, ref int current, out string name, CancellationToken cancellationToken = default);
}

public sealed class ServerDispatchGeneratorTests
{
    private const string SampleSource = """
        using Opc.Classic.Generators;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Test;

        [OpcInterface("00000000-0000-0000-0000-000000000044")]
        [OpcGenerateServerDispatch]
        public partial interface IGeneratedServer
        {
            [OpcMethod(3)] Task<int> ReadAsync(int id, CancellationToken ct);
            [OpcMethod(4)] Task WriteAsync(string value, CancellationToken ct);
        }
        """;

    [Test]
    public async Task Generator_emits_server_dispatcher_class_and_switch_cases()
    {
        string generated = GeneratedDispatchSource(SampleSource);

        await Assert.That(generated).Contains("partial class IGeneratedServerServerDispatcher : global::Opc.Classic.Hosting.IOpcServerDispatcher");
        await Assert.That(generated).Contains("case 3: return await Dispatch_ReadAsync");
        await Assert.That(generated).Contains("case 4: return await Dispatch_WriteAsync");
    }

    [Test]
    public async Task Unknown_opnum_returns_E_NOTIMPL()
    {
        var dispatcher = new IServerDispatchRoundTripServerDispatcher(new RoundTripImpl());

        DispatchResult result = await dispatcher.DispatchAsync(999, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(result.Payload.IsEmpty).IsTrue();
    }

    [Test]
    public async Task Dispatch_decodes_request_calls_impl_and_encodes_return_value()
    {
        var impl = new RoundTripImpl();
        var dispatcher = new IServerDispatchRoundTripServerDispatcher(impl);
        byte[] request = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteInt32(2);
            writer.WriteInt32(5);
        });

        DispatchResult result = await dispatcher.DispatchAsync(IServerDispatchRoundTrip.Opnums.AddAsync, request, CancellationToken.None);

        var reader = new NdrReader(result.Payload.Span);
        int sum = reader.ReadInt32();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(sum).IsEqualTo(7);
        await Assert.That(impl.LastAdd).IsEqualTo((2, 5));
    }

    [Test]
    public async Task Dispatch_handles_ref_out_multi_response_shape()
    {
        var dispatcher = new IServerDispatchRoundTripServerDispatcher(new RoundTripImpl());
        byte[] request = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteInt32(10);
            writer.WriteInt32(20);
        });

        DispatchResult result = await dispatcher.DispatchAsync(IServerDispatchRoundTrip.Opnums.AdjustAsync, request, CancellationToken.None);

        var reader = new NdrReader(result.Payload.Span);
        int returnValue = reader.ReadInt32();
        int current = reader.ReadInt32();
        string? name = reader.ReadUnicodeStringPtr();
        await Assert.That(returnValue).IsEqualTo(31);
        await Assert.That(current).IsEqualTo(21);
        await Assert.That(name).IsEqualTo("item-10");
    }

    private static byte[] WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static string GeneratedDispatchSource(string source)
    {
        GeneratorDriverRunResult result = RunGenerator(source, out Compilation outputCompilation, out _);
        ThrowIfCompilationHasErrors(outputCompilation);
        return result.Results.SelectMany(static generator => generator.GeneratedSources)
            .Single(static generated => generated.HintName.EndsWith(".OpcServerDispatch.g.cs", StringComparison.Ordinal))
            .SourceText.ToString();
    }

    private static GeneratorDriverRunResult RunGenerator(string source, out Compilation outputCompilation, out ImmutableArray<Diagnostic> driverDiagnostics)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "OpcServerDispatchGeneratorTestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions())],
            references: References(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        ISourceGenerator[] generators =
        [
            new OpcInterfaceGenerator().AsSourceGenerator(),
            new OpcServerDispatchGenerator().AsSourceGenerator(),
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators, parseOptions: ParseOptions());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out driverDiagnostics);
        return driver.GetRunResult();
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

        yield return MetadataReference.CreateFromFile(typeof(IOpcServerDispatcher).Assembly.Location);
    }

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

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed class RoundTripImpl : IServerDispatchRoundTrip
    {
        public (int Left, int Right) LastAdd { get; private set; }

        public Task<int> AddAsync(int left, int right, CancellationToken cancellationToken = default)
        {
            LastAdd = (left, right);
            return Task.FromResult(left + right);
        }

        public Task<int> AdjustAsync(int id, ref int current, out string name, CancellationToken cancellationToken = default)
        {
            current++;
            name = "item-" + id.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return Task.FromResult(id + current);
        }
    }
}
