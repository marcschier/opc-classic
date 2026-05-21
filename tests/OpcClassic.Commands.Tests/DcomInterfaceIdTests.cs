//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using OpcClassic.Commands.Dcom;
using TUnit.Core;

namespace OpcClassic.Commands.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCCommandInformation_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCCommandInformation.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCCommandInformation);
    }

    [Test]
    public async Task IOPCCommandExecution_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCCommandExecution.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCCommandExecution);
    }

    [Test]
    public async Task IOPCCommandCallback_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCCommandCallback.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCCommandCallback);
    }
}
