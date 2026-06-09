//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Xml;

/// <summary>
/// XML namespace URIs and SOAP action constants from the OPC XML-DA 1.0
/// specification. All elements / attributes on the wire are bound to
/// <see cref="XmlDaNamespace"/>; the SOAPAction header carries the
/// per-operation constant.
/// </summary>
public static class XmlDaConstants {
    /// <summary>The XML namespace URI for OPC XML-DA 1.0 elements.</summary>
    public const string XmlDaNamespace = "http://opcfoundation.org/webservices/XMLDA/1.0/";

    /// <summary>SOAP 1.1 envelope namespace.</summary>
    public const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

    /// <summary>SOAP 1.1 encoding namespace.</summary>
    public const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";

    /// <summary>XSI namespace for xsi:type and xsi:nil attributes.</summary>
    public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>XSD namespace for primitive type references.</summary>
    public const string XsdNamespace = "http://www.w3.org/2001/XMLSchema";

    // ---- SOAPAction values (one per XML-DA operation) ----

    /// <summary>SOAPAction for the <c>GetStatus</c> operation.</summary>
    public const string SoapActionGetStatus = XmlDaNamespace + "GetStatus";

    /// <summary>SOAPAction for the <c>Read</c> operation.</summary>
    public const string SoapActionRead = XmlDaNamespace + "Read";

    /// <summary>SOAPAction for the <c>Write</c> operation.</summary>
    public const string SoapActionWrite = XmlDaNamespace + "Write";

    /// <summary>SOAPAction for the <c>Subscribe</c> operation.</summary>
    public const string SoapActionSubscribe = XmlDaNamespace + "Subscribe";

    /// <summary>SOAPAction for the <c>SubscriptionPolledRefresh</c> operation.</summary>
    public const string SoapActionSubscriptionPolledRefresh = XmlDaNamespace + "SubscriptionPolledRefresh";

    /// <summary>SOAPAction for the <c>SubscriptionCancel</c> operation.</summary>
    public const string SoapActionSubscriptionCancel = XmlDaNamespace + "SubscriptionCancel";

    /// <summary>SOAPAction for the <c>Browse</c> operation.</summary>
    public const string SoapActionBrowse = XmlDaNamespace + "Browse";

    /// <summary>SOAPAction for the <c>GetProperties</c> operation.</summary>
    public const string SoapActionGetProperties = XmlDaNamespace + "GetProperties";
}
