// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Text;

namespace Opc.Classic.Dcom.Smb.Tests.Pcap;

internal static class PcapFileReader
{
    private const uint LinkTypeNull = 0;
    private const uint LinkTypeEthernet = 1;
    private const ushort SmbPort = 445;
    private const int Smb2HeaderSize = 64;

    internal enum PacketDirection
    {
        ClientToServer,
        ServerToClient,
    }

    internal readonly record struct Smb2Packet(
        DateTimeOffset Timestamp,
        PacketDirection Direction,
        ReadOnlyMemory<byte> Payload);

    public static IReadOnlyList<Smb2Packet> ReadPackets(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        if (string.Equals(Path.GetExtension(filePath), ".txt", StringComparison.OrdinalIgnoreCase))
        {
            return ReadTextFixture(filePath);
        }

        return ReadPcapFixture(filePath);
    }

    private static IReadOnlyList<Smb2Packet> ReadTextFixture(string filePath)
    {
        var packets = new List<Smb2Packet>();
        foreach (string rawLine in File.ReadLines(filePath))
        {
            string line = StripComment(rawLine).Trim();
            if (line.Length == 0)
            {
                continue;
            }

            int arrowLength = 2;
            int arrowIndex = line.IndexOf("->", StringComparison.Ordinal);
            if (arrowIndex < 0)
            {
                arrowLength = 1;
                arrowIndex = line.IndexOf('→', StringComparison.Ordinal);
            }
            if (arrowIndex < 0)
            {
                throw new FormatException($"Fixture line does not contain a direction arrow: {rawLine}");
            }

            var direction = ParseDirection(line[..arrowIndex].Trim());
            byte[] payload = StripOptionalNetBiosHeader(ParseHex(line[(arrowIndex + arrowLength)..]));
            packets.Add(new Smb2Packet(DateTimeOffset.UnixEpoch.AddTicks(packets.Count), direction, payload));
        }

        return packets;
    }

    private static IReadOnlyList<Smb2Packet> ReadPcapFixture(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        ReadOnlySpan<byte> source = fileBytes;
        if (source.Length < 24)
        {
            throw new FormatException("PCAP file is shorter than the global header.");
        }

        bool littleEndian = GetPcapEndian(source[..4]);
        uint linkType = ReadUInt32(source.Slice(20, 4), littleEndian);
        if (linkType != LinkTypeEthernet && linkType != LinkTypeNull)
        {
            throw new NotSupportedException($"Unsupported PCAP link type {linkType}; expected LINKTYPE_ETHERNET or LINKTYPE_NULL.");
        }

        var packets = new List<Smb2Packet>();
        int offset = 24;
        while (offset < source.Length)
        {
            if (source.Length - offset < 16)
            {
                throw new FormatException("PCAP packet header is truncated.");
            }

            uint seconds = ReadUInt32(source.Slice(offset, 4), littleEndian);
            uint microseconds = ReadUInt32(source.Slice(offset + 4, 4), littleEndian);
            uint includedLength = ReadUInt32(source.Slice(offset + 8, 4), littleEndian);
            offset += 16;
            if (includedLength > int.MaxValue || source.Length - offset < includedLength)
            {
                throw new FormatException("PCAP packet payload is truncated.");
            }

            var timestamp = DateTimeOffset.UnixEpoch
                .AddSeconds(seconds)
                .AddTicks(microseconds * TimeSpan.TicksPerMicrosecond);
            ExtractPacket(source.Slice(offset, (int)includedLength), linkType, timestamp, packets);
            offset += (int)includedLength;
        }

        return packets;
    }

    private static void ExtractPacket(
        ReadOnlySpan<byte> packet,
        uint linkType,
        DateTimeOffset timestamp,
        List<Smb2Packet> packets)
    {
        if (linkType == LinkTypeEthernet)
        {
            ReadOnlySpan<byte> ethernetPayload = GetEthernetPayload(packet);
            if (!ethernetPayload.IsEmpty)
            {
                ExtractIpPacket(ethernetPayload, timestamp, packets);
            }
            return;
        }

        if (packet.Length > 4)
        {
            ExtractIpPacket(packet[4..], timestamp, packets);
        }
    }

    private static ReadOnlySpan<byte> GetEthernetPayload(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 14)
        {
            return default;
        }

        int payloadOffset = 14;
        ushort etherType = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(12, 2));
        while (etherType is 0x8100 or 0x88A8)
        {
            if (packet.Length < payloadOffset + 4)
            {
                return default;
            }
            etherType = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(payloadOffset + 2, 2));
            payloadOffset += 4;
        }

        return etherType is 0x0800 or 0x86DD ? packet[payloadOffset..] : default;
    }

    private static void ExtractIpPacket(
        ReadOnlySpan<byte> packet,
        DateTimeOffset timestamp,
        List<Smb2Packet> packets)
    {
        if (packet.IsEmpty)
        {
            return;
        }

        int version = packet[0] >> 4;
        if (version == 4)
        {
            ExtractIpv4Packet(packet, timestamp, packets);
        }
        else if (version == 6)
        {
            ExtractIpv6Packet(packet, timestamp, packets);
        }
    }

    private static void ExtractIpv4Packet(
        ReadOnlySpan<byte> packet,
        DateTimeOffset timestamp,
        List<Smb2Packet> packets)
    {
        if (packet.Length < 20)
        {
            return;
        }

        int headerLength = (packet[0] & 0x0F) * 4;
        ushort totalLength = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(2, 2));
        ushort flagsAndFragment = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(6, 2));
        if (headerLength < 20 || totalLength < headerLength || packet[9] != 6 || (flagsAndFragment & 0x3FFF) != 0)
        {
            return;
        }

        int availableLength = Math.Min(totalLength, packet.Length);
        ExtractTcpSegment(packet.Slice(headerLength, availableLength - headerLength), timestamp, packets);
    }

    private static void ExtractIpv6Packet(
        ReadOnlySpan<byte> packet,
        DateTimeOffset timestamp,
        List<Smb2Packet> packets)
    {
        if (packet.Length < 40 || packet[6] != 6)
        {
            return;
        }

        ushort payloadLength = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(4, 2));
        int availableLength = Math.Min(packet.Length, 40 + payloadLength);
        ExtractTcpSegment(packet.Slice(40, availableLength - 40), timestamp, packets);
    }

    private static void ExtractTcpSegment(
        ReadOnlySpan<byte> segment,
        DateTimeOffset timestamp,
        List<Smb2Packet> packets)
    {
        if (segment.Length < 20)
        {
            return;
        }

        ushort sourcePort = BinaryPrimitives.ReadUInt16BigEndian(segment[..2]);
        ushort destinationPort = BinaryPrimitives.ReadUInt16BigEndian(segment.Slice(2, 2));
        int headerLength = (segment[12] >> 4) * 4;
        if (headerLength < 20 || segment.Length < headerLength)
        {
            return;
        }

        ExtractSmb2Frames(segment[headerLength..], sourcePort, destinationPort, timestamp, packets);
    }

    private static void ExtractSmb2Frames(
        ReadOnlySpan<byte> tcpPayload,
        ushort sourcePort,
        ushort destinationPort,
        DateTimeOffset timestamp,
        List<Smb2Packet> packets)
    {
        if (tcpPayload.IsEmpty)
        {
            return;
        }

        bool addedNetBiosFrame = false;
        for (int offset = 0; offset + 4 <= tcpPayload.Length && tcpPayload[offset] == 0;)
        {
            int frameLength = (tcpPayload[offset + 1] << 16) | (tcpPayload[offset + 2] << 8) | tcpPayload[offset + 3];
            if (frameLength == 0 || tcpPayload.Length - offset - 4 < frameLength)
            {
                break;
            }

            ReadOnlySpan<byte> smb2Payload = tcpPayload.Slice(offset + 4, frameLength);
            if (IsSmb2Packet(smb2Payload) && TryGetDirection(sourcePort, destinationPort, smb2Payload, out var frameDirection))
            {
                packets.Add(new Smb2Packet(timestamp, frameDirection, smb2Payload.ToArray()));
                addedNetBiosFrame = true;
            }
            offset += 4 + frameLength;
        }

        if (!addedNetBiosFrame && IsSmb2Packet(tcpPayload) && TryGetDirection(sourcePort, destinationPort, tcpPayload, out var payloadDirection))
        {
            packets.Add(new Smb2Packet(timestamp, payloadDirection, tcpPayload.ToArray()));
        }
    }

    private static bool TryGetDirection(
        ushort sourcePort,
        ushort destinationPort,
        ReadOnlySpan<byte> smb2Payload,
        out PacketDirection direction)
    {
        if (destinationPort == SmbPort)
        {
            direction = PacketDirection.ClientToServer;
            return true;
        }
        if (sourcePort == SmbPort)
        {
            direction = PacketDirection.ServerToClient;
            return true;
        }

        uint flags = BinaryPrimitives.ReadUInt32LittleEndian(smb2Payload.Slice(16, 4));
        direction = (flags & 0x00000001) == 0
            ? PacketDirection.ClientToServer
            : PacketDirection.ServerToClient;
        return true;
    }

    private static bool IsSmb2Packet(ReadOnlySpan<byte> payload) =>
        payload.Length >= Smb2HeaderSize &&
        payload[0] == 0xFE &&
        payload[1] == (byte)'S' &&
        payload[2] == (byte)'M' &&
        payload[3] == (byte)'B';

    private static byte[] StripOptionalNetBiosHeader(byte[] payload)
    {
        if (payload.Length < 4 || payload[0] != 0)
        {
            return payload;
        }

        int frameLength = (payload[1] << 16) | (payload[2] << 8) | payload[3];
        if (frameLength != payload.Length - 4 || !IsSmb2Packet(payload.AsSpan(4)))
        {
            return payload;
        }

        return payload.AsSpan(4).ToArray();
    }

    private static PacketDirection ParseDirection(string direction) =>
        direction.Equals("client", StringComparison.OrdinalIgnoreCase) ||
        direction.Equals("client-to-server", StringComparison.OrdinalIgnoreCase) ||
        direction.Equals("c2s", StringComparison.OrdinalIgnoreCase)
            ? PacketDirection.ClientToServer
            : direction.Equals("server", StringComparison.OrdinalIgnoreCase) ||
              direction.Equals("server-to-client", StringComparison.OrdinalIgnoreCase) ||
              direction.Equals("s2c", StringComparison.OrdinalIgnoreCase)
                ? PacketDirection.ServerToClient
                : throw new FormatException($"Unknown packet direction '{direction}'.");

    private static byte[] ParseHex(string text)
    {
        var hex = new StringBuilder(text.Length);
        foreach (char character in text)
        {
            if (IsHexDigit(character))
            {
                hex.Append(character);
            }
            else if (!char.IsWhiteSpace(character) && character is not ':' and not ',' and not '_')
            {
                throw new FormatException($"Unexpected character '{character}' in hex payload.");
            }
        }

        if (hex.Length == 0 || hex.Length % 2 != 0)
        {
            throw new FormatException("Hex payload must contain a non-empty even number of hex digits.");
        }

        return Convert.FromHexString(hex.ToString());
    }

    private static string StripComment(string line)
    {
        int index = line.IndexOf('#', StringComparison.Ordinal);
        return index < 0 ? line : line[..index];
    }

    private static bool GetPcapEndian(ReadOnlySpan<byte> magic)
    {
        if (magic[0] == 0xD4 && magic[1] == 0xC3 && magic[2] == 0xB2 && magic[3] == 0xA1)
        {
            return true;
        }
        if (magic[0] == 0xA1 && magic[1] == 0xB2 && magic[2] == 0xC3 && magic[3] == 0xD4)
        {
            return false;
        }

        throw new FormatException("Unsupported PCAP magic; expected 0xA1B2C3D4.");
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> source, bool littleEndian) =>
        littleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(source) : BinaryPrimitives.ReadUInt32BigEndian(source);

    private static bool IsHexDigit(char character) =>
        character is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';
}
