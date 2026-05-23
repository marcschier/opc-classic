//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using Opc.Classic;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests;

public sealed class NdrOpcEventServerStatusCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static OpcServerStatus ReadOne(byte[] bytes)
    {
        var reader = new NdrReader(bytes);
        return NdrOpcEventServerStatusCodec.Read(ref reader);
    }

    private static OpcServerStatus BuildSample(
        OpcServerState state = OpcServerState.Running,
        string vendorInfo = "Acme AE Server") => new()
    {
        Spec = OpcStatusSpec.Ae,
        StartTime = new DateTimeOffset(2026, 5, 22, 0, 0, 0, TimeSpan.Zero),
        CurrentTime = new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero),
        LastUpdateTime = new DateTimeOffset(2026, 5, 22, 10, 29, 50, TimeSpan.Zero),
        State = state,
        ServerVersion = new Version(1, 10, 42),
        VendorInfo = vendorInfo,
        GroupCount = 17,
        BandWidth = 4500,
    };

    [Test]
    public async Task RoundTrip_TypicalRunningServer()
    {
        var input = BuildSample();
        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcEventServerStatusCodec.Write(ref writer, input), capacity: 512);
        var back = ReadOne(bytes);

        await Assert.That(back.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(back.StartTime.UtcDateTime).IsEqualTo(input.StartTime.UtcDateTime);
        await Assert.That(back.CurrentTime.UtcDateTime).IsEqualTo(input.CurrentTime.UtcDateTime);
        await Assert.That(back.LastUpdateTime.UtcDateTime).IsEqualTo(input.LastUpdateTime.UtcDateTime);
        await Assert.That(back.ServerVersion.Major).IsEqualTo(1);
        await Assert.That(back.ServerVersion.Minor).IsEqualTo(10);
        await Assert.That(back.ServerVersion.Build).IsEqualTo(42);
        await Assert.That(back.VendorInfo).IsEqualTo("Acme AE Server");
        await Assert.That(back.GroupCount).IsEqualTo(0);
        await Assert.That(back.BandWidth).IsEqualTo(0u);
    }

    [Test]
    [Arguments(OpcServerState.Running, 1)]
    [Arguments(OpcServerState.Failed, 2)]
    [Arguments(OpcServerState.NoConfig, 3)]
    [Arguments(OpcServerState.Suspended, 4)]
    [Arguments(OpcServerState.Test, 5)]
    [Arguments(OpcServerState.CommFault, 6)]
    public async Task RoundTrip_AllEventServerStates_UsesAeWireValues(OpcServerState state, int expectedWireValue)
    {
        var input = BuildSample(state: state);
        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcEventServerStatusCodec.Write(ref writer, input), capacity: 512);
        uint wireValue = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24, 4));
        var back = ReadOne(bytes);

        await Assert.That(wireValue).IsEqualTo((uint)expectedWireValue);
        await Assert.That(back.State).IsEqualTo(state);
    }

    [Test]
    public async Task RoundTrip_UnicodeVendorInfo()
    {
        var input = BuildSample(vendorInfo: "Müller AE 株式会社");
        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcEventServerStatusCodec.Write(ref writer, input), capacity: 512);
        var back = ReadOne(bytes);

        await Assert.That(back.VendorInfo).IsEqualTo("Müller AE 株式会社");
    }

    [Test]
    public async Task DecodedSpec_IsAe()
    {
        var input = BuildSample();
        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcEventServerStatusCodec.Write(ref writer, input), capacity: 512);
        var back = ReadOne(bytes);

        await Assert.That(back.Spec).IsEqualTo(OpcStatusSpec.Ae);
    }

    [Test]
    public async Task WireShape_ExcludesDaGroupCountAndBandwidth()
    {
        var input = BuildSample();
        var bytes = WriteOne((ref NdrWriter writer) => NdrOpcEventServerStatusCodec.Write(ref writer, input), capacity: 512);
        ushort major = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(28, 2));
        ushort minor = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(30, 2));
        ushort build = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(32, 2));
        ushort reserved = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(34, 2));

        await Assert.That(major).IsEqualTo((ushort)1);
        await Assert.That(minor).IsEqualTo((ushort)10);
        await Assert.That(build).IsEqualTo((ushort)42);
        await Assert.That(reserved).IsEqualTo((ushort)0);
    }
}
