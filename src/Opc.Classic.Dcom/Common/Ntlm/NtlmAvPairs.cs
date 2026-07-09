// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;

namespace Opc.Classic.Dcom.Internal.Ntlm;

public static class NtlmAvPairs
{
    public const ushort MsvAvEol = 0x0000;
    public const ushort MsvAvFlags = 0x0006;
    public const ushort MsvAvChannelBindings = 0x000A;
    public const uint MsvAvFlagsMic = 0x00000002;

    public static bool HasMicFlag(ReadOnlySpan<byte> targetInformation) =>
        TryGet(targetInformation, MsvAvFlags, out var flagsValue) &&
        flagsValue.Length >= sizeof(uint) &&
        (BinaryPrimitives.ReadUInt32LittleEndian(flagsValue[..sizeof(uint)]) & MsvAvFlagsMic) != 0;

    public static byte[] AddMicFlag(ReadOnlySpan<byte> targetInformation)
    {
        uint flags = 0;
        if (TryGet(targetInformation, MsvAvFlags, out var flagsValue) && flagsValue.Length >= sizeof(uint))
        {
            flags = BinaryPrimitives.ReadUInt32LittleEndian(flagsValue[..sizeof(uint)]);
        }

        Span<byte> value = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(value, flags | MsvAvFlagsMic);
        return AddOrReplace(targetInformation, MsvAvFlags, value);
    }

    public static byte[] AddOrReplace(ReadOnlySpan<byte> targetInformation, ushort avId, ReadOnlySpan<byte> value)
    {
        using var output = new MemoryStream(targetInformation.Length + 4 + value.Length + 4);
        var offset = 0;
        var wroteValue = false;

        while (offset + 4 <= targetInformation.Length)
        {
            var currentAvId = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset, sizeof(ushort)));
            var length = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset + sizeof(ushort), sizeof(ushort)));
            offset += 4;
            if (length > targetInformation.Length - offset)
            {
                throw new ArgumentException("NTLM target information AV_PAIR length is invalid.", nameof(targetInformation));
            }

            if (currentAvId == MsvAvEol)
            {
                if (!wroteValue)
                {
                    Write(output, avId, value);
                }

                Write(output, MsvAvEol, ReadOnlySpan<byte>.Empty);
                return output.ToArray();
            }

            if (currentAvId == avId)
            {
                Write(output, avId, value);
                wroteValue = true;
            }
            else
            {
                Write(output, currentAvId, targetInformation.Slice(offset, length));
            }

            offset += length;
        }

        if (!wroteValue)
        {
            Write(output, avId, value);
        }

        Write(output, MsvAvEol, ReadOnlySpan<byte>.Empty);
        return output.ToArray();
    }

    public static bool TryGet(ReadOnlySpan<byte> targetInformation, ushort avId, out ReadOnlySpan<byte> value)
    {
        var offset = 0;
        while (offset + 4 <= targetInformation.Length)
        {
            var currentAvId = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset, sizeof(ushort)));
            var length = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset + sizeof(ushort), sizeof(ushort)));
            offset += 4;
            if (length > targetInformation.Length - offset)
            {
                break;
            }

            if (currentAvId == avId)
            {
                value = targetInformation.Slice(offset, length);
                return true;
            }

            if (currentAvId == MsvAvEol)
            {
                break;
            }

            offset += length;
        }

        value = ReadOnlySpan<byte>.Empty;
        return false;
    }

    public static void Write(Stream output, ushort avId, ReadOnlySpan<byte> value)
    {
        ArgumentNullException.ThrowIfNull(output);

        Span<byte> header = stackalloc byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(header, avId);
        BinaryPrimitives.WriteUInt16LittleEndian(header[sizeof(ushort)..], checked((ushort)value.Length));
        output.Write(header);
        output.Write(value);
    }
}
