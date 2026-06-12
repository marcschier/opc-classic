//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Text;
using System.Xml;
using Opc.Classic.Xml.Serialization;

namespace Opc.Classic.Xml.Tests;

public sealed class XmlDaValueSerializerTests
{
    private static XmlDaValue RoundTrip(XmlDaValue value)
    {
        string requestXml = SerializeWriteValue(value);
        string valueElement = ExtractValueElement(requestXml);
        string responseXml = $$"""
            <?xml version="1.0"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                  <ReadResult ServerState="running" />
                  <RItemList>
                    <Items ItemName="Tag1">
                      {{valueElement}}
                      <Quality QualityField="good" />
                    </Items>
                  </RItemList>
                </ReadResponse>
              </soap:Body>
            </soap:Envelope>
            """;

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(responseXml));
        using var reader = new SoapEnvelopeReader(ms);
        return ReadSerializer.ReadResponse(reader).Items[0].Value!;
    }

    private static string SerializeWriteValue(XmlDaValue value)
    {
        using var ms = new MemoryStream();
        using (var writer = new SoapEnvelopeWriter(ms))
        {
            WriteSerializer.WriteRequest(writer, new XmlDaWriteRequest(
                new XmlDaRequestHeader(null, null),
                new[] { new XmlDaWriteItem("Tag1", null, value) }));
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static string ExtractValueElement(string xml)
    {
        int start = xml.IndexOf("<Value", StringComparison.Ordinal);
        int end = xml.IndexOf("</Value>", start, StringComparison.Ordinal);
        if (start < 0 || end < 0)
        {
            throw new InvalidDataException("Serialized XML did not contain a Value element.");
        }

        return xml[start..(end + "</Value>".Length)];
    }

    [Test]
    public async Task ArrayOfByte_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfByte(new sbyte[] { -128, 0, 127 }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfByte);
        await Assert.That(parsed.AsArrayOfByte()!.SequenceEqual(new sbyte[] { -128, 0, 127 })).IsTrue();
    }

    [Test]
    public async Task ArrayOfShort_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfShort(new short[] { -123, 0, 456 }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfShort);
        await Assert.That(parsed.AsArrayOfShort()!.SequenceEqual(new short[] { -123, 0, 456 })).IsTrue();
    }

    [Test]
    public async Task ArrayOfInt_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfInt(new[] { -1, 0, 123456 }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfInt);
        await Assert.That(parsed.AsArrayOfInt()!.SequenceEqual(new[] { -1, 0, 123456 })).IsTrue();
    }

    [Test]
    public async Task ArrayOfLong_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfLong(new[] { -1L, 0L, 9_000_000_000L }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfLong);
        await Assert.That(parsed.AsArrayOfLong()!.SequenceEqual(new[] { -1L, 0L, 9_000_000_000L })).IsTrue();
    }

    [Test]
    public async Task ArrayOfFloat_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfFloat(new[] { -1.25f, 0f, 3.5f }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfFloat);
        await Assert.That(parsed.AsArrayOfFloat()!.SequenceEqual(new[] { -1.25f, 0f, 3.5f })).IsTrue();
    }

    [Test]
    public async Task ArrayOfDouble_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfDouble(new[] { -1.25d, 0d, 3.5d }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfDouble);
        await Assert.That(parsed.AsArrayOfDouble()!.SequenceEqual(new[] { -1.25d, 0d, 3.5d })).IsTrue();
    }

    [Test]
    public async Task ArrayOfString_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfString(new string?[] { "alpha", null, "gamma" }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfString);
        await Assert.That(parsed.AsArrayOfString()!.SequenceEqual(new string?[] { "alpha", null, "gamma" })).IsTrue();
    }

    [Test]
    public async Task ArrayOfBool_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfArrayOfBool(new[] { true, false, true }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfBoolean);
        await Assert.That(parsed.AsArrayOfBool()!.SequenceEqual(new[] { true, false, true })).IsTrue();
    }

    [Test]
    public async Task ArrayOfDateTime_RoundTrips()
    {
        var values = new[]
        {
            new DateTimeOffset(2026, 5, 22, 3, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 23, 4, 5, 6, TimeSpan.Zero),
        };

        var parsed = RoundTrip(XmlDaValue.OfArrayOfDateTime(values));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.ArrayOfDateTime);
        await Assert.That(parsed.AsArrayOfDateTime()!.SequenceEqual(values)).IsTrue();
    }

    [Test]
    public async Task Base64Binary_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfBase64Binary(new byte[] { 0, 1, 2, 255 }));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.Base64Binary);
        await Assert.That(parsed.AsBase64Binary()!.SequenceEqual(new byte[] { 0, 1, 2, 255 })).IsTrue();
    }

    [Test]
    public async Task Decimal_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfDecimal(123.456m));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.Decimal);
        await Assert.That(parsed.AsDecimal()).IsEqualTo(123.456m);
    }

    [Test]
    public async Task Time_RoundTrips()
    {
        var expected = new TimeOnly(12, 34, 56);
        var parsed = RoundTrip(XmlDaValue.OfTime(expected));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.Time);
        await Assert.That(parsed.AsTime()).IsEqualTo(expected);
    }

    [Test]
    public async Task Date_RoundTrips()
    {
        var expected = new DateOnly(2026, 5, 22);
        var parsed = RoundTrip(XmlDaValue.OfDate(expected));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.Date);
        await Assert.That(parsed.AsDate()).IsEqualTo(expected);
    }

    [Test]
    public async Task Duration_RoundTrips()
    {
        var expected = TimeSpan.FromDays(1) + TimeSpan.FromMinutes(90);
        var parsed = RoundTrip(XmlDaValue.OfDuration(expected));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.Duration);
        await Assert.That(parsed.AsDuration()).IsEqualTo(expected);
    }

    [Test]
    public async Task QName_RoundTrips()
    {
        var parsed = RoundTrip(XmlDaValue.OfQName(new XmlQualifiedName("S_CLAMP")));

        await Assert.That(parsed.Type).IsEqualTo(XmlDaValueType.QName);
        await Assert.That(parsed.AsQName()!.Name).IsEqualTo("S_CLAMP");
    }
}
