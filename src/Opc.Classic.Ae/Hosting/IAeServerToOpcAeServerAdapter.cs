// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ae.Dcom;
using Opc.Classic.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// Adapts a managed AE server that implements both <see cref="IOpcAeServer"/>
/// and <see cref="IAeServer"/> so that the source-generated
/// <c>IOPCEventServerServerDispatcher</c> can bridge wire-shape calls to the
/// high-level <see cref="IAeServer"/> API when the underlying server has not
/// supplied an explicit <see cref="IOPCEventServer"/> implementation.
/// </summary>
/// <remarks>
/// This adapter is wired in by <see cref="OpcAeServerHost"/> only for the
/// managed in-process listener path. The Windows CCW path (<c>OpcAeServerCcw</c>)
/// dispatches through <see cref="OpcAeServerDispatcher"/> directly and is
/// therefore unaffected. The adapter delegates every <see cref="IOPCEventServer"/>
/// method to the underlying server first and only falls back to
/// <see cref="IAeServer"/> when the underlying call throws
/// <see cref="OpcResultId.NotImplemented"/>, preserving any explicit interface
/// implementations the underlying server provides (for example
/// <c>SampleAeServer</c>'s explicit <c>GetConditionStateAsync</c> and
/// <c>AckConditionAsync</c> overrides).
/// </remarks>
public sealed class IAeServerToOpcAeServerAdapter : IOpcAeServer
{
    private readonly IOpcAeServer _underlying;
    private readonly IAeServer _aeServer;
    private readonly Func<IOpcInterfaceRef, IOPCEventSink>? _eventSinkFactory;

    public IAeServerToOpcAeServerAdapter(IOpcAeServer underlying, Func<IOpcInterfaceRef, IOPCEventSink>? eventSinkFactory = null)
    {
        ArgumentNullException.ThrowIfNull(underlying);
        _underlying = underlying;
        _aeServer = underlying as IAeServer
            ?? throw new ArgumentException("Underlying server must also implement IAeServer.", nameof(underlying));
        _eventSinkFactory = eventSinkFactory;
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
        _underlying.GetStatusAsync(cancellationToken);

    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
        _underlying.QueryAvailableFiltersAsync(cancellationToken);

    Task IOPCEventServer.CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out IOPCEventSubscriptionMgt subscription,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.CreateEventSubscriptionAsync(
                active, bufferTime, maxSize, clientSubscription, requestedInterfaceId,
                out subscription, out revisedBufferTime, out revisedMaxSize, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            revisedBufferTime = bufferTime;
            revisedMaxSize = maxSize;
            // The IOPCEventServer wire-shape requires the subscription tearoff
            // to be populated before the returned Task completes. AE server
            // implementations typically complete CreateSubscriptionAsync
            // synchronously; this matches the existing CCW bridge in
            // OpcAeServerDispatcher.CreateEventSubscriptionAdapterAsync.
#pragma warning disable VSTHRD002, VSTHRD103
            IAeSubscription aeSubscription = _aeServer
                .CreateSubscriptionAsync(active, bufferTime, maxSize, cancellationToken)
                .ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002, VSTHRD103
            subscription = OpcAeServerDispatcher.CreateEventSubscriptionAdapter(
                aeSubscription, bufferTime, maxSize, clientSubscription, _eventSinkFactory);
            return Task.CompletedTask;
        }
    }

    Task IOPCEventServer.QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.QueryEventCategoriesAsync(eventType, out eventCategories, out eventCategoryDescriptions, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
#pragma warning disable VSTHRD002, VSTHRD103
            IReadOnlyList<uint> categories = _aeServer
                .QueryEventCategoriesAsync((EventType)eventType, cancellationToken)
                .ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002, VSTHRD103
            eventCategories = new int[categories.Count];
            eventCategoryDescriptions = new string[categories.Count];
            for (int i = 0; i < categories.Count; i++)
            {
                eventCategories[i] = unchecked((int)categories[i]);
                eventCategoryDescriptions[i] = $"Category {categories[i]}";
            }
            return Task.CompletedTask;
        }
    }

    Task<string[]> IOPCEventServer.QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.QueryConditionNamesAsync(eventCategory, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return BridgeStringListAsync(
                _aeServer.QueryConditionNamesAsync(unchecked((uint)eventCategory), cancellationToken));
        }
    }

    Task<string[]> IOPCEventServer.QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.QuerySubConditionNamesAsync(conditionName, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return Task.FromResult(Array.Empty<string>());
        }
    }

    Task<string[]> IOPCEventServer.QuerySourceConditionsAsync(string source, CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.QuerySourceConditionsAsync(source, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return Task.FromResult(Array.Empty<string>());
        }
    }

    Task IOPCEventServer.QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.QueryEventAttributesAsync(eventCategory, out attributeIds, out attributeDescriptions, out attributeTypes, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            // IAeServer does not model per-category attribute metadata; report
            // none so the wire round-trip succeeds with an empty attribute list.
            attributeIds = [];
            attributeDescriptions = [];
            attributeTypes = [];
            return Task.CompletedTask;
        }
    }

    Task IOPCEventServer.TranslateToItemIDsAsync(
        string source,
        int eventCategory,
        string conditionName,
        string subconditionName,
        int[] associatedAttributeIds,
        out string[] attributeItemIds,
        out string[] nodeNames,
        out Guid[] classIds,
        CancellationToken cancellationToken) =>
        _underlying.TranslateToItemIDsAsync(source, eventCategory, conditionName, subconditionName, associatedAttributeIds, out attributeItemIds, out nodeNames, out classIds, cancellationToken);

    Task<OpcConditionState> IOPCEventServer.GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken) =>
        _underlying.GetConditionStateAsync(source, conditionName, attributeIds, cancellationToken);

    Task IOPCEventServer.EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.EnableConditionByAreaAsync(areas, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return BridgeOpcResultAsync(_aeServer.EnableConditionsByAreaAsync(areas, cancellationToken));
        }
    }

    Task IOPCEventServer.EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.EnableConditionBySourceAsync(sources, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return BridgeOpcResultAsync(_aeServer.EnableConditionsByAreaAsync(sources, cancellationToken));
        }
    }

    Task IOPCEventServer.DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.DisableConditionByAreaAsync(areas, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return BridgeOpcResultAsync(_aeServer.DisableConditionsByAreaAsync(areas, cancellationToken));
        }
    }

    Task IOPCEventServer.DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken)
    {
        IOPCEventServer underlying = _underlying;
        try
        {
            return underlying.DisableConditionBySourceAsync(sources, cancellationToken);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return BridgeOpcResultAsync(_aeServer.DisableConditionsByAreaAsync(sources, cancellationToken));
        }
    }

    Task<int[]> IOPCEventServer.AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken) =>
        _underlying.AckConditionAsync(dwCount, acknowledgerId, comment, sources, conditionNames, activeTimes, cookies, cancellationToken);

    Task IOPCEventServer.CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        out IOPCEventAreaBrowser areaBrowser,
        CancellationToken cancellationToken) =>
        _underlying.CreateAreaBrowserAsync(requestedInterfaceId, out areaBrowser, cancellationToken);

    private static async Task<string[]> BridgeStringListAsync(Task<IReadOnlyList<string>> task)
    {
#pragma warning disable VSTHRD003
        IReadOnlyList<string> names = await task.ConfigureAwait(false);
#pragma warning restore VSTHRD003
        return names switch
        {
            string[] arr => arr,
            _ => names.ToArray(),
        };
    }

    private static async Task BridgeOpcResultAsync(Task<OpcResultId> task)
    {
#pragma warning disable VSTHRD003
        OpcResultId result = await task.ConfigureAwait(false);
#pragma warning restore VSTHRD003
        if (result.Code != OpcResultId.Ok.Code)
        {
            throw new OpcException(result);
        }
    }
}
