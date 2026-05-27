//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>AE dispatcher adapter that delegates to the source-generated IOPCEventServer dispatcher.</summary>
public sealed class OpcAeServerDispatcher : IOpcAeServerDispatcher
{
    private readonly IOpcAeServer _server;
    private readonly IOPCEventServerServerDispatcher _serverDispatcher;

    /// <summary>Initializes a new instance of the <see cref="OpcAeServerDispatcher" /> class.</summary>
    public OpcAeServerDispatcher(IOpcAeServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _serverDispatcher = new IOPCEventServerServerDispatcher(server);
    }

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCEventServer.InterfaceId)
        {
            return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
        }

        return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
            .ToNdrCallResult();
    }

    /// <inheritdoc />
    public async Task<IOpcAeAreaBrowserDispatcher> CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        await server.CreateAreaBrowserAsync(requestedInterfaceId, out IOPCEventAreaBrowser areaBrowser, cancellationToken).ConfigureAwait(false);
        if (areaBrowser is null)
        {
            throw new OpcException(OpcResultId.NotImplemented);
        }
        return areaBrowser is IOpcAeAreaBrowserDispatcher dispatcher
            ? dispatcher
            : new EventAreaBrowserAdapter(areaBrowser);
    }

    /// <inheritdoc />
    public Task<IOPCEventSubscriptionMgt> CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        try
        {
            Task createTask = server.CreateEventSubscriptionAsync(
                active,
                bufferTime,
                maxSize,
                clientSubscription,
                requestedInterfaceId,
                out IOPCEventSubscriptionMgt subscription,
                out revisedBufferTime,
                out revisedMaxSize,
                cancellationToken);
            return CompleteCreateEventSubscriptionAsync(createTask, subscription);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code && _server is IAeServer aeServer)
        {
            revisedBufferTime = bufferTime;
            revisedMaxSize = maxSize;
            return CreateEventSubscriptionAdapterAsync(aeServer, active, bufferTime, maxSize, clientSubscription, cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task RemoveSubscriptionAsync(IOPCEventSubscriptionMgt subscription, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        cancellationToken.ThrowIfCancellationRequested();
        if (subscription is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync().AsTask();
        }
        if (subscription is IDisposable disposable)
        {
            disposable.Dispose();
        }
        return Task.CompletedTask;
    }

    private static async Task<IOPCEventSubscriptionMgt> CompleteCreateEventSubscriptionAsync(Task createTask, IOPCEventSubscriptionMgt? subscription)
    {
#pragma warning disable VSTHRD003
        await createTask.ConfigureAwait(false);
#pragma warning restore VSTHRD003
        return subscription ?? throw new OpcException(OpcResultId.NotImplemented);
    }

    private static async Task<IOPCEventSubscriptionMgt> CreateEventSubscriptionAdapterAsync(
        IAeServer server,
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        CancellationToken cancellationToken)
    {
        IAeSubscription subscription = await server.CreateSubscriptionAsync(active, bufferTime, maxSize, cancellationToken).ConfigureAwait(false);
        return new EventSubscriptionAdapter(subscription, bufferTime, maxSize, clientSubscription);
    }

    private sealed class EventAreaBrowserAdapter : IOpcAeAreaBrowserDispatcher
    {
        private readonly IOPCEventAreaBrowser _browser;

        public EventAreaBrowserAdapter(IOPCEventAreaBrowser browser) =>
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));

        public Task ChangeBrowsePositionAsync(int browseDirection, string? position, CancellationToken cancellationToken = default) =>
            _browser.ChangeBrowsePositionAsync(browseDirection, position, cancellationToken);

        public async Task<string[]> BrowseAreasAsync(int browseFilterType, string filterCriteria, CancellationToken cancellationToken = default)
        {
            await _browser.BrowseOPCAreasAsync(browseFilterType, filterCriteria, out IEnumString enumString, cancellationToken).ConfigureAwait(false);
            if (enumString is IOpcAeStringEnumerator stringEnumerator)
            {
                return await stringEnumerator.ToArrayAsync(cancellationToken).ConfigureAwait(false);
            }
            throw new OpcException(OpcResultId.NotImplemented);
        }

        public Task<string> GetQualifiedAreaNameAsync(string areaName, CancellationToken cancellationToken = default) =>
            _browser.GetQualifiedAreaNameAsync(areaName, cancellationToken);

        public Task<string> GetQualifiedSourceNameAsync(string sourceName, CancellationToken cancellationToken = default) =>
            _browser.GetQualifiedSourceNameAsync(sourceName, cancellationToken);
    }

    private sealed class EventSubscriptionAdapter : IOPCEventSubscriptionMgt, IAsyncDisposable
    {
        private readonly IAeSubscription _subscription;
        private readonly ConcurrentDictionary<int, int[]> _returnedAttributes = new();
        private int _bufferTime;
        private int _maxSize;
        private int _clientSubscription;

        public EventSubscriptionAdapter(IAeSubscription subscription, int bufferTime, int maxSize, int clientSubscription)
        {
            _subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
            _bufferTime = bufferTime;
            _maxSize = maxSize;
            _clientSubscription = clientSubscription;
        }

        public async Task SetFilterAsync(
            int eventType,
            int[] eventCategories,
            int lowSeverity,
            int highSeverity,
            string[] areas,
            string[] sources,
            CancellationToken cancellationToken = default)
        {
            var filter = new SubscriptionFilter
            {
                EventTypes = (EventType)eventType,
                MinSeverity = lowSeverity,
                MaxSeverity = highSeverity,
                EventCategories = ToUInt32Array(eventCategories),
                Areas = Copy(areas),
                Sources = Copy(sources),
            };
            await _subscription.SetFilterAsync(filter, cancellationToken).ConfigureAwait(false);
        }

        public Task GetFilterAsync(
            out int eventType,
            out int[] eventCategories,
            out int lowSeverity,
            out int highSeverity,
            out string[] areas,
            out string[] sources,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SubscriptionFilter filter = _subscription.Filter;
            eventType = (int)filter.EventTypes;
            eventCategories = ToInt32Array(filter.EventCategories);
            lowSeverity = filter.MinSeverity;
            highSeverity = filter.MaxSeverity;
            areas = ToStringArray(filter.Areas);
            sources = ToStringArray(filter.Sources);
            return Task.CompletedTask;
        }

        public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _returnedAttributes[eventCategory] = Copy(attributeIds);
            return Task.CompletedTask;
        }

        public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_returnedAttributes.TryGetValue(eventCategory, out int[]? attributeIds)
                ? Copy(attributeIds)
                : Array.Empty<int>());
        }

        public Task RefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            _ = connection;
            return _subscription.RefreshAsync(cancellationToken);
        }

        public Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            _ = connection;
            return _subscription.CancelRefreshAsync(cancellationToken);
        }

        public Task GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            active = _subscription.Active;
            bufferTime = _bufferTime;
            maxSize = _maxSize;
            clientSubscription = _clientSubscription;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(
            bool active,
            int bufferTime,
            int maxSize,
            int clientSubscription,
            out int revisedBufferTime,
            out int revisedMaxSize,
            CancellationToken cancellationToken = default)
        {
            revisedBufferTime = bufferTime;
            revisedMaxSize = maxSize;
            Task setActiveTask = _subscription.SetActiveAsync(active, cancellationToken);
            return CompleteSetStateAsync(setActiveTask, bufferTime, maxSize, clientSubscription);
        }

        public ValueTask DisposeAsync() => _subscription.DisposeAsync();

        private async Task CompleteSetStateAsync(Task setActiveTask, int bufferTime, int maxSize, int clientSubscription)
        {
#pragma warning disable VSTHRD003
            await setActiveTask.ConfigureAwait(false);
#pragma warning restore VSTHRD003
            _bufferTime = bufferTime;
            _maxSize = maxSize;
            _clientSubscription = clientSubscription;
        }

        private static int[] Copy(int[]? values)
        {
            if (values is null || values.Length == 0)
            {
                return Array.Empty<int>();
            }

            var copy = new int[values.Length];
            Array.Copy(values, copy, values.Length);
            return copy;
        }

        private static string[] Copy(string[]? values)
        {
            if (values is null || values.Length == 0)
            {
                return Array.Empty<string>();
            }

            var copy = new string[values.Length];
            Array.Copy(values, copy, values.Length);
            return copy;
        }

        private static uint[] ToUInt32Array(int[]? values)
        {
            if (values is null || values.Length == 0)
            {
                return Array.Empty<uint>();
            }

            var result = new uint[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = unchecked((uint)values[i]);
            }
            return result;
        }

        private static int[] ToInt32Array(System.Collections.Generic.IReadOnlyList<uint> values)
        {
            if (values.Count == 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                result[i] = unchecked((int)values[i]);
            }
            return result;
        }

        private static string[] ToStringArray(System.Collections.Generic.IReadOnlyList<string> values)
        {
            if (values.Count == 0)
            {
                return Array.Empty<string>();
            }

            var result = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                result[i] = values[i];
            }
            return result;
        }
    }
}
