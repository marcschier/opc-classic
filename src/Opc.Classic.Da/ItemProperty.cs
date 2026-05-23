//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Da;

/// <summary>
/// A single property value attached to an OPC DA item. The result of
/// <c>IOPCItemProperties::GetItemProperties</c> or
/// <c>IOPCBrowse::GetProperties</c> rows.
/// </summary>
public sealed class ItemProperty
{
    /// <summary>The property identifier.</summary>
    public PropertyID PropertyId { get; init; }

    /// <summary>Server-supplied human description of the property.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Canonical CLR type of the property value (may be <see langword="null"/> if unknown).</summary>
    public Type? DataType { get; init; }

    /// <summary>
    /// The property value, when the caller requested values inline; otherwise
    /// <see langword="null"/>. Use <see cref="ResultId"/> to detect per-property
    /// success/failure.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>HRESULT for this property — may indicate that the value is unsupported.</summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;

    /// <summary>
    /// For properties that surface a separate item (DA 3.0 <c>LookupItemIDs</c>),
    /// the item identifier the property has been promoted to. <see langword="null"/>
    /// for the usual inline-value case.
    /// </summary>
    public string? ItemName { get; init; }

    /// <summary>Associated item path (mirror of <see cref="ItemName"/>).</summary>
    public string? ItemPath { get; init; }
}
