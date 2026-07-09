// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Opc.Classic.MigrationAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OcmGeneral02_ManualVariantCodeFix)), Shared]
public sealed class OcmGeneral02_ManualVariantCodeFix : CodeFixProvider
{
    private const string Title = "Use OpcVariant factory";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.ManualVariant.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
        SyntaxNode node = root.FindNode(context.Span);
        context.RegisterCodeFix(
            CodeAction.Create(Title, cancellationToken => ApplyAsync(context.Document, node, cancellationToken), Title),
            context.Diagnostics);
    }

    private static async Task<Document> ApplyAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(document, cancellationToken).ConfigureAwait(false);
        SyntaxNode? replacementTarget = node.FirstAncestorOrSelf<ObjectCreationExpressionSyntax>() ??
            (SyntaxNode?)node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (replacementTarget is null)
        {
            return document;
        }

        string argument = replacementTarget switch
        {
            ObjectCreationExpressionSyntax objectCreation => objectCreation.ArgumentList?.Arguments.FirstOrDefault()?.ToString() ?? "value",
            InvocationExpressionSyntax invocation => invocation.ArgumentList.Arguments.FirstOrDefault()?.ToString() ?? "value",
            _ => "value",
        };
        string factoryName = replacementTarget is InvocationExpressionSyntax invocationExpression &&
            string.Equals(LegacySyntaxFacts.GetInvocationName(invocationExpression), "GetObjectForNativeVariant", System.StringComparison.Ordinal)
                ? "FromNativeVariant"
                : "FromObject";
        ExpressionSyntax replacement = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(
            "OpcVariant." + factoryName + "(" + argument + ")").WithTriviaFrom(replacementTarget);
        root = root.ReplaceNode(replacementTarget, replacement);
        root = MigrationCodeFixHelpers.AddUsing(root, "Opc.Classic.Core");
        return await MigrationCodeFixHelpers.FormatRootAsync(document, root, cancellationToken).ConfigureAwait(false);
    }
}
