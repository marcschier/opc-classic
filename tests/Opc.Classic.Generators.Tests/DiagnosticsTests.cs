//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Opc.Classic;
using Opc.Classic.Generators;
using TUnit.Core;

namespace Opc.Classic.Generators.Tests;

public sealed class DiagnosticsTests {
    [Test]
    public async Task InvalidOpcInterfaceGuid_reports_OPCGEN001() {
        const string source = """
            using Opc.Classic.Generators;

            namespace Test;

            [OpcInterface("not-a-guid")]
            public partial interface IBadGuid
            {
            }
            """;

        await AssertDiagnosticAsync(source, "OPCGEN001", DiagnosticSeverity.Error);
    }

    [Test]
    public async Task NonPartialOpcInterface_reports_OPCGEN002() {
        const string source = """
            using Opc.Classic.Generators;

            namespace Test;

            [OpcInterface("00000000-0000-0000-0000-000000000001")]
            public interface INonPartial
            {
            }
            """;

        await AssertDiagnosticAsync(source, "OPCGEN002", DiagnosticSeverity.Error);
    }

    [Test]
    public async Task DuplicateOpcMethodOpnum_reports_OPCGEN003() {
        const string source = """
            using System.Threading.Tasks;
            using Opc.Classic.Generators;

            namespace Test;

            [OpcInterface("00000000-0000-0000-0000-000000000002")]
            public partial interface IDuplicateOpnum
            {
                [OpcMethod(3)] Task FirstAsync();
                [OpcMethod(3)] Task SecondAsync();
            }
            """;

        await AssertDiagnosticAsync(source, "OPCGEN003", DiagnosticSeverity.Error);
    }

    [Test]
    public async Task UnsupportedOpcMethodSignature_reports_OPCGEN007() {
        const string source = """
            using Opc.Classic.Generators;

            namespace Test;

            [OpcInterface("00000000-0000-0000-0000-000000000003")]
            [GenerateOpcProxy]
            public partial interface IUnsupportedSignature
            {
                [OpcMethod(3)] void Ping();
            }
            """;

        await AssertDiagnosticAsync(source, "OPCGEN007", DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task MissingReturnCodec_reports_OPCGEN009() {
        const string source = """
            using System.Threading.Tasks;
            using Opc.Classic.Generators;

            namespace Test;

            public sealed class UnknownPayload { }

            [OpcInterface("00000000-0000-0000-0000-000000000004")]
            [GenerateOpcProxy]
            public partial interface IMissingReturnCodec
            {
                [OpcMethod(3)] Task<UnknownPayload> GetAsync();
            }
            """;

        await AssertDiagnosticAsync(source, "OPCGEN009", DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task UnsupportedParameter_reports_OPCGEN010() {
        const string source = """
            using System.Threading.Tasks;
            using Opc.Classic.Generators;

            namespace Test;

            public sealed class UnknownPayload { }

            [OpcInterface("00000000-0000-0000-0000-000000000005")]
            [GenerateOpcProxy]
            public partial interface IUnsupportedParameter
            {
                [OpcMethod(3)] Task SendAsync(UnknownPayload payload);
            }
            """;

        await AssertDiagnosticAsync(source, "OPCGEN010", DiagnosticSeverity.Warning);
    }

    private static async Task AssertDiagnosticAsync(string source, string diagnosticId, DiagnosticSeverity severity) {
        Diagnostic diagnostic = DiagnosticsFor(source).First(diagnostic => diagnostic.Id == diagnosticId);

        await Assert.That(diagnostic.Severity).IsEqualTo(severity);
    }

    private static ImmutableArray<Diagnostic> DiagnosticsFor(string source) {
        GeneratorDriverRunResult result = RunGenerator(source, out ImmutableArray<Diagnostic> driverDiagnostics);
        return result.Results
            .SelectMany(static generator => generator.Diagnostics)
            .Concat(driverDiagnostics)
            .ToImmutableArray();
    }

    private static GeneratorDriverRunResult RunGenerator(string source, out ImmutableArray<Diagnostic> driverDiagnostics) {
        var compilation = CSharpCompilation.Create(
            assemblyName: "OpcGeneratorDiagnosticsTestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions())],
            references: References(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        ISourceGenerator[] generators =
        [
            new OpcInterfaceGenerator().AsSourceGenerator(),
            new OpcProxyGenerator().AsSourceGenerator(),
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators, parseOptions: ParseOptions());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out driverDiagnostics);
        return driver.GetRunResult();
    }

    private static CSharpParseOptions ParseOptions() => CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

    private static IEnumerable<MetadataReference> References() {
        string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrEmpty(trustedPlatformAssemblies)) {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator)) {
                yield return MetadataReference.CreateFromFile(path);
            }
        }

        yield return MetadataReference.CreateFromFile(typeof(ICallChannel).Assembly.Location);
    }
}
