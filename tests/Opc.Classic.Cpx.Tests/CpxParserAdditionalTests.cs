//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Cpx.Tests;

public sealed class CpxParserAdditionalTests
{
    [Test]
    public async Task OpcBinaryDictionaryParser_ParsesSingleTypeDescriptionRootAndFieldOptions()
    {
        const string xml = """
            <TypeDescription xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                             TypeID="Packet"
                             DefaultBigEndian="false"
                             DefaultStringEncoding="UTF-8"
                             DefaultCharWidth="1">
              <Integer Name="UnsignedByte" Length="1" Signed="false" />
              <FloatingPoint Name="Temperature" Length="4" />
              <CharString Name="Label" Length="6" />
              <TypeReference Name="Nested" TypeID="NestedPacket" ElementCount="2" />
            </TypeDescription>
            """;

        TypeDictionary dictionary = OpcBinaryDictionaryParser.Parse(xml);
        TypeDescription packet = dictionary.TryGetByTypeId("Packet")!;

        await Assert.That(dictionary.Name).IsEqualTo(string.Empty);
        await Assert.That(dictionary.DefaultBigEndian).IsFalse();
        await Assert.That(dictionary.DefaultStringEncoding).IsEqualTo("UTF-8");
        await Assert.That(dictionary.DefaultCharWidth).IsEqualTo(1);
        await Assert.That(packet.Fields.Count).IsEqualTo(4);
        await Assert.That(packet.Fields[0].Kind).IsEqualTo(TypeKind.UInt8);
        await Assert.That(packet.Fields[1].Kind).IsEqualTo(TypeKind.Single);
        await Assert.That(packet.Fields[2].Kind).IsEqualTo(TypeKind.String);
        await Assert.That(packet.Fields[3].Kind).IsEqualTo(TypeKind.StructReference);
        await Assert.That(packet.Fields[3].TypeId).IsEqualTo("NestedPacket");
        await Assert.That(packet.Fields[3].ElementCount).IsEqualTo(2);
    }

    [Test]
    public async Task OpcBinaryDictionaryParser_ParsesNamespacedTypesAndLittleEndianFieldOverride()
    {
        const string xml = """
            <opc:TypeDictionary xmlns:opc="http://opcfoundation.org/OPCBinary/1.0/"
                                xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                Name="Plant"
                                DefaultBigEndian="true">
              <opc:TypeDescription TypeID="Header">
                <opc:Integer Name="LittleCounter" xsi:type="UInt16" DefaultBigEndian="false" />
              </opc:TypeDescription>
            </opc:TypeDictionary>
            """;

        TypeDictionary dictionary = OpcBinaryDictionaryParser.Parse(xml);
        TypeDescription header = dictionary.TryGet("Header")!;

        await Assert.That(dictionary.Name).IsEqualTo("Plant");
        await Assert.That(dictionary.DefaultBigEndian).IsTrue();
        await Assert.That(header.Fields[0].Kind).IsEqualTo(TypeKind.UInt16);
        await Assert.That(header.Fields[0].ByteOrder).IsEqualTo(ByteOrder.LittleEndian);
    }

    [Test]
    public async Task OpcBinaryDictionaryParser_InvalidDocuments_ThrowFormatException()
    {
        await Assert.That(() => OpcBinaryDictionaryParser.Parse("<Root />"))
            .Throws<FormatException>();
        await Assert.That(() => OpcBinaryDictionaryParser.Parse("<TypeDictionary Name=\"Empty\" />"))
            .Throws<FormatException>();
        await Assert.That(() => OpcBinaryDictionaryParser.Parse("<TypeDescription TypeID=\"Empty\" />"))
            .Throws<FormatException>();
        await Assert.That(() => OpcBinaryDictionaryParser.Parse("<TypeDescription><Int8 Name=\"MissingTypeId\" /></TypeDescription>"))
            .Throws<FormatException>();
        await Assert.That(() => OpcBinaryDictionaryParser.Parse("<TypeDescription TypeID=\"Bad\"><Unsupported Name=\"X\" /></TypeDescription>"))
            .Throws<FormatException>();
    }

    [Test]
    public async Task XmlSchemaParser_ParsesNamedComplexTypesChoiceAllAndNumericArrays()
    {
        const string schema = """
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       xmlns:tns="http://example.com/cpx"
                       targetNamespace="http://example.com/cpx">
              <xs:element name="Plant" type="tns:PlantType" />
              <xs:element name="LooseText" type="xs:token" />
              <xs:complexType name="PlantType">
                <xs:sequence>
                  <xs:element name="Motor" type="tns:MotorType" maxOccurs="2" />
                  <xs:element name="Mode" type="xs:normalizedString" />
                </xs:sequence>
              </xs:complexType>
              <xs:complexType name="MotorType">
                <xs:choice>
                  <xs:element name="Running" type="xs:boolean" />
                  <xs:element name="Speed" type="xs:float" />
                </xs:choice>
              </xs:complexType>
              <xs:complexType name="AllFields">
                <xs:all>
                  <xs:element name="Blob" type="xs:base64Binary" />
                  <xs:element name="Identifier" type="xs:guid" />
                </xs:all>
              </xs:complexType>
            </xs:schema>
            """;

        TypeDictionary dictionary = XmlSchemaParser.Parse(schema);
        TypeDescription plant = dictionary.TryGetByTypeId("Plant")!;
        TypeDescription motor = dictionary.TryGetByTypeId("MotorType")!;
        TypeDescription looseText = dictionary.TryGetByTypeId("LooseText")!;
        TypeDescription allFields = dictionary.TryGetByTypeId("AllFields")!;

        await Assert.That(dictionary.Name).IsEqualTo("http://example.com/cpx");
        await Assert.That(plant.Fields[0].Kind).IsEqualTo(TypeKind.StructReference);
        await Assert.That(plant.Fields[0].TypeId).IsEqualTo("MotorType");
        await Assert.That(plant.Fields[0].ElementCount).IsEqualTo(2);
        await Assert.That(plant.Fields[1].Kind).IsEqualTo(TypeKind.String);
        await Assert.That(motor.Fields[0].Kind).IsEqualTo(TypeKind.Boolean);
        await Assert.That(motor.Fields[1].Kind).IsEqualTo(TypeKind.Single);
        await Assert.That(looseText.Type).IsEqualTo(TypeKind.String);
        await Assert.That(allFields.Fields[0].Kind).IsEqualTo(TypeKind.Blob);
        await Assert.That(allFields.Fields[1].Kind).IsEqualTo(TypeKind.Guid);
    }

    [Test]
    public async Task XmlSchemaParser_InvalidSchemaRoots_ThrowFormatException()
    {
        await Assert.That(() => XmlSchemaParser.Parse("<notSchema />"))
            .Throws<FormatException>();
        await Assert.That(() => XmlSchemaParser.Parse("<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" />"))
            .Throws<FormatException>();
        await Assert.That(() => XmlSchemaParser.Parse("<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"><xs:element type=\"xs:string\" /></xs:schema>"))
            .Throws<FormatException>();
    }
}
