// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Globalization;
using System.Text;

namespace Opc.Classic.SnapshotTests.Support;

internal static class HexDumpFormatter
{
    public static string Format(string codecName, string sampleDescription, ReadOnlySpan<byte> bytes)
    {
        var builder = new StringBuilder();
        builder.Append("Codec: ").Append(codecName).Append(' ').AppendLine(sampleDescription);
        builder.Append("Bytes: ").AppendLine(bytes.Length.ToString(CultureInfo.InvariantCulture));
        builder.AppendLine();

        for (int offset = 0; offset < bytes.Length; offset += 16)
        {
            int count = Math.Min(16, bytes.Length - offset);
            builder.Append(offset.ToString("x8", CultureInfo.InvariantCulture)).Append("  ");

            for (int i = 0; i < 16; i++)
            {
                if (i < count)
                {
                    builder.Append(bytes[offset + i].ToString("x2", CultureInfo.InvariantCulture));
                }
                else
                {
                    builder.Append("  ");
                }

                builder.Append(i == 7 ? "  " : " ");
            }

            builder.Append(" | ");
            for (int i = 0; i < count; i++)
            {
                byte value = bytes[offset + i];
                builder.Append(value is >= 0x20 and <= 0x7E ? (char)value : '.');
            }
            builder.AppendLine("|");
        }

        return builder.ToString();
    }
}
