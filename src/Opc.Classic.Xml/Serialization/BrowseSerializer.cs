//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0 <c>Browse</c> operation.
/// </summary>
public static class BrowseSerializer
{
    /// <summary>Writes a complete SOAP envelope carrying a <c>Browse</c> request.</summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaBrowseRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("Browse");

        if (!string.IsNullOrEmpty(request.Header.LocaleId))
        {
            writer.Writer.WriteAttributeString("LocaleID", request.Header.LocaleId);
        }
        if (!string.IsNullOrEmpty(request.Header.ClientRequestHandle))
        {
            writer.Writer.WriteAttributeString("ClientRequestHandle", request.Header.ClientRequestHandle);
        }

        if (!string.IsNullOrEmpty(request.ItemPath))
        {
            writer.Writer.WriteAttributeString("ItemPath", request.ItemPath);
        }
        if (!string.IsNullOrEmpty(request.ItemName))
        {
            writer.Writer.WriteAttributeString("ItemName", request.ItemName);
        }
        if (!string.IsNullOrEmpty(request.ContinuationPoint))
        {
            writer.Writer.WriteAttributeString("ContinuationPoint", request.ContinuationPoint);
        }
        if (request.MaxElementsReturned > 0)
        {
            writer.Writer.WriteAttributeString("MaxElementsReturned",
                request.MaxElementsReturned.ToString(CultureInfo.InvariantCulture));
        }
        writer.Writer.WriteAttributeString("BrowseFilter", MapFilter(request.BrowseFilter));
        if (!string.IsNullOrEmpty(request.ElementNameFilter))
        {
            writer.Writer.WriteAttributeString("ElementNameFilter", request.ElementNameFilter);
        }

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    private static string MapFilter(XmlDaBrowseFilter filter) => filter switch
    {
        XmlDaBrowseFilter.All => "all",
        XmlDaBrowseFilter.Branch => "branch",
        XmlDaBrowseFilter.Item => "item",
        _ => "all",
    };

    /// <summary>
    /// Reads a SOAP-wrapped <c>BrowseResponse</c> and returns the decoded
    /// <see cref="XmlDaBrowseResponse"/>.
    /// </summary>
    public static XmlDaBrowseResponse ReadResponse(SoapEnvelopeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "BrowseResponse", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Expected BrowseResponse but found '{operationName}'.");
        }

        var serverState = XmlDaServerState.Running;
        var elements = new List<XmlDaBrowseElement>();
        string continuationPoint = string.Empty;
        bool moreElements = false;

        var r = reader.Reader;
        if (!r.IsEmptyElement)
        {
            ReadResponseBody(r, ref serverState, elements, ref continuationPoint, ref moreElements);
        }

        return new XmlDaBrowseResponse(serverState, elements, continuationPoint, moreElements);
    }

    private static void ReadResponseBody(
        XmlReader r,
        ref XmlDaServerState serverState,
        List<XmlDaBrowseElement> elements,
        ref string continuationPoint,
        ref bool moreElements)
    {
        int responseDepth = r.Depth;
        bool alreadyAdvanced = false;
        while (true)
        {
            if (!alreadyAdvanced)
            {
                if (!r.Read()) { break; }
            }
            alreadyAdvanced = false;
            if (r.Depth <= responseDepth) { break; }
            if (r.NodeType != XmlNodeType.Element) { continue; }

            if (string.Equals(r.LocalName, "BrowseResult", StringComparison.Ordinal))
            {
                string? stateAttr = r.GetAttribute("ServerState");
                if (!string.IsNullOrEmpty(stateAttr))
                {
                    serverState = ParseServerState(stateAttr);
                }
            }
            else if (string.Equals(r.LocalName, "Elements", StringComparison.Ordinal))
            {
                elements.Add(ReadElement(r));
            }
            else if (string.Equals(r.LocalName, "ContinuationPoint", StringComparison.Ordinal))
            {
                continuationPoint = r.ReadElementContentAsString();
                alreadyAdvanced = true;
            }
            else if (string.Equals(r.LocalName, "MoreElements", StringComparison.Ordinal))
            {
                string raw = r.ReadElementContentAsString();
                moreElements = raw.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                               raw.Equals("1", StringComparison.Ordinal);
                alreadyAdvanced = true;
            }
            else
            {
                r.Skip();
                alreadyAdvanced = true;
            }
        }
    }

    private static XmlDaBrowseElement ReadElement(XmlReader r)
    {
        string name = r.GetAttribute("Name") ?? string.Empty;
        string itemPath = r.GetAttribute("ItemPath") ?? string.Empty;
        string itemName = r.GetAttribute("ItemName") ?? string.Empty;
        bool isItem = ParseBool(r.GetAttribute("IsItem"));
        bool hasChildren = ParseBool(r.GetAttribute("HasChildren"));

        if (!r.IsEmptyElement)
        {
            r.Skip();
        }
        return new XmlDaBrowseElement(name, itemPath, itemName, isItem, hasChildren);
    }

    private static bool ParseBool(string? value) =>
        !string.IsNullOrEmpty(value) &&
        (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("1", StringComparison.Ordinal));

    private static XmlDaServerState ParseServerState(string value) => value switch
    {
        "running" => XmlDaServerState.Running,
        "failed" => XmlDaServerState.Failed,
        "noConfig" => XmlDaServerState.NoConfig,
        "suspended" => XmlDaServerState.Suspended,
        "test" => XmlDaServerState.Test,
        "commFault" => XmlDaServerState.CommFault,
        _ => throw new InvalidDataException(
            $"Unknown XML-DA serverState value '{value}'."),
    };
}
