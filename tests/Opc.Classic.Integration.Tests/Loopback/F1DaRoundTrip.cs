//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F1DaRoundTrip
{
    [Test]
    public async Task GetStatus_round_trips_through_InMemoryCallChannel()
    {
        var expected = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = new DateTimeOffset(2026, 5, 22, 8, 0, 0, TimeSpan.Zero),
            CurrentTime = new DateTimeOffset(2026, 5, 22, 8, 1, 0, TimeSpan.Zero),
            LastUpdateTime = new DateTimeOffset(2026, 5, 22, 8, 0, 45, TimeSpan.Zero),
            State = OpcServerState.Running,
            ServerVersion = new Version(2, 5, 1),
            VendorInfo = "Phase 13 managed loopback",
            GroupCount = 3,
            BandWidth = 4_500u,
        };
        var response = EncodeStatus(expected);
        var observedInterfaceId = Guid.Empty;
        var observedOpnum = -1;
        var observedPayloadLength = -1;

        var channel = new InMemoryCallChannel((interfaceId, opnum, payload, _) =>
        {
            observedInterfaceId = interfaceId;
            observedOpnum = opnum;
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, response));
        });
        var proxy = new IOPCServerClientProxy(channel);

        var actual = await proxy.GetStatusAsync(CancellationToken.None);

        await Assert.That(actual.Spec).IsEqualTo(expected.Spec);
        await Assert.That(actual.State).IsEqualTo(expected.State);
        await Assert.That(actual.ServerVersion).IsEqualTo(expected.ServerVersion);
        await Assert.That(actual.VendorInfo).IsEqualTo(expected.VendorInfo);
        await Assert.That(actual.GroupCount).IsEqualTo(expected.GroupCount);
        await Assert.That(actual.BandWidth).IsEqualTo(expected.BandWidth);
        await Assert.That(actual.StartTime.UtcDateTime).IsEqualTo(expected.StartTime.UtcDateTime);
        await Assert.That(actual.CurrentTime.UtcDateTime).IsEqualTo(expected.CurrentTime.UtcDateTime);
        await Assert.That(actual.LastUpdateTime.UtcDateTime).IsEqualTo(expected.LastUpdateTime.UtcDateTime);
        await Assert.That(observedInterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
        var expectedPayloadLength = 0;
        await Assert.That(observedPayloadLength).IsEqualTo(expectedPayloadLength);
    }

    [Test]
    public async Task RemoveGroup_round_trips_with_success_status()
    {
        var expectedServerGroupHandle = 42;
        var expectedForce = true;
        var observedInterfaceId = Guid.Empty;
        var observedOpnum = -1;
        ReadOnlyMemory<byte> capturedPayload = ReadOnlyMemory<byte>.Empty;

        var channel = new InMemoryCallChannel((interfaceId, opnum, payload, _) =>
        {
            observedInterfaceId = interfaceId;
            observedOpnum = opnum;
            capturedPayload = payload.ToArray();
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new IOPCServerClientProxy(channel);

        await proxy.RemoveGroupAsync(expectedServerGroupHandle, expectedForce, CancellationToken.None);

        var (actualServerGroupHandle, actualForce) = DecodeRemoveGroupRequest(capturedPayload.Span);
        await Assert.That(observedInterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCServer.Opnums.RemoveGroupAsync);
        await Assert.That(actualServerGroupHandle).IsEqualTo(expectedServerGroupHandle);
        await Assert.That(actualForce).IsEqualTo(expectedForce);
        var expectedCallCount = 1;
        await Assert.That(channel.CallLog.Count).IsEqualTo(expectedCallCount);
    }

    private static ReadOnlyMemory<byte> EncodeStatus(OpcServerStatus status)
    {
        var buffer = new byte[512];
        var writer = new NdrWriter(buffer);
        // IOPCServer::GetStatus declares [out] OPCSERVERSTATUS **ppServerStatus,
        // a NDR unique pointer (MS-RPCE §14.3.10). The wire layout is:
        //   [4-byte referent ID][OPCSERVERSTATUS struct]
        // Use a non-zero referent so the proxy decoder treats the pointer as
        // non-null and reads the struct that follows.
        writer.WriteUInt32(0x00020000u);
        NdrOpcServerStatusCodec.Write(ref writer, status);
        return buffer.AsMemory(0, writer.Position).ToArray();
    }

    private static (int ServerGroupHandle, bool Force) DecodeRemoveGroupRequest(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        var serverGroupHandle = reader.ReadInt32();
        var force = reader.ReadInt32() != 0;
        return (serverGroupHandle, force);
    }
}
