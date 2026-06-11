//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Related Write-operation DTO record types are clearer grouped than fragmented

using System.Collections.Generic;

namespace Opc.Classic.Xml;

/// <summary>A single item-write request entry inside a <see cref="XmlDaWriteRequest"/>.</summary>
/// <param name="ItemName">The OPC-Foundation-defined item identifier.</param>
/// <param name="ClientItemHandle">Free-form client correlation ID — echoed in the response.</param>
/// <param name="Value">The value to write.</param>
public sealed record XmlDaWriteItem(string ItemName, string? ClientItemHandle, XmlDaValue Value);

/// <summary>An XML-DA <c>Write</c> request payload.</summary>
/// <param name="Header">Common LocaleID / ClientRequestHandle header.</param>
/// <param name="Items">The items to write.</param>
/// <param name="ReturnValuesOnReply">If true, server echoes the current value/quality/timestamp per item in the response.</param>
/// <param name="ReturnErrorText">If true, server includes ErrorText on per-item errors.</param>
public sealed record XmlDaWriteRequest(
    XmlDaRequestHeader Header,
    IReadOnlyList<XmlDaWriteItem> Items,
    bool ReturnValuesOnReply = false,
    bool ReturnErrorText = true);

/// <summary>A single item-write result in an XML-DA <c>WriteResponse</c>.</summary>
/// <param name="ItemName">The server-echoed item name.</param>
/// <param name="ClientItemHandle">The server-echoed client handle.</param>
/// <param name="ResultId">Per-item result code (e.g. <c>S_OK</c>, <c>E_BADRIGHTS</c>) as the spec QName string.</param>
/// <param name="ErrorText">Optional error explanation (only when ReturnErrorText was set and the write failed).</param>
public sealed record XmlDaWriteItemResult(
    string ItemName,
    string? ClientItemHandle,
    string? ResultId,
    string? ErrorText)
{
    /// <summary>Type-safe interpretation of <see cref="ResultId"/>.</summary>
    public XmlDaErrorCode ResultCode => XmlDaErrorCodes.ParseResultId(ResultId);
}

/// <summary>An XML-DA <c>WriteResponse</c> payload.</summary>
/// <param name="ServerState">Top-level <c>WriteResult.ServerState</c>.</param>
/// <param name="Items">Per-item write results.</param>
public sealed record XmlDaWriteResponse(
    XmlDaServerState ServerState,
    IReadOnlyList<XmlDaWriteItemResult> Items);
