//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Smb;

namespace Opc.Classic.Dcom.Smb.Tests.Pcap;

public sealed class Smb2PcapReplayer
{
    private const int HeaderSize = 64;
    private const int NegotiateClientGuidOffset = HeaderSize + 12;
    private const uint Smb2FlagsServerToRedir = 0x00000001;
    private readonly IReadOnlyList<PcapFileReader.Smb2Packet> _packets;

    internal Smb2PcapReplayer(IReadOnlyList<PcapFileReader.Smb2Packet> packets) =>
        _packets = packets ?? throw new ArgumentNullException(nameof(packets));

    public static Smb2PcapReplayer FromFile(string filePath) =>
        new(PcapFileReader.ReadPackets(filePath));

    public async Task<ReplayResult> ReplayNegotiateAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        await using var transport = new ReplayTransport(_packets);
        await using var connection = new Smb2Connection(new Smb2ConnectionOptions(host), transport);
        Smb2NegotiateResponse response = await connection.NegotiateAsync(cancellationToken);
        transport.AssertConsumed();
        return new ReplayResult(
            connection.NegotiatedDialect,
            response.SecurityMode,
            response.Capabilities,
            transport.MatchedClientPackets,
            transport.FedServerPackets);
    }

    public readonly record struct ReplayResult(
        Smb2Dialect NegotiatedDialect,
        ushort SecurityMode,
        uint Capabilities,
        int MatchedClientPackets,
        int FedServerPackets)
    {
        private const ushort SigningRequiredMask = 0x0002;
        private const uint EncryptionCapabilityMask = 0x00000040;

        public bool SigningRequired => (SecurityMode & SigningRequiredMask) != 0;

        public bool EncryptionSupported => (Capabilities & EncryptionCapabilityMask) != 0;
    }

    private sealed class ReplayTransport : ISmb2Transport
    {
        private readonly IReadOnlyList<PcapFileReader.Smb2Packet> _packets;
        private int _nextPacket;

        public ReplayTransport(IReadOnlyList<PcapFileReader.Smb2Packet> packets) =>
            _packets = packets;

        public int MatchedClientPackets { get; private set; }

        public int FedServerPackets { get; private set; }

        public Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PcapFileReader.Smb2Packet expected = TakeNext(PcapFileReader.PacketDirection.ClientToServer);
            CompareClientPacket(expected.Payload.Span, packet.Span);
            MatchedClientPackets++;
            return Task.CompletedTask;
        }

        public Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PcapFileReader.Smb2Packet packet = TakeNext(PcapFileReader.PacketDirection.ServerToClient);
            FedServerPackets++;
            return Task.FromResult(packet.Payload);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void AssertConsumed()
        {
            if (_nextPacket != _packets.Count)
            {
                throw new InvalidOperationException($"Replay consumed {_nextPacket} of {_packets.Count} SMB2 fixture packets.");
            }
        }

        private PcapFileReader.Smb2Packet TakeNext(PcapFileReader.PacketDirection direction)
        {
            if (_nextPacket >= _packets.Count)
            {
                throw new InvalidOperationException($"Replay expected a {direction} packet, but the fixture is exhausted.");
            }

            PcapFileReader.Smb2Packet packet = _packets[_nextPacket];
            if (packet.Direction != direction)
            {
                throw new InvalidOperationException(
                    $"Replay expected a {direction} packet at index {_nextPacket}, but fixture has {packet.Direction}.");
            }

            _nextPacket++;
            return packet;
        }
    }

    private static void CompareClientPacket(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> actual)
    {
        if (expected.Length != actual.Length)
        {
            throw new InvalidOperationException(
                $"Client packet length mismatch: expected {expected.Length} bytes, actual {actual.Length} bytes.");
        }

        for (int offset = 0; offset < expected.Length; offset++)
        {
            if (expected[offset] == actual[offset] || ShouldIgnoreClientByte(expected, actual, offset))
            {
                continue;
            }

            throw new InvalidOperationException(
                $"Client packet mismatch at offset 0x{offset:X4}: expected 0x{expected[offset]:X2}, actual 0x{actual[offset]:X2}.");
        }
    }

    private static bool ShouldIgnoreClientByte(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> actual, int offset) =>
        offset >= NegotiateClientGuidOffset &&
        offset < NegotiateClientGuidOffset + 16 &&
        IsNegotiateRequest(expected) &&
        IsNegotiateRequest(actual);

    private static bool IsNegotiateRequest(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < HeaderSize + 36 ||
            packet[0] != 0xFE ||
            packet[1] != (byte)'S' ||
            packet[2] != (byte)'M' ||
            packet[3] != (byte)'B')
        {
            return false;
        }

        return BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(12, 2)) == (ushort)Smb2Command.Negotiate &&
            (BinaryPrimitives.ReadUInt32LittleEndian(packet.Slice(16, 4)) & Smb2FlagsServerToRedir) == 0 &&
            BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(HeaderSize, 2)) == 36;
    }
}
