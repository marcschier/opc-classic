// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureFileProcessorTests
{
    private static readonly Guid s_iid = new("33333333-4444-5555-6666-777777777777");

    [Test]
    public async Task DecodeAsync_PcapEthernetIpv4Tcp_DecodesBind()
    {
        string folder = CreateFolder();
        try
        {
            string path = Path.Combine(folder, "external.pcap");
            WritePcap(path, NewIpv4TcpFrame(NewBind(41)));

            CaptureFileDecodeResult result = await CaptureFileProcessor.DecodeAsync(
                path,
                cancellationToken: TestContext.Current!.CancellationToken);

            await Assert.That(result.Status.Format).IsEqualTo("pcap");
            await Assert.That(result.Status.Ipv4TcpPackets).IsEqualTo(1);
            await Assert.That(result.Status.MidSessionLikely).IsFalse();
            await Assert.That(result.Pdus.Count).IsEqualTo(1);
            await Assert.That(result.Pdus[0].PduType).IsEqualTo("bind");
        }
        finally
        {
            TestDirectories.DeleteIfExists(folder);
        }
    }

    [Test]
    public async Task DecodeAsync_PcapNgEthernetIpv6Tcp_DecodesBind()
    {
        string folder = CreateFolder();
        try
        {
            string path = Path.Combine(folder, "external.pcapng");
            WritePcapNg(path, NewIpv6TcpFrame(NewBind(42)));

            CaptureFileDecodeResult result = await CaptureFileProcessor.DecodeAsync(
                path,
                cancellationToken: TestContext.Current!.CancellationToken);

            await Assert.That(result.Status.Format).IsEqualTo("pcapng");
            await Assert.That(result.Status.Ipv6TcpPackets).IsEqualTo(1);
            await Assert.That(result.Pdus.Count).IsEqualTo(1);
            await Assert.That(result.Pdus[0].CallId).IsEqualTo(42);
        }
        finally
        {
            TestDirectories.DeleteIfExists(folder);
        }
    }

    [Test]
    public async Task DecodeAsync_IpFragmentAndMidSession_AreReported()
    {
        string folder = CreateFolder();
        try
        {
            string fragmentedPath = Path.Combine(folder, "fragmented.pcap");
            byte[] fragment = NewIpv4TcpFrame(NewBind(43));
            BinaryPrimitives.WriteUInt16BigEndian(fragment.AsSpan(14 + 6, 2), 0x2000);
            WritePcap(fragmentedPath, fragment);
            CaptureFileDecodeResult fragmented = await CaptureFileProcessor.DecodeAsync(
                fragmentedPath,
                cancellationToken: TestContext.Current!.CancellationToken);

            string midSessionPath = Path.Combine(folder, "mid-session.pcap");
            WritePcap(midSessionPath, NewIpv4TcpFrame(NewRequest(44)));
            CaptureFileDecodeResult midSession = await CaptureFileProcessor.DecodeAsync(
                midSessionPath,
                cancellationToken: TestContext.Current.CancellationToken);

            await Assert.That(fragmented.Status.FragmentedIpPackets).IsEqualTo(1);
            await Assert.That(fragmented.Pdus.Count).IsEqualTo(0);
            await Assert.That(midSession.Status.MidSessionLikely).IsTrue();
            await Assert.That(midSession.Pdus.Count).IsEqualTo(1);
        }
        finally
        {
            TestDirectories.DeleteIfExists(folder);
        }
    }

    [Test]
    public async Task DecodeAsync_EnforcesFileSizeLimit()
    {
        string folder = CreateFolder();
        try
        {
            string path = Path.Combine(folder, "large.pcap");
            WritePcap(path, NewIpv4TcpFrame(NewBind(45)));

            await Assert.That(async () => await CaptureFileProcessor.DecodeAsync(
                path,
                maxFileBytes: 8,
                cancellationToken: TestContext.Current!.CancellationToken))
                .Throws<CaptureException>();
        }
        finally
        {
            TestDirectories.DeleteIfExists(folder);
        }
    }

    [Test]
    public async Task DecodeAsync_SnaplenTruncatedPacketReportsFailureAndContinues()
    {
        string folder = CreateFolder();
        try
        {
            string path = Path.Combine(folder, "snaplen.pcap");
            byte[] truncatedOriginal = NewIpv4TcpFrame(NewBind(46));
            byte[] valid = NewIpv4TcpFrame(NewBind(47));
            WritePcap(
                path,
                (truncatedOriginal[..30], truncatedOriginal.Length),
                (valid, valid.Length));

            CaptureFileDecodeResult result = await CaptureFileProcessor.DecodeAsync(
                path,
                cancellationToken: TestContext.Current!.CancellationToken);

            await Assert.That(result.Status.TruncatedPackets).IsEqualTo(1);
            await Assert.That(result.Status.PacketFailureCount).IsGreaterThanOrEqualTo(1);
            await Assert.That(result.Status.PacketFailures.Any(
                failure => failure.Code == "snaplen_truncated")).IsTrue();
            await Assert.That(result.Status.PacketFailures.All(
                failure => failure.Message.Length <= 512)).IsTrue();
            await Assert.That(result.Pdus.Count).IsEqualTo(1);
            await Assert.That(result.Pdus[0].CallId).IsEqualTo(47);
        }
        finally
        {
            TestDirectories.DeleteIfExists(folder);
        }
    }

    [Test]
    public async Task ReadBoundedAsync_RejectsStreamGrowthAfterOpen()
    {
        await using var stream = new GrowingReadStream(
            initial: new byte[] { 1, 2, 3, 4 },
            growth: new byte[] { 5 });

        var exception = await Assert.That(async () => await CaptureFileProcessor.ReadBoundedAsync(
            stream,
            openedLength: 4,
            maxFileBytes: 8,
            TestContext.Current!.CancellationToken)).Throws<CaptureException>();

        await Assert.That(exception!.Message).Contains("changed while being read");
    }

    [Test]
    public async Task ReadBoundedAsync_UsesOpenedHandleWhenPathIsReplaced()
    {
        string folder = CreateFolder();
        try
        {
            string path = Path.Combine(folder, "replace.pcap");
            byte[] original = new byte[] { 1, 2, 3, 4 };
            byte[] replacement = new byte[] { 9, 8, 7, 6 };
            await File.WriteAllBytesAsync(path, original, TestContext.Current!.CancellationToken);
            await using var opened = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            File.Move(path, path + ".old");
            await File.WriteAllBytesAsync(path, replacement, TestContext.Current.CancellationToken);

            byte[] read = await CaptureFileProcessor.ReadBoundedAsync(
                opened,
                openedLength: original.Length,
                maxFileBytes: 8,
                TestContext.Current.CancellationToken);

            await Assert.That(read).IsEquivalentTo(original);
        }
        finally
        {
            TestDirectories.DeleteIfExists(folder);
        }
    }

    private static byte[] NewBind(int callId) =>
        PduCodec.EncodePdu(
            new BindPdu
            {
                CallId = callId,
                MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
                MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
                ContextList =
                [
                    new PresentationContext(
                        0,
                        new PresentationSyntax(new UUID(s_iid.ToString("D")), 1, 0)),
                ],
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

    private static byte[] NewRequest(int callId)
    {
        byte[] stub = OrpcEnvelope.BuildRequestStub(new byte[] { 1, 2, 3 }, Guid.Empty);
        return PduCodec.EncodePdu(
            new RequestCoPdu
            {
                CallId = callId,
                ContextId = 0,
                Opnum = 1,
                AllocationHint = stub.Length,
                Stub = stub,
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
    }

    private static byte[] NewIpv4TcpFrame(byte[] payload)
    {
        byte[] frame = NewEthernetFrame(0x0800, 20 + 20 + payload.Length);
        int ip = 14;
        frame[ip] = 0x45;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ip + 2, 2), (ushort)(20 + 20 + payload.Length));
        frame[ip + 8] = 64;
        frame[ip + 9] = 6;
        frame[ip + 12] = 10;
        frame[ip + 15] = 1;
        frame[ip + 16] = 10;
        frame[ip + 19] = 2;
        WriteTcp(frame, ip + 20, payload);
        return frame;
    }

    private static byte[] NewIpv6TcpFrame(byte[] payload)
    {
        byte[] frame = NewEthernetFrame(0x86DD, 40 + 20 + payload.Length);
        int ip = 14;
        frame[ip] = 0x60;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ip + 4, 2), (ushort)(20 + payload.Length));
        frame[ip + 6] = 6;
        frame[ip + 7] = 64;
        frame[ip + 8] = 0x20;
        frame[ip + 9] = 0x01;
        frame[ip + 23] = 1;
        frame[ip + 24] = 0x20;
        frame[ip + 25] = 0x01;
        frame[ip + 39] = 2;
        WriteTcp(frame, ip + 40, payload);
        return frame;
    }

    private static byte[] NewEthernetFrame(ushort etherType, int payloadLength)
    {
        byte[] frame = new byte[14 + payloadLength];
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(12, 2), etherType);
        return frame;
    }

    private static void WriteTcp(byte[] frame, int offset, byte[] payload)
    {
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(offset, 2), 50000);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(offset + 2, 2), 135);
        frame[offset + 12] = 0x50;
        frame[offset + 13] = 0x18;
        payload.CopyTo(frame.AsSpan(offset + 20));
    }

    private static void WritePcap(string path, byte[] frame)
        => WritePcap(path, (frame, frame.Length));

    private static void WritePcap(
        string path,
        params (byte[] Data, int OriginalLength)[] packets)
    {
        using FileStream stream = File.Create(path);
        Span<byte> header = stackalloc byte[24];
        BinaryPrimitives.WriteUInt32LittleEndian(header, 0xA1B2C3D4);
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(4, 2), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(6, 2), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(16, 4), 65535);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(20, 4), 1);
        stream.Write(header);
        for (int i = 0; i < packets.Length; i++)
        {
            Span<byte> packet = new byte[16];
            BinaryPrimitives.WriteUInt32LittleEndian(packet, checked((uint)(i + 1)));
            BinaryPrimitives.WriteUInt32LittleEndian(packet.Slice(8, 4), (uint)packets[i].Data.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(packet.Slice(12, 4), (uint)packets[i].OriginalLength);
            stream.Write(packet);
            stream.Write(packets[i].Data);
        }
    }

    private static void WritePcapNg(string path, byte[] frame)
    {
        using FileStream stream = File.Create(path);
        WriteBlock(stream, 0x0A0D0D0A, body =>
        {
            BinaryPrimitives.WriteUInt32LittleEndian(body, 0x1A2B3C4D);
            BinaryPrimitives.WriteUInt16LittleEndian(body.Slice(4, 2), 1);
            BinaryPrimitives.WriteUInt64LittleEndian(body.Slice(8, 8), ulong.MaxValue);
        }, 16);
        WriteBlock(stream, 1, body =>
        {
            BinaryPrimitives.WriteUInt16LittleEndian(body, 1);
            BinaryPrimitives.WriteUInt32LittleEndian(body.Slice(4, 4), 65535);
        }, 8);
        int paddedLength = (frame.Length + 3) & ~3;
        WriteBlock(stream, 6, body =>
        {
            BinaryPrimitives.WriteUInt32LittleEndian(body.Slice(8, 4), 1_000_000);
            BinaryPrimitives.WriteUInt32LittleEndian(body.Slice(12, 4), (uint)frame.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(body.Slice(16, 4), (uint)frame.Length);
            frame.CopyTo(body.Slice(20));
        }, 20 + paddedLength);
    }

    private static void WriteBlock(
        Stream stream,
        uint type,
        SpanWriter writeBody,
        int bodyLength)
    {
        int totalLength = 12 + bodyLength;
        byte[] block = new byte[totalLength];
        BinaryPrimitives.WriteUInt32LittleEndian(block, type);
        BinaryPrimitives.WriteUInt32LittleEndian(block.AsSpan(4, 4), (uint)totalLength);
        writeBody(block.AsSpan(8, bodyLength));
        BinaryPrimitives.WriteUInt32LittleEndian(block.AsSpan(totalLength - 4, 4), (uint)totalLength);
        stream.Write(block);
    }

    private delegate void SpanWriter(Span<byte> span);

    private static string CreateFolder()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "CaptureFileProcessorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class GrowingReadStream : Stream
    {
        private readonly byte[] _initial;
        private readonly byte[] _growth;
        private long _length;
        private int _position;

        public GrowingReadStream(byte[] initial, byte[] growth)
        {
            _initial = initial;
            _growth = growth;
            _length = initial.Length;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            if (_position < _initial.Length)
            {
                int count = Math.Min(buffer.Length, _initial.Length - _position);
                _initial.AsSpan(_position, count).CopyTo(buffer);
                _position += count;
                if (_position == _initial.Length)
                {
                    _length = _initial.Length + _growth.Length;
                }
                return count;
            }
            int growthOffset = _position - _initial.Length;
            if (growthOffset >= _growth.Length)
            {
                return 0;
            }
            int growthCount = Math.Min(buffer.Length, _growth.Length - growthOffset);
            _growth.AsSpan(growthOffset, growthCount).CopyTo(buffer);
            _position += growthCount;
            return growthCount;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer.Span));
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
