//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Commands.Dcom;

namespace Opc.Classic.Commands.Tests;

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
