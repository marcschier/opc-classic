// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0
/// <c>SubscriptionPolledRefresh</c> operation — the periodic polling
/// endpoint that drains accumulated value changes since the last poll.
/// </summary>
public static class SubscriptionPolledRefreshSerializer
{
    /// <summary>
    /// Writes a complete SOAP envelope carrying a polled-refresh request.
    /// </summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaSubscriptionPolledRefreshRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ServerSubHandles);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("SubscriptionPolledRefresh");

        if (!string.IsNullOrEmpty(request.Header.LocaleId))
        {
            writer.Writer.WriteAttributeString("LocaleID", request.Header.LocaleId);
        }
        if (!string.IsNullOrEmpty(request.Header.ClientRequestHandle))
        {
            writer.Writer.WriteAttributeString("ClientRequestHandle", request.Header.ClientRequestHandle);
        }
        if (request.HoldTime.HasValue)
        {
            writer.Writer.WriteAttributeString("HoldTime",
                request.HoldTime.Value.UtcDateTime.ToString("o", CultureInfo.InvariantCulture));
        }
        writer.Writer.WriteAttributeString("WaitTime",
            request.WaitTime.ToString(CultureInfo.InvariantCulture));
        writer.Writer.WriteAttributeString("ReturnAllItems",
            request.ReturnAllItems ? "true" : "false");

        writer.Writer.WriteStartElement("Options", XmlDaConstants.XmlDaNamespace);
        writer.Writer.WriteAttributeString("ReturnErrorText",
            request.ReturnErrorText ? "true" : "false");
        writer.Writer.WriteEndElement();

        foreach (var handle in request.ServerSubHandles)
        {
            writer.Writer.WriteStartElement("ServerSubHandles", XmlDaConstants.XmlDaNamespace);
            writer.Writer.WriteString(handle);
            writer.Writer.WriteEndElement();
        }

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    /// <summary>
    /// Reads a SOAP-wrapped polled-refresh response.
    /// </summary>
    public static XmlDaSubscriptionPolledRefreshResponse ReadResponse(SoapEnvelopeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "SubscriptionPolledRefreshResponse", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Expected SubscriptionPolledRefreshResponse but found '{operationName}'.");
        }

        var serverState = XmlDaServerState.Running;
        bool dataBufferOverflow = false;
        var invalidHandles = new List<string>();
        var itemLists = new List<XmlDaSubscriptionItemList>();

        var r = reader.Reader;
        if (!r.IsEmptyElement)
        {
            ReadResponseBody(r, ref serverState, ref dataBufferOverflow, invalidHandles, itemLists);
        }

        return new XmlDaSubscriptionPolledRefreshResponse(
            serverState, dataBufferOverflow, invalidHandles, itemLists);
    }

    private static void ReadResponseBody(
        XmlReader r,
        ref XmlDaServerState serverState,
        ref bool dataBufferOverflow,
        List<string> invalidHandles,
        List<XmlDaSubscriptionItemList> itemLists)
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

            if (string.Equals(r.LocalName, "SubscriptionPolledRefreshResult", StringComparison.Ordinal))
            {
                ReadResult(r, ref serverState, ref dataBufferOverflow);
            }
            else if (string.Equals(r.LocalName, "InvalidServerSubHandles", StringComparison.Ordinal))
            {
                invalidHandles.Add(r.ReadElementContentAsString());
                alreadyAdvanced = true;
            }
            else if (string.Equals(r.LocalName, "RItemList", StringComparison.Ordinal))
            {
                itemLists.Add(ReadItemList(r));
            }
            else
            {
                r.Skip();
                alreadyAdvanced = true;
            }
        }
    }

    private static void ReadResult(
        XmlReader r,
        ref XmlDaServerState serverState,
        ref bool dataBufferOverflow)
    {
        string? stateAttr = r.GetAttribute("ServerState");
        if (!string.IsNullOrEmpty(stateAttr))
        {
            serverState = ParseServerState(stateAttr);
        }
        string? overflowAttr = r.GetAttribute("DataBufferOverflow");
        if (!string.IsNullOrEmpty(overflowAttr))
        {
            dataBufferOverflow = overflowAttr.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                 overflowAttr.Equals("1", StringComparison.Ordinal);
        }
    }

    private static XmlDaSubscriptionItemList ReadItemList(XmlReader r)
    {
        string subscriptionHandle = r.GetAttribute("SubscriptionHandle") ?? string.Empty;
        var items = new List<XmlDaItemValueResult>();

        if (r.IsEmptyElement)
        {
            return new XmlDaSubscriptionItemList(subscriptionHandle, items);
        }

        int listDepth = r.Depth;
        while (r.Read() && r.Depth > listDepth)
        {
            if (r.NodeType != XmlNodeType.Element) { continue; }
            if (string.Equals(r.LocalName, "Items", StringComparison.Ordinal))
            {
                items.Add(ItemValueReader.ReadOneItem(r));
            }
            else
            {
                r.Skip();
            }
        }
        return new XmlDaSubscriptionItemList(subscriptionHandle, items);
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
