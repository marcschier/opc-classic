//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Opc.Classic.MigrationAnalyzer.CodeFixes;

internal static class MigrationCodeFixHelpers {
    public static async Task<Document> ReplaceNodeAndFormatAsync(
        Document document,
        SyntaxNode oldNode,
        SyntaxNode newNode,
        CancellationToken cancellationToken) {
        SyntaxNode root = await GetRequiredRootAsync(document, cancellationToken).ConfigureAwait(false);
        SyntaxNode newRoot = root.ReplaceNode(oldNode, newNode.WithAdditionalAnnotations(Formatter.Annotation));
        Document newDocument = document.WithSyntaxRoot(newRoot);
        return await Formatter.FormatAsync(newDocument, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public static async Task<SyntaxNode> GetRequiredRootAsync(Document document, CancellationToken cancellationToken) {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        return root ?? throw new InvalidOperationException("The document does not contain a syntax root.");
    }

    public static async Task<Document> FormatRootAsync(Document document, SyntaxNode root, CancellationToken cancellationToken) {
        Document newDocument = document.WithSyntaxRoot(root.WithAdditionalAnnotations(Formatter.Annotation));
        return await Formatter.FormatAsync(newDocument, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public static SyntaxNode AddUsing(SyntaxNode root, string namespaceName) {
        if (root is not CompilationUnitSyntax compilationUnit) {
            return root;
        }

        bool hasUsing = compilationUnit.Usings.Any(usingDirective => string.Equals(usingDirective.Name?.ToString(), namespaceName, StringComparison.Ordinal));
        if (hasUsing) {
            return root;
        }

        UsingDirectiveSyntax usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
        return compilationUnit.AddUsings(usingDirective);
    }

    public static SyntaxNode EnsureContainingMethodIsAwaitable(SyntaxNode root, SyntaxNode node) {
        MethodDeclarationSyntax? method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method is null || method.Modifiers.Any(SyntaxKind.AsyncKeyword)) {
            return root;
        }

        TypeSyntax returnType = method.ReturnType;
        if (returnType is PredefinedTypeSyntax predefinedType && predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword)) {
            returnType = SyntaxFactory.ParseTypeName("Task").WithTriviaFrom(method.ReturnType);
        }
        else if (!IsTaskLike(returnType)) {
            returnType = SyntaxFactory.ParseTypeName("Task<" + returnType.ToString() + ">").WithTriviaFrom(method.ReturnType);
        }

        SyntaxToken asyncKeyword = SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            .WithTrailingTrivia(SyntaxFactory.Space);
        MethodDeclarationSyntax updatedMethod = method
            .WithReturnType(returnType)
            .WithModifiers(method.Modifiers.Add(asyncKeyword));

        return root.ReplaceNode(method, updatedMethod);
    }

    public static SyntaxNode EnsureCancellationTokenParameter(SyntaxNode root, SyntaxNode node) {
        MethodDeclarationSyntax? method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method is null || method.ParameterList.Parameters.Any(parameter => string.Equals(parameter.Identifier.ValueText, "ct", StringComparison.Ordinal))) {
            return root;
        }

        ParameterSyntax parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("ct"))
            .WithType(SyntaxFactory.ParseTypeName("CancellationToken"))
            .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
        MethodDeclarationSyntax updatedMethod = method.WithParameterList(method.ParameterList.AddParameters(parameter));
        return root.ReplaceNode(method, updatedMethod);
    }

    public static ExpressionSyntax AwaitExpression(string expressionText) =>
        SyntaxFactory.ParseExpression("await " + expressionText);

    public static string ArgumentsWithCancellationToken(ArgumentListSyntax argumentList) {
        string arguments = string.Join(", ", argumentList.Arguments.Select(static argument => argument.ToString()));
        return string.IsNullOrWhiteSpace(arguments) ? "ct" : arguments + ", ct";
    }

    public static string ReceiverText(InvocationExpressionSyntax invocation) {
        ExpressionSyntax? receiver = LegacySyntaxFacts.GetInvocationReceiver(invocation);
        return receiver?.ToString() ?? string.Empty;
    }

    private static bool IsTaskLike(TypeSyntax returnType) {
        string text = returnType.ToString();
        return text.Equals("Task", StringComparison.Ordinal) ||
               text.StartsWith("Task<", StringComparison.Ordinal) ||
               text.Equals("ValueTask", StringComparison.Ordinal) ||
               text.StartsWith("ValueTask<", StringComparison.Ordinal) ||
               text.Equals("System.Threading.Tasks.Task", StringComparison.Ordinal) ||
               text.StartsWith("System.Threading.Tasks.Task<", StringComparison.Ordinal);
    }
}







