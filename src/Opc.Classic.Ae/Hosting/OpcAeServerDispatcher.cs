//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public Task QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.QueryEventCategoriesAsync(eventType, out eventCategories, out eventCategoryDescriptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.QueryConditionNamesAsync(eventCategory, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.QuerySubConditionNamesAsync(conditionName, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.QuerySourceConditionsAsync(source, cancellationToken);
    }

    /// <inheritdoc />
    public Task QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.QueryEventAttributesAsync(eventCategory, out attributeIds, out attributeDescriptions, out attributeTypes, cancellationToken);
    }

    /// <inheritdoc />
    public Task TranslateToItemIDsAsync(
        string source,
        int eventCategory,
        string conditionName,
        string subconditionName,
        int[] associatedAttributeIds,
        out string[] attributeItemIds,
        out string[] nodeNames,
        out Guid[] classIds,
        CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.TranslateToItemIDsAsync(source, eventCategory, conditionName, subconditionName, associatedAttributeIds, out attributeItemIds, out nodeNames, out classIds, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OpcConditionState> GetConditionStateAsync(string source, string conditionName, int[] attributeIds, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.GetConditionStateAsync(source, conditionName, attributeIds, cancellationToken);
    }

    /// <inheritdoc />
    public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.EnableConditionByAreaAsync(areas, cancellationToken);
    }

    /// <inheritdoc />
    public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.EnableConditionBySourceAsync(sources, cancellationToken);
    }

    /// <inheritdoc />
    public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.DisableConditionByAreaAsync(areas, cancellationToken);
    }

    /// <inheritdoc />
    public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.DisableConditionBySourceAsync(sources, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int[]> AckConditionAsync(
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        return server.AckConditionAsync(sources?.Length ?? 0, acknowledgerId, comment, sources!, conditionNames, activeTimes, cookies, cancellationToken);
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

    private sealed class EventSubscriptionAdapter : IOPCEventSubscriptionMgt, IOpcAeEventSinkRegistration, IAsyncDisposable
    {
        private readonly IAeSubscription _subscription;
        private readonly ConcurrentDictionary<int, int[]> _returnedAttributes = new();
        private readonly ConcurrentDictionary<int, IOPCEventSink> _sinks = new();
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _refreshes = new();
        private readonly ConcurrentDictionary<string, OpcEventNotification> _refreshSnapshot = new(StringComparer.Ordinal);
        private readonly CancellationTokenSource _disposeCts = new();
        private readonly Lock _pumpLock = new();
        private Task? _eventPumpTask;
        private int _bufferTime;
        private int _maxSize;
        private int _clientSubscription;
        private int _nextSinkCookie;

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

        public Task<int> AdviseEventSinkAsync(IOPCEventSink sink, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sink);
            cancellationToken.ThrowIfCancellationRequested();
            EnsureEventPumpStarted();
            int cookie = Interlocked.Increment(ref _nextSinkCookie);
            if (!_sinks.TryAdd(cookie, sink))
            {
                throw new OpcException(OpcResultId.Fail);
            }
            return Task.FromResult(cookie);
        }

        public Task UnadviseEventSinkAsync(int connection, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _sinks.TryRemove(connection, out _);
            return Task.CompletedTask;
        }

        public async Task RefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
            if (!_refreshes.TryAdd(connection, cts))
            {
                throw new OpcException(OpcAeResultId.Busy);
            }
            try
            {
                await _subscription.RefreshAsync(cts.Token).ConfigureAwait(false);
                await DeliverRefreshSnapshotAsync(connection, cts.Token).ConfigureAwait(false);
            }
            finally
            {
                _refreshes.TryRemove(connection, out _);
            }
        }

        public async Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_refreshes.TryGetValue(connection, out CancellationTokenSource? cts))
            {
                await cts.CancelAsync().ConfigureAwait(false);
            }
            await _subscription.CancelRefreshAsync(cancellationToken).ConfigureAwait(false);
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

        public async ValueTask DisposeAsync()
        {
            await _disposeCts.CancelAsync().ConfigureAwait(false);
            if (_eventPumpTask is not null)
            {
                try
                {
#pragma warning disable VSTHRD003
                    await _eventPumpTask.ConfigureAwait(false);
#pragma warning restore VSTHRD003
                }
                catch (OperationCanceledException)
                {
                }
            }
            foreach (CancellationTokenSource refresh in _refreshes.Values)
            {
                await refresh.CancelAsync().ConfigureAwait(false);
                refresh.Dispose();
            }
            _refreshes.Clear();
            _disposeCts.Dispose();
            await _subscription.DisposeAsync().ConfigureAwait(false);
        }

        private async Task CompleteSetStateAsync(Task setActiveTask, int bufferTime, int maxSize, int clientSubscription)
        {
#pragma warning disable VSTHRD003
            await setActiveTask.ConfigureAwait(false);
#pragma warning restore VSTHRD003
            _bufferTime = bufferTime;
            _maxSize = maxSize;
            _clientSubscription = clientSubscription;
        }

        private void EnsureEventPumpStarted()
        {
            lock (_pumpLock)
            {
                _eventPumpTask ??= Task.Run(PumpEventsAsync);
            }
        }

        private async Task PumpEventsAsync()
        {
            try
            {
                await foreach (EventNotification notification in _subscription.Events.WithCancellation(_disposeCts.Token).ConfigureAwait(false))
                {
                    OpcEventNotification opcNotification = ToOpcEventNotification(notification);
                    RememberRefreshCandidate(opcNotification);
                    await FanOutAsync(refresh: false, lastRefresh: false, [opcNotification], _disposeCts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
            {
            }
        }

        private async Task FanOutAsync(bool refresh, bool lastRefresh, OpcEventNotification[] events, CancellationToken cancellationToken)
        {
            if (_sinks.IsEmpty && !(refresh && lastRefresh))
            {
                return;
            }
            foreach (KeyValuePair<int, IOPCEventSink> sink in _sinks.ToArray())
            {
                await sink.Value.OnEventAsync(_clientSubscription, refresh, lastRefresh, events, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DeliverRefreshSnapshotAsync(int connection, CancellationToken cancellationToken)
        {
            if (!_sinks.TryGetValue(connection, out IOPCEventSink? sink))
            {
                throw new OpcException(OpcResultId.InvalidArg);
            }

            OpcEventNotification[] events = _refreshSnapshot.Values.ToArray();
            if (events.Length == 0)
            {
                await sink.OnEventAsync(_clientSubscription, refresh: true, lastRefresh: true, Array.Empty<OpcEventNotification>(), cancellationToken).ConfigureAwait(false);
                return;
            }

            int chunkSize = _maxSize > 0 ? Math.Min(_maxSize, events.Length) : events.Length;
            for (int offset = 0; offset < events.Length; offset += chunkSize)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int count = Math.Min(chunkSize, events.Length - offset);
                var fragment = new OpcEventNotification[count];
                Array.Copy(events, offset, fragment, 0, count);
                bool lastRefresh = offset + count >= events.Length;
                await sink.OnEventAsync(_clientSubscription, refresh: true, lastRefresh, fragment, cancellationToken).ConfigureAwait(false);
            }
        }

        private void RememberRefreshCandidate(OpcEventNotification notification)
        {
            if ((notification.EventType & (uint)EventType.Condition) == 0 || string.IsNullOrEmpty(notification.Source) || string.IsNullOrEmpty(notification.ConditionName))
            {
                return;
            }

            string key = string.Concat(notification.Source, "\u001F", notification.ConditionName);
            var state = (ConditionState)notification.NewState;
            if ((state & ConditionState.Active) != ConditionState.None || notification.AckRequired)
            {
                _refreshSnapshot[key] = notification;
            }
            else
            {
                _refreshSnapshot.TryRemove(key, out _);
            }
        }

        private OpcEventNotification ToOpcEventNotification(EventNotification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            return new OpcEventNotification(
                changeMask: 0,
                newState: (ushort)notification.NewState,
                source: notification.Source,
                time: notification.Time,
                message: notification.Message,
                eventType: (uint)notification.EventType,
                eventCategory: notification.EventCategory,
                severity: unchecked((uint)Math.Max(0, notification.Severity)),
                conditionName: notification.ConditionName,
                subconditionName: notification.SubConditionName,
                quality: notification.Quality,
                ackRequired: notification.AckRequired,
                activeTime: notification.ActiveTime == default ? notification.Time : notification.ActiveTime,
                cookie: unchecked((uint)notification.Cookie),
                eventAttributes: BuildEventAttributes(notification),
                actorId: notification.Actor);
        }

        private OpcVariant[] BuildEventAttributes(EventNotification notification)
        {
            if (_returnedAttributes.TryGetValue(unchecked((int)notification.EventCategory), out int[]? attributeIds) && attributeIds.Length > 0)
            {
                var values = new OpcVariant[attributeIds.Length];
                for (int i = 0; i < attributeIds.Length; i++)
                {
                    values[i] = notification.Attributes.TryGetValue(unchecked((uint)attributeIds[i]), out object? value)
                        ? ToVariant(value)
                        : OpcVariant.Null;
                }
                return values;
            }

            if (notification.Attributes.Count == 0)
            {
                return Array.Empty<OpcVariant>();
            }

            return notification.Attributes
                .OrderBy(static entry => entry.Key)
                .Select(static entry => ToVariant(entry.Value))
                .ToArray();
        }

        private static OpcVariant ToVariant(object? value) => value switch
        {
            null => OpcVariant.Null,
            OpcVariant variant => variant,
            string text => OpcVariant.FromString(text),
            bool boolean => OpcVariant.FromBoolean(boolean),
            sbyte int8 => OpcVariant.FromInt8(int8),
            byte uint8 => OpcVariant.FromUInt8(uint8),
            short int16 => OpcVariant.FromInt16(int16),
            ushort uint16 => OpcVariant.FromUInt16(uint16),
            int int32 => OpcVariant.FromInt32(int32),
            uint uint32 => OpcVariant.FromUInt32(uint32),
            long int64 => OpcVariant.FromInt64(int64),
            ulong uint64 => OpcVariant.FromUInt64(uint64),
            float single => OpcVariant.FromSingle(single),
            double real => OpcVariant.FromDouble(real),
            DateTime date => OpcVariant.FromDate(date),
            DateTimeOffset date => OpcVariant.FromDate(date.UtcDateTime),
            _ => OpcVariant.Empty,
        };

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
