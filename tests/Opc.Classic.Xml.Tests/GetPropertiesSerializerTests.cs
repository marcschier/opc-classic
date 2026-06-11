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

public sealed class GetPropertiesSerializerTests
{
    private static string SerializeRequest(XmlDaGetPropertiesRequest req)
    {
        using var ms = new MemoryStream();
        using (var w = new SoapEnvelopeWriter(ms))
        {
            GetPropertiesSerializer.WriteRequest(w, req);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static XmlDaGetPropertiesResponse Deserialize(string xml)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var r = new SoapEnvelopeReader(ms);
        return GetPropertiesSerializer.ReadResponse(r);
    }

    [Test]
    public async Task Request_EmitsItemIDsAndPropertyNames()
    {
        var xml = SerializeRequest(new XmlDaGetPropertiesRequest(
            new XmlDaRequestHeader(null, null),
            ItemPath: "",
            ItemNames: new[] { "Tag1", "Tag2" },
            PropertyNames: new[] { "DataType", "AccessRights" }));
        await Assert.That(xml).Contains("<ItemIDs");
        await Assert.That(xml).Contains("ItemName=\"Tag1\"");
        await Assert.That(xml).Contains("ItemName=\"Tag2\"");
        await Assert.That(xml).Contains("<PropertyNames");
        await Assert.That(xml).Contains("Name=\"DataType\"");
        await Assert.That(xml).Contains("Name=\"AccessRights\"");
    }

    [Test]
    public async Task Request_EmitsRequestFlags()
    {
        var xml = SerializeRequest(new XmlDaGetPropertiesRequest(
            new XmlDaRequestHeader(null, null),
            ItemPath: "",
            ItemNames: System.Array.Empty<string>(),
            PropertyNames: System.Array.Empty<string>(),
            ReturnAllProperties: true,
            ReturnPropertyValues: true,
            ReturnErrorText: false));
        await Assert.That(xml).Contains("ReturnAllProperties=\"true\"");
        await Assert.That(xml).Contains("ReturnPropertyValues=\"true\"");
        await Assert.That(xml).Contains("ReturnErrorText=\"false\"");
    }

    [Test]
    public async Task Response_DecodesPropertiesWithValues()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetPropertiesResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                        xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <GetPropertiesResult ServerState="running" />
                  <PropertyLists ItemName="Tag1">
                    <Properties Name="DataType" Description="The data type">
                      <Value xsi:type="xsd:QName">xsd:double</Value>
                    </Properties>
                    <Properties Name="AccessRights" Description="Read/write rights">
                      <Value xsi:type="xsd:int">3</Value>
                    </Properties>
                  </PropertyLists>
                </GetPropertiesResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.ServerState).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(resp.PropertyLists.Count).IsEqualTo(1);
        var list = resp.PropertyLists[0];
        await Assert.That(list.ItemName).IsEqualTo("Tag1");
        await Assert.That(list.Properties.Count).IsEqualTo(2);
        await Assert.That(list.Properties[0].Name).IsEqualTo("DataType");
        await Assert.That(list.Properties[0].Value!.RawText).IsEqualTo("xsd:double");
        await Assert.That(list.Properties[0].Value!.AsQName()!.Name).IsEqualTo("double");
        await Assert.That(list.Properties[0].Value!.AsQName()!.Namespace).IsEqualTo(XmlDaConstants.XsdNamespace);
        await Assert.That(list.Properties[1].Name).IsEqualTo("AccessRights");
        await Assert.That(list.Properties[1].Value!.AsInt32()).IsEqualTo(3);
    }

    [Test]
    public async Task Response_DecodesMultiplePropertyLists()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetPropertiesResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                        xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <GetPropertiesResult ServerState="running" />
                  <PropertyLists ItemName="Tag1">
                    <Properties Name="DataType"><Value xsi:type="xsd:QName">xsd:double</Value></Properties>
                  </PropertyLists>
                  <PropertyLists ItemName="Tag2">
                    <Properties Name="DataType"><Value xsi:type="xsd:QName">xsd:int</Value></Properties>
                  </PropertyLists>
                </GetPropertiesResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.PropertyLists.Count).IsEqualTo(2);
        await Assert.That(resp.PropertyLists[0].ItemName).IsEqualTo("Tag1");
        await Assert.That(resp.PropertyLists[1].ItemName).IsEqualTo("Tag2");
    }

    [Test]
    public async Task Response_DecodesPerItemResultId()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetPropertiesResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetPropertiesResult ServerState="running" />
                  <PropertyLists ItemName="MissingTag" ResultID="E_UNKNOWNITEMID" />
                </GetPropertiesResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        await Assert.That(resp.PropertyLists[0].ResultId).IsEqualTo("E_UNKNOWNITEMID");
        await Assert.That(resp.PropertyLists[0].Properties.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Response_DecodesPropertyWithoutValue_WhenNotRequested()
    {
        const string xml = """
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <GetPropertiesResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <GetPropertiesResult ServerState="running" />
                  <PropertyLists ItemName="Tag1">
                    <Properties Name="DataType" Description="DataType" />
                  </PropertyLists>
                </GetPropertiesResponse>
              </soap:Body>
            </soap:Envelope>
            """;
        var resp = Deserialize(xml);
        var prop = resp.PropertyLists[0].Properties[0];
        await Assert.That(prop.Name).IsEqualTo("DataType");
        await Assert.That(prop.Description).IsEqualTo("DataType");
        await Assert.That(prop.Value).IsNull();
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
        catch (System.IO.InvalidDataException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }
}
