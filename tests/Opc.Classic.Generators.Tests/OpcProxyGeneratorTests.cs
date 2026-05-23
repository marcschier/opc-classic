//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Opc.Classic.Generators;
using TUnit.Core;

namespace Opc.Classic.Generators.Tests;

public sealed class OpcProxyGeneratorTests
{
    private const string SampleSource = """
        using System.Threading;
        using System.Threading.Tasks;
        using Opc.Classic.Generators;

        namespace Test
        {
            [OpcInterface("00000000-0000-0000-0000-000000000001")]
            [GenerateOpcProxy]
            public partial interface ITestProxy
            {
                Task<int> ReadAsync(int id, CancellationToken ct);
                Task WriteAsync(string value);
                Task<string> GetValueAsync();
            }
        }
        """;

    [Test]
    public async Task Generator_RunsWithoutDiagnostics_OnPartialOpcInterface()
    {
        GeneratorRunResult result = RunGenerator(SampleSource, out Compilation outputCompilation, out ImmutableArray<Diagnostic> driverDiagnostics);

        await Assert.That(driverDiagnostics.Length).IsEqualTo(0);
        await Assert.That(result.Diagnostics.Length).IsEqualTo(0);
        await Assert.That(outputCompilation.GetDiagnostics().Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsFalse();
    }

    [Test]
    public async Task GeneratedProxy_DeclaresPartialClientProxyClass()
    {
        string generated = GeneratedProxySource(SampleSource);

        await Assert.That(generated).Contains("partial class ITestProxyClientProxy : ITestProxy");
    }

    [Test]
    public async Task GeneratedProxy_DeclaresCallChannelFieldAndConstructor()
    {
        string generated = GeneratedProxySource(SampleSource);

        await Assert.That(generated).Contains("private readonly global::Opc.Classic.ICallChannel _channel;");
        await Assert.That(generated).Contains("public ITestProxyClientProxy(global::Opc.Classic.ICallChannel channel)");
        await Assert.That(generated).Contains("_channel = channel ?? throw new global::System.ArgumentNullException(nameof(channel));");
    }

    [Test]
    public async Task GeneratedProxy_StubsEveryMethodWithNotImplementedException()
    {
        string generated = GeneratedProxySource(SampleSource);

        await Assert.That(generated).Contains("per-method shim for 'ITestProxy.ReadAsync' TBD");
        await Assert.That(generated).Contains("per-method shim for 'ITestProxy.WriteAsync' TBD");
        await Assert.That(generated).Contains("per-method shim for 'ITestProxy.GetValueAsync' TBD");
        await Assert.That(CountOccurrences(generated, "throw new global::System.NotImplementedException")).IsEqualTo(3);
    }

    [Test]
    public async Task GeneratedProxy_MatchesAsyncMethodSignatures()
    {
        string generated = GeneratedProxySource(SampleSource);

        await Assert.That(generated).Contains("public global::System.Threading.Tasks.Task<int> ReadAsync(int id, global::System.Threading.CancellationToken ct)");
        await Assert.That(generated).Contains("public global::System.Threading.Tasks.Task WriteAsync(string value)");
        await Assert.That(generated).Contains("public global::System.Threading.Tasks.Task<string> GetValueAsync()");
    }

    [Test]
    public async Task NonPartialInterface_ReportsOpcgen004Diagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            using Opc.Classic.Generators;

            namespace Test
            {
                [OpcInterface("00000000-0000-0000-0000-000000000001")]
                [GenerateOpcProxy]
                public interface IBadProxy
                {
                    Task PingAsync();
                }
            }
            """;

        GeneratorRunResult result = RunGenerator(source, out _, out _);

        await Assert.That(result.Diagnostics.Any(static diagnostic => diagnostic.Id == "OPCGEN004")).IsTrue();
    }

    private static string GeneratedProxySource(string source)
    {
        GeneratorRunResult result = RunGenerator(source, out _, out _);
        return result.GeneratedSources.Single(static generated => generated.HintName.EndsWith(".OpcProxy.g.cs", StringComparison.Ordinal)).SourceText.ToString();
    }

    private static GeneratorRunResult RunGenerator(string source, out Compilation outputCompilation, out ImmutableArray<Diagnostic> driverDiagnostics)
    {
        var compilation = CreateCompilation(source);
        ISourceGenerator[] generators =
        [
            new OpcInterfaceGenerator().AsSourceGenerator(),
            new OpcProxyGenerator().AsSourceGenerator(),
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators, parseOptions: ParseOptions());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out driverDiagnostics);
        return driver.GetRunResult().Results.Single(static result =>
            result.GeneratedSources.Any(static source => source.HintName.EndsWith(".OpcProxy.g.cs", StringComparison.Ordinal)) ||
            result.Diagnostics.Any(static diagnostic => diagnostic.Id is "OPCGEN004" or "OPCGEN005"));
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create(
            assemblyName: "OpcProxyGeneratorTestAssembly",
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

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
