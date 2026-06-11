//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading.Tasks;
using CsCheck;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.PropertyTests.Codecs;

public sealed class NdrPrimitivesRoundTripTests
{
    [Test]
    public Task Byte_RoundTrips()
    {
        Gen.Byte.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, byte v) => writer.WriteByte(v),
            static (ref NdrReader reader) => reader.ReadByte()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task SByte_RoundTrips()
    {
        Gen.SByte.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, sbyte v) => writer.WriteInt8(v),
            static (ref NdrReader reader) => reader.ReadInt8()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Int16_RoundTrips()
    {
        Gen.Short.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, short v) => writer.WriteInt16(v),
            static (ref NdrReader reader) => reader.ReadInt16()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task UInt16_RoundTrips()
    {
        Gen.UShort.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, ushort v) => writer.WriteUInt16(v),
            static (ref NdrReader reader) => reader.ReadUInt16()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Int32_RoundTrips()
    {
        Gen.Int.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, int v) => writer.WriteInt32(v),
            static (ref NdrReader reader) => reader.ReadInt32()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task UInt32_RoundTrips()
    {
        Gen.UInt.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, uint v) => writer.WriteUInt32(v),
            static (ref NdrReader reader) => reader.ReadUInt32()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Int64_RoundTrips()
    {
        Gen.Long.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, long v) => writer.WriteInt64(v),
            static (ref NdrReader reader) => reader.ReadInt64()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task UInt64_RoundTrips()
    {
        Gen.ULong.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, ulong v) => writer.WriteUInt64(v),
            static (ref NdrReader reader) => reader.ReadUInt64()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Single_RoundTrips_BitExactly()
    {
        Gen.Single.Sample(value => CodecProperty.RoundTrips(
            value,
            static (ref NdrWriter writer, float v) => writer.WriteSingle(v),
            static (ref NdrReader reader) => reader.ReadSingle(),
            static (left, right) => BitConverter.SingleToUInt32Bits(left) == BitConverter.SingleToUInt32Bits(right)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Double_RoundTrips_BitExactly()
    {
        Gen.Double.Sample(value => CodecProperty.RoundTrips(
            value,
            static (ref NdrWriter writer, double v) => writer.WriteDouble(v),
            static (ref NdrReader reader) => reader.ReadDouble(),
            static (left, right) => BitConverter.DoubleToUInt64Bits(left) == BitConverter.DoubleToUInt64Bits(right)),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Boolean_RoundTrips()
    {
        Gen.Bool.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, bool v) => writer.WriteBoolean(v),
            static (ref NdrReader reader) => reader.ReadBoolean()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Guid_RoundTrips()
    {
        Gen.Guid.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, Guid v) => writer.WriteGuid(v),
            static (ref NdrReader reader) => reader.ReadGuid()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task FileTime_RoundTrips_AllEightBytes()
    {
        Gen.Long.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, long v) => writer.WriteFileTime(v),
            static (ref NdrReader reader) => reader.ReadFileTime()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Lpwstr_RoundTrips_RandomUnicode()
    {
        CodecProperty.MediumStringGen.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, string v) => writer.WriteUnicodeString(v),
            static (ref NdrReader reader) => reader.ReadUnicodeString(),
            capacity: 1024), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Lpwstr_RoundTrips_LongSurrogatePatterns()
    {
        Gen.Int[0, 65535].Sample(length =>
        {
            string value = BuildUnicodePattern(length);
            return CodecProperty.RoundTripsByEquals(
                value,
                static (ref NdrWriter writer, string v) => writer.WriteUnicodeString(v),
                static (ref NdrReader reader) => reader.ReadUnicodeString(),
                capacity: 256 * 1024);
        }, iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task LpwstrPointer_RoundTrips_NullAndUnicode()
    {
        CodecProperty.NullableShortStringGen.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, string? v) => writer.WriteUnicodeStringPtr(v),
            static (ref NdrReader reader) => reader.ReadUnicodeStringPtr()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Bstr_RoundTrips_NullAndUnicode()
    {
        CodecProperty.NullableShortStringGen.Sample(value => CodecProperty.RoundTripsByEquals(
            value,
            static (ref NdrWriter writer, string? v) =>
            {
                if (v is null)
                {
                    writer.WriteNullBstr();
                }
                else
                {
                    writer.WriteBstr(v);
                }
            },
            static (ref NdrReader reader) => reader.ReadBstr()), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    private static string BuildUnicodePattern(int length)
    {
        var chars = new char[length];
        int position = 0;
        while (position < length)
        {
            if (position + 1 < length && (position % 5) == 0)
            {
                chars[position++] = '\uD83D';
                chars[position++] = '\uDE00';
                continue;
            }

            chars[position] = (position % 6) switch
            {
                0 => '\0',
                1 => 'A',
                2 => 'Ω',
                3 => '中',
                4 => '\uD7FF',
                _ => '\uE000',
            };
            position++;
        }
        return new string(chars);
    }
}
