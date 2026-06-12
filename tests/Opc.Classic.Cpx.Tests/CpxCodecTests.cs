//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Cpx.Tests;

public sealed class OpcBinaryCodecTests
{
    private const string FunctionBlockDictionary = """
        <?xml version="1.0" encoding="utf-8" ?>
        <TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/"
                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        DefaultBigEndian="true">
          <TypeDescription TypeID="FunctionBlockHeader">
            <CharString Name="Block Tag" xsi:type="Ascii" Length="8" />
            <Integer Name="Execution Time" xsi:type="Int32" />
            <Integer Name="Execution Frequency" xsi:type="Int32" />
            <Integer Name="Number of Parameters" xsi:type="Int16" />
          </TypeDescription>
        </TypeDictionary>
        """;

    [Test]
    public async Task OpcBinaryDictionaryParser_ParsesTypeDictionary()
    {
        var dictionary = OpcBinaryDictionaryParser.Parse(FunctionBlockDictionary);
        var type = dictionary.TryGetByTypeId("FunctionBlockHeader");

        await Assert.That(dictionary.DefaultBigEndian).IsTrue();
        await Assert.That(type).IsNotNull();
        await Assert.That(type!.Fields.Count).IsEqualTo(4);
        await Assert.That(type.Fields[0].Name).IsEqualTo("Block Tag");
        await Assert.That(type.Fields[0].Kind).IsEqualTo(TypeKind.String);
        await Assert.That(type.Fields[0].Length).IsEqualTo(8);
        await Assert.That(type.Fields[0].StringEncoding).IsEqualTo("ASCII");
        await Assert.That(type.Fields[1].Kind).IsEqualTo(TypeKind.Int32);
    }

    [Test]
    public async Task OpcBinaryEncoderDecoder_RoundTripsFunctionBlockHeader()
    {
        var dictionary = OpcBinaryDictionaryParser.Parse(FunctionBlockDictionary);
        var type = dictionary.TryGetByTypeId("FunctionBlockHeader")!;
        var value = CreateValue(type, new Dictionary<string, object?>
        {
            ["Block Tag"] = "FB-100",
            ["Execution Time"] = 1_000,
            ["Execution Frequency"] = 50,
            ["Number of Parameters"] = (short)3,
        });

        var encoded = OpcBinaryEncoder.Encode(value, type, dictionary);
        var decoded = OpcBinaryDecoder.Decode(encoded, type, dictionary);

        await Assert.That(encoded.Length).IsEqualTo(18);
        await Assert.That(encoded[8]).IsEqualTo((byte)0);
        await Assert.That(encoded[11]).IsEqualTo((byte)0xE8);
        await Assert.That(decoded.TryGet<string>("Block Tag", out var tag)).IsTrue();
        await Assert.That(tag).IsEqualTo("FB-100");
        await Assert.That(decoded.TryGet<int>("Execution Time", out var executionTime)).IsTrue();
        await Assert.That(executionTime).IsEqualTo(1_000);
        await Assert.That(decoded.TryGet<int>("Execution Frequency", out var frequency)).IsTrue();
        await Assert.That(frequency).IsEqualTo(50);
        await Assert.That(decoded.TryGet<short>("Number of Parameters", out var parameters)).IsTrue();
        await Assert.That(parameters).IsEqualTo((short)3);
    }

    private static ComplexValue CreateValue(TypeDescription type, IReadOnlyDictionary<string, object?> fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields,
        };
}

public sealed class XmlSchemaCodecTests
{
    private const string ConnectionSchema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://opcfoundation.org/ComplexData/Sample1.xsd"
                   elementFormDefault="qualified">
          <xs:element name="Connection">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="DeviceName" type="xs:string" />
                <xs:element name="WaitTime" type="xs:unsignedInt" />
                <xs:element name="Status">
                  <xs:complexType>
                    <xs:sequence>
                      <xs:element name="ConnectState" type="xs:boolean" />
                      <xs:element name="LastConnectTime" type="xs:dateTime" />
                      <xs:element name="ConnectFailCount" type="xs:unsignedInt" />
                      <xs:element name="IsConnected" type="xs:boolean" />
                    </xs:sequence>
                  </xs:complexType>
                </xs:element>
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    [Test]
    public async Task XmlSchemaParser_ParsesNestedComplexTypes()
    {
        var dictionary = XmlSchemaParser.Parse(ConnectionSchema);
        var connection = dictionary.TryGetByTypeId("Connection");
        var status = dictionary.TryGetByTypeId("Connection/Status");

        await Assert.That(dictionary.Name).IsEqualTo("http://opcfoundation.org/ComplexData/Sample1.xsd");
        await Assert.That(connection).IsNotNull();
        await Assert.That(status).IsNotNull();
        await Assert.That(connection!.Fields.Count).IsEqualTo(3);
        await Assert.That(connection.Fields[2].Kind).IsEqualTo(TypeKind.StructReference);
        await Assert.That(connection.Fields[2].TypeId).IsEqualTo("Connection/Status");
        await Assert.That(status!.Fields[2].Kind).IsEqualTo(TypeKind.UInt32);
    }

    [Test]
    public async Task XmlComplexValueSerializer_RoundTripsNestedValue()
    {
        var dictionary = XmlSchemaParser.Parse(ConnectionSchema);
        var connectionType = dictionary.TryGetByTypeId("Connection")!;
        var statusType = dictionary.TryGetByTypeId("Connection/Status")!;
        var lastConnectTime = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var status = CreateValue(statusType, new Dictionary<string, object?>
        {
            ["ConnectState"] = true,
            ["LastConnectTime"] = lastConnectTime,
            ["ConnectFailCount"] = 2u,
            ["IsConnected"] = true,
        });
        var connection = CreateValue(connectionType, new Dictionary<string, object?>
        {
            ["DeviceName"] = "Device00",
            ["WaitTime"] = 250u,
            ["Status"] = status,
        });

        var xml = XmlComplexValueSerializer.Serialize(connection, connectionType, dictionary);
        var roundTrip = XmlComplexValueSerializer.Deserialize(xml, connectionType, dictionary);
        var roundTripStatus = (ComplexValue)roundTrip["Status"]!;

        await Assert.That(roundTrip.TryGet<string>("DeviceName", out var deviceName)).IsTrue();
        await Assert.That(deviceName).IsEqualTo("Device00");
        await Assert.That(roundTrip.TryGet<uint>("WaitTime", out var waitTime)).IsTrue();
        await Assert.That(waitTime).IsEqualTo(250u);
        await Assert.That(roundTripStatus.TryGet<bool>("ConnectState", out var connectState)).IsTrue();
        await Assert.That(connectState).IsTrue();
        await Assert.That(roundTripStatus.TryGet<DateTime>("LastConnectTime", out var parsedTime)).IsTrue();
        await Assert.That(parsedTime).IsEqualTo(lastConnectTime);
        await Assert.That(roundTripStatus.TryGet<uint>("ConnectFailCount", out var failCount)).IsTrue();
        await Assert.That(failCount).IsEqualTo(2u);
    }

    private static ComplexValue CreateValue(TypeDescription type, IReadOnlyDictionary<string, object?> fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields,
        };
}

public sealed class CpxConstantsAndNamespaceTests
{
    [Test]
    public async Task OpcComplexDataConstants_MatchSpecAndOpcErrorHeader()
    {
        var typeSystemIds = new Dictionary<string, string>
        {
            [nameof(TypeDictionary.XmlSchemaTypeSystemId)] = TypeDictionary.XmlSchemaTypeSystemId,
            [nameof(TypeDictionary.OpcBinaryTypeSystemId)] = TypeDictionary.OpcBinaryTypeSystemId,
        };
        var propertyIds = new Dictionary<string, int>
        {
            [nameof(OpcComplexDataProperty.TypeSystemId)] = OpcComplexDataProperty.TypeSystemId,
            [nameof(OpcComplexDataProperty.DataFilterValue)] = OpcComplexDataProperty.DataFilterValue,
        };
        var resultCodes = new Dictionary<string, int>
        {
            [nameof(OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED)] = OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED,
            [nameof(OpcComplexDataResult.OPCCPX_E_FILTER_DUPLICATE)] = OpcComplexDataResult.OPCCPX_E_FILTER_DUPLICATE,
            [nameof(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID)] = OpcComplexDataResult.OPCCPX_E_FILTER_INVALID,
            [nameof(OpcComplexDataResult.OPCCPX_E_FILTER_ERROR)] = OpcComplexDataResult.OPCCPX_E_FILTER_ERROR,
            [nameof(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA)] = OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA,
        };

        await Assert.That(typeSystemIds[nameof(TypeDictionary.XmlSchemaTypeSystemId)]).IsEqualTo("XMLSchema");
        await Assert.That(typeSystemIds[nameof(TypeDictionary.OpcBinaryTypeSystemId)]).IsEqualTo("OPCBinary");
        await Assert.That(propertyIds[nameof(OpcComplexDataProperty.TypeSystemId)]).IsEqualTo(600);
        await Assert.That(propertyIds[nameof(OpcComplexDataProperty.DataFilterValue)]).IsEqualTo(609);
        await Assert.That(resultCodes[nameof(OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED)]).IsEqualTo(unchecked((int)0xC0040407));
        await Assert.That(resultCodes[nameof(OpcComplexDataResult.OPCCPX_E_FILTER_DUPLICATE)]).IsEqualTo(unchecked((int)0xC0040408));
        await Assert.That(resultCodes[nameof(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID)]).IsEqualTo(unchecked((int)0xC0040409));
        await Assert.That(resultCodes[nameof(OpcComplexDataResult.OPCCPX_E_FILTER_ERROR)]).IsEqualTo(unchecked((int)0xC004040A));
        await Assert.That(resultCodes[nameof(OpcComplexDataResult.OPCCPX_S_FILTER_NO_DATA)]).IsEqualTo(0x0004040B);
    }

    [Test]
    public async Task CpxNamespaceBuilder_BuildsDiscoveryConversionAndFilterPaths()
    {
        await Assert.That(CpxNamespaceBuilder.BuildTypePath("XMLSchema", "Sample1", "Connection/Status"))
            .IsEqualTo("/CPX/XMLSchema/Sample1/Connection/Status");
        await Assert.That(CpxNamespaceBuilder.GetDictionarySegment("http://opcfoundation.org/ComplexData/Sample1.xsd"))
            .IsEqualTo("Sample1.xsd");
        await Assert.That(CpxNamespaceBuilder.BuildConversionPath("/Sample/Connections/Device00", "XML"))
            .IsEqualTo("/Sample/Connections/Device00/CPX/XML");
        await Assert.That(CpxNamespaceBuilder.BuildDataFilterPath("/Sample/Connections/Device00", "XML", "Filter01"))
            .IsEqualTo("/Sample/Connections/Device00/CPX/XML/DataFilters/Filter01");
    }
}
