//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace OpcClassic.Discovery.Tests;

public sealed class OpcDiscoveryFactoryTests
{
    [Test]
    public async Task Combines_entries_from_multiple_strategies()
    {
        var first = CreateEntry("10138C2C-0000-0000-0000-000000000011", "Vendor.First.1");
        var second = CreateEntry("10138C2C-0000-0000-0000-000000000012", "Vendor.Second.1");
        var discovery = new OpcDiscoveryFactory(
            new StubDiscovery(first),
            new StubDiscovery(second));

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(2);
        await Assert.That(entries[0]).IsEqualTo(first);
        await Assert.That(entries[1]).IsEqualTo(second);
    }

    [Test]
    public async Task Deduplicates_by_clsid_across_strategies()
    {
        var clsid = Guid.Parse("10138C2C-0000-0000-0000-000000000013");
        var first = CreateEntry(clsid, "Vendor.First.1");
        var duplicate = CreateEntry(clsid, "Vendor.Duplicate.1");
        var discovery = new OpcDiscoveryFactory(
            new StubDiscovery(first),
            new StubDiscovery(duplicate));

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(1);
        await Assert.That(entries[0]).IsEqualTo(first);
    }

    [Test]
    public async Task Empty_strategy_list_throws_ArgumentException()
    {
        await Assert.That(() => new OpcDiscoveryFactory()).Throws<ArgumentException>();
    }

    [Test]
    public async Task Skips_strategies_that_throw_NotImplementedException()
    {
        var local = CreateEntry("10138C2C-0000-0000-0000-000000000014", "Vendor.Local.1");
        var discovery = new OpcDiscoveryFactory(
            new ThrowingDiscovery(),
            new LocalEnum(new[] { local }, includeWindowsRegistry: false),
            new ThrowingDiscovery());

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(1);
        await Assert.That(entries[0]).IsEqualTo(local);
    }

    private static async Task<List<OpcServerEntry>> ToListAsync(IOpcDiscovery discovery)
    {
        var entries = new List<OpcServerEntry>();
        await foreach (var entry in discovery.DiscoverAsync())
        {
            entries.Add(entry);
        }

        return entries;
    }

    private static OpcServerEntry CreateEntry(string clsid, string progId) =>
        CreateEntry(Guid.Parse(clsid), progId);

    private static OpcServerEntry CreateEntry(Guid clsid, string progId) =>
        new(
            clsid,
            progId,
            progId,
            "localhost",
            Array.Empty<Guid>());

    private sealed class StubDiscovery : IOpcDiscovery
    {
        private readonly IReadOnlyList<OpcServerEntry> _entries;

        public StubDiscovery(params OpcServerEntry[] entries)
        {
            _entries = entries;
        }

        public async IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
            string? host = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask.ConfigureAwait(false);

            foreach (var entry in _entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
            }
        }
    }

    private sealed class ThrowingDiscovery : IOpcDiscovery
    {
        public IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
            string? host = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
