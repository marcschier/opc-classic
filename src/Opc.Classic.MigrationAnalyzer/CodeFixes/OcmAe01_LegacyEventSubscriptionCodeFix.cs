// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Opc.Classic.MigrationAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OcmAe01_LegacyEventSubscriptionCodeFix)), Shared]
public sealed class OcmAe01_LegacyEventSubscriptionCodeFix : CodeFixProvider
{
    private const string Title = "Use await foreach OpcEventNotification subscription";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.LegacyEventSubscription.Id);

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
        MethodDeclarationSyntax? method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        LocalDeclarationStatementSyntax? localStatement = node.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
        if (localStatement is not null)
        {
            string receiver = "server";
            VariableDeclaratorSyntax variable = localStatement.Declaration.Variables.First();
            if (variable.Initializer?.Value is InvocationExpressionSyntax invocation)
            {
                receiver = MigrationCodeFixHelpers.ReceiverText(invocation);
            }

            StatementSyntax replacement = SyntaxFactory.ParseStatement(
                "await foreach (OpcEventNotification notification in " + receiver + ".SubscribeAsync(ct))\n{\n    await handler.HandleAsync(notification, ct);\n}")
                .WithTriviaFrom(localStatement)
                .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);
            root = root.ReplaceNode(localStatement, replacement);
        }

        if (method is not null)
        {
            root = MigrationCodeFixHelpers.EnsureContainingMethodIsAwaitable(root, root.FindNode(method.Span));
            root = MigrationCodeFixHelpers.EnsureCancellationTokenParameter(root, root.FindNode(method.Span));
        }

        root = MigrationCodeFixHelpers.AddUsing(root, "System.Threading");
        root = MigrationCodeFixHelpers.AddUsing(root, "System.Threading.Tasks");
        root = MigrationCodeFixHelpers.AddUsing(root, "Opc.Classic.Ae");
        return await MigrationCodeFixHelpers.FormatRootAsync(document, root, cancellationToken).ConfigureAwait(false);
    }
}
