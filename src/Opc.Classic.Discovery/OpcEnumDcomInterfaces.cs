//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1707, MA0048 // OPC IDL naming preserved; grouped internal proxy types share this file

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;

namespace Opc.Classic.Discovery.Dcom;

internal sealed class IOPCServerListClientProxy
{
    private static readonly Guid InterfaceId = OpcGuids.IID_IOPCServerList;
    private readonly ICallChannel _channel;

    public IOPCServerListClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public Task<IOpcInterfaceRef> EnumClassesOfCategoriesAsync(
        Guid[] implementedCategories,
        Guid[] requiredCategories,
        CancellationToken cancellationToken = default) =>
        OpcEnumProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            InterfaceId,
            opnum: 3,
            OpcEnumProxyCodec.EncodeCategoryRequest(implementedCategories, requiredCategories),
            "IOPCServerList::EnumClassesOfCategories",
            cancellationToken);

    public async Task<OpcServerListClassDetails> GetClassDetailsAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<byte> payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteGuid(classId));
        NdrCallResult result = await OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            opnum: 4,
            payload,
            "IOPCServerList::GetClassDetails",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        return new OpcServerListClassDetails(
            reader.ReadUnicodeStringPtr(),
            reader.ReadUnicodeStringPtr(),
            null);
    }
}

internal sealed class IOPCServerList2ClientProxy
{
    private static readonly Guid InterfaceId = OpcGuids.IID_IOPCServerList2;
    private readonly ICallChannel _channel;

    public IOPCServerList2ClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public Task<IOpcInterfaceRef> EnumClassesOfCategoriesAsync(
        Guid[] implementedCategories,
        Guid[] requiredCategories,
        CancellationToken cancellationToken = default) =>
        OpcEnumProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            InterfaceId,
            opnum: 3,
            OpcEnumProxyCodec.EncodeCategoryRequest(implementedCategories, requiredCategories),
            "IOPCServerList2::EnumClassesOfCategories",
            cancellationToken);

    public async Task<OpcServerListClassDetails> GetClassDetailsAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<byte> payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteGuid(classId));
        NdrCallResult result = await OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            opnum: 4,
            payload,
            "IOPCServerList2::GetClassDetails",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        return new OpcServerListClassDetails(
            reader.ReadUnicodeStringPtr(),
            reader.ReadUnicodeStringPtr(),
            reader.ReadUnicodeStringPtr());
    }
}

internal sealed class IOPCEnumGUIDClientProxy
{
    private static readonly Guid InterfaceId = OpcGuids.IID_IOPCEnumGUID;
    private readonly ICallChannel _channel;

    public IOPCEnumGUIDClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public async Task<OpcEnumGuidNextResult> NextAsync(int count, CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<byte> payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteInt32(count));
        NdrCallResult result = await OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            opnum: 3,
            payload,
            "IOPCEnumGUID::Next",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        Guid[] classIds = reader.ReadConformantGuidArray();
        int fetched = reader.ReadInt32();
        return new OpcEnumGuidNextResult(classIds, fetched);
    }
}

internal sealed record OpcServerListClassDetails(string? ProgId, string? UserType, string? VerIndProgId);

internal sealed record OpcEnumGuidNextResult(Guid[] ClassIds, int Fetched);

internal static class OpcEnumProxyCodec
{
    private const int DefaultPayloadSize = 1024;
    private const int MaximumPayloadSize = 65536;

    internal delegate void NdrWriteAction(ref NdrWriter writer);

    public static ReadOnlyMemory<byte> EncodeCategoryRequest(Guid[] implementedCategories, Guid[] requiredCategories) =>
        WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteConformantGuidArray(implementedCategories ?? Array.Empty<Guid>());
            writer.WriteConformantGuidArray(requiredCategories ?? Array.Empty<Guid>());
        });

    public static async Task<IOpcInterfaceRef> InvokeInterfaceRefAsync(
        ICallChannel channel,
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> payload,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        NdrCallResult result = await InvokeAsync(channel, interfaceId, opnum, payload, operationDescription, cancellationToken)
            .ConfigureAwait(false);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return OpcInterfaceRefCodec.Read(ref reader);
    }

    public static async Task<NdrCallResult> InvokeAsync(
        ICallChannel channel,
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> payload,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        NdrCallResult result = await channel.InvokeAsync(interfaceId, opnum, payload, cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), operationDescription);
        return result;
    }

    public static byte[] WritePayload(NdrWriteAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        for (int size = DefaultPayloadSize; size <= MaximumPayloadSize; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                action(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < MaximumPayloadSize)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the OPCEnum DCOM payload.");
    }
}
