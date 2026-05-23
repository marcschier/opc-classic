//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OpcClassic;
using OpcClassic.Da;
using TUnit.Core;

namespace OpcClassic.Generators.Tests;

public sealed class OpcProxyGeneratorArrayTests
{
    private const string SampleSource = """
        using OpcClassic;
        using OpcClassic.Da;
        using OpcClassic.Generators;
        using System.Threading.Tasks;

        namespace Test;

        public sealed class UnknownPayload { }

        [OpcInterface("33333333-4444-5555-6666-777777777780")]
        [GenerateOpcProxy]
        public partial interface IArrayMarshalTest
        {
            [OpcMethod(1)] Task<int[]> GetHandlesAsync();
            [OpcMethod(2)] Task WriteIntArrayAsync(int[] handles);
            [OpcMethod(3)] Task<OpcItemState[]> ReadAllAsync(int[] serverHandles);
            [OpcMethod(4)] Task<string[]> GetItemIDsAsync();
            [OpcMethod(5)] Task<UnknownPayload[]> MethodReturningArrayOfUnregisteredTypeAsync();
        }
        """;

    [Test]
    public async Task GetHandlesAsync_decodes_int32_conformant_array()
    {
        string method = GeneratedMethodSection("GetHandlesAsync");

        await Assert.That(method).Contains("var __opcCount = (int)__opcReader.ReadUInt32();");
        await Assert.That(method).Contains("var __opcArray = new global::System.Int32[__opcCount];");
        await Assert.That(method).Contains("for (int __opcIndex = 0; __opcIndex < __opcCount; __opcIndex++)");
        await Assert.That(method).Contains("__opcArray[__opcIndex] = __opcReader.ReadInt32();");
        await Assert.That(method).Contains("return __opcArray;");
    }

    [Test]
    public async Task WriteIntArrayAsync_encodes_int32_conformant_array()
    {
        string method = GeneratedMethodSection("WriteIntArrayAsync");

        await Assert.That(method).Contains("__opcWriter.WriteUInt32((uint)(handles?.Length ?? 0));");
        await Assert.That(method).Contains("if (handles != null)");
        await Assert.That(method).Contains("foreach (var __opcItem in handles)");
        await Assert.That(method).Contains("__opcWriter.WriteInt32(__opcItem);");
    }

    [Test]
    public async Task ReadAllAsync_encodes_primitive_array_and_decodes_complex_array()
    {
        string method = GeneratedMethodSection("ReadAllAsync");

        await Assert.That(method).Contains("__opcWriter.WriteUInt32((uint)(serverHandles?.Length ?? 0));");
        await Assert.That(method).Contains("foreach (var __opcItem in serverHandles)");
        await Assert.That(method).Contains("__opcWriter.WriteInt32(__opcItem);");
        await Assert.That(method).Contains("var __opcArray = new global::OpcClassic.Da.OpcItemState[__opcCount];");
        await Assert.That(method).Contains("__opcArray[__opcIndex] = global::OpcClassic.Da.Ndr.NdrOpcItemStateCodec.Read(ref __opcReader);");
    }

    [Test]
    public async Task GetItemIDsAsync_decodes_string_conformant_array()
    {
        string method = GeneratedMethodSection("GetItemIDsAsync");

        await Assert.That(method).Contains("var __opcCount = (int)__opcReader.ReadUInt32();");
        await Assert.That(method).Contains("var __opcArray = new global::System.String[__opcCount];");
        await Assert.That(method).Contains("__opcArray[__opcIndex] = __opcReader.ReadUnicodeStringPtr()!;");
        await Assert.That(method).Contains("return __opcArray;");
    }

    [Test]
    public async Task ArrayOfUnregisteredType_falls_back_to_empty_payload_placeholder()
    {
        string method = GeneratedMethodSection("MethodReturningArrayOfUnregisteredTypeAsync");

        await Assert.That(method).Contains("global::System.ReadOnlyMemory<byte>.Empty");
        await Assert.That(method).Contains("return default!;");
        await Assert.That(method.Contains("ReadUInt32()", StringComparison.Ordinal)).IsFalse();
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
            assemblyName: "OpcProxyGeneratorArrayTestAssembly",
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
        yield return MetadataReference.CreateFromFile(typeof(OpcItemState).Assembly.Location);
    }
}
