// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// OPC DA's <c>OPCITEMATTRIBUTES</c> — the full per-item attribute set
/// returned by <c>IOPCItemMgt::CreateEnumerator</c>.
/// </summary>
/// <param name="AccessPath">Server-defined access path (typically empty).</param>
/// <param name="ItemId">Server-namespace item identifier.</param>
/// <param name="Active">Current active state for sampling.</param>
/// <param name="ClientHandle">Client-side correlation handle echoed back in callbacks.</param>
/// <param name="ServerHandle">Server-assigned item handle; pass back on Read/Write calls.</param>
/// <param name="AccessRights">OPC_READABLE (0x1) | OPC_WRITEABLE (0x2) bitfield.</param>
/// <param name="Blob">Opaque server cookie blob; empty if none.</param>
/// <param name="RequestedDataType">Client-requested VARTYPE; VT_EMPTY = use server canonical.</param>
/// <param name="CanonicalDataType">The server's preferred VARTYPE for this item.</param>
/// <param name="EUType">OPCEUTYPE value: None=0, Analog=1, Enumerated=2.</param>
/// <param name="EUInfo">Engineering-unit information VARIANT; often VT_EMPTY when <paramref name="EUType" /> is None.</param>
public sealed record OpcItemAttributes(
    string? AccessPath,
    string? ItemId,
    bool Active,
    int ClientHandle,
    int ServerHandle,
    int AccessRights,
    byte[] Blob,
    VarType RequestedDataType,
    VarType CanonicalDataType,
    int EUType,
    OpcVariant EUInfo);
