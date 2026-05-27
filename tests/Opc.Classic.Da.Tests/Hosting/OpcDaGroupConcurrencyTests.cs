//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Concurrency stress test for <see cref="OpcDaGroup"/>: the enumerator
/// snapshot must be stable under concurrent item add/remove, and parallel
/// readers must not observe corrupted state.
/// </summary>
public sealed class OpcDaGroupConcurrencyTests
{
    [Test]
    public async Task CreateEnumerator_during_concurrent_AddItems_produces_stable_snapshot()
    {
        var group = CreateGroup();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        Task[] adders = Enumerable.Range(0, 4).Select(i => Task.Run(async () =>
        {
            for (int j = 0; j < 50; j++)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    return;
                }
                var defs = new[] { new OpcItemDef("", $"T{i}.{j}", true, 1, null, VarType.VT_I4) };
                await group.AddItemsAsync(defs, out _, out _, cts.Token);
            }
        }, cts.Token)).ToArray();

        Task<bool> enumerator = Task.Run(async () =>
        {
            try
            {
                for (int k = 0; k < 100; k++)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    IOpcInterfaceRef iref = await group.CreateEnumeratorAsync(
                        IEnumOPCItemAttributes.InterfaceId, cts.Token);
                    if (iref.Ipid == Guid.Empty)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
        }, cts.Token);

        await Task.WhenAll(adders);
        bool stable = await enumerator;

        await Assert.That(stable).IsTrue();
        await Assert.That(group.ItemCount).IsGreaterThanOrEqualTo(50); // ~200 expected; race-friendly bound
    }

    [Test]
    public async Task RemoveItems_during_concurrent_Reads_does_not_throw()
    {
        var group = CreateGroup();
        var defs = Enumerable.Range(0, 100)
            .Select(i => new OpcItemDef("", $"Tag.{i}", true, i, null, VarType.VT_I4))
            .ToArray();
        await group.AddItemsAsync(defs, out OpcItemResult[] results, out _, TestContext.Current!.CancellationToken);
        int[] handles = results.Select(r => r.ServerHandle).ToArray();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        Task remover = Task.Run(async () =>
        {
            // Small initial delay so the reader gets at least one iteration in.
            await Task.Delay(50, cts.Token);
            foreach (int handle in handles)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    return;
                }
                await group.RemoveItemsAsync(new[] { handle }, cts.Token);
            }
        }, cts.Token);

        Task<int> reader = Task.Run(async () =>
        {
            int reads = 0;
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await group.ReadAsync(dataSource: 0, handles, out _, cts.Token);
                    reads++;
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
            return reads;
        }, cts.Token);

        await remover;
        cts.Cancel();
        int totalReads = await reader;

        // Reader must have completed at least one ReadAsync without throwing,
        // proving that concurrent RemoveItems doesn't corrupt the read path.
        await Assert.That(totalReads).IsGreaterThanOrEqualTo(1);
    }

    private static OpcDaGroup CreateGroup() => new(
        name: "stress",
        serverHandle: 1,
        clientHandle: 1,
        active: true,
        requestedUpdateRate: 1000,
        timeBias: 0,
        percentDeadband: 0f,
        localeId: 1033);
}
