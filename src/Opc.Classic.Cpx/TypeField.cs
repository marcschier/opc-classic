//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Cpx;

/// <summary>
/// A field declared by an OPC Complex Data type description.
/// </summary>
/// <param name="Name">Field name. Empty when the OPCBinary field is anonymous.</param>
/// <param name="Kind">OPCBinary field kind.</param>
/// <param name="TypeId">Referenced type identifier for <see cref="TypeKind.StructReference"/> fields.</param>
/// <param name="Length">Fixed field length, in bytes or characters depending on <paramref name="Kind"/>.</param>
/// <param name="ElementCount">Fixed element count for array fields.</param>
/// <param name="ElementCountFieldName">Sibling field that supplies a variable element count.</param>
/// <param name="FieldTerminator">Hex-encoded field terminator for terminated arrays.</param>
public sealed record TypeField(
    string Name,
    TypeKind Kind,
    string? TypeId = null,
    int? Length = null,
    int? ElementCount = null,
    string? ElementCountFieldName = null,
    string? FieldTerminator = null)
{
    /// <summary>Field name. Empty when the OPCBinary field is anonymous.</summary>
    public string Name { get; init; } = Name ?? string.Empty;

    /// <summary>OPCBinary field kind.</summary>
    public TypeKind Kind { get; init; } = ValidateKind(Kind);

    /// <summary>Referenced type identifier for <see cref="TypeKind.StructReference"/> fields.</summary>
    public string? TypeId { get; init; } = Normalize(TypeId);

    /// <summary>Fixed field length, in bytes or characters depending on <see cref="Kind"/>.</summary>
    public int? Length { get; init; } = ValidateNonNegative(Length, nameof(Length));

    /// <summary>Fixed element count for array fields.</summary>
    public int? ElementCount { get; init; } = ValidateNonNegative(ElementCount, nameof(ElementCount));

    /// <summary>Sibling field that supplies a variable element count.</summary>
    public string? ElementCountFieldName { get; init; } = Normalize(ElementCountFieldName);

    /// <summary>Hex-encoded field terminator for terminated arrays.</summary>
    public string? FieldTerminator { get; init; } = Normalize(FieldTerminator);

    private static TypeKind ValidateKind(TypeKind kind)
    {
        if (kind == TypeKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "A field must declare a concrete OPCBinary type kind.");
        }

        return kind;
    }

    private static int? ValidateNonNegative(int? value, string parameterName)
    {
        if (value is < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Field counts and lengths cannot be negative.");
        }

        return value;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
