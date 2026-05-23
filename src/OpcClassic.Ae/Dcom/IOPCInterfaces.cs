//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC AE DCOM-projection interfaces. Each [OpcInterface] partial interface is
// extended by the OpcInterfaceGenerator to carry a compile-time-known
// InterfaceId. [OpcMethod] declarations drive generated call shims for the
// IDL shapes currently covered by the primitive, array, and AE complex codecs.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCEventServer not IOpcEventServer)
#pragma warning disable MA0048 // Multiple interface declarations grouped for readability

using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Generators;

namespace OpcClassic.Ae.Dcom;

/// <summary><c>IOPCEventServer</c> — top-level AE server interface (IID_IOPCEventServer).</summary>
[OpcInterface("65168851-5783-11D1-84A0-00608CB8A7E9")]
[GenerateOpcProxy]
public partial interface IOPCEventServer
{
    /// <summary><c>IOPCEventServer::GetStatus</c> (opnum 3). Returns the AE server runtime state.</summary>
    [OpcMethod(3)]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QueryAvailableFilters</c> (opnum 5). Returns the supported filter mask.</summary>
    [OpcMethod(5)]
    Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QueryConditionNames</c> (opnum 7). Returns condition names for an event category.</summary>
    [OpcMethod(7)]
    Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QuerySubConditionNames</c> (opnum 8). Returns sub-condition names for a condition.</summary>
    [OpcMethod(8)]
    Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::QuerySourceConditions</c> (opnum 9). Returns condition names for a source.</summary>
    [OpcMethod(9)]
    Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::GetConditionState</c> (opnum 10). Returns a condition-state snapshot.</summary>
    [OpcMethod(10)]
    Task<OpcConditionState> GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::EnableConditionByArea</c> (opnum 11). Enables conditions by area.</summary>
    [OpcMethod(11)]
    Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::EnableConditionBySource</c> (opnum 12). Enables conditions by source.</summary>
    [OpcMethod(12)]
    Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::DisableConditionByArea</c> (opnum 13). Disables conditions by area.</summary>
    [OpcMethod(13)]
    Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::DisableConditionBySource</c> (opnum 14). Disables conditions by source.</summary>
    [OpcMethod(14)]
    Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer::AckCondition</c> (opnum 15). Acknowledges condition events and returns per-event HRESULTs.</summary>
    [OpcMethod(15)]
    Task<int[]> AckConditionAsync(
        string acknowledgerId,
        string comment,
        long[] activeTimes,
        int[] cookies,
        string[] sources,
        string[] conditionNames,
        CancellationToken cancellationToken = default);

    // CreateEventSubscription/CreateAreaBrowser return COM interface pointers; multi-out catalog queries remain deferred.
}

/// <summary><c>IOPCEventServer2</c> — AE 1.10 enable-/disable-conditions extensions (IID_IOPCEventServer2).</summary>
[OpcInterface("71BBE88E-9564-4BCD-BCFC-71C558D94F2D")]
[GenerateOpcProxy]
public partial interface IOPCEventServer2
{
    /// <summary><c>IOPCEventServer2::EnableConditionByArea2</c> (opnum 16). Enables conditions by area with per-area HRESULTs.</summary>
    [OpcMethod(16)]
    Task<int[]> EnableConditionByArea2Async(string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::EnableConditionBySource2</c> (opnum 17). Enables conditions by source with per-source HRESULTs.</summary>
    [OpcMethod(17)]
    Task<int[]> EnableConditionBySource2Async(string[] sources, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::DisableConditionByArea2</c> (opnum 18). Disables conditions by area with per-area HRESULTs.</summary>
    [OpcMethod(18)]
    Task<int[]> DisableConditionByArea2Async(string[] areas, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventServer2::DisableConditionBySource2</c> (opnum 19). Disables conditions by source with per-source HRESULTs.</summary>
    [OpcMethod(19)]
    Task<int[]> DisableConditionBySource2Async(string[] sources, CancellationToken cancellationToken = default);

    // GetEnableStateByArea/Source have three correlated out arrays and are deferred until multi-out records are registered.
}

/// <summary><c>IOPCEventSubscriptionMgt</c> — AE event subscription management (IID_IOPCEventSubscriptionMgt).</summary>
[OpcInterface("65168855-5783-11D1-84A0-00608CB8A7E9")]
[GenerateOpcProxy]
public partial interface IOPCEventSubscriptionMgt
{
    /// <summary><c>IOPCEventSubscriptionMgt::SetFilter</c> (opnum 3). Updates the subscription filter.</summary>
    [OpcMethod(3)]
    Task SetFilterAsync(
        int eventType,
        int[] eventCategories,
        int lowSeverity,
        int highSeverity,
        string[] areas,
        string[] sources,
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

    // GetFilter/GetState/SetState require multi-out result records and are deferred.
}

/// <summary><c>IOPCEventSubscriptionMgt2</c> — AE 1.10 keep-alive extensions (IID_IOPCEventSubscriptionMgt2).</summary>
[OpcInterface("94C955DC-3684-4CCB-AFAB-F898CE19AAC3")]
[GenerateOpcProxy]
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
public partial interface IOPCEventAreaBrowser
{
    /// <summary><c>IOPCEventAreaBrowser::ChangeBrowsePosition</c> (opnum 3). Moves the browser cursor.</summary>
    [OpcMethod(3)]
    Task ChangeBrowsePositionAsync(int browseDirection, string? position, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventAreaBrowser::GetQualifiedAreaName</c> (opnum 5). Returns a fully-qualified area name.</summary>
    [OpcMethod(5)]
    Task<string> GetQualifiedAreaNameAsync(string areaName, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEventAreaBrowser::GetQualifiedSourceName</c> (opnum 6). Returns a fully-qualified source name.</summary>
    [OpcMethod(6)]
    Task<string> GetQualifiedSourceNameAsync(string sourceName, CancellationToken cancellationToken = default);

    // BrowseOPCAreas returns IEnumString and is deferred until COM interface-pointer returns are supported.
}

/// <summary><c>IOPCEventSink</c> — AE event-delivery callback sink (IID_IOPCEventSink).</summary>
[OpcInterface("6516885F-5783-11D1-84A0-00608CB8A7E9")]
[GenerateOpcProxy]
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
