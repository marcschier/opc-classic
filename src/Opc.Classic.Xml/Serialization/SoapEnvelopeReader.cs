//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// AOT-safe SOAP 1.1 envelope reader. Symmetric to SoapEnvelopeWriter.
//

using System;
using System.IO;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

/// <summary>
/// SOAP 1.1 envelope reader for OPC XML-DA response payloads.
/// </summary>
public sealed class SoapEnvelopeReader : IDisposable
{
    private readonly XmlReader _reader;
    private readonly bool _ownsReader;

    /// <summary>The underlying XML reader (caller may consume operation-specific elements through it).</summary>
    public XmlReader Reader => _reader;

    /// <summary>Wraps an existing XmlReader (ownership stays with caller).</summary>
    public SoapEnvelopeReader(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
        _ownsReader = false;
    }

    /// <summary>Creates a reader over the given stream.</summary>
    public SoapEnvelopeReader(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var settings = new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Document,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            CloseInput = false,
            DtdProcessing = DtdProcessing.Prohibit,        // XXE defence
            XmlResolver = null,                              // XXE defence
        };
        _reader = XmlReader.Create(stream, settings);
        _ownsReader = true;
    }

    /// <summary>
    /// Reads through the SOAP envelope and body wrappers and positions the
    /// underlying <see cref="XmlReader"/> on the first operation-response
    /// element (a child of soap:Body). Throws if the envelope is malformed
    /// or if the response is a SOAP Fault.
    /// </summary>
    /// <returns>The local name of the operation response element.</returns>
    public string AdvanceToOperationResponse()
    {
        // Advance to soap:Envelope
        if (!_reader.ReadToFollowing("Envelope", XmlDaConstants.SoapEnvelopeNamespace))
        {
            throw new InvalidDataException("SOAP envelope not found.");
        }

        if (!_reader.ReadToDescendant("Body", XmlDaConstants.SoapEnvelopeNamespace))
        {
            throw new InvalidDataException("SOAP Body not found inside Envelope.");
        }

        // Now positioned on soap:Body — advance to its first child element.
        if (!_reader.Read())
        {
            throw new InvalidDataException("SOAP Body is empty.");
        }

        // Skip whitespace / non-element nodes.
        while (_reader.NodeType != XmlNodeType.Element)
        {
            if (_reader.NodeType == XmlNodeType.EndElement)
            {
                throw new InvalidDataException("SOAP Body contains no operation element.");
            }

            if (!_reader.Read())
            {
                throw new InvalidDataException("Unexpected end of XML inside SOAP Body.");
            }
        }

        // SOAP Faults surface as <soap:Fault> children of <soap:Body>.
        if (string.Equals(_reader.NamespaceURI, XmlDaConstants.SoapEnvelopeNamespace, StringComparison.Ordinal)
            && string.Equals(_reader.LocalName, "Fault", StringComparison.Ordinal))
        {
            throw ReadFault();
        }

        return _reader.LocalName;
    }

    private XmlDaSoapFaultException ReadFault()
    {
        string faultCode = string.Empty;
        string faultString = string.Empty;

        if (!_reader.IsEmptyElement)
        {
            int faultDepth = _reader.Depth;
            bool alreadyAdvanced = false;
            while (true)
            {
                if (!alreadyAdvanced && !_reader.Read())
                {
                    break;
                }
                alreadyAdvanced = false;
                if (_reader.Depth <= faultDepth)
                {
                    break;
                }
                if (_reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (string.Equals(_reader.LocalName, "faultcode", StringComparison.Ordinal))
                {
                    faultCode = _reader.ReadElementContentAsString();
                    alreadyAdvanced = true;
                }
                else if (string.Equals(_reader.LocalName, "faultstring", StringComparison.Ordinal))
                {
                    faultString = _reader.ReadElementContentAsString();
                    alreadyAdvanced = true;
                }
                else
                {
                    _reader.Skip();
                    alreadyAdvanced = true;
                }
            }
        }

        return new XmlDaSoapFaultException(faultCode, faultString, XmlDaErrorCodes.Parse(faultCode));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsReader)
        {
            _reader.Dispose();
        }
    }
}
