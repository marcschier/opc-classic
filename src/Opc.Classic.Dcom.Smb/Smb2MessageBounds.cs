//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Smb;

internal static class Smb2MessageBounds {
    public static void EnsureBodyWithinDefaultQuota(ReadOnlySpan<byte> source, string messageName) {
        if (source.Length > Smb2Constants.MaxNetBiosFrameSize - Smb2Constants.PacketHeaderSize) {
            throw new Smb2ProtocolException(
                $"{messageName} body length {source.Length} exceeds the SMB2 quota of {Smb2Constants.MaxNetBiosFrameSize} bytes.");
        }
    }

    public static ReadOnlySpan<byte> GetPayloadSlice(
        ReadOnlySpan<byte> source,
        long packetOffset,
        uint length,
        string fieldName) {
        if (length == 0) {
            return ReadOnlySpan<byte>.Empty;
        }

        long bodyOffset = packetOffset - Smb2Constants.PacketHeaderSize;
        long end = bodyOffset + length;
        if (bodyOffset < 0 || bodyOffset > source.Length || end > source.Length) {
            throw new Smb2ProtocolException($"{fieldName} offset out of range.");
        }

        if (length > Smb2Constants.MaxNetBiosFrameSize - Smb2Constants.PacketHeaderSize) {
            throw new Smb2ProtocolException(
                $"{fieldName} length {length} exceeds the SMB2 payload quota of {Smb2Constants.MaxNetBiosFrameSize} bytes.");
        }

        return source.Slice((int)bodyOffset, checked((int)length));
    }
}
