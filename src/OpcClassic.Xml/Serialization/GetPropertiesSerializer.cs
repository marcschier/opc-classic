//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace OpcClassic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0 <c>GetProperties</c> operation.
/// </summary>
public static class GetPropertiesSerializer
{
    /// <summary>Writes a complete SOAP envelope carrying a <c>GetProperties</c> request.</summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaGetPropertiesRequest request)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ItemNames);
        ArgumentNullException.ThrowIfNull(request.PropertyNames);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("GetProperties");

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
        writer.Writer.WriteAttributeString("ReturnAllProperties",
            request.ReturnAllProperties ? "true" : "false");
        writer.Writer.WriteAttributeString("ReturnPropertyValues",
            request.ReturnPropertyValues ? "true" : "false");
        writer.Writer.WriteAttributeString("ReturnErrorText",
            request.ReturnErrorText ? "true" : "false");

        foreach (var itemName in request.ItemNames)
        {
            writer.Writer.WriteStartElement("ItemIDs", XmlDaConstants.XmlDaNamespace);
            writer.Writer.WriteAttributeString("ItemName", itemName);
            writer.Writer.WriteEndElement();
        }

        foreach (var propertyName in request.PropertyNames)
        {
            writer.Writer.WriteStartElement("PropertyNames", XmlDaConstants.XmlDaNamespace);
            writer.Writer.WriteAttributeString("Name", propertyName);
            writer.Writer.WriteEndElement();
        }

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    /// <summary>Reads a SOAP-wrapped <c>GetPropertiesResponse</c>.</summary>
    public static XmlDaGetPropertiesResponse ReadResponse(SoapEnvelopeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "GetPropertiesResponse", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Expected GetPropertiesResponse but found '{operationName}'.");
        }

        var serverState = XmlDaServerState.Running;
        var propertyLists = new List<XmlDaItemPropertyList>();

        var r = reader.Reader;
        if (!r.IsEmptyElement)
        {
            ReadResponseBody(r, ref serverState, propertyLists);
        }

        return new XmlDaGetPropertiesResponse(serverState, propertyLists);
    }

    private static void ReadResponseBody(
        XmlReader r,
        ref XmlDaServerState serverState,
        List<XmlDaItemPropertyList> propertyLists)
    {
        int responseDepth = r.Depth;
        while (r.Read() && r.Depth > responseDepth)
        {
            if (r.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (string.Equals(r.LocalName, "GetPropertiesResult", StringComparison.Ordinal))
            {
                string? stateAttr = r.GetAttribute("ServerState");
                if (!string.IsNullOrEmpty(stateAttr))
                {
                    serverState = ParseServerState(stateAttr);
                }
            }
            else if (string.Equals(r.LocalName, "PropertyLists", StringComparison.Ordinal))
            {
                propertyLists.Add(ReadPropertyList(r));
            }
        }
    }

    private static XmlDaItemPropertyList ReadPropertyList(XmlReader r)
    {
        string itemName = r.GetAttribute("ItemName") ?? string.Empty;
        string itemPath = r.GetAttribute("ItemPath") ?? string.Empty;
        string? resultId = r.GetAttribute("ResultID");
        var properties = new List<XmlDaPropertyValue>();

        if (r.IsEmptyElement)
        {
            return new XmlDaItemPropertyList(itemName, itemPath, properties, resultId);
        }

        int listDepth = r.Depth;
        while (r.Read() && r.Depth > listDepth)
        {
            if (r.NodeType != XmlNodeType.Element) { continue; }
            if (string.Equals(r.LocalName, "Properties", StringComparison.Ordinal))
            {
                properties.Add(ReadOneProperty(r));
            }
            else
            {
                r.Skip();
            }
        }
        return new XmlDaItemPropertyList(itemName, itemPath, properties, resultId);
    }

    private static XmlDaPropertyValue ReadOneProperty(XmlReader r)
    {
        string name = r.GetAttribute("Name") ?? string.Empty;
        string? description = r.GetAttribute("Description");
        string? resultId = r.GetAttribute("ResultID");
        XmlDaValue? value = null;

        if (r.IsEmptyElement)
        {
            return new XmlDaPropertyValue(name, description, value, resultId);
        }

        int propDepth = r.Depth;
        bool alreadyAdvanced = false;
        while (true)
        {
            if (!alreadyAdvanced)
            {
                if (!r.Read()) { break; }
            }
            alreadyAdvanced = false;
            if (r.Depth <= propDepth) { break; }
            if (r.NodeType != XmlNodeType.Element) { continue; }

            if (string.Equals(r.LocalName, "Value", StringComparison.Ordinal))
            {
                value = ReadValue(r);
                alreadyAdvanced = true;
            }
            else
            {
                r.Skip();
                alreadyAdvanced = true;
            }
        }

        return new XmlDaPropertyValue(name, description, value, resultId);
    }

    private static XmlDaValue ReadValue(XmlReader r)
    {
        string xsiType = r.GetAttribute("type", XmlDaConstants.XsiNamespace) ?? string.Empty;
        string content = r.ReadElementContentAsString();
        return ParseValueByType(xsiType, content);
    }

    private static XmlDaValue ParseValueByType(string xsiType, string content)
    {
        int colon = xsiType.LastIndexOf(':');
        string localType = colon >= 0 ? xsiType[(colon + 1)..] : xsiType;

        return localType switch
        {
            "string" or "QName" => XmlDaValue.OfString(content),
            "int" or "integer" => XmlDaValue.OfInt32(int.Parse(content, CultureInfo.InvariantCulture)),
            "double" => XmlDaValue.OfDouble(double.Parse(content, CultureInfo.InvariantCulture)),
            "boolean" => XmlDaValue.OfBoolean(
                content.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                content.Equals("1", StringComparison.Ordinal)),
            "dateTime" => XmlDaValue.OfDateTime(
                DateTimeOffset.Parse(content, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)),
            _ => XmlDaValue.Unknown(content),
        };
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
