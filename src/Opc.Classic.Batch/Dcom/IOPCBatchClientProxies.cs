//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCBatchServer)
#pragma warning disable MA0048 // Related Batch proxy shims are grouped for readability

using Opc.Classic.Batch.Ndr;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Batch.Dcom;

public sealed class IOPCBatchServerClientProxy : IOPCBatchServer
{
    private readonly ICallChannel _channel;

    public IOPCBatchServerClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public async Task<string> GetDelimiterAsync(CancellationToken cancellationToken = default)
    {
        NdrCallResult result = await OpcBatchProxyCodec.InvokeAsync(
            _channel,
            IOPCBatchServer.InterfaceId,
            IOPCBatchServer.Opnums.GetDelimiterAsync,
            ReadOnlyMemory<byte>.Empty,
            "IOPCBatchServer::GetDelimiter",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadUnicodeStringPtr()!;
    }

    public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid riid, CancellationToken cancellationToken = default) =>
        OpcBatchProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            IOPCBatchServer.InterfaceId,
            IOPCBatchServer.Opnums.CreateEnumeratorAsync,
            OpcBatchProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteGuid(riid)),
            "IOPCBatchServer::CreateEnumerator",
            cancellationToken);
}

public sealed class IOPCBatchServer2ClientProxy : IOPCBatchServer2
{
    private readonly ICallChannel _channel;

    public IOPCBatchServer2ClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public Task<IOpcInterfaceRef> CreateFilteredEnumeratorAsync(
        Guid riid,
        OpcBatchSummaryFilter filter,
        string model,
        CancellationToken cancellationToken = default) =>
        OpcBatchProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            IOPCBatchServer2.InterfaceId,
            IOPCBatchServer2.Opnums.CreateFilteredEnumeratorAsync,
            OpcBatchProxyCodec.WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteGuid(riid);
                NdrOpcBatchSummaryFilterCodec.Write(ref writer, filter);
                writer.WriteUnicodeStringPtr(model);
            }),
            "IOPCBatchServer2::CreateFilteredEnumerator",
            cancellationToken);
}

public sealed class IEnumOPCBatchSummaryClientProxy : IEnumOPCBatchSummary
{
    private readonly ICallChannel _channel;

    public IEnumOPCBatchSummaryClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public async Task<OpcBatchSummary[]> NextAsync(int count, CancellationToken cancellationToken = default)
    {
        NdrCallResult result = await OpcBatchProxyCodec.InvokeAsync(
            _channel,
            IEnumOPCBatchSummary.InterfaceId,
            IEnumOPCBatchSummary.Opnums.NextAsync,
            OpcBatchProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteInt32(count)),
            "IEnumOPCBatchSummary::Next",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        return OpcBatchProxyCodec.ReadArray(ref reader, NdrOpcBatchSummaryCodec.Read);
    }

    public Task SkipAsync(int count, CancellationToken cancellationToken = default) =>
        OpcBatchProxyCodec.InvokeNoResultAsync(
            _channel,
            IEnumOPCBatchSummary.InterfaceId,
            IEnumOPCBatchSummary.Opnums.SkipAsync,
            OpcBatchProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteInt32(count)),
            "IEnumOPCBatchSummary::Skip",
            cancellationToken);

    public Task ResetAsync(CancellationToken cancellationToken = default) =>
        OpcBatchProxyCodec.InvokeNoResultAsync(
            _channel,
            IEnumOPCBatchSummary.InterfaceId,
            IEnumOPCBatchSummary.Opnums.ResetAsync,
            ReadOnlyMemory<byte>.Empty,
            "IEnumOPCBatchSummary::Reset",
            cancellationToken);

    public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default) =>
        OpcBatchProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            IEnumOPCBatchSummary.InterfaceId,
            IEnumOPCBatchSummary.Opnums.CloneAsync,
            ReadOnlyMemory<byte>.Empty,
            "IEnumOPCBatchSummary::Clone",
            cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        NdrCallResult result = await OpcBatchProxyCodec.InvokeAsync(
            _channel,
            IEnumOPCBatchSummary.InterfaceId,
            IEnumOPCBatchSummary.Opnums.CountAsync,
            ReadOnlyMemory<byte>.Empty,
            "IEnumOPCBatchSummary::Count",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadInt32();
    }
}

public sealed class IOPCBatchServerServerDispatcher : IOpcServerDispatcher
{
    private readonly IOPCBatchServer _impl;

    public IOPCBatchServerServerDispatcher(IOPCBatchServer impl) =>
        _impl = impl ?? throw new ArgumentNullException(nameof(impl));

    public async ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return opnum switch
            {
                IOPCBatchServer.Opnums.GetDelimiterAsync => await DispatchGetDelimiterAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                IOPCBatchServer.Opnums.CreateEnumeratorAsync => await DispatchCreateEnumeratorAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                _ => DispatchResult.NotImplemented(opnum),
            };
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
    }

    private async ValueTask<DispatchResult> DispatchGetDelimiterAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        _ = requestPayload;
        string delimiter = await _impl.GetDelimiterAsync(cancellationToken).ConfigureAwait(false);
        return OpcBatchProxyCodec.Success((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(delimiter));
    }

    private async ValueTask<DispatchResult> DispatchCreateEnumeratorAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        Guid riid = reader.ReadGuid();
        IOpcInterfaceRef interfaceRef = await _impl.CreateEnumeratorAsync(riid, cancellationToken).ConfigureAwait(false);
        return OpcBatchProxyCodec.Success((ref NdrWriter writer) => OpcBatchProxyCodec.WriteInterfaceRef(ref writer, interfaceRef));
    }
}

public sealed class IOPCBatchServer2ServerDispatcher : IOpcServerDispatcher
{
    private readonly IOPCBatchServer2 _impl;

    public IOPCBatchServer2ServerDispatcher(IOPCBatchServer2 impl) =>
        _impl = impl ?? throw new ArgumentNullException(nameof(impl));

    public async ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return opnum switch
            {
                IOPCBatchServer2.Opnums.CreateFilteredEnumeratorAsync => await DispatchCreateFilteredEnumeratorAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                _ => DispatchResult.NotImplemented(opnum),
            };
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
    }

    private async ValueTask<DispatchResult> DispatchCreateFilteredEnumeratorAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        Guid riid = reader.ReadGuid();
        OpcBatchSummaryFilter filter = NdrOpcBatchSummaryFilterCodec.Read(ref reader);
        string model = reader.ReadUnicodeStringPtr()!;
        IOpcInterfaceRef interfaceRef = await _impl.CreateFilteredEnumeratorAsync(riid, filter, model, cancellationToken).ConfigureAwait(false);
        return OpcBatchProxyCodec.Success((ref NdrWriter writer) => OpcBatchProxyCodec.WriteInterfaceRef(ref writer, interfaceRef));
    }
}

public sealed class IEnumOPCBatchSummaryServerDispatcher : IOpcServerDispatcher
{
    private readonly IEnumOPCBatchSummary _impl;

    public IEnumOPCBatchSummaryServerDispatcher(IEnumOPCBatchSummary impl) =>
        _impl = impl ?? throw new ArgumentNullException(nameof(impl));

    public async ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return opnum switch
            {
                IEnumOPCBatchSummary.Opnums.NextAsync => await DispatchNextAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                IEnumOPCBatchSummary.Opnums.SkipAsync => await DispatchSkipAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                IEnumOPCBatchSummary.Opnums.ResetAsync => await DispatchResetAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                IEnumOPCBatchSummary.Opnums.CloneAsync => await DispatchCloneAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                IEnumOPCBatchSummary.Opnums.CountAsync => await DispatchCountAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                _ => DispatchResult.NotImplemented(opnum),
            };
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
    }

    private async ValueTask<DispatchResult> DispatchNextAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int count = reader.ReadInt32();
        OpcBatchSummary[] summaries = await _impl.NextAsync(count, cancellationToken).ConfigureAwait(false);
        return OpcBatchProxyCodec.Success((ref NdrWriter writer) =>
        {
            writer.WriteUInt32((uint)(summaries?.Length ?? 0));
            if (summaries is not null)
            {
                foreach (OpcBatchSummary summary in summaries)
                {
                    NdrOpcBatchSummaryCodec.Write(ref writer, summary);
                }
            }
        });
    }

    private async ValueTask<DispatchResult> DispatchSkipAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int count = reader.ReadInt32();
        await _impl.SkipAsync(count, cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(Array.Empty<byte>());
    }

    private async ValueTask<DispatchResult> DispatchResetAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        _ = requestPayload;
        await _impl.ResetAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(Array.Empty<byte>());
    }

    private async ValueTask<DispatchResult> DispatchCloneAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        _ = requestPayload;
        IOpcInterfaceRef interfaceRef = await _impl.CloneAsync(cancellationToken).ConfigureAwait(false);
        return OpcBatchProxyCodec.Success((ref NdrWriter writer) => OpcBatchProxyCodec.WriteInterfaceRef(ref writer, interfaceRef));
    }

    private async ValueTask<DispatchResult> DispatchCountAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        _ = requestPayload;
        int count = await _impl.CountAsync(cancellationToken).ConfigureAwait(false);
        return OpcBatchProxyCodec.Success((ref NdrWriter writer) => writer.WriteInt32(count));
    }
}

internal static class OpcBatchProxyCodec
{
    private const int DefaultPayloadSize = 1024;
    private const int MaximumPayloadSize = 65536;

    internal delegate void NdrWriteAction(ref NdrWriter writer);

    internal delegate T NdrReadFunc<T>(ref NdrReader reader);

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

    public static async Task InvokeNoResultAsync(
        ICallChannel channel,
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> payload,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        _ = await InvokeAsync(channel, interfaceId, opnum, payload, operationDescription, cancellationToken).ConfigureAwait(false);
    }

    public static DispatchResult Success(NdrWriteAction action) =>
        DispatchResult.Success(WritePayload(action).ToArray());

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

    public static void WriteInterfaceRef(ref NdrWriter writer, IOpcInterfaceRef interfaceRef) =>
        OpcInterfaceRefCodec.Write(ref writer, interfaceRef);

    public static ReadOnlyMemory<byte> WritePayload(NdrWriteAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        for (int size = DefaultPayloadSize; size <= MaximumPayloadSize; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                action(ref writer);
                return buffer.AsMemory(0, writer.Position);
            }
            catch (InvalidOperationException) when (size < MaximumPayloadSize)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the OPC Batch DCOM payload.");
    }

    public static T[] ReadArray<T>(ref NdrReader reader, NdrReadFunc<T> read)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new T[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = read(ref reader);
        }

        return values;
    }
}
