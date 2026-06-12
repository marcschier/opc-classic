//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Round-trip + alignment + edge-case tests for the managed NDR writer/reader pair.
//
// IMPORTANT: NdrReader and NdrWriter are ref structs containing Span<T>.
// They CANNOT be preserved across `await` boundaries (CS4007). All test
// methods follow the pattern: do all NDR work in a synchronous helper that
// produces value-type outputs, then await the assertions on those outputs.
//

using Opc.Classic.Ndr;

namespace Opc.Classic.Tests;

public sealed class NdrPrimitiveTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 64)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
            throw new InvalidOperationException("expected " + typeof(T).Name + " but none was thrown");
        }
        catch (T)
        {
            // expected
        }
    }

    // -------- Byte / Boolean --------

    [Test]
    public async Task WriteByte_ProducesSingleByteAtPositionZero()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteByte(0xAB));
        await Assert.That(bytes.Length).IsEqualTo(1);
        await Assert.That(bytes[0]).IsEqualTo((byte)0xAB);
    }

    [Test]
    public async Task ReadByte_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteByte(0x7F));
        byte read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadByte();
        }
        await Assert.That(read).IsEqualTo((byte)0x7F);
    }

    [Test]
    public async Task Boolean_RoundTrips_True()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteBoolean(true));
        bool read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadBoolean();
        }
        await Assert.That(bytes[0]).IsEqualTo((byte)1);
        await Assert.That(read).IsTrue();
    }

    [Test]
    public async Task Boolean_RoundTrips_False()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteBoolean(false));
        await Assert.That(bytes[0]).IsEqualTo((byte)0);
    }

    // -------- Integers (little-endian + alignment) --------

    [Test]
    public async Task Int32_IsLittleEndian()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteInt32(0x12345678));
        await Assert.That(bytes[0]).IsEqualTo((byte)0x78);
        await Assert.That(bytes[1]).IsEqualTo((byte)0x56);
        await Assert.That(bytes[2]).IsEqualTo((byte)0x34);
        await Assert.That(bytes[3]).IsEqualTo((byte)0x12);
    }

    [Test]
    public async Task Int32_AlignsTo4_AfterByte()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteByte(0x01);
            w.WriteInt32(0x44332211);
        });
        await Assert.That(bytes.Length).IsEqualTo(8);
        await Assert.That(bytes[0]).IsEqualTo((byte)0x01);
        await Assert.That(bytes[1]).IsEqualTo((byte)0x00);
        await Assert.That(bytes[2]).IsEqualTo((byte)0x00);
        await Assert.That(bytes[3]).IsEqualTo((byte)0x00);
        await Assert.That(bytes[4]).IsEqualTo((byte)0x11);
        await Assert.That(bytes[7]).IsEqualTo((byte)0x44);
    }

    [Test]
    public async Task Int32_RoundTrips_WithAlignment()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteByte(0x01);
            w.WriteInt32(unchecked((int)0xDEADBEEFu));
        });
        byte readByte;
        int readInt;
        {
            var r = new NdrReader(bytes);
            readByte = r.ReadByte();
            readInt = r.ReadInt32();
        }
        await Assert.That(readByte).IsEqualTo((byte)0x01);
        await Assert.That(readInt).IsEqualTo(unchecked((int)0xDEADBEEFu));
    }

    [Test]
    public async Task UInt16_AlignsTo2_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteByte(0xFF);
            w.WriteUInt16(0xCAFE);
        });
        byte readByte;
        ushort readShort;
        {
            var r = new NdrReader(bytes);
            readByte = r.ReadByte();
            readShort = r.ReadUInt16();
        }
        await Assert.That(bytes.Length).IsEqualTo(4);
        await Assert.That(readByte).IsEqualTo((byte)0xFF);
        await Assert.That(readShort).IsEqualTo((ushort)0xCAFE);
    }

    [Test]
    public async Task Int64_AlignsTo8_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteByte(0x42);
            w.WriteInt64(0x0102030405060708L);
        });
        byte readByte;
        long readLong;
        {
            var r = new NdrReader(bytes);
            readByte = r.ReadByte();
            readLong = r.ReadInt64();
        }
        await Assert.That(bytes.Length).IsEqualTo(16);
        await Assert.That(readByte).IsEqualTo((byte)0x42);
        await Assert.That(readLong).IsEqualTo(0x0102030405060708L);
    }

    [Test]
    public async Task UInt32_NegativeBitPattern_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteUInt32(0xFFFFFFFFu));
        uint read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadUInt32();
        }
        await Assert.That(read).IsEqualTo(0xFFFFFFFFu);
    }

    // -------- Floats --------

    [Test]
    public async Task Single_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteSingle(3.14159f));
        float read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadSingle();
        }
        await Assert.That(read).IsEqualTo(3.14159f);
    }

    [Test]
    public async Task Double_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteDouble(2.71828182845904));
        double read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadDouble();
        }
        await Assert.That(read).IsEqualTo(2.71828182845904);
    }

    [Test]
    public async Task Double_AlignsTo8()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteInt32(0x11223344);
            w.WriteDouble(1.0);
        });
        await Assert.That(bytes.Length).IsEqualTo(16);
    }

    // -------- GUID + FILETIME --------

    [Test]
    public async Task Guid_RoundTrips()
    {
        var input = new Guid("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        var bytes = WriteOne((ref NdrWriter w) => w.WriteGuid(input));
        Guid read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadGuid();
        }
        await Assert.That(bytes.Length).IsEqualTo(16);
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task FileTime_RoundTrips_AsTwoLittleEndianHalves()
    {
        const long ticks = 0x01D9_1234_5678_9ABC;
        var bytes = WriteOne((ref NdrWriter w) => w.WriteFileTime(ticks));
        long read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadFileTime();
        }
        await Assert.That(bytes.Length).IsEqualTo(8);
        await Assert.That(read).IsEqualTo(ticks);
    }

    [Test]
    public async Task FileTime_AlignsTo4_NotTo8()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteByte(0xFF);
            w.WriteFileTime(0x0123456789ABCDEFL);
        });
        await Assert.That(bytes.Length).IsEqualTo(12);
    }

    // -------- Conformance header --------

    [Test]
    public async Task ConformanceHeader_WritesAsUInt32()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteConformanceHeader(42));
        int read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadConformanceHeader();
        }
        await Assert.That(bytes.Length).IsEqualTo(4);
        await Assert.That(read).IsEqualTo(42);
    }

    [Test]
    public async Task ConformanceHeader_Negative_Throws()
    {
        Throws<ArgumentOutOfRangeException>(() =>
        {
            var buf = new byte[16];
            var w = new NdrWriter(buf);
            w.WriteConformanceHeader(-1);
        });
        await Task.CompletedTask;
    }

    // -------- Referent IDs --------

    [Test]
    public async Task ReferentId_NonZero_StartsAtConventionalValue()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteReferentId());
        bool nonNull;
        uint id;
        {
            var r = new NdrReader(bytes);
            nonNull = r.TryReadReferentId(out id);
        }
        await Assert.That(nonNull).IsTrue();
        await Assert.That(id).IsEqualTo(0x00020000u);
    }

    [Test]
    public async Task ReferentId_AssignsIncrementingValues()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteReferentId();
            w.WriteReferentId();
        });
        uint id1, id2;
        {
            var r = new NdrReader(bytes);
            r.TryReadReferentId(out id1);
            r.TryReadReferentId(out id2);
        }
        await Assert.That(id2 - id1).IsEqualTo(4u);
    }

    [Test]
    public async Task NullReferent_DecodesAsNull()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteNullReferent());
        bool nonNull;
        uint id;
        {
            var r = new NdrReader(bytes);
            nonNull = r.TryReadReferentId(out id);
        }
        await Assert.That(nonNull).IsFalse();
        await Assert.That(id).IsEqualTo(0u);
    }

    // -------- Unicode string --------

    [Test]
    public async Task UnicodeString_RoundTrips_BasicAscii()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteUnicodeString("Hello"));
        string read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadUnicodeString();
        }
        await Assert.That(read).IsEqualTo("Hello");
    }

    [Test]
    public async Task UnicodeString_EmptyString_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteUnicodeString(string.Empty));
        string read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadUnicodeString();
        }
        await Assert.That(read).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task UnicodeString_NonAscii_RoundTrips()
    {
        var input = "Ä-中文-🙂";
        var bytes = WriteOne((ref NdrWriter w) => w.WriteUnicodeString(input), capacity: 128);
        string read;
        {
            var r = new NdrReader(bytes);
            read = r.ReadUnicodeString();
        }
        await Assert.That(read).IsEqualTo(input);
    }

    [Test]
    public async Task UnicodeString_IncludesNullTerminatorOnWire()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteUnicodeString("AB"));
        await Assert.That(bytes.Length).IsEqualTo(18);
        await Assert.That(BitConverter.ToUInt32(bytes, 0)).IsEqualTo(3u);
        await Assert.That(BitConverter.ToUInt32(bytes, 8)).IsEqualTo(3u);
        await Assert.That(bytes[16]).IsEqualTo((byte)0);
        await Assert.That(bytes[17]).IsEqualTo((byte)0);
    }

    // -------- Buffer overflow detection --------

    [Test]
    public async Task WriteInt32_BufferTooSmall_Throws()
    {
        Throws<InvalidOperationException>(() =>
        {
            var buf = new byte[2];
            var w = new NdrWriter(buf);
            w.WriteInt32(123);
        });
        await Task.CompletedTask;
    }

    [Test]
    public async Task ReadInt32_PastEnd_Throws()
    {
        Throws<InvalidOperationException>(() =>
        {
            var r = new NdrReader(new byte[] { 0x01, 0x02 });
            _ = r.ReadInt32();
        });
        await Task.CompletedTask;
    }

    // -------- Alignment edge cases --------

    [Test]
    public async Task AlignTo_InvalidBoundary_Throws()
    {
        Throws<ArgumentOutOfRangeException>(() =>
        {
            var buf = new byte[16];
            var w = new NdrWriter(buf);
            w.AlignTo(3);
        });
        await Task.CompletedTask;
    }

    [Test]
    public async Task AlignTo_AlreadyAligned_DoesNothing()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteInt32(1);
            w.AlignTo(4);
            w.WriteInt32(2);
        });
        await Assert.That(bytes.Length).IsEqualTo(8);
    }

    [Test]
    public async Task Position_TracksWrites()
    {
        var buf = new byte[64];
        int p1, p2, p3;
        {
            var w = new NdrWriter(buf);
            w.WriteByte(1); p1 = w.Position;
            w.WriteInt32(2); p2 = w.Position;
            w.WriteInt64(3); p3 = w.Position;
        }
        await Assert.That(p1).IsEqualTo(1);
        await Assert.That(p2).IsEqualTo(8);
        await Assert.That(p3).IsEqualTo(16);
    }

    // -------- Composite round-trip --------

    [Test]
    public async Task CompositeStructure_RoundTrips()
    {
        var inputGuid = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00");
        const long inputTime = 0x01_DA_AB_CD_EF_01_23_45;

        var bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteByte(0x01);
            w.WriteInt32(unchecked((int)0xC0040001u));
            w.WriteGuid(inputGuid);
            w.WriteFileTime(inputTime);
            w.WriteUnicodeString("Tag.Value");
        }, capacity: 128);

        byte rByte;
        int rInt;
        Guid rGuid;
        long rTime;
        string rStr;
        {
            var r = new NdrReader(bytes);
            rByte = r.ReadByte();
            rInt = r.ReadInt32();
            rGuid = r.ReadGuid();
            rTime = r.ReadFileTime();
            rStr = r.ReadUnicodeString();
        }
        await Assert.That(rByte).IsEqualTo((byte)0x01);
        await Assert.That(rInt).IsEqualTo(unchecked((int)0xC0040001u));
        await Assert.That(rGuid).IsEqualTo(inputGuid);
        await Assert.That(rTime).IsEqualTo(inputTime);
        await Assert.That(rStr).IsEqualTo("Tag.Value");
    }
}
