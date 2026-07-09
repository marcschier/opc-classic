// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Discovery;

/// <summary>
/// Per-spec OPC Discovery (OPCEnum) interface set used to seed DCE/RPC
/// presentation contexts in the initial bind PDU so an after-the-fact
/// <c>AlterContext</c> is never required.
/// </summary>
/// <remarks>
/// Some OPCEnum implementations (and most production OPC servers) respond
/// to a single-IID bind with <c>PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED</c>
/// when the IID has not been preloaded into the presentation context table.
/// Declaring the full Discovery IID set in the initial bind avoids that class
/// of failure.
/// </remarks>
public static class OpcDiscoverySpecCatalog
{
    private static readonly Guid[] s_discovery =
    {
        OpcGuids.IID_IOPCServerList2,
        OpcGuids.IID_IOPCServerList,
        OpcGuids.IID_IOPCEnumGUID,
        OpcGuids.IID_IEnumGUID,
        OpcGuids.IID_IRemUnknown,
        OpcGuids.IID_IRemUnknown2,
    };

    /// <summary>
    /// OPC Discovery (OPCEnum) IIDs to pre-declare in the initial DCE bind.
    /// </summary>
    public static IReadOnlyList<Guid> Discovery => s_discovery;
}
