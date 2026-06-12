//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Exercises NdrReader/NdrVariantExtensions decode-fail messages:
// must include a multi-line hex window centered on the failure offset so
// a developer can compare the failing wire bytes against a Wireshark
// capture or canonical MIDL layout.
//

using Opc.Classic.Ndr;

namespace Opc.Classic.Tests;

public sealed class NdrReaderHexContextTests
{
    private delegate T NdrReadFunc<T>(ref NdrReader reader);

    private static (bool Threw, string Message) TryRead(byte[] bytes, NdrReadFunc<object?> read)
    {
        try
        {
            var reader = new NdrReader(bytes);
            read(ref reader);
            return (false, string.Empty);
        }
        catch (InvalidOperationException ex)
        {
            return (true, ex.Message);
        }
        catch (System.IO.InvalidDataException ex)
        {
            return (true, ex.Message);
        }
    }

    [Test]
    public async Task LpwstrOffsetMismatch_ExceptionMessageIncludesHexWindow()
    {
        // A 32-byte buffer that decodes as an LPWSTR with offset != 0; the
        // reader must throw and the message must carry a hex window showing
        // the failure position.
        var bytes = new byte[]
        {
            0x10, 0x00, 0x00, 0x00,   // max_count = 16
            0x55, 0xAA, 0x55, 0xAA,   // offset (non-zero) — failure trigger
            0x10, 0x00, 0x00, 0x00,   // actual_count = 16
            0x41, 0x00, 0x42, 0x00, 0x43, 0x00, 0x44, 0x00,
            0x45, 0x00, 0x46, 0x00, 0x47, 0x00, 0x48, 0x00,
            0x49, 0x00, 0x4A, 0x00,
        };

        (bool threw, string message) = TryRead(bytes, static (ref NdrReader r) => r.ReadUnicodeString());

        await Assert.That(threw).IsTrue();
        await Assert.That(message.Contains("offset must be 0", StringComparison.Ordinal)).IsTrue();
        await Assert.That(message.Contains("Wire context", StringComparison.Ordinal)).IsTrue();
        await Assert.That(message.Contains(">>", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task FormatHexContext_RendersBoundedWindowAroundPosition()
    {
        byte[] bytes = new byte[64];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)i;
        }

        string formatted = NdrReader.FormatHexContext(bytes, position: 32, contextBytes: 8);

        await Assert.That(formatted.Contains("Wire context", StringComparison.Ordinal)).IsTrue();
        await Assert.That(formatted.Contains("position 32", StringComparison.Ordinal)).IsTrue();
        await Assert.That(formatted.Contains(">>", StringComparison.Ordinal)).IsTrue();
        // The marker should appear immediately before the byte at the failure
        // offset (0x20 == 32); guard against accidental off-by-one regressions.
        await Assert.That(formatted.Contains(">>20", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task FormatHexContext_EmptyBufferReturnsEmptyString()
    {
        string formatted = NdrReader.FormatHexContext(ReadOnlySpan<byte>.Empty, position: 0);

        await Assert.That(formatted).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task FormatHexContext_PositionPastBufferStillRenders()
    {
        byte[] bytes = new byte[8];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(0xA0 + i);
        }

        // When EnsureAvailable fails, _position can equal _buffer.Length; the
        // helper must still produce a useful window of the trailing bytes
        // (without a >> marker, since the position is at end-of-buffer).
        string formatted = NdrReader.FormatHexContext(bytes, position: bytes.Length, contextBytes: 16);

        await Assert.That(formatted.Contains("Wire context", StringComparison.Ordinal)).IsTrue();
        await Assert.That(formatted.Contains("a0 a1", StringComparison.Ordinal)).IsTrue();
    }
}
