//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using OpcClassic.Dx.Dcom;
using TUnit.Core;

namespace OpcClassic.Dx.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCConfiguration_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCConfiguration.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCConfiguration);
    }
}
