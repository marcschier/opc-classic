//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Dcom.Remoting;

namespace Opc.Classic.Da.Dcom;

/// <summary>
/// Per-spec OPC DA interface set used to seed DCE/RPC presentation contexts.
/// </summary>
public static class OpcSpecCatalog {
    private static readonly Guid[] s_da =
    {
        IOPCServer.InterfaceId,
        IOPCCommon.InterfaceId,
        IOPCBrowse.InterfaceId,
        IOPCBrowseServerAddressSpace.InterfaceId,
        IOPCItemProperties.InterfaceId,
        IOPCItemIO.InterfaceId,
        IOPCItemMgt.InterfaceId,
        IOPCSyncIO.InterfaceId,
        IOPCSyncIO2.InterfaceId,
        IOPCAsyncIO2.InterfaceId,
        IOPCAsyncIO3.InterfaceId,
        IOPCGroupStateMgt.InterfaceId,
        IOPCGroupStateMgt2.InterfaceId,
        IConnectionPoint.InterfaceId,
        IConnectionPointContainer.InterfaceId,
        IRemUnknown.InterfaceId,
    };

    /// <summary>OPC DA IIDs to pre-declare in the initial DCE bind.</summary>
    public static IReadOnlyList<Guid> Da => s_da;
}
