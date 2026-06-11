//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Activation;

/// <summary>NDR codec for the legacy <c>IActivation::RemoteActivation</c> method body.</summary>
public static class IActivationCodec
{
    private const int InitialBufferSize = 4096;
    private const int MaximumBufferSize = 1024 * 1024;
    private const int MaxRequestedInterfaces = 0x8000;
    private const int MaxRequestedProtocolSequences = 0x8000;

    private delegate void NdrWriteAction(ref NdrWriter writer);

    /// <summary>Encodes a legacy <c>RemoteActivation</c> request body, excluding the ORPC envelope.</summary>
    public static byte[] EncodeRemoteActivationRequest(RemoteActivationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteGuid(request.Clsid);
            writer.WriteUnicodeStringPtr(request.ObjectName);
            WriteMInterfacePointerPtr(ref writer, request.ObjectStorage.Span);
            writer.WriteUInt32(request.ClientImpLevel);
            writer.WriteUInt32(request.Mode);

            writer.WriteUInt32(unchecked((uint)request.RequestedIids.Count));
            _ = writer.WriteReferentId();
            writer.WriteConformanceHeader(request.RequestedIids.Count);
            for (int i = 0; i < request.RequestedIids.Count; i++)
            {
                writer.WriteGuid(request.RequestedIids[i]);
            }

            writer.WriteUInt16(checked((ushort)request.RequestedProtocolSequences.Count));
            writer.WriteConformanceHeader(request.RequestedProtocolSequences.Count);
            for (int i = 0; i < request.RequestedProtocolSequences.Count; i++)
            {
                writer.WriteUInt16(request.RequestedProtocolSequences[i]);
            }
        });
    }

    /// <summary>Decodes a legacy <c>RemoteActivation</c> request body, excluding the ORPC envelope.</summary>
    public static RemoteActivationRequest DecodeRemoteActivationRequest(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        Guid clsid = reader.ReadGuid();
        string? objectName = reader.ReadUnicodeStringPtr();
        byte[] objectStorage = ReadMInterfacePointerPtr(ref reader);
        uint clientImpLevel = reader.ReadUInt32();
        uint mode = reader.ReadUInt32();
        uint interfaceCount = reader.ReadUInt32();
        if (interfaceCount is 0 or > MaxRequestedInterfaces)
        {
            throw new InvalidOperationException("IActivation request IID count is outside the allowed range.");
        }

        if (!reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("IActivation request IID array pointer is null.");
        }

        int iidArrayCount = reader.ReadConformanceHeader();
        int requestedInterfaceCount = checked((int)interfaceCount);
        if (iidArrayCount != requestedInterfaceCount)
        {
            throw new InvalidOperationException("IActivation request IID array size does not match the Interfaces field.");
        }

        var iids = new Guid[iidArrayCount];
        for (int i = 0; i < iids.Length; i++)
        {
            iids[i] = reader.ReadGuid();
        }

        ushort protocolSequenceCount = reader.ReadUInt16();
        if (protocolSequenceCount is 0 or > MaxRequestedProtocolSequences)
        {
            throw new InvalidOperationException("IActivation request protocol sequence count is outside the allowed range.");
        }

        int encodedProtocolSequenceCount = reader.ReadConformanceHeader();
        if (encodedProtocolSequenceCount != protocolSequenceCount)
        {
            throw new InvalidOperationException("IActivation request protocol sequence array size does not match the count field.");
        }

        var protocolSequences = new ushort[encodedProtocolSequenceCount];
        for (int i = 0; i < protocolSequences.Length; i++)
        {
            protocolSequences[i] = reader.ReadUInt16();
        }

        return new RemoteActivationRequest(clsid, iids, clientImpLevel, mode, protocolSequences)
        {
            ObjectName = objectName,
            ObjectStorage = objectStorage,
        };
    }

    /// <summary>Encodes a legacy <c>RemoteActivation</c> response body, excluding the ORPC envelope.</summary>
    public static byte[] EncodeRemoteActivationResponse(RemoteActivationResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.InterfaceResults.Count > MaxRequestedInterfaces)
        {
            throw new ArgumentException("IActivation response interface result count is too large.", nameof(response));
        }

        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt64(UInt64FromGuid(response.Oxid));
            WriteDualStringArrayPointerPointer(ref writer, response.OxidBindings.Span);
            writer.WriteGuid(response.IpidRemUnknown);
            writer.WriteUInt32(response.AuthnHint);
            writer.WriteUInt16(response.ServerVersion.Major);
            writer.WriteUInt16(response.ServerVersion.Minor);
            writer.WriteInt32(response.Hresult);

            writer.WriteConformanceHeader(response.InterfaceResults.Count);
            for (int i = 0; i < response.InterfaceResults.Count; i++)
            {
                if (response.InterfaceResults[i].ObjRef.IsEmpty)
                {
                    writer.WriteNullReferent();
                }
                else
                {
                    _ = writer.WriteReferentId();
                }
            }

            for (int i = 0; i < response.InterfaceResults.Count; i++)
            {
                ReadOnlySpan<byte> objRef = response.InterfaceResults[i].ObjRef.Span;
                if (!objRef.IsEmpty)
                {
                    WriteMInterfacePointer(ref writer, objRef);
                }
            }

            writer.WriteConformanceHeader(response.InterfaceResults.Count);
            for (int i = 0; i < response.InterfaceResults.Count; i++)
            {
                writer.WriteInt32(response.InterfaceResults[i].Hresult);
            }
        });
    }

    /// <summary>Decodes a legacy <c>RemoteActivation</c> response body, excluding the ORPC envelope.</summary>
    public static RemoteActivationResponse DecodeRemoteActivationResponse(ReadOnlySpan<byte> payload, int expectedInterfaceCount)
    {
        if (expectedInterfaceCount is <= 0 or > MaxRequestedInterfaces)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedInterfaceCount), expectedInterfaceCount, "Expected interface count is outside the allowed range.");
        }

        var reader = new NdrReader(payload);
        ulong oxid = reader.ReadUInt64();
        byte[] oxidBindings = ReadDualStringArrayPointerPointer(ref reader);
        Guid ipidRemUnknown = reader.ReadGuid();
        uint authnHint = reader.ReadUInt32();
        ushort major = reader.ReadUInt16();
        ushort minor = reader.ReadUInt16();
        int hresult = reader.ReadInt32();

        int interfaceDataCount = reader.ReadConformanceHeader();
        if (interfaceDataCount != expectedInterfaceCount)
        {
            throw new InvalidOperationException("IActivation response interface pointer array size does not match the request.");
        }

        var hasInterfaceData = new bool[interfaceDataCount];
        for (int i = 0; i < hasInterfaceData.Length; i++)
        {
            hasInterfaceData[i] = reader.TryReadReferentId(out _);
        }

        var objRefs = new byte[interfaceDataCount][];
        for (int i = 0; i < objRefs.Length; i++)
        {
            objRefs[i] = hasInterfaceData[i] ? ReadMInterfacePointer(ref reader) : Array.Empty<byte>();
        }

        int resultCount = reader.ReadConformanceHeader();
        if (resultCount != expectedInterfaceCount)
        {
            throw new InvalidOperationException("IActivation response HRESULT array size does not match the request.");
        }

        var interfaceResults = new RemoteActivationInterfaceResult[resultCount];
        for (int i = 0; i < interfaceResults.Length; i++)
        {
            int interfaceHresult = reader.ReadInt32();
            interfaceResults[i] = new RemoteActivationInterfaceResult(interfaceHresult, objRefs[i]);
        }

        return new RemoteActivationResponse(
            hresult,
            GuidFromUInt64(oxid),
            ipidRemUnknown,
            authnHint,
            (major, minor),
            interfaceResults)
        {
            OxidBindings = oxidBindings,
        };
    }

    private static void ValidateRequest(RemoteActivationRequest request)
    {
        if (request.RequestedIids.Count is 0 or > MaxRequestedInterfaces)
        {
            throw new ArgumentException("IActivation requires between 1 and 32768 requested IIDs.", nameof(request));
        }

        if (request.RequestedProtocolSequences.Count is 0 or > MaxRequestedProtocolSequences)
        {
            throw new ArgumentException("IActivation requires between 1 and 32768 requested protocol sequences.", nameof(request));
        }
    }

    private static void WriteMInterfacePointerPtr(ref NdrWriter writer, ReadOnlySpan<byte> objRef)
    {
        if (objRef.IsEmpty)
        {
            writer.WriteNullReferent();
            return;
        }

        _ = writer.WriteReferentId();
        WriteMInterfacePointer(ref writer, objRef);
    }

    private static byte[] ReadMInterfacePointerPtr(ref NdrReader reader) =>
        reader.TryReadReferentId(out _) ? ReadMInterfacePointer(ref reader) : Array.Empty<byte>();

    private static void WriteMInterfacePointer(ref NdrWriter writer, ReadOnlySpan<byte> objRef)
    {
        writer.WriteUInt32(unchecked((uint)objRef.Length));
        writer.WriteUInt32(unchecked((uint)objRef.Length));
        writer.WriteRawBytes(objRef);
        writer.AlignTo(4);
    }

    private static byte[] ReadMInterfacePointer(ref NdrReader reader)
    {
        uint maxCount = reader.ReadUInt32();
        uint actualCount = reader.ReadUInt32();
        if (actualCount > maxCount || actualCount > reader.RemainingBytes)
        {
            throw new InvalidOperationException("MInterfacePointer byte count exceeds the remaining payload.");
        }

        byte[] objRef = reader.ReadRawBytes(checked((int)actualCount)).ToArray();
        reader.AlignTo(4);
        return objRef;
    }

    private static void WriteDualStringArrayPointerPointer(ref NdrWriter writer, ReadOnlySpan<byte> dualStringArray)
    {
        _ = writer.WriteReferentId();
        _ = writer.WriteReferentId();
        if (dualStringArray.IsEmpty)
        {
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            return;
        }

        writer.WriteRawBytes(dualStringArray);
        writer.AlignTo(4);
    }

    private static byte[] ReadDualStringArrayPointerPointer(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return Array.Empty<byte>();
        }

        if (!reader.TryReadReferentId(out _))
        {
            return Array.Empty<byte>();
        }

        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        if (entryCount > reader.RemainingBytes / sizeof(ushort))
        {
            throw new InvalidOperationException("DUALSTRINGARRAY entry count exceeds the remaining payload.");
        }

        byte[] dualStringArray = new byte[sizeof(ushort) + sizeof(ushort) + (entryCount * sizeof(ushort))];
        BinaryPrimitives.WriteUInt16LittleEndian(dualStringArray.AsSpan(0, sizeof(ushort)), entryCount);
        BinaryPrimitives.WriteUInt16LittleEndian(dualStringArray.AsSpan(sizeof(ushort), sizeof(ushort)), securityOffset);
        for (int i = 0; i < entryCount; i++)
        {
            ushort entry = reader.ReadUInt16();
            BinaryPrimitives.WriteUInt16LittleEndian(dualStringArray.AsSpan(4 + (i * sizeof(ushort)), sizeof(ushort)), entry);
        }

        reader.AlignTo(4);
        return dualStringArray;
    }

    private static Guid GuidFromUInt64(ulong value)
    {
        Span<byte> bytes = stackalloc byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        return new Guid(bytes);
    }

    private static ulong UInt64FromGuid(Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        bool ok = value.TryWriteBytes(bytes);
        if (!ok)
        {
            throw new InvalidOperationException("Guid.TryWriteBytes failed unexpectedly.");
        }

        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    private static byte[] WritePayload(NdrWriteAction action)
    {
        for (int size = InitialBufferSize; size <= MaximumBufferSize; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                action(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < MaximumBufferSize)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the IActivation RemoteActivation payload.");
    }
}
