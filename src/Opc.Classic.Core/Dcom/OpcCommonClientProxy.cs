//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom;

/// <summary>Managed proxy for the OPC Common <c>IOPCCommon</c> debug/metadata methods.</summary>
public sealed class OpcCommonClientProxy
{
    private const int DefaultPayloadSize = 256;
    private const int MaximumPayloadSize = 8192;

    private readonly ICallChannel _channel;

    /// <summary>Initializes a new instance of the <see cref="OpcCommonClientProxy" /> class.</summary>
    public OpcCommonClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    /// <summary>OPC Common <c>IOPCCommon</c> interface identifier.</summary>
    public static Guid InterfaceId => OpcGuids.IID_IOPCCommon;

    /// <summary>OPC Common <c>IOPCCommon</c> DCE/RPC operation numbers.</summary>
    public static class Opnums
    {
        /// <summary><c>IOPCCommon::SetClientName</c> operation number.</summary>
        public const int SetClientName = 7;
    }

    /// <summary>
    /// Sets the optional client name that servers may use for diagnostics and debugging metadata.
    /// </summary>
    public async Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientName);

        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(clientName));
        NdrCallResult result = await _channel.InvokeAsync(
            InterfaceId,
            Opnums.SetClientName,
            payload,
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IOPCCommon::SetClientName");
    }

    internal static byte[] WritePayload(NdrWriteAction action)
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

        throw new InvalidOperationException("Unable to encode the IOPCCommon DCOM payload.");
    }

    internal delegate void NdrWriteAction(ref NdrWriter writer);
}
