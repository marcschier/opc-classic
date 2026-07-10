// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Batch.Dcom;

/// <summary>
/// Per-spec OPC Batch interface set used to seed DCE/RPC presentation contexts in
/// the initial bind PDU so an after-the-fact <c>AlterContext</c> is never required.
/// </summary>
/// <remarks>
/// Some production OPC servers respond to a single-IID bind with
/// <c>PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED</c> when the IID has not
/// been preloaded into the presentation context table. Declaring the full Batch IID
/// set in the initial bind avoids that class of failure (mirrors
/// <c>Opc.Classic.Da.Dcom.OpcSpecCatalog</c>).
/// </remarks>
public static class OpcBatchSpecCatalog
{
    private static readonly Guid[] s_batch =
    {
        IOPCBatchServer.InterfaceId,
        IOPCBatchServer2.InterfaceId,
        IOPCEnumerationSets.InterfaceId,
    };

    /// <summary>
    /// OPC Batch IIDs to pre-declare in the initial DCE bind.
    /// </summary>
    public static IReadOnlyList<Guid> Batch => s_batch;
}
