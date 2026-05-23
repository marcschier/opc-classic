//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Round-trip tests for the SOAP 1.1 envelope writer/reader pair.
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using Opc.Classic.Xml;
using Opc.Classic.Xml.Serialization;
using TUnit.Core;

namespace Opc.Classic.Xml.Tests;

public sealed class SoapEnvelopeTests
{
    private static string WriteEmptyGetStatusRequest()
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            w.WriteEnvelopeStart();
            w.WriteBodyStart();
            w.WriteOperationStart("GetStatus");
            w.Writer.WriteAttributeString("LocaleID", "en-US");
            w.WriteOperationEnd();
            w.WriteBodyEnd();
            w.WriteEnvelopeEnd();
            w.Flush();
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    [Test]
    public async Task EnvelopeStart_WritesSoapNamespaceDeclaration()
    {
        var xml = WriteEmptyGetStatusRequest();
        await Assert.That(xml).Contains("xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"");
    }

    [Test]
    public async Task EnvelopeStart_WritesXsiNamespaceDeclaration()
    {
        var xml = WriteEmptyGetStatusRequest();
        await Assert.That(xml).Contains("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
    }

    [Test]
    public async Task Body_IsBoundToSoapNamespace()
    {
        var xml = WriteEmptyGetStatusRequest();
        await Assert.That(xml).Contains("soap:Body");
    }

    [Test]
    public async Task OperationStart_EmitsXmlDaNamespace()
    {
        var xml = WriteEmptyGetStatusRequest();
        await Assert.That(xml).Contains("xmlns=\"http://opcfoundation.org/webservices/XMLDA/1.0/\"");
    }

    [Test]
    public async Task OperationAttribute_FlowsThrough()
    {
        var xml = WriteEmptyGetStatusRequest();
        await Assert.That(xml).Contains("LocaleID=\"en-US\"");
    }

    [Test]
    public async Task Reader_AdvancesToOperationResponse_Element()
    {
        var xml = WriteEmptyGetStatusRequest();
        string opName;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        using (var r = new SoapEnvelopeReader(ms))
        {
            opName = r.AdvanceToOperationResponse();
        }
        await Assert.That(opName).IsEqualTo("GetStatus");
    }

    [Test]
    public async Task Reader_RejectsMissingEnvelope()
    {
        const string bad = "<?xml version=\"1.0\"?><root />";
        bool threw = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(bad)))
        using (var r = new SoapEnvelopeReader(ms))
        {
            try
            {
                r.AdvanceToOperationResponse();
            }
            catch (InvalidDataException)
            {
                threw = true;
            }
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Reader_DetectsSoapFault()
    {
        const string fault = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <soap:Fault>
                  <faultcode>soap:Server</faultcode>
                  <faultstring>boom</faultstring>
                </soap:Fault>
              </soap:Body>
            </soap:Envelope>
            """;
        bool threw = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(fault)))
        using (var r = new SoapEnvelopeReader(ms))
        {
            try
            {
                r.AdvanceToOperationResponse();
            }
            catch (InvalidDataException)
            {
                threw = true;
            }
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Reader_RejectsDtd_XxeDefence()
    {
        // The reader is configured with DtdProcessing.Prohibit + XmlResolver=null
        // to defend against XML external-entity attacks.
        const string xxe = """
            <?xml version="1.0"?>
            <!DOCTYPE root [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body><GetStatus /></soap:Body>
            </soap:Envelope>
            """;
        bool threw = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xxe)))
        using (var r = new SoapEnvelopeReader(ms))
        {
            try
            {
                r.AdvanceToOperationResponse();
            }
            catch (XmlException)
            {
                threw = true;
            }
            catch (InvalidDataException)
            {
                threw = true;
            }
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task EnvelopeWriter_WrapsXmlWriter()
    {
        var sb = new StringBuilder();
        var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
        using var xw = XmlWriter.Create(sb, settings);
        using (var sw = new SoapEnvelopeWriter(xw))
        {
            sw.WriteEnvelopeStart();
            sw.WriteBodyStart();
            sw.WriteOperationStart("Read");
            sw.WriteOperationEnd();
            sw.WriteBodyEnd();
            sw.WriteEnvelopeEnd();
            sw.Flush();
        }
        var xml = sb.ToString();
        await Assert.That(xml).Contains("<soap:Envelope");
        await Assert.That(xml).Contains("<Read xmlns=\"http://opcfoundation.org/webservices/XMLDA/1.0/\" />");
    }
}
