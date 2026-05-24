// SPDX-License-Identifier: MIT

using System;
using System.Globalization;

namespace SharpCifs.Dcerpc;

public sealed class Uuid {
    public Uuid(string value) => Parse(value);

    public int TimeLow { get; set; }

    public short TimeMid { get; set; }

    public short TimeHiAndVersion { get; set; }

    public byte ClockSeqHiAndReserved { get; set; }

    public byte ClockSeqLow { get; set; }

    public byte[] Node { get; set; } = new byte[6];

    public void Parse(string value) {
        var parts = value.Split('-');
        if (parts.Length != 5 || parts[3].Length != 4 || parts[4].Length != 12) {
            throw new FormatException("Invalid UUID format.");
        }

        TimeLow = unchecked((int)uint.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        TimeMid = unchecked((short)ushort.Parse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        TimeHiAndVersion = unchecked((short)ushort.Parse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        ClockSeqHiAndReserved = byte.Parse(parts[3][..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        ClockSeqLow = byte.Parse(parts[3][2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        Node = new byte[6];
        for (var i = 0; i < Node.Length; i++) {
            Node[i] = byte.Parse(parts[4].AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
    }

    public override string ToString() =>
        $"{(uint)TimeLow:x8}-{(ushort)TimeMid:x4}-{(ushort)TimeHiAndVersion:x4}-" +
        $"{ClockSeqHiAndReserved:x2}{ClockSeqLow:x2}-{Convert.ToHexString(Node).ToLowerInvariant()}";
}