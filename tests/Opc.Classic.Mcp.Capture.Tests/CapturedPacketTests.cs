//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CapturedPacketTests
{
    [Test]
    public async Task RecordEquality_SameMemoryAndAnnotations_AreEqual()
    {
        byte[] bytes = [0x01, 0x02, 0x03];
        var timestamp = new DateTimeOffset(2026, 6, 7, 12, 0, 0, TimeSpan.Zero);
        IReadOnlyDictionary<string, string?> annotations = new Dictionary<string, string?>
        {
            ["source_file"] = "call.hex",
        };
        var left = new CapturedPacket(timestamp, bytes.Length, bytes, 1, annotations);
        var right = new CapturedPacket(timestamp, bytes.Length, bytes, 1, annotations);

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.Data.ToArray().SequenceEqual(bytes)).IsTrue();
        await Assert.That(left.Annotations["source_file"]).IsEqualTo("call.hex");
    }

    [Test]
    public async Task WithExpression_ChangesOnlyRequestedProperty()
    {
        byte[] bytes = [0xAA];
        var timestamp = new DateTimeOffset(2026, 6, 7, 12, 0, 0, TimeSpan.Zero);
        var packet = new CapturedPacket(timestamp, 64, bytes, 101, new Dictionary<string, string?>());

        CapturedPacket changed = packet with { OriginalLength = 128 };

        await Assert.That(changed.Timestamp).IsEqualTo(timestamp);
        await Assert.That(changed.OriginalLength).IsEqualTo(128);
        await Assert.That(changed.LinkType).IsEqualTo(101);
        await Assert.That(packet.OriginalLength).IsEqualTo(64);
    }
}
