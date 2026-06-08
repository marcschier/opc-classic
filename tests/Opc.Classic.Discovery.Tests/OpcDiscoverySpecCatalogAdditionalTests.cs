//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Discovery.Dcom;
using TUnit.Core;

namespace Opc.Classic.Discovery.Tests;

public sealed class OpcDiscoverySpecCatalogAdditionalTests
{
    [Test]
    public async Task Discovery_catalog_Exposes_concrete_iids_in_bind_order()
    {
        IReadOnlyList<Guid> discovery = OpcDiscoverySpecCatalog.Discovery;

        await Assert.That(discovery.Count).IsEqualTo(6);
        await Assert.That(discovery[0]).IsEqualTo(Guid.Parse("9DD0B56C-AD9E-43EE-8305-487F3188BF7A"));
        await Assert.That(discovery[1]).IsEqualTo(Guid.Parse("13486D50-4821-11D2-A494-3CB306C10000"));
        await Assert.That(discovery[2]).IsEqualTo(Guid.Parse("55C382C8-21C7-4E88-96C1-BECFB1E3F483"));
        await Assert.That(discovery[3]).IsEqualTo(Guid.Parse("0002E000-0000-0000-C000-000000000046"));
        await Assert.That(discovery[4]).IsEqualTo(Guid.Parse("00000131-0000-0000-C000-000000000046"));
        await Assert.That(discovery[5]).IsEqualTo(Guid.Parse("00000143-0000-0000-C000-000000000046"));
    }

    [Test]
    public async Task IOPCEnumGUIDClientProxy_Exposes_concrete_iid_and_opnums()
    {
        int next = ReadOpnum(nameof(IOPCEnumGUIDClientProxy.Opnums.Next));
        int skip = ReadOpnum(nameof(IOPCEnumGUIDClientProxy.Opnums.Skip));
        int reset = ReadOpnum(nameof(IOPCEnumGUIDClientProxy.Opnums.Reset));
        int clone = ReadOpnum(nameof(IOPCEnumGUIDClientProxy.Opnums.Clone));

        await Assert.That(IOPCEnumGUIDClientProxy.InterfaceId).IsEqualTo(Guid.Parse("55C382C8-21C7-4E88-96C1-BECFB1E3F483"));
        await Assert.That(next).IsEqualTo(3);
        await Assert.That(skip).IsEqualTo(4);
        await Assert.That(reset).IsEqualTo(5);
        await Assert.That(clone).IsEqualTo(6);
    }

    [Test]
    public async Task OpcEnumGuidNextResult_Construction_and_equality_preserve_values()
    {
        Guid[] classIds =
        [
            Guid.Parse("10138C2C-0000-0000-0000-00000000D001"),
            Guid.Parse("10138C2C-0000-0000-0000-00000000D002"),
        ];
        var result = new OpcEnumGuidNextResult(classIds, 2);
        var same = new OpcEnumGuidNextResult(classIds, 2);
        var differentFetched = result with { Fetched = 1 };

        await Assert.That(result.ClassIds.Length).IsEqualTo(2);
        await Assert.That(result.ClassIds[0]).IsEqualTo(Guid.Parse("10138C2C-0000-0000-0000-00000000D001"));
        await Assert.That(result.Fetched).IsEqualTo(2);
        await Assert.That(result).IsEqualTo(same);
        await Assert.That(result == differentFetched).IsFalse();
    }

    private static int ReadOpnum(string name) =>
        (int)typeof(IOPCEnumGUIDClientProxy.Opnums).GetField(name)!.GetRawConstantValue()!;
}
