//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.IO;
using System.Text;
using Opc.Classic.Xml;
using Opc.Classic.Xml.Serialization;
using TUnit.Core;

namespace Opc.Classic.Xml.Tests;

public sealed class WriteSerializerTests
{
    private static string SerializeRequest(XmlDaWriteRequest req)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            WriteSerializer.WriteRequest(w, req);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaWriteResponse Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return WriteSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsValueElementWithXsiType_Double()
    {
        var xml = SerializeRequest(new XmlDaWriteRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaWriteItem("Tag1", "h1", XmlDaValue.OfDouble(42.5)) }));

        await Assert.That(xml).Contains("xsi:type=\"xsd:double\"");
        await Assert.That(xml).Contains(">42.5</");
    }

    [Test]
    public async Task Request_EmitsValueElementWithXsiType_Boolean()
    {
        var xml = SerializeRequest(new XmlDaWriteRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaWriteItem("Switch", null, XmlDaValue.OfBoolean(true)) }));

        await Assert.That(xml).Contains("xsi:type=\"xsd:boolean\"");
        await Assert.That(xml).Contains(">true</");
    }

    [Test]
    public async Task Request_EmitsValueElementWithXsiType_Int()
    {
        var xml = SerializeRequest(new XmlDaWriteRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaWriteItem("Counter", null, XmlDaValue.OfInt32(99)) }));

        await Assert.That(xml).Contains("xsi:type=\"xsd:int\"");
        await Assert.That(xml).Contains(">99</");
    }

    [Test]
    public async Task Request_EmitsValueElementWithXsiType_String()
    {
        var xml = SerializeRequest(new XmlDaWriteRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaWriteItem("Label", null, XmlDaValue.OfString("hello")) }));

        await Assert.That(xml).Contains("xsi:type=\"xsd:string\"");
        await Assert.That(xml).Contains(">hello</");
    }

    [Test]
    public async Task Request_EmitsReturnValuesOnReply_True()
    {
        var xml = SerializeRequest(new XmlDaWriteRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaWriteItem("Tag1", null, XmlDaValue.OfInt32(1)) },
            ReturnValuesOnReply: true));

        await Assert.That(xml).Contains("ReturnValuesOnReply=\"true\"");
    }

    [Test]
    public async Task Request_EmitsClientHandle_AndOmitsWhenEmpty()
    {
        var withHandle = SerializeRequest(new XmlDaWriteRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaWriteItem("Tag1", "h-42", XmlDaValue.OfInt32(1)) }));
        await Assert.That(withHandle).Contains("ClientItemHandle=\"h-42\"");

        var withoutHandle = SerializeRequest(new XmlDaWriteRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaWriteItem("Tag1", null, XmlDaValue.OfInt32(1)) }));
        await Assert.That(withoutHandle).DoesNotContain("ClientItemHandle");
    }

    [Test]
    public async Task Response_DecodesPerItemResultId_AndServerState()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <WriteResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <WriteResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Tag1" ClientItemHandle="h1" ResultID="S_OK" />
                    <Items ItemName="Tag2" ClientItemHandle="h2" ResultID="E_BADRIGHTS">
                      <ErrorText>Item is read-only</ErrorText>
                    </Items>
                  </RItemList>
                </WriteResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ServerState).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(resp.Items.Count).IsEqualTo(2);
        await Assert.That(resp.Items[0].ItemName).IsEqualTo("Tag1");
        await Assert.That(resp.Items[0].ResultId).IsEqualTo("S_OK");
        await Assert.That(resp.Items[1].ResultId).IsEqualTo("E_BADRIGHTS");
        await Assert.That(resp.Items[1].ErrorText).IsEqualTo("Item is read-only");
    }

    [Test]
    public async Task Response_HandlesEmptyResponseList()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <WriteResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <WriteResult ServerState="running" />
                  <RItemList />
                </WriteResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.Items.Count).IsEqualTo(0);
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
