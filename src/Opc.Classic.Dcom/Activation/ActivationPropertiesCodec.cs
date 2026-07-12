// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Ndr;

#pragma warning disable MA0048 // Activation DTO records are kept with their codec to avoid scattering protocol shapes.

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Codec for the MS-DCOM Activation Properties BLOB and the
/// IRemoteSCMActivator::RemoteCreateInstance opnum 4 method body.
/// </summary>
public static class ActivationPropertiesCodec
{
    private const int InitialBufferSize = 512;
    private const int MaximumBufferSize = 1024 * 1024;
    private const int MaxRequestedInterfaces = 0x8000;
    private const int MinActivationProperties = 1;
    private const int MaxActivationProperties = 10;
    private const uint ObjRefSignature = 0x574F454D;
    private const uint ObjRefCustom = 0x00000004;
    private const byte TypeSerializationVersion = 0x01;
    private const byte TypeSerializationLittleEndian = 0x10;
    private const ushort CommonHeaderLength = 0x0008;
    private const uint CommonHeaderFiller = 0xCCCCCCCC;

    private static readonly Guid IidActivationPropertiesIn = new("000001A2-0000-0000-C000-000000000046");
    private static readonly Guid IidActivationPropertiesOut = new("000001A3-0000-0000-C000-000000000046");
    private static readonly Guid ClsidActivationPropertiesIn = new("00000338-0000-0000-C000-000000000046");
    private static readonly Guid ClsidActivationPropertiesOut = new("00000339-0000-0000-C000-000000000046");
    private static readonly Guid ClsidInstantiationInfo = new("000001AB-0000-0000-C000-000000000046");
    private static readonly Guid ClsidScmRequestInfo = new("000001AA-0000-0000-C000-000000000046");
    private static readonly Guid ClsidServerLocationInfo = new("000001A4-0000-0000-C000-000000000046");
    private static readonly Guid ClsidScmReplyInfo = new("000001B6-0000-0000-C000-000000000046");
    private static readonly Guid ClsidPropsOutInfo = new("00000339-0000-0000-C000-000000000046");

    private delegate void NdrWriteAction(ref NdrWriter writer);

    /// <summary>
    /// Encodes an opnum 4 request body, excluding the ORPCTHIS envelope.
    /// </summary>
    public static byte[] EncodeRemoteCreateInstanceRequest(
        Guid classId,
        IReadOnlyList<Guid> requestedIids,
        IReadOnlyList<ushort> requestedProtocolSequences,
        uint classContext = 0x14,
        uint clientImpersonationLevel = 2,
        (ushort Major, ushort Minor) clientComVersion = default)
    {
        byte[] blob = EncodeActivationPropertiesInBlob(
            classId,
            requestedIids,
            requestedProtocolSequences,
            classContext,
            clientImpersonationLevel,
            clientComVersion == default ? ((ushort)5, (ushort)7) : clientComVersion);
        byte[] objRef = EncodeCustomObjRef(IidActivationPropertiesIn, ClsidActivationPropertiesIn, blob);

        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteNullReferent();
            _ = writer.WriteReferentId();
            WriteMInterfacePointer(ref writer, objRef);
        });
    }

    /// <summary>
    /// Encodes an ActivationPropertiesIn BLOB for RemoteCreateInstance.
    /// </summary>
    public static byte[] EncodeActivationPropertiesInBlob(
        Guid classId,
        IReadOnlyList<Guid> requestedIids,
        IReadOnlyList<ushort> requestedProtocolSequences,
        uint classContext = 0x14,
        uint clientImpersonationLevel = 2,
        (ushort Major, ushort Minor) clientComVersion = default)
    {
        ArgumentNullException.ThrowIfNull(requestedIids);
        ArgumentNullException.ThrowIfNull(requestedProtocolSequences);
        if (requestedIids.Count is 0 or > MaxRequestedInterfaces)
        {
            throw new ArgumentException("RemoteCreateInstance requires between 1 and 32768 requested IIDs.", nameof(requestedIids));
        }

        if (requestedProtocolSequences.Count == 0)
        {
            throw new ArgumentException("RemoteCreateInstance requires at least one requested protocol sequence.", nameof(requestedProtocolSequences));
        }

        (ushort Major, ushort Minor) version = clientComVersion == default ? ((ushort)5, (ushort)7) : clientComVersion;
        byte[] instantiation = EncodeInstantiationInfo(classId, requestedIids, classContext, version);
        byte[] scmRequest = EncodeScmRequestInfo(clientImpersonationLevel, requestedProtocolSequences);
        byte[] location = EncodeLocationInfo();

        Guid[] propertyIds = { ClsidInstantiationInfo, ClsidScmRequestInfo, ClsidServerLocationInfo };
        byte[][] properties = { instantiation, scmRequest, location };
        return EncodeActivationPropertiesBlob(propertyIds, properties);
    }

    /// <summary>
    /// Decodes an opnum 4 request body, excluding the ORPCTHIS envelope.
    /// </summary>
    public static RemoteCreateInstanceActivationRequest DecodeRemoteCreateInstanceRequest(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        if (reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("IRemoteSCMActivator pUnkOuter must be NULL.");
        }

        if (!reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("IRemoteSCMActivator pActProperties must be non-NULL.");
        }

        byte[] objRef = ReadMInterfacePointer(ref reader);
        ReadOnlySpan<byte> blob = DecodeCustomObjRef(objRef, IidActivationPropertiesIn, ClsidActivationPropertiesIn);
        ActivationPropertiesInData properties = DecodeActivationPropertiesInBlob(blob);
        return new RemoteCreateInstanceActivationRequest(
            properties.ClassId,
            properties.RequestedIids,
            properties.RequestedProtocolSequences,
            blob.ToArray());
    }

    /// <summary>
    /// Attempts to decode an opnum 4 request body without throwing.
    /// </summary>
    public static bool TryDecodeRemoteCreateInstanceRequest(ReadOnlySpan<byte> payload, out RemoteCreateInstanceActivationRequest request)
    {
        try
        {
            request = DecodeRemoteCreateInstanceRequest(payload);
            return true;
        }
        catch (InvalidOperationException)
        {
            request = RemoteCreateInstanceActivationRequest.Empty;
            return false;
        }
    }

    /// <summary>
    /// Decodes an ActivationPropertiesIn BLOB.
    /// </summary>
    public static ActivationPropertiesInData DecodeActivationPropertiesInBlob(ReadOnlySpan<byte> blob)
    {
        DecodedActivationBlob decoded = DecodeActivationPropertiesBlob(blob);
        Guid classId = Guid.Empty;
        Guid[] requestedIids = Array.Empty<Guid>();
        ushort[] protocolSequences = Array.Empty<ushort>();
        uint clientImpLevel = 0;

        for (int i = 0; i < decoded.PropertyIds.Length; i++)
        {
            if (decoded.PropertyIds[i] == ClsidInstantiationInfo)
            {
                DecodeInstantiationInfo(decoded.PropertyBodies[i], out classId, out requestedIids);
            }
            else if (decoded.PropertyIds[i] == ClsidScmRequestInfo)
            {
                DecodeScmRequestInfo(decoded.PropertyBodies[i], out clientImpLevel, out protocolSequences);
            }
        }

        if (classId == Guid.Empty || requestedIids.Length == 0)
        {
            throw new InvalidOperationException("ActivationPropertiesIn is missing InstantiationInfoData.");
        }

        if (protocolSequences.Length == 0)
        {
            throw new InvalidOperationException("ActivationPropertiesIn is missing ScmRequestInfoData protocol sequences.");
        }

        return new ActivationPropertiesInData(classId, requestedIids, protocolSequences, clientImpLevel);
    }

    /// <summary>
    /// Encodes an ActivationPropertiesOut BLOB. This is primarily used by tests
    /// and by managed loopback activators.
    /// </summary>
    public static byte[] EncodeActivationPropertiesOutBlob(
        ulong oxid,
        ReadOnlySpan<byte> oxidBindings,
        Guid ipidRemUnknown,
        uint authnHint,
        (ushort Major, ushort Minor) serverVersion,
        IReadOnlyList<ActivationInterfaceResult> interfaceResults)
    {
        ArgumentNullException.ThrowIfNull(interfaceResults);
        if (interfaceResults.Count is 0 or > MaxRequestedInterfaces)
        {
            throw new ArgumentException("ActivationPropertiesOut requires between 1 and 32768 interface results.", nameof(interfaceResults));
        }

        byte[] scmReply = EncodeScmReplyInfo(oxid, oxidBindings, ipidRemUnknown, authnHint, serverVersion);
        byte[] propsOut = EncodePropsOutInfo(interfaceResults);
        Guid[] propertyIds = { ClsidScmReplyInfo, ClsidPropsOutInfo };
        byte[][] properties = { scmReply, propsOut };
        return EncodeActivationPropertiesBlob(propertyIds, properties);
    }

    /// <summary>
    /// Encodes an opnum 4 response body, excluding the ORPCTHAT envelope.
    /// </summary>
    public static byte[] EncodeRemoteCreateInstanceResponse(
        ulong oxid,
        ReadOnlySpan<byte> oxidBindings,
        Guid ipidRemUnknown,
        uint authnHint,
        (ushort Major, ushort Minor) serverVersion,
        IReadOnlyList<ActivationInterfaceResult> interfaceResults,
        int hresult = 0)
    {
        byte[] blob = EncodeActivationPropertiesOutBlob(oxid, oxidBindings, ipidRemUnknown, authnHint, serverVersion, interfaceResults);
        byte[] objRef = EncodeCustomObjRef(IidActivationPropertiesOut, ClsidActivationPropertiesOut, blob);
        return WritePayload((ref NdrWriter writer) =>
        {
            _ = writer.WriteReferentId();
            WriteMInterfacePointer(ref writer, objRef);
            writer.WriteInt32(hresult);
        });
    }

    /// <summary>
    /// Decodes an opnum 4 response body, excluding the ORPCTHAT envelope.
    /// </summary>
    public static ActivationPropertiesOutData DecodeRemoteCreateInstanceResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        byte[] objRef;
        if (payload.Length >= sizeof(uint) && BinaryPrimitives.ReadUInt32LittleEndian(payload) == ObjRefSignature)
        {
            objRef = payload.ToArray();
        }
        else
        {
            if (!reader.TryReadReferentId(out _))
            {
                int hresult = reader.RemainingBytes >= sizeof(int) ? reader.ReadInt32() : 0;
                throw new InvalidOperationException($"IRemoteSCMActivator returned a NULL ppActProperties pointer (HRESULT 0x{unchecked((uint)hresult):X8}).");
            }

            objRef = ReadMInterfacePointer(ref reader);
            if (reader.RemainingBytes >= sizeof(int))
            {
                _ = reader.ReadInt32();
            }
        }

        ReadOnlySpan<byte> blob = DecodeCustomObjRef(objRef, IidActivationPropertiesOut, ClsidActivationPropertiesOut);
        return DecodeActivationPropertiesOutBlob(blob);
    }

    /// <summary>
    /// Attempts to decode an opnum 4 response body without throwing.
    /// </summary>
    public static bool TryDecodeRemoteCreateInstanceResponse(ReadOnlySpan<byte> payload, out ActivationPropertiesOutData response)
    {
        try
        {
            response = DecodeRemoteCreateInstanceResponse(payload);
            return true;
        }
        catch (InvalidOperationException)
        {
            response = ActivationPropertiesOutData.Empty;
            return false;
        }
        catch (ArgumentException)
        {
            response = ActivationPropertiesOutData.Empty;
            return false;
        }
    }

    /// <summary>
    /// Decodes an ActivationPropertiesOut BLOB.
    /// </summary>
    public static ActivationPropertiesOutData DecodeActivationPropertiesOutBlob(ReadOnlySpan<byte> blob)
    {
        DecodedActivationBlob decoded = DecodeActivationPropertiesBlob(blob);
        ScmReplyData? scmReply = null;
        ActivationInterfaceResult[]? interfaceResults = null;

        for (int i = 0; i < decoded.PropertyIds.Length; i++)
        {
            if (decoded.PropertyIds[i] == ClsidScmReplyInfo)
            {
                scmReply = DecodeScmReplyInfo(decoded.PropertyBodies[i]);
            }
            else if (decoded.PropertyIds[i] == ClsidPropsOutInfo)
            {
                interfaceResults = DecodePropsOutInfo(decoded.PropertyBodies[i]);
            }
        }

        if (scmReply is null)
        {
            throw new InvalidOperationException("ActivationPropertiesOut is missing ScmReplyInfoData.");
        }

        if (interfaceResults is null || interfaceResults.Length == 0)
        {
            throw new InvalidOperationException("ActivationPropertiesOut is missing PropsOutInfo.");
        }

        return new ActivationPropertiesOutData(
            scmReply.Oxid,
            scmReply.OxidBindings,
            scmReply.IpidRemUnknown,
            scmReply.AuthnHint,
            scmReply.ServerVersion,
            interfaceResults);
    }

    private static byte[] EncodeActivationPropertiesBlob(IReadOnlyList<Guid> propertyIds, IReadOnlyList<byte[]> properties)
    {
        if (propertyIds.Count != properties.Count || propertyIds.Count is < MinActivationProperties or > MaxActivationProperties)
        {
            throw new ArgumentException("Activation property metadata does not match the property array.", nameof(propertyIds));
        }

        uint propertyBytes = 0;
        var propertySizes = new uint[properties.Count];
        for (int i = 0; i < properties.Count; i++)
        {
            propertySizes[i] = checked((uint)properties[i].Length);
            propertyBytes = checked(propertyBytes + propertySizes[i]);
        }

        byte[] header = EncodeCustomHeader(0, 0, propertyIds, propertySizes);
        uint totalSize = checked((uint)header.Length + propertyBytes);
        header = EncodeCustomHeader(totalSize, 0, propertyIds, propertySizes);
        header = EncodeCustomHeader(totalSize, checked((uint)header.Length), propertyIds, propertySizes);

        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(totalSize);
            writer.WriteUInt32(0);
            writer.WriteRawBytes(header);
            for (int i = 0; i < properties.Count; i++)
            {
                writer.WriteRawBytes(properties[i]);
            }
        });
    }

    private static DecodedActivationBlob DecodeActivationPropertiesBlob(ReadOnlySpan<byte> blob)
    {
        var reader = new NdrReader(blob);
        uint dwSize = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        if (dwSize > reader.RemainingBytes)
        {
            throw new InvalidOperationException("Activation properties dwSize exceeds the remaining BLOB length.");
        }

        ReadOnlySpan<byte> activationPayload = blob.Slice(reader.Position, checked((int)dwSize));
        var activationReader = new NdrReader(activationPayload);
        ReadOnlySpan<byte> headerBody = ReadTypeSerializationStream(ref activationReader, out int headerStreamSize);
        DecodeCustomHeader(headerBody, out uint totalSize, out uint headerSize, out Guid[] propertyIds, out uint[] propertySizes);
        if (totalSize != dwSize || headerSize != headerStreamSize)
        {
            throw new InvalidOperationException("Activation CustomHeader size fields do not match the BLOB layout.");
        }

        if (propertyIds.Length != propertySizes.Length)
        {
            throw new InvalidOperationException("Activation CustomHeader property metadata length mismatch.");
        }

        var propertyBodies = new byte[propertyIds.Length][];
        for (int i = 0; i < propertyIds.Length; i++)
        {
            uint propertySize = propertySizes[i];
            if (propertySize > activationReader.RemainingBytes)
            {
                throw new InvalidOperationException("Activation property stream length exceeds the remaining BLOB length.");
            }

            ReadOnlySpan<byte> stream = activationPayload.Slice(activationReader.Position, checked((int)propertySize));
            var streamReader = new NdrReader(stream);
            ReadOnlySpan<byte> propertyBody = ReadTypeSerializationStream(ref streamReader, out int streamSize);
            if (streamSize != propertySize)
            {
                throw new InvalidOperationException("Activation property stream size does not match CustomHeader.pSizes.");
            }

            propertyBodies[i] = propertyBody.ToArray();
            _ = activationReader.ReadRawBytes(checked((int)propertySize));
        }

        return new DecodedActivationBlob(propertyIds, propertyBodies);
    }

    private static byte[] EncodeCustomHeader(uint totalSize, uint headerSize, IReadOnlyList<Guid> propertyIds, IReadOnlyList<uint> propertySizes) =>
        EncodeTypeSerializationStream((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(totalSize);
            writer.WriteUInt32(headerSize);
            writer.WriteUInt32(0);
            writer.WriteUInt32(2);
            writer.WriteUInt32(checked((uint)propertyIds.Count));
            writer.WriteGuid(Guid.Empty);
            _ = writer.WriteReferentId();
            _ = writer.WriteReferentId();
            writer.WriteNullReferent();
            writer.WriteConformanceHeader(propertyIds.Count);
            for (int i = 0; i < propertyIds.Count; i++)
            {
                writer.WriteGuid(propertyIds[i]);
            }

            writer.WriteConformanceHeader(propertySizes.Count);
            for (int i = 0; i < propertySizes.Count; i++)
            {
                writer.WriteUInt32(propertySizes[i]);
            }
        });

    private static void DecodeCustomHeader(
        ReadOnlySpan<byte> body,
        out uint totalSize,
        out uint headerSize,
        out Guid[] propertyIds,
        out uint[] propertySizes)
    {
        var reader = new NdrReader(body);
        totalSize = reader.ReadUInt32();
        headerSize = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        uint cIfs = reader.ReadUInt32();
        if (cIfs is < MinActivationProperties or > MaxActivationProperties)
        {
            throw new InvalidOperationException("Activation CustomHeader cIfs is outside the allowed range.");
        }

        _ = reader.ReadGuid();
        bool hasClassIds = reader.TryReadReferentId(out _);
        bool hasSizes = reader.TryReadReferentId(out _);
        if (reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("Activation CustomHeader pdwReserved must be NULL.");
        }

        if (!hasClassIds || !hasSizes)
        {
            throw new InvalidOperationException("Activation CustomHeader property arrays must be non-NULL.");
        }

        int classIdCount = reader.ReadConformanceHeader();
        if (classIdCount != cIfs)
        {
            throw new InvalidOperationException("Activation CustomHeader pclsid count does not match cIfs.");
        }

        propertyIds = new Guid[classIdCount];
        for (int i = 0; i < propertyIds.Length; i++)
        {
            propertyIds[i] = reader.ReadGuid();
        }

        int sizeCount = reader.ReadConformanceHeader();
        if (sizeCount != cIfs)
        {
            throw new InvalidOperationException("Activation CustomHeader pSizes count does not match cIfs.");
        }

        propertySizes = new uint[sizeCount];
        for (int i = 0; i < propertySizes.Length; i++)
        {
            propertySizes[i] = reader.ReadUInt32();
        }
    }

    private static byte[] EncodeInstantiationInfo(
        Guid classId,
        IReadOnlyList<Guid> requestedIids,
        uint classContext,
        (ushort Major, ushort Minor) clientComVersion)
    {
        byte[] firstPass = EncodeInstantiationInfoCore(classId, requestedIids, classContext, clientComVersion, 0);
        return EncodeInstantiationInfoCore(classId, requestedIids, classContext, clientComVersion, checked((uint)firstPass.Length));
    }

    private static byte[] EncodeInstantiationInfoCore(
        Guid classId,
        IReadOnlyList<Guid> requestedIids,
        uint classContext,
        (ushort Major, ushort Minor) clientComVersion,
        uint thisSize) =>
        EncodeTypeSerializationStream((ref NdrWriter writer) =>
        {
            writer.WriteGuid(classId);
            writer.WriteUInt32(classContext);
            writer.WriteUInt32(0);
            writer.WriteInt32(0);
            writer.WriteUInt32(checked((uint)requestedIids.Count));
            writer.WriteUInt32(0);
            _ = writer.WriteReferentId();
            writer.WriteUInt32(thisSize);
            writer.WriteUInt16(clientComVersion.Major);
            writer.WriteUInt16(clientComVersion.Minor);
            writer.WriteConformanceHeader(requestedIids.Count);
            for (int i = 0; i < requestedIids.Count; i++)
            {
                writer.WriteGuid(requestedIids[i]);
            }
        });

    private static void DecodeInstantiationInfo(ReadOnlySpan<byte> body, out Guid classId, out Guid[] requestedIids)
    {
        var reader = new NdrReader(body);
        classId = reader.ReadGuid();
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        _ = reader.ReadInt32();
        uint cIid = reader.ReadUInt32();
        if (cIid is 0 or > MaxRequestedInterfaces)
        {
            throw new InvalidOperationException("InstantiationInfoData cIID is outside the allowed range.");
        }

        _ = reader.ReadUInt32();
        bool hasIids = reader.TryReadReferentId(out _);
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        if (!hasIids)
        {
            throw new InvalidOperationException("InstantiationInfoData pIID must be non-NULL.");
        }

        int iidCount = reader.ReadConformanceHeader();
        if (iidCount != cIid)
        {
            throw new InvalidOperationException("InstantiationInfoData pIID count does not match cIID.");
        }

        requestedIids = new Guid[iidCount];
        for (int i = 0; i < requestedIids.Length; i++)
        {
            requestedIids[i] = reader.ReadGuid();
        }
    }

    private static byte[] EncodeScmRequestInfo(uint clientImpLevel, IReadOnlyList<ushort> protocolSequences) =>
        EncodeTypeSerializationStream((ref NdrWriter writer) =>
        {
            writer.WriteNullReferent();
            _ = writer.WriteReferentId();
            writer.WriteUInt32(clientImpLevel);
            writer.WriteUInt16(checked((ushort)protocolSequences.Count));
            _ = writer.WriteReferentId();
            writer.WriteConformanceHeader(protocolSequences.Count);
            for (int i = 0; i < protocolSequences.Count; i++)
            {
                writer.WriteUInt16(protocolSequences[i]);
            }
        });

    private static void DecodeScmRequestInfo(ReadOnlySpan<byte> body, out uint clientImpLevel, out ushort[] protocolSequences)
    {
        var reader = new NdrReader(body);
        if (reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("ScmRequestInfoData pdwReserved must be NULL.");
        }

        if (!reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("ScmRequestInfoData remoteRequest must be non-NULL.");
        }

        clientImpLevel = reader.ReadUInt32();
        ushort count = reader.ReadUInt16();
        bool hasProtocolSequences = reader.TryReadReferentId(out _);
        if (!hasProtocolSequences)
        {
            throw new InvalidOperationException("customREMOTE_REQUEST_SCM_INFO pRequestedProtseqs must be non-NULL.");
        }

        int encodedCount = reader.ReadConformanceHeader();
        if (encodedCount != count)
        {
            throw new InvalidOperationException("customREMOTE_REQUEST_SCM_INFO protocol sequence count mismatch.");
        }

        protocolSequences = new ushort[encodedCount];
        for (int i = 0; i < protocolSequences.Length; i++)
        {
            protocolSequences[i] = reader.ReadUInt16();
        }
    }

    private static byte[] EncodeLocationInfo() =>
        EncodeTypeSerializationStream((ref NdrWriter writer) =>
        {
            writer.WriteNullReferent();
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
        });

    private static byte[] EncodeScmReplyInfo(
        ulong oxid,
        ReadOnlySpan<byte> oxidBindings,
        Guid ipidRemUnknown,
        uint authnHint,
        (ushort Major, ushort Minor) serverVersion)
    {
        byte[] encodedBindings = oxidBindings.ToArray();
        return EncodeTypeSerializationStream((ref NdrWriter writer) =>
        {
            writer.WriteNullReferent();
            _ = writer.WriteReferentId();
            writer.WriteUInt64(oxid);
            _ = writer.WriteReferentId();
            writer.WriteGuid(ipidRemUnknown);
            writer.WriteUInt32(authnHint);
            writer.WriteUInt16(serverVersion.Major);
            writer.WriteUInt16(serverVersion.Minor);
            WriteDualStringArray(ref writer, encodedBindings);
        });
    }

    private static ScmReplyData DecodeScmReplyInfo(ReadOnlySpan<byte> body)
    {
        var reader = new NdrReader(body);
        if (reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("ScmReplyInfoData pdwReserved must be NULL.");
        }

        if (!reader.TryReadReferentId(out _))
        {
            throw new InvalidOperationException("ScmReplyInfoData remoteReply must be non-NULL.");
        }

        ulong oxid = reader.ReadUInt64();
        bool hasBindings = reader.TryReadReferentId(out _);
        Guid ipidRemUnknown = reader.ReadGuid();
        uint authnHint = reader.ReadUInt32();
        ushort major = reader.ReadUInt16();
        ushort minor = reader.ReadUInt16();
        if (!hasBindings)
        {
            throw new InvalidOperationException("customREMOTE_REPLY_SCM_INFO pdsaOxidBindings must be non-NULL.");
        }

        byte[] oxidBindings = ReadDualStringArray(ref reader);
        return new ScmReplyData(oxid, oxidBindings, ipidRemUnknown, authnHint, (major, minor));
    }

    private static byte[] EncodePropsOutInfo(IReadOnlyList<ActivationInterfaceResult> interfaceResults) =>
        EncodeTypeSerializationStream((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(checked((uint)interfaceResults.Count));
            _ = writer.WriteReferentId();
            _ = writer.WriteReferentId();
            _ = writer.WriteReferentId();

            writer.WriteConformanceHeader(interfaceResults.Count);
            for (int i = 0; i < interfaceResults.Count; i++)
            {
                writer.WriteGuid(interfaceResults[i].Iid);
            }

            writer.WriteConformanceHeader(interfaceResults.Count);
            for (int i = 0; i < interfaceResults.Count; i++)
            {
                writer.WriteInt32(interfaceResults[i].Hresult);
            }

            writer.WriteConformanceHeader(interfaceResults.Count);
            for (int i = 0; i < interfaceResults.Count; i++)
            {
                if (interfaceResults[i].ObjRef.Length == 0)
                {
                    writer.WriteNullReferent();
                }
                else
                {
                    _ = writer.WriteReferentId();
                }
            }

            for (int i = 0; i < interfaceResults.Count; i++)
            {
                if (interfaceResults[i].ObjRef.Length != 0)
                {
                    WriteMInterfacePointer(ref writer, interfaceResults[i].ObjRef);
                }
            }
        });

    private static ActivationInterfaceResult[] DecodePropsOutInfo(ReadOnlySpan<byte> body)
    {
        var reader = new NdrReader(body);
        uint cIfs = reader.ReadUInt32();
        if (cIfs is 0 or > MaxRequestedInterfaces)
        {
            throw new InvalidOperationException("PropsOutInfo cIfs is outside the allowed range.");
        }

        bool hasIids = reader.TryReadReferentId(out _);
        bool hasResults = reader.TryReadReferentId(out _);
        bool hasInterfaceData = reader.TryReadReferentId(out _);
        if (!hasIids || !hasResults || !hasInterfaceData)
        {
            throw new InvalidOperationException("PropsOutInfo arrays must be non-NULL.");
        }

        int iidCount = reader.ReadConformanceHeader();
        if (iidCount != cIfs)
        {
            throw new InvalidOperationException("PropsOutInfo piid count does not match cIfs.");
        }

        var iids = new Guid[iidCount];
        for (int i = 0; i < iids.Length; i++)
        {
            iids[i] = reader.ReadGuid();
        }

        int resultCount = reader.ReadConformanceHeader();
        if (resultCount != cIfs)
        {
            throw new InvalidOperationException("PropsOutInfo phresults count does not match cIfs.");
        }

        var hresults = new int[resultCount];
        for (int i = 0; i < hresults.Length; i++)
        {
            hresults[i] = reader.ReadInt32();
        }

        int dataCount = reader.ReadConformanceHeader();
        if (dataCount != cIfs)
        {
            throw new InvalidOperationException("PropsOutInfo ppIntfData count does not match cIfs.");
        }

        var hasObjRef = new bool[dataCount];
        for (int i = 0; i < hasObjRef.Length; i++)
        {
            hasObjRef[i] = reader.TryReadReferentId(out _);
        }

        var results = new ActivationInterfaceResult[dataCount];
        for (int i = 0; i < results.Length; i++)
        {
            byte[] objRef = hasObjRef[i] ? ReadMInterfacePointer(ref reader) : Array.Empty<byte>();
            results[i] = new ActivationInterfaceResult(iids[i], hresults[i], objRef);
        }

        return results;
    }

    private static byte[] EncodeTypeSerializationStream(NdrWriteAction action)
    {
        byte[] body = WritePayload(action);
        int paddedBodyLength = checked(body.Length + Padding(body.Length, 8));
        var result = new byte[16 + paddedBodyLength];
        result[0] = TypeSerializationVersion;
        result[1] = TypeSerializationLittleEndian;
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(2, sizeof(ushort)), CommonHeaderLength);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, sizeof(uint)), CommonHeaderFiller);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(8, sizeof(uint)), checked((uint)paddedBodyLength));
        body.CopyTo(result.AsSpan(16));
        return result;
    }

    private static ReadOnlySpan<byte> ReadTypeSerializationStream(ref NdrReader reader, out int streamSize)
    {
        int start = reader.Position;
        byte version = reader.ReadUInt8();
        byte endianness = reader.ReadUInt8();
        ushort headerLength = reader.ReadUInt16();
        uint filler = reader.ReadUInt32();
        if (version != TypeSerializationVersion || endianness != TypeSerializationLittleEndian || headerLength != CommonHeaderLength || filler != CommonHeaderFiller)
        {
            throw new InvalidOperationException("Unsupported NDR type serialization stream header.");
        }

        uint objectBufferLength = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        if (objectBufferLength > reader.RemainingBytes)
        {
            throw new InvalidOperationException("NDR type serialization object buffer exceeds the remaining payload.");
        }

        ReadOnlySpan<byte> body = reader.ReadRawBytes(checked((int)objectBufferLength));
        streamSize = reader.Position - start;
        if ((streamSize & 7) != 0)
        {
            throw new InvalidOperationException("NDR type serialization stream is not 8-byte aligned.");
        }

        return body;
    }

    private static byte[] EncodeCustomObjRef(Guid iid, Guid clsid, ReadOnlySpan<byte> objectData)
    {
        byte[] encodedObjectData = objectData.ToArray();
        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(ObjRefSignature);
            writer.WriteUInt32(ObjRefCustom);
            writer.WriteGuid(iid);
            writer.WriteGuid(clsid);
            writer.WriteUInt32(0);
            writer.WriteUInt32(checked((uint)encodedObjectData.Length + 8));
            writer.WriteRawBytes(encodedObjectData);
        });
    }

    private static ReadOnlySpan<byte> DecodeCustomObjRef(ReadOnlySpan<byte> objRef, Guid expectedIid, Guid expectedClsid)
    {
        var reader = new NdrReader(objRef);
        uint signature = reader.ReadUInt32();
        uint flags = reader.ReadUInt32();
        Guid iid = reader.ReadGuid();
        Guid clsid = reader.ReadGuid();
        _ = reader.ReadUInt32();
        uint objectReferenceSize = reader.ReadUInt32();
        if (signature != ObjRefSignature || flags != ObjRefCustom || iid != expectedIid || clsid != expectedClsid)
        {
            throw new InvalidOperationException("OBJREF_CUSTOM did not contain the expected activation-properties identifiers.");
        }

        if (objectReferenceSize != 0 && objectReferenceSize != reader.RemainingBytes + 8u)
        {
            throw new InvalidOperationException("OBJREF_CUSTOM ObjectReferenceSize does not match the custom object data length.");
        }

        return objRef.Slice(reader.Position);
    }

    private static void WriteMInterfacePointer(ref NdrWriter writer, ReadOnlySpan<byte> objRef)
    {
        writer.WriteUInt32(checked((uint)objRef.Length));
        writer.WriteUInt32(checked((uint)objRef.Length));
        writer.WriteRawBytes(objRef);
        writer.AlignTo(4);
    }

    private static byte[] ReadMInterfacePointer(ref NdrReader reader)
    {
        uint maxCount = reader.ReadUInt32();
        uint count = reader.ReadUInt32();
        if (count > maxCount || count > reader.RemainingBytes)
        {
            throw new InvalidOperationException("MInterfacePointer byte count exceeds the remaining payload.");
        }

        byte[] objRef = reader.ReadRawBytes(checked((int)count)).ToArray();
        reader.AlignTo(4);
        return objRef;
    }

    private static void WriteDualStringArray(ref NdrWriter writer, ReadOnlySpan<byte> dualStringArray)
    {
        if (dualStringArray.IsEmpty)
        {
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            return;
        }

        writer.WriteRawBytes(dualStringArray);
        writer.AlignTo(4);
    }

    private static byte[] ReadDualStringArray(ref NdrReader reader)
    {
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

        throw new InvalidOperationException("Unable to encode the activation properties payload.");
    }

    private static int Padding(int length, int boundary)
    {
        int remainder = length & (boundary - 1);
        return remainder == 0 ? 0 : boundary - remainder;
    }

    private sealed record DecodedActivationBlob(Guid[] PropertyIds, byte[][] PropertyBodies);
    private sealed record ScmReplyData(ulong Oxid, byte[] OxidBindings, Guid IpidRemUnknown, uint AuthnHint, (ushort Major, ushort Minor) ServerVersion);
}

/// <summary>
/// Decoded ActivationPropertiesIn fields needed by managed activation dispatch.
/// </summary>
public sealed record ActivationPropertiesInData(
    Guid ClassId,
    IReadOnlyList<Guid> RequestedIids,
    IReadOnlyList<ushort> RequestedProtocolSequences,
    uint ClientImpersonationLevel);

/// <summary>
/// Decoded RemoteCreateInstance request fields.
/// </summary>
public sealed record RemoteCreateInstanceActivationRequest(
    Guid ClassId,
    IReadOnlyList<Guid> RequestedIids,
    IReadOnlyList<ushort> RequestedProtocolSequences,
    byte[] ActivationPropertiesBlob)
{
    /// <summary>
    /// Empty sentinel used by TryDecode helpers.
    /// </summary>
    public static RemoteCreateInstanceActivationRequest Empty { get; } = new(Guid.Empty, Array.Empty<Guid>(), Array.Empty<ushort>(), Array.Empty<byte>());
}

/// <summary>
/// Decoded ActivationPropertiesOut fields returned by the SCM.
/// </summary>
public sealed record ActivationPropertiesOutData(
    ulong Oxid,
    byte[] OxidBindings,
    Guid IpidRemUnknown,
    uint AuthnHint,
    (ushort Major, ushort Minor) ServerVersion,
    IReadOnlyList<ActivationInterfaceResult> InterfaceResults)
{
    /// <summary>
    /// Empty sentinel used by TryDecode helpers.
    /// </summary>
    public static ActivationPropertiesOutData Empty { get; } = new(0, Array.Empty<byte>(), Guid.Empty, 0, default, Array.Empty<ActivationInterfaceResult>());
}

/// <summary>
/// Per-interface PropsOutInfo result.
/// </summary>
public sealed record ActivationInterfaceResult(Guid Iid, int Hresult, byte[] ObjRef);
