// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Discovery.Tests;

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
    public async Task Skips_strategies_that_throw_protocol_errors()
    {
        var local = CreateEntry("10138C2C-0000-0000-0000-000000000014", "Vendor.Local.1");
        var discovery = new OpcDiscoveryFactory(
            new ProtocolErrorDiscovery(),
            new LocalEnum(new[] { local }, includeWindowsRegistry: false));

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(1);
        await Assert.That(entries[0]).IsEqualTo(local);
    }

    [Test]
    public async Task Does_not_mask_NotImplementedException()
    {
        var discovery = new OpcDiscoveryFactory(new ThrowingDiscovery());

        Exception exception = await CaptureAsync(() => ToListAsync(discovery));

        await Assert.That(exception is NotImplementedException).IsTrue();
    }

    [Test]
    public async Task Includes_OpcEnumClient_when_configured()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000015");
        var server = new SyntheticOpcEnumServer()
            .AddServer(Opc.Classic.OpcGuids.CATID_OPCDAServer20, classId, "Vendor.OpcEnum.1", "Vendor OpcEnum", "Vendor.OpcEnum");
        var discovery = new OpcDiscoveryFactory(
            new OpcEnumClient("opc-host", server, new[] { Opc.Classic.OpcGuids.CATID_OPCDAServer20 }));

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(1);
        await Assert.That(entries[0].Clsid).IsEqualTo(classId);
        await Assert.That(entries[0].ProgId).IsEqualTo("Vendor.OpcEnum.1");
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

    private static async Task<Exception> CaptureAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ex;
        }

        throw new InvalidOperationException("Expected an exception.");
    }

    private sealed class ProtocolErrorDiscovery : IOpcDiscovery
    {
        public IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
            string? host = null,
            CancellationToken cancellationToken = default)
        {
            _ = host;
            _ = cancellationToken;
            throw new OpcException(new OpcResultId(unchecked((int)0x80004005u), "E_FAIL"));
        }
    }

    private sealed class ThrowingDiscovery : IOpcDiscovery
    {
        public IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
            string? host = null,
            CancellationToken cancellationToken = default)
        {
            _ = host;
            _ = cancellationToken;
            throw new NotImplementedException();
        }
    }
}
