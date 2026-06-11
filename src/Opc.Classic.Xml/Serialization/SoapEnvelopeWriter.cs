//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// AOT-safe SOAP 1.1 envelope serialization for OPC XML-DA 1.0.
//
// Hand-rolled XmlWriter / XmlReader pipeline — System.Xml.Serialization
// uses reflection and emits IL2026 warnings under PublishAot. Every method
// here works with concrete types and AOT-clean APIs.
//

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// SOAP 1.1 envelope writer for OPC XML-DA request payloads.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// var ms = new MemoryStream();
/// var w = new SoapEnvelopeWriter(ms);
/// w.WriteEnvelopeStart();
/// w.WriteBodyStart();
/// // ... write per-operation request element ...
/// w.WriteBodyEnd();
/// w.WriteEnvelopeEnd();
/// w.Flush();
/// </code>
/// </remarks>
public sealed class SoapEnvelopeWriter : IDisposable
{
    private readonly XmlWriter _writer;
    private readonly bool _ownsWriter;

    /// <summary>The underlying XML writer (useful for emitting operation-specific elements).</summary>
    public XmlWriter Writer => _writer;

    /// <summary>Wraps an existing XmlWriter (ownership stays with caller).</summary>
    public SoapEnvelopeWriter(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        _writer = writer;
        _ownsWriter = false;
    }

    /// <summary>Creates a writer that emits to the given stream with UTF-8 encoding.</summary>
    public SoapEnvelopeWriter(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = false,
            OmitXmlDeclaration = false,
            CloseOutput = false,
            ConformanceLevel = ConformanceLevel.Document,
        };
        _writer = XmlWriter.Create(stream, settings);
        _ownsWriter = true;
    }

    /// <summary>Writes <c>&lt;soap:Envelope xmlns:soap="..."&gt;</c>.</summary>
    public void WriteEnvelopeStart()
    {
        _writer.WriteStartDocument();
        _writer.WriteStartElement("soap", "Envelope", XmlDaConstants.SoapEnvelopeNamespace);
        _writer.WriteAttributeString("xmlns", "xsi", null, XmlDaConstants.XsiNamespace);
        _writer.WriteAttributeString("xmlns", "xsd", null, XmlDaConstants.XsdNamespace);
    }

    /// <summary>Writes the closing <c>&lt;/soap:Envelope&gt;</c> and end-of-document.</summary>
    public void WriteEnvelopeEnd()
    {
        _writer.WriteEndElement();
        _writer.WriteEndDocument();
    }

    /// <summary>Writes <c>&lt;soap:Body&gt;</c>.</summary>
    public void WriteBodyStart()
    {
        _writer.WriteStartElement("soap", "Body", XmlDaConstants.SoapEnvelopeNamespace);
    }

    /// <summary>Writes <c>&lt;/soap:Body&gt;</c>.</summary>
    public void WriteBodyEnd()
    {
        _writer.WriteEndElement();
    }

    /// <summary>
    /// Writes a top-level operation element bound to the XML-DA namespace
    /// (e.g. <c>&lt;GetStatus xmlns="..."&gt;</c>). Caller writes the
    /// operation-specific attributes / children, then calls
    /// <see cref="WriteOperationEnd"/>.
    /// </summary>
    public void WriteOperationStart(string localName)
    {
        ArgumentException.ThrowIfNullOrEmpty(localName);
        _writer.WriteStartElement(localName, XmlDaConstants.XmlDaNamespace);
    }

    /// <summary>Closes the operation element.</summary>
    public void WriteOperationEnd()
    {
        _writer.WriteEndElement();
    }

    /// <summary>Flushes the underlying writer.</summary>
    public void Flush() => _writer.Flush();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsWriter)
        {
            _writer.Dispose();
        }
    }
}
