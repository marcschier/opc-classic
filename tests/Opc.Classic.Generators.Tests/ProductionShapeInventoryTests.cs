// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Opc.Classic.Generators.Tests;

public sealed partial class ProductionShapeInventoryTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly HashSet<string> NonShapeDiagnosticIds =
    [
        "OPCGEN004",
        "OPCGEN005",
        "OPCGEN101",
        "OPCGEN102",
    ];

    [Test]
    public async Task Production_shapes_match_inventory_and_generated_sources_compile()
    {
        string repositoryRoot = FindRepositoryRoot();
        Audit audit = Analyze(repositoryRoot);
        IReadOnlyList<InventoryEntry> expectedInventory = Load<IReadOnlyList<InventoryEntry>>("ProductionShapeInventory.json");
        MigrationManifest migration = Load<MigrationManifest>("ProductionShapeMigrationManifest.json");

        ValidateInventory(audit, expectedInventory);
        ValidateSuppressions(audit, migration);
        ValidateDiagnostics(audit, migration);
        ValidateFallbacks(audit, migration, repositoryRoot);
        ValidateManualWirePaths(audit, migration, repositoryRoot);

        await Assert.That(audit.Methods.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task Audit_rejects_generated_source_compilation_errors()
    {
        SyntaxTree input = CSharpSyntaxTree.ParseText(
            "namespace Test; public partial class Shape { }",
            ParseOptions(),
            "Input.cs");
        SyntaxTree generated = CSharpSyntaxTree.ParseText(
            "namespace Test; public partial class Shape { private MissingType _value; }",
            ParseOptions(),
            "Shape.Generated.g.cs");
        Compilation compilation = CSharpCompilation.Create(
            "GeneratedErrorAudit",
            [input, generated],
            TrustedPlatformReferencePaths().Select(static path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        bool rejected = false;
        try
        {
            ThrowOnGeneratedErrors("Test", [input], compilation, []);
        }
        catch (InvalidOperationException exception) when (
            exception.Message.Contains("MissingType", StringComparison.Ordinal))
        {
            rejected = true;
        }

        await Assert.That(rejected).IsTrue();
    }

    [Test]
    public async Task Correlation_categories_require_resolved_semantic_attributes()
    {
        const string source = """
            using System;
            using System.Threading.Tasks;
            using Opc.Classic.Generators;

            namespace Test;

            [OpcInterface("50000000-0000-0000-0000-000000000001")]
            [GenerateOpcProxy]
            public partial interface IChild { }

            [OpcInterface("50000000-0000-0000-0000-000000000002")]
            [GenerateOpcProxy]
            public partial interface IShape
            {
                [OpcMethod(3)] Task<int[]> PlainArrayAsync(int count);
                [OpcMethod(4)]
                [return: OpcArrayCount(nameof(count))]
                Task<int[]> CorrelatedArrayAsync(int count);
                [OpcMethod(5)] Task<IChild> PlainInterfaceAsync(Guid iid);
                [OpcMethod(6)]
                [return: OpcIidIs(nameof(iid))]
                Task<IChild> CorrelatedInterfaceAsync(Guid iid);
            }
            """;
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, ParseOptions(), "SemanticCorrelations.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticCorrelationAudit",
            [tree],
            TrustedPlatformReferencePaths().Select(static path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [
                new OpcInterfaceGenerator().AsSourceGenerator(),
                new OpcProxyGenerator().AsSourceGenerator(),
                new OpcServerDispatchGenerator().AsSourceGenerator(),
            ],
            parseOptions: ParseOptions());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
        SemanticModel semanticModel = outputCompilation.GetSemanticModel(tree);
        var syntaxMethods = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .ToDictionary(static method => method.Identifier.ValueText, StringComparer.Ordinal);

        MethodShape plainArray = TestShape("PlainArrayAsync");
        MethodShape correlatedArray = TestShape("CorrelatedArrayAsync");
        MethodShape plainInterface = TestShape("PlainInterfaceAsync");
        MethodShape correlatedInterface = TestShape("CorrelatedInterfaceAsync");

        await Assert.That(plainArray.Shapes.Contains("count-correlated arrays")).IsFalse();
        await Assert.That(correlatedArray.Shapes.Contains("count-correlated arrays")).IsTrue();
        await Assert.That(plainInterface.Shapes.Contains("interface pointer/iid_is")).IsFalse();
        await Assert.That(correlatedInterface.Shapes.Contains("interface pointer/iid_is")).IsTrue();

        MethodShape TestShape(string methodName)
        {
            MethodDeclarationSyntax syntax = syntaxMethods[methodName];
            var symbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(syntax)!;
            return Shape(
                @"D:\repo\src\Opc.Classic.Da\Dcom\SemanticCorrelations.cs",
                "Test.IShape",
                syntax,
                symbol,
                client: true,
                server: false);
        }
    }

    private static Audit Analyze(string repositoryRoot)
    {
        string sourceRoot = Path.Combine(repositoryRoot, "src");
        ParsedSource[] allSources = Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}.artifacts-testgen{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Generators{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .Select(static path =>
            {
                string text = File.ReadAllText(path);
                return new ParsedSource(path, text, CSharpSyntaxTree.ParseText(text, ParseOptions(), path));
            })
            .ToArray();
        HashSet<string> unsupportedDiagnosticIds = UnsupportedShapeDiagnosticIds(sourceRoot);
        string[] projectNames = allSources
            .Where(static source =>
                source.Text.Contains("[GenerateOpcProxy]", StringComparison.Ordinal) ||
                source.Text.Contains("[OpcGenerateServerDispatch]", StringComparison.Ordinal))
            .Select(source => Path.GetRelativePath(sourceRoot, source.Path)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var methods = ImmutableArray.CreateBuilder<MethodShape>();
        var interfaces = new Dictionary<string, InterfaceShape>(StringComparer.Ordinal);
        var diagnostics = ImmutableArray.CreateBuilder<ObservedDiagnostic>();
        foreach (string projectName in projectNames)
        {
            string projectRoot = Path.Combine(sourceRoot, projectName);
            ParsedSource[] projectSources = allSources
                .Where(source => source.Path.StartsWith(projectRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            SyntaxTree globalUsings = GlobalUsings(projectName);
            SyntaxTree[] inputTrees = projectSources.Select(static source => source.Tree).Append(globalUsings).ToArray();
            var compilation = CSharpCompilation.Create(
                projectName + ".ProductionShapeAudit",
                inputTrees,
                ProjectReferences(projectName),
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    allowUnsafe: true,
                    nullableContextOptions: NullableContextOptions.Enable));
            GeneratorDriver driver = CSharpGeneratorDriver.Create(
                [
                    new OpcInterfaceGenerator().AsSourceGenerator(),
                    new OpcProxyGenerator().AsSourceGenerator(),
                    new OpcServerDispatchGenerator().AsSourceGenerator(),
                ],
                parseOptions: ParseOptions());
            driver = driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out Compilation outputCompilation,
                out ImmutableArray<Diagnostic> driverDiagnostics);
            GeneratorDriverRunResult runResult = driver.GetRunResult();

            ThrowOnGeneratedErrors(projectName, inputTrees, outputCompilation, driverDiagnostics);
            diagnostics.AddRange(ObservedDiagnostics(runResult, driverDiagnostics, unsupportedDiagnosticIds));
            CollectSemanticShapes(projectSources, outputCompilation, methods, interfaces);
        }

        return new Audit(
            methods.ToImmutable(),
            interfaces,
            CollectImplementations(allSources),
            CollectManualWirePaths(allSources, sourceRoot),
            diagnostics.Distinct().OrderBy(static item => item.Key, StringComparer.Ordinal).ToImmutableArray(),
            CollectSuppressions(allSources, sourceRoot, unsupportedDiagnosticIds));
    }

    private static HashSet<string> UnsupportedShapeDiagnosticIds(string sourceRoot)
    {
        string[] sources =
        [
            File.ReadAllText(Path.Combine(sourceRoot, "Opc.Classic.Generators", "OpcProxyGenerator.cs")),
            File.ReadAllText(Path.Combine(sourceRoot, "Opc.Classic.Generators", "OpcServerDispatchGenerator.cs")),
        ];
        Match[] ids = sources.SelectMany(static source => DescriptorIdRegex().Matches(source)).ToArray();
        int descriptorCount = sources.Sum(static source => DescriptorDeclarationRegex().Count(source));
        var result = ids.Select(static match => match.Groups["id"].Value).ToHashSet(StringComparer.Ordinal);
        if (ids.Length != descriptorCount || result.Count != ids.Length)
        {
            throw new InvalidOperationException("Every proxy/server diagnostic descriptor must have one unique literal OPCGEN ID.");
        }

        string[] staleStructuralIds = NonShapeDiagnosticIds.Except(result, StringComparer.Ordinal).ToArray();
        if (staleStructuralIds.Length > 0)
        {
            throw new InvalidOperationException("Stale structural diagnostic IDs: " + string.Join(", ", staleStructuralIds));
        }

        result.ExceptWith(NonShapeDiagnosticIds);
        return result;
    }

    private static SyntaxTree GlobalUsings(string projectName) =>
        CSharpSyntaxTree.ParseText(
            """
            global using System;
            global using System.Collections.Generic;
            global using System.IO;
            global using System.Linq;
            global using System.Net.Http;
            global using System.Threading;
            global using System.Threading.Tasks;
            """,
            ParseOptions(),
            projectName + ".GlobalUsings.g.cs");

    private static IEnumerable<MetadataReference> ProjectReferences(string projectName)
    {
        HashSet<string> paths = TrustedPlatformReferencePaths();

        string projectAssemblyPath = Path.Combine(AppContext.BaseDirectory, projectName + ".dll");
        if (!File.Exists(projectAssemblyPath))
        {
            throw new FileNotFoundException($"Missing real project reference for {projectName}.", projectAssemblyPath);
        }

        var queue = new Queue<AssemblyName>(ReflectionAssembly.LoadFrom(projectAssemblyPath).GetReferencedAssemblies());
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (queue.TryDequeue(out AssemblyName? name))
        {
            if (name.FullName is null || !visited.Add(name.FullName))
            {
                continue;
            }

            try
            {
                ReflectionAssembly assembly = ReflectionAssembly.Load(name);
                if (!string.IsNullOrEmpty(assembly.Location))
                {
                    paths.Add(assembly.Location);
                }
                foreach (AssemblyName referenced in assembly.GetReferencedAssemblies())
                {
                    queue.Enqueue(referenced);
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        return paths.Select(static path => MetadataReference.CreateFromFile(path));
    }

    private static HashSet<string> TrustedPlatformReferencePaths()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trusted)
        {
            paths.UnionWith(trusted.Split(Path.PathSeparator));
        }
        return paths;
    }

    private static void ThrowOnGeneratedErrors(
        string projectName,
        IReadOnlyCollection<SyntaxTree> inputTrees,
        Compilation outputCompilation,
        ImmutableArray<Diagnostic> driverDiagnostics)
    {
        var inputs = inputTrees.ToHashSet();
        Diagnostic[] errors = driverDiagnostics
            .Concat(outputCompilation.GetDiagnostics())
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Where(diagnostic =>
                diagnostic.Location.SourceTree is null ||
                !inputs.Contains(diagnostic.Location.SourceTree))
            .Distinct()
            .ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                $"Generated production source for {projectName} does not compile:" + Environment.NewLine +
                string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        }
    }

    private static ImmutableArray<ObservedDiagnostic> ObservedDiagnostics(
        GeneratorDriverRunResult runResult,
        ImmutableArray<Diagnostic> driverDiagnostics,
        IReadOnlySet<string> unsupportedDiagnosticIds)
    {
        var result = ImmutableArray.CreateBuilder<ObservedDiagnostic>();
        foreach (Diagnostic diagnostic in runResult.Results
                     .SelectMany(static item => item.Diagnostics)
                     .Concat(driverDiagnostics)
                     .Where(diagnostic => unsupportedDiagnosticIds.Contains(diagnostic.Id)))
        {
            SyntaxTree? tree = diagnostic.Location.SourceTree;
            SyntaxNode? node = tree?.GetRoot().FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            MethodDeclarationSyntax? method = node?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            InterfaceDeclarationSyntax? type = method?.FirstAncestorOrSelf<InterfaceDeclarationSyntax>();
            if (tree is null || method is null || type is null)
            {
                throw new InvalidOperationException(
                    $"Cannot map {diagnostic.Id}: {diagnostic.GetMessage(CultureInfo.InvariantCulture)} to a production method.");
            }

            result.Add(new ObservedDiagnostic(
                diagnostic.Id,
                NamespaceName((CompilationUnitSyntax)tree.GetRoot()) + "." + type.Identifier.ValueText,
                method.Identifier.ValueText));
        }

        return result.ToImmutable();
    }

    private static void CollectSemanticShapes(
        IEnumerable<ParsedSource> sources,
        Compilation outputCompilation,
        ImmutableArray<MethodShape>.Builder methods,
        IDictionary<string, InterfaceShape> interfaces)
    {
        foreach (ParsedSource source in sources)
        {
            SemanticModel semanticModel = outputCompilation.GetSemanticModel(source.Tree);
            foreach (InterfaceDeclarationSyntax declaration in source.Tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol interfaceSymbol ||
                    !HasAttribute(interfaceSymbol.GetAttributes(), "OpcInterfaceAttribute"))
                {
                    continue;
                }

                string interfaceName = interfaceSymbol.ToDisplayString();
                bool client = HasAttribute(interfaceSymbol.GetAttributes(), "GenerateOpcProxyAttribute");
                bool server = HasAttribute(interfaceSymbol.GetAttributes(), "OpcGenerateServerDispatchAttribute");
                var interfaceMethods = ImmutableArray.CreateBuilder<MethodShape>();
                foreach (MethodDeclarationSyntax methodSyntax in declaration.Members.OfType<MethodDeclarationSyntax>())
                {
                    if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol methodSymbol ||
                        !HasAttribute(methodSymbol.GetAttributes(), "OpcMethodAttribute"))
                    {
                        continue;
                    }

                    MethodShape shape = Shape(source.Path, interfaceName, methodSyntax, methodSymbol, client, server);
                    interfaceMethods.Add(shape);
                    if (shape.IsGenerated)
                    {
                        methods.Add(shape);
                    }
                }

                interfaces.Add(interfaceName, new InterfaceShape(interfaceName, source.Path, client, server, interfaceMethods.ToImmutable()));
            }
        }
    }

    private static MethodShape Shape(
        string sourcePath,
        string interfaceName,
        MethodDeclarationSyntax syntax,
        IMethodSymbol method,
        bool client,
        bool server)
    {
        IParameterSymbol[] parameters = method.Parameters.Where(static parameter => !IsCancellationToken(parameter.Type)).ToArray();
        ITypeSymbol? resultType = TaskResult(method.ReturnType);
        ITypeSymbol[] types = parameters.Select(static parameter => parameter.Type)
            .Concat(resultType is null ? [] : [resultType])
            .ToArray();
        bool hasArray = types.Any(static type => type is IArrayTypeSymbol);
        bool hasScalar = types.Length == 0 || types.Any(static type => type is not IArrayTypeSymbol);
        bool correlatedArray = parameters.Any(static parameter => HasAttribute(parameter.GetAttributes(), "OpcArrayCountAttribute")) ||
            HasAttribute(method.GetReturnTypeAttributes(), "OpcArrayCountAttribute") ||
            HasAttribute(method.GetReturnTypeAttributes(), "OpcEnumeratorArrayAttribute");
        bool iidIs = parameters.Any(static parameter => HasAttribute(parameter.GetAttributes(), "OpcIidIsAttribute")) ||
            HasAttribute(method.GetReturnTypeAttributes(), "OpcIidIsAttribute");
        bool pointerArray = resultType is IArrayTypeSymbol &&
            HasAttribute(method.GetReturnTypeAttributes(), "OpcUniquePointerAttribute");
        pointerArray |= parameters.Any(static parameter =>
            parameter.Type is IArrayTypeSymbol &&
            (parameter.RefKind is RefKind.Out or RefKind.Ref ||
             HasAttribute(parameter.GetAttributes(), "OpcUniquePointerAttribute")));
        int outCount = parameters.Count(static parameter => parameter.RefKind is RefKind.Out or RefKind.Ref);

        var shapes = ImmutableArray.CreateBuilder<string>();
        if (hasScalar) { shapes.Add("scalar"); }
        if (hasArray) { shapes.Add("array"); }
        if (correlatedArray) { shapes.Add("count-correlated arrays"); }
        if (HasAttribute(method.GetAttributes(), "OpcGenerateMultiOutRecordAttribute") || outCount >= 2) { shapes.Add("multi-out records"); }
        if (iidIs) { shapes.Add("interface pointer/iid_is"); }
        if (pointerArray) { shapes.Add("pointer arrays"); }
        if (method.Name.StartsWith("Clone", StringComparison.Ordinal)) { shapes.Add("clone"); }
        if (types.Any(IsCompound)) { shapes.Add("nested/compound records"); }

        return new MethodShape(
            interfaceName,
            method.Name,
            sourcePath,
            client,
            server,
            shapes.ToImmutable(),
            SpecReference(sourcePath, interfaceName, syntax));
    }

    private static bool IsCompound(ITypeSymbol type)
    {
        ITypeSymbol candidate = type is IArrayTypeSymbol array ? array.ElementType : type;
        return candidate.SpecialType == SpecialType.None &&
            candidate.TypeKind != TypeKind.Interface &&
            candidate.ToDisplayString() is not "System.Guid" and
            not "System.DateTime" and
            not "Opc.Classic.OpcVariant" and
            not "System.Threading.CancellationToken";
    }

    private static ITypeSymbol? TaskResult(ITypeSymbol returnType) =>
        returnType is INamedTypeSymbol { Name: "Task", TypeArguments.Length: 1 } task
            ? task.TypeArguments[0]
            : null;

    private static bool IsCancellationToken(ITypeSymbol type) =>
        type.ToDisplayString() == "System.Threading.CancellationToken";

    private static bool HasAttribute(ImmutableArray<AttributeData> attributes, string name) =>
        attributes.Any(attribute => attribute.AttributeClass?.Name == name);

    private static Dictionary<string, Implementation> CollectImplementations(IEnumerable<ParsedSource> sources)
    {
        var result = new Dictionary<string, Implementation>(StringComparer.Ordinal);
        foreach (ParsedSource source in sources)
        {
            string ns = NamespaceName((CompilationUnitSyntax)source.Tree.GetRoot());
            foreach (ClassDeclarationSyntax type in source.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                string className = type.Identifier.ValueText;
                string? side = className.EndsWith("ClientProxy", StringComparison.Ordinal)
                    ? "client"
                    : className.EndsWith("ServerDispatcher", StringComparison.Ordinal)
                        ? "server"
                        : null;
                if (side is null)
                {
                    continue;
                }

                string simpleInterface = side == "client"
                    ? className[..^"ClientProxy".Length]
                    : className[..^"ServerDispatcher".Length];
                IEnumerable<string> methodNames = side == "client"
                    ? type.Members.OfType<MethodDeclarationSyntax>()
                        .Where(static method => method.Modifiers.Any(SyntaxKind.PublicKeyword))
                        .Select(static method => method.Identifier.ValueText)
                    : type.Members.OfType<MethodDeclarationSyntax>()
                        .Select(static method => method.Identifier.ValueText)
                        .Where(static method => method.StartsWith("Dispatch", StringComparison.Ordinal) && method != "DispatchAsync")
                        .Select(static method => method["Dispatch".Length..]);
                string interfaceName = ns + "." + simpleInterface;
                result[interfaceName + "|" + side] = new Implementation(
                    interfaceName,
                    side,
                    className,
                    source.Path,
                    methodNames.ToImmutableHashSet(StringComparer.Ordinal));
            }
        }

        return result;
    }

    private static ImmutableArray<ManualWirePath> CollectManualWirePaths(
        IEnumerable<ParsedSource> sources,
        string sourceRoot)
    {
        var result = ImmutableArray.CreateBuilder<ManualWirePath>();
        string repositoryRoot = Path.GetDirectoryName(sourceRoot)!;
        foreach (ParsedSource source in sources)
        {
            string ns = NamespaceName((CompilationUnitSyntax)source.Tree.GetRoot());
            foreach (ClassDeclarationSyntax type in source.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                bool isDispatcher = type.BaseList?.Types.Any(static baseType =>
                    baseType.Type.ToString().EndsWith("IOpcServerDispatcher", StringComparison.Ordinal)) == true;
                bool isManualClientProxy =
                    type.Identifier.ValueText.EndsWith("ClientProxy", StringComparison.Ordinal) &&
                    !type.Modifiers.Any(SyntaxKind.PartialKeyword) &&
                    type.Members.OfType<MethodDeclarationSyntax>().Any(static method =>
                        method.Modifiers.Any(SyntaxKind.PublicKeyword));
                bool isWireCodec =
                    type.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                    (type.Identifier.ValueText.EndsWith("Wire", StringComparison.Ordinal) ||
                     type.Identifier.ValueText.EndsWith("ProxyCodec", StringComparison.Ordinal));
                if (!isDispatcher && !isManualClientProxy && !isWireCodec)
                {
                    continue;
                }

                string kind = isDispatcher ? "dispatcher" : isManualClientProxy ? "client-proxy" : "codec";
                string typeName = QualifiedTypeName(ns, type);
                ImmutableHashSet<string> methods = type.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => kind != "client-proxy" || method.Modifiers.Any(SyntaxKind.PublicKeyword))
                    .Select(static method => method.Identifier.ValueText)
                    .ToImmutableHashSet(StringComparer.Ordinal);
                result.Add(new ManualWirePath(
                    Path.GetRelativePath(repositoryRoot, source.Path).Replace('\\', '/'),
                    typeName,
                    kind,
                    methods));
            }
        }

        return result.OrderBy(static item => item.Key, StringComparer.Ordinal).ToImmutableArray();
    }

    private static string QualifiedTypeName(string ns, ClassDeclarationSyntax type)
    {
        string[] containingTypes = type.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .Reverse()
            .Select(static containing => containing.Identifier.ValueText)
            .ToArray();
        string prefix = string.IsNullOrEmpty(ns) ? string.Empty : ns + ".";
        return prefix + string.Join(".", containingTypes.Append(type.Identifier.ValueText));
    }

    private static ImmutableArray<ObservedSuppression> CollectSuppressions(
        IEnumerable<ParsedSource> sources,
        string sourceRoot,
        IReadOnlySet<string> unsupportedDiagnosticIds)
    {
        var result = ImmutableArray.CreateBuilder<ObservedSuppression>();
        foreach (ParsedSource source in sources)
        {
            foreach (Match match in SuppressionRegex().Matches(source.Text))
            {
                string[] ids = DiagnosticIdRegex().Matches(match.Value)
                    .Select(static id => id.Value)
                    .Where(unsupportedDiagnosticIds.Contains)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (ids.Length > 0)
                {
                    result.Add(new ObservedSuppression(
                        Path.GetRelativePath(Path.GetDirectoryName(sourceRoot)!, source.Path).Replace('\\', '/'),
                        string.Join(",", ids)));
                }
            }
        }

        return result.ToImmutable();
    }

    private static void ValidateInventory(Audit audit, IReadOnlyList<InventoryEntry> expected)
    {
        foreach (InventoryEntry item in expected)
        {
            MethodShape actual = audit.Methods.First(method => method.Key == item.Key);
            EqualSet($"shapes for {item.Key}", item.ShapeCategories, actual.Shapes);
            EqualSet($"generator sides for {item.Key}", item.GeneratorSides, actual.GeneratorSides);
            if (item.SpecReference != actual.SpecReference)
            {
                throw new InvalidOperationException($"Specification reference for {item.Key} must be {actual.SpecReference}.");
            }
        }

        EqualSet("generated method inventory", expected.Select(static item => item.Key), audit.Methods.Select(static item => item.Key));
    }

    private static void ValidateSuppressions(Audit audit, MigrationManifest migration) =>
        EqualSet(
            "diagnostic suppressions",
            migration.DiagnosticSuppressions.Select(static item => item.Key),
            audit.Suppressions.Select(static item => item.Key));

    private static void ValidateDiagnostics(Audit audit, MigrationManifest migration)
    {
        foreach (UnsupportedDiagnosticEntry item in migration.UnsupportedDiagnostics)
        {
            ValidateManifestShape(audit, item.Interface, item.Method, item.ShapeCategories, item.SpecReference);
        }
        EqualSet(
            "unsupported diagnostics",
            migration.UnsupportedDiagnostics.Select(static item => item.Key),
            audit.Diagnostics.Select(static item => item.Key));
    }

    private static void ValidateFallbacks(Audit audit, MigrationManifest migration, string repositoryRoot)
    {
        var actualFallbacks = new HashSet<string>(StringComparer.Ordinal);
        foreach (Implementation implementation in audit.Implementations.Values)
        {
            if (!audit.Interfaces.TryGetValue(implementation.Interface, out InterfaceShape? contract))
            {
                continue;
            }
            bool missingGenerator = implementation.Side == "client" ? !contract.Client : !contract.Server;
            if (!missingGenerator)
            {
                continue;
            }
            foreach (MethodShape method in contract.Methods.Where(method => implementation.Methods.Contains(method.Method)))
            {
                actualFallbacks.Add(method.Key + "|" + implementation.Side);
            }
        }

        foreach (FallbackEntry item in migration.HandWrittenFallbacks)
        {
            ValidateManifestShape(audit, item.Interface, item.Method, item.ShapeCategories, item.SpecReference);
            if (!audit.Implementations.TryGetValue(item.Interface + "|" + item.Side, out Implementation? implementation) ||
                implementation.TypeName != item.ImplementationType ||
                !implementation.Methods.Contains(item.Method) ||
                Path.GetFullPath(implementation.SourcePath) !=
                Path.GetFullPath(Path.Combine(repositoryRoot, item.ImplementationPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                throw new InvalidOperationException($"Invalid hand-written fallback {item.Key}.");
            }
        }

        EqualSet("hand-written fallbacks", migration.HandWrittenFallbacks.Select(static item => item.Key), actualFallbacks);
    }

    private static void ValidateManualWirePaths(
        Audit audit,
        MigrationManifest migration,
        string repositoryRoot)
    {
        foreach (ManualWirePathEntry item in migration.ManualWirePaths)
        {
            ManualWirePath actual = audit.ManualWirePaths.First(path => path.Key == item.Key);
            if (Path.GetFullPath(Path.Combine(repositoryRoot, actual.ImplementationPath.Replace('/', Path.DirectorySeparatorChar))) !=
                Path.GetFullPath(Path.Combine(repositoryRoot, item.ImplementationPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                throw new InvalidOperationException($"Invalid manual wire path {item.Key}.");
            }
            EqualSet($"manual wire methods for {item.Key}", item.Methods, actual.Methods);
        }

        EqualSet(
            "manual dispatcher/codec paths",
            migration.ManualWirePaths.Select(static item => item.Key),
            audit.ManualWirePaths.Select(static item => item.Key));
    }

    private static void ValidateManifestShape(
        Audit audit,
        string interfaceName,
        string methodName,
        IReadOnlyCollection<string> shapes,
        string specReference)
    {
        MethodShape method = audit.Interfaces[interfaceName].Methods.First(item => item.Method == methodName);
        EqualSet($"manifest shapes for {method.Key}", shapes, method.Shapes);
        if (specReference != method.SpecReference)
        {
            throw new InvalidOperationException($"Manifest specification for {method.Key} must be {method.SpecReference}.");
        }
    }

    private static void EqualSet(string name, IEnumerable<string> expected, IEnumerable<string> actual)
    {
        string[] missing = actual.Except(expected, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        string[] stale = expected.Except(actual, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (missing.Length > 0 || stale.Length > 0)
        {
            throw new InvalidOperationException(
                $"{name} differ." + Environment.NewLine +
                "Add:" + Environment.NewLine + string.Join(Environment.NewLine, missing) + Environment.NewLine +
                "Remove:" + Environment.NewLine + string.Join(Environment.NewLine, stale));
        }
    }

    private static T Load<T>(string fileName) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, fileName)), JsonOptions)
        ?? throw new InvalidOperationException($"Could not deserialize {fileName}.");

    private static string SpecReference(string sourcePath, string interfaceName, MethodDeclarationSyntax method)
    {
        Match match = SpecMethodRegex().Match(method.GetLeadingTrivia().ToFullString());
        string member = match.Success
            ? match.Groups["method"].Value
            : interfaceName[(interfaceName.LastIndexOf('.') + 1)..] + "::" +
              (method.Identifier.ValueText.EndsWith("Async", StringComparison.Ordinal)
                  ? method.Identifier.ValueText[..^"Async".Length]
                  : method.Identifier.ValueText);
        string spec = sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Ae{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC AE 1.10" :
            sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Batch{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC Batch" :
            sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Commands{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC Commands 1.00" :
            sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Cpx{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC Complex Data 1.00" :
            sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Da{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC DA" :
            sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Dx{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC DX 1.00" :
            sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Hda{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC HDA 1.20" :
            sourcePath.Contains($"{Path.DirectorySeparatorChar}Opc.Classic.Security{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ? "OPC Security 1.00" :
            "MS-DCOM";
        return spec + " IDL: " + member;
    }

    private static string NamespaceName(CompilationUnitSyntax root) =>
        root.Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? string.Empty;

    private static CSharpParseOptions ParseOptions() =>
        CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    [GeneratedRegex(@"id:\s*""(?<id>OPCGEN\d{3})""", RegexOptions.CultureInvariant)]
    private static partial Regex DescriptorIdRegex();

    [GeneratedRegex(@"private\s+static\s+readonly\s+DiagnosticDescriptor\s+\w+\s*=", RegexOptions.CultureInvariant)]
    private static partial Regex DescriptorDeclarationRegex();

    [GeneratedRegex(@"#pragma\s+warning\s+disable[^\r\n]*OPCGEN\d{3}[^\r\n]*", RegexOptions.CultureInvariant)]
    private static partial Regex SuppressionRegex();

    [GeneratedRegex(@"OPCGEN\d{3}", RegexOptions.CultureInvariant)]
    private static partial Regex DiagnosticIdRegex();

    [GeneratedRegex(@"<c>(?<method>[^<]*::[^<]*)</c>", RegexOptions.CultureInvariant)]
    private static partial Regex SpecMethodRegex();

    private sealed record ParsedSource(string Path, string Text, SyntaxTree Tree);
    private sealed record Audit(
        ImmutableArray<MethodShape> Methods,
        IReadOnlyDictionary<string, InterfaceShape> Interfaces,
        IReadOnlyDictionary<string, Implementation> Implementations,
        ImmutableArray<ManualWirePath> ManualWirePaths,
        ImmutableArray<ObservedDiagnostic> Diagnostics,
        ImmutableArray<ObservedSuppression> Suppressions);
    private sealed record InterfaceShape(
        string Name,
        string SourcePath,
        bool Client,
        bool Server,
        ImmutableArray<MethodShape> Methods);
    private sealed record MethodShape(
        string Interface,
        string Method,
        string SourcePath,
        bool Client,
        bool Server,
        ImmutableArray<string> Shapes,
        string SpecReference)
    {
        public string Key => Interface + "." + Method;
        public bool IsGenerated => Client || Server;
        public ImmutableArray<string> GeneratorSides => Client && Server ? ["client", "server"] : Client ? ["client"] : Server ? ["server"] : [];
    }
    private sealed record Implementation(
        string Interface,
        string Side,
        string TypeName,
        string SourcePath,
        ImmutableHashSet<string> Methods);
    private sealed record ManualWirePath(
        string ImplementationPath,
        string ImplementationType,
        string Kind,
        ImmutableHashSet<string> Methods)
    {
        public string Key => ImplementationType + "|" + Kind;
    }
    private sealed record ObservedDiagnostic(string Id, string Interface, string Method)
    {
        public string Key => Id + "|" + Interface + "|" + Method;
    }
    private sealed record ObservedSuppression(string SourcePath, string Ids)
    {
        public string Key => SourcePath + "|" + Ids;
    }
    private sealed record InventoryEntry(
        string Interface,
        string Method,
        IReadOnlyList<string> GeneratorSides,
        IReadOnlyList<string> ShapeCategories,
        string SpecReference)
    {
        public string Key => Interface + "." + Method;
    }
    private sealed record MigrationManifest(
        IReadOnlyList<SuppressionEntry> DiagnosticSuppressions,
        IReadOnlyList<UnsupportedDiagnosticEntry> UnsupportedDiagnostics,
        IReadOnlyList<FallbackEntry> HandWrittenFallbacks,
        IReadOnlyList<ManualWirePathEntry> ManualWirePaths);
    private sealed record SuppressionEntry(string SourcePath, IReadOnlyList<string> DiagnosticIds)
    {
        public string Key => SourcePath + "|" + string.Join(",", DiagnosticIds.Order(StringComparer.Ordinal));
    }
    private sealed record UnsupportedDiagnosticEntry(
        string DiagnosticId,
        string Interface,
        string Method,
        IReadOnlyList<string> ShapeCategories,
        string SpecReference)
    {
        public string Key => DiagnosticId + "|" + Interface + "|" + Method;
    }
    private sealed record FallbackEntry(
        string Interface,
        string Method,
        string Side,
        IReadOnlyList<string> ShapeCategories,
        string SpecReference,
        string ImplementationPath,
        string ImplementationType)
    {
        public string Key => Interface + "." + Method + "|" + Side;
    }
    private sealed record ManualWirePathEntry(
        string ImplementationPath,
        string ImplementationType,
        string Kind,
        IReadOnlyList<string> Methods)
    {
        public string Key => ImplementationType + "|" + Kind;
    }
}
