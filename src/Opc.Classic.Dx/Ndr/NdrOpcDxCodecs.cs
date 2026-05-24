//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.IO;
using Opc.Classic.Ndr;

#pragma warning disable MA0048 // The DX codec table is intentionally grouped for spec readability.

namespace Opc.Classic.Dx.Ndr;

/// <summary>Registry of OPC DX structure codecs enabled for generated and hand-written proxies.</summary>
public static class NdrOpcDxCodecRegistry
{
    /// <summary>The 16 OPC DX codec entries covered by the DX 1.00 configuration and status structures.</summary>
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

/// <summary>NDR codec for <c>OpcDxItemIdentifier</c>.</summary>
public static class NdrOpcDxItemIdentifierCodec
{
    /// <summary>Writes an item identifier.</summary>
    public static void Write(ref NdrWriter writer, DxItemIdentifier value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUnicodeStringPtr(value.ItemPath);
        writer.WriteUnicodeStringPtr(value.ItemName);
        writer.WriteUnicodeStringPtr(value.Version);
        writer.WriteUInt32(unchecked((uint)value.Reserved));
    }

    /// <summary>Reads an item identifier.</summary>
    public static DxItemIdentifier Read(ref NdrReader reader)
    {
        string? itemPath = reader.ReadUnicodeStringPtr();
        string? itemName = reader.ReadUnicodeStringPtr();
        string? version = reader.ReadUnicodeStringPtr();
        int reserved = unchecked((int)reader.ReadUInt32());
        return new DxItemIdentifier(itemPath, itemName, version, reserved);
    }
}

/// <summary>NDR codec for <c>OpcDxIdentifiedResult</c>.</summary>
public static class NdrOpcDxIdentifiedResultCodec
{
    /// <summary>Writes an identified result.</summary>
    public static void Write(ref NdrWriter writer, DxIdentifiedResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUnicodeStringPtr(value.ItemPath);
        writer.WriteUnicodeStringPtr(value.ItemName);
        writer.WriteUnicodeStringPtr(value.Version);
        writer.WriteInt32(value.ResultId.Code);
    }

    /// <summary>Reads an identified result.</summary>
    public static DxIdentifiedResult Read(ref NdrReader reader)
    {
        string? itemPath = reader.ReadUnicodeStringPtr();
        string? itemName = reader.ReadUnicodeStringPtr();
        string? version = reader.ReadUnicodeStringPtr();
        int resultCode = reader.ReadInt32();
        return new DxIdentifiedResult(itemPath, itemName, version, new OpcResultId(resultCode, null));
    }
}

/// <summary>NDR codec for <c>OpcDxGeneralResponse</c>.</summary>
public static class NdrOpcDxGeneralResponseCodec
{
    /// <summary>Writes a general response.</summary>
    public static void Write(ref NdrWriter writer, DxGeneralResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUnicodeStringPtr(value.ConfigurationVersion);
        NdrOpcDxIdentifiedResultArrayCodec.Write(ref writer, value.IdentifiedResults);
        writer.WriteUInt32(unchecked((uint)value.Reserved));
    }

    /// <summary>Reads a general response.</summary>
    public static DxGeneralResponse Read(ref NdrReader reader)
    {
        string? configurationVersion = reader.ReadUnicodeStringPtr();
        DxIdentifiedResult[] results = NdrOpcDxIdentifiedResultArrayCodec.Read(ref reader);
        int reserved = unchecked((int)reader.ReadUInt32());
        return new DxGeneralResponse(configurationVersion, results, reserved);
    }
}

/// <summary>NDR codec for <c>OpcDxSourceServer</c>.</summary>
public static class NdrOpcDxSourceServerCodec
{
    /// <summary>Writes a source server.</summary>
    public static void Write(ref NdrWriter writer, DxSourceServer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUInt32(unchecked((uint)value.Mask));
        writer.WriteUnicodeStringPtr(value.ItemPath);
        writer.WriteUnicodeStringPtr(value.ItemName);
        writer.WriteUnicodeStringPtr(value.Version);
        writer.WriteUnicodeStringPtr(value.Name);
        writer.WriteUnicodeStringPtr(value.Description);
        writer.WriteUnicodeStringPtr(value.ServerType);
        writer.WriteUnicodeStringPtr(value.ServerUrl);
        writer.WriteInt32(value.DefaultConnected == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteUInt32(unchecked((uint)value.Reserved));
    }

    /// <summary>Reads a source server.</summary>
    public static DxSourceServer Read(ref NdrReader reader)
    {
        var mask = (DxMask)reader.ReadUInt32();
        string? itemPath = reader.ReadUnicodeStringPtr();
        string? itemName = reader.ReadUnicodeStringPtr();
        string? version = reader.ReadUnicodeStringPtr();
        string? name = reader.ReadUnicodeStringPtr();
        string? description = reader.ReadUnicodeStringPtr();
        string? serverType = reader.ReadUnicodeStringPtr();
        string? serverUrl = reader.ReadUnicodeStringPtr();
        bool defaultConnectedRaw = reader.ReadInt32() != 0;
        int reserved = unchecked((int)reader.ReadUInt32());

        return new DxSourceServer(
            name: Has(mask, DxMask.Name) ? name : null,
            serverUrl: Has(mask, DxMask.ServerUrl) ? serverUrl : null,
            description: Has(mask, DxMask.Description) ? description : null,
            serverType: Has(mask, DxMask.ServerType) ? serverType : null,
            itemPath: Has(mask, DxMask.ItemPath) ? itemPath : null,
            itemName: Has(mask, DxMask.ItemName) ? itemName : null,
            version: Has(mask, DxMask.Version) ? version : null,
            defaultConnected: Has(mask, DxMask.DefaultSourceServerConnected) ? defaultConnectedRaw : null,
            mask: (int)mask,
            reserved: reserved);
    }

    private static bool Has(DxMask mask, DxMask bit) => (mask & bit) != DxMask.None;
}

/// <summary>NDR codec for <c>OpcDxConnection</c>.</summary>
public static class NdrOpcDxConnectionCodec
{
    /// <summary>Writes a DX connection.</summary>
    public static void Write(ref NdrWriter writer, DxConnection value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUInt32(unchecked((uint)value.Mask));
        writer.WriteUnicodeStringPtr(value.ItemPath);
        writer.WriteUnicodeStringPtr(value.ItemName);
        writer.WriteUnicodeStringPtr(value.Version);
        NdrOpcDxStringArrayCodec.Write(ref writer, value.BrowsePaths);
        writer.WriteUnicodeStringPtr(value.Name);
        writer.WriteUnicodeStringPtr(value.Description);
        writer.WriteUnicodeStringPtr(value.Keyword);
        writer.WriteInt32(value.DefaultSourceItemConnected == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteInt32(value.DefaultTargetItemConnected == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteInt32(value.DefaultOverridden == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteVariant(value.DefaultOverrideValue ?? OpcVariant.Empty);
        writer.WriteVariant(value.SubstituteValue ?? OpcVariant.Empty);
        writer.WriteInt32(value.EnableSubstituteValue == true ? NdrOpcDxCodecHelpers.Win32BoolTrue : 0);
        writer.WriteUnicodeStringPtr(value.TargetItemPath);
        writer.WriteUnicodeStringPtr(value.TargetItemName);
        writer.WriteUnicodeStringPtr(value.SourceServerName);
        writer.WriteUnicodeStringPtr(value.SourceItemPath);
        writer.WriteUnicodeStringPtr(value.SourceItemName);
        writer.WriteUInt32(unchecked((uint)(value.SourceItemQueueSize ?? 0)));
        writer.WriteUInt32(unchecked((uint)(value.UpdateRateMilliseconds ?? 0)));
        writer.WriteSingle(value.DeadbandPercent ?? 0);
        writer.WriteUnicodeStringPtr(value.VendorData);
    }

    /// <summary>Reads a DX connection.</summary>
    public static DxConnection Read(ref NdrReader reader)
    {
        var mask = (DxMask)reader.ReadUInt32();
        string? itemPath = reader.ReadUnicodeStringPtr();
        string? itemName = reader.ReadUnicodeStringPtr();
        string? version = reader.ReadUnicodeStringPtr();
        string[] browsePaths = NdrOpcDxStringArrayCodec.Read(ref reader);
        string? name = reader.ReadUnicodeStringPtr();
        string? description = reader.ReadUnicodeStringPtr();
        string? keyword = reader.ReadUnicodeStringPtr();
        bool defaultSourceItemConnectedRaw = reader.ReadInt32() != 0;
        bool defaultTargetItemConnectedRaw = reader.ReadInt32() != 0;
        bool defaultOverriddenRaw = reader.ReadInt32() != 0;
        OpcVariant defaultOverrideValue = reader.ReadVariant();
        OpcVariant substituteValue = reader.ReadVariant();
        bool enableSubstituteValueRaw = reader.ReadInt32() != 0;
        string? targetItemPath = reader.ReadUnicodeStringPtr();
        string? targetItemName = reader.ReadUnicodeStringPtr();
        string? sourceServerName = reader.ReadUnicodeStringPtr();
        string? sourceItemPath = reader.ReadUnicodeStringPtr();
        string? sourceItemName = reader.ReadUnicodeStringPtr();
        int sourceItemQueueSize = unchecked((int)reader.ReadUInt32());
        int updateRateMilliseconds = unchecked((int)reader.ReadUInt32());
        float deadbandPercent = reader.ReadSingle();
        string? vendorData = reader.ReadUnicodeStringPtr();

        return new DxConnection(
            name: Has(mask, DxMask.Name) ? name : null,
            description: Has(mask, DxMask.Description) ? description : null,
            itemPath: Has(mask, DxMask.ItemPath) ? itemPath : null,
            itemName: Has(mask, DxMask.ItemName) ? itemName : null,
            version: Has(mask, DxMask.Version) ? version : null,
            browsePaths: Has(mask, DxMask.BrowsePaths) ? browsePaths : Array.Empty<string>(),
            keyword: Has(mask, DxMask.Keyword) ? keyword : null,
            defaultSourceItemConnected: Has(mask, DxMask.DefaultSourceItemConnected) ? defaultSourceItemConnectedRaw : null,
            defaultTargetItemConnected: Has(mask, DxMask.DefaultTargetItemConnected) ? defaultTargetItemConnectedRaw : null,
            defaultOverridden: Has(mask, DxMask.DefaultOverridden) ? defaultOverriddenRaw : null,
            defaultOverrideValue: Has(mask, DxMask.DefaultOverrideValue) ? defaultOverrideValue : null,
            substituteValue: Has(mask, DxMask.SubstituteValue) ? substituteValue : null,
            enableSubstituteValue: Has(mask, DxMask.EnableSubstituteValue) ? enableSubstituteValueRaw : null,
            targetItemPath: Has(mask, DxMask.TargetItemPath) ? targetItemPath : null,
            targetItemName: Has(mask, DxMask.TargetItemName) ? targetItemName : null,
            sourceServerName: Has(mask, DxMask.SourceServerName) ? sourceServerName : null,
            sourceItemPath: Has(mask, DxMask.SourceItemPath) ? sourceItemPath : null,
            sourceItemName: Has(mask, DxMask.SourceItemName) ? sourceItemName : null,
            sourceItemQueueSize: Has(mask, DxMask.SourceItemQueueSize) ? sourceItemQueueSize : null,
            updateRateMilliseconds: Has(mask, DxMask.UpdateRate) ? updateRateMilliseconds : null,
            deadbandPercent: Has(mask, DxMask.DeadBand) ? deadbandPercent : null,
            vendorData: Has(mask, DxMask.VendorData) ? vendorData : null,
            mask: (int)mask);
    }

    private static bool Has(DxMask mask, DxMask bit) => (mask & bit) != DxMask.None;
}

/// <summary>NDR codec for OPC DX <c>OPCError</c>.</summary>
public static class NdrOpcDxErrorCodec
{
    /// <summary>Writes an OPC DX error.</summary>
    public static void Write(ref NdrWriter writer, DxError value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteInt32(value.Id.Code);
        writer.WriteUnicodeStringPtr(value.Text);
    }

    /// <summary>Reads an OPC DX error.</summary>
    public static DxError Read(ref NdrReader reader)
    {
        int code = reader.ReadInt32();
        string? text = reader.ReadUnicodeStringPtr();
        return new DxError(new OpcResultId(code, text), text);
    }
}

/// <summary>NDR codec for OPC DX <c>DXQuality</c>.</summary>
public static class NdrOpcDxQualityCodec
{
    /// <summary>Writes DX quality.</summary>
    public static void Write(ref NdrWriter writer, DxQuality value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteUInt32((uint)value.Quality);
        writer.WriteUInt32((uint)value.LimitBits);
        writer.WriteUInt64(value.VendorBits);
    }

    /// <summary>Reads DX quality.</summary>
    public static DxQuality Read(ref NdrReader reader)
    {
        var quality = (DxQualityStatus)reader.ReadUInt32();
        var limitBits = (DxLimitStatus)reader.ReadUInt32();
        ulong vendorBits = reader.ReadUInt64();
        return new DxQuality(quality, limitBits, vendorBits);
    }
}

/// <summary>NDR codec for OPC DX <c>ServerStatus</c>.</summary>
public static class NdrOpcDxServerStatusCodec
{
    /// <summary>Writes server status.</summary>
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

    /// <summary>Reads server status.</summary>
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

/// <summary>NDR codec for OPC DX <c>DXConnectionStatus</c>.</summary>
public static class NdrOpcDxConnectionStatusCodec
{
    /// <summary>Writes connection status.</summary>
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

    /// <summary>Reads connection status.</summary>
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

/// <summary>NDR codec for OPC DX <c>DXSourceServerStatus</c>.</summary>
public static class NdrOpcDxSourceServerStatusCodec
{
    /// <summary>Writes source-server status.</summary>
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

    /// <summary>Reads source-server status.</summary>
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

/// <summary>NDR codec for counted LPWSTR arrays used by OPC DX structures.</summary>
public static class NdrOpcDxStringArrayCodec
{
    /// <summary>Writes a counted string array.</summary>
    public static void Write(ref NdrWriter writer, string?[]? values)
    {
        values ??= Array.Empty<string?>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (var value in values)
        {
            writer.WriteUnicodeStringPtr(value);
        }
    }

    /// <summary>Reads a counted string array.</summary>
    public static string[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "string array");
        var values = new string[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = reader.ReadUnicodeStringPtr() ?? string.Empty;
        }

        return values;
    }
}

/// <summary>NDR codec for counted HRESULT arrays.</summary>
public static class NdrOpcDxInt32ArrayCodec
{
    /// <summary>Writes a counted HRESULT array.</summary>
    public static void Write(ref NdrWriter writer, int[]? values)
    {
        values ??= Array.Empty<int>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    /// <summary>Reads a counted HRESULT array.</summary>
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
}

/// <summary>NDR codec for counted <see cref="DxItemIdentifier" /> arrays.</summary>
public static class NdrOpcDxItemIdentifierArrayCodec
{
    /// <summary>Writes item identifiers.</summary>
    public static void Write(ref NdrWriter writer, DxItemIdentifier[]? values)
    {
        values ??= Array.Empty<DxItemIdentifier>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (var value in values)
        {
            NdrOpcDxItemIdentifierCodec.Write(ref writer, value);
        }
    }

    /// <summary>Reads item identifiers.</summary>
    public static DxItemIdentifier[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "item identifier array");
        var values = new DxItemIdentifier[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = NdrOpcDxItemIdentifierCodec.Read(ref reader);
        }

        return values;
    }
}

/// <summary>NDR codec for counted <see cref="DxIdentifiedResult" /> arrays.</summary>
public static class NdrOpcDxIdentifiedResultArrayCodec
{
    /// <summary>Writes identified results.</summary>
    public static void Write(ref NdrWriter writer, DxIdentifiedResult[]? values)
    {
        values ??= Array.Empty<DxIdentifiedResult>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (var value in values)
        {
            NdrOpcDxIdentifiedResultCodec.Write(ref writer, value);
        }
    }

    /// <summary>Reads identified results.</summary>
    public static DxIdentifiedResult[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "identified result array");
        var values = new DxIdentifiedResult[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = NdrOpcDxIdentifiedResultCodec.Read(ref reader);
        }

        return values;
    }
}

/// <summary>NDR codec for counted <see cref="DxSourceServer" /> arrays.</summary>
public static class NdrOpcDxSourceServerArrayCodec
{
    /// <summary>Writes source servers.</summary>
    public static void Write(ref NdrWriter writer, DxSourceServer[]? values)
    {
        values ??= Array.Empty<DxSourceServer>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (var value in values)
        {
            NdrOpcDxSourceServerCodec.Write(ref writer, value);
        }
    }

    /// <summary>Reads source servers.</summary>
    public static DxSourceServer[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "source server array");
        var values = new DxSourceServer[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = NdrOpcDxSourceServerCodec.Read(ref reader);
        }

        return values;
    }
}

/// <summary>NDR codec for counted <see cref="DxConnection" /> arrays.</summary>
public static class NdrOpcDxConnectionArrayCodec
{
    /// <summary>Writes DX connections.</summary>
    public static void Write(ref NdrWriter writer, DxConnection[]? values)
    {
        values ??= Array.Empty<DxConnection>();
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (var value in values)
        {
            NdrOpcDxConnectionCodec.Write(ref writer, value);
        }
    }

    /// <summary>Reads DX connections.</summary>
    public static DxConnection[] Read(ref NdrReader reader)
    {
        int count = NdrOpcDxCodecHelpers.ReadCount(ref reader, "DX connection array");
        var values = new DxConnection[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = NdrOpcDxConnectionCodec.Read(ref reader);
        }

        return values;
    }
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

    internal static void WriteFileTime(ref NdrWriter writer, DateTimeOffset value) =>
        writer.WriteFileTime(value.UtcTicks - FileTimeEpochOffsetTicks);

    internal static DateTimeOffset ReadFileTime(ref NdrReader reader)
    {
        long fileTimeTicks = reader.ReadFileTime();
        return new DateTimeOffset(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
    }
}
