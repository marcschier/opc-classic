//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// AOT-safe serializer for the OPC XML-DA 1.0 <c>SubscriptionCancel</c>
/// operation. The simplest XML-DA operation: two attributes in, one
/// attribute out, no nested elements.
/// </summary>
public static class SubscriptionCancelSerializer {
    /// <summary>Writes a complete SOAP envelope carrying a <c>SubscriptionCancel</c> request.</summary>
    public static void WriteRequest(SoapEnvelopeWriter writer, XmlDaSubscriptionCancelRequest request) {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.ServerSubHandle);

        writer.WriteEnvelopeStart();
        writer.WriteBodyStart();
        writer.WriteOperationStart("SubscriptionCancel");

        writer.Writer.WriteAttributeString("ServerSubHandle", request.ServerSubHandle);
        if (!string.IsNullOrEmpty(request.ClientRequestHandle)) {
            writer.Writer.WriteAttributeString("ClientRequestHandle", request.ClientRequestHandle);
        }

        writer.WriteOperationEnd();
        writer.WriteBodyEnd();
        writer.WriteEnvelopeEnd();
        writer.Flush();
    }

    /// <summary>
    /// Reads a SOAP-wrapped <c>SubscriptionCancelResponse</c>.
    /// </summary>
    public static XmlDaSubscriptionCancelResponse ReadResponse(SoapEnvelopeReader reader) {
        ArgumentNullException.ThrowIfNull(reader);

        string operationName = reader.AdvanceToOperationResponse();
        if (!string.Equals(operationName, "SubscriptionCancelResponse", StringComparison.Ordinal)) {
            throw new InvalidDataException(
                $"Expected SubscriptionCancelResponse but found '{operationName}'.");
        }

        string? clientHandle = reader.Reader.GetAttribute("ClientRequestHandle");
        return new XmlDaSubscriptionCancelResponse(clientHandle);
    }
}
