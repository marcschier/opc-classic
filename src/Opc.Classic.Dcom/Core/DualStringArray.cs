// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Represents array of network address and security bindings.
/// </summary>
[Serializable]
internal sealed class DualStringArray
{
    /// <summary>
    /// String bindings
    /// </summary>
    public StringBinding[] StringBindings { get; private set; }

    /// <summary>
    /// Security bindings
    /// </summary>
    public SecurityBinding[] SecurityBindings { get; private set; }

    /// <summary>
    /// Length
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Create array
    /// </summary>
    private DualStringArray() { }

    /// <summary>
    /// Will get called from Oxid Resolver
    /// </summary>
    /// <param name="port">Network port used by the RPC endpoint or string binding.</param>
    internal DualStringArray(int port)
    {
        // create bindings here.
        StringBindings = new StringBinding[2]; // only 1
        StringBindings[0] = new StringBinding(port, false);

        Length = StringBindings[0].Length;

        StringBindings[1] = new StringBinding(port, true);

        Length = Length + StringBindings[1].Length + 2; // null termination

        _secOffset = Length;

        SecurityBindings = new SecurityBinding[1]; // support only winnt NTLM
        SecurityBindings[0] = new SecurityBinding(0x0a, 0xffff, "");
        Length += SecurityBindings[0].Length;
        // null termination, 2 bytes for num entries and 2 bytes for sec offset.
        Length = Length + 2 + 2 + 2;
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <returns>A new <see cref="DualStringArray"/> instance built from <paramref name="ndr"/>.</returns>
    internal static DualStringArray Decode(NdrCodec ndr)
    {
        (int numEntries, int securityOffset) = ReadHeader(ndr);

        if (securityOffset > numEntries)
        {
            throw new InvalidDataException(
                $"DUALSTRINGARRAY security offset {securityOffset} exceeds the declared {numEntries} entries.");
        }

        if (numEntries == 0)
        {
            return new DualStringArray
            {
                StringBindings = [],
                SecurityBindings = [],
                Length = sizeof(ushort) + sizeof(ushort),
                _secOffset = 0,
            };
        }

        if (securityOffset == 0)
        {
            throw new InvalidDataException("DUALSTRINGARRAY is missing the string-binding terminator.");
        }

        if (securityOffset == numEntries)
        {
            throw new InvalidDataException("DUALSTRINGARRAY is missing the security-binding terminator.");
        }

        byte[] payload = ReadPayload(ndr, numEntries);
        int payloadLength = payload.Length;

        int securityOffsetBytes = securityOffset * sizeof(ushort);
        var stringCodec = CreateSectionCodec(payload, 0, securityOffsetBytes);
        var securityCodec = CreateSectionCodec(payload, securityOffsetBytes, payloadLength - securityOffsetBytes);

        var dualStringArray = new DualStringArray
        {
            StringBindings = DecodeStringBindings(stringCodec, securityOffsetBytes),
            SecurityBindings = DecodeSecurityBindings(securityCodec, payloadLength),
            Length = sizeof(ushort) + sizeof(ushort) + payloadLength,
            _secOffset = securityOffsetBytes,
        };
        return dualStringArray;
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    public void Encode(NdrCodec ndr)
    {
        // fill num entries
        // this is total length/2. since they are all shorts
        ndr.WriteUnsignedShort((Length - 4) / 2);
        ndr.WriteUnsignedShort(_secOffset / 2);

        if (Length == sizeof(ushort) + sizeof(ushort))
        {
            return;
        }

        var i = 0;
        if (StringBindings != null)
        {
            while (i < StringBindings.Length)
            {
                StringBindings[i].Encode(ndr);
                i++;
            }
            ndr.WriteUnsignedShort(0);
        }

        i = 0;
        if (SecurityBindings != null)
        {
            while (i < SecurityBindings.Length)
            {
                SecurityBindings[i].Encode(ndr);
                i++;
            }
            ndr.WriteUnsignedShort(0);
        }
    }

    private int _secOffset;

    private static (int NumEntries, int SecurityOffset) ReadHeader(NdrCodec ndr)
    {
        try
        {
            return (ndr.ReadUnsignedShort(), ndr.ReadUnsignedShort());
        }
        catch (EndOfStreamException ex)
        {
            throw new EndOfStreamException("DUALSTRINGARRAY header is truncated.", ex);
        }
    }

    private static byte[] ReadPayload(NdrCodec ndr, int numEntries)
    {
        int payloadLength = checked(numEntries * sizeof(ushort));
        var payload = new byte[payloadLength];
        try
        {
            ndr.ReadOctetArray(payload, 0, payload.Length);
            return payload;
        }
        catch (EndOfStreamException ex)
        {
            throw new EndOfStreamException(
                $"DUALSTRINGARRAY is truncated: declared {numEntries} entries ({payloadLength} bytes).",
                ex);
        }
    }

    private static NdrCodec CreateSectionCodec(byte[] payload, int offset, int length)
    {
        var buffer = new NdrBuffer(payload, offset);
        buffer.SetLength(length);
        return new NdrCodec { Buffer = buffer };
    }

    private static StringBinding[] DecodeStringBindings(NdrCodec ndr, int sectionEnd)
    {
        var bindings = new List<StringBinding>();
        try
        {
            while (ndr.Buffer.Index < sectionEnd)
            {
                StringBinding binding = StringBinding.Decode(ndr);
                if (binding is not null)
                {
                    bindings.Add(binding);
                    continue;
                }

                if (ndr.Buffer.Index != sectionEnd)
                {
                    throw new InvalidDataException(
                        "DUALSTRINGARRAY string bindings terminated before the declared security offset.");
                }

                return bindings.ToArray();
            }
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidDataException(
                "DUALSTRINGARRAY string bindings are unterminated at the declared security offset.",
                ex);
        }

        throw new InvalidDataException(
            "DUALSTRINGARRAY string bindings do not terminate at the declared security offset.");
    }

    private static SecurityBinding[] DecodeSecurityBindings(NdrCodec ndr, int sectionEnd)
    {
        var bindings = new List<SecurityBinding>();
        try
        {
            while (ndr.Buffer.Index < sectionEnd)
            {
                SecurityBinding binding = SecurityBinding.Decode(ndr);
                if (binding is not null)
                {
                    bindings.Add(binding);
                    continue;
                }

                if (ndr.Buffer.Index != sectionEnd)
                {
                    throw new InvalidDataException(
                        "DUALSTRINGARRAY security bindings contain trailing data after their terminator.");
                }

                return bindings.ToArray();
            }
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidDataException(
                "DUALSTRINGARRAY security bindings are unterminated within the declared entry count.",
                ex);
        }

        throw new InvalidDataException(
            "DUALSTRINGARRAY security bindings are missing their final terminator.");
    }
}
