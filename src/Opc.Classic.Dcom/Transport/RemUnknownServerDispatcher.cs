// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Server-side dispatcher for <c>IRemUnknown</c>/<c>IRemUnknown2</c>.
/// </summary>
public sealed class RemUnknownServerDispatcher : IRpcRequestContextDispatcher
{
    /// <summary><c>IRemUnknown</c> interface identifier.</summary>
    public static readonly Guid InterfaceId = OpcGuids.IID_IRemUnknown;

    /// <summary><c>IRemUnknown2</c> interface identifier.</summary>
    public static readonly Guid InterfaceId2 = OpcGuids.IID_IRemUnknown2;

    private const int ENoInterface = unchecked((int)0x80004002u);
    private const int CoEObjectNotRegistered = unchecked((int)0x800401FBu);
    private const int EInvalidArg = unchecked((int)0x80070057u);
    private const int RpcSProcnumOutOfRange = unchecked((int)0x800706D1u);
    private const int MaxInterfaceRefs = 0x8000;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly ulong _oxid;

    /// <summary>Initializes a new instance of the <see cref="RemUnknownServerDispatcher" /> class.</summary>
    public RemUnknownServerDispatcher(OpcObjectRegistry objectRegistry)
    {
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _oxid = UInt64FromGuid(Guid.NewGuid());
    }

    /// <inheritdoc />
    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(opnum, requestPayload, isEstablished: true, cancellationToken);

    ValueTask<DispatchResult> IRpcRequestContextDispatcher.DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        RpcRequestContext requestContext,
        CancellationToken cancellationToken) =>
        DispatchAsync(opnum, requestPayload, requestContext.IsEstablished, cancellationToken);

    private ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        bool isEstablished,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!isEstablished)
        {
            return new ValueTask<DispatchResult>(DispatchResult.Fault(global::Opc.Classic.OpcResultId.AccessDenied.Code));
        }

        try
        {
            return opnum switch
            {
                3 => new ValueTask<DispatchResult>(RemQueryInterface(requestPayload.Span)),
                4 => new ValueTask<DispatchResult>(RemAddRef(requestPayload.Span)),
                5 => new ValueTask<DispatchResult>(RemRelease(requestPayload.Span)),
                _ => new ValueTask<DispatchResult>(DispatchResult.Fault(RpcSProcnumOutOfRange)),
            };
        }
        catch (InvalidOperationException)
        {
            return new ValueTask<DispatchResult>(DispatchResult.Fault(EInvalidArg));
        }
    }

    private DispatchResult RemQueryInterface(ReadOnlySpan<byte> request)
    {
        var reader = new NdrReader(request);
        Guid ripid = reader.ReadGuid();
        uint cRefs = reader.ReadUInt32();
        ushort cIids = reader.ReadUInt16();
        int iidCount = reader.ReadConformanceHeader();
        if (iidCount != cIids || iidCount > MaxInterfaceRefs)
        {
            return DispatchResult.Fault(EInvalidArg);
        }

        var requestedIids = new Guid[iidCount];
        for (int i = 0; i < requestedIids.Length; i++)
        {
            requestedIids[i] = reader.ReadGuid();
        }

        if (!_objectRegistry.TryGetInterfaceDispatchers(ripid, out IReadOnlyDictionary<Guid, IOpcServerDispatcher>? dispatchers))
        {
            return DispatchResult.Fault(CoEObjectNotRegistered);
        }

        var results = new OpcRemQIResult[iidCount];
        for (int i = 0; i < requestedIids.Length; i++)
        {
            Guid iid = requestedIids[i];
            if (iid == OpcGuids.IID_IUnknown || dispatchers.ContainsKey(iid))
            {
                if (cRefs != 0)
                {
                    _objectRegistry.AddPublicRefs(ripid, cRefs);
                }

                results[i] = new OpcRemQIResult(0, flags: 0, publicRefs: cRefs, oxid: _oxid, oid: UInt64FromGuid(ripid), ipid: ripid);
            }
            else
            {
                results[i] = new OpcRemQIResult(ENoInterface, flags: 0, publicRefs: 0, oxid: 0, oid: 0, ipid: Guid.Empty);
            }
        }

        return DispatchResult.Success(WritePayload((ref NdrWriter writer) =>
        {
            _ = writer.WriteReferentId();
            writer.WriteConformanceHeader(results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                NdrRemQIResultCodec.Write(ref writer, results[i]);
            }
        }));
    }

    private DispatchResult RemAddRef(ReadOnlySpan<byte> request)
    {
        RemInterfaceRef[] refs = ReadInterfaceRefs(request);
        var results = new int[refs.Length];
        for (int i = 0; i < refs.Length; i++)
        {
            if (!_objectRegistry.Contains(refs[i].Ipid))
            {
                results[i] = CoEObjectNotRegistered;
                continue;
            }

            uint publicRefs = refs[i].PublicRefs;
            _ = refs[i].PrivateRefs;
            if (publicRefs != 0)
            {
                if (!_objectRegistry.AddPublicRefs(refs[i].Ipid, publicRefs))
                {
                    results[i] = CoEObjectNotRegistered;
                    continue;
                }
            }

            results[i] = 0;
        }

        return DispatchResult.Success(WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteConformanceHeader(results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                writer.WriteInt32(results[i]);
            }
        }));
    }

    private DispatchResult RemRelease(ReadOnlySpan<byte> request)
    {
        RemInterfaceRef[] refs = ReadInterfaceRefs(request);
        for (int i = 0; i < refs.Length; i++)
        {
            if (!_objectRegistry.Contains(refs[i].Ipid))
            {
                continue;
            }

            uint releaseCount = refs[i].PublicRefs;
            _ = refs[i].PrivateRefs;
            if (releaseCount == 0)
            {
                continue;
            }

            _objectRegistry.ReleasePublicRefs(refs[i].Ipid, releaseCount);
        }

        return DispatchResult.Success(ReadOnlyMemory<byte>.Empty);
    }

    private static RemInterfaceRef[] ReadInterfaceRefs(ReadOnlySpan<byte> request)
    {
        var reader = new NdrReader(request);
        ushort count = reader.ReadUInt16();
        int encodedCount = reader.ReadConformanceHeader();
        if (encodedCount != count || encodedCount > MaxInterfaceRefs)
        {
            throw new InvalidOperationException("IRemUnknown interface-ref array count is invalid.");
        }

        var refs = new RemInterfaceRef[encodedCount];
        for (int i = 0; i < refs.Length; i++)
        {
            Guid ipid = reader.ReadGuid();
            uint publicRefs = reader.ReadUInt32();
            uint privateRefs = reader.ReadUInt32();
            refs[i] = new RemInterfaceRef(ipid, publicRefs, privateRefs);
        }

        return refs;
    }

    private static byte[] WritePayload(NdrWriteAction write)
    {
        for (int size = 256; size <= 1024 * 1024; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                write(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < 1024 * 1024)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the IRemUnknown DCOM payload.");
    }

    private static ulong UInt64FromGuid(Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        bool ok = value.TryWriteBytes(bytes);
        if (!ok)
        {
            throw new InvalidOperationException("Guid.TryWriteBytes failed unexpectedly.");
        }

        return BitConverter.ToUInt64(bytes);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct RemInterfaceRef
    {
        public RemInterfaceRef(Guid ipid, uint publicRefs, uint privateRefs)
        {
            Ipid = ipid;
            PublicRefs = publicRefs;
            PrivateRefs = privateRefs;
        }

        public Guid Ipid { get; }

        public uint PublicRefs { get; }

        public uint PrivateRefs { get; }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
