//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Opc.Classic.MigrationAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OcmDa01_LegacyServerCreationCodeFix)), Shared]
public sealed class OcmDa01_LegacyServerCreationCodeFix : CodeFixProvider
{
    private const string Title = "Use await using OpcDaClient.ConnectAsync";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(MigrationDiagnosticDescriptors.LegacyServerCreation.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
        ObjectCreationExpressionSyntax? objectCreation = root.FindNode(context.Span).FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();
        if (objectCreation is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(Title, cancellationToken => ApplyAsync(context.Document, objectCreation, cancellationToken), Title),
            context.Diagnostics);
    }

    private static async Task<Document> ApplyAsync(
        Document document,
        ObjectCreationExpressionSyntax objectCreation,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = await MigrationCodeFixHelpers.GetRequiredRootAsync(document, cancellationToken).ConfigureAwait(false);
        MethodDeclarationSyntax? method = objectCreation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        string urlArgument = objectCreation.ArgumentList?.Arguments.FirstOrDefault()?.ToString() ?? "url";
        ExpressionSyntax connectExpression = MigrationCodeFixHelpers.AwaitExpression(
            "OpcDaClient.ConnectAsync(" + urlArgument + ", options)");

        LocalDeclarationStatementSyntax? localStatement = objectCreation.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
        if (localStatement is not null && localStatement.Declaration.Variables.Count == 1)
        {
            VariableDeclaratorSyntax variable = localStatement.Declaration.Variables[0];
            string declaration = localStatement.Declaration.Type + " " + variable.Identifier.ValueText + " = " + connectExpression + ";";
            StatementSyntax replacement = SyntaxFactory.ParseStatement("await using " + declaration)
                .WithTriviaFrom(localStatement)
                .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);
            root = root.ReplaceNode(localStatement, replacement);
        }
        else
        {
            root = root.ReplaceNode(objectCreation, connectExpression.WithTriviaFrom(objectCreation));
        }

        if (method is not null)
        {
            root = MigrationCodeFixHelpers.EnsureContainingMethodIsAwaitable(root, root.FindNode(method.Span));
        }

        root = MigrationCodeFixHelpers.AddUsing(root, "System.Threading.Tasks");
        root = MigrationCodeFixHelpers.AddUsing(root, "Opc.Classic.Da");
        return await MigrationCodeFixHelpers.FormatRootAsync(document, root, cancellationToken).ConfigureAwait(false);
    }
}






