//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// Verifies the OpcInterfaceGenerator-emitted InterfaceId on every DA DCOM
// interface stub matches the canonical IID recorded in OpcClassic.Core.OpcGuids.
//
// If the generator drifts (e.g., emits a different format, fails to emit, picks
// wrong attribute argument), the per-interface assertion below catches it
// before any call-shim generator binds against a wrong IID.
//

using OpcClassic.Da.Dcom;
using TUnit.Core;

namespace OpcClassic.Da.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCServer_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCServer.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCServer);
    }

    [Test]
    public async Task IOPCBrowse_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCBrowse.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCBrowse);
    }

    [Test]
    public async Task IOPCBrowseServerAddressSpace_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCBrowseServerAddressSpace.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCBrowseServerAddressSpace);
    }

    [Test]
    public async Task IOPCItemProperties_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCItemProperties.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCItemProperties);
    }

    [Test]
    public async Task IOPCItemIO_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCItemIO.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCItemIO);
    }

    [Test]
    public async Task IOPCItemMgt_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCItemMgt.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCItemMgt);
    }

    [Test]
    public async Task IOPCGroupStateMgt_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCGroupStateMgt.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCGroupStateMgt);
    }

    [Test]
    public async Task IOPCGroupStateMgt2_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCGroupStateMgt2.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCGroupStateMgt2);
    }

    [Test]
    public async Task IOPCSyncIO_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCSyncIO.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCSyncIO);
    }

    [Test]
    public async Task IOPCSyncIO2_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCSyncIO2.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCSyncIO2);
    }

    [Test]
    public async Task IOPCAsyncIO2_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCAsyncIO2.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCAsyncIO2);
    }

    [Test]
    public async Task IOPCDataCallback_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCDataCallback.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCDataCallback);
    }
}
