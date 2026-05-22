//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Threading.Tasks;
using OpcClassic;
using OpcClassic.Da;
using OpcClassic.Da.Ndr;
using OpcClassic.Ndr;
using TUnit.Core;
using VerifyTUnit;

namespace OpcClassic.Da.Tests.Snapshots;

public sealed class NdrOpcItemStateCodecSnapshotTests
{
    [Test]
    public async Task OpcItemState_with_R8_value_round_trips_to_known_bytes()
    {
        var state = new OpcItemState(
            ClientHandle: 0x12345678,
            Timestamp: new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero),
            Quality: OpcQuality.Good,
            Value: OpcVariant.FromDouble(3.14));

        await Verifier.Verify(WriteHex(state));
    }

    [Test]
    public async Task OpcItemState_with_VT_EMPTY_value_round_trips()
    {
        var state = new OpcItemState(
            ClientHandle: 7,
            Timestamp: DateTimeOffset.UnixEpoch,
            Quality: OpcQuality.Bad,
            Value: OpcVariant.Empty);

        await Verifier.Verify(WriteHex(state, capacity: 64));
    }

    [Test]
    public async Task OpcItemState_with_string_value_round_trips()
    {
        var state = new OpcItemState(
            ClientHandle: 42,
            Timestamp: new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Quality: OpcQuality.Uncertain,
            Value: OpcVariant.FromString("hello world"));

        await Verifier.Verify(WriteHex(state, capacity: 128));
    }

    private static string WriteHex(OpcItemState state, int capacity = 256)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        NdrOpcItemStateCodec.Write(ref writer, state);
        return Convert.ToHexString(buffer.AsSpan(0, writer.Position));
    }
}
