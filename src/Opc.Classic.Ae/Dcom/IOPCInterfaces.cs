//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC AE DCOM-projection interfaces. Each [OpcInterface] partial interface is
// extended by the OpcInterfaceGenerator to carry a compile-time-known
// InterfaceId. [OpcMethod] declarations drive generated call shims for the
// IDL shapes currently covered by the primitive, array, AE complex, multi-out,
// and interface-pointer codecs.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCEventServer not IOpcEventServer)
#pragma warning disable MA0048 // Multiple interface declarations grouped for readability
#pragma warning disable OPCGEN104, OPCGEN105 // IFACE pointer responses are decoded by generated client proxies.

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Generators;

namespace Opc.Classic.Ae.Dcom;

/// <summary><c>IOPCEventServer</c> — top-level AE server interface (IID_IOPCEventServer).</summary>
[OpcInterface("65168851-5783-11D1-84A0-00608CB8A7E9")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCEventServer
{
    /// <summary><c>IOPCEventServer::GetStatus</c> (opnum 3). Returns the AE server runtime state.</summary>
    /// <remarks>
    /// IDL: <c>[out] OPCEVENTSERVERSTATUS **ppEventServerStatus</c>. The double-star
    /// shape is NDR unique-pointer; <see cref="OpcUniquePointerAttribute"/> instructs
    /// the proxy to skip the 4-byte referent before invoking the struct codec.
    /// </remarks>
    [OpcMethod(3)]
    [return: OpcUniquePointer]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::CreateEventSubscription</c> (opnum 4). Creates a subscription object.</summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out IOPCEventSubscriptionMgt subscription,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QueryAvailableFilters</c> (opnum 5). Returns the supported filter mask.</summary>
    [OpcMethod(5)]
    Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QueryEventCategories</c> (opnum 6). Returns category IDs and descriptions.</summary>
    [OpcMethod(6)]
    [OpcGenerateMultiOutRecord]
    Task QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QueryConditionNames</c> (opnum 7). Returns condition names for an event category.</summary>
    [OpcMethod(7)]
    Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QuerySubConditionNames</c> (opnum 8). Returns sub-condition names for a condition.</summary>
    [OpcMethod(8)]
    Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QuerySourceConditions</c> (opnum 9). Returns condition names for a source.</summary>
    [OpcMethod(9)]
    Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QueryEventAttributes</c> (opnum 10). Returns attribute IDs, descriptions, and VARTYPEs.</summary>
    [OpcMethod(10)]
    [OpcGenerateMultiOutRecord]
    Task QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::TranslateToItemIDs</c> (opnum 11). Maps event attributes to DA item IDs.</summary>
    [OpcMethod(11)]
    [OpcGenerateMultiOutRecord]
    Task TranslateToItemIDsAsync(
        string source,
        int eventCategory,
        string conditionName,
        string subconditionName,
        int[] associatedAttributeIds,
        out string[] attributeItemIds,
        out string[] nodeNames,
        out Guid[] classIds,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::GetConditionState</c> (opnum 12). Returns a condition-state snapshot.</summary>
    [OpcMethod(12)]
    [return: OpcUniquePointer]
    Task<OpcConditionState> GetConditionStateAsync(
        string source,
        string conditionName,
        [OpcEmitArrayCount] int[] attributeIds,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::EnableConditionByArea</c> (opnum 13). Enables conditions by area.</summary>
    [OpcMethod(13)]
    Task EnableConditionByAreaAsync([OpcEmitArrayCount, OpcDeferredElements] string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::EnableConditionBySource</c> (opnum 14). Enables conditions by source.</summary>
    [OpcMethod(14)]
    Task EnableConditionBySourceAsync([OpcEmitArrayCount, OpcDeferredElements] string[] sources, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::DisableConditionByArea</c> (opnum 15). Disables conditions by area.</summary>
    [OpcMethod(15)]
    Task DisableConditionByAreaAsync([OpcEmitArrayCount, OpcDeferredElements] string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::DisableConditionBySource</c> (opnum 16). Disables conditions by source.</summary>
    [OpcMethod(16)]
    Task DisableConditionBySourceAsync([OpcEmitArrayCount, OpcDeferredElements] string[] sources, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::AckCondition</c> (opnum 17). Acknowledges condition events and returns per-event HRESULTs.</summary>
    /// <remarks>
    /// IDL signature: <c>HRESULT AckCondition(DWORD dwCount, LPWSTR szAcknowledgerID,
    /// LPWSTR szComment, [size_is(N)] LPWSTR *pszSource, [size_is(N)] LPWSTR *pszConditionName,
    /// [size_is(N)] FILETIME *pftActiveTime, [size_is(N)] DWORD *pdwCookie, ...)</c>.
    /// Note: dwCount is the FIRST wire field (before the string parameters), so we
    /// expose it as an explicit leading parameter rather than relying on
    /// <see cref="OpcEmitArrayCountAttribute"/> which would emit it before the
    /// arrays only.
    /// </remarks>
    [OpcMethod(17)]
    [return: OpcUniquePointer]
    Task<int[]> AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        [OpcDeferredElements] string[] sources,
        [OpcDeferredElements] string[] conditionNames,
        [OpcFileTimeElements] long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::CreateAreaBrowser</c> (opnum 18). Creates an area browser object.</summary>
    [OpcMethod(18)]
    Task CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        out IOPCEventAreaBrowser areaBrowser,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCEventServer2</c> — AE 1.10 interface derived from <c>IOPCEventServer</c>; new methods start at opnum 19.</summary>
[OpcInterface("71BBE88E-9564-4BCD-BCFC-71C558D94F2D")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCEventServer2
{
    /// <summary><c>IOPCEventServer2::EnableConditionByArea2</c> (opnum 19). Enables conditions by area with per-area HRESULTs.</summary>
    [OpcMethod(19)]
    [return: OpcUniquePointer]
    Task<int[]> EnableConditionByArea2Async([OpcEmitArrayCount, OpcDeferredElements] string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::EnableConditionBySource2</c> (opnum 20). Enables conditions by source with per-source HRESULTs.</summary>
    [OpcMethod(20)]
    [return: OpcUniquePointer]
    Task<int[]> EnableConditionBySource2Async([OpcEmitArrayCount, OpcDeferredElements] string[] sources, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::DisableConditionByArea2</c> (opnum 21). Disables conditions by area with per-area HRESULTs.</summary>
    [OpcMethod(21)]
    [return: OpcUniquePointer]
    Task<int[]> DisableConditionByArea2Async([OpcEmitArrayCount, OpcDeferredElements] string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::DisableConditionBySource2</c> (opnum 22). Disables conditions by source with per-source HRESULTs.</summary>
    [OpcMethod(22)]
    [return: OpcUniquePointer]
    Task<int[]> DisableConditionBySource2Async([OpcEmitArrayCount, OpcDeferredElements] string[] sources, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::GetEnableStateByArea</c> (opnum 23). Returns direct and effective enable state per area.</summary>
    [OpcMethod(23)]
    [OpcGenerateMultiOutRecord]
    Task GetEnableStateByAreaAsync(
        string[] areas,
        out bool[] enabled,
        out bool[] effectivelyEnabled,
        out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::GetEnableStateBySource</c> (opnum 24). Returns direct and effective enable state per source.</summary>
    [OpcMethod(24)]
    [OpcGenerateMultiOutRecord]
    Task GetEnableStateBySourceAsync(
        string[] sources,
        out bool[] enabled,
        out bool[] effectivelyEnabled,
        out int[] errors,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCEventSubscriptionMgt</c> — AE event subscription management (IID_IOPCEventSubscriptionMgt).</summary>
[OpcInterface("65168855-5783-11D1-84A0-00608CB8A7E9")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCEventSubscriptionMgt
{
    /// <summary><c>IOPCEventSubscriptionMgt::SetFilter</c> (opnum 3). Updates the subscription filter.</summary>
    [OpcMethod(3)]
    Task SetFilterAsync(
        int eventType,
        [OpcEmitArrayCount] int[] eventCategories,
        int lowSeverity,
        int highSeverity,
        [OpcEmitArrayCount, OpcDeferredElements] string[] areas,
        [OpcEmitArrayCount, OpcDeferredElements] string[] sources,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt::GetFilter</c> (opnum 4). Returns the current subscription filter.</summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task GetFilterAsync(
        out int eventType,
        out int[] eventCategories,
        out int lowSeverity,
        out int highSeverity,
        out string[] areas,
        out string[] sources,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt::SelectReturnedAttributes</c> (opnum 5). Selects attributes returned for a category.</summary>
    [OpcMethod(5)]
    Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt::GetReturnedAttributes</c> (opnum 6). Returns selected attributes for a category.</summary>
    [OpcMethod(6)]
    Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt::Refresh</c> (opnum 7). Starts a condition refresh.</summary>
    [OpcMethod(7)]
    Task RefreshAsync(int connection, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt::CancelRefresh</c> (opnum 8). Cancels a condition refresh.</summary>
    [OpcMethod(8)]
    Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt::GetState</c> (opnum 9). Returns active state, buffering, and client handle.</summary>
    [OpcMethod(9)]
    [OpcGenerateMultiOutRecord]
    Task GetStateAsync(
        out bool active,
        out int bufferTime,
        out int maxSize,
        out int clientSubscription,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt::SetState</c> (opnum 10). Updates state and returns revised buffering.</summary>
    [OpcMethod(10)]
    [OpcGenerateMultiOutRecord]
    Task SetStateAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCEventSubscriptionMgt2</c> — AE 1.10 keep-alive extensions (IID_IOPCEventSubscriptionMgt2).</summary>
[OpcInterface("94C955DC-3684-4CCB-AFAB-F898CE19AAC3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCEventSubscriptionMgt2
{
    /// <summary><c>IOPCEventSubscriptionMgt2::SetKeepAlive</c> (opnum 11). Sets the keep-alive time and returns the revised value.</summary>
    [OpcMethod(11)]
    Task<int> SetKeepAliveAsync(int keepAliveTime, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventSubscriptionMgt2::GetKeepAlive</c> (opnum 12). Returns the current keep-alive time.</summary>
    [OpcMethod(12)]
    Task<int> GetKeepAliveAsync(CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCEventAreaBrowser</c> — AE area-namespace browser (IID_IOPCEventAreaBrowser).</summary>
[OpcInterface("65168857-5783-11D1-84A0-00608CB8A7E9")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCEventAreaBrowser
{
    /// <summary><c>IOPCEventAreaBrowser::ChangeBrowsePosition</c> (opnum 3). Moves the browser cursor.</summary>
    [OpcMethod(3)]
    Task ChangeBrowsePositionAsync(int browseDirection, string? position, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventAreaBrowser::BrowseOPCAreas</c> (opnum 4). Returns an <c>IEnumString</c> over areas or sources.</summary>
    [OpcMethod(4)]
    Task BrowseOPCAreasAsync(
        int browseFilterType,
        string filterCriteria,
        out IEnumString enumString,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventAreaBrowser::GetQualifiedAreaName</c> (opnum 5). Returns a fully-qualified area name.</summary>
    [OpcMethod(5)]
    Task<string> GetQualifiedAreaNameAsync(string areaName, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventAreaBrowser::GetQualifiedSourceName</c> (opnum 6). Returns a fully-qualified source name.</summary>
    [OpcMethod(6)]
    Task<string> GetQualifiedSourceNameAsync(string sourceName, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCEventSink</c> — AE event-delivery callback sink (IID_IOPCEventSink).</summary>
[OpcInterface("6516885F-5783-11D1-84A0-00608CB8A7E9")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCEventSink
{
    /// <summary><c>IOPCEventSink::OnEvent</c> (opnum 3). Delivers event notifications to the client callback sink.</summary>
    [OpcMethod(3)]
    Task OnEventAsync(
        int clientSubscription,
        bool refresh,
        bool lastRefresh,
        OpcEventNotification[] events,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IEnumString</c> — COM string enumerator returned by AE browser methods.</summary>
[OpcInterface("00000101-0000-0000-C000-000000000046")]
[GenerateOpcProxy]
public partial interface IEnumString
{
}
