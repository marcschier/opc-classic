// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Cpx;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Samples.CpxClient;
using Opc.Classic.Samples.CpxServer;

namespace Opc.Classic.Samples.Cpx.Tests;

public sealed class CpxSampleTests
{
    [Test]
    public async Task Server_BrowseAndPropertiesExposeDeterministicCpxCatalog()
    {
        var server = new CpxSampleServer();

        OpcBrowseResult root = await server.BrowseAsync(null, OpcBrowseElementKind.All);
        OpcBrowseResult cpx = await server.BrowseAsync("CPX", OpcBrowseElementKind.Branches);
        OpcBrowseResult binary = await server.BrowseAsync("Binary", OpcBrowseElementKind.Items);
        (OpcVariant dictionaryId, int dictionaryError) = server.TryGetPropertyValue(
            "Binary.NestedArrayBits",
            OpcComplexDataProperty.DictionaryId);
        (OpcVariant typeId, int typeError) = server.TryGetPropertyValue(
            "Binary.NestedArrayBits",
            OpcComplexDataProperty.TypeId);

        await Assert.That(root.Branches).Contains("CPX");
        await Assert.That(root.Branches).Contains("Binary");
        await Assert.That(cpx.Branches).Contains(TypeDictionary.OpcBinaryTypeSystemId);
        await Assert.That(cpx.Branches).Contains(TypeDictionary.XmlSchemaTypeSystemId);
        await Assert.That(binary.Items).Contains("NestedArrayBits");
        await Assert.That(CpxSampleCatalog.ItemIds.Count).IsEqualTo(6);
        await Assert.That(dictionaryError).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(dictionaryId.AsString()).IsEqualTo(CpxSampleCatalog.OpcBinaryDictionaryId);
        await Assert.That(typeError).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(typeId.AsString()).IsEqualTo("TelemetryPacket");
    }

    [Test]
    public async Task Server_BinaryPayloadDecodesNestedArrayAndBitString()
    {
        var server = new CpxSampleServer();
        TypeDescription type = server.BinaryDictionary.TryGetByTypeId("TelemetryPacket")!;
        OpcSafeArray payload = server.ReadItem("Binary.NestedArrayBits").AsSafeArray()!;

        ComplexValue decoded = OpcBinaryDecoder.Decode((byte[])payload.Data, type, server.BinaryDictionary);
        var samples = (object?[])decoded["Samples"]!;
        var detail = (ComplexValue)decoded["Detail"]!;

        await Assert.That(decoded.TryGet<byte>("Count", out byte count)).IsTrue();
        await Assert.That(count).IsEqualTo((byte)3);
        await Assert.That(samples.Length).IsEqualTo(3);
        await Assert.That((ushort)samples[2]!).IsEqualTo((ushort)300);
        await Assert.That(((byte[])decoded["Flags"]!).Length).IsEqualTo(2);
        await Assert.That(detail.TryGet<string>("Status", out string status)).IsTrue();
        await Assert.That(status).IsEqualTo("Running");
    }

    [Test]
    public async Task Client_IntegrationAndSmokeReportsValidInvalidAndUnsupportedPaths()
    {
        using var output = new StringWriter();

        CpxClientReport report = await CpxClientDemo.RunAsync(new CpxSampleServer(), output);
        string text = output.ToString();

        await Assert.That(report.BrowsedItems.Count).IsEqualTo(6);
        await Assert.That(report.DecodedItems).Contains("Binary.Primitives");
        await Assert.That(report.DecodedItems).Contains("Binary.NestedArrayBits");
        await Assert.That(report.DecodedItems).Contains("Xml.OptionalPresent");
        await Assert.That(report.InvalidPayloads).Contains("Binary.InvalidPayload");
        await Assert.That(report.InvalidPayloads).Contains("Xml.OptionalMissing");
        await Assert.That(report.UnsupportedTypeSystems).Contains("Vendor-CBOR-1");
        await Assert.That(report.FilterResult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(report.ConversionResult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(report.UnsupportedFilterResult).IsEqualTo(OpcComplexDataResult.OPCCPX_E_FILTER_INVALID);
        await Assert.That(report.UnsupportedConversionResult).IsEqualTo(OpcComplexDataResult.OPCCPX_E_TYPE_CHANGED);
        await Assert.That(text).Contains("Reference bounds:");
        await Assert.That(text).Contains("unsupported vendor type system");
    }
}
