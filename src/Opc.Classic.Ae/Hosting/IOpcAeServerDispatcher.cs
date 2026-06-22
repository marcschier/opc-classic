// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// Dispatches NDR-encoded AE DCOM calls to a managed AE server implementation.
/// </summary>
public interface IOpcAeServerDispatcher
{
    /// <summary>
    /// Routes an incoming interface/opnum request and returns an HRESULT plus NDR response body.
    /// </summary>
    Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a dispatcher for an <c>IOPCEventAreaBrowser</c> instance.
    /// </summary>
    Task<IOpcAeAreaBrowserDispatcher> CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Creates a dispatcher for an <c>IOPCEventSubscriptionMgt</c> instance.
    /// </summary>
    Task<IOPCEventSubscriptionMgt> CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default) =>
        throw NotImplemented(out revisedBufferTime, out revisedMaxSize);

    /// <summary>
    /// Returns event category IDs and descriptions.
    /// </summary>
    Task QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken = default) =>
        throw NotImplemented(out eventCategories, out eventCategoryDescriptions);

    /// <summary>
    /// Returns condition names for an event category.
    /// </summary>
    Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Returns sub-condition names for a condition.
    /// </summary>
    Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Returns condition names for a source.
    /// </summary>
    Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Returns event attribute IDs, descriptions, and VARIANT types.
    /// </summary>
    Task QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken = default) =>
        throw NotImplemented(out attributeIds, out attributeDescriptions, out attributeTypes);

    /// <summary>
    /// Maps event attributes to DA item identifiers.
    /// </summary>
    Task TranslateToItemIDsAsync(
        string source,
        int eventCategory,
        string conditionName,
        string subconditionName,
        int[] associatedAttributeIds,
        out string[] attributeItemIds,
        out string[] nodeNames,
        out Guid[] classIds,
        CancellationToken cancellationToken = default) =>
        throw NotImplemented(out attributeItemIds, out nodeNames, out classIds);

    /// <summary>
    /// Returns a condition-state snapshot.
    /// </summary>
    Task<OpcConditionState> GetConditionStateAsync(string source, string conditionName, int[] attributeIds, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Enables conditions by area.
    /// </summary>
    Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Enables conditions by source.
    /// </summary>
    Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Disables conditions by area.
    /// </summary>
    Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Disables conditions by source.
    /// </summary>
    Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Acknowledges conditions and returns per-event HRESULTs.
    /// </summary>
    /// <remarks>
    /// IDL signature: <c>HRESULT AckCondition(DWORD dwCount, LPWSTR szAcknowledgerID, LPWSTR szComment,
    /// [size_is(dwCount)] LPWSTR *pszSource, [size_is(dwCount)] LPWSTR *pszConditionName,
    /// [size_is(dwCount)] FILETIME *pftActiveTime, [size_is(dwCount)] DWORD *pdwCookie, ...)</c>.
    /// </remarks>
    Task<int[]> AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>
    /// Registers a client <c>IOPCEventSink</c> for a subscription connection point.
    /// </summary>
    Task<int> AdviseEventSinkAsync(IOPCEventSubscriptionMgt subscription, IOPCEventSink sink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        ArgumentNullException.ThrowIfNull(sink);
        cancellationToken.ThrowIfCancellationRequested();
        return subscription is IOpcAeEventSinkRegistration registration
            ? registration.AdviseEventSinkAsync(sink, cancellationToken)
            : throw new OpcException(OpcResultId.NotImplemented);
    }

    /// <summary>
    /// Unregisters a client <c>IOPCEventSink</c> from a subscription connection point.
    /// </summary>
    Task UnadviseEventSinkAsync(IOPCEventSubscriptionMgt subscription, int connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        cancellationToken.ThrowIfCancellationRequested();
        return subscription is IOpcAeEventSinkRegistration registration
            ? registration.UnadviseEventSinkAsync(connection, cancellationToken)
            : throw new OpcException(OpcResultId.NotImplemented);
    }

    /// <summary>
    /// Removes a subscription created by <see cref="CreateEventSubscriptionAsync" />.
    /// </summary>
    Task RemoveSubscriptionAsync(IOPCEventSubscriptionMgt subscription, CancellationToken cancellationToken = default)
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

    private static OpcException NotImplemented<T1, T2>(out T1 value1, out T2 value2)
    {
        value1 = default!;
        value2 = default!;
        return new OpcException(OpcResultId.NotImplemented);
    }

    private static OpcException NotImplemented<T1, T2, T3>(out T1 value1, out T2 value2, out T3 value3)
    {
        value1 = default!;
        value2 = default!;
        value3 = default!;
        return new OpcException(OpcResultId.NotImplemented);
    }
}
