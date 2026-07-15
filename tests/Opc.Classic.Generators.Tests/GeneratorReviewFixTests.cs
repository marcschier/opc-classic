// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Generators.Tests;

[OpcInterface("51000000-0000-0000-0000-000000000001")]
[GenerateOpcProxy]
public partial interface IReviewChild
{
}

[OpcInterface("51000000-0000-0000-0000-000000000002")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IReviewShapes
{
    [OpcMethod(3)]
    [return: OpcIidIs(nameof(iid))]
    Task<IOpcInterfaceRef> GetRawAsync(Guid iid, CancellationToken cancellationToken = default);

    [OpcMethod(4)]
    Task AcceptRawAsync(Guid iid, [OpcIidIs(nameof(iid))] IOpcInterfaceRef value, CancellationToken cancellationToken = default);

    [OpcMethod(5)]
    Task GetChildrenAsync(Guid iid, [OpcIidIs(nameof(iid))] out IReviewChild?[] children, CancellationToken cancellationToken = default);

    [OpcMethod(6)]
    Task<int> SumAsync([OpcEmitArrayCount] int[] values, CancellationToken cancellationToken = default);
}

public sealed class GeneratorReviewFixTests
{
    [Test]
    public async Task Raw_iid_is_return_and_parameter_use_MInterfacePointer_framing_on_both_sides()
    {
        Guid iid = IReviewChild.InterfaceId;
        var implementation = new ReviewShapesImplementation();
        var dispatcher = CreateDispatcher(implementation);
        var proxy = new IReviewShapesClientProxy(new DispatcherChannel(dispatcher));

        IOpcInterfaceRef result = await proxy.GetRawAsync(iid);
        await proxy.AcceptRawAsync(iid, CreateInterfaceRef(iid));

        await Assert.That(result.Iid).IsEqualTo(iid);
        await Assert.That(implementation.Accepted?.Iid).IsEqualTo(iid);
    }

    [Test]
    public async Task Interface_pointer_array_uses_two_pass_referents_then_bodies_and_preserves_nulls()
    {
        Guid iid = IReviewChild.InterfaceId;
        var implementation = new ReviewShapesImplementation();
        var dispatcher = CreateDispatcher(implementation);
        byte[] request = WritePayload((ref NdrWriter writer) => writer.WriteGuid(iid));

        DispatchResult response = await dispatcher.DispatchAsync(IReviewShapes.Opnums.GetChildrenAsync, request, CancellationToken.None);

        await Assert.That(Convert.ToHexString(response.Payload.Span[..16])).IsEqualTo("03000000000002000000000004000200");

        var proxy = new IReviewShapesClientProxy(new DispatcherChannel(dispatcher));
        await proxy.GetChildrenAsync(iid, out IReviewChild?[] children);

        await Assert.That(children.Length).IsEqualTo(3);
        await Assert.That(children[0]).IsTypeOf<IReviewChildClientProxy>();
        await Assert.That(children[1]).IsNull();
        await Assert.That(children[2]).IsTypeOf<IReviewChildClientProxy>();
    }

    [Test]
    public async Task Dispatcher_rejects_inconsistent_explicit_and_conformant_array_counts_before_invocation()
    {
        var implementation = new ReviewShapesImplementation();
        var dispatcher = CreateDispatcher(implementation);
        byte[] payload = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(2);
            writer.WriteUInt32(1);
            writer.WriteInt32(7);
        });

        bool rejected = false;
        try
        {
            _ = await dispatcher.DispatchAsync(IReviewShapes.Opnums.SumAsync, payload, CancellationToken.None);
        }
        catch (InvalidDataException)
        {
            rejected = true;
        }
        await Assert.That(rejected).IsTrue();
        await Assert.That(implementation.SumInvocationCount).IsEqualTo(0);
    }

    [Test]
    public async Task Generated_source_snapshots_show_framing_deferred_layout_and_count_validation()
    {
        GeneratorDriverRunResult result = RunGenerators(SnapshotSource, out Compilation compilation);
        ThrowIfErrors(compilation);
        string proxy = GeneratedSource(result, "Snapshot.IService.OpcProxy.g.cs");
        string server = GeneratedSource(result, "Snapshot.IService.OpcServerDispatch.g.cs");

        await Assert.That(proxy).Contains("OpcMInterfacePointerCodec.Write(ref");
        await Assert.That(proxy).Contains("OpcMInterfacePointerCodec.Read(ref");
        await Assert.That(proxy).Contains("TryReadReferentId(out _)");
        await Assert.That(proxy).Contains("ReadMInterfacePointerBody(ref");
        await Assert.That(server).Contains("WriteUniquePointerReferent(__opcInterfaceRef is not null)");
        await Assert.That(server).Contains("WriteMInterfacePointerBody(ref");
        await Assert.That(server).Contains("uint valuesExplicitCount = __opcReader.ReadUInt32();");
        await Assert.That(server).Contains("checked((uint)values.Length) != valuesExplicitCount");
    }

    private const string SnapshotSource = """
        using System;
        using System.Threading.Tasks;
        using Opc.Classic.Dcom;
        using Opc.Classic.Generators;

        namespace Snapshot;

        [OpcInterface("52000000-0000-0000-0000-000000000001")]
        [GenerateOpcProxy]
        public partial interface IChild { }

        [OpcInterface("52000000-0000-0000-0000-000000000002")]
        [GenerateOpcProxy]
        [OpcGenerateServerDispatch]
        public partial interface IService
        {
            [OpcMethod(3)]
            [return: OpcIidIs(nameof(iid))]
            Task<IOpcInterfaceRef> GetRawAsync(Guid iid);

            [OpcMethod(4)]
            Task PutRawAsync(Guid iid, [OpcIidIs(nameof(iid))] IOpcInterfaceRef value);

            [OpcMethod(5)]
            Task ChildrenAsync(Guid iid, [OpcIidIs(nameof(iid))] out IChild?[] children);

            [OpcMethod(6)]
            Task CountedAsync([OpcEmitArrayCount] int[] values);
        }
        """;

    private static IReviewShapesServerDispatcher CreateDispatcher(ReviewShapesImplementation implementation) =>
        new(implementation, (iid, value) => value is IReviewChild ? CreateInterfaceRef(iid) : null);

    private static IOpcInterfaceRef CreateInterfaceRef(Guid iid) =>
        new OpcInterfaceRef(iid, 0, 1, 1, 2, Guid.Parse("51000000-0000-0000-0000-000000000003"), 0, []);

    private static byte[] WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[4096];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static GeneratorDriverRunResult RunGenerators(string source, out Compilation outputCompilation)
    {
        var compilation = CSharpCompilation.Create(
            "GeneratorReviewFixSnapshot",
            [CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest))],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        ISourceGenerator[] generators =
        [
            new OpcInterfaceGenerator().AsSourceGenerator(),
            new OpcProxyGenerator().AsSourceGenerator(),
            new OpcServerDispatchGenerator().AsSourceGenerator(),
        ];
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators, parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out _);
        return driver.GetRunResult();
    }

    private static IEnumerable<MetadataReference> References()
    {
        string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrEmpty(trustedPlatformAssemblies))
        {
            foreach (string path in trustedPlatformAssemblies.Split(Path.PathSeparator))
            {
                yield return MetadataReference.CreateFromFile(path);
            }
        }
        yield return MetadataReference.CreateFromFile(typeof(ICallChannel).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(OpcSubProxyHelper).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(IOpcServerDispatcher).Assembly.Location);
    }

    private static string GeneratedSource(GeneratorDriverRunResult result, string suffix) =>
        result.Results.SelectMany(static generator => generator.GeneratedSources)
            .Single(source => source.HintName.EndsWith(suffix, StringComparison.Ordinal))
            .SourceText.ToString();

    private static void ThrowIfErrors(Compilation compilation)
    {
        Diagnostic[] errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed class ReviewShapesImplementation : IReviewShapes
    {
        public IOpcInterfaceRef? Accepted { get; private set; }
        public int SumInvocationCount { get; private set; }

        public Task<IOpcInterfaceRef> GetRawAsync(Guid requestedIid, CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateInterfaceRef(requestedIid));

        public Task AcceptRawAsync(Guid requestedIid, IOpcInterfaceRef value, CancellationToken cancellationToken = default)
        {
            _ = requestedIid;
            Accepted = value;
            return Task.CompletedTask;
        }

        public Task GetChildrenAsync(Guid requestedIid, out IReviewChild?[] children, CancellationToken cancellationToken = default)
        {
            _ = requestedIid;
            children = [new ReviewChild(), null, new ReviewChild()];
            return Task.CompletedTask;
        }

        public Task<int> SumAsync(int[] values, CancellationToken cancellationToken = default)
        {
            SumInvocationCount++;
            return Task.FromResult(values.Sum());
        }

        private sealed class ReviewChild : IReviewChild { }
    }

    private sealed class DispatcherChannel(IReviewShapesServerDispatcher dispatcher) : ICallChannel
    {
        public async Task<NdrCallResult> InvokeAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken = default)
        {
            _ = interfaceId;
            DispatchResult result = await dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken);
            return new NdrCallResult(result.Hresult, result.Payload);
        }
    }
}
