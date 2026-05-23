//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Opc.Classic.MigrationAnalyzer;

internal static class LegacySyntaxFacts
{
    private static readonly string[] LegacyNamespacePrefixes =
    {
        "OpcCom",
        "OpcRcw",
        "Opc",
    };

    public static bool IsOpcComServerCreation(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
    {
        string typeName = Normalize(objectCreation.Type.ToString());
        if (typeName.Equals("OpcCom.Da.Server", StringComparison.Ordinal) ||
            typeName.Equals("OpcCom.Server", StringComparison.Ordinal))
        {
            return true;
        }

        ITypeSymbol? type = semanticModel.GetTypeInfo(objectCreation.Type).Type;
        if (type is null)
        {
            return false;
        }

        string fullyQualifiedName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return (fullyQualifiedName.Equals("global::OpcCom.Da.Server", StringComparison.Ordinal) ||
                fullyQualifiedName.Equals("global::OpcCom.Server", StringComparison.Ordinal)) &&
               !IsOpcClassicSymbol(type);
    }

    public static bool IsLegacyInvocationReceiver(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        ExpressionSyntax? receiver = GetInvocationReceiver(invocation);
        if (receiver is null)
        {
            return false;
        }

        ITypeSymbol? receiverType = semanticModel.GetTypeInfo(receiver).Type;
        if (receiverType is not null)
        {
            return IsLegacyOpcSymbol(receiverType);
        }

        string receiverText = receiver.ToString();
        return receiverText.StartsWith("OpcCom.", StringComparison.Ordinal) ||
               receiverText.StartsWith("OpcRcw.", StringComparison.Ordinal);
    }

    public static ExpressionSyntax? GetInvocationReceiver(InvocationExpressionSyntax invocation) => invocation.Expression switch
    {
        MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
        MemberBindingExpressionSyntax => null,
        _ => null,
    };

    public static string? GetInvocationName(InvocationExpressionSyntax invocation) => invocation.Expression switch
    {
        MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText,
        _ => null,
    };

    public static bool IsLegacyOpcSymbol(ITypeSymbol symbol)
    {
        if (IsOpcClassicSymbol(symbol))
        {
            return false;
        }

        string namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        foreach (string prefix in LegacyNamespacePrefixes)
        {
            if (namespaceName.Equals(prefix, StringComparison.Ordinal) ||
                namespaceName.StartsWith(prefix + ".", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsOpcClassicSymbol(ITypeSymbol symbol)
    {
        string namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return namespaceName.Equals("Opc.Classic", StringComparison.Ordinal) ||
               namespaceName.StartsWith("Opc.Classic.", StringComparison.Ordinal);
    }

    public static bool StartsWithOpcRcwNamespace(string name) =>
        name.Equals("OpcRcw", StringComparison.Ordinal) ||
        name.StartsWith("OpcRcw.", StringComparison.Ordinal);

    public static string MapOpcRcwNamespace(string name)
    {
        if (name.StartsWith("OpcRcw.Da", StringComparison.Ordinal))
        {
            return "Opc.Classic.Da" + name.Substring("OpcRcw.Da".Length);
        }

        if (name.StartsWith("OpcRcw.Hda", StringComparison.Ordinal))
        {
            return "Opc.Classic.Hda" + name.Substring("OpcRcw.Hda".Length);
        }

        if (name.StartsWith("OpcRcw.Ae", StringComparison.Ordinal))
        {
            return "Opc.Classic.Ae" + name.Substring("OpcRcw.Ae".Length);
        }

        if (name.StartsWith("OpcRcw.Comn", StringComparison.Ordinal))
        {
            return "Opc.Classic.Core" + name.Substring("OpcRcw.Comn".Length);
        }

        return "Opc.Classic";
    }

    private static string Normalize(string value) => value.StartsWith("global::", StringComparison.Ordinal)
        ? value.Substring("global::".Length)
        : value;
}
