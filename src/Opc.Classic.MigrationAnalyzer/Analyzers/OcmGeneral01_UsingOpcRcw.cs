// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Opc.Classic.MigrationAnalyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OcmGeneral01_UsingOpcRcw : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.UsingOpcRcw);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);
        context.RegisterSyntaxNodeAction(AnalyzeQualifiedName, SyntaxKind.QualifiedName);
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
    {
        var usingDirective = (UsingDirectiveSyntax)context.Node;
        string namespaceName = usingDirective.Name?.ToString() ?? string.Empty;
        if (LegacySyntaxFacts.StartsWithOpcRcwNamespace(namespaceName))
        {
            context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.UsingOpcRcw, usingDirective.GetLocation()));
        }
    }

    private static void AnalyzeQualifiedName(SyntaxNodeAnalysisContext context)
    {
        var qualifiedName = (QualifiedNameSyntax)context.Node;
        if (qualifiedName.Parent is QualifiedNameSyntax ||
            qualifiedName.Parent is UsingDirectiveSyntax ||
            !LegacySyntaxFacts.StartsWithOpcRcwNamespace(qualifiedName.ToString()))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.UsingOpcRcw, qualifiedName.GetLocation()));
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;
        if (memberAccess.Expression is MemberAccessExpressionSyntax ||
            !LegacySyntaxFacts.StartsWithOpcRcwNamespace(memberAccess.ToString()))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.UsingOpcRcw, memberAccess.GetLocation()));
    }
}
