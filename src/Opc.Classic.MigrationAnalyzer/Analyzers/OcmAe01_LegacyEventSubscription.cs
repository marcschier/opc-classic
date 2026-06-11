//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Opc.Classic.MigrationAnalyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OcmAe01_LegacyEventSubscription : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.LegacyEventSubscription);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (VariableDeclarationSyntax)context.Node;
        if (IsEventSubscriptionType(declaration.Type, context.SemanticModel))
        {
            context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.LegacyEventSubscription, declaration.Type.GetLocation()));
        }
    }

    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;
        if (parameter.Type is not null && IsEventSubscriptionType(parameter.Type, context.SemanticModel))
        {
            context.ReportDiagnostic(Diagnostic.Create(MigrationDiagnosticDescriptors.LegacyEventSubscription, parameter.Type.GetLocation()));
        }
    }

    private static bool IsEventSubscriptionType(TypeSyntax typeSyntax, SemanticModel semanticModel)
    {
        string typeText = typeSyntax.ToString();
        if (typeText.IndexOf("IOPCEventSubscription", StringComparison.Ordinal) >= 0)
        {
            return true;
        }

        ITypeSymbol? type = semanticModel.GetTypeInfo(typeSyntax).Type;
        return type is not null &&
               type.Name.Equals("IOPCEventSubscription", StringComparison.Ordinal) &&
               LegacySyntaxFacts.IsLegacyOpcSymbol(type);
    }
}

