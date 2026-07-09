// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// MS-RPCE endpoint-mapper tower codec for ncacn_ip_tcp bindings.
/// </summary>
public static class EndpointMapperTower
{
    public static readonly Guid NdrTransferSyntaxId = new("8a885d04-1ceb-11c9-9fe8-08002b104860");

    private const ushort FloorCount = 5;
    private const byte UuidProtocolIdentifier = 0x0D;
    private const byte ConnectionOrientedRpcProtocolIdentifier = 0x0B;
    private const byte TcpProtocolIdentifier = 0x07;
    private const byte IpProtocolIdentifier = 0x09;
    private const ushort NdrMajorVersion = 2;
    private const ushort NdrMinorVersion = 0;
    private const int MaxTowerLength = 2000;

    public static byte[] EncodeTcpTower(
        Guid interfaceId,
        ushort interfaceMajorVersion,
        ushort interfaceMinorVersion,
        IPEndPoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        IPAddress address = NormalizeAddress(endpoint.Address);
        byte[] addressBytes = address.GetAddressBytes();
        if (addressBytes.Length != 4)
        {
            throw new ArgumentException("Endpoint mapper TCP towers require an IPv4 address.", nameof(endpoint));
        }

        var buffer = new byte[2 + 25 + 25 + 7 + 7 + 9];
        int offset = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), FloorCount);
        offset += 2;
        offset = WriteUuidFloor(buffer, offset, interfaceId, interfaceMajorVersion, interfaceMinorVersion);
        offset = WriteUuidFloor(buffer, offset, NdrTransferSyntaxId, NdrMajorVersion, NdrMinorVersion);
        offset = WriteSmallFloor(buffer, offset, ConnectionOrientedRpcProtocolIdentifier, [0x00, 0x00]);

        Span<byte> portBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(portBytes, checked((ushort)endpoint.Port));
        offset = WriteSmallFloor(buffer, offset, TcpProtocolIdentifier, portBytes);
        offset = WriteSmallFloor(buffer, offset, IpProtocolIdentifier, addressBytes);

        if (offset != buffer.Length)
        {
            throw new InvalidOperationException("Endpoint mapper tower encoder produced an unexpected length.");
        }

        return buffer;
    }

    public static bool TryDecodeTcpTower(ReadOnlySpan<byte> tower, out EndpointMapperTowerBinding binding)
    {
        binding = default;
        if (tower.Length < 2 || BinaryPrimitives.ReadUInt16LittleEndian(tower[..2]) < FloorCount)
        {
            return false;
        }

        int offset = 2;
        if (!TryReadUuidFloor(tower, ref offset, out Guid interfaceId, out ushort interfaceMajor, out ushort interfaceMinor)
            || !TryReadUuidFloor(tower, ref offset, out Guid transferSyntaxId, out ushort transferMajor, out ushort transferMinor)
            || transferSyntaxId != NdrTransferSyntaxId
            || transferMajor != NdrMajorVersion
            || transferMinor != NdrMinorVersion
            || !TryReadSmallFloor(tower, ref offset, ConnectionOrientedRpcProtocolIdentifier, out ReadOnlySpan<byte> rpcData)
            || rpcData.Length != 2
            || !TryReadSmallFloor(tower, ref offset, TcpProtocolIdentifier, out ReadOnlySpan<byte> portData)
            || portData.Length != 2
            || !TryReadSmallFloor(tower, ref offset, IpProtocolIdentifier, out ReadOnlySpan<byte> ipData)
            || ipData.Length != 4)
        {
            return false;
        }

        int port = BinaryPrimitives.ReadUInt16BigEndian(portData);
        var address = new IPAddress(ipData);
        binding = new EndpointMapperTowerBinding(interfaceId, interfaceMajor, interfaceMinor, address, port);
        return true;
    }

    public static bool TryReadTower(ReadOnlySpan<byte> payload, int offset, out byte[] tower, out int bytesRead)
    {
        tower = [];
        bytesRead = 0;
        if (offset < 0 || offset + 4 > payload.Length)
        {
            return false;
        }

        uint first = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(offset, 4));
        if (first <= MaxTowerLength
            && offset + 4 + first <= payload.Length
            && LooksLikeTower(payload.Slice(offset + 4, checked((int)first))))
        {
            tower = payload.Slice(offset + 4, checked((int)first)).ToArray();
            bytesRead = 4 + tower.Length;
            return true;
        }

        if (offset + 8 > payload.Length)
        {
            return false;
        }

        uint length = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(offset + 4, 4));
        if (first == length
            && length <= MaxTowerLength
            && offset + 8 + length <= payload.Length
            && LooksLikeTower(payload.Slice(offset + 8, checked((int)length))))
        {
            tower = payload.Slice(offset + 8, checked((int)length)).ToArray();
            bytesRead = 8 + tower.Length;
            return true;
        }

        return false;
    }

    public static void WriteTowerPointee(ref Opc.Classic.Ndr.NdrWriter writer, ReadOnlySpan<byte> tower)
    {
        if (tower.Length > MaxTowerLength)
        {
            throw new ArgumentOutOfRangeException(nameof(tower), "Endpoint mapper tower exceeds MS-RPCE maximum length.");
        }

        writer.WriteUInt32(unchecked((uint)tower.Length));
        writer.WriteUInt32(unchecked((uint)tower.Length));
        writer.WriteRawBytes(tower);
        writer.AlignTo(4);
    }

    private static IPAddress NormalizeAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            return address.MapToIPv4();
        }

        if (Equals(address, IPAddress.Any) || Equals(address, IPAddress.IPv6Any))
        {
            return IPAddress.Loopback;
        }

        return address;
    }

    private static int WriteUuidFloor(Span<byte> buffer, int offset, Guid id, ushort major, ushort minor)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), 19);
        offset += 2;
        buffer[offset++] = UuidProtocolIdentifier;
        bool ok = id.TryWriteBytes(buffer.Slice(offset, 16));
        if (!ok)
        {
            throw new InvalidOperationException("Guid.TryWriteBytes failed unexpectedly.");
        }
        offset += 16;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), major);
        offset += 2;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), 2);
        offset += 2;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), minor);
        return offset + 2;
    }

    private static int WriteSmallFloor(Span<byte> buffer, int offset, byte protocolIdentifier, ReadOnlySpan<byte> rhs)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), 1);
        offset += 2;
        buffer[offset++] = protocolIdentifier;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), checked((ushort)rhs.Length));
        offset += 2;
        rhs.CopyTo(buffer[offset..]);
        return offset + rhs.Length;
    }

    private static bool TryReadUuidFloor(
        ReadOnlySpan<byte> tower,
        ref int offset,
        out Guid id,
        out ushort major,
        out ushort minor)
    {
        id = Guid.Empty;
        major = 0;
        minor = 0;
        if (offset + 25 > tower.Length
            || BinaryPrimitives.ReadUInt16LittleEndian(tower.Slice(offset, 2)) != 19
            || tower[offset + 2] != UuidProtocolIdentifier)
        {
            return false;
        }

        id = new Guid(tower.Slice(offset + 3, 16));
        major = BinaryPrimitives.ReadUInt16LittleEndian(tower.Slice(offset + 19, 2));
        if (BinaryPrimitives.ReadUInt16LittleEndian(tower.Slice(offset + 21, 2)) != 2)
        {
            return false;
        }

        minor = BinaryPrimitives.ReadUInt16LittleEndian(tower.Slice(offset + 23, 2));
        offset += 25;
        return true;
    }

    private static bool TryReadSmallFloor(ReadOnlySpan<byte> tower, ref int offset, byte protocolIdentifier, out ReadOnlySpan<byte> rhs)
    {
        rhs = default;
        if (offset + 5 > tower.Length
            || BinaryPrimitives.ReadUInt16LittleEndian(tower.Slice(offset, 2)) != 1
            || tower[offset + 2] != protocolIdentifier)
        {
            return false;
        }

        ushort rhsLength = BinaryPrimitives.ReadUInt16LittleEndian(tower.Slice(offset + 3, 2));
        if (offset + 5 + rhsLength > tower.Length)
        {
            return false;
        }

        rhs = tower.Slice(offset + 5, rhsLength);
        offset += 5 + rhsLength;
        return true;
    }

    private static bool LooksLikeTower(ReadOnlySpan<byte> tower) =>
        TryDecodeTcpTower(tower, out _);
}
