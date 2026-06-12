//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using CsCheck;
using Opc.Classic.Tests.Fuzz;

namespace Opc.Classic.Dcom.Smb.Tests.Fuzz;

public sealed class Smb2DecoderFuzzTests
{
    private static readonly Type[] AllowedSmb2Exceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(NotSupportedException),
        typeof(InvalidOperationException),
        typeof(Smb2ProtocolException),
    ];

    [Test]
    [Category("Fuzz")]
    public async Task Smb2PacketHeader_Read_RandomBytes_DoesNotCrash()
    {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static Smb2PacketHeader (ReadOnlyMemory<byte> bytes) => Smb2PacketHeader.Read(bytes.Span),
                AllowedSmb2Exceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        FuzzHarness.MutateValid(ValidHeader()).Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static Smb2PacketHeader (ReadOnlyMemory<byte> bytes) => Smb2PacketHeader.Read(bytes.Span),
                AllowedSmb2Exceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    [Arguments("SMB2-NegotiateResponse")]
    [Arguments("SMB2-SessionSetupResponse")]
    [Arguments("SMB2-TreeConnectResponse")]
    [Arguments("SMB2-CreateResponse")]
    [Arguments("SMB2-ReadResponse")]
    [Arguments("SMB2-IoctlResponse")]
    public async Task Smb2ResponseDecoder_Read_RandomAndMutatedBytes_DoesNotCrash(string surface)
    {
        var parser = ParserFor(surface);
        FuzzHarness.BytesEdgeWeighted.Sample(
            input => FuzzHarness.AssertParseDoesNotCrash(input, parser, AllowedSmb2Exceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        FuzzHarness.MutateValid(ValidBodyFor(surface)).Sample(
            input => FuzzHarness.AssertParseDoesNotCrash(input, parser, AllowedSmb2Exceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    [Arguments("SMB2-Header")]
    [Arguments("SMB2-NegotiateResponse")]
    [Arguments("SMB2-SessionSetupResponse")]
    [Arguments("SMB2-TreeConnectResponse")]
    [Arguments("SMB2-CreateResponse")]
    [Arguments("SMB2-ReadResponse")]
    [Arguments("SMB2-IoctlResponse")]
    public async Task Smb2Decoder_CorpusReplay_DoesNotCrash(string surface)
    {
        var parser = surface == "SMB2-Header"
            ? static object (ReadOnlyMemory<byte> bytes) => Smb2PacketHeader.Read(bytes.Span)
            : ParserFor(surface);

        foreach (object[] row in FuzzHarness.LoadCorpus(surface))
        {
            FuzzHarness.AssertParseDoesNotCrash((byte[])row[0], parser, AllowedSmb2Exceptions);
        }

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    private static Func<ReadOnlyMemory<byte>, object> ParserFor(string surface) => surface switch
    {
        "SMB2-NegotiateResponse" => static bytes => Smb2NegotiateResponse.Read(bytes.Span),
        "SMB2-SessionSetupResponse" => static bytes => ExerciseSessionSetup(bytes.ToArray()),
        "SMB2-TreeConnectResponse" => static bytes => Smb2TreeConnectResponse.Read(bytes.Span),
        "SMB2-CreateResponse" => static bytes => ExerciseCreate(bytes.ToArray()),
        "SMB2-ReadResponse" => static bytes => ExerciseRead(bytes.ToArray()),
        "SMB2-IoctlResponse" => static bytes => ExerciseIoctl(bytes.ToArray()),
        _ => throw new ArgumentOutOfRangeException(nameof(surface), surface, "Unknown SMB2 fuzz surface."),
    };

    private static byte[] ValidBodyFor(string surface) => surface switch
    {
        "SMB2-NegotiateResponse" => NegotiateResponseBody(),
        "SMB2-SessionSetupResponse" => SessionSetupResponseBody(),
        "SMB2-TreeConnectResponse" => TreeConnectResponseBody(),
        "SMB2-CreateResponse" => CreateResponseBody(),
        "SMB2-ReadResponse" => ReadResponseBody(),
        "SMB2-IoctlResponse" => IoctlResponseBody(),
        _ => throw new ArgumentOutOfRangeException(nameof(surface), surface, "Unknown SMB2 fuzz surface."),
    };

    private static byte[] ValidHeader()
    {
        byte[] buffer = new byte[64];
        var header = new Smb2PacketHeader(1, 0, Smb2Command.Negotiate, 1, 1, 0, 1, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
        header.Write(buffer);
        return buffer;
    }

    private static byte[] NegotiateResponseBody()
    {
        byte[] body = new byte[72];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 65);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(2), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(4), (ushort)Smb2Dialect.Smb300);
        Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00").TryWriteBytes(body.AsSpan(8, 16));
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(28), 0x10000);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(32), 0x10000);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(36), 0x10000);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(56), 128);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(58), 8);
        new byte[] { 0x60, 0x06, 0x06, 0x04, 0x2b, 0x06, 0x01, 0x05 }.CopyTo(body.AsSpan(64));
        return body;
    }

    private static byte[] SessionSetupResponseBody()
    {
        byte[] body = new byte[12];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(4), 72);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(6), 4);
        body.AsSpan(8, 4).Fill(0x42);
        return body;
    }

    private static byte[] TreeConnectResponseBody()
    {
        byte[] body = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 16);
        body[2] = 0x02;
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(4), 0x40);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(8), 0x800);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(12), 0x001F01FF);
        return body;
    }

    private static byte[] CreateResponseBody()
    {
        byte[] body = new byte[88];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 89);
        BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(64), 1);
        BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(72), 2);
        return body;
    }

    private static byte[] ReadResponseBody()
    {
        byte[] body = new byte[20];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 17);
        body[2] = 80;
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(4), 4);
        body.AsSpan(16, 4).Fill(0x24);
        return body;
    }

    private static byte[] IoctlResponseBody()
    {
        byte[] body = new byte[56];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 49);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(4), 0x0011c017);
        BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(16), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(32), 112);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(36), 8);
        body.AsSpan(48, 8).Fill(0x33);
        return body;
    }

    private static object ExerciseSessionSetup(byte[] body)
    {
        var conn = Connection(
            Packet(Smb2Command.Negotiate, NegotiateResponseBody()),
            Packet(Smb2Command.SessionSetup, body, sessionId: 0x1000));
        _ = conn.NegotiateAsync().GetAwaiter().GetResult();
        conn.SessionSetupAsync(OneRoundTripBlobProvider()).GetAwaiter().GetResult();
        return true;
    }

    private static object ExerciseCreate(byte[] body)
    {
        var conn = EstablishedConnection(Packet(Smb2Command.Create, body, sessionId: 0x1000, treeId: 0x2000));
        _ = conn.OpenNamedPipeAsync("winreg").GetAwaiter().GetResult();
        return true;
    }

    private static object ExerciseRead(byte[] body)
    {
        var conn = EstablishedConnection(
            Packet(Smb2Command.Create, CreateResponseBody(), sessionId: 0x1000, treeId: 0x2000),
            Packet(Smb2Command.Read, body, sessionId: 0x1000, treeId: 0x2000));
        var pipe = conn.OpenNamedPipeAsync("winreg").GetAwaiter().GetResult();
        _ = pipe.ReadAsync(16).GetAwaiter().GetResult();
        return true;
    }

    private static object ExerciseIoctl(byte[] body)
    {
        var conn = EstablishedConnection(
            Packet(Smb2Command.Create, CreateResponseBody(), sessionId: 0x1000, treeId: 0x2000),
            Packet(Smb2Command.Ioctl, body, sessionId: 0x1000, treeId: 0x2000));
        var pipe = conn.OpenNamedPipeAsync("winreg").GetAwaiter().GetResult();
        _ = pipe.TransceiveAsync(new byte[] { 0x01 }).GetAwaiter().GetResult();
        return true;
    }

    private static Smb2Connection EstablishedConnection(params byte[][] tail)
    {
        var packets = new List<byte[]>
        {
            Packet(Smb2Command.Negotiate, NegotiateResponseBody()),
            Packet(Smb2Command.SessionSetup, SessionSetupResponseBody(), sessionId: 0x1000),
            Packet(Smb2Command.TreeConnect, TreeConnectResponseBody(), sessionId: 0x1000, treeId: 0x2000),
        };
        packets.AddRange(tail);
        var conn = Connection([.. packets]);
        _ = conn.NegotiateAsync().GetAwaiter().GetResult();
        conn.SessionSetupAsync(OneRoundTripBlobProvider()).GetAwaiter().GetResult();
        _ = conn.TreeConnectIpcAsync().GetAwaiter().GetResult();
        return conn;
    }

    private static Smb2Connection Connection(params byte[][] packets) =>
        new(new Smb2ConnectionOptions("test"), new MockSmb2Transport(packets));

    private static NtlmsspBlobProvider OneRoundTripBlobProvider()
    {
        var calls = 0;
        return _ => calls++ == 0 ? new byte[] { 0x01 } : null;
    }

    private static byte[] Packet(
        Smb2Command command,
        byte[] body,
        ulong sessionId = 0,
        uint treeId = 0)
    {
        var packet = new byte[64 + body.Length];
        var header = new Smb2PacketHeader(1, 0, command, 1, 1, 0, 1, 0, treeId, sessionId, ReadOnlyMemory<byte>.Empty);
        header.Write(packet);
        body.CopyTo(packet.AsSpan(64));
        return packet;
    }

    private sealed class MockSmb2Transport : ISmb2Transport
    {
        private readonly Queue<ReadOnlyMemory<byte>> _responses;

        public MockSmb2Transport(params byte[][] responses)
        {
            _responses = new Queue<ReadOnlyMemory<byte>>();
            foreach (byte[] response in responses)
            {
                _responses.Enqueue(response);
            }
        }

        public Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_responses.Count == 0)
            {
                throw new EndOfStreamException("No queued SMB2 fuzz response.");
            }

            return Task.FromResult(_responses.Dequeue());
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
