//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Related Browse-operation DTO record types are clearer grouped than fragmented

namespace Opc.Classic.Xml;

/// <summary>
/// <c>BrowseFilter</c> values from the XML-DA spec.
/// </summary>
public enum XmlDaBrowseFilter
{
    /// <summary>Return both branches and leaves.</summary>
    All = 0,

    /// <summary>Return only branches (organizational levels).</summary>
    Branch = 1,

    /// <summary>Return only items (leaf nodes).</summary>
    Item = 2,
}

/// <summary>An XML-DA <c>Browse</c> request payload.</summary>
/// <param name="Header">Common LocaleID / ClientRequestHandle header.</param>
/// <param name="ItemName">The branch to browse below. Empty = browse the root.</param>
/// <param name="ItemPath">Optional vendor-defined path scoping.</param>
/// <param name="ContinuationPoint">Opaque token from a previous response to resume paging. Empty = start fresh.</param>
/// <param name="MaxElementsReturned">Server-side cap on response size. 0 = no limit.</param>
/// <param name="BrowseFilter">Branch-vs-item filter.</param>
/// <param name="ElementNameFilter">Optional name-pattern filter (server-specific syntax).</param>
public sealed record XmlDaBrowseRequest(
    XmlDaRequestHeader Header,
    string ItemName = "",
    string ItemPath = "",
    string ContinuationPoint = "",
    int MaxElementsReturned = 0,
    XmlDaBrowseFilter BrowseFilter = XmlDaBrowseFilter.All,
    string ElementNameFilter = "");

/// <summary>A single browse element in an XML-DA <c>BrowseResponse</c>.</summary>
/// <param name="Name">Display name of the element.</param>
/// <param name="ItemPath">Vendor-defined path (typically empty).</param>
/// <param name="ItemName">Fully-qualified item identifier (the address-space path).</param>
/// <param name="IsItem">True if the element is a leaf (a readable item).</param>
/// <param name="HasChildren">True if the element has child elements that could be browsed.</param>
public sealed record XmlDaBrowseElement(
    string Name,
    string ItemPath,
    string ItemName,
    bool IsItem,
    bool HasChildren);

/// <summary>An XML-DA <c>BrowseResponse</c> payload.</summary>
/// <param name="ServerState">Top-level <c>BrowseResult.ServerState</c>.</param>
/// <param name="Elements">The browsed elements.</param>
/// <param name="ContinuationPoint">Server-supplied token for paging; empty when no more pages exist.</param>
/// <param name="MoreElements">True if additional elements exist beyond what was returned.</param>
public sealed record XmlDaBrowseResponse(
    XmlDaServerState ServerState,
    IReadOnlyList<XmlDaBrowseElement> Elements,
    string ContinuationPoint,
    bool MoreElements);
