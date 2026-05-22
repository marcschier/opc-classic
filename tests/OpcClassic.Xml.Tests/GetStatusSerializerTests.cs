//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.IO;
using System.Linq;
using System.Text;
using OpcClassic.Xml;
using OpcClassic.Xml.Serialization;
using TUnit.Core;

namespace OpcClassic.Xml.Tests;

public sealed class GetStatusSerializerTests
{
    private static string SerializeRequest(XmlDaRequestHeader header)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            GetStatusSerializer.WriteRequest(w, header);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaServerStatus Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return GetStatusSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsLocaleIdAttribute()
    {
        var xml = SerializeRequest(new XmlDaRequestHeader("en-US", null));
        await Assert.That(xml).Contains("LocaleID=\"en-US\"");
    }

    [Test]
    public async Task Request_EmitsClientRequestHandle()
    {
        var xml = SerializeRequest(new XmlDaRequestHeader(null, "req-42"));
        await Assert.That(xml).Contains("ClientRequestHandle=\"req-42\"");
    }

    [Test]
    public async Task Request_OmitsAttributes_WhenHeaderFieldsEmpty()
    {
        var xml = SerializeRequest(new XmlDaRequestHeader(null, null));
        await Assert.That(xml).DoesNotContain("LocaleID");
        await Assert.That(xml).DoesNotContain("ClientRequestHandle");
    }

    [Test]
    public async Task Response_DecodesServerState_Running()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="running" />
                  <Status StartTime="2026-05-22T03:00:00+00:00" ProductVersion="1.0.0" VendorInfo="Acme" />
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var status = Deserialize(xml);
        await Assert.That(status.ServerState).IsEqualTo(XmlDaServerState.Running);
    }

    [Test]
    public async Task Response_DecodesAllServerStates()
    {
        XmlDaServerState ParseOne(string state)
        {
            string xml = $"""
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                      <GetStatusResult ServerState="{state}" />
                      <Status />
                    </GetStatusResponse>
                  </soap:Body>
                </soap:Envelope>
                """;
            return Deserialize(xml).ServerState;
        }
        await Assert.That(ParseOne("running")).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(ParseOne("failed")).IsEqualTo(XmlDaServerState.Failed);
        await Assert.That(ParseOne("noConfig")).IsEqualTo(XmlDaServerState.NoConfig);
        await Assert.That(ParseOne("suspended")).IsEqualTo(XmlDaServerState.Suspended);
        await Assert.That(ParseOne("test")).IsEqualTo(XmlDaServerState.Test);
        await Assert.That(ParseOne("commFault")).IsEqualTo(XmlDaServerState.CommFault);
    }

    [Test]
    public async Task Response_RejectsUnknownServerState()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="bogus" />
                  <Status />
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        bool threw = false;
        try { Deserialize(xml); }
        catch (InvalidDataException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Response_DecodesStartTime_ProductVersion_VendorInfo()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="running" />
                  <Status StartTime="2026-05-22T10:30:00+02:00"
                          ProductVersion="2.5.1"
                          VendorInfo="Acme Industrial" />
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var status = Deserialize(xml);
        await Assert.That(status.ProductVersion).IsEqualTo("2.5.1");
        await Assert.That(status.VendorInfo).IsEqualTo("Acme Industrial");
        await Assert.That(status.StartTime.Year).IsEqualTo(2026);
        await Assert.That(status.StartTime.Hour).IsEqualTo(10);
        await Assert.That(status.StartTime.Offset.TotalHours).IsEqualTo(2);
    }

    [Test]
    public async Task Response_DecodesSupportedLocaleIds_MultipleEntries()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="running" />
                  <Status StartTime="2026-05-22T00:00:00Z" ProductVersion="1.0" VendorInfo="">
                    <SupportedLocaleIDs>en-US</SupportedLocaleIDs>
                    <SupportedLocaleIDs>de-DE</SupportedLocaleIDs>
                    <SupportedLocaleIDs>fr-FR</SupportedLocaleIDs>
                  </Status>
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var status = Deserialize(xml);
        await Assert.That(status.SupportedLocaleIds.Count).IsEqualTo(3);
        await Assert.That(status.SupportedLocaleIds.SequenceEqual(new[] { "en-US", "de-DE", "fr-FR" })).IsTrue();
    }

    [Test]
    public async Task Response_DecodesSupportedInterfaceVersions()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="running" />
                  <Status StartTime="2026-05-22T00:00:00Z" ProductVersion="1.0" VendorInfo="">
                    <SupportedInterfaceVersions>XML_DA_Version_1_0</SupportedInterfaceVersions>
                  </Status>
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var status = Deserialize(xml);
        await Assert.That(status.SupportedInterfaceVersions.Count).IsEqualTo(1);
        await Assert.That(status.SupportedInterfaceVersions[0]).IsEqualTo("XML_DA_Version_1_0");
    }

    [Test]
    public async Task Response_DecodesStatusInfo()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="running" />
                  <Status StartTime="2026-05-22T00:00:00Z" ProductVersion="1.0" VendorInfo="">
                    <StatusInfo>Acme OPC Server v2.5 - 17 active subscriptions</StatusInfo>
                  </Status>
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var status = Deserialize(xml);
        await Assert.That(status.StatusInfo).IsEqualTo("Acme OPC Server v2.5 - 17 active subscriptions");
    }

    [Test]
    public async Task Response_HandlesEmptyStatusInfo()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="running" />
                  <Status StartTime="2026-05-22T00:00:00Z" ProductVersion="1.0" VendorInfo="" />
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var status = Deserialize(xml);
        await Assert.That(status.StatusInfo).IsNull();
    }

    [Test]
    public async Task Response_RejectsWrongOperation()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/" />
              </soap:Body>
            </soap:Envelope>
            """;
        bool threw = false;
        try { Deserialize(xml); }
        catch (InvalidDataException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Request_Then_Response_RoundTrips()
    {
        // Produce a request, build a synthetic response, then deserialize.
        // Demonstrates the writer + reader work as a connected pipeline.
        var reqXml = SerializeRequest(new XmlDaRequestHeader("en-US", "client-42"));
        await Assert.That(reqXml).Contains("<GetStatus");
        await Assert.That(reqXml).Contains("LocaleID=\"en-US\"");

        const string respXml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetStatusResult ServerState="running" />
                  <Status StartTime="2026-05-22T03:00:00Z" ProductVersion="1.0" VendorInfo="Acme">
                    <SupportedLocaleIDs>en-US</SupportedLocaleIDs>
                    <SupportedInterfaceVersions>XML_DA_Version_1_0</SupportedInterfaceVersions>
                  </Status>
                </GetStatusResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var status = Deserialize(respXml);
        await Assert.That(status.ServerState).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(status.SupportedLocaleIds[0]).IsEqualTo("en-US");
        await Assert.That(status.SupportedInterfaceVersions[0]).IsEqualTo("XML_DA_Version_1_0");
    }
}
