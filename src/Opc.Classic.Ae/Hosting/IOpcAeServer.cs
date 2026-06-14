//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// Contract implemented by user code to provide an in-process managed AE server.
/// </summary>
public interface IOpcAeServer : IOPCEventServer
{
    /// <summary>
    /// Gets the AE server runtime status snapshot.
    /// </summary>
    new Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the AE filter mask supported by the server.
    /// </summary>
    new Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Negotiated server LCID for localized AE server strings.
    /// </summary>
    int LocaleId => this is IAeServer aeServer ? aeServer.LocaleId : 0;

    /// <summary>
    /// Sets the active locale for subsequent server-supplied strings.
    /// </summary>
    Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default) =>
        this is IAeServer aeServer
            ? aeServer.SetLocaleAsync(localeId, cancellationToken)
            : CompletedWithCancellationCheck(cancellationToken);

    /// <summary>
    /// Lists the locale IDs the server supports.
    /// </summary>
    Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
        this is IAeServer aeServer
            ? aeServer.GetSupportedLocalesAsync(cancellationToken)
            : Task.FromResult<IReadOnlyList<int>>(new[] { LocaleId });

    /// <summary>
    /// Resolves an HRESULT to the server's human-readable text in the current locale.
    /// </summary>
    Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
        this is IAeServer aeServer
            ? aeServer.GetErrorTextAsync(resultId, cancellationToken)
            : Task.FromResult(resultId.ToString());

    /// <summary>
    /// Supplies a client name that servers may use for diagnostics and logging.
    /// </summary>
    Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientName);
        return this is IAeServer aeServer
            ? aeServer.SetClientNameAsync(clientName, cancellationToken)
            : CompletedWithCancellationCheck(cancellationToken);
    }

    Task IOPCEventServer.CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out IOPCEventSubscriptionMgt subscription,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken) =>
        throw NotImplemented(out subscription, out revisedBufferTime, out revisedMaxSize);

    Task IOPCEventServer.QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken) =>
        throw NotImplemented(out eventCategories, out eventCategoryDescriptions);

    Task<string[]> IOPCEventServer.QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<string[]> IOPCEventServer.QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<string[]> IOPCEventServer.QuerySourceConditionsAsync(string source, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken) =>
        throw NotImplemented(out attributeIds, out attributeDescriptions, out attributeTypes);

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
        throw NotImplemented(out attributeItemIds, out nodeNames, out classIds);

    Task<OpcConditionState> IOPCEventServer.GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<int[]> IOPCEventServer.AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        out IOPCEventAreaBrowser areaBrowser,
        CancellationToken cancellationToken) =>
        throw NotImplemented(out areaBrowser);

    private static OpcException NotImplemented() => new(OpcResultId.NotImplemented);

#pragma warning disable VSTHRD200 // Helper used internally to forward immediate completion; no Async suffix needed.
    private static Task CompletedWithCancellationCheck(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
#pragma warning restore VSTHRD200

    private static OpcException NotImplemented<T>(out T value)
    {
        value = default!;
        return NotImplemented();
    }

    private static OpcException NotImplemented<T1, T2>(out T1 value1, out T2 value2)
    {
        value1 = default!;
        value2 = default!;
        return NotImplemented();
    }

    private static OpcException NotImplemented<T1, T2, T3>(out T1 value1, out T2 value2, out T3 value3)
    {
        value1 = default!;
        value2 = default!;
        value3 = default!;
        return NotImplemented();
    }
}
