// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Dcom;

public sealed class IConnectionPointClientProxy : IConnectionPoint
{
    private const uint ObjRefSignature = 0x574F454D;
    private const uint ObjRefStandard = 0x00000001;
    private const int ObjRefStandardHeaderSize = 68;

    private readonly ICallChannel _channel;

    public IConnectionPointClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public async Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default)
    {
        NdrCallResult result = await _channel.InvokeAsync(
            IConnectionPoint.InterfaceId,
            IConnectionPoint.Opnums.GetConnectionInterfaceAsync,
            ReadOnlyMemory<byte>.Empty,
            cancellationToken).ConfigureAwait(false);

        ThrowIfFailed(result);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadGuid();
    }

    public async Task<int> AdviseAsync(IOpcInterfaceRef sink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);

        ReadOnlyMemory<byte> payload = EncodeInterfaceRef(sink);
        NdrCallResult result = await _channel.InvokeAsync(
            IConnectionPoint.InterfaceId,
            IConnectionPoint.Opnums.AdviseAsync,
            payload,
            cancellationToken).ConfigureAwait(false);

        ThrowIfFailed(result);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadInt32();
    }

    public async Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[4];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(cookie);

        NdrCallResult result = await _channel.InvokeAsync(
            IConnectionPoint.InterfaceId,
            IConnectionPoint.Opnums.UnadviseAsync,
            buffer,
            cancellationToken).ConfigureAwait(false);

        ThrowIfFailed(result);
    }

    private static ReadOnlyMemory<byte> EncodeInterfaceRef(IOpcInterfaceRef interfaceRef)
    {
        int bindingCount = interfaceRef.ResolverBindings.Count;
        if (bindingCount > ushort.MaxValue)
        {
            throw new ArgumentException("Interface reference has too many resolver binding entries.", nameof(interfaceRef));
        }

        var buffer = new byte[checked(ObjRefStandardHeaderSize + (bindingCount * 2))];
        var writer = new NdrWriter(buffer);
        writer.WriteUInt32(ObjRefSignature);
        writer.WriteUInt32(ObjRefStandard);
        writer.WriteGuid(interfaceRef.Iid);
        writer.WriteUInt32(interfaceRef.Flags);
        writer.WriteUInt32(interfaceRef.PublicRefs);
        writer.WriteUInt64(interfaceRef.Oxid);
        writer.WriteUInt64(interfaceRef.Oid);
        writer.WriteGuid(interfaceRef.Ipid);
        writer.WriteUInt16((ushort)bindingCount);
        writer.WriteUInt16(interfaceRef.SecurityOffset);
        foreach (ushort binding in interfaceRef.ResolverBindings)
        {
            writer.WriteUInt16(binding);
        }

        return buffer.AsMemory(0, writer.Position);
    }

    private static void ThrowIfFailed(NdrCallResult result)
    {
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }
    }
}
