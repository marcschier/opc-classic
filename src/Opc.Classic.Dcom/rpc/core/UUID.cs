// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Globalization;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Parsed wire representation of a DCE/MSRPC UUID (RFC 4122 §4.1.2). Holds
/// the 5-field little-endian DCE layout (TimeLow / TimeMid /
/// TimeHiAndVersion / ClockSeqHiAndReserved / ClockSeqLow / Node[6]) so the
/// fields can be NDR-encoded directly without going through
/// <see cref="System.Guid"/>'s by-component decomposition.
/// </summary>
public class UUID : NdrOp
{

    /// <summary>
    /// Canonical "nil" UUID (all zero bytes) per RFC 4122 §4.1.7.
    /// </summary>
    public const string NIL_UUID = "00000000-0000-0000-0000-000000000000";

    /// <summary>
    /// Create a UUID equal to <see cref="NIL_UUID"/>.
    /// </summary>
    public UUID() : this(NIL_UUID)
    {
    }

    /// <summary>
    /// Create a UUID from an RFC 4122 8-4-4-4-12 hexadecimal string.
    /// </summary>
    /// <param name="str">Canonical UUID string such as <c>00000131-0000-0000-c000-000000000046</c>.</param>
    public UUID(string str) => Parse(str);

    private int _timeLow;
    private short _timeMid;
    private short _timeHiAndVersion;
    private byte _clockSeqHiAndReserved;
    private byte _clockSeqLow;
    private byte[] _node = new byte[6];

    /// <inheritdoc/>
    public override void Encode(NdrCodec ndr, NdrBuffer dst)
    {
        dst.Enc_ndr_long(_timeLow);
        dst.Enc_ndr_short(_timeMid);
        dst.Enc_ndr_short(_timeHiAndVersion);
        dst.Enc_ndr_small(_clockSeqHiAndReserved);
        dst.Enc_ndr_small(_clockSeqLow);
        Array.Copy(_node, 0, dst.Buf, dst.Index, _node.Length);
        dst.Index += _node.Length;
    }

    /// <inheritdoc/>
    public override void Decode(NdrCodec ndr, NdrBuffer src)
    {
        _timeLow = src.Dec_ndr_long();
        _timeMid = (short)src.Dec_ndr_short();
        _timeHiAndVersion = (short)src.Dec_ndr_short();
        _clockSeqHiAndReserved = (byte)src.Dec_ndr_small();
        _clockSeqLow = (byte)src.Dec_ndr_small();
        _node = new byte[6];
        Array.Copy(src.Buf, src.Index, _node, 0, _node.Length);
        src.Index += _node.Length;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{(uint)_timeLow:x8}-{(ushort)_timeMid:x4}-{(ushort)_timeHiAndVersion:x4}-" +
        $"{_clockSeqHiAndReserved:x2}{_clockSeqLow:x2}-{Convert.ToHexString(_node).ToLowerInvariant()}";

    /// <summary>
    /// Parse an RFC 4122 8-4-4-4-12 hexadecimal string into the five DCE
    /// fields. Throws <see cref="FormatException"/> when the string does not
    /// match the canonical UUID form.
    /// </summary>
    /// <param name="uuid">Canonical UUID string such as <c>00000131-0000-0000-c000-000000000046</c>.</param>
    public void Parse(string uuid)
    {
        var parts = uuid.Split('-');
        if (parts.Length != 5 || parts[3].Length != 4 || parts[4].Length != 12)
        {
            throw new FormatException("Invalid UUID format.");
        }

        _timeLow = unchecked((int)uint.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        _timeMid = unchecked((short)ushort.Parse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        _timeHiAndVersion = unchecked((short)ushort.Parse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        _clockSeqHiAndReserved = byte.Parse(parts[3][..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        _clockSeqLow = byte.Parse(parts[3][2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        _node = new byte[6];
        for (var i = 0; i < _node.Length; i++)
        {
            _node[i] = byte.Parse(parts[4].AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
    }
}
