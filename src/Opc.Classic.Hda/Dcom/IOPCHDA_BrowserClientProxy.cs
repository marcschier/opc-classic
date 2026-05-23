//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCHDA_Browser with underscore)

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Dcom;

/// <summary>Client proxy for <see cref="IOPCHDA_Browser" /> methods that return COM interface references.</summary>
public sealed class IOPCHDA_BrowserClientProxy : IOPCHDA_Browser
{
    private const int InitialBufferSize = 8192;

    private readonly ICallChannel _channel;

    /// <summary>Initializes a new instance of the <see cref="IOPCHDA_BrowserClientProxy" /> class.</summary>
    public IOPCHDA_BrowserClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    /// <inheritdoc />
    public async Task<IOpcInterfaceRef> GetEnumAsync(int browseType, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[InitialBufferSize];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(browseType);

        NdrCallResult result = await InvokeAsync(IOPCHDA_Browser.Opnums.GetEnumAsync, buffer.AsMemory(0, writer.Position), cancellationToken).ConfigureAwait(false);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return OpcInterfaceRefCodec.Read(ref reader);
    }

    /// <inheritdoc />
    public async Task ChangeBrowsePositionAsync(int browseDirection, string browseString, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[InitialBufferSize];
        var writer = new NdrWriter(buffer);
        writer.WriteInt32(browseDirection);
        writer.WriteUnicodeStringPtr(browseString);

        _ = await InvokeAsync(IOPCHDA_Browser.Opnums.ChangeBrowsePositionAsync, buffer.AsMemory(0, writer.Position), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> GetItemIDAsync(string node, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[InitialBufferSize];
        var writer = new NdrWriter(buffer);
        writer.WriteUnicodeStringPtr(node);

        NdrCallResult result = await InvokeAsync(IOPCHDA_Browser.Opnums.GetItemIDAsync, buffer.AsMemory(0, writer.Position), cancellationToken).ConfigureAwait(false);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadUnicodeStringPtr()!;
    }

    /// <inheritdoc />
    public async Task<string> GetBranchPositionAsync(CancellationToken cancellationToken = default)
    {
        NdrCallResult result = await InvokeAsync(IOPCHDA_Browser.Opnums.GetBranchPositionAsync, ReadOnlyMemory<byte>.Empty, cancellationToken).ConfigureAwait(false);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadUnicodeStringPtr()!;
    }

    private async Task<NdrCallResult> InvokeAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        NdrCallResult result = await _channel.InvokeAsync(IOPCHDA_Browser.InterfaceId, opnum, payload, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }

        return result;
    }
}
