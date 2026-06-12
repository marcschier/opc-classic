//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Related Subscribe-operation DTO record types are clearer grouped than fragmented

namespace Opc.Classic.Xml;

/// <summary>A single item entry inside a <see cref="XmlDaSubscribeRequest"/>.</summary>
/// <param name="ItemName">Item identifier.</param>
/// <param name="ClientItemHandle">Client correlation ID.</param>
/// <param name="RequestedSamplingRate">Per-item sampling rate in ms. 0 = use the subscription's default.</param>
/// <param name="Deadband">Per-item percentage deadband (0..100) — only triggers updates when value moves more than this.</param>
public sealed record XmlDaSubscribeItem(
    string ItemName,
    string? ClientItemHandle = null,
    int RequestedSamplingRate = 0,
    float Deadband = 0f);

/// <summary>An XML-DA <c>Subscribe</c> request payload.</summary>
/// <param name="Header">Common LocaleID / ClientRequestHandle header.</param>
/// <param name="Items">Items to subscribe to.</param>
/// <param name="ItemPath">Vendor-defined path scoping (applied to all items).</param>
/// <param name="RequestedSamplingRate">Default sampling rate for items that don't specify their own.</param>
/// <param name="SubscriptionPingRate">Keep-alive ping rate in ms.</param>
/// <param name="ReturnValuesOnReply">If true, server returns the initial values in the Subscribe response.</param>
/// <param name="ReturnErrorText">If true, server includes ErrorText on per-item errors.</param>
/// <param name="EnableBuffering">If true, server buffers all changes (otherwise only the latest is kept per item).</param>
public sealed record XmlDaSubscribeRequest(
    XmlDaRequestHeader Header,
    IReadOnlyList<XmlDaSubscribeItem> Items,
    string ItemPath = "",
    int RequestedSamplingRate = 0,
    int SubscriptionPingRate = 0,
    bool ReturnValuesOnReply = false,
    bool ReturnErrorText = true,
    bool EnableBuffering = false);

/// <summary>An XML-DA <c>SubscribeResponse</c> payload.</summary>
/// <param name="ServerState">Top-level server state.</param>
/// <param name="ServerSubHandle">The opaque server-side subscription handle. Pass to SubscriptionPolledRefresh / SubscriptionCancel.</param>
/// <param name="RevisedSamplingRate">The server-revised default sampling rate; may differ from the requested rate.</param>
/// <param name="Items">Per-item initial values + per-item revised sampling rates (populated when ReturnValuesOnReply was true).</param>
public sealed record XmlDaSubscribeResponse(
    XmlDaServerState ServerState,
    string ServerSubHandle,
    int RevisedSamplingRate,
    IReadOnlyList<XmlDaItemValueResult> Items);
