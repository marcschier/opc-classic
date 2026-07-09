// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Tests.Orpc;

public sealed class OrpcEnvelopeTests
{
    [Test]
    public async Task OrpcThis_round_trips_default_values()
    {
        var expected = new OrpcThis();

        byte[] bytes = WriteOrpcThis(expected);
        OrpcThis actual = ReadOrpcThis(bytes, out int position);

        await Assert.That(actual.Version).IsEqualTo(OrpcComVersion.Default);
        await Assert.That(actual.Flags).IsEqualTo(0u);
        await Assert.That(actual.CausalityId).IsEqualTo(expected.CausalityId);
        await Assert.That(actual.Extensions is null).IsTrue();
        await Assert.That(position).IsEqualTo(bytes.Length);
    }

    [Test]
    public async Task OrpcThis_round_trips_empty_extension_array()
    {
        var expected = new OrpcThis
        {
            CausalityId = Guid.NewGuid(),
            Extensions = Array.Empty<OrpcExtent>(),
        };

        byte[] bytes = WriteOrpcThis(expected, 64);
        OrpcThis actual = ReadOrpcThis(bytes, out int position);

        await Assert.That(actual.Version).IsEqualTo(OrpcComVersion.Default);
        await Assert.That(actual.Flags).IsEqualTo(0u);
        await Assert.That(actual.CausalityId).IsEqualTo(expected.CausalityId);
        await Assert.That(actual.Extensions is not null).IsTrue();
        await Assert.That(actual.Extensions!.Count).IsEqualTo(0);
        await Assert.That(position).IsEqualTo(bytes.Length);
    }

    [Test]
    public async Task OrpcThat_round_trips_default_values()
    {
        var expected = new OrpcThat();

        byte[] bytes = WriteOrpcThat(expected);
        OrpcThat actual = ReadOrpcThat(bytes, out int position);

        await Assert.That(actual.Flags).IsEqualTo(0u);
        await Assert.That(actual.Extensions is null).IsTrue();
        await Assert.That(position).IsEqualTo(bytes.Length);
    }

    [Test]
    public async Task OrpcThis_ends_on_four_byte_alignment()
    {
        (int payloadStart, int finalPosition) = WriteOrpcThisThenUInt32();

        await Assert.That(payloadStart).IsEqualTo(OrpcThis.NullExtensionsWireSize);
        await Assert.That(payloadStart % 4).IsEqualTo(0);
        await Assert.That(finalPosition).IsEqualTo(payloadStart + sizeof(uint));
    }

    private static OrpcThis ReadOrpcThis(byte[] bytes, out int position)
    {
        var reader = new NdrReader(bytes);
        OrpcThis value = OrpcThis.Read(ref reader);
        position = reader.Position;
        return value;
    }

    private static OrpcThat ReadOrpcThat(byte[] bytes, out int position)
    {
        var reader = new NdrReader(bytes);
        OrpcThat value = OrpcThat.Read(ref reader);
        position = reader.Position;
        return value;
    }

    private static (int PayloadStart, int FinalPosition) WriteOrpcThisThenUInt32()
    {
        byte[] buffer = new byte[OrpcThis.NullExtensionsWireSize + sizeof(uint)];
        var writer = new NdrWriter(buffer);
        new OrpcThis().Write(ref writer);
        int payloadStart = writer.Position;
        writer.WriteUInt32(0x11223344u);
        return (payloadStart, writer.Position);
    }

    private static byte[] WriteOrpcThis(OrpcThis value, int capacity = OrpcThis.NullExtensionsWireSize)
    {
        byte[] buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        value.Write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static byte[] WriteOrpcThat(OrpcThat value, int capacity = OrpcThat.NullExtensionsWireSize)
    {
        byte[] buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        value.Write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }
}
