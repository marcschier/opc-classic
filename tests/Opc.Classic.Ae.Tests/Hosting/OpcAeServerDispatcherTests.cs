//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants and captured call values.

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests.Hosting;

public sealed class OpcAeServerDispatcherTests
{
    [Test]
    public async Task DispatchGetStatus_calls_server_and_returns_status()
    {
        var server = new StubAeServer();
        var dispatcher = new OpcAeServerDispatcher(server);

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCEventServer.InterfaceId,
            IOPCEventServer.Opnums.GetStatusAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        var reader = new NdrReader(result.ResponsePayload.Span);
        // Server emits OPCEVENTSERVERSTATUS as a unique pointer (referent + struct).
        _ = reader.ReadUInt32();
        OpcServerStatus status = NdrOpcEventServerStatusCodec.Read(ref reader);
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.GetStatusCallCount).IsEqualTo(1);
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Ae);
        await Assert.That(status.VendorInfo).IsEqualTo(ReadVendorInfo());
    }

    [Test]
    public async Task DispatchQueryAvailableFilters_returns_filter_mask()
    {
        var server = new StubAeServer { FilterMask = 7 };
        var dispatcher = new OpcAeServerDispatcher(server);

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCEventServer.InterfaceId,
            IOPCEventServer.Opnums.QueryAvailableFiltersAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        var reader = new NdrReader(result.ResponsePayload.Span);
        int filters = reader.ReadInt32();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.QueryAvailableFiltersCallCount).IsEqualTo(1);
        await Assert.That(filters).IsEqualTo(7);
    }

    private static string ReadVendorInfo() => "AE Dispatcher Test Server";

    private static OpcServerStatus BuildStatus() => new()
    {
        Spec = OpcStatusSpec.Ae,
        StartTime = DateTimeOffset.UnixEpoch,
        CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(1),
        LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(2),
        State = OpcServerState.Running,
        ServerVersion = new Version(1, 10, 1),
        VendorInfo = ReadVendorInfo(),
    };

    private sealed class StubAeServer : IOpcAeServer
    {
        public int FilterMask { get; init; }

        public int GetStatusCallCount { get; private set; }

        public int QueryAvailableFiltersCallCount { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            GetStatusCallCount++;
            return Task.FromResult(BuildStatus());
        }

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
        {
            QueryAvailableFiltersCallCount++;
            return Task.FromResult(FilterMask);
        }
    }
}
