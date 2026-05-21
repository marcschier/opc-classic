//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using OpcClassic.Batch.Dcom;
using TUnit.Core;

namespace OpcClassic.Batch.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCBatchServer_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCBatchServer.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCBatchServer);
    }

    [Test]
    public async Task IOPCBatchServer2_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCBatchServer2.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCBatchServer2);
    }

    [Test]
    public async Task IEnumOPCBatchSummary_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IEnumOPCBatchSummary.InterfaceId).IsEqualTo(OpcGuids.IID_IEnumOPCBatchSummary);
    }

    [Test]
    public async Task IOPCEnumerationSets_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCEnumerationSets.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCEnumerationSets);
    }
}
