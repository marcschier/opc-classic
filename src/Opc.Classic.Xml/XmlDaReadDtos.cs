//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Related Read-operation DTO record types are clearer grouped than fragmented

using System;
using System.Collections.Generic;

namespace Opc.Classic.Xml;

/// <summary>A single item-read request entry inside a <see cref="XmlDaReadRequest"/>.</summary>
/// <param name="ItemName">The OPC-Foundation-defined item identifier (e.g. <c>Channel1.Device1.Tag1</c>).</param>
/// <param name="ClientItemHandle">Free-form client correlation ID — echoed in the response.</param>
/// <param name="MaxAge">Maximum age (in ms) of a cached value the server may return. 0 = force read from device.</param>
public sealed record XmlDaReadItem(string ItemName, string? ClientItemHandle, int MaxAge = 0);

/// <summary>An XML-DA <c>Read</c> request payload.</summary>
/// <param name="Header">Common LocaleID / ClientRequestHandle header.</param>
/// <param name="Items">The items to read.</param>
/// <param name="ReturnErrorText">If true, server fills the ErrorText field on per-item errors.</param>
public sealed record XmlDaReadRequest(
    XmlDaRequestHeader Header,
    IReadOnlyList<XmlDaReadItem> Items,
    bool ReturnErrorText = true);

/// <summary>A single item-read result in an XML-DA <c>ReadResponse</c>.</summary>
/// <param name="ItemName">The server-echoed item name (matches the request).</param>
/// <param name="ClientItemHandle">The server-echoed client handle.</param>
/// <param name="Value">The current value, or <see langword="null"/> if the read failed.</param>
/// <param name="Quality">DA-style packed quality byte (low 8 bits of OpcQuality).</param>
/// <param name="Timestamp">Server-reported timestamp of the value (UTC).</param>
/// <param name="ResultId">Per-item result code (e.g. <c>S_OK</c>, <c>E_UNKNOWNITEMID</c>) as the spec QName string.</param>
public sealed record XmlDaItemValueResult(
    string ItemName,
    string? ClientItemHandle,
    XmlDaValue? Value,
    OpcQuality Quality,
    DateTimeOffset? Timestamp,
    string? ResultId) {
    /// <summary>Type-safe interpretation of <see cref="ResultId"/>.</summary>
    public XmlDaErrorCode ResultCode => XmlDaErrorCodes.ParseResultId(ResultId);
}

/// <summary>An XML-DA <c>ReadResponse</c> payload.</summary>
/// <param name="ServerState">Top-level <c>ReadResult.ServerState</c>.</param>
/// <param name="Items">Per-item read results.</param>
public sealed record XmlDaReadResponse(
    XmlDaServerState ServerState,
    IReadOnlyList<XmlDaItemValueResult> Items);
