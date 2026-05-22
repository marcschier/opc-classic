//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using OpcClassic.Xml;
using TUnit.Core;

namespace OpcClassic.Xml.Tests;

public sealed class XmlDaServerStateTests
{
    private static int IntValue(XmlDaServerState state) => (int)state;

    [Test]
    public async Task Running_IsZero()
    {
        await Assert.That(IntValue(XmlDaServerState.Running)).IsEqualTo(0);
    }

    [Test]
    public async Task Failed_IsOne()
    {
        await Assert.That(IntValue(XmlDaServerState.Failed)).IsEqualTo(1);
    }

    [Test]
    public async Task AllStates_AreOrderedPerSpec()
    {
        await Assert.That(IntValue(XmlDaServerState.NoConfig)).IsEqualTo(2);
        await Assert.That(IntValue(XmlDaServerState.Suspended)).IsEqualTo(3);
        await Assert.That(IntValue(XmlDaServerState.Test)).IsEqualTo(4);
        await Assert.That(IntValue(XmlDaServerState.CommFault)).IsEqualTo(5);
    }
}
