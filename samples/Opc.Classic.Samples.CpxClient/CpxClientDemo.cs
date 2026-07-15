// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Cpx;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Samples.CpxServer;

namespace Opc.Classic.Samples.CpxClient;

public static class CpxClientDemo
{
    public static async Task<CpxClientReport> RunAsync(
        CpxSampleServer server,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);
        ArgumentNullException.ThrowIfNull(output);

        var browsedItems = new List<string>();
        var decodedItems = new List<string>();
        var invalidPayloads = new List<string>();
        var unsupportedTypeSystems = new List<string>();
        ComplexValue? telemetry = null;
        TypeDictionary? telemetryDictionary = null;
        TypeDescription? telemetryType = null;

        OpcBrowseResult root = await server.BrowseAsync(null, OpcBrowseElementKind.All, cancellationToken).ConfigureAwait(false);
        await output.WriteLineAsync($"Root branches: {string.Join(", ", root.Branches)}").ConfigureAwait(false);

        foreach (string branch in new[] { "Binary", "Xml", "Vendor" })
        {
            OpcBrowseResult result = await server.BrowseAsync(branch, OpcBrowseElementKind.Items, cancellationToken).ConfigureAwait(false);
            foreach (string leaf in result.Items)
            {
                string itemId = await server.GetItemIdAsync(branch, leaf, cancellationToken).ConfigureAwait(false);
                browsedItems.Add(itemId);

                var typeSystem = ReadStringProperty(server, itemId, OpcComplexDataProperty.TypeSystemId);
                var dictionaryId = ReadStringProperty(server, itemId, OpcComplexDataProperty.DictionaryId);
                var typeId = ReadStringProperty(server, itemId, OpcComplexDataProperty.TypeId);
                var dictionaryValue = ReadStringProperty(server, itemId, OpcComplexDataProperty.Dictionary);
                var typeDescription = ReadStringProperty(server, itemId, OpcComplexDataProperty.TypeDescription);
                await output.WriteLineAsync(
                    $"{itemId}: system={typeSystem}, dictionary={dictionaryId}, type={typeId}, description={typeDescription}")
                    .ConfigureAwait(false);

                if (typeSystem.Equals(TypeDictionary.OpcBinaryTypeSystemId, StringComparison.Ordinal))
                {
                    try
                    {
                        TypeDictionary dictionary = OpcBinaryDictionaryParser.Parse(dictionaryValue);
                        TypeDescription type = dictionary.TryGetByTypeId(typeId)
                            ?? throw new KeyNotFoundException($"Type '{typeId}' was not found.");
                        ComplexValue value = OpcBinaryDecoder.Decode(ReadBytes(server.ReadItem(itemId)), type, dictionary);
                        decodedItems.Add(itemId);

                        if (itemId.Equals("Binary.NestedArrayBits", StringComparison.Ordinal))
                        {
                            telemetry = value;
                            telemetryDictionary = dictionary;
                            telemetryType = type;
                        }
                    }
                    catch (Exception exception) when (exception is FormatException or InvalidOperationException or KeyNotFoundException)
                    {
                        invalidPayloads.Add(itemId);
                        await output.WriteLineAsync($"{itemId}: invalid payload ({exception.GetType().Name})").ConfigureAwait(false);
                    }

                    continue;
                }

                if (typeSystem.Equals(TypeDictionary.XmlSchemaTypeSystemId, StringComparison.Ordinal))
                {
                    try
                    {
                        TypeDictionary dictionary = XmlSchemaParser.Parse(dictionaryValue);
                        TypeDescription type = dictionary.TryGetByTypeId(typeId)
                            ?? throw new KeyNotFoundException($"Type '{typeId}' was not found.");
                        _ = XmlComplexValueSerializer.Deserialize(ReadString(server.ReadItem(itemId)), type, dictionary);
                        decodedItems.Add(itemId);
                    }
                    catch (Exception exception) when (exception is FormatException or InvalidOperationException or KeyNotFoundException)
                    {
                        invalidPayloads.Add(itemId);
                        await output.WriteLineAsync($"{itemId}: invalid payload ({exception.GetType().Name})").ConfigureAwait(false);
                    }

                    continue;
                }

                unsupportedTypeSystems.Add(typeSystem);
                await output.WriteLineAsync($"{itemId}: unsupported vendor type system '{typeSystem}'").ConfigureAwait(false);
            }
        }

        if (telemetry is null || telemetryDictionary is null || telemetryType is null)
        {
            throw new InvalidOperationException("The deterministic telemetry item was not decoded.");
        }

        IOpcCpxDataFilter filter = server.DataFilter;
        OpcCpxFilterResult filterResult = filter.Apply(
            telemetry,
            telemetryType,
            "Detail.Status = Running AND Count = 3");
        OpcCpxFilterResult unsupportedFilter = filter.Apply(telemetry, telemetryType, "Detail.Status LIKE 'Running'");

        TypeDictionary requestedDictionary = CreateRequestedDictionary();
        TypeDescription requestedType = requestedDictionary.TryGetByTypeId("TelemetryPacket32")!;
        IOpcCpxTypeConverter converter = server.TypeConverter;
        OpcCpxConversionResult conversion = converter.Convert(
            telemetry,
            telemetryType,
            requestedType,
            telemetryDictionary,
            requestedDictionary);
        OpcCpxConversionResult unsupportedConversion = converter.Convert(
            new byte[] { 0x80 },
            TypeKind.BitString,
            TypeKind.UInt16);

        await output.WriteLineAsync(
            $"Reference bounds: conversionDepth={OpcCpxReferenceTypeConverter.MaxNestingDepth}, "
            + $"arrayElements={OpcCpxReferenceTypeConverter.MaxArrayElements}, "
            + $"filterLength={OpcCpxReferenceDataFilter.MaxExpressionLength}, "
            + $"filterComparisons={OpcCpxReferenceDataFilter.MaxComparisons}")
            .ConfigureAwait(false);
        await output.WriteLineAsync(
            $"Results: filter=0x{filterResult.Error:X8}, conversion=0x{conversion.Error:X8}, "
            + $"vendorFilter=0x{unsupportedFilter.Error:X8}, bitConversion=0x{unsupportedConversion.Error:X8}")
            .ConfigureAwait(false);

        return new CpxClientReport(
            browsedItems,
            decodedItems,
            invalidPayloads,
            unsupportedTypeSystems,
            filterResult.Error,
            conversion.Error,
            unsupportedFilter.Error,
            unsupportedConversion.Error);
    }

    private static string ReadStringProperty(CpxSampleServer server, string itemId, int propertyId)
    {
        (OpcVariant value, int error) = server.TryGetPropertyValue(itemId, propertyId);
        if (error != OpcResultId.Ok.Code || value.AsString() is not { } text)
        {
            throw new InvalidOperationException($"CPX property {propertyId} failed with 0x{error:X8}.");
        }

        return text;
    }

    private static byte[] ReadBytes(OpcVariant value)
    {
        OpcSafeArray array = value.AsSafeArray()
            ?? throw new InvalidOperationException("Expected a byte SAFEARRAY.");
        return array.Data as byte[]
            ?? throw new InvalidOperationException("Expected VT_UI1 SAFEARRAY storage.");
    }

    private static string ReadString(OpcVariant value) =>
        value.AsString() ?? throw new InvalidOperationException("Expected a string payload.");

    private static TypeDictionary CreateRequestedDictionary()
    {
        var detail = new TypeDescription(
            "TelemetryDetail32",
            "TelemetryDetail32",
            TypeKind.StructReference,
            isComplex: true,
            [
                new TypeField("Label", TypeKind.String, Length: 8),
                new TypeField("Temperature", TypeKind.Double),
                new TypeField("Status", TypeKind.String, Length: 8),
            ]);
        var packet = new TypeDescription(
            "TelemetryPacket32",
            "TelemetryPacket32",
            TypeKind.StructReference,
            isComplex: true,
            [
                new TypeField("Version", TypeKind.UInt32),
                new TypeField("Enabled", TypeKind.Boolean),
                new TypeField("Flags", TypeKind.BitString, Length: 9),
                new TypeField("Count", TypeKind.UInt32),
                new TypeField("Samples", TypeKind.UInt32, ElementCountFieldName: "Count"),
                new TypeField("Detail", TypeKind.StructReference, detail.TypeId),
            ]);
        return new TypeDictionary("Requested", [detail, packet], defaultBigEndian: false);
    }
}
