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

public sealed class BrowseSerializerTests
{
    private static string SerializeRequest(XmlDaBrowseRequest req)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            BrowseSerializer.WriteRequest(w, req);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaBrowseResponse Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return BrowseSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsBrowseElement_WithItemNameAndFilter()
    {
        var xml = SerializeRequest(new XmlDaBrowseRequest(
            new XmlDaRequestHeader(null, null),
            ItemName: "Channel1.Device1",
            BrowseFilter: XmlDaBrowseFilter.Item));

        await Assert.That(xml).Contains("<Browse");
        await Assert.That(xml).Contains("ItemName=\"Channel1.Device1\"");
        await Assert.That(xml).Contains("BrowseFilter=\"item\"");
    }

    [Test]
    public async Task Request_EmitsAllFilter_AsDefault()
    {
        var xml = SerializeRequest(new XmlDaBrowseRequest(
            new XmlDaRequestHeader(null, null)));
        await Assert.That(xml).Contains("BrowseFilter=\"all\"");
    }

    [Test]
    public async Task Request_EmitsBranchFilter()
    {
        var xml = SerializeRequest(new XmlDaBrowseRequest(
            new XmlDaRequestHeader(null, null),
            BrowseFilter: XmlDaBrowseFilter.Branch));
        await Assert.That(xml).Contains("BrowseFilter=\"branch\"");
    }

    [Test]
    public async Task Request_EmitsMaxElementsReturned_WhenNonZero()
    {
        var xml = SerializeRequest(new XmlDaBrowseRequest(
            new XmlDaRequestHeader(null, null),
            MaxElementsReturned: 250));
        await Assert.That(xml).Contains("MaxElementsReturned=\"250\"");
    }

    [Test]
    public async Task Request_EmitsContinuationPoint_ForPaging()
    {
        var xml = SerializeRequest(new XmlDaBrowseRequest(
            new XmlDaRequestHeader(null, null),
            ContinuationPoint: "opaque-token-abc"));
        await Assert.That(xml).Contains("ContinuationPoint=\"opaque-token-abc\"");
    }

    [Test]
    public async Task Response_DecodesElements_WithFlags()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <BrowseResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <BrowseResult ServerState="running" />
                  <Elements Name="Channel1" ItemName="Channel1" IsItem="false" HasChildren="true" />
                  <Elements Name="Device1" ItemName="Channel1.Device1" IsItem="false" HasChildren="true" />
                  <Elements Name="Tag1" ItemName="Channel1.Device1.Tag1" IsItem="true" HasChildren="false" />
                  <ContinuationPoint></ContinuationPoint>
                  <MoreElements>false</MoreElements>
                </BrowseResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ServerState).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(resp.Elements.Count).IsEqualTo(3);
        await Assert.That(resp.Elements[0].Name).IsEqualTo("Channel1");
        await Assert.That(resp.Elements[0].IsItem).IsFalse();
        await Assert.That(resp.Elements[0].HasChildren).IsTrue();
        await Assert.That(resp.Elements[2].Name).IsEqualTo("Tag1");
        await Assert.That(resp.Elements[2].IsItem).IsTrue();
        await Assert.That(resp.Elements[2].HasChildren).IsFalse();
        await Assert.That(resp.MoreElements).IsFalse();
        await Assert.That(resp.ContinuationPoint).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Response_DecodesContinuationPoint_AndMoreElements()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <BrowseResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <BrowseResult ServerState="running" />
                  <Elements Name="Tag1" ItemName="X.Tag1" IsItem="true" HasChildren="false" />
                  <ContinuationPoint>page-cookie-7</ContinuationPoint>
                  <MoreElements>true</MoreElements>
                </BrowseResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ContinuationPoint).IsEqualTo("page-cookie-7");
        await Assert.That(resp.MoreElements).IsTrue();
    }

    [Test]
    public async Task Response_HandlesEmptyElements()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <BrowseResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <BrowseResult ServerState="running" />
                  <MoreElements>false</MoreElements>
                </BrowseResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.Elements.Count).IsEqualTo(0);
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
