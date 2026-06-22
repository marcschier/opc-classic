// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Integration.Tests.Support;

namespace Opc.Classic.Integration.Tests.Da;

public sealed class DaBrowseContinuationPointTests
{
    private const int BrowseFilterItems = 3;
    private const int ItemCount = 50;
    private const string HierarchicalBrowseRoot = "Site.Building";

    [Test]
    [Category("Da.Loopback")]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Browse_with_zero_max_returns_all_items_without_continuation(bool hierarchical)
    {
        await using BrowseLoopback loopback = await BrowseLoopback.StartAsync(hierarchical);
        string? continuationPoint = string.Empty;

        await loopback.Proxy.BrowseAsync(
            loopback.ItemId,
            ref continuationPoint,
            maxElementsReturned: 0,
            browseFilter: BrowseFilterItems,
            elementNameFilter: "*",
            vendorFilter: string.Empty,
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: Array.Empty<int>(),
            out bool moreElements,
            out OpcBrowseElementResult[] browseElements,
            TestContext.Current!.CancellationToken);

        await Assert.That(moreElements).IsFalse();
        await Assert.That(continuationPoint).IsEqualTo(string.Empty);
        await Assert.That(browseElements.Select(static item => item.ItemId).SequenceEqual(loopback.ExpectedItemIds)).IsTrue();
    }

    [Test]
    [Category("Da.Loopback")]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Browse_pages_with_size_ten_until_all_items_are_returned(bool hierarchical)
    {
        await using BrowseLoopback loopback = await BrowseLoopback.StartAsync(hierarchical);

        (string[] itemIds, string[] continuationPoints) = await ReadPagedItemIdsAsync(loopback, pageSize: 10);

        await Assert.That(itemIds.SequenceEqual(loopback.ExpectedItemIds)).IsTrue();
        await Assert.That(itemIds.Distinct(StringComparer.Ordinal).Count()).IsEqualTo(ItemCount);
        await Assert.That(continuationPoints.Length).IsEqualTo(4);
        await Assert.That(continuationPoints.All(static token => !string.IsNullOrEmpty(token))).IsTrue();
    }

    [Test]
    [Category("Da.Loopback")]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Browse_page_size_seven_crosses_page_boundaries_without_losing_items(bool hierarchical)
    {
        await using BrowseLoopback loopback = await BrowseLoopback.StartAsync(hierarchical);

        (string[] itemIds, string[] continuationPoints) = await ReadPagedItemIdsAsync(loopback, pageSize: 7);

        await Assert.That(itemIds.SequenceEqual(loopback.ExpectedItemIds)).IsTrue();
        await Assert.That(itemIds.Distinct(StringComparer.Ordinal).Count()).IsEqualTo(ItemCount);
        await Assert.That(continuationPoints.Length).IsEqualTo(7);
        await Assert.That(itemIds[0]).IsEqualTo(loopback.ExpectedItemIds[0]);
        await Assert.That(itemIds[^1]).IsEqualTo(loopback.ExpectedItemIds[^1]);
    }

    [Test]
    [Category("Da.Loopback")]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Browse_reuses_server_issued_opaque_continuation_token_unmodified(bool hierarchical)
    {
        await using BrowseLoopback loopback = await BrowseLoopback.StartAsync(hierarchical);
        string? continuationPoint = string.Empty;

        await loopback.Proxy.BrowseAsync(
            loopback.ItemId,
            ref continuationPoint,
            maxElementsReturned: 10,
            browseFilter: BrowseFilterItems,
            elementNameFilter: "*",
            vendorFilter: string.Empty,
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: Array.Empty<int>(),
            out bool moreElements,
            out OpcBrowseElementResult[] firstPage,
            TestContext.Current!.CancellationToken);

        string issuedToken = continuationPoint ?? string.Empty;
        await Assert.That(moreElements).IsTrue();
        await Assert.That(issuedToken.StartsWith("opc-da-browse:", StringComparison.Ordinal)).IsTrue();
        await Assert.That(firstPage.Select(static item => item.ItemId).SequenceEqual(loopback.ExpectedItemIds.Take(10))).IsTrue();

        continuationPoint = issuedToken;
        await loopback.Proxy.BrowseAsync(
            loopback.ItemId,
            ref continuationPoint,
            maxElementsReturned: 10,
            browseFilter: BrowseFilterItems,
            elementNameFilter: "*",
            vendorFilter: string.Empty,
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: Array.Empty<int>(),
            out moreElements,
            out OpcBrowseElementResult[] secondPage,
            TestContext.Current.CancellationToken);

        await Assert.That(moreElements).IsTrue();
        await Assert.That(secondPage.Select(static item => item.ItemId).SequenceEqual(loopback.ExpectedItemIds.Skip(10).Take(10))).IsTrue();
        await Assert.That(issuedToken).IsNotEqualTo(continuationPoint ?? string.Empty);
    }

    [Test]
    [Category("Da.Loopback")]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Browse_with_invalid_continuation_token_returns_E_INVALIDCONTINUATIONPOINT(bool hierarchical)
    {
        await using BrowseLoopback loopback = await BrowseLoopback.StartAsync(hierarchical);
        string? continuationPoint = "bogus-continuation-token";

        OpcException exception = await CaptureAsync<OpcException>(() => loopback.Proxy.BrowseAsync(
            loopback.ItemId,
            ref continuationPoint,
            maxElementsReturned: 10,
            browseFilter: BrowseFilterItems,
            elementNameFilter: "*",
            vendorFilter: string.Empty,
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: Array.Empty<int>(),
            out _,
            out _,
            TestContext.Current!.CancellationToken));

        await Assert.That(exception.ResultId.Code).IsEqualTo(OpcResultId.InvalidContinuationPoint.Code);
    }

    private static async Task<(string[] ItemIds, string[] ContinuationPoints)> ReadPagedItemIdsAsync(
        BrowseLoopback loopback,
        int pageSize)
    {
        string? continuationPoint = string.Empty;
        var itemIds = new List<string>();
        var continuationPoints = new List<string>();
        while (true)
        {
            await loopback.Proxy.BrowseAsync(
                loopback.ItemId,
                ref continuationPoint,
                maxElementsReturned: pageSize,
                browseFilter: BrowseFilterItems,
                elementNameFilter: "*",
                vendorFilter: string.Empty,
                returnAllProperties: false,
                returnPropertyValues: false,
                propertyIds: Array.Empty<int>(),
                out bool moreElements,
                out OpcBrowseElementResult[] browseElements,
                TestContext.Current!.CancellationToken);

            itemIds.AddRange(browseElements.Select(static item => item.ItemId ?? string.Empty));
            if (!moreElements)
            {
                await Assert.That(continuationPoint).IsEqualTo(string.Empty);
                return (itemIds.ToArray(), continuationPoints.ToArray());
            }

            continuationPoints.Add(continuationPoint ?? string.Empty);
        }
    }

    private static async Task<TException> CaptureAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Expected {typeof(TException).Name}, but caught {exception.GetType().Name}.", exception);
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}, but no exception was thrown.");
    }

    private static IOpcAddressSpace CreateAddressSpace(bool hierarchical)
    {
        if (!hierarchical)
        {
            return new FlatAddressSpace(ExpectedFlatItemIds());
        }

        var addressSpace = new InMemoryAddressSpace("Site");
        addressSpace.AddBranch(HierarchicalBrowseRoot);
        for (int i = 1; i <= ItemCount; i++)
        {
            addressSpace.AddItem(HierarchicalBrowseRoot, $"Tag{i}");
        }

        return addressSpace;
    }

    private static string[] ExpectedFlatItemIds() =>
        Enumerable.Range(1, ItemCount).Select(static i => $"Tag{i}").ToArray();

    private static string[] ExpectedHierarchicalItemIds() =>
        Enumerable.Range(1, ItemCount).Select(static i => $"{HierarchicalBrowseRoot}.Tag{i}").ToArray();

    private sealed class BrowseLoopback : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly OpcDaServerHost _host;
        private readonly DcomCallChannel _channel;

        private BrowseLoopback(
            ServiceProvider provider,
            OpcDaServerHost host,
            DcomCallChannel channel,
            IOPCBrowseClientProxy proxy,
            string itemId,
            string[] expectedItemIds)
        {
            _provider = provider;
            _host = host;
            _channel = channel;
            Proxy = proxy;
            ItemId = itemId;
            ExpectedItemIds = expectedItemIds;
        }

        public IOPCBrowseClientProxy Proxy { get; }
        public string ItemId { get; }
        public string[] ExpectedItemIds { get; }

        public static async Task<BrowseLoopback> StartAsync(bool hierarchical)
        {
            IOpcAddressSpace addressSpace = CreateAddressSpace(hierarchical);
            ServiceProvider provider = BuildServiceProvider(addressSpace);
            OpcDaServerHost host = provider.GetRequiredService<OpcDaServerHost>();
            await host.StartAsync(TestContext.Current!.CancellationToken);
            DcomCallChannel channel = await ConnectBrowseClientAsync(host);
            return new BrowseLoopback(
                provider,
                host,
                channel,
                new IOPCBrowseClientProxy(channel),
                hierarchical ? HierarchicalBrowseRoot : string.Empty,
                hierarchical ? ExpectedHierarchicalItemIds() : ExpectedFlatItemIds());
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.DisposeAsync();
            await _host.StopAsync(CancellationToken.None);
            await _provider.DisposeAsync();
        }

        private static ServiceProvider BuildServiceProvider(IOpcAddressSpace addressSpace)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(addressSpace);
            services.AddSingleton<IOpcDaServer>(_ => new StubDaServer("Managed DA browse continuation test server"));
            services.AddSingleton<IOpcDaDataChangePublisher, OpcDaDataChangePublisher>();
            services.AddSingleton<OpcObjectRegistry>();
            services.AddSingleton<OpcDaServerHost>();
            services.AddSingleton<IOpcServerHost>(static sp => sp.GetRequiredService<OpcDaServerHost>());
            services.Configure<OpcDaServerOptions>(static o =>
            {
                o.Clsid = Guid.NewGuid();
                o.ProgId = "Managed.Da.BrowseContinuation.1";
                o.FriendlyName = "Managed DA browse continuation test server";
                o.ListenAddress = "127.0.0.1:0";
            });
            return services.BuildServiceProvider();
        }

        private static async Task<DcomCallChannel> ConnectBrowseClientAsync(OpcDaServerHost host)
        {
            var bound = (IPEndPoint?)host.LocalEndpoint
                ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");

            return await DcomCallChannelFactory.ConnectTcpAsync(
                bound.Address.ToString(),
                bound.Port,
                NoOpAuthContext.Instance,
                TestContext.Current!.CancellationToken);
        }
    }

    private sealed class FlatAddressSpace : IOpcAddressSpace
    {
        private readonly string[] _items;

        public FlatAddressSpace(string[] items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public bool IsHierarchical => false;

        public Task<OpcBrowseResult> BrowseAsync(
            string? branchPath,
            OpcBrowseElementKind kind,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(branchPath))
            {
                return Task.FromResult(OpcBrowseResult.Empty);
            }

            IReadOnlyList<string> items = kind == OpcBrowseElementKind.Branches ? Array.Empty<string>() : _items;
            return Task.FromResult(new OpcBrowseResult(Array.Empty<string>(), items));
        }

        public Task<string> GetItemIdAsync(
            string? currentBranchPath,
            string itemDataId,
            CancellationToken cancellationToken = default)
        {
            _ = currentBranchPath;
            ArgumentException.ThrowIfNullOrEmpty(itemDataId);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(itemDataId);
        }
    }
}
