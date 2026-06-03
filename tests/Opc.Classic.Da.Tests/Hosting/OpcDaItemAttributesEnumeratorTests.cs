//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for the stateful <see cref="OpcDaItemAttributesEnumerator"/> +
/// the OpcDaGroup.CreateEnumeratorAsync registry integration.
/// </summary>
public sealed class OpcDaItemAttributesEnumeratorTests
{
    [Test]
    public async Task NextAsync_returns_batches_until_exhausted()
    {
        OpcItemAttributes[] snapshot = BuildSnapshot(5);
        var enumerator = new OpcDaItemAttributesEnumerator(snapshot);

        await enumerator.NextAsync(3, out OpcItemAttributes[] first, out int firstFetched, TestContext.Current!.CancellationToken);
        await enumerator.NextAsync(3, out OpcItemAttributes[] second, out int secondFetched, TestContext.Current!.CancellationToken);
        await enumerator.NextAsync(3, out OpcItemAttributes[] third, out int thirdFetched, TestContext.Current!.CancellationToken);

        await Assert.That(first.Length).IsEqualTo(3);
        await Assert.That(firstFetched).IsEqualTo(3);
        await Assert.That(first[0].ItemId).IsEqualTo("Tag.0");
        await Assert.That(first[2].ItemId).IsEqualTo("Tag.2");
        await Assert.That(second.Length).IsEqualTo(2);
        await Assert.That(secondFetched).IsEqualTo(2);
        await Assert.That(second[0].ItemId).IsEqualTo("Tag.3");
        await Assert.That(third.Length).IsEqualTo(0); // exhausted
        await Assert.That(thirdFetched).IsEqualTo(0);
    }

    [Test]
    public async Task NextAsync_zero_or_negative_count_returns_empty_array()
    {
        var enumerator = new OpcDaItemAttributesEnumerator(BuildSnapshot(2));

        await enumerator.NextAsync(0, out OpcItemAttributes[] zero, out int zeroFetched, TestContext.Current!.CancellationToken);
        await enumerator.NextAsync(-1, out OpcItemAttributes[] neg, out int negFetched, TestContext.Current!.CancellationToken);

        await Assert.That(zero.Length).IsEqualTo(0);
        await Assert.That(neg.Length).IsEqualTo(0);
        await Assert.That(zeroFetched).IsEqualTo(0);
        await Assert.That(negFetched).IsEqualTo(0);
        await Assert.That(enumerator.Position).IsEqualTo(0);
    }

    [Test]
    public async Task SkipAsync_advances_cursor()
    {
        var enumerator = new OpcDaItemAttributesEnumerator(BuildSnapshot(5));

        await enumerator.SkipAsync(2, TestContext.Current!.CancellationToken);
        await enumerator.NextAsync(1, out OpcItemAttributes[] next, out int _, TestContext.Current!.CancellationToken);

        await Assert.That(enumerator.Position).IsEqualTo(3);
        await Assert.That(next[0].ItemId).IsEqualTo("Tag.2");
    }

    [Test]
    public async Task SkipAsync_beyond_end_clamps_at_length()
    {
        var enumerator = new OpcDaItemAttributesEnumerator(BuildSnapshot(3));

        await enumerator.SkipAsync(99, TestContext.Current!.CancellationToken);
        await enumerator.NextAsync(5, out OpcItemAttributes[] next, out int fetched, TestContext.Current!.CancellationToken);

        await Assert.That(enumerator.Position).IsEqualTo(3);
        await Assert.That(next.Length).IsEqualTo(0);
        await Assert.That(fetched).IsEqualTo(0);
    }

    [Test]
    public async Task ResetAsync_returns_cursor_to_zero()
    {
        var enumerator = new OpcDaItemAttributesEnumerator(BuildSnapshot(3));
        await enumerator.NextAsync(2, out OpcItemAttributes[] _, out int _, TestContext.Current!.CancellationToken);

        await enumerator.ResetAsync(TestContext.Current!.CancellationToken);

        await Assert.That(enumerator.Position).IsEqualTo(0);
        await enumerator.NextAsync(1, out OpcItemAttributes[] next, out int _, TestContext.Current!.CancellationToken);
        await Assert.That(next[0].ItemId).IsEqualTo("Tag.0");
    }

    [Test]
    public async Task NextAsync_LastBatchReportsTrueFetchedCount()
    {
        // Snapshot size 3, request 5 → fetched=3, array length=3.
        var enumerator = new OpcDaItemAttributesEnumerator(BuildSnapshot(3));

        await enumerator.NextAsync(5, out OpcItemAttributes[] batch, out int fetched, TestContext.Current!.CancellationToken);

        await Assert.That(batch.Length).IsEqualTo(3);
        await Assert.That(fetched).IsEqualTo(3);
    }

    [Test]
    public async Task CloneAsync_returns_interface_ref_with_correct_iid()
    {
        var enumerator = new OpcDaItemAttributesEnumerator(BuildSnapshot(3));

        IOpcInterfaceRef cloneRef = await enumerator.CloneAsync(TestContext.Current!.CancellationToken);

        await Assert.That(cloneRef.Iid).IsEqualTo(IEnumOPCItemAttributes.InterfaceId);
        await Assert.That(cloneRef.Ipid).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task CloneAsync_registers_with_registry_when_attached()
    {
        var registry = new OpcObjectRegistry();
        var enumerator = new OpcDaItemAttributesEnumerator(BuildSnapshot(3), registry);

        IOpcInterfaceRef cloneRef = await enumerator.CloneAsync(TestContext.Current!.CancellationToken);

        await Assert.That(registry.Contains(cloneRef.Ipid)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(cloneRef.Ipid, IEnumOPCItemAttributes.InterfaceId, out _)).IsTrue();
    }

    [Test]
    public async Task OpcDaGroup_CreateEnumeratorAsync_registers_enumerator_when_registry_attached()
    {
        var registry = new OpcObjectRegistry();
        var group = new OpcDaGroup("g", 1, 1, true, 1000, 0, 0f, 1033, registry);
        // Add 2 items so the snapshot is non-empty
        await group.AddItemsAsync(
            new[]
            {
                new OpcItemDef("", "Tag.A", true, 1, null, VarType.VT_I4),
                new OpcItemDef("", "Tag.B", false, 2, null, VarType.VT_R8),
            },
            out OpcItemResult[] _,
            out int[] _,
            TestContext.Current!.CancellationToken);

        IOpcInterfaceRef iref = await group.CreateEnumeratorAsync(
            IEnumOPCItemAttributes.InterfaceId,
            TestContext.Current!.CancellationToken);

        await Assert.That(iref.Iid).IsEqualTo(IEnumOPCItemAttributes.InterfaceId);
        await Assert.That(registry.Contains(iref.Ipid)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(iref.Ipid, IEnumOPCItemAttributes.InterfaceId, out _)).IsTrue();
    }

    private static OpcItemAttributes[] BuildSnapshot(int count) =>
        Enumerable.Range(0, count)
            .Select(i => new OpcItemAttributes(
                AccessPath: "",
                ItemId: $"Tag.{i}",
                Active: true,
                ClientHandle: i,
                ServerHandle: i + 100,
                AccessRights: 0x3,
                Blob: Array.Empty<byte>(),
                RequestedDataType: VarType.VT_I4,
                CanonicalDataType: VarType.VT_I4,
                EUType: 0,
                EUInfo: OpcVariant.Empty))
            .ToArray();
}
