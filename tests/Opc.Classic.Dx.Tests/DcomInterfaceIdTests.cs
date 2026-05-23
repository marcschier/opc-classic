//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dx.Dcom;
using TUnit.Core;

namespace Opc.Classic.Dx.Tests;

public sealed class DcomInterfaceIdTests
{
    [Test]
    public async Task IOPCConfiguration_InterfaceId_MatchesOpcDxIdl()
    {
        var expected = new Guid("C130D281-F4AA-4779-8846-C2C4CB444F2A");

        await Assert.That(IOPCConfiguration.InterfaceId).IsEqualTo(expected);
    }

    [Test]
    public async Task IOPCDXServer_InterfaceId_MatchesManagedDxShimValue()
    {
        var expected = new Guid("D5D8F8E9-6F45-43F2-B19E-3FAE3DA88A7C");

        await Assert.That(IOPCDXServer.InterfaceId).IsEqualTo(expected);
    }
}