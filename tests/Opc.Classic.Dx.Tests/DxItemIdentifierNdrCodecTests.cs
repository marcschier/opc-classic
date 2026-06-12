//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dx.Ndr;
using Opc.Classic.Ndr;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dx.Tests;

public sealed class DxItemIdentifierNdrCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task ItemIdentifierCodec_RoundTripsAllFields()
    {
        var input = new DxItemIdentifier(
            ItemPath: "DX/DXConnectionsRoot/Area1",
            ItemName: "Tank1_to_HMI",
            Version: "cfg-17",
            Reserved: unchecked((int)0xFFFFFFFFu));

        DxItemIdentifier decoded = RoundTrip(input, NdrOpcDxItemIdentifierCodec.Write, NdrOpcDxItemIdentifierCodec.Read);

        await Assert.That(decoded).IsEqualTo(input);
        await Assert.That(decoded.Reserved).IsEqualTo(-1);
    }

    [Test]
    public async Task ItemIdentifierCodec_RoundTripsNullStrings()
    {
        var input = new DxItemIdentifier(Reserved: 1234);

        DxItemIdentifier decoded = RoundTrip(input, NdrOpcDxItemIdentifierCodec.Write, NdrOpcDxItemIdentifierCodec.Read);

        await Assert.That(decoded.ItemPath).IsNull();
        await Assert.That(decoded.ItemName).IsNull();
        await Assert.That(decoded.Version).IsNull();
        await Assert.That(decoded.Reserved).IsEqualTo(1234);
    }

    [Test]
    public async Task ItemIdentifierCodec_NullValue_ThrowsArgumentNullException()
    {
        await Assert.That(() => WritePayload((ref NdrWriter writer) => NdrOpcDxItemIdentifierCodec.Write(ref writer, null!)))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ItemIdentifierArrayCodec_RoundTripsMultipleIdentifiers()
    {
        DxItemIdentifier[] input =
        [
            new("DX/SourceServers", "PLC1", "cfg-1", 0),
            new("DX/DXConnectionsRoot", "ConnectionA", "cfg-2", 99),
        ];

        ReadOnlyMemory<byte> payload = WritePayload((ref NdrWriter writer) => NdrOpcDxItemIdentifierArrayCodec.Write(ref writer, input));
        var reader = new NdrReader(payload.Span);
        DxItemIdentifier[] decoded = NdrOpcDxItemIdentifierArrayCodec.Read(ref reader);

        await Assert.That(BitConverter.ToUInt32(payload.Span[..4])).IsEqualTo(2u);
        await Assert.That(decoded.Length).IsEqualTo(2);
        await Assert.That(decoded[0]).IsEqualTo(input[0]);
        await Assert.That(decoded[1]).IsEqualTo(input[1]);
    }

    [Test]
    public async Task ItemIdentifierArrayCodec_NullArray_WritesZeroCount()
    {
        ReadOnlyMemory<byte> payload = WritePayload((ref NdrWriter writer) => NdrOpcDxItemIdentifierArrayCodec.Write(ref writer, null));
        var reader = new NdrReader(payload.Span);
        DxItemIdentifier[] decoded = NdrOpcDxItemIdentifierArrayCodec.Read(ref reader);

        await Assert.That(payload.Length).IsEqualTo(4);
        await Assert.That(BitConverter.ToUInt32(payload.Span)).IsEqualTo(0u);
        await Assert.That(decoded.Length).IsEqualTo(0);
    }

    [Test]
    public async Task DxErrorCodec_RoundTripsCodeAndTextIntoResultId()
    {
        var input = new DxError(
            new OpcResultId(unchecked((int)0xC0040703u), "Version mismatch from server"),
            "Version mismatch from server");

        DxError decoded = RoundTrip(input, NdrOpcDxErrorCodec.Write, NdrOpcDxErrorCodec.Read);

        await Assert.That(decoded).IsEqualTo(input);
        await Assert.That(decoded.Id.Code).IsEqualTo(unchecked((int)0xC0040703u));
        await Assert.That(decoded.Id.Description).IsEqualTo("Version mismatch from server");
        await Assert.That(decoded.Text).IsEqualTo("Version mismatch from server");
    }

    [Test]
    public async Task DxErrorCodec_RoundTripsNullTextAsNullDescription()
    {
        var input = new DxError(new OpcResultId(unchecked((int)0x80004005u), null), null);

        DxError decoded = RoundTrip(input, NdrOpcDxErrorCodec.Write, NdrOpcDxErrorCodec.Read);

        await Assert.That(decoded.Id.Code).IsEqualTo(unchecked((int)0x80004005u));
        await Assert.That(decoded.Id.Description).IsNull();
        await Assert.That(decoded.Text).IsNull();
    }

    [Test]
    public async Task DxErrorCodec_NullValue_ThrowsArgumentNullException()
    {
        await Assert.That(() => WritePayload((ref NdrWriter writer) => NdrOpcDxErrorCodec.Write(ref writer, null!)))
            .Throws<ArgumentNullException>();
    }

    private static T RoundTrip<T>(T value, NdrWriteFunc<T> write, NdrReadFunc<T> read)
    {
        ReadOnlyMemory<byte> payload = WritePayload((ref NdrWriter writer) => write(ref writer, value));
        var reader = new NdrReader(payload.Span);
        return read(ref reader);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 2048)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private delegate void NdrWriteFunc<in T>(ref NdrWriter writer, T value);
    private delegate T NdrReadFunc<out T>(ref NdrReader reader);
}
