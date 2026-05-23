//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Related GetProperties DTO record types are clearer grouped than fragmented

using System.Collections.Generic;

namespace Opc.Classic.Xml;

/// <summary>An XML-DA <c>GetProperties</c> request payload.</summary>
/// <param name="Header">Common LocaleID / ClientRequestHandle header.</param>
/// <param name="ItemPath">Optional vendor-defined path scoping (applied to all items).</param>
/// <param name="ItemNames">Items whose properties to fetch.</param>
/// <param name="PropertyNames">Specific properties to fetch (e.g. "DataType"); empty + ReturnAllProperties=true returns everything.</param>
/// <param name="ReturnAllProperties">If true, server returns every property of every item.</param>
/// <param name="ReturnPropertyValues">If true, server includes the current value alongside the metadata.</param>
/// <param name="ReturnErrorText">If true, server includes ErrorText on per-property errors.</param>
public sealed record XmlDaGetPropertiesRequest(
    XmlDaRequestHeader Header,
    string ItemPath,
    IReadOnlyList<string> ItemNames,
    IReadOnlyList<string> PropertyNames,
    bool ReturnAllProperties = true,
    bool ReturnPropertyValues = false,
    bool ReturnErrorText = true);

/// <summary>A single property's metadata + (optionally) value.</summary>
/// <param name="Name">Property name (e.g. <c>DataType</c>, <c>EUType</c>).</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Value">Current value (only set when ReturnPropertyValues was true on the request).</param>
/// <param name="ResultId">Per-property result code; null on success.</param>
public sealed record XmlDaPropertyValue(
    string Name,
    string? Description,
    XmlDaValue? Value,
    string? ResultId);

/// <summary>All properties available for one item.</summary>
/// <param name="ItemName">The item these properties belong to.</param>
/// <param name="ItemPath">Vendor-defined path.</param>
/// <param name="Properties">The properties.</param>
/// <param name="ResultId">Per-item result code (e.g. E_UNKNOWNITEMID); null on success.</param>
public sealed record XmlDaItemPropertyList(
    string ItemName,
    string ItemPath,
    IReadOnlyList<XmlDaPropertyValue> Properties,
    string? ResultId);

/// <summary>An XML-DA <c>GetPropertiesResponse</c> payload.</summary>
public sealed record XmlDaGetPropertiesResponse(
    XmlDaServerState ServerState,
    IReadOnlyList<XmlDaItemPropertyList> PropertyLists);
