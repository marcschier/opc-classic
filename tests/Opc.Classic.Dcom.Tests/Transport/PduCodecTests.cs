//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class PduCodecTests
{
    [Test]
    public async Task DecodePdu_round_trips_AlterContextPdu()
    {
        // AlterContextPdu was previously missing from the client-side DecodePdu
        // switch. ocom-1a adds it because the server-side processor must accept
        // it from clients adding a new presentation context.
        var pdu = new AlterContextPdu
        {
            CallId = 7,
            ContextList = [],
        };

        byte[] encoded = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        ConnectionOrientedPdu decoded = PduCodec.DecodePdu(encoded);

        await Assert.That(decoded).IsTypeOf<AlterContextPdu>();
        await Assert.That(((AlterContextPdu)decoded).CallId).IsEqualTo(7);
    }

    [Test]
    public async Task DecodePdu_round_trips_CancelCoPdu()
    {
        var pdu = new CancelCoPdu { CallId = 42 };

        byte[] encoded = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        ConnectionOrientedPdu decoded = PduCodec.DecodePdu(encoded);

        await Assert.That(decoded).IsTypeOf<CancelCoPdu>();
        await Assert.That(((CancelCoPdu)decoded).CallId).IsEqualTo(42);
    }

    [Test]
    public async Task DecodePdu_round_trips_OrphanedPdu()
    {
        var pdu = new OrphanedPdu { CallId = 99 };

        byte[] encoded = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        ConnectionOrientedPdu decoded = PduCodec.DecodePdu(encoded);

        await Assert.That(decoded).IsTypeOf<OrphanedPdu>();
        await Assert.That(((OrphanedPdu)decoded).CallId).IsEqualTo(99);
    }

    [Test]
    public async Task DecodePdu_throws_on_unknown_pdu_type()
    {
        byte[] frame = new byte[ConnectionOrientedPdu.HEADER_LENGTH];
        frame[ConnectionOrientedPdu.TYPE_OFFSET] = 0xEF;
        frame[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET] = (byte)frame.Length;

        await Assert.That(() => { _ = PduCodec.DecodePdu(frame); }).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task DecodePdu_throws_on_undersized_frame()
    {
        await Assert.That(() => { _ = PduCodec.DecodePdu(new byte[4]); }).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task TryGetFragmentLength_returns_false_when_buffer_too_small()
    {
        var buffer = new ReadOnlySequence<byte>(new byte[4]);

        bool ok = PduCodec.TryGetFragmentLength(buffer, out int length);

        await Assert.That(ok).IsFalse();
        await Assert.That(length).IsEqualTo(0);
    }

    [Test]
    public async Task TryGetFragmentLength_reads_fragment_length_from_header()
    {
        byte[] frame = new byte[ConnectionOrientedPdu.HEADER_LENGTH];
        frame[ConnectionOrientedPdu.TYPE_OFFSET] = RequestCoPdu.REQUEST_TYPE;
        frame[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET] = 0x34;
        frame[ConnectionOrientedPdu.FRAG_LENGTH_OFFSET + 1] = 0x12;
        var buffer = new ReadOnlySequence<byte>(frame);

        bool ok = PduCodec.TryGetFragmentLength(buffer, out int length);

        await Assert.That(ok).IsTrue();
        await Assert.That(length).IsEqualTo(0x1234);
    }

    [Test]
    public async Task ReadPduFrameAsync_reads_complete_fragment_from_pipe()
    {
        var pdu = new ShutdownPdu { CallId = 5 };
        byte[] frame = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(frame, TestContext.Current!.CancellationToken);
        await pipe.Writer.CompleteAsync();

        byte[] read = await PduCodec.ReadPduFrameAsync(pipe.Reader, TestContext.Current!.CancellationToken);

        await Assert.That(read.Length).IsEqualTo(frame.Length);
        ConnectionOrientedPdu decoded = PduCodec.DecodePdu(read);
        await Assert.That(decoded).IsTypeOf<ShutdownPdu>();
        await Assert.That(((ShutdownPdu)decoded).CallId).IsEqualTo(5);
    }

    [Test]
    public async Task ReadPduFrameAsync_throws_on_truncated_stream()
    {
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(new byte[4], TestContext.Current!.CancellationToken);
        await pipe.Writer.CompleteAsync();

        await Assert.That(async () => { _ = await PduCodec.ReadPduFrameAsync(pipe.Reader, TestContext.Current!.CancellationToken); })
            .Throws<System.IO.EndOfStreamException>();
    }
}
