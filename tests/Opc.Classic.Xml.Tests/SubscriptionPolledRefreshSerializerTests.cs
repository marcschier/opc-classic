// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Text;
using Opc.Classic.Xml.Serialization;

namespace Opc.Classic.Xml.Tests;

public sealed class SubscriptionPolledRefreshSerializerTests
{
    private static string SerializeRequest(XmlDaSubscriptionPolledRefreshRequest req)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            SubscriptionPolledRefreshSerializer.WriteRequest(w, req);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaSubscriptionPolledRefreshResponse Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return SubscriptionPolledRefreshSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsMultipleServerSubHandles()
    {
        var xml = SerializeRequest(new XmlDaSubscriptionPolledRefreshRequest(
            new XmlDaRequestHeader(null, null),
            new[] { "sub-1", "sub-2", "sub-3" }));
        await Assert.That(xml).Contains("<ServerSubHandles>sub-1</ServerSubHandles>");
        await Assert.That(xml).Contains("<ServerSubHandles>sub-2</ServerSubHandles>");
        await Assert.That(xml).Contains("<ServerSubHandles>sub-3</ServerSubHandles>");
    }

    [Test]
    public async Task Request_EmitsWaitTime_AndReturnAllItems()
    {
        var xml = SerializeRequest(new XmlDaSubscriptionPolledRefreshRequest(
            new XmlDaRequestHeader(null, null),
            new[] { "sub-x" },
            WaitTime: 5000,
            ReturnAllItems: true));
        await Assert.That(xml).Contains("WaitTime=\"5000\"");
        await Assert.That(xml).Contains("ReturnAllItems=\"true\"");
    }

    [Test]
    public async Task Request_EmitsHoldTime_WhenSet()
    {
        var holdTime = new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero);
        var xml = SerializeRequest(new XmlDaSubscriptionPolledRefreshRequest(
            new XmlDaRequestHeader(null, null),
            new[] { "sub-x" },
            HoldTime: holdTime));
        await Assert.That(xml).Contains("HoldTime=\"2026-05-22T10:30:00");
    }

    [Test]
    public async Task Response_DecodesPerSubscriptionItemLists()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <SubscriptionPolledRefreshResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                                                    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                                    xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <SubscriptionPolledRefreshResult ServerState="running" />
                  <RItemList SubscriptionHandle="sub-x">
                    <Items ItemName="Tag1"><Value xsi:type="xsd:double">42.0</Value><Quality QualityField="good" /></Items>
                  </RItemList>
                  <RItemList SubscriptionHandle="sub-y">
                    <Items ItemName="Tag2"><Value xsi:type="xsd:int">7</Value><Quality QualityField="good" /></Items>
                  </RItemList>
                </SubscriptionPolledRefreshResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ItemLists.Count).IsEqualTo(2);
        await Assert.That(resp.ItemLists[0].SubscriptionHandle).IsEqualTo("sub-x");
        await Assert.That(resp.ItemLists[0].Items[0].Value!.AsDouble()).IsEqualTo(42.0);
        await Assert.That(resp.ItemLists[1].SubscriptionHandle).IsEqualTo("sub-y");
        await Assert.That(resp.ItemLists[1].Items[0].Value!.AsInt32()).IsEqualTo(7);
    }

    [Test]
    public async Task Response_DecodesInvalidServerSubHandles()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <SubscriptionPolledRefreshResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <SubscriptionPolledRefreshResult ServerState="running" />
                  <InvalidServerSubHandles>sub-bad-1</InvalidServerSubHandles>
                  <InvalidServerSubHandles>sub-bad-2</InvalidServerSubHandles>
                </SubscriptionPolledRefreshResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.InvalidServerSubHandles.Count).IsEqualTo(2);
        await Assert.That(resp.InvalidServerSubHandles[0]).IsEqualTo("sub-bad-1");
        await Assert.That(resp.InvalidServerSubHandles[1]).IsEqualTo("sub-bad-2");
    }

    [Test]
    public async Task Response_DecodesDataBufferOverflow_Flag()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <SubscriptionPolledRefreshResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <SubscriptionPolledRefreshResult ServerState="running" DataBufferOverflow="true" />
                </SubscriptionPolledRefreshResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.DataBufferOverflow).IsTrue();
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
