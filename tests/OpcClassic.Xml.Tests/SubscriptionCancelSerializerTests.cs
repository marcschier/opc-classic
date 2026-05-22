//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.IO;
using System.Text;
using OpcClassic.Xml;
using OpcClassic.Xml.Serialization;
using TUnit.Core;

namespace OpcClassic.Xml.Tests;

public sealed class SubscriptionCancelSerializerTests
{
    private static string SerializeRequest(XmlDaSubscriptionCancelRequest req)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            SubscriptionCancelSerializer.WriteRequest(w, req);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaSubscriptionCancelResponse Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return SubscriptionCancelSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsServerSubHandle()
    {
        var xml = SerializeRequest(new XmlDaSubscriptionCancelRequest("sub-handle-7"));
        await Assert.That(xml).Contains("<SubscriptionCancel");
        await Assert.That(xml).Contains("ServerSubHandle=\"sub-handle-7\"");
    }

    [Test]
    public async Task Request_EmitsClientRequestHandle_WhenPresent()
    {
        var xml = SerializeRequest(new XmlDaSubscriptionCancelRequest("sub-x", "req-42"));
        await Assert.That(xml).Contains("ClientRequestHandle=\"req-42\"");
    }

    [Test]
    public async Task Request_OmitsClientRequestHandle_WhenAbsent()
    {
        var xml = SerializeRequest(new XmlDaSubscriptionCancelRequest("sub-x"));
        await Assert.That(xml).DoesNotContain("ClientRequestHandle");
    }

    [Test]
    public async Task Request_ThrowsOnEmptyServerSubHandle()
    {
        bool threw = false;
        try
        {
            SerializeRequest(new XmlDaSubscriptionCancelRequest(string.Empty));
        }
        catch (ArgumentException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Response_DecodesEchoedHandle()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <SubscriptionCancelResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                                             ClientRequestHandle="req-99" />
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ClientRequestHandle).IsEqualTo("req-99");
    }

    [Test]
    public async Task Response_HandlesMissingClientHandle_AsNull()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <SubscriptionCancelResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/" />
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ClientRequestHandle).IsNull();
    }

    [Test]
    public async Task Response_RejectsWrongOperation()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <BrowseResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/" />
              </soap:Body>
            </soap:Envelope>
            """;
        bool threw = false;
        try { Deserialize(xml); }
        catch (InvalidDataException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }
}
