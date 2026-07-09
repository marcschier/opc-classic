// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0 <c>Subscribe</c> operation.
/// </summary>
public static class SubscribeSerializer
{
    /// <summary>
    /// Writes a complete SOAP envelope carrying a <c>Subscribe</c> request.
    /// </summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaSubscribeRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Items);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("Subscribe");

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
        if (request.SubscriptionPingRate > 0)
        {
            writer.Writer.WriteAttributeString("SubscriptionPingRate",
                request.SubscriptionPingRate.ToString(CultureInfo.InvariantCulture));
        }

        writer.Writer.WriteStartElement("Options", XmlDaConstants.XmlDaNamespace);
        writer.Writer.WriteAttributeString("ReturnErrorText",
            request.ReturnErrorText ? "true" : "false");
        writer.Writer.WriteEndElement();

        writer.Writer.WriteStartElement("ItemList", XmlDaConstants.XmlDaNamespace);
        if (!string.IsNullOrEmpty(request.ItemPath))
        {
            writer.Writer.WriteAttributeString("ItemPath", request.ItemPath);
        }
        if (request.RequestedSamplingRate > 0)
        {
            writer.Writer.WriteAttributeString("RequestedSamplingRate",
                request.RequestedSamplingRate.ToString(CultureInfo.InvariantCulture));
        }
        writer.Writer.WriteAttributeString("EnableBuffering",
            request.EnableBuffering ? "true" : "false");

        foreach (var item in request.Items)
        {
            WriteItem(writer.Writer, item);
        }
        writer.Writer.WriteEndElement();

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    private static void WriteItem(XmlWriter xw, XmlDaSubscribeItem item)
    {
        xw.WriteStartElement("Items", XmlDaConstants.XmlDaNamespace);
        xw.WriteAttributeString("ItemName", item.ItemName);
        if (!string.IsNullOrEmpty(item.ClientItemHandle))
        {
            xw.WriteAttributeString("ClientItemHandle", item.ClientItemHandle);
        }
        if (item.RequestedSamplingRate > 0)
        {
            xw.WriteAttributeString("RequestedSamplingRate",
                item.RequestedSamplingRate.ToString(CultureInfo.InvariantCulture));
        }
        if (item.Deadband > 0f)
        {
            xw.WriteAttributeString("Deadband",
                item.Deadband.ToString(CultureInfo.InvariantCulture));
        }
        xw.WriteEndElement();
    }

    /// <summary>
    /// Reads a SOAP-wrapped <c>SubscribeResponse</c>.
    /// </summary>
    public static XmlDaSubscribeResponse ReadResponse(SoapEnvelopeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "SubscribeResponse", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Expected SubscribeResponse but found '{operationName}'.");
        }

        var serverState = XmlDaServerState.Running;
        string serverSubHandle = string.Empty;
        int revisedRate = 0;
        var items = new List<XmlDaItemValueResult>();

        var r = reader.Reader;
        if (!r.IsEmptyElement)
        {
            ReadResponseBody(r, ref serverState, ref serverSubHandle, ref revisedRate, items);
        }

        return new XmlDaSubscribeResponse(serverState, serverSubHandle, revisedRate, items);
    }

    private static void ReadResponseBody(
        XmlReader r,
        ref XmlDaServerState serverState,
        ref string serverSubHandle,
        ref int revisedRate,
        List<XmlDaItemValueResult> items)
    {
        int responseDepth = r.Depth;
        while (r.Read() && r.Depth > responseDepth)
        {
            if (r.NodeType != XmlNodeType.Element) { continue; }

            if (string.Equals(r.LocalName, "SubscribeResult", StringComparison.Ordinal))
            {
                ReadSubscribeResult(r, ref serverState, ref serverSubHandle, ref revisedRate);
            }
            else if (string.Equals(r.LocalName, "RItemList", StringComparison.Ordinal))
            {
                ReadItemList(r, items);
            }
        }
    }

    private static void ReadSubscribeResult(
        XmlReader r,
        ref XmlDaServerState serverState,
        ref string serverSubHandle,
        ref int revisedRate)
    {
        string? stateAttr = r.GetAttribute("ServerState");
        if (!string.IsNullOrEmpty(stateAttr))
        {
            serverState = ParseServerState(stateAttr);
        }
        string? handleAttr = r.GetAttribute("ServerSubHandle");
        if (!string.IsNullOrEmpty(handleAttr))
        {
            serverSubHandle = handleAttr;
        }
        string? rateAttr = r.GetAttribute("RevisedSamplingRate");
        if (!string.IsNullOrEmpty(rateAttr) &&
            int.TryParse(rateAttr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int rate))
        {
            revisedRate = rate;
        }
    }

    private static void ReadItemList(XmlReader r, List<XmlDaItemValueResult> items)
    {
        if (r.IsEmptyElement) { return; }
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
