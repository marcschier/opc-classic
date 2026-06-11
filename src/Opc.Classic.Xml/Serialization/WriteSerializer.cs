//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// AOT-safe serializer for OPC XML-DA 1.0 Write request + response.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0 <c>Write</c> operation.
/// </summary>
public static class WriteSerializer
{
    /// <summary>Writes a complete SOAP envelope carrying a <c>Write</c> request.</summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaWriteRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Items);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("Write");

        if (!string.IsNullOrEmpty(request.Header.LocaleId))
        {
            writer.Writer.WriteAttributeString("LocaleID", request.Header.LocaleId);
        }
        if (!string.IsNullOrEmpty(request.Header.ClientRequestHandle))
        {
            writer.Writer.WriteAttributeString("ClientRequestHandle", request.Header.ClientRequestHandle);
        }

        writer.Writer.WriteAttributeString("ReturnValuesOnReply",
            request.ReturnValuesOnReply ? "true" : "false");

        writer.Writer.WriteStartElement("Options", XmlDaConstants.XmlDaNamespace);
        writer.Writer.WriteAttributeString("ReturnErrorText",
            request.ReturnErrorText ? "true" : "false");
        writer.Writer.WriteEndElement();

        writer.Writer.WriteStartElement("ItemList", XmlDaConstants.XmlDaNamespace);
        foreach (var item in request.Items)
        {
            WriteRequestItem(writer.Writer, item);
        }
        writer.Writer.WriteEndElement();

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    private static void WriteRequestItem(XmlWriter xw, XmlDaWriteItem item)
    {
        xw.WriteStartElement("Items", XmlDaConstants.XmlDaNamespace);
        xw.WriteAttributeString("ItemName", item.ItemName);
        if (!string.IsNullOrEmpty(item.ClientItemHandle))
        {
            xw.WriteAttributeString("ClientItemHandle", item.ClientItemHandle);
        }

        XmlDaValueSerializer.WriteValueElement(xw, item.Value);

        xw.WriteEndElement();
    }

    /// <summary>
    /// Reads a SOAP-wrapped <c>WriteResponse</c> and returns the decoded
    /// <see cref="XmlDaWriteResponse"/>. Throws on malformed payloads.
    /// </summary>
    public static XmlDaWriteResponse ReadResponse(SoapEnvelopeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "WriteResponse", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Expected WriteResponse but found '{operationName}'.");
        }

        var serverState = XmlDaServerState.Running;
        var items = new List<XmlDaWriteItemResult>();

        var r = reader.Reader;
        if (r.IsEmptyElement)
        {
            return new XmlDaWriteResponse(serverState, items);
        }

        int responseDepth = r.Depth;
        while (r.Read() && r.Depth > responseDepth)
        {
            if (r.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (string.Equals(r.LocalName, "WriteResult", StringComparison.Ordinal))
            {
                string? stateAttr = r.GetAttribute("ServerState");
                if (!string.IsNullOrEmpty(stateAttr))
                {
                    serverState = ParseServerState(stateAttr);
                }
            }
            else if (string.Equals(r.LocalName, "RItemList", StringComparison.Ordinal))
            {
                ReadItemList(r, items);
            }
        }

        return new XmlDaWriteResponse(serverState, items);
    }

    private static void ReadItemList(XmlReader r, List<XmlDaWriteItemResult> items)
    {
        if (r.IsEmptyElement)
        {
            return;
        }

        int listDepth = r.Depth;
        while (r.Read() && r.Depth > listDepth)
        {
            if (r.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (string.Equals(r.LocalName, "Items", StringComparison.Ordinal))
            {
                items.Add(ReadOneItem(r));
            }
            else
            {
                r.Skip();
            }
        }
    }

    private static XmlDaWriteItemResult ReadOneItem(XmlReader r)
    {
        string itemName = r.GetAttribute("ItemName") ?? string.Empty;
        string? clientHandle = r.GetAttribute("ClientItemHandle");
        string? resultId = r.GetAttribute("ResultID");
        string? errorText = null;

        if (!r.IsEmptyElement)
        {
            int itemDepth = r.Depth;
            bool alreadyAdvanced = false;
            while (true)
            {
                if (!alreadyAdvanced)
                {
                    if (!r.Read()) { break; }
                }
                alreadyAdvanced = false;
                if (r.Depth <= itemDepth) { break; }
                if (r.NodeType != XmlNodeType.Element) { continue; }

                if (string.Equals(r.LocalName, "ErrorText", StringComparison.Ordinal))
                {
                    errorText = r.ReadElementContentAsString();
                    alreadyAdvanced = true;
                }
                else
                {
                    r.Skip();
                    alreadyAdvanced = true;
                }
            }
        }

        return new XmlDaWriteItemResult(itemName, clientHandle, resultId, errorText);
    }

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
