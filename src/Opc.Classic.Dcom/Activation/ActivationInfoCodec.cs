//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Ndr;

namespace SharpInterop.Core;

/// <summary>
/// NDR codec for the managed IRemoteSCMActivator v5.6 activation property array.
/// </summary>
public static class ActivationInfoCodec {
    private const uint Signature = 0x36504143; // "CAP6" little-endian marker for this managed shadow.
    private const int InitialBufferSize = 4096;

    private delegate void NdrWriteCallback(ref NdrWriter writer);

    /// <summary>Encodes activation properties into a versioned NDR property array.</summary>
    public static byte[] Encode(ActivationProperties properties) {
        ArgumentNullException.ThrowIfNull(properties);

        var payloads = new List<(ActivationPropertyId Id, byte[] Payload)>
        {
            (ActivationPropertyId.SpecialProperties, EncodeSpecialProperties(properties.SpecialProperties)),
        };

        if (properties.InstanceInfo is not null) {
            payloads.Add((ActivationPropertyId.InstanceInfo, EncodeInstanceInfo(properties.InstanceInfo)));
        }

        if (properties.LocationInfo is not null) {
            payloads.Add((ActivationPropertyId.LocationInfo, EncodeLocationInfo(properties.LocationInfo)));
        }

        if (properties.ScmReplyInfo is not null) {
            payloads.Add((ActivationPropertyId.ScmReplyInfo, EncodeScmReplyInfo(properties.ScmReplyInfo)));
        }

        if (properties.SecurityInfo is not null) {
            payloads.Add((ActivationPropertyId.SecurityInfo, EncodeSecurityInfo(properties.SecurityInfo)));
        }

        foreach (ActivationProperty property in properties.CustomProperties) {
            payloads.Add((property.Id, property.Payload));
        }

        int capacity = 16;
        foreach ((_, byte[] payload) in payloads) {
            capacity += 8 + payload.Length + Padding(payload.Length, 4);
        }

        var buffer = new byte[Math.Max(capacity, InitialBufferSize)];
        var writer = new NdrWriter(buffer);
        writer.WriteUInt32(Signature);
        writer.WriteUInt16(5);
        writer.WriteUInt16(6);
        writer.WriteUInt32((uint)payloads.Count);

        foreach ((ActivationPropertyId id, byte[] payload) in payloads) {
            writer.WriteUInt32((uint)id);
            writer.WriteUInt32((uint)payload.Length);
            writer.WriteRawBytes(payload);
            writer.AlignTo(4);
        }

        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    /// <summary>Decodes activation properties from a versioned NDR property array.</summary>
    public static ActivationProperties Decode(ReadOnlySpan<byte> payload) {
        if (payload.IsEmpty) {
            return ActivationProperties.Empty;
        }

        var reader = new NdrReader(payload);
        uint signature = reader.ReadUInt32();
        if (signature != Signature) {
            return new ActivationProperties(
                SpecialPropertiesData.Empty,
                null,
                null,
                null,
                null,
                new[] { new ActivationProperty(0, payload) });
        }

        ushort major = reader.ReadUInt16();
        ushort minor = reader.ReadUInt16();
        uint count = reader.ReadUInt32();
        if (count > 1024) {
            throw new InvalidOperationException("Activation property count is unreasonably large.");
        }

        SpecialPropertiesData? specialProperties = null;
        InstanceInfo? instanceInfo = null;
        LocationInfo? locationInfo = null;
        ScmReplyInfo? scmReplyInfo = null;
        SecurityInfo? securityInfo = null;
        var customProperties = new List<ActivationProperty>();

        for (uint i = 0; i < count; i++) {
            var id = (ActivationPropertyId)reader.ReadUInt32();
            uint length = reader.ReadUInt32();
            if (length > int.MaxValue || length > reader.RemainingBytes) {
                throw new InvalidOperationException("Activation property payload length exceeds the remaining data.");
            }

            ReadOnlySpan<byte> propertyPayload = reader.ReadRawBytes((int)length);
            reader.AlignTo(4);

            switch (id) {
                case ActivationPropertyId.SpecialProperties:
                    specialProperties = DecodeSpecialProperties(propertyPayload, major, minor);
                    break;
                case ActivationPropertyId.InstanceInfo:
                    instanceInfo = DecodeInstanceInfo(propertyPayload);
                    break;
                case ActivationPropertyId.LocationInfo:
                    locationInfo = DecodeLocationInfo(propertyPayload);
                    break;
                case ActivationPropertyId.ScmReplyInfo:
                    scmReplyInfo = DecodeScmReplyInfo(propertyPayload);
                    break;
                case ActivationPropertyId.SecurityInfo:
                    securityInfo = DecodeSecurityInfo(propertyPayload);
                    break;
                default:
                    customProperties.Add(new ActivationProperty(id, propertyPayload));
                    break;
            }
        }

        return new ActivationProperties(
            specialProperties ?? new SpecialPropertiesData(new ActivationComVersion(major, minor), 0, 0, Guid.Empty, Array.Empty<int>()),
            instanceInfo,
            locationInfo,
            scmReplyInfo,
            securityInfo,
            customProperties);
    }

    /// <summary>Attempts to decode activation properties without throwing.</summary>
    public static bool TryDecode(ReadOnlySpan<byte> payload, out ActivationProperties properties) {
        try {
            properties = Decode(payload);
            return true;
        }
        catch (InvalidOperationException) {
            properties = ActivationProperties.Empty;
            return false;
        }
    }

    private static byte[] EncodeSpecialProperties(SpecialPropertiesData value) => WritePayload((ref NdrWriter writer) => {
        writer.WriteUInt16(value.ClientVersion.Major);
        writer.WriteUInt16(value.ClientVersion.Minor);
        writer.WriteInt32(value.Mode);
        writer.WriteInt32(value.ClassContext);
        writer.WriteGuid(value.RequestedIid);
        writer.WriteUInt32((uint)value.SpecialProperties.Count);
        foreach (int property in value.SpecialProperties) {
            writer.WriteInt32(property);
        }
    });

    private static SpecialPropertiesData DecodeSpecialProperties(ReadOnlySpan<byte> payload, ushort fallbackMajor, ushort fallbackMinor) {
        if (payload.IsEmpty) {
            return new SpecialPropertiesData(new ActivationComVersion(fallbackMajor, fallbackMinor), 0, 0, Guid.Empty, Array.Empty<int>());
        }

        var reader = new NdrReader(payload);
        var version = new ActivationComVersion(reader.ReadUInt16(), reader.ReadUInt16());
        int mode = reader.ReadInt32();
        int classContext = reader.ReadInt32();
        Guid requestedIid = reader.ReadGuid();
        uint count = reader.ReadUInt32();
        if (count > 1024) {
            throw new InvalidOperationException("SPECIAL_PROPERTIES_DATA count is unreasonably large.");
        }

        var specialProperties = new int[count];
        for (int i = 0; i < specialProperties.Length; i++) {
            specialProperties[i] = reader.ReadInt32();
        }

        return new SpecialPropertiesData(version, mode, classContext, requestedIid, specialProperties);
    }

    private static byte[] EncodeInstanceInfo(InstanceInfo value) => WritePayload((ref NdrWriter writer) => {
        writer.WriteGuid(value.Clsid);
        writer.WriteGuid(value.RequestedIid);
        writer.WriteInt32(value.ClassContext);
        writer.WriteInt32(value.Mode);
    });

    private static InstanceInfo DecodeInstanceInfo(ReadOnlySpan<byte> payload) {
        var reader = new NdrReader(payload);
        return new InstanceInfo(reader.ReadGuid(), reader.ReadGuid(), reader.ReadInt32(), reader.ReadInt32());
    }

    private static byte[] EncodeLocationInfo(LocationInfo value) => WritePayload((ref NdrWriter writer) => {
        writer.WriteUnicodeStringPtr(value.MachineName);
        writer.WriteInt32(value.ProcessId);
        writer.WriteUInt32((uint)value.ProtocolSequences.Count);
        foreach (int protocolSequence in value.ProtocolSequences) {
            writer.WriteInt32(protocolSequence);
        }
    });

    private static LocationInfo DecodeLocationInfo(ReadOnlySpan<byte> payload) {
        var reader = new NdrReader(payload);
        string? machineName = reader.ReadUnicodeStringPtr();
        int processId = reader.ReadInt32();
        uint count = reader.ReadUInt32();
        if (count > 1024) {
            throw new InvalidOperationException("LOCATION_INFO protocol sequence count is unreasonably large.");
        }

        var protocolSequences = new int[count];
        for (int i = 0; i < protocolSequences.Length; i++) {
            protocolSequences[i] = reader.ReadInt32();
        }

        return new LocationInfo(machineName, processId, protocolSequences);
    }

    private static byte[] EncodeScmReplyInfo(ScmReplyInfo value) => WritePayload((ref NdrWriter writer) => {
        writer.WriteInt32(value.Hresult);
        writer.WriteGuid(value.Oxid);
        writer.WriteGuid(value.Oid);
        writer.WriteGuid(value.Ipid);
        writer.WriteUInt32((uint)value.ObjRef.Length);
        writer.WriteRawBytes(value.ObjRef);
        writer.AlignTo(4);
    });

    private static ScmReplyInfo DecodeScmReplyInfo(ReadOnlySpan<byte> payload) {
        var reader = new NdrReader(payload);
        int hresult = reader.ReadInt32();
        Guid oxid = reader.ReadGuid();
        Guid oid = reader.ReadGuid();
        Guid ipid = reader.ReadGuid();
        uint objRefLength = reader.ReadUInt32();
        if (objRefLength > int.MaxValue || objRefLength > reader.RemainingBytes) {
            throw new InvalidOperationException("SCM_REPLY_INFO OBJREF length exceeds the remaining data.");
        }

        byte[] objRef = reader.ReadRawBytes((int)objRefLength).ToArray();
        return new ScmReplyInfo(hresult, oxid, oid, ipid, objRef, copy: false);
    }

    private static byte[] EncodeSecurityInfo(SecurityInfo value) => WritePayload((ref NdrWriter writer) => {
        writer.WriteInt32(value.AuthenticationLevel);
        writer.WriteInt32(value.ImpersonationLevel);
        writer.WriteInt32(value.Capabilities);
    });

    private static SecurityInfo DecodeSecurityInfo(ReadOnlySpan<byte> payload) {
        var reader = new NdrReader(payload);
        return new SecurityInfo(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
    }

    private static byte[] WritePayload(NdrWriteCallback write) {
        var buffer = new byte[InitialBufferSize];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static int Padding(int length, int boundary) {
        int misaligned = length & (boundary - 1);
        return misaligned == 0 ? 0 : boundary - misaligned;
    }
}
