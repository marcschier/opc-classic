// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Opc.Classic.MigrationAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OcmDa02_LegacyBrowseCodeFix)), Shared]
public sealed class OcmDa02_LegacyBrowseCodeFix : CodeFixProvider
{
    private const string Title = "Use BrowseAsync with CancellationToken";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.LegacyBrowse.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
        InvocationExpressionSyntax? invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(Title, cancellationToken => ApplyAsync(context.Document, invocation, cancellationToken), Title),
            context.Diagnostics);
    }

    private static async Task<Document> ApplyAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(document, cancellationToken).ConfigureAwait(false);
        MethodDeclarationSyntax? method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        string replacementText = MigrationCodeFixHelpers.ReceiverText(invocation) +
            ".BrowseAsync(" + MigrationCodeFixHelpers.ArgumentsWithCancellationToken(invocation.ArgumentList) + ")";
        ExpressionSyntax replacement = MigrationCodeFixHelpers.AwaitExpression(replacementText).WithTriviaFrom(invocation);
        root = root.ReplaceNode(invocation, replacement);
        if (method is not null)
        {
            root = MigrationCodeFixHelpers.EnsureContainingMethodIsAwaitable(root, root.FindNode(method.Span));
            root = MigrationCodeFixHelpers.EnsureCancellationTokenParameter(root, root.FindNode(method.Span));
        }

        root = MigrationCodeFixHelpers.AddUsing(root, "System.Threading");
        root = MigrationCodeFixHelpers.AddUsing(root, "System.Threading.Tasks");
        root = MigrationCodeFixHelpers.AddUsing(root, "Opc.Classic.Da");
        return await MigrationCodeFixHelpers.FormatRootAsync(document, root, cancellationToken).ConfigureAwait(false);
    }
}
