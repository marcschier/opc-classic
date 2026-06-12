//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Text;
using Opc.Classic.Xml.Serialization;

namespace Opc.Classic.Xml.Tests;

public sealed class ReadSerializerTests
{
    private static string SerializeRequest(XmlDaReadRequest req)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            ReadSerializer.WriteRequest(w, req);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaReadResponse Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return ReadSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsItemsElement_PerItem()
    {
        var xml = SerializeRequest(new XmlDaReadRequest(
            new XmlDaRequestHeader("en-US", "req-1"),
            new[]
            {
                new XmlDaReadItem("Tag1", "h1"),
                new XmlDaReadItem("Tag2", "h2", MaxAge: 100),
            }));
        await Assert.That(xml).Contains("ItemName=\"Tag1\"");
        await Assert.That(xml).Contains("ItemName=\"Tag2\"");
        await Assert.That(xml).Contains("ClientItemHandle=\"h1\"");
        await Assert.That(xml).Contains("MaxAge=\"100\"");
    }

    [Test]
    public async Task Request_EmitsOptionsReturnErrorText()
    {
        var xml = SerializeRequest(new XmlDaReadRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaReadItem("Tag1", null) },
            ReturnErrorText: true));
        await Assert.That(xml).Contains("ReturnErrorText=\"true\"");
    }

    [Test]
    public async Task Request_OmitsClientHandle_WhenEmpty()
    {
        var xml = SerializeRequest(new XmlDaReadRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaReadItem("Tag1", null) }));
        await Assert.That(xml).DoesNotContain("ClientItemHandle");
    }

    [Test]
    public async Task Response_Decodes_DoubleValue()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Tag1" ClientItemHandle="h1" Timestamp="2026-05-22T03:00:00Z">
                      <Value xsi:type="xsd:double">42.5</Value>
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ServerState).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(resp.Items.Count).IsEqualTo(1);
        var item = resp.Items[0];
        await Assert.That(item.ItemName).IsEqualTo("Tag1");
        await Assert.That(item.ClientItemHandle).IsEqualTo("h1");
        await Assert.That(item.Value!.Type).IsEqualTo(XmlDaValueType.Double);
        await Assert.That(item.Value!.AsDouble()).IsEqualTo(42.5);
    }

    [Test]
    public async Task Response_Decodes_IntValue()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Counter">
                      <Value xsi:type="xsd:int">12345</Value>
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.Items[0].Value!.AsInt32()).IsEqualTo(12345);
    }

    [Test]
    public async Task Response_Decodes_BooleanValue()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Switch">
                      <Value xsi:type="xsd:boolean">true</Value>
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.Items[0].Value!.AsBoolean()).IsEqualTo(true);
    }

    [Test]
    public async Task Response_Decodes_StringValue()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Label">
                      <Value xsi:type="xsd:string">Hello, OPC.</Value>
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.Items[0].Value!.AsString()).IsEqualTo("Hello, OPC.");
    }

    [Test]
    public async Task Response_Decodes_UnknownXsiType_AsRawText()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="WeirdType">
                      <Value xsi:type="xsd:anyURI">urn:unsupported</Value>
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        var v = resp.Items[0].Value!;
        await Assert.That(v.Type).IsEqualTo(XmlDaValueType.Unknown);
        await Assert.That(v.RawText).IsEqualTo("urn:unsupported");
    }

    [Test]
    public async Task Response_Decodes_BadQuality()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="BrokenTag">
                      <Quality QualityField="bad" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        var item = resp.Items[0];
        await Assert.That(item.Quality.Quality).IsEqualTo(OpcQualityKind.Bad);
        await Assert.That(item.Value).IsNull();
    }

    [Test]
    public async Task Response_DecodesMultipleItems()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Tag1"><Value xsi:type="xsd:int">1</Value><Quality QualityField="good" /></Items>
                    <Items ItemName="Tag2"><Value xsi:type="xsd:int">2</Value><Quality QualityField="good" /></Items>
                    <Items ItemName="Tag3"><Value xsi:type="xsd:int">3</Value><Quality QualityField="good" /></Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.Items.Count).IsEqualTo(3);
        await Assert.That(resp.Items.Select(i => i.Value!.AsInt32()).SequenceEqual(new int?[] { 1, 2, 3 })).IsTrue();
    }

    [Test]
    public async Task Response_DecodesTimestamp_AsUtc()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Tag1" Timestamp="2026-05-22T10:30:00+02:00">
                      <Value xsi:type="xsd:double">1.0</Value>
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        var item = resp.Items[0];
        await Assert.That(item.Timestamp).IsNotNull();
        await Assert.That(item.Timestamp!.Value.Offset.TotalHours).IsEqualTo(2);
    }

    [Test]
    public async Task Response_RejectsWrongOperation()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/" />
              </soap:Body>
            </soap:Envelope>
            """;
        bool threw = false;
        try { Deserialize(xml); }
        catch (InvalidDataException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }
}
