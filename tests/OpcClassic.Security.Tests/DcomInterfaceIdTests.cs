//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using OpcClassic.Security.Dcom;
using TUnit.Core;

namespace OpcClassic.Security.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCSecurityNT_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCSecurityNT.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCSecurityNT);
    }

    [Test]
    public async Task IOPCSecurityPrivate_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCSecurityPrivate.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCSecurityPrivate);
    }
}
