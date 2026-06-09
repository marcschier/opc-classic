//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Related PolledRefresh DTO record types are clearer grouped than fragmented

using System;
using System.Collections.Generic;

namespace Opc.Classic.Xml;

/// <summary>An XML-DA <c>SubscriptionPolledRefresh</c> request payload.</summary>
/// <param name="Header">Common LocaleID / ClientRequestHandle header.</param>
/// <param name="ServerSubHandles">Handles for the subscriptions to refresh.</param>
/// <param name="HoldTime">Earliest time the server may return; permits aligning polls to a wall clock.</param>
/// <param name="WaitTime">Maximum time (ms) the server should wait for changes before returning.</param>
/// <param name="ReturnAllItems">If true, return current values for every item, not just those that have changed.</param>
/// <param name="ReturnErrorText">If true, server includes ErrorText on per-item errors.</param>
public sealed record XmlDaSubscriptionPolledRefreshRequest(
    XmlDaRequestHeader Header,
    IReadOnlyList<string> ServerSubHandles,
    DateTimeOffset? HoldTime = null,
    int WaitTime = 0,
    bool ReturnAllItems = false,
    bool ReturnErrorText = true);

/// <summary>Per-subscription item-list returned by SubscriptionPolledRefresh.</summary>
/// <param name="SubscriptionHandle">The ServerSubHandle these items belong to.</param>
/// <param name="Items">The changed (or all, when ReturnAllItems was set) items.</param>
public sealed record XmlDaSubscriptionItemList(
    string SubscriptionHandle,
    IReadOnlyList<XmlDaItemValueResult> Items);

/// <summary>An XML-DA <c>SubscriptionPolledRefreshResponse</c> payload.</summary>
/// <param name="ServerState">Top-level server state.</param>
/// <param name="DataBufferOverflow">True if the server's internal buffer overflowed during the polling interval.</param>
/// <param name="InvalidServerSubHandles">Subscription handles the server didn't recognise.</param>
/// <param name="ItemLists">Per-subscription value lists.</param>
public sealed record XmlDaSubscriptionPolledRefreshResponse(
    XmlDaServerState ServerState,
    bool DataBufferOverflow,
    IReadOnlyList<string> InvalidServerSubHandles,
    IReadOnlyList<XmlDaSubscriptionItemList> ItemLists);
