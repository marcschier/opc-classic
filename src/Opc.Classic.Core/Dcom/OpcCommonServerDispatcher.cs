// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom;

/// <summary>
/// Server-side dispatcher for OPC Common <c>IOPCCommon</c> methods.
/// </summary>
public sealed class OpcCommonServerDispatcher : IOpcServerDispatcher
{
    private readonly IOpcCommonServer _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcCommonServerDispatcher" /> class.
    /// </summary>
    public OpcCommonServerDispatcher(IOpcCommonServer server) =>
        _server = server ?? throw new ArgumentNullException(nameof(server));

    /// <inheritdoc />
    public async ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return opnum switch
            {
                OpcCommonClientProxy.Opnums.SetLocaleId => await DispatchSetLocaleIdAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                OpcCommonClientProxy.Opnums.GetLocaleId => await DispatchGetLocaleIdAsync(cancellationToken).ConfigureAwait(false),
                OpcCommonClientProxy.Opnums.QueryAvailableLocaleIds => await DispatchQueryAvailableLocaleIdsAsync(cancellationToken).ConfigureAwait(false),
                OpcCommonClientProxy.Opnums.GetErrorString => await DispatchGetErrorStringAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                OpcCommonClientProxy.Opnums.SetClientName => await DispatchSetClientNameAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                _ => DispatchResult.NotImplemented(opnum),
            };
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
    }

    private async ValueTask<DispatchResult> DispatchSetLocaleIdAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int localeId = reader.ReadInt32();
        await _server.SetLocaleIdAsync(localeId, cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(Array.Empty<byte>());
    }

    private async ValueTask<DispatchResult> DispatchGetLocaleIdAsync(CancellationToken cancellationToken)
    {
        int localeId = await _server.GetLocaleIdAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(WritePayload((ref NdrWriter writer) => writer.WriteInt32(localeId)));
    }

    private async ValueTask<DispatchResult> DispatchQueryAvailableLocaleIdsAsync(CancellationToken cancellationToken)
    {
        int[] localeIds = await _server.QueryAvailableLocaleIdsAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(WritePayload((ref NdrWriter writer) => writer.WriteConformantInt32Array(localeIds)));
    }

    private async ValueTask<DispatchResult> DispatchGetErrorStringAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int errorCode = reader.ReadInt32();
        string errorString = await _server.GetErrorStringAsync(errorCode, cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(errorString)));
    }

    private async ValueTask<DispatchResult> DispatchSetClientNameAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        string clientName = reader.ReadUnicodeStringPtr() ?? string.Empty;
        await _server.SetClientNameAsync(clientName, cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(Array.Empty<byte>());
    }

    private static byte[] WritePayload(NdrWriteAction write)
    {
        ArgumentNullException.ThrowIfNull(write);

        for (int size = 256; size <= 8192; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                write(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < 8192)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the IOPCCommon DCOM payload.");
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
