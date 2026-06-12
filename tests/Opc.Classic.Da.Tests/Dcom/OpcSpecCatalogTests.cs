//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Remoting;

namespace Opc.Classic.Da.Tests.Dcom;

public sealed class OpcSpecCatalogTests
{
    [Test]
    public async Task Da_returns_prebind_iids_in_expected_order()
    {
        Guid[] expected =
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

        await Assert.That(OpcSpecCatalog.Da.Count).IsEqualTo(expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            await Assert.That(OpcSpecCatalog.Da[i]).IsEqualTo(expected[i]);
        }
    }
}
