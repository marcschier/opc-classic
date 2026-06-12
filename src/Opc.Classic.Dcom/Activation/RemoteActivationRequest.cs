//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Decoded server-side <c>IActivation::RemoteActivation</c> request fields.
/// Mirrors the input-side of <c>[MS-DCOM] §3.1.2.5.2.3.1</c>.
/// </summary>
/// <param name="Clsid">Class identifier to activate.</param>
/// <param name="RequestedIids">Interface IIDs the client wants to bind to.</param>
/// <param name="ClientImpLevel">Client impersonation level (RPC_C_IMP_LEVEL_*).</param>
/// <param name="Mode">Activation mode (MODE_GET_CLASS_OBJECT, etc.).</param>
/// <param name="RequestedProtocolSequences">Preferred protocol sequences for the activated object.</param>
public sealed record RemoteActivationRequest(
    Guid Clsid,
    IReadOnlyList<Guid> RequestedIids,
    uint ClientImpLevel,
    uint Mode,
    IReadOnlyList<ushort> RequestedProtocolSequences)
{
    /// <summary>
    /// Optional object name (used by <c>MODE_GET_CLASS_OBJECT</c> handlers).
    /// </summary>
    public string? ObjectName { get; init; }

    /// <summary>
    /// Optional pre-marshaled object-storage interface pointer (rarely populated).
    /// </summary>
    public ReadOnlyMemory<byte> ObjectStorage { get; init; }
}
