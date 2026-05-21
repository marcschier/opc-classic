//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using OpcClassic.Hda.Dcom;
using TUnit.Core;

namespace OpcClassic.Hda.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCHDA_Server_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_Server.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_Server);
    }

    [Test]
    public async Task IOPCHDA_Browser_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_Browser.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_Browser);
    }

    [Test]
    public async Task IOPCHDA_SyncRead_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_SyncRead.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_SyncRead);
    }

    [Test]
    public async Task IOPCHDA_SyncUpdate_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_SyncUpdate.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_SyncUpdate);
    }

    [Test]
    public async Task IOPCHDA_SyncAnnotations_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_SyncAnnotations.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_SyncAnnotations);
    }

    [Test]
    public async Task IOPCHDA_AsyncRead_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_AsyncRead.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_AsyncRead);
    }

    [Test]
    public async Task IOPCHDA_AsyncUpdate_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_AsyncUpdate.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_AsyncUpdate);
    }

    [Test]
    public async Task IOPCHDA_AsyncAnnotations_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_AsyncAnnotations.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_AsyncAnnotations);
    }

    [Test]
    public async Task IOPCHDA_Playback_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_Playback.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_Playback);
    }

    [Test]
    public async Task IOPCHDA_DataCallback_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCHDA_DataCallback.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCHDA_DataCallback);
    }
}
