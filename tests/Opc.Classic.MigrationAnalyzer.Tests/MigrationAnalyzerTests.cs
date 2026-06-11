//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Opc.Classic.MigrationAnalyzer.Analyzers;
using Opc.Classic.MigrationAnalyzer.CodeFixes;
using TUnit.Core;

namespace Opc.Classic.MigrationAnalyzer.Tests;

public sealed class MigrationAnalyzerTests
{
    [Test]
    public async Task LegacyServerCreation_reports_OCMDA001()
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(new OcmDa01_LegacyServerCreation(), LegacyServerCreationSource);
        await Assert.That(diagnostics.Single().Id).IsEqualTo("OCMDA001");
    }

    [Test]
    public async Task LegacyServerCreation_code_fix_connects_async_with_await_using()
    {
        string fixedSource = await ApplyCodeFixAsync(new OcmDa01_LegacyServerCreation(), new OcmDa01_LegacyServerCreationCodeFix(), LegacyServerCreationSource);
        await Assert.That(fixedSource).Contains("await using var server = await OpcDaClient.ConnectAsync(url, options);");
        await Assert.That(fixedSource).Contains("using Opc.Classic.Da;");
    }

    [Test]
    public async Task LegacyBrowse_reports_OCMDA002()
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(new OcmDa02_LegacyBrowse(), LegacyBrowseSource);
        await Assert.That(diagnostics.Single().Id).IsEqualTo("OCMDA002");
    }

    [Test]
    public async Task LegacyBrowse_code_fix_uses_browse_async_with_cancellation_token()
    {
        string fixedSource = await ApplyCodeFixAsync(new OcmDa02_LegacyBrowse(), new OcmDa02_LegacyBrowseCodeFix(), LegacyBrowseSource);
        await Assert.That(fixedSource).Contains("await server.BrowseAsync(itemId, filters, ct)");
        await Assert.That(fixedSource).Contains("CancellationToken ct = default");
    }

    [Test]
    public async Task LegacyRead_reports_OCMDA003()
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(new OcmDa03_LegacyRead(), LegacyReadSource);
        await Assert.That(diagnostics.Single().Id).IsEqualTo("OCMDA003");
    }

    [Test]
    public async Task LegacyRead_code_fix_uses_read_async_with_cancellation_token()
    {
        string fixedSource = await ApplyCodeFixAsync(new OcmDa03_LegacyRead(), new OcmDa03_LegacyReadCodeFix(), LegacyReadSource);
        await Assert.That(fixedSource).Contains("await group.ReadAsync(items, ct)");
        await Assert.That(fixedSource).Contains("async Task ReadValues");
    }

    [Test]
    public async Task LegacyEventSubscription_reports_OCMAE001()
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(new OcmAe01_LegacyEventSubscription(), LegacyEventSubscriptionSource);
        await Assert.That(diagnostics.Single().Id).IsEqualTo("OCMAE001");
    }

    [Test]
    public async Task LegacyEventSubscription_code_fix_uses_await_foreach()
    {
        string fixedSource = await ApplyCodeFixAsync(new OcmAe01_LegacyEventSubscription(), new OcmAe01_LegacyEventSubscriptionCodeFix(), LegacyEventSubscriptionSource);
        await Assert.That(fixedSource).Contains("await foreach (OpcEventNotification notification in server.SubscribeAsync(ct))");
        await Assert.That(fixedSource).Contains("await handler.HandleAsync(notification, ct);");
    }

    [Test]
    public async Task LegacySyncReadRaw_reports_OCMHDA001()
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(new OcmHda01_LegacySyncReadRaw(), LegacySyncReadRawSource);
        await Assert.That(diagnostics.Single().Id).IsEqualTo("OCMHDA001");
    }

    [Test]
    public async Task LegacySyncReadRaw_code_fix_uses_read_raw_async()
    {
        string fixedSource = await ApplyCodeFixAsync(new OcmHda01_LegacySyncReadRaw(), new OcmHda01_LegacySyncReadRawCodeFix(), LegacySyncReadRawSource);
        await Assert.That(fixedSource).Contains("await historian.ReadRawAsync(itemId, start, end, ct)");
        await Assert.That(fixedSource).Contains("using Opc.Classic.Hda;");
    }

    [Test]
    public async Task UsingOpcRcw_reports_OCMGEN001()
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(new OcmGeneral01_UsingOpcRcw(), UsingOpcRcwSource);
        await Assert.That(diagnostics.Single().Id).IsEqualTo("OCMGEN001");
    }

    [Test]
    public async Task UsingOpcRcw_code_fix_rewrites_namespace()
    {
        string fixedSource = await ApplyCodeFixAsync(new OcmGeneral01_UsingOpcRcw(), new OcmGeneral01_UsingOpcRcwCodeFix(), UsingOpcRcwSource);
        await Assert.That(fixedSource).Contains("using Opc.Classic.Da;");
        await Assert.That(fixedSource).DoesNotContain("using OpcRcw.Da;");
    }

    [Test]
    public async Task ManualVariant_reports_OCMGEN002()
    {
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(new OcmGeneral02_ManualVariant(), ManualVariantSource);
        await Assert.That(diagnostics.Single().Id).IsEqualTo("OCMGEN002");
    }

    [Test]
    public async Task ManualVariant_code_fix_uses_opc_variant_factory()
    {
        string fixedSource = await ApplyCodeFixAsync(new OcmGeneral02_ManualVariant(), new OcmGeneral02_ManualVariantCodeFix(), ManualVariantSource);
        await Assert.That(fixedSource).Contains("OpcVariant.FromObject(value)");
        await Assert.That(fixedSource).Contains("using Opc.Classic.Core;");
    }

    [Test]
    public async Task OpcClassic_usage_does_not_report_false_positives()
    {
        DiagnosticAnalyzer[] analyzers =
        [
            new OcmDa01_LegacyServerCreation(),
            new OcmDa02_LegacyBrowse(),
            new OcmDa03_LegacyRead(),
            new OcmAe01_LegacyEventSubscription(),
            new OcmHda01_LegacySyncReadRaw(),
            new OcmGeneral01_UsingOpcRcw(),
            new OcmGeneral02_ManualVariant(),
        ];

        foreach (DiagnosticAnalyzer analyzer in analyzers)
        {
            ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(analyzer, OpcClassicSource);
            await Assert.That(diagnostics.Length).IsEqualTo(0);
        }
    }

    private const string LegacyServerCreationSource = """
        namespace OpcCom.Da
        {
            public sealed class Server
            {
                public Server(string url) { }
            }
        }

        namespace Test
        {
            public sealed class Demo
            {
                public void Connect(string url)
                {
                    var server = new OpcCom.Da.Server(url);
                }
            }
        }
        """;

    private const string LegacyBrowseSource = """
        namespace OpcCom.Da
        {
            public sealed class Server
            {
                public object Browse(string itemId, object filters) => new object();
            }
        }

        namespace Test
        {
            public sealed class Demo
            {
                public void Browse(OpcCom.Da.Server server, string itemId, object filters)
                {
                    var branches = server.Browse(itemId, filters);
                }
            }
        }
        """;

    private const string LegacyReadSource = """
        namespace OpcCom.Da
        {
            public sealed class Group
            {
                public object Read(object items) => new object();
            }
        }

        namespace Test
        {
            public sealed class Demo
            {
                public void ReadValues(OpcCom.Da.Group group, object items)
                {
                    var values = group.Read(items);
                }
            }
        }
        """;

    private const string LegacyEventSubscriptionSource = """
        namespace OpcRcw.Ae
        {
            public interface IOPCEventSubscription { }

            public sealed class EventServer
            {
                public IOPCEventSubscription CreateSubscription(object callback) => throw null!;
            }
        }

        namespace Test
        {
            public sealed class Demo
            {
                public void Subscribe(OpcRcw.Ae.EventServer server, object callback)
                {
                    OpcRcw.Ae.IOPCEventSubscription subscription = server.CreateSubscription(callback);
                }
            }
        }
        """;

    private const string LegacySyncReadRawSource = """
        namespace OpcCom.Hda
        {
            public sealed class Historian
            {
                public object SyncReadRaw(string itemId, object start, object end) => new object();
            }
        }

        namespace Test
        {
            public sealed class Demo
            {
                public void ReadHistory(OpcCom.Hda.Historian historian, string itemId, object start, object end)
                {
                    var values = historian.SyncReadRaw(itemId, start, end);
                }
            }
        }
        """;

    private const string UsingOpcRcwSource = """
        using OpcRcw.Da;

        namespace Test
        {
            public sealed class Demo
            {
            }
        }
        """;

    private const string ManualVariantSource = """
        namespace Opc
        {
            public sealed class VariantValue
            {
                public VariantValue(object value) { }
            }
        }

        namespace Test
        {
            public sealed class Demo
            {
                public object Wrap(object value) => new Opc.VariantValue(value);
            }
        }
        """;

    private const string OpcClassicSource = """
        using System.Threading;
        using System.Threading.Tasks;
        using Opc.Classic.Core;
        using Opc.Classic.Da;
        using Opc.Classic.Hda;

        namespace Opc.Classic.Core
        {
            public sealed class OpcVariant
            {
                public static OpcVariant FromObject(object value) => new OpcVariant();
            }
        }

        namespace Opc.Classic.Da
        {
            public interface IOpcDaBrowse
            {
                Task<object> BrowseAsync(string itemId, object filters, CancellationToken ct);
            }

            public interface IOpcDaSyncIO
            {
                Task<object> ReadAsync(object items, CancellationToken ct);
            }
        }

        namespace Opc.Classic.Hda
        {
            public interface IOpcHdaSyncReadAsync
            {
                Task<object> ReadRawAsync(string itemId, object start, object end, CancellationToken ct);
            }
        }

        namespace Test
        {
            public sealed class Demo
            {
                public async Task UseClassicAsync(IOpcDaBrowse browser, IOpcDaSyncIO syncIo, IOpcHdaSyncReadAsync historian, CancellationToken ct)
                {
                    object filters = new object();
                    object items = new object();
                    object start = new object();
                    object end = new object();
                    await browser.BrowseAsync("", filters, ct);
                    await syncIo.ReadAsync(items, ct);
                    await historian.ReadRawAsync("", start, end, ct);
                    OpcVariant.FromObject(items);
                }
            }
        }
        """;

    private static async Task<ImmutableArray<Diagnostic>> DiagnosticsAsync(DiagnosticAnalyzer analyzer, string source)
    {
        CSharpCompilation compilation = CreateCompilation(source);
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static async Task<string> ApplyCodeFixAsync(DiagnosticAnalyzer analyzer, CodeFixProvider codeFix, string source)
    {
        using var workspace = new AdhocWorkspace();
        Project project = workspace
            .AddProject("MigrationAnalyzerCodeFixTest", LanguageNames.CSharp)
            .WithParseOptions(ParseOptions())
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReferences(References());
        Document document = workspace.AddDocument(project.Id, "Test.cs", SourceText.From(source));
        ImmutableArray<Diagnostic> diagnostics = await DiagnosticsAsync(analyzer, source);
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(
            document,
            diagnostics.Single(),
            (action, _) => actions.Add(action),
            CancellationToken.None);

        await codeFix.RegisterCodeFixesAsync(context);
        CodeAction action = actions.Single();
        IEnumerable<CodeActionOperation> operations = await action.GetOperationsAsync(CancellationToken.None);
        ApplyChangesOperation applyChangesOperation = operations.OfType<ApplyChangesOperation>().Single();
        Document? updatedDocument = applyChangesOperation.ChangedSolution.GetDocument(document.Id);
        SourceText text = await updatedDocument!.GetTextAsync();
        return text.ToString();
    }

    private static CSharpCompilation CreateCompilation(string source) => CSharpCompilation.Create(
        assemblyName: "MigrationAnalyzerTests",
        syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions())],
        references: References(),
        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static CSharpParseOptions ParseOptions() => CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

    private static IEnumerable<MetadataReference> References()
    {
        string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrEmpty(trustedPlatformAssemblies))
        {
            yield break;
        }

        foreach (string path in trustedPlatformAssemblies.Split(Path.PathSeparator))
        {
            yield return MetadataReference.CreateFromFile(path);
        }
    }
}
