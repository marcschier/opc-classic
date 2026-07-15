// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Cpx;
using Opc.Classic.Cpx.Hosting;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;

namespace Opc.Classic.Samples.CpxServer;

public sealed class CpxSampleServer :
    IOpcDaServer,
    IOpcAddressSpace,
    IOpcItemPropertyProvider,
    IOpcItemPropertyMetadataProvider
{
    private static readonly DateTimeOffset StartTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly string[] _itemIds;
    private readonly IReadOnlyDictionary<string, OpcVariant> _items;
    private readonly OpcCpxAddressSpace _addressSpace;
    private readonly OpcCpxItemProperties _properties;

    public CpxSampleServer()
    {
        BinaryDictionary = OpcBinaryDictionaryParser.Parse(CpxSampleCatalog.OpcBinaryDictionary);
        XmlDictionary = XmlSchemaParser.Parse(CpxSampleCatalog.XmlSchemaDictionary);
        VendorDictionary = TypeDictionary.FromTypes(
            new TypeDescription(
                "VendorEnvelope",
                "VendorEnvelope",
                TypeKind.StructReference,
                isComplex: true,
                [new TypeField("Payload", TypeKind.Blob)]));

        _itemIds = CpxSampleCatalog.ItemIds.ToArray();
        var options = CreateOptions(BinaryDictionary, XmlDictionary, VendorDictionary);
        _addressSpace = new OpcCpxAddressSpace(new SampleAddressSpace(_itemIds), options);
        _properties = new OpcCpxItemProperties(options);
        _items = CreateItems(BinaryDictionary);
        TypeConverter = new OpcCpxReferenceTypeConverter();
        DataFilter = new OpcCpxReferenceDataFilter();
    }

    public TypeDictionary BinaryDictionary { get; }

    public TypeDictionary XmlDictionary { get; }

    public TypeDictionary VendorDictionary { get; }

    public IOpcCpxTypeConverter TypeConverter { get; }

    public IOpcCpxDataFilter DataFilter { get; }

    public bool IsHierarchical => _addressSpace.IsHierarchical;

    public IReadOnlyList<string> ItemIds => _itemIds;

    public OpcVariant ReadItem(string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        return _items.TryGetValue(itemId, out var value)
            ? value
            : throw new OpcException(OpcResultId.UnknownItemId);
    }

    public Task<OpcBrowseResult> BrowseAsync(
        string? branchPath,
        OpcBrowseElementKind kind,
        CancellationToken cancellationToken = default) =>
        _addressSpace.BrowseAsync(branchPath, kind, cancellationToken);

    public Task<string> GetItemIdAsync(
        string? currentBranchPath,
        string itemDataId,
        CancellationToken cancellationToken = default) =>
        _addressSpace.GetItemIdAsync(currentBranchPath, itemDataId, cancellationToken);

    public (OpcVariant Value, int Error) TryGetPropertyValue(string itemId, int propertyId) =>
        _properties.TryGetPropertyValue(itemId, propertyId);

    public IReadOnlyList<OpcStandardProperty> GetAvailableProperties(string itemId) =>
        _properties.GetAvailableProperties(itemId);

    public (string ItemId, int Error) TryGetPropertyItemId(string itemId, int propertyId) =>
        _properties.TryGetPropertyItemId(itemId, propertyId);

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = 0,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Opc.Classic managed CPX sample server",
        });
    }

    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        _ = name;
        _ = active;
        _ = requestedUpdateRate;
        _ = clientHandle;
        _ = localeId;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotSupported);
    }

    public Task AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientGroupHandle,
        int timeBias,
        float percentDeadband,
        int localeId,
        Guid requestedInterfaceId,
        out int serverGroupHandle,
        out int revisedUpdateRate,
        out IOpcInterfaceRef group,
        CancellationToken cancellationToken = default)
    {
        _ = name;
        _ = active;
        _ = requestedUpdateRate;
        _ = timeBias;
        _ = percentDeadband;
        _ = localeId;
        cancellationToken.ThrowIfCancellationRequested();
        serverGroupHandle = 0;
        revisedUpdateRate = 0;
        group = new OpcInterfaceRef(
            requestedInterfaceId,
            0,
            1,
            1,
            unchecked((ulong)(uint)clientGroupHandle),
            Guid.Parse("11111111-2222-3333-4444-555555555555"),
            0,
            []);
        throw new OpcException(OpcResultId.NotSupported);
    }

    public Task RemoveGroupAsync(
        int serverGroupHandle,
        bool force,
        CancellationToken cancellationToken = default)
    {
        _ = serverGroupHandle;
        _ = force;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotSupported);
    }

    public Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        _ = localeId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"Opc.Classic CPX sample error 0x{errorCode:X8}");
    }

    private static OpcCpxOptions CreateOptions(
        TypeDictionary binaryDictionary,
        TypeDictionary xmlDictionary,
        TypeDictionary vendorDictionary)
    {
        var options = new OpcCpxOptions()
            .AddDictionary(
                TypeDictionary.OpcBinaryTypeSystemId,
                CpxSampleCatalog.OpcBinaryDictionaryId,
                binaryDictionary,
                CpxSampleCatalog.OpcBinaryDictionary,
                CreateTypeDescriptions(binaryDictionary),
                "SampleBinary")
            .AddDictionary(
                TypeDictionary.XmlSchemaTypeSystemId,
                CpxSampleCatalog.XmlDictionaryId,
                xmlDictionary,
                CpxSampleCatalog.XmlSchemaDictionary,
                CreateTypeDescriptions(xmlDictionary),
                "SampleXml")
            .AddDictionary(
                "Vendor-CBOR-1",
                CpxSampleCatalog.VendorDictionaryId,
                vendorDictionary,
                CpxSampleCatalog.VendorDictionary,
                CreateTypeDescriptions(vendorDictionary),
                "VendorCbor");

        options
            .AddComplexItem(
                "Binary.Primitives",
                TypeDictionary.OpcBinaryTypeSystemId,
                CpxSampleCatalog.OpcBinaryDictionaryId,
                "PrimitiveRecord",
                "PT0S",
                "ReadOnly")
            .AddComplexItem(
                "Binary.NestedArrayBits",
                TypeDictionary.OpcBinaryTypeSystemId,
                CpxSampleCatalog.OpcBinaryDictionaryId,
                "TelemetryPacket",
                "PT0S",
                "ReadOnly",
                unfilteredItemId: "Binary.NestedArrayBits",
                dataFilterValue: "Detail.Status = Running AND Count = 3")
            .AddComplexItem(
                "Binary.InvalidPayload",
                TypeDictionary.OpcBinaryTypeSystemId,
                CpxSampleCatalog.OpcBinaryDictionaryId,
                "TelemetryPacket",
                "PT0S",
                "ReadOnly")
            .AddComplexItem(
                "Xml.OptionalPresent",
                TypeDictionary.XmlSchemaTypeSystemId,
                CpxSampleCatalog.XmlDictionaryId,
                "DeviceEnvelope",
                "PT0S",
                "ReadOnly")
            .AddComplexItem(
                "Xml.OptionalMissing",
                TypeDictionary.XmlSchemaTypeSystemId,
                CpxSampleCatalog.XmlDictionaryId,
                "DeviceEnvelope",
                "PT0S",
                "ReadOnly")
            .AddComplexItem(
                "Vendor.CustomPayload",
                "Vendor-CBOR-1",
                CpxSampleCatalog.VendorDictionaryId,
                "VendorEnvelope",
                "PT0S",
                "VendorDefined");

        return options;
    }

    private static IReadOnlyDictionary<string, OpcVariant> CreateItems(TypeDictionary dictionary)
    {
        var primitiveType = dictionary.TryGetByTypeId("PrimitiveRecord")!;
        var detailType = dictionary.TryGetByTypeId("TelemetryDetail")!;
        var packetType = dictionary.TryGetByTypeId("TelemetryPacket")!;

        var primitives = CreateValue(
            primitiveType,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Enabled"] = true,
                ["Sequence"] = 42,
                ["SetPoint"] = 12.5D,
                ["Code"] = "P-100",
            });
        var detail = CreateValue(
            detailType,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Label"] = "reactor",
                ["Temperature"] = 325.25D,
                ["Status"] = "Running",
            });
        var packet = CreateValue(
            packetType,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Version"] = (byte)1,
                ["Enabled"] = true,
                ["Flags"] = new byte[] { 0xA5, 0x80 },
                ["Count"] = (byte)3,
                ["Samples"] = new object?[] { (ushort)100, (ushort)200, (ushort)300 },
                ["Detail"] = detail,
            });

        return new Dictionary<string, OpcVariant>(StringComparer.Ordinal)
        {
            ["Binary.Primitives"] = ToByteArrayVariant(OpcBinaryEncoder.Encode(primitives, primitiveType, dictionary)),
            ["Binary.NestedArrayBits"] = ToByteArrayVariant(OpcBinaryEncoder.Encode(packet, packetType, dictionary)),
            ["Binary.InvalidPayload"] = ToByteArrayVariant([0x01, 0x01, 0xA5]),
            ["Xml.OptionalPresent"] = OpcVariant.FromString(CpxSampleCatalog.XmlOptionalPresentPayload),
            ["Xml.OptionalMissing"] = OpcVariant.FromString(CpxSampleCatalog.XmlOptionalMissingPayload),
            ["Vendor.CustomPayload"] = ToByteArrayVariant([0xA1, 0x61, 0x76, 0x01]),
        };
    }

    private static IReadOnlyDictionary<string, string> CreateTypeDescriptions(TypeDictionary dictionary)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var type in dictionary.Types)
        {
            values.Add(type.TypeId, $"TypeID={type.TypeId}; Fields={type.Fields.Count}");
        }

        return values;
    }

    private static ComplexValue CreateValue(
        TypeDescription type,
        IReadOnlyDictionary<string, object?> fields) =>
        new()
        {
            Type = new StructType { Name = type.Name },
            Fields = fields,
        };

    private static OpcVariant ToByteArrayVariant(byte[] value) =>
        OpcVariant.FromSafeArray(OpcSafeArray.OfUInt8(value));

    private sealed class SampleAddressSpace : IOpcAddressSpace
    {
        private readonly IReadOnlyList<string> _itemIds;

        internal SampleAddressSpace(IReadOnlyList<string> itemIds)
        {
            _itemIds = itemIds;
        }

        public bool IsHierarchical => true;

        public Task<OpcBrowseResult> BrowseAsync(
            string? branchPath,
            OpcBrowseElementKind kind,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(branchPath))
            {
                IReadOnlyList<string> branches = kind == OpcBrowseElementKind.Items
                    ? []
                    : ["Binary", "Xml", "Vendor"];
                return Task.FromResult(new OpcBrowseResult(branches, []));
            }

            var normalized = branchPath.Trim().Trim('/', '\\');
            var prefix = normalized + ".";
            var items = kind == OpcBrowseElementKind.Branches
                ? []
                : _itemIds
                    .Where(itemId => itemId.StartsWith(prefix, StringComparison.Ordinal))
                    .Select(itemId => itemId[prefix.Length..])
                    .ToArray();
            return Task.FromResult(new OpcBrowseResult([], items));
        }

        public Task<string> GetItemIdAsync(
            string? currentBranchPath,
            string itemDataId,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemDataId);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(string.IsNullOrWhiteSpace(currentBranchPath)
                ? itemDataId
                : $"{currentBranchPath.Trim().Trim('/', '\\')}.{itemDataId}");
        }
    }
}
