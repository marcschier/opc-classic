//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

#pragma warning disable MA0048 // Request and response DTOs grouped for the trivial SubscriptionCancel operation

namespace OpcClassic.Xml;

/// <summary>An XML-DA <c>SubscriptionCancel</c> request payload.</summary>
/// <param name="ServerSubHandle">The subscription handle returned by an earlier <c>Subscribe</c> response.</param>
/// <param name="ClientRequestHandle">Free-form client correlation ID — echoed in the response.</param>
public sealed record XmlDaSubscriptionCancelRequest(
    string ServerSubHandle,
    string? ClientRequestHandle = null);

/// <summary>An XML-DA <c>SubscriptionCancelResponse</c> payload.</summary>
/// <param name="ClientRequestHandle">Server-echoed client correlation ID.</param>
public sealed record XmlDaSubscriptionCancelResponse(string? ClientRequestHandle);
