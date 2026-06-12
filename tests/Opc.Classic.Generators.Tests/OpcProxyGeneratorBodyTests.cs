//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Opc.Classic.Generators.Tests;

public sealed class OpcProxyGeneratorBodyTests
{
    private const string SampleSource = """
        using Opc.Classic.Generators;
        using System.Threading;
        using System.Threading.Tasks;

        namespace Test;

        [OpcInterface("00000000-0000-0000-0000-000000000002")]
        [GenerateOpcProxy]
        public partial interface ITestService
        {
            [OpcMethod(3)] Task DoNothingAsync(CancellationToken ct);
            [OpcMethod(4)] Task<int> ReadAsync(int id, CancellationToken ct);
            [OpcMethod(5)] Task<string> GetNameAsync();
            Task<string> WithoutOpcMethodAsync();
            [OpcMethod(6)] void SyncMethod();
            [OpcMethod(7)] Task BadAsync(out int x);
        }
        """;

    [Test]
    public async Task DoNothingAsync_emits_invoke_async_no_result()
    {
        string method = GeneratedMethodSection("DoNothingAsync");

        await Assert.That(method).Contains("InvokeAsync(");
        await Assert.That(method).Contains("global::Test.ITestService.InterfaceId");
        await Assert.That(method).Contains("global::Test.ITestService.Opnums.DoNothingAsync");
        await Assert.That(method.Contains("NotImplementedException", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task ReadAsync_emits_invoke_async_with_int_marshalling()
    {
        string method = GeneratedMethodSection("ReadAsync");

        await Assert.That(method).Contains("InvokeAsync(");
        await Assert.That(method).Contains("global::Test.ITestService.Opnums.ReadAsync");
        await Assert.That(method).Contains("WriteInt32(id)");
        await Assert.That(method).Contains("ReadInt32()");
        await Assert.That(method.Contains("return default!;", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task GetNameAsync_uses_CancellationToken_None()
    {
        string method = GeneratedMethodSection("GetNameAsync");

        await Assert.That(method).Contains("global::System.Threading.CancellationToken.None");
        await Assert.That(method).Contains("global::Test.ITestService.Opnums.GetNameAsync");
    }

    [Test]
    public async Task WithoutOpcMethodAsync_keeps_NotImplementedException_stub()
    {
        string method = GeneratedMethodSection("WithoutOpcMethodAsync");

        await Assert.That(method).Contains("NotImplementedException");
        await Assert.That(method.Contains("InvokeAsync", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task SyncMethod_keeps_NotImplementedException_stub()
    {
        string method = GeneratedMethodSection("SyncMethod");

        await Assert.That(method).Contains("NotImplementedException");
        await Assert.That(method.Contains("InvokeAsync", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task BadAsync_with_out_param_decodes_response_without_OPCGEN006()
    {
        GeneratorDriverRunResult result = RunGenerator(SampleSource, out Compilation outputCompilation, out ImmutableArray<Diagnostic> driverDiagnostics);
        ThrowIfCompilationHasErrors(outputCompilation);
        string generated = GeneratedProxySource(result);
        string method = MethodSection(generated, "BadAsync");
        var diagnostics = result.Results.SelectMany(static generator => generator.Diagnostics).Concat(driverDiagnostics);

        await Assert.That(method).Contains("InvokeAsync");
        await Assert.That(method).Contains("ReadInt32()");
        await Assert.That(method).Contains("x = __opcDecoded;");
        await Assert.That(method.Contains("NotImplementedException", StringComparison.Ordinal)).IsFalse();
        await Assert.That(diagnostics.Any(static diagnostic => diagnostic.Id == "OPCGEN006")).IsFalse();
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
            assemblyName: "OpcProxyGeneratorBodyTestAssembly",
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
}
