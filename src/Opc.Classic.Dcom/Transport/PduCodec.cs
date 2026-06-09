//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Shared DCE/RPC connection-oriented PDU codec used by both the client-side
/// <see cref="DcomCallChannel" /> and the server-side
/// <c>RpcServerConnectionProcessor</c>.
/// </summary>
/// <remarks>
/// Encodes and decodes the full set of connection-oriented PDU types the
/// managed DCOM stack speaks today: bind/bind_ack/bind_nak, alter_context/
/// alter_context_resp, auth3, request/response/fault, shutdown, cancel, and
/// orphaned. PDUs are framed using the standard DCE/RPC common header; the
/// 16-bit fragment-length field at offset 8 is honored when reading from a
/// <see cref="PipeReader" />.
/// </remarks>
public static class PduCodec {
    /// <summary>
    /// Reads exactly one full DCE/RPC fragment from <paramref name="input" />,
    /// honoring the fragment-length field of the common header.
    /// </summary>
    /// <param name="input">The transport's <see cref="PipeReader" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bytes of a single fragment (header + body + optional auth verifier).</returns>
    /// <exception cref="EndOfStreamException">If the input completes before a full fragment arrives.</exception>
    public static async ValueTask<byte[]> ReadPduFrameAsync(PipeReader input, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(input);

        while (true) {
            ReadResult result = await input.ReadAsync(cancellationToken).ConfigureAwait(false);
            ReadOnlySequence<byte> buffer = result.Buffer;
            try {
                if (TryGetFragmentLength(buffer, out int fragmentLength) && buffer.Length >= fragmentLength) {
                    ReadOnlySequence<byte> frame = buffer.Slice(0, fragmentLength);
                    return frame.ToArray();
                }

                if (result.IsCompleted) {
                    throw new EndOfStreamException("Transport completed before a full DCE/RPC PDU arrived.");
                }
            }
            finally {
                if (TryGetFragmentLength(buffer, out int fragmentLength) && buffer.Length >= fragmentLength) {
                    input.AdvanceTo(buffer.GetPosition(fragmentLength));
                }
                else {
                    input.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }
    }

    /// <summary>
    /// Returns <see langword="true" /> when <paramref name="buffer" /> holds at
    /// least the DCE/RPC common header and yields the advertised fragment
    /// length out of it.
    /// </summary>
    public static bool TryGetFragmentLength(ReadOnlySequence<byte> buffer, out int fragmentLength) {
        fragmentLength = 0;
        if (buffer.Length < ConnectionOrientedPdu.HEADER_LENGTH) {
            return false;
        }

        Span<byte> lengthBytes = stackalloc byte[2];
        buffer.Slice(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2).CopyTo(lengthBytes);
        fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(lengthBytes);
        return fragmentLength >= ConnectionOrientedPdu.HEADER_LENGTH;
    }

    /// <summary>
    /// Decodes the bytes of a single fragment (post-auth-strip) into a
    /// <see cref="ConnectionOrientedPdu" /> of the appropriate concrete type.
    /// </summary>
    /// <param name="bytes">The fragment bytes.</param>
    /// <returns>The decoded PDU.</returns>
    /// <exception cref="InvalidOperationException">If the PDU type is unknown.</exception>
    public static ConnectionOrientedPdu DecodePdu(byte[] bytes) {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length < ConnectionOrientedPdu.HEADER_LENGTH) {
            throw new InvalidOperationException("DCE/RPC frame is shorter than the common header.");
        }

        byte type = bytes[ConnectionOrientedPdu.TYPE_OFFSET];
        ConnectionOrientedPdu pdu = type switch {
            RequestCoPdu.REQUEST_TYPE => new RequestCoPdu(),
            ResponseCoPdu.RESPONSE_TYPE => new ResponseCoPdu(),
            FaultCoPdu.FAULT_TYPE => new FaultCoPdu(),
            BindPdu.BIND_TYPE => new BindPdu(),
            BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE => new BindAcknowledgePdu(),
            BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE => new BindNoAcknowledgePdu(),
            AlterContextPdu.ALTER_CONTEXT_TYPE => new AlterContextPdu(),
            AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE => new AlterContextResponsePdu(),
            ShutdownPdu.SHUTDOWN_TYPE => new ShutdownPdu(),
            Auth3Pdu.AUTH3_TYPE => new Auth3Pdu(),
            CancelCoPdu.CANCEL_TYPE => new CancelCoPdu(),
            OrphanedPdu.ORPHANED_TYPE => new OrphanedPdu(),
            _ => throw new InvalidOperationException($"Unknown DCE/RPC PDU type 0x{type:X2}."),
        };

        var ndr = new NdrCodec { Format = pdu.Format };
        var buffer = new NdrBuffer(bytes, 0) { Length = bytes.Length };
        pdu.Decode(ndr, buffer);
        return pdu;
    }

    /// <summary>
    /// Encodes a <see cref="ConnectionOrientedPdu" /> into a byte array. The
    /// returned buffer is tightly sized to the encoded fragment.
    /// </summary>
    /// <param name="pdu">The PDU to encode.</param>
    /// <param name="maxTransmitFragment">
    /// The maximum transmit fragment size negotiated for this connection.
    /// Determines the working buffer capacity.
    /// </param>
    public static byte[] EncodePdu(ConnectionOrientedPdu pdu, int maxTransmitFragment) {
        ArgumentNullException.ThrowIfNull(pdu);
        int capacity = Math.Max(maxTransmitFragment, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        var ndr = new NdrCodec { Format = pdu.Format };
        var buffer = new NdrBuffer(new byte[capacity], 0);
        pdu.Encode(ndr, buffer);
        return buffer.Buf.AsSpan(0, buffer.Length).ToArray();
    }
}
