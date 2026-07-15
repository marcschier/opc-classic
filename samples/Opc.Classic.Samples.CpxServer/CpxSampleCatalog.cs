// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Samples.CpxServer;

public static class CpxSampleCatalog
{
    public const string OpcBinaryDictionaryId = "urn:opc-classic:samples:cpx:binary";
    public const string XmlDictionaryId = "urn:opc-classic:samples:cpx:xml";
    public const string VendorDictionaryId = "urn:vendor:sample:cbor";

    public const string OpcBinaryDictionary = """
        <?xml version="1.0" encoding="utf-8"?>
        <TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/"
                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        Name="Opc.Classic CPX Samples"
                        DefaultBigEndian="false"
                        DefaultStringEncoding="ASCII"
                        DefaultCharWidth="1">
          <TypeDescription TypeID="PrimitiveRecord">
            <Boolean Name="Enabled" />
            <Integer Name="Sequence" xsi:type="Int32" />
            <FloatingPoint Name="SetPoint" Length="8" />
            <CharString Name="Code" xsi:type="Ascii" Length="8" />
          </TypeDescription>
          <TypeDescription TypeID="TelemetryDetail">
            <CharString Name="Label" xsi:type="Ascii" Length="8" />
            <FloatingPoint Name="Temperature" Length="8" />
            <CharString Name="Status" xsi:type="Ascii" Length="8" />
          </TypeDescription>
          <TypeDescription TypeID="TelemetryPacket">
            <Integer Name="Version" xsi:type="UInt8" />
            <Boolean Name="Enabled" />
            <BitString Name="Flags" Length="9" />
            <Integer Name="Count" xsi:type="UInt8" />
            <Integer Name="Samples" xsi:type="UInt16" ElementCountRef="Count" />
            <TypeReference Name="Detail" TypeID="TelemetryDetail" />
          </TypeDescription>
        </TypeDictionary>
        """;

    public const string XmlSchemaDictionary = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   xmlns:tns="urn:opc-classic:cpx:sample"
                   targetNamespace="urn:opc-classic:cpx:sample"
                   elementFormDefault="qualified">
          <xs:element name="DeviceEnvelope">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="DeviceId" type="xs:string" />
                <xs:element name="ReadingCount" type="xs:unsignedByte" />
                <xs:element name="Readings" type="xs:double" maxOccurs="2" />
                <xs:element name="Status">
                  <xs:complexType>
                    <xs:sequence>
                      <xs:element name="Mode" type="xs:string" />
                      <xs:element name="Active" type="xs:boolean" />
                      <xs:element name="OptionalNote" type="xs:string" minOccurs="0" />
                    </xs:sequence>
                  </xs:complexType>
                </xs:element>
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    public const string XmlOptionalPresentPayload = """
        <DeviceEnvelope xmlns="urn:opc-classic:cpx:sample">
          <DeviceId>device-01</DeviceId>
          <ReadingCount>2</ReadingCount>
          <Readings>12.5</Readings>
          <Readings>13.75</Readings>
          <Status>
            <Mode>Automatic</Mode>
            <Active>true</Active>
            <OptionalNote>calibrated</OptionalNote>
          </Status>
        </DeviceEnvelope>
        """;

    public const string XmlOptionalMissingPayload = """
        <DeviceEnvelope xmlns="urn:opc-classic:cpx:sample">
          <DeviceId>device-02</DeviceId>
          <ReadingCount>2</ReadingCount>
          <Readings>10.0</Readings>
          <Readings>11.0</Readings>
          <Status>
            <Mode>Manual</Mode>
            <Active>false</Active>
          </Status>
        </DeviceEnvelope>
        """;

    public const string VendorDictionary = """
        {"format":"vendor-cbor","version":1,"type":"VendorEnvelope"}
        """;

    public static IReadOnlyList<string> ItemIds { get; } =
    [
        "Binary.Primitives",
        "Binary.NestedArrayBits",
        "Binary.InvalidPayload",
        "Xml.OptionalPresent",
        "Xml.OptionalMissing",
        "Vendor.CustomPayload",
    ];
}
