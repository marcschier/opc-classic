// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
using Opc.Classic.Ndr;

#pragma warning disable MA0048 // The DX codec table is intentionally grouped for spec readability.

namespace Opc.Classic.Dx.Ndr;

/// <summary>
/// Registry of OPC DX structure codecs enabled for generated and hand-written proxies.
/// </summary>
public static class NdrOpcDxCodecRegistry
{
    /// <summary>
    /// The 16 OPC DX codec entries covered by the DX 1.00 configuration and status structures.
    /// </summary>
    public static IReadOnlyList<string> RegisteredCodecNames { get; } = new[]
    {
        "OPCDX_ITEM_IDENTIFIER",
        "OPCDX_IDENTIFIED_RESULT",
        "OPCDX_GENERAL_RESPONSE",
        "OPCDX_SOURCE_SERVER",
        "OPCDX_CONNECTION",
        "OPCDX_ERROR",
        "OPCDX_QUALITY",
        "OPCDX_SERVER_STATUS",
        "OPCDX_CONNECTION_STATUS",
        "OPCDX_SOURCE_SERVER_STATUS",
        "OPCDX_STRING_ARRAY",
        "OPCDX_HRESULT_ARRAY",
        "OPCDX_ITEM_IDENTIFIER_ARRAY",
        "OPCDX_IDENTIFIED_RESULT_ARRAY",
        "OPCDX_SOURCE_SERVER_ARRAY",
        "OPCDX_CONNECTION_ARRAY",
    };
}

/// <summary>
/// NDR codec for <c>OpcDxItemIdentifier</c>.
/// </summary>
public static class NdrOpcDxItemIdentifierCodec
{
    /// <summary>
    /// Writes an item identifier.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxItemIdentifier value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteInline(ref writer, value);
        WriteDeferred(ref writer, value);
    }

    /// <summary>
    /// Reads an item identifier.
    /// </summary>
    public static DxItemIdentifier Read(ref NdrReader reader)
    {
        ItemIdentifierInline inline = ReadInline(ref reader);
        return ApplyDeferred(ref reader, inline);
    }

    internal static void WriteConformantArrayBody(ref NdrWriter writer, DxItemIdentifier[] values)
    {
        foreach (DxItemIdentifier value in values) { WriteInline(ref writer, value); }
        foreach (DxItemIdentifier value in values) { WriteDeferred(ref writer, value); }
    }

    internal static DxItemIdentifier[] ReadConformantArrayBody(ref NdrReader reader, int count)
    {
        var inline = new ItemIdentifierInline[count];
        for (int i = 0; i < count; i++) { inline[i] = ReadInline(ref reader); }
        var values = new DxItemIdentifier[count];
        for (int i = 0; i < count; i++) { values[i] = ApplyDeferred(ref reader, inline[i]); }
        return values;
    }

    private static void WriteInline(ref NdrWriter writer, DxItemIdentifier value)
    {
        writer.WriteUniquePointerReferent(value.ItemPath is not null);
        writer.WriteUniquePointerReferent(value.ItemName is not null);
        writer.WriteUniquePointerReferent(value.Version is not null);
        writer.WriteUInt32(unchecked((uint)value.Reserved));
    }

    private static void WriteDeferred(ref NdrWriter writer, DxItemIdentifier value)
    {
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemPath);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemName);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Version);
    }

    private static ItemIdentifierInline ReadInline(ref NdrReader reader) =>
        new(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());

    private static DxItemIdentifier ApplyDeferred(ref NdrReader reader, ItemIdentifierInline inline) =>
        new(
            NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemPathRef),
            NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemNameRef),
            NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.VersionRef),
            unchecked((int)inline.Reserved));

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct ItemIdentifierInline(
        uint ItemPathRef,
        uint ItemNameRef,
        uint VersionRef,
        uint Reserved);
}

/// <summary>
/// NDR codec for <c>OpcDxIdentifiedResult</c>.
/// </summary>
public static class NdrOpcDxIdentifiedResultCodec
{
    /// <summary>
    /// Writes an identified result.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxIdentifiedResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteInline(ref writer, value);
        WriteDeferred(ref writer, value);
    }

    /// <summary>
    /// Reads an identified result.
    /// </summary>
    public static DxIdentifiedResult Read(ref NdrReader reader)
    {
        IdentifiedResultInline inline = ReadInline(ref reader);
        return ApplyDeferred(ref reader, inline);
    }

    internal static void WriteConformantArrayBody(ref NdrWriter writer, DxIdentifiedResult[] values)
    {
        foreach (DxIdentifiedResult value in values) { WriteInline(ref writer, value); }
        foreach (DxIdentifiedResult value in values) { WriteDeferred(ref writer, value); }
    }

    internal static DxIdentifiedResult[] ReadConformantArrayBody(ref NdrReader reader, int count)
    {
        var inline = new IdentifiedResultInline[count];
        for (int i = 0; i < count; i++) { inline[i] = ReadInline(ref reader); }
        var values = new DxIdentifiedResult[count];
        for (int i = 0; i < count; i++) { values[i] = ApplyDeferred(ref reader, inline[i]); }
        return values;
    }

    private static void WriteInline(ref NdrWriter writer, DxIdentifiedResult value)
    {
        writer.WriteUniquePointerReferent(value.ItemPath is not null);
        writer.WriteUniquePointerReferent(value.ItemName is not null);
        writer.WriteUniquePointerReferent(value.Version is not null);
        writer.WriteInt32(value.ResultId.Code);
    }

    private static void WriteDeferred(ref NdrWriter writer, DxIdentifiedResult value)
    {
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemPath);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemName);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Version);
    }

    private static IdentifiedResultInline ReadInline(ref NdrReader reader) =>
        new(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadInt32());

    private static DxIdentifiedResult ApplyDeferred(ref NdrReader reader, IdentifiedResultInline inline) =>
        new(
            NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemPathRef),
            NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemNameRef),
            NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.VersionRef),
            new OpcResultId(inline.ResultCode, null));

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct IdentifiedResultInline(
        uint ItemPathRef,
        uint ItemNameRef,
        uint VersionRef,
        int ResultCode);
}

/// <summary>
/// NDR codec for <c>OpcDxGeneralResponse</c>.
/// </summary>
public static class NdrOpcDxGeneralResponseCodec
{
    /// <summary>
    /// Writes a general response.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxGeneralResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        DxIdentifiedResult[] results = value.IdentifiedResults;
        writer.WriteUniquePointerReferent(value.ConfigurationVersion is not null);
        writer.WriteUInt32(checked((uint)results.Length));
        writer.WriteUniquePointerReferent(results.Length > 0);
        writer.WriteUInt32(unchecked((uint)value.Reserved));
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ConfigurationVersion);
        if (results.Length > 0)
        {
            NdrOpcDxIdentifiedResultArrayCodec.Write(ref writer, results);
        }
    }

    /// <summary>
    /// Reads a general response.
    /// </summary>
    public static DxGeneralResponse Read(ref NdrReader reader)
    {
        uint configurationVersionRef = reader.ReadUInt32();
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "general response identified-result count");
        uint resultsRef = reader.ReadUInt32();
        int reserved = unchecked((int)reader.ReadUInt32());
        string? configurationVersion = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, configurationVersionRef);
        DxIdentifiedResult[] results = resultsRef == 0u
            ? Array.Empty<DxIdentifiedResult>()
            : NdrOpcDxIdentifiedResultArrayCodec.Read(ref reader, count);
        if (resultsRef == 0u && count != 0)
        {
            throw new InvalidDataException("OPC DX general response has a null result pointer with a non-zero count.");
        }
        return new DxGeneralResponse(configurationVersion, results, reserved);
    }
}

/// <summary>
/// NDR codec for <c>OpcDxSourceServer</c>.
/// </summary>
public static class NdrOpcDxSourceServerCodec
{
    /// <summary>
    /// Writes a source server.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxSourceServer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteInline(ref writer, value);
        WriteDeferred(ref writer, value);
    }

    /// <summary>
    /// Reads a source server.
    /// </summary>
    public static DxSourceServer Read(ref NdrReader reader)
    {
        SourceServerInline inline = ReadInline(ref reader);
        return ApplyDeferred(ref reader, inline);
    }

    private static bool Has(DxMask mask, DxMask bit) => (mask & bit) != DxMask.None;

    internal static void WriteConformantArrayBody(ref NdrWriter writer, DxSourceServer[] values)
    {
        foreach (DxSourceServer value in values) { WriteInline(ref writer, value); }
        foreach (DxSourceServer value in values) { WriteDeferred(ref writer, value); }
    }

    internal static DxSourceServer[] ReadConformantArrayBody(ref NdrReader reader, int count)
    {
        var inline = new SourceServerInline[count];
        for (int i = 0; i < count; i++) { inline[i] = ReadInline(ref reader); }
        var values = new DxSourceServer[count];
        for (int i = 0; i < count; i++) { values[i] = ApplyDeferred(ref reader, inline[i]); }
        return values;
    }

    private static void WriteInline(ref NdrWriter writer, DxSourceServer value)
    {
        writer.WriteUInt32(unchecked((uint)value.Mask));
        writer.WriteUniquePointerReferent(value.ItemPath is not null);
        writer.WriteUniquePointerReferent(value.ItemName is not null);
        writer.WriteUniquePointerReferent(value.Version is not null);
        writer.WriteUniquePointerReferent(value.Name is not null);
        writer.WriteUniquePointerReferent(value.Description is not null);
        writer.WriteUniquePointerReferent(value.ServerType is not null);
        writer.WriteUniquePointerReferent(value.ServerUrl is not null);
        writer.WriteInt32(value.DefaultConnected == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteUInt32(unchecked((uint)value.Reserved));
    }

    private static void WriteDeferred(ref NdrWriter writer, DxSourceServer value)
    {
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemPath);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemName);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Version);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Name);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Description);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ServerType);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ServerUrl);
    }

    private static SourceServerInline ReadInline(ref NdrReader reader) =>
        new(
            (DxMask)reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadInt32(),
            reader.ReadUInt32());

    private static DxSourceServer ApplyDeferred(ref NdrReader reader, SourceServerInline inline)
    {
        string? itemPath = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemPathRef);
        string? itemName = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemNameRef);
        string? version = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.VersionRef);
        string? name = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.NameRef);
        string? description = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.DescriptionRef);
        string? serverType = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ServerTypeRef);
        string? serverUrl = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ServerUrlRef);
        return new DxSourceServer(
            name: Has(inline.Mask, DxMask.Name) ? name : null,
            serverUrl: Has(inline.Mask, DxMask.ServerUrl) ? serverUrl : null,
            description: Has(inline.Mask, DxMask.Description) ? description : null,
            serverType: Has(inline.Mask, DxMask.ServerType) ? serverType : null,
            itemPath: Has(inline.Mask, DxMask.ItemPath) ? itemPath : null,
            itemName: Has(inline.Mask, DxMask.ItemName) ? itemName : null,
            version: Has(inline.Mask, DxMask.Version) ? version : null,
            defaultConnected: Has(inline.Mask, DxMask.DefaultSourceServerConnected) ? inline.DefaultConnected != 0 : null,
            mask: (int)inline.Mask,
            reserved: unchecked((int)inline.Reserved));
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct SourceServerInline(
        DxMask Mask,
        uint ItemPathRef,
        uint ItemNameRef,
        uint VersionRef,
        uint NameRef,
        uint DescriptionRef,
        uint ServerTypeRef,
        uint ServerUrlRef,
        int DefaultConnected,
        uint Reserved);
}

/// <summary>
/// NDR codec for <c>OpcDxConnection</c>.
/// </summary>
public static class NdrOpcDxConnectionCodec
{
    // MIDL FC_USER_MARSHAL members inside the FC_BOGUS_STRUCT use this
    // 4-byte inline marker; each wireVARIANT body is emitted in the deferred
    // phase at the member's position relative to surrounding pointer fields.
    private const uint UserMarshalReferent = 0x72657355;

    /// <summary>
    /// Writes a DX connection.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxConnection value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteInline(ref writer, value);
        WriteDeferred(ref writer, value);
    }

    /// <summary>
    /// Reads a DX connection.
    /// </summary>
    public static DxConnection Read(ref NdrReader reader)
    {
        ConnectionInline inline = ReadInline(ref reader);
        return ApplyDeferred(ref reader, inline);
    }

    private static bool Has(DxMask mask, DxMask bit) => (mask & bit) != DxMask.None;

    internal static void WriteConformantArrayBody(ref NdrWriter writer, DxConnection[] values)
    {
        foreach (DxConnection value in values) { WriteInline(ref writer, value); }
        foreach (DxConnection value in values) { WriteDeferred(ref writer, value); }
    }

    internal static DxConnection[] ReadConformantArrayBody(ref NdrReader reader, int count)
    {
        var inline = new ConnectionInline[count];
        for (int i = 0; i < count; i++) { inline[i] = ReadInline(ref reader); }
        var values = new DxConnection[count];
        for (int i = 0; i < count; i++) { values[i] = ApplyDeferred(ref reader, inline[i]); }
        return values;
    }

    private static void WriteInline(ref NdrWriter writer, DxConnection value)
    {
        string[] browsePaths = value.BrowsePaths;
        writer.WriteUInt32(unchecked((uint)value.Mask));
        writer.WriteUniquePointerReferent(value.ItemPath is not null);
        writer.WriteUniquePointerReferent(value.ItemName is not null);
        writer.WriteUniquePointerReferent(value.Version is not null);
        writer.WriteUInt32(checked((uint)browsePaths.Length));
        writer.WriteUniquePointerReferent(browsePaths.Length > 0);
        writer.WriteUniquePointerReferent(value.Name is not null);
        writer.WriteUniquePointerReferent(value.Description is not null);
        writer.WriteUniquePointerReferent(value.Keyword is not null);
        writer.WriteInt32(value.DefaultSourceItemConnected == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteInt32(value.DefaultTargetItemConnected == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteInt32(value.DefaultOverridden == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteUInt32(UserMarshalReferent);
        writer.WriteUInt32(UserMarshalReferent);
        writer.WriteInt32(value.EnableSubstituteValue == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteUniquePointerReferent(value.TargetItemPath is not null);
        writer.WriteUniquePointerReferent(value.TargetItemName is not null);
        writer.WriteUniquePointerReferent(value.SourceServerName is not null);
        writer.WriteUniquePointerReferent(value.SourceItemPath is not null);
        writer.WriteUniquePointerReferent(value.SourceItemName is not null);
        writer.WriteUInt32(unchecked((uint)(value.SourceItemQueueSize ?? 0)));
        writer.WriteUInt32(unchecked((uint)(value.UpdateRateMilliseconds ?? 0)));
        writer.WriteSingle(value.DeadbandPercent ?? 0);
        writer.WriteUniquePointerReferent(value.VendorData is not null);
    }

    private static void WriteDeferred(ref NdrWriter writer, DxConnection value)
    {
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemPath);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.ItemName);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Version);
        if (value.BrowsePaths.Length > 0)
        {
            NdrOpcDxStringArrayCodec.Write(ref writer, value.BrowsePaths);
        }
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Name);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Description);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.Keyword);
        writer.WriteVariant(value.DefaultOverrideValue ?? OpcVariant.Empty);
        writer.WriteVariant(value.SubstituteValue ?? OpcVariant.Empty);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.TargetItemPath);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.TargetItemName);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.SourceServerName);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.SourceItemPath);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.SourceItemName);
        NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value.VendorData);
    }

    private static ConnectionInline ReadInline(ref NdrReader reader) =>
        new(
            (DxMask)reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            NdrOpcDxCodecHelpers.ReadCount(ref reader, "browse path count"),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadSingle(),
            reader.ReadUInt32());

    private static DxConnection ApplyDeferred(ref NdrReader reader, ConnectionInline inline)
    {
        string? itemPath = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemPathRef);
        string? itemName = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.ItemNameRef);
        string? version = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.VersionRef);
        string[] browsePaths = inline.BrowsePathsRef == 0u
            ? Array.Empty<string>()
            : NdrOpcDxStringArrayCodec.Read(ref reader, inline.BrowsePathCount);
        if (inline.BrowsePathsRef == 0u && inline.BrowsePathCount != 0)
        {
            throw new InvalidDataException("OPC DX connection has a null browse-path pointer with a non-zero count.");
        }
        string? name = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.NameRef);
        string? description = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.DescriptionRef);
        string? keyword = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.KeywordRef);
        OpcVariant defaultOverrideValue = ReadDeferredVariant(ref reader, inline.DefaultOverrideValueRef);
        OpcVariant substituteValue = ReadDeferredVariant(ref reader, inline.SubstituteValueRef);
        string? targetItemPath = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.TargetItemPathRef);
        string? targetItemName = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.TargetItemNameRef);
        string? sourceServerName = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.SourceServerNameRef);
        string? sourceItemPath = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.SourceItemPathRef);
        string? sourceItemName = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.SourceItemNameRef);
        string? vendorData = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, inline.VendorDataRef);

        return new DxConnection(
            name: Has(inline.Mask, DxMask.Name) ? name : null,
            description: Has(inline.Mask, DxMask.Description) ? description : null,
            itemPath: Has(inline.Mask, DxMask.ItemPath) ? itemPath : null,
            itemName: Has(inline.Mask, DxMask.ItemName) ? itemName : null,
            version: Has(inline.Mask, DxMask.Version) ? version : null,
            browsePaths: Has(inline.Mask, DxMask.BrowsePaths) ? browsePaths : Array.Empty<string>(),
            keyword: Has(inline.Mask, DxMask.Keyword) ? keyword : null,
            defaultSourceItemConnected: Has(inline.Mask, DxMask.DefaultSourceItemConnected) ? inline.DefaultSourceItemConnected != 0 : null,
            defaultTargetItemConnected: Has(inline.Mask, DxMask.DefaultTargetItemConnected) ? inline.DefaultTargetItemConnected != 0 : null,
            defaultOverridden: Has(inline.Mask, DxMask.DefaultOverridden) ? inline.DefaultOverridden != 0 : null,
            defaultOverrideValue: Has(inline.Mask, DxMask.DefaultOverrideValue) ? defaultOverrideValue : null,
            substituteValue: Has(inline.Mask, DxMask.SubstituteValue) ? substituteValue : null,
            enableSubstituteValue: Has(inline.Mask, DxMask.EnableSubstituteValue) ? inline.EnableSubstituteValue != 0 : null,
            targetItemPath: Has(inline.Mask, DxMask.TargetItemPath) ? targetItemPath : null,
            targetItemName: Has(inline.Mask, DxMask.TargetItemName) ? targetItemName : null,
            sourceServerName: Has(inline.Mask, DxMask.SourceServerName) ? sourceServerName : null,
            sourceItemPath: Has(inline.Mask, DxMask.SourceItemPath) ? sourceItemPath : null,
            sourceItemName: Has(inline.Mask, DxMask.SourceItemName) ? sourceItemName : null,
            sourceItemQueueSize: Has(inline.Mask, DxMask.SourceItemQueueSize) ? unchecked((int)inline.SourceItemQueueSize) : null,
            updateRateMilliseconds: Has(inline.Mask, DxMask.UpdateRate) ? unchecked((int)inline.UpdateRate) : null,
            deadbandPercent: Has(inline.Mask, DxMask.DeadBand) ? inline.DeadBand : null,
            vendorData: Has(inline.Mask, DxMask.VendorData) ? vendorData : null,
            mask: (int)inline.Mask);
    }

    private static OpcVariant ReadDeferredVariant(ref NdrReader reader, uint referent)
    {
        if (referent != UserMarshalReferent)
        {
            throw new InvalidDataException(
                $"OPC DX connection VARIANT user-marshal referent 0x{referent:X8} is invalid.");
        }
        return reader.ReadVariant();
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct ConnectionInline(
        DxMask Mask,
        uint ItemPathRef,
        uint ItemNameRef,
        uint VersionRef,
        int BrowsePathCount,
        uint BrowsePathsRef,
        uint NameRef,
        uint DescriptionRef,
        uint KeywordRef,
        int DefaultSourceItemConnected,
        int DefaultTargetItemConnected,
        int DefaultOverridden,
        uint DefaultOverrideValueRef,
        uint SubstituteValueRef,
        int EnableSubstituteValue,
        uint TargetItemPathRef,
        uint TargetItemNameRef,
        uint SourceServerNameRef,
        uint SourceItemPathRef,
        uint SourceItemNameRef,
        uint SourceItemQueueSize,
        uint UpdateRate,
        float DeadBand,
        uint VendorDataRef);
}

/// <summary>
/// NDR codec for OPC DX <c>OPCError</c>.
/// </summary>
public static class NdrOpcDxErrorCodec
{
    /// <summary>
    /// Writes an OPC DX error.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxError value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteInt32(value.Id.Code);
        writer.WriteUnicodeStringPtr(value.Text);
    }

    /// <summary>
    /// Reads an OPC DX error.
    /// </summary>
    public static DxError Read(ref NdrReader reader)
    {
        int code = reader.ReadInt32();
        string? text = reader.ReadUnicodeStringPtr();
        return new DxError(new OpcResultId(code, text), text);
    }
}

/// <summary>
/// NDR codec for OPC DX <c>DXQuality</c>.
/// </summary>
public static class NdrOpcDxQualityCodec
{
    /// <summary>
    /// Writes DX quality.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxQuality value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUInt32((uint)value.Quality);
        writer.WriteUInt32((uint)value.LimitBits);
        writer.WriteUInt64(value.VendorBits);
    }

    /// <summary>
    /// Reads DX quality.
    /// </summary>
    public static DxQuality Read(ref NdrReader reader)
    {
        var quality = (DxQualityStatus)reader.ReadUInt32();
        var limitBits = (DxLimitStatus)reader.ReadUInt32();
        ulong vendorBits = reader.ReadUInt64();
        return new DxQuality(quality, limitBits, vendorBits);
    }
}

/// <summary>
/// NDR codec for OPC DX <c>ServerStatus</c>.
/// </summary>
public static class NdrOpcDxServerStatusCodec
{
    /// <summary>
    /// Writes server status.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxServerStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUInt32((uint)value.ServerState);
        writer.WriteUnicodeStringPtr(value.ConfigurationVersion);
        writer.WriteUInt32(value.DxConnectionCount);
        writer.WriteUInt32(value.MaxDxConnections);
        writer.WriteInt32(value.DirtyFlag ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteInt32(value.ErrorId.Code);
        writer.WriteUnicodeStringPtr(value.ErrorDiagnostic);
        NdrOpcDxStringArrayCodec.Write(ref writer, value.SourceServerTypes);
        writer.WriteUInt32(value.MaxQueueSize);
    }

    /// <summary>
    /// Reads server status.
    /// </summary>
    public static DxServerStatus Read(ref NdrReader reader)
    {
        var serverState = (DxServerState)reader.ReadUInt32();
        string? configurationVersion = reader.ReadUnicodeStringPtr();
        uint dxConnectionCount = reader.ReadUInt32();
        uint maxDxConnections = reader.ReadUInt32();
        bool dirtyFlag = reader.ReadInt32() != 0;
        int errorId = reader.ReadInt32();
        string? errorDiagnostic = reader.ReadUnicodeStringPtr();
        string[] sourceServerTypes = NdrOpcDxStringArrayCodec.Read(ref reader);
        uint maxQueueSize = reader.ReadUInt32();
        return new DxServerStatus(
            serverState,
            configurationVersion,
            dxConnectionCount,
            maxDxConnections,
            dirtyFlag,
            new OpcResultId(errorId, null),
            errorDiagnostic,
            sourceServerTypes,
            maxQueueSize);
    }
}

/// <summary>
/// NDR codec for OPC DX <c>DXConnectionStatus</c>.
/// </summary>
public static class NdrOpcDxConnectionStatusCodec
{
    /// <summary>
    /// Writes connection status.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxConnectionStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUInt32((uint)value.DxConnectionState);
        writer.WriteVariant(value.WriteValue);
        NdrOpcDxCodecHelpers.WriteFileTime(ref writer, value.WriteTimestamp);
        NdrOpcDxQualityCodec.Write(ref writer, value.WriteQuality);
        writer.WriteInt32(value.WriteErrorId.Code);
        writer.WriteUnicodeStringPtr(value.WriteErrorDiagnostic);
        writer.WriteVariant(value.SourceValue);
        NdrOpcDxCodecHelpers.WriteFileTime(ref writer, value.SourceTimestamp);
        NdrOpcDxQualityCodec.Write(ref writer, value.SourceQuality);
        writer.WriteInt32(value.SourceErrorId.Code);
        writer.WriteUnicodeStringPtr(value.SourceErrorDiagnostic);
        writer.WriteUInt32(value.ActualUpdateRate);
        writer.WriteUInt32(value.QueueHighWaterMark);
        writer.WriteUInt32(value.QueueFlushCount);
        writer.WriteInt32(value.SourceItemConnected ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteInt32(value.TargetItemConnected ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteInt32(value.Overridden ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteVariant(value.OverrideValue);
    }

    /// <summary>
    /// Reads connection status.
    /// </summary>
    public static DxConnectionStatus Read(ref NdrReader reader)
    {
        var state = (DxConnectionState)reader.ReadUInt32();
        OpcVariant writeValue = reader.ReadVariant();
        DateTimeOffset writeTimestamp = NdrOpcDxCodecHelpers.ReadFileTime(ref reader);
        DxQuality writeQuality = NdrOpcDxQualityCodec.Read(ref reader);
        int writeErrorId = reader.ReadInt32();
        string? writeErrorDiagnostic = reader.ReadUnicodeStringPtr();
        OpcVariant sourceValue = reader.ReadVariant();
        DateTimeOffset sourceTimestamp = NdrOpcDxCodecHelpers.ReadFileTime(ref reader);
        DxQuality sourceQuality = NdrOpcDxQualityCodec.Read(ref reader);
        int sourceErrorId = reader.ReadInt32();
        string? sourceErrorDiagnostic = reader.ReadUnicodeStringPtr();
        uint actualUpdateRate = reader.ReadUInt32();
        uint queueHighWaterMark = reader.ReadUInt32();
        uint queueFlushCount = reader.ReadUInt32();
        bool sourceItemConnected = reader.ReadInt32() != 0;
        bool targetItemConnected = reader.ReadInt32() != 0;
        bool overridden = reader.ReadInt32() != 0;
        OpcVariant overrideValue = reader.ReadVariant();
        return new DxConnectionStatus(
            state,
            writeValue,
            writeTimestamp,
            writeQuality,
            new OpcResultId(writeErrorId, null),
            writeErrorDiagnostic,
            sourceValue,
            sourceTimestamp,
            sourceQuality,
            new OpcResultId(sourceErrorId, null),
            sourceErrorDiagnostic,
            actualUpdateRate,
            queueHighWaterMark,
            queueFlushCount,
            sourceItemConnected,
            targetItemConnected,
            overridden,
            overrideValue);
    }
}

/// <summary>
/// NDR codec for OPC DX <c>DXSourceServerStatus</c>.
/// </summary>
public static class NdrOpcDxSourceServerStatusCodec
{
    /// <summary>
    /// Writes source-server status.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxSourceServerStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUInt32((uint)value.ConnectStatus);
        writer.WriteInt32(value.ErrorId.Code);
        writer.WriteUnicodeStringPtr(value.ErrorDiagnostic);
        NdrOpcDxCodecHelpers.WriteFileTime(ref writer, value.LastConnectTimestamp);
        NdrOpcDxCodecHelpers.WriteFileTime(ref writer, value.LastConnectFailTimestamp);
        writer.WriteUInt32(value.ConnectFailCount);
        writer.WriteUInt32(value.PingTime);
        NdrOpcDxCodecHelpers.WriteFileTime(ref writer, value.LastDataChangeTimestamp);
        writer.WriteInt32(value.SourceServerConnected ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
    }

    /// <summary>
    /// Reads source-server status.
    /// </summary>
    public static DxSourceServerStatus Read(ref NdrReader reader)
    {
        var connectStatus = (DxConnectStatus)reader.ReadUInt32();
        int errorId = reader.ReadInt32();
        string? errorDiagnostic = reader.ReadUnicodeStringPtr();
        DateTimeOffset lastConnectTimestamp = NdrOpcDxCodecHelpers.ReadFileTime(ref reader);
        DateTimeOffset lastConnectFailTimestamp = NdrOpcDxCodecHelpers.ReadFileTime(ref reader);
        uint connectFailCount = reader.ReadUInt32();
        uint pingTime = reader.ReadUInt32();
        DateTimeOffset lastDataChangeTimestamp = NdrOpcDxCodecHelpers.ReadFileTime(ref reader);
        bool connected = reader.ReadInt32() != 0;
        return new DxSourceServerStatus(
            connectStatus,
            new OpcResultId(errorId, null),
            errorDiagnostic,
            lastConnectTimestamp,
            lastConnectFailTimestamp,
            connectFailCount,
            pingTime,
            lastDataChangeTimestamp,
            connected);
    }
}

/// <summary>
/// NDR codec for counted LPWSTR arrays used by OPC DX structures.
/// </summary>
public static class NdrOpcDxStringArrayCodec
{
    /// <summary>
    /// Writes a counted string array.
    /// </summary>
    public static void Write(ref NdrWriter writer, string?[]? values)
    {
        values ??= Array.Empty<string?>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (var value in values)
        {
            writer.WriteUniquePointerReferent(value is not null);
        }
        foreach (var value in values)
        {
            NdrOpcDxCodecHelpers.WriteDeferredString(ref writer, value);
        }
    }

    /// <summary>
    /// Reads a counted string array.
    /// </summary>
    public static string[] Read(ref NdrReader reader) =>
        Read(ref reader, expectedCount: null);

    internal static string[] Read(ref NdrReader reader, int expectedCount) =>
        Read(ref reader, (int?)expectedCount);

    private static string[] Read(ref NdrReader reader, int? expectedCount)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "string array");
        NdrOpcDxCodecHelpers.ValidateExpectedCount(count, expectedCount, "string array");
        var referents = new uint[count];
        for (int i = 0; i < count; i++)
        {
            referents[i] = reader.ReadUInt32();
        }
        var values = new string[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = NdrOpcDxCodecHelpers.ReadDeferredString(ref reader, referents[i]) ?? string.Empty;
        }

        return values;
    }
}

/// <summary>
/// NDR codec for counted HRESULT arrays.
/// </summary>
public static class NdrOpcDxInt32ArrayCodec
{
    /// <summary>
    /// Writes a counted HRESULT array.
    /// </summary>
    public static void Write(ref NdrWriter writer, int[]? values)
    {
        values ??= Array.Empty<int>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    /// <summary>
    /// Reads a counted HRESULT array.
    /// </summary>
    public static int[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "HRESULT array");
        var values = new int[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = reader.ReadInt32();
        }

        return values;
    }

    internal static void WriteUnique(ref NdrWriter writer, int[]? values)
    {
        values ??= Array.Empty<int>();
        writer.WriteUniquePointerReferent(values.Length > 0);
        if (values.Length > 0)
        {
            Write(ref writer, values);
        }
    }

    internal static int[] ReadUnique(ref NdrReader reader, int? expectedCount = null)
    {
        if (!reader.TryReadReferentId(out _))
        {
            NdrOpcDxCodecHelpers.ValidateExpectedCount(0, expectedCount, "HRESULT array");
            return [];
        }
        int[] values = Read(ref reader);
        NdrOpcDxCodecHelpers.ValidateExpectedCount(values.Length, expectedCount, "HRESULT array");
        return values;
    }
}

/// <summary>
/// NDR codec for counted <see cref="DxItemIdentifier" /> arrays.
/// </summary>
public static class NdrOpcDxItemIdentifierArrayCodec
{
    /// <summary>
    /// Writes item identifiers.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxItemIdentifier[]? values)
    {
        values ??= Array.Empty<DxItemIdentifier>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        NdrOpcDxItemIdentifierCodec.WriteConformantArrayBody(ref writer, values);
    }

    /// <summary>
    /// Reads item identifiers.
    /// </summary>
    public static DxItemIdentifier[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "item identifier array");
        return NdrOpcDxItemIdentifierCodec.ReadConformantArrayBody(ref reader, count);
    }
}

/// <summary>
/// NDR codec for counted <see cref="DxIdentifiedResult" /> arrays.
/// </summary>
public static class NdrOpcDxIdentifiedResultArrayCodec
{
    /// <summary>
    /// Writes identified results.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxIdentifiedResult[]? values)
    {
        values ??= Array.Empty<DxIdentifiedResult>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        NdrOpcDxIdentifiedResultCodec.WriteConformantArrayBody(ref writer, values);
    }

    /// <summary>
    /// Reads identified results.
    /// </summary>
    public static DxIdentifiedResult[] Read(ref NdrReader reader) =>
        Read(ref reader, expectedCount: null);

    internal static DxIdentifiedResult[] Read(ref NdrReader reader, int expectedCount) =>
        Read(ref reader, (int?)expectedCount);

    private static DxIdentifiedResult[] Read(ref NdrReader reader, int? expectedCount)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "identified result array");
        NdrOpcDxCodecHelpers.ValidateExpectedCount(count, expectedCount, "identified result array");
        return NdrOpcDxIdentifiedResultCodec.ReadConformantArrayBody(ref reader, count);
    }
}

/// <summary>
/// NDR codec for counted <see cref="DxSourceServer" /> arrays.
/// </summary>
public static class NdrOpcDxSourceServerArrayCodec
{
    /// <summary>
    /// Writes source servers.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxSourceServer[]? values)
    {
        values ??= Array.Empty<DxSourceServer>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        NdrOpcDxSourceServerCodec.WriteConformantArrayBody(ref writer, values);
    }

    /// <summary>
    /// Reads source servers.
    /// </summary>
    public static DxSourceServer[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "source server array");
        return NdrOpcDxSourceServerCodec.ReadConformantArrayBody(ref reader, count);
    }

    public static void WriteUnique(ref NdrWriter writer, DxSourceServer[]? values)
    {
        values ??= Array.Empty<DxSourceServer>();
        writer.WriteUniquePointerReferent(values.Length > 0);
        if (values.Length > 0)
        {
            Write(ref writer, values);
        }
    }

    public static DxSourceServer[] ReadUnique(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        return Read(ref reader);
    }
}

/// <summary>
/// NDR codec for counted <see cref="DxConnection" /> arrays.
/// </summary>
public static class NdrOpcDxConnectionArrayCodec
{
    /// <summary>
    /// Writes DX connections.
    /// </summary>
    public static void Write(ref NdrWriter writer, DxConnection[]? values)
    {
        values ??= Array.Empty<DxConnection>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        NdrOpcDxConnectionCodec.WriteConformantArrayBody(ref writer, values);
    }

    /// <summary>
    /// Reads DX connections.
    /// </summary>
    public static DxConnection[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "DX connection array");
        return NdrOpcDxConnectionCodec.ReadConformantArrayBody(ref reader, count);
    }

    public static void WriteUnique(ref NdrWriter writer, DxConnection[]? values)
    {
        values ??= Array.Empty<DxConnection>();
        writer.WriteUniquePointerReferent(values.Length > 0);
        if (values.Length > 0)
        {
            Write(ref writer, values);
        }
    }

    public static DxConnection[] ReadUnique(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        return Read(ref reader);
    }
}

/// <summary>
/// NDR codec for the compound <see cref="DxConnectionQueryResult" /> response.
/// </summary>
public static class NdrOpcDxConnectionQueryResultCodec
{
    /// <summary>Writes mask errors followed by matching DX connections.</summary>
    public static void Write(ref NdrWriter writer, DxConnectionQueryResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        NdrOpcDxInt32ArrayCodec.WriteUnique(ref writer, value.Errors);
        writer.WriteUInt32(checked((uint)value.Connections.Length));
        NdrOpcDxConnectionArrayCodec.WriteUnique(ref writer, value.Connections);
    }

    /// <summary>Reads mask errors followed by matching DX connections.</summary>
    public static DxConnectionQueryResult Read(ref NdrReader reader) =>
        new(
            NdrOpcDxInt32ArrayCodec.ReadUnique(ref reader),
            ReadConnections(ref reader));

    private static DxConnection[] ReadConnections(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "QueryDXConnections output count");
        DxConnection[] connections = NdrOpcDxConnectionArrayCodec.ReadUnique(ref reader);
        NdrOpcDxCodecHelpers.ValidateExpectedCount(connections.Length, count, "QueryDXConnections output array");
        return connections;
    }
}

/// <summary>
/// NDR codec for the compound <see cref="DxUpdateConnectionsResult" /> response.
/// </summary>
public static class NdrOpcDxUpdateConnectionsResultCodec
{
    /// <summary>Writes mask errors followed by the general response.</summary>
    public static void Write(ref NdrWriter writer, DxUpdateConnectionsResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        NdrOpcDxInt32ArrayCodec.WriteUnique(ref writer, value.Errors);
        NdrOpcDxGeneralResponseCodec.Write(ref writer, value.Response);
    }

    /// <summary>Reads mask errors followed by the general response.</summary>
    public static DxUpdateConnectionsResult Read(ref NdrReader reader) =>
        new(
            NdrOpcDxInt32ArrayCodec.ReadUnique(ref reader),
            NdrOpcDxGeneralResponseCodec.Read(ref reader));
}

/// <summary>
/// NDR codec for the compound <see cref="DxDeleteConnectionsResult" /> response.
/// </summary>
public static class NdrOpcDxDeleteConnectionsResultCodec
{
    /// <summary>Writes mask errors followed by the general response.</summary>
    public static void Write(ref NdrWriter writer, DxDeleteConnectionsResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        NdrOpcDxInt32ArrayCodec.WriteUnique(ref writer, value.MaskErrors);
        NdrOpcDxGeneralResponseCodec.Write(ref writer, value.Response);
    }

    /// <summary>Reads mask errors followed by the general response.</summary>
    public static DxDeleteConnectionsResult Read(ref NdrReader reader) =>
        new(
            NdrOpcDxInt32ArrayCodec.ReadUnique(ref reader),
            NdrOpcDxGeneralResponseCodec.Read(ref reader));
}

internal static class NdrOpcDxCodecHelpers
{
    internal const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    internal static int ReadCount(ref NdrReader reader, string description)
    {
        uint count = reader.ReadUInt32();
        if (count > int.MaxValue)
        {
            throw new InvalidDataException($"OPC DX {description} count {count} is too large.");
        }

        return (int)count;
    }

    internal static void ValidateExpectedCount(int actual, int? expected, string description)
    {
        if (expected.HasValue && actual != expected.Value)
        {
            throw new InvalidDataException(
                $"OPC DX {description} count {actual} does not match the correlated count {expected.Value}.");
        }
    }

    internal static void WriteDeferredString(ref NdrWriter writer, string? value)
    {
        if (value is not null)
        {
            writer.WriteUnicodeString(value);
        }
    }

    internal static string? ReadDeferredString(ref NdrReader reader, uint referent) =>
        referent == 0u ? null : reader.ReadUnicodeString();

    internal static void WriteFileTime(ref NdrWriter writer, DateTimeOffset value) =>
        writer.WriteFileTime(value.UtcTicks - FileTimeEpochOffsetTicks);

    internal static DateTimeOffset ReadFileTime(ref NdrReader reader)
    {
        long fileTimeTicks = reader.ReadFileTime();
        return new DateTimeOffset(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
    }
}
