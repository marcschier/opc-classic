// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// OPC DA's <c>OPCITEMDEF</c> — the per-item input to
/// <c>IOPCItemMgt::AddItems</c> / <c>ValidateItems</c>: identifies the
/// item to add by access-path + item-ID, configures its initial active
/// state, supplies the client handle and any cookie blob, and requests
/// a preferred data type.
/// </summary>
/// <param name="AccessPath">Server-defined access path (typically empty).</param>
/// <param name="ItemId">Server-namespace item identifier.</param>
/// <param name="Active">If true, the item starts active (will be sampled).</param>
/// <param name="ClientHandle">Client-side correlation handle echoed back in callbacks.</param>
/// <param name="Blob">Opaque server cookie blob (typically empty).</param>
/// <param name="RequestedDataType">Preferred VARTYPE for reads; VT_EMPTY = use server's canonical.</param>
public sealed record OpcItemDef(
    string? AccessPath,
    string? ItemId,
    bool Active,
    int ClientHandle,
    byte[]? Blob,
    VarType RequestedDataType);
