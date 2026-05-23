//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Ae.Dcom;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCEventServer_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCEventServer.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCEventServer);
    }

    [Test]
    public async Task IOPCEventServer2_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCEventServer2.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCEventServer2);
    }

    [Test]
    public async Task IOPCEventSubscriptionMgt_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCEventSubscriptionMgt.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCEventSubscriptionMgt);
    }

    [Test]
    public async Task IOPCEventSubscriptionMgt2_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCEventSubscriptionMgt2.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCEventSubscriptionMgt2);
    }

    [Test]
    public async Task IOPCEventAreaBrowser_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCEventAreaBrowser.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCEventAreaBrowser);
    }

    [Test]
    public async Task IOPCEventSink_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCEventSink.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCEventSink);
    }
}
