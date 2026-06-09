//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Opc.Classic.MigrationAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OcmGeneral01_UsingOpcRcwCodeFix)), Shared]
public sealed class OcmGeneral01_UsingOpcRcwCodeFix : CodeFixProvider {
    private const string Title = "Use Opc.Classic namespace (add the Opc.Classic NuGet package if needed)";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.UsingOpcRcw.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
        SyntaxNode node = root.FindNode(context.Span);
        context.RegisterCodeFix(
            CodeAction.Create(Title, cancellationToken => ApplyAsync(context.Document, node, cancellationToken), Title),
            context.Diagnostics);
    }

    private static async Task<Document> ApplyAsync(Document document, SyntaxNode node, CancellationToken cancellationToken) {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(document, cancellationToken).ConfigureAwait(false);
        SyntaxNode currentNode = root.FindNode(node.Span);
        SyntaxNode replacementTarget = currentNode.FirstAncestorOrSelf<UsingDirectiveSyntax>() ??
            currentNode.FirstAncestorOrSelf<QualifiedNameSyntax>() ??
            currentNode.FirstAncestorOrSelf<MemberAccessExpressionSyntax>() ??
            currentNode;

        string replacementName = LegacySyntaxFacts.MapOpcRcwNamespace(replacementTarget switch {
            UsingDirectiveSyntax usingDirective => usingDirective.Name?.ToString() ?? "OpcRcw",
            _ => replacementTarget.ToString(),
        });

        SyntaxNode replacement = replacementTarget switch {
            UsingDirectiveSyntax usingDirective => usingDirective.WithName(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName(replacementName)),
            QualifiedNameSyntax => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName(replacementName).WithTriviaFrom(replacementTarget),
            MemberAccessExpressionSyntax => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(replacementName).WithTriviaFrom(replacementTarget),
            _ => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName(replacementName).WithTriviaFrom(replacementTarget),
        };

        root = root.ReplaceNode(replacementTarget, replacement);
        return await MigrationCodeFixHelpers.FormatRootAsync(document, root, cancellationToken).ConfigureAwait(false);
    }
}




