// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Opc.Classic.MigrationAnalyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OcmDa02_LegacyBrowse : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.LegacyBrowse);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (!string.Equals(LegacySyntaxFacts.GetInvocationName(invocation), "Browse", StringComparison.Ordinal) ||
            !LegacySyntaxFacts.IsLegacyInvocationReceiver(invocation, context.SemanticModel))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.LegacyBrowse, invocation.GetLocation()));
    }
}
