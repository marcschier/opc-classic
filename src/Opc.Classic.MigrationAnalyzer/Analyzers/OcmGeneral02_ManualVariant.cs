// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Opc.Classic.MigrationAnalyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OcmGeneral02_ManualVariant : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.ManualVariant);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
        if (!objectCreation.Type.ToString().EndsWith("VariantValue", StringComparison.Ordinal))
        {
            return;
        }

        ITypeSymbol? type = context.SemanticModel.GetTypeInfo(objectCreation.Type).Type;
        if (type is not null && LegacySyntaxFacts.IsOpcClassicSymbol(type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.ManualVariant, objectCreation.GetLocation()));
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        string? invocationName = LegacySyntaxFacts.GetInvocationName(invocation);
        if (invocationName is null || !IsMarshalVariantMethod(invocationName))
        {
            return;
        }

        ExpressionSyntax? receiver = LegacySyntaxFacts.GetInvocationReceiver(invocation);
        if (receiver is null || !string.Equals(receiver.ToString(), "Marshal", StringComparison.Ordinal))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.ManualVariant, invocation.GetLocation()));
    }

    private static bool IsMarshalVariantMethod(string invocationName) =>
        invocationName.StartsWith("GetVariant", StringComparison.Ordinal) ||
        invocationName.Equals("GetObjectForNativeVariant", StringComparison.Ordinal) ||
        invocationName.Equals("GetNativeVariantForObject", StringComparison.Ordinal);
}
