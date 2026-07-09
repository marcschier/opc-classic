// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using System.Runtime.InteropServices;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Minimal MS-RPCE endpoint-mapper responder for ept_map over ncacn_ip_tcp.
/// </summary>
public sealed class EndpointMapperDispatcher : IRpcRawNdrDispatcher
{
    public static readonly Guid InterfaceId = new("e1af8308-5d1f-11c9-91a4-08002b14a0fa");

    public const int EndpointMapperPort = 135;
    public const uint EptSuccess = 0x00000000;
    public const uint EptNotRegistered = 0x16C9A0D6;

    private const int EptMapOpnum = 3;
    private const int ContextHandleWireSize = 20;
    private const int MaxTowers = 500;
    private const int RpcSProcnumOutOfRange = unchecked((int)0x800706D1u);
    private const int EInvalidArg = unchecked((int)0x80070057u);

    private readonly Func<IPEndPoint?> _endpointProvider;
    private readonly HashSet<Guid> _mappedInterfaces;

    public EndpointMapperDispatcher(Func<IPEndPoint?> endpointProvider)
        : this(endpointProvider, DefaultMappedInterfaces())
    {
    }

    public EndpointMapperDispatcher(Func<IPEndPoint?> endpointProvider, IEnumerable<Guid> mappedInterfaces)
    {
        ArgumentNullException.ThrowIfNull(endpointProvider);
        ArgumentNullException.ThrowIfNull(mappedInterfaces);
        _endpointProvider = endpointProvider;
        _mappedInterfaces = new HashSet<Guid>(mappedInterfaces);
    }

    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return opnum == EptMapOpnum
            ? new ValueTask<DispatchResult>(Map(requestPayload.Span))
            : new ValueTask<DispatchResult>(DispatchResult.Fault(RpcSProcnumOutOfRange));
    }

    private DispatchResult Map(ReadOnlySpan<byte> request)
    {
        if (!TryDecodeMapRequest(request, out EndpointMapperMapRequest mapRequest))
        {
            return DispatchResult.Fault(EInvalidArg);
        }

        if (mapRequest.MaxTowers == 0
            || !_mappedInterfaces.Contains(mapRequest.RequestedInterfaceId)
            || _endpointProvider() is not IPEndPoint endpoint)
        {
            return DispatchResult.Success(EncodeMapResponse([], mapRequest.MaxTowers, EptNotRegistered));
        }

        byte[] tower = EndpointMapperTower.EncodeTcpTower(
            mapRequest.RequestedInterfaceId,
            mapRequest.RequestedInterfaceMajorVersion,
            mapRequest.RequestedInterfaceMinorVersion,
            endpoint);
        return DispatchResult.Success(EncodeMapResponse([tower], mapRequest.MaxTowers, EptSuccess));
    }

    private static bool TryDecodeMapRequest(ReadOnlySpan<byte> request, out EndpointMapperMapRequest mapRequest)
    {
        mapRequest = default;
        if (request.Length < 8)
        {
            return false;
        }

        uint objReferent = BinaryPrimitives.ReadUInt32LittleEndian(request[..4]);
        uint maxTowers = TryReadMaxTowers(request, objReferent != 0);
        if (maxTowers > MaxTowers)
        {
            return false;
        }

        if (!TryFindMapTower(request, out byte[] tower)
            || !EndpointMapperTower.TryDecodeTcpTower(tower, out EndpointMapperTowerBinding binding))
        {
            return false;
        }

        mapRequest = new EndpointMapperMapRequest(
            binding.InterfaceId,
            binding.InterfaceMajorVersion,
            binding.InterfaceMinorVersion,
            maxTowers);
        return true;
    }

    private static uint TryReadMaxTowers(ReadOnlySpan<byte> request, bool hasObject)
    {
        int deferredOffset = 8 + ContextHandleWireSize;
        if (deferredOffset + 4 <= request.Length)
        {
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(request.Slice(deferredOffset, 4));
            if (value <= MaxTowers)
            {
                return value;
            }
        }

        int inlineOffset = 8;
        if (hasObject)
        {
            inlineOffset += 16;
        }

        if (EndpointMapperTower.TryReadTower(request, inlineOffset, out _, out int towerBytes))
        {
            int maxOffset = inlineOffset + towerBytes + ContextHandleWireSize;
            if (maxOffset + 4 <= request.Length)
            {
                return BinaryPrimitives.ReadUInt32LittleEndian(request.Slice(maxOffset, 4));
            }
        }

        return 1;
    }

    private static bool TryFindMapTower(ReadOnlySpan<byte> request, out byte[] tower)
    {
        for (int offset = 0; offset <= request.Length - 4; offset += 4)
        {
            if (EndpointMapperTower.TryReadTower(request, offset, out tower, out _))
            {
                return true;
            }
        }

        tower = [];
        return false;
    }

    private static byte[] EncodeMapResponse(IReadOnlyList<byte[]> towers, uint maxTowers, uint status)
    {
        uint actualMaxTowers = Math.Min(maxTowers, MaxTowers);
        uint numTowers = Math.Min(unchecked((uint)towers.Count), actualMaxTowers);
        int towerBytes = 0;
        for (int i = 0; i < numTowers; i++)
        {
            towerBytes += 8 + towers[i].Length + PaddingTo4(towers[i].Length);
        }

        int arrayHeaderBytes = actualMaxTowers == 0 ? 0 : 12 + checked((int)numTowers) * 4;
        var buffer = new byte[ContextHandleWireSize + 4 + 4 + arrayHeaderBytes + towerBytes + 4];
        var writer = new NdrWriter(buffer);

        writer.WriteRawBytes(new byte[ContextHandleWireSize]);
        writer.WriteUInt32(numTowers);
        if (actualMaxTowers == 0)
        {
            writer.WriteNullReferent();
        }
        else
        {
            _ = writer.WriteReferentId();
            writer.WriteUInt32(actualMaxTowers);
            writer.WriteUInt32(0);
            writer.WriteUInt32(numTowers);
            for (int i = 0; i < numTowers; i++)
            {
                _ = writer.WriteReferentId();
            }

            for (int i = 0; i < numTowers; i++)
            {
                EndpointMapperTower.WriteTowerPointee(ref writer, towers[i]);
            }
        }

        writer.WriteUInt32(status);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static int PaddingTo4(int length)
    {
        int misaligned = length & 3;
        return misaligned == 0 ? 0 : 4 - misaligned;
    }

    private static Guid[] DefaultMappedInterfaces() =>
    [
        Guid.Parse(Interfaces.IID_IRemoteSCMActivator),
        Guid.Parse(Interfaces.IID_IActivation),
        OpcGuids.IID_IObjectExporter,
    ];

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct EndpointMapperMapRequest(
        Guid RequestedInterfaceId,
        ushort RequestedInterfaceMajorVersion,
        ushort RequestedInterfaceMinorVersion,
        uint MaxTowers);
}
