// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
// AOT-safe serializer for OPC XML-DA 1.0 GetStatus request + response.
//
// Wire shape (request):
//   <soap:Envelope ...>
//     <soap:Body>
//       <GetStatus xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
//                  LocaleID="en-US" ClientRequestHandle="..." />
//     </soap:Body>
//   </soap:Envelope>
//
// Wire shape (response, abridged):
//   <GetStatusResponse>
//     <GetStatusResult RcvTime="..." ReplyTime="..." RevisedLocaleID="en-US"
//                      ServerState="running" />
//     <Status StartTime="..." ProductVersion="..." VendorInfo="..." >
//       <SupportedLocaleIDs>en-US</SupportedLocaleIDs>
//       ...
//       <SupportedInterfaceVersions>XML_DA_Version_1_0</SupportedInterfaceVersions>
//       ...
//       <StatusInfo>...</StatusInfo>
//     </Status>
//   </GetStatusResponse>
//

using System.Globalization;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0 <c>GetStatus</c> operation.
/// </summary>
public static class GetStatusSerializer
{
    /// <summary>
    /// Writes a complete SOAP envelope carrying a <c>GetStatus</c> request
    /// (request body is empty other than the LocaleID / ClientRequestHandle
    /// attributes inherited from <see cref="XmlDaRequestHeader"/>).
    /// </summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaRequestHeader header)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(header);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("GetStatus");

        if (!string.IsNullOrEmpty(header.LocaleId))
        {
            writer.Writer.WriteAttributeString("LocaleID", header.LocaleId);
        }

        if (!string.IsNullOrEmpty(header.ClientRequestHandle))
        {
            writer.Writer.WriteAttributeString("ClientRequestHandle", header.ClientRequestHandle);
        }

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    /// <summary>
    /// Reads a SOAP-wrapped <c>GetStatusResponse</c> and returns the
    /// decoded <see cref="XmlDaServerStatus"/>. Throws
    /// <see cref="InvalidDataException"/> if the response is malformed or
    /// is a SOAP Fault.
    /// </summary>
    public static XmlDaServerStatus ReadResponse(SoapEnvelopeReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "GetStatusResponse", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Expected GetStatusResponse but found '{operationName}'.");
        }

        XmlReader r = reader.Reader;

        // Defaults — overridden as we walk the response.
        DateTimeOffset startTime = default;
        string productVersion = string.Empty;
        string vendorInfo = string.Empty;
        var supportedLocales = new List<string>();
        var supportedInterfaceVersions = new List<string>();
        var serverState = XmlDaServerState.Running;
        string? statusInfo = null;

        // We're positioned on <GetStatusResponse>. Walk its descendants.
        if (!r.IsEmptyElement)
        {
            int responseDepth = r.Depth;
            while (r.Read() && r.Depth > responseDepth)
            {
                if (r.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (string.Equals(r.LocalName, "GetStatusResult", StringComparison.Ordinal))
                {
                    string? stateAttr = r.GetAttribute("ServerState");
                    if (!string.IsNullOrEmpty(stateAttr))
                    {
                        serverState = ParseServerState(stateAttr);
                    }
                }
                else if (string.Equals(r.LocalName, "Status", StringComparison.Ordinal))
                {
                    ReadStatusElement(r, ref startTime, ref productVersion, ref vendorInfo,
                        supportedLocales, supportedInterfaceVersions, ref statusInfo);
                }
            }
        }

        return new XmlDaServerStatus(
            StartTime: startTime,
            ProductVersion: productVersion,
            VendorInfo: vendorInfo,
            SupportedLocaleIds: supportedLocales,
            SupportedInterfaceVersions: supportedInterfaceVersions,
            ServerState: serverState,
            StatusInfo: statusInfo);
    }

    private static void ReadStatusElement(
        XmlReader r,
        ref DateTimeOffset startTime,
        ref string productVersion,
        ref string vendorInfo,
        List<string> supportedLocales,
        List<string> supportedInterfaceVersions,
        ref string? statusInfo)
    {
        string? startAttr = r.GetAttribute("StartTime");
        if (!string.IsNullOrEmpty(startAttr) &&
            DateTimeOffset.TryParse(startAttr, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var parsedStart))
        {
            startTime = parsedStart;
        }

        productVersion = r.GetAttribute("ProductVersion") ?? string.Empty;
        vendorInfo = r.GetAttribute("VendorInfo") ?? string.Empty;

        if (r.IsEmptyElement)
        {
            return;
        }

        ReadStatusChildren(r, supportedLocales, supportedInterfaceVersions, ref statusInfo);
    }

    private static void ReadStatusChildren(
        XmlReader r,
        List<string> supportedLocales,
        List<string> supportedInterfaceVersions,
        ref string? statusInfo)
    {
        // ReadElementContentAsString advances past the end tag, so we use an
        // 'alreadyAdvanced' flag to avoid double-reading and skipping siblings.
        int statusDepth = r.Depth;
        bool alreadyAdvanced = false;

        while (true)
        {
            if (!alreadyAdvanced)
            {
                if (!r.Read())
                {
                    break;
                }
            }
            alreadyAdvanced = false;

            if (r.Depth <= statusDepth)
            {
                break;
            }

            if (r.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (string.Equals(r.LocalName, "SupportedLocaleIDs", StringComparison.Ordinal))
            {
                supportedLocales.Add(r.ReadElementContentAsString());
                alreadyAdvanced = true;
            }
            else if (string.Equals(r.LocalName, "SupportedInterfaceVersions", StringComparison.Ordinal))
            {
                supportedInterfaceVersions.Add(r.ReadElementContentAsString());
                alreadyAdvanced = true;
            }
            else if (string.Equals(r.LocalName, "StatusInfo", StringComparison.Ordinal))
            {
                statusInfo = r.ReadElementContentAsString();
                alreadyAdvanced = true;
            }
            else
            {
                r.Skip();
                alreadyAdvanced = true;
            }
        }
    }

    private static XmlDaServerState ParseServerState(string value)
    {
        return value switch
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
}
