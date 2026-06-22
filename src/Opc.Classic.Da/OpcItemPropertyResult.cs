// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// OPC DA's <c>OPCITEMPROPERTY</c> — a single property of an item
/// returned by <c>IOPCItemProperties::GetItemProperties</c>: data type,
/// property ID, optional item-ID for indirect properties, description,
/// current value, and a per-property error code.
/// </summary>
/// <param name="DataType">VARTYPE of the value (when ErrorId indicates success).</param>
/// <param name="PropertyId">OPC-defined property ID (e.g. 100=Value, 101=Quality, 102=Timestamp).</param>
/// <param name="ItemId">For indirect properties — the address-space item the property points at. Null otherwise.</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Value">Current value of the property.</param>
/// <param name="ErrorId">HRESULT — 0 on success; nonzero indicates the property couldn't be retrieved.</param>
public sealed record OpcItemPropertyResult(
    VarType DataType,
    int PropertyId,
    string? ItemId,
    string? Description,
    OpcVariant Value,
    int ErrorId);
