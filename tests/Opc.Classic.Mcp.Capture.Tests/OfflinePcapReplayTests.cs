// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using SharpPcap;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class OfflinePcapReplayTests
{
    private const int EthernetLinkType = 1;
    private static readonly Guid s_interfaceId = Guid.Parse("22222222-3333-4444-5555-666666666666");
    private static readonly DateTimeOffset s_timestamp = new(2026, 6, 7, 12, 34, 56, TimeSpan.Zero);

    [Test, NotInParallel, Category("Integration")]
    public async Task ReadAllAsync_OfflinePcapFile_ReplaysAndDecodesDceRpcBindFrame()
    {
        string directory = CreateArtifactDirectory();
        try
        {
            byte[] frame = NewTcpFrame(NewBindPayload(callId: 42));
            string pcapPath = Path.Combine(directory, "capture.pcap");
            WritePcapFile(pcapPath, s_timestamp, frame);
            var source = new PcapCaptureSource(directory);

            List<CapturedPacket> packets;
            try
            {
                packets = await source.ReadAllAsync(null, TestContext.Current!.CancellationToken).ToListAsync();
            }
            catch (Exception ex) when (IsNativePcapUnavailable(ex))
            {
                string reason = $"Offline pcap replay requires native libpcap/Npcap: {ex.GetType().Name}: {ex.Message}";
                await Assert.That(reason).IsNotEqualTo(string.Empty);
                return;
            }
            finally
            {
                await source.DisposeAsync();
            }

            IReadOnlyList<DecodedOpcPdu> decoded = new OpcDcomDecoder().DecodeAll(packets);

            await Assert.That(source.GetRawPcapFilePath()).IsEqualTo(pcapPath);
            await Assert.That(packets.Count).IsEqualTo(1);
            await Assert.That(packets[0].Timestamp).IsEqualTo(s_timestamp);
            await Assert.That(packets[0].OriginalLength).IsEqualTo(frame.Length);
            await Assert.That(packets[0].LinkType).IsEqualTo(EthernetLinkType);
            await Assert.That(packets[0].Data.ToArray().SequenceEqual(frame)).IsTrue();
            await Assert.That(decoded.Count).IsEqualTo(1);
            await Assert.That(decoded[0].PduType).IsEqualTo("bind");
            await Assert.That(decoded[0].CallId).IsEqualTo(42);
            await Assert.That(decoded[0].SourceEndpoint).IsEqualTo("10.1.2.3:50001");
            await Assert.That(decoded[0].DestinationEndpoint).IsEqualTo("10.4.5.6:135");
            await Assert.That(decoded[0].ContextList.Count).IsEqualTo(1);
            await Assert.That(decoded[0].ContextList[0].ContextId).IsEqualTo(7);
            await Assert.That(decoded[0].ContextList[0].AbstractSyntaxIid).IsEqualTo(s_interfaceId);
            await Assert.That(decoded[0].ContextList[0].MajorVersion).IsEqualTo(1);
            await Assert.That(decoded[0].ContextList[0].MinorVersion).IsEqualTo(0);
        }
        finally
        {
            DeleteIfExists(directory);
        }
    }

    private static byte[] NewBindPayload(int callId)
    {
        var pdu = new BindPdu
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList =
            [
                new PresentationContext(
                    7,
                    new PresentationSyntax(new UUID(s_interfaceId.ToString("D")), 1, 0)),
            ],
        };

        return PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
    }

    private static byte[] NewTcpFrame(byte[] tcpPayload)
    {
        byte[] frame = new byte[14 + 20 + 20 + tcpPayload.Length];

        frame[0] = 0x00;
        frame[1] = 0x11;
        frame[2] = 0x22;
        frame[3] = 0x33;
        frame[4] = 0x44;
        frame[5] = 0x55;
        frame[6] = 0x66;
        frame[7] = 0x77;
        frame[8] = 0x88;
        frame[9] = 0x99;
        frame[10] = 0xAA;
        frame[11] = 0xBB;
        frame[12] = 0x08;
        frame[13] = 0x00;

        int ipOffset = 14;
        frame[ipOffset] = 0x45;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ipOffset + 2, 2), (ushort)(20 + 20 + tcpPayload.Length));
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(ipOffset + 4, 2), 0x1000);
        frame[ipOffset + 8] = 64;
        frame[ipOffset + 9] = 6;
        frame[ipOffset + 12] = 10;
        frame[ipOffset + 13] = 1;
        frame[ipOffset + 14] = 2;
        frame[ipOffset + 15] = 3;
        frame[ipOffset + 16] = 10;
        frame[ipOffset + 17] = 4;
        frame[ipOffset + 18] = 5;
        frame[ipOffset + 19] = 6;

        int tcpOffset = ipOffset + 20;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset, 2), 50001);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 2, 2), 135);
        frame[tcpOffset + 12] = 0x50;
        frame[tcpOffset + 13] = 0x18;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 14, 2), 8192);
        tcpPayload.CopyTo(frame.AsSpan(tcpOffset + 20));

        return frame;
    }

    private static void WritePcapFile(string path, DateTimeOffset timestamp, byte[] frame)
    {
        using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        Span<byte> header = stackalloc byte[24];
        BinaryPrimitives.WriteUInt32LittleEndian(header[..4], 0xA1B2C3D4);
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(4, 2), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(6, 2), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(16, 4), 65535);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(20, 4), EthernetLinkType);
        stream.Write(header);

        Span<byte> packetHeader = stackalloc byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(packetHeader[..4], checked((uint)timestamp.ToUnixTimeSeconds()));
        BinaryPrimitives.WriteUInt32LittleEndian(packetHeader.Slice(8, 4), checked((uint)frame.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(packetHeader.Slice(12, 4), checked((uint)frame.Length));
        stream.Write(packetHeader);
        stream.Write(frame);
    }

    private static bool IsNativePcapUnavailable(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is DllNotFoundException or TypeInitializationException)
            {
                return true;
            }

            if (current is PcapException && IsNativePcapFailureMessage(current.Message))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNativePcapFailureMessage(string message)
        => message.Contains("libpcap", StringComparison.OrdinalIgnoreCase)
        || message.Contains("Npcap", StringComparison.OrdinalIgnoreCase)
        || message.Contains("wpcap", StringComparison.OrdinalIgnoreCase);

    private static string CreateArtifactDirectory()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "OfflinePcapReplayTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
