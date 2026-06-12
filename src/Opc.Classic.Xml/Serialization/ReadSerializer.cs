//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// AOT-safe serializer for OPC XML-DA 1.0 Read request + response.
//

using System.Globalization;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0 <c>Read</c> operation.
/// </summary>
public static class ReadSerializer
{
    /// <summary>
    /// Writes a complete SOAP envelope carrying a <c>Read</c> request.
    /// </summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaReadRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Items);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("Read");

        if (!string.IsNullOrEmpty(request.Header.LocaleId))
        {
            writer.Writer.WriteAttributeString("LocaleID", request.Header.LocaleId);
        }
        if (!string.IsNullOrEmpty(request.Header.ClientRequestHandle))
        {
            writer.Writer.WriteAttributeString("ClientRequestHandle", request.Header.ClientRequestHandle);
        }

        writer.Writer.WriteStartElement("Options", XmlDaConstants.XmlDaNamespace);
        writer.Writer.WriteAttributeString("ReturnErrorText",
            request.ReturnErrorText ? "true" : "false");
        writer.Writer.WriteEndElement();

        writer.Writer.WriteStartElement("ItemList", XmlDaConstants.XmlDaNamespace);
        foreach (var item in request.Items)
        {
            writer.Writer.WriteStartElement("Items", XmlDaConstants.XmlDaNamespace);
            writer.Writer.WriteAttributeString("ItemName", item.ItemName);
            if (!string.IsNullOrEmpty(item.ClientItemHandle))
            {
                writer.Writer.WriteAttributeString("ClientItemHandle", item.ClientItemHandle);
            }
            if (item.MaxAge > 0)
            {
                writer.Writer.WriteAttributeString("MaxAge",
                    item.MaxAge.ToString(CultureInfo.InvariantCulture));
            }
            writer.Writer.WriteEndElement();
        }
        writer.Writer.WriteEndElement();

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    /// <summary>
    /// Reads a SOAP-wrapped <c>ReadResponse</c> and returns the decoded
    /// <see cref="XmlDaReadResponse"/>. Throws
    /// <see cref="InvalidDataException"/> on malformed payloads or SOAP Faults.
    /// </summary>
    public static XmlDaReadResponse ReadResponse(SoapEnvelopeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "ReadResponse", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Expected ReadResponse but found '{operationName}'.");
        }

        var serverState = XmlDaServerState.Running;
        var items = new List<XmlDaItemValueResult>();

        var r = reader.Reader;
        if (r.IsEmptyElement)
        {
            return new XmlDaReadResponse(serverState, items);
        }

        int responseDepth = r.Depth;
        while (r.Read() && r.Depth > responseDepth)
        {
            if (r.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (string.Equals(r.LocalName, "ReadResult", StringComparison.Ordinal))
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

        return new XmlDaReadResponse(serverState, items);
    }

    private static void ReadItemList(XmlReader r, List<XmlDaItemValueResult> items)
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

    private static XmlDaItemValueResult ReadOneItem(XmlReader r)
    {
        string itemName = r.GetAttribute("ItemName") ?? string.Empty;
        string? clientHandle = r.GetAttribute("ClientItemHandle");
        string? timestampAttr = r.GetAttribute("Timestamp");
        string? resultId = r.GetAttribute("ResultID");

        DateTimeOffset? timestamp = null;
        if (!string.IsNullOrEmpty(timestampAttr) &&
            DateTimeOffset.TryParse(timestampAttr, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var parsedTs))
        {
            timestamp = parsedTs;
        }

        XmlDaValue? value = null;
        var quality = new OpcQuality((ushort)OpcQualityKind.Good);

        if (r.IsEmptyElement)
        {
            return new XmlDaItemValueResult(itemName, clientHandle, value, quality, timestamp, resultId);
        }

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

            if (string.Equals(r.LocalName, "Value", StringComparison.Ordinal))
            {
                value = ReadValue(r);
                alreadyAdvanced = true;
            }
            else if (string.Equals(r.LocalName, "Quality", StringComparison.Ordinal))
            {
                quality = ReadQuality(r);
            }
            else
            {
                r.Skip();
                alreadyAdvanced = true;
            }
        }

        return new XmlDaItemValueResult(itemName, clientHandle, value, quality, timestamp, resultId);
    }

    private static XmlDaValue ReadValue(XmlReader r) => XmlDaValueSerializer.ReadValue(r);

    private static OpcQuality ReadQuality(XmlReader r)
    {
        string? field = r.GetAttribute("QualityField");
        string? limit = r.GetAttribute("LimitField");
        ushort raw = 0;
        if (!string.IsNullOrEmpty(field))
        {
            raw |= ParseQualityKind(field);
        }
        if (!string.IsNullOrEmpty(limit))
        {
            raw |= ParseQualityLimit(limit);
        }
        if (!r.IsEmptyElement)
        {
            r.Skip();
        }
        return new OpcQuality(raw);
    }

    private static ushort ParseQualityKind(string value) => value switch
    {
        "good" => (ushort)OpcQualityKind.Good,
        "goodNonSpecific" => (ushort)OpcQualityKind.Good,
        "uncertain" => (ushort)OpcQualityKind.Uncertain,
        "bad" => (ushort)OpcQualityKind.Bad,
        _ => (ushort)OpcQualityKind.Bad,
    };

    private static ushort ParseQualityLimit(string value) => value switch
    {
        "none" => (ushort)((int)OpcQualityLimit.NotLimited << 6),
        "low" => (ushort)((int)OpcQualityLimit.Low << 6),
        "high" => (ushort)((int)OpcQualityLimit.High << 6),
        "constant" => (ushort)((int)OpcQualityLimit.Constant << 6),
        _ => 0,
    };

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
