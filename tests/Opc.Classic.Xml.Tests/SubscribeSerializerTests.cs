//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using System.Text;
using Opc.Classic;
using Opc.Classic.Xml;
using Opc.Classic.Xml.Serialization;
using TUnit.Core;

namespace Opc.Classic.Xml.Tests;

public sealed class SubscribeSerializerTests
{
    private static string SerializeRequest(XmlDaSubscribeRequest req)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            SubscribeSerializer.WriteRequest(w, req);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaSubscribeResponse Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return SubscribeSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsItemsWithIndividualRates()
    {
        var xml = SerializeRequest(new XmlDaSubscribeRequest(
            new XmlDaRequestHeader(null, null),
            new[]
            {
                new XmlDaSubscribeItem("Fast", "h1", RequestedSamplingRate: 50),
                new XmlDaSubscribeItem("Slow", "h2", RequestedSamplingRate: 5000),
                new XmlDaSubscribeItem("DeadbandTag", "h3", Deadband: 1.5f),
            },
            RequestedSamplingRate: 1000));

        await Assert.That(xml).Contains("<Subscribe");
        await Assert.That(xml).Contains("ItemName=\"Fast\"");
        await Assert.That(xml).Contains("RequestedSamplingRate=\"50\"");
        await Assert.That(xml).Contains("RequestedSamplingRate=\"5000\"");
        await Assert.That(xml).Contains("Deadband=\"1.5\"");
    }

    [Test]
    public async Task Request_EmitsTopLevelRequestedSamplingRate()
    {
        var xml = SerializeRequest(new XmlDaSubscribeRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaSubscribeItem("Tag1", null) },
            RequestedSamplingRate: 500));
        await Assert.That(xml).Contains("RequestedSamplingRate=\"500\"");
    }

    [Test]
    public async Task Request_EmitsReturnValuesOnReply_AndPingRate()
    {
        var xml = SerializeRequest(new XmlDaSubscribeRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaSubscribeItem("Tag1", null) },
            SubscriptionPingRate: 30000,
            ReturnValuesOnReply: true));
        await Assert.That(xml).Contains("ReturnValuesOnReply=\"true\"");
        await Assert.That(xml).Contains("SubscriptionPingRate=\"30000\"");
    }

    [Test]
    public async Task Request_EmitsEnableBuffering()
    {
        var xml = SerializeRequest(new XmlDaSubscribeRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaSubscribeItem("Tag1", null) },
            EnableBuffering: true));
        await Assert.That(xml).Contains("EnableBuffering=\"true\"");
    }

    [Test]
    public async Task Response_DecodesServerSubHandle_AndRevisedRate()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <SubscribeResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                                    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                    xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <SubscribeResult ServerState="running" ServerSubHandle="sub-abc-99" RevisedSamplingRate="750" />
                </SubscribeResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ServerState).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(resp.ServerSubHandle).IsEqualTo("sub-abc-99");
        await Assert.That(resp.RevisedSamplingRate).IsEqualTo(750);
    }

    [Test]
    public async Task Response_DecodesInitialItemValues_WhenPresent()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <SubscribeResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                                    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                    xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <SubscribeResult ServerState="running" ServerSubHandle="sub-x" RevisedSamplingRate="1000" />
                  <RItemList>
                    <Items ItemName="Tag1" ClientItemHandle="h1">
                      <Value xsi:type="xsd:double">42.5</Value>
                      <Quality QualityField="good" />
                    </Items>
                    <Items ItemName="Tag2" ClientItemHandle="h2">
                      <Value xsi:type="xsd:int">99</Value>
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </SubscribeResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.Items.Count).IsEqualTo(2);
        await Assert.That(resp.Items[0].Value!.AsDouble()).IsEqualTo(42.5);
        await Assert.That(resp.Items[1].Value!.AsInt32()).IsEqualTo(99);
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
}
