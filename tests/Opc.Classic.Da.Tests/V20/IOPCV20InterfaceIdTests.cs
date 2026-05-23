// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Da.V20.Dcom;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.V20;

public sealed class IOPCV20InterfaceIdTests
{
    [Test]
    public async Task IOPCSyncIO_V20_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCSyncIO.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCSyncIO);
    }

    [Test]
    public async Task IOPCAsyncIO_V20_InterfaceId_MatchesOpcGuids()
    {
        await Assert.That(IOPCAsyncIO.InterfaceId).IsEqualTo(OpcGuids.IID_IOPCAsyncIO);
    }

    [Test]
    public async Task V20Interfaces_AreInLegacyDcomNamespace()
    {
        await Assert.That(typeof(IOPCSyncIO).Namespace).IsEqualTo("Opc.Classic.Da.V20.Dcom");
        await Assert.That(typeof(IOPCAsyncIO).Namespace).IsEqualTo("Opc.Classic.Da.V20.Dcom");
    }
}
