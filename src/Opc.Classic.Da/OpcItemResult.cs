//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Da;

/// <summary>
/// OPC DA's <c>OPCITEMRESULT</c> — the per-item result of
/// <c>IOPCItemMgt::AddItems</c> / <c>ValidateItems</c>: the server's
/// item handle, canonical data type, access rights, and any blob
/// the server returned for fast caller-side cookie-style re-binding.
/// </summary>
/// <param name="ServerHandle">Server-assigned item handle; pass back on Read/Write calls.</param>
/// <param name="CanonicalDataType">The server's preferred VARTYPE for this item.</param>
/// <param name="AccessRights">OPC_READABLE (0x1) | OPC_WRITEABLE (0x2) bitfield.</param>
/// <param name="Blob">Opaque server-provided blob for caller-side use; empty if the server returned none.</param>
public sealed record OpcItemResult(
    int ServerHandle,
    VarType CanonicalDataType,
    int AccessRights,
    byte[] Blob);
