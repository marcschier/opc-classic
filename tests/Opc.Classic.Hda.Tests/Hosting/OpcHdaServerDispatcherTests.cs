//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants and captured call values.

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Hda.Tests.Hosting;

public sealed class OpcHdaServerDispatcherTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task DispatchGetHistorianStatus_calls_server_and_returns_status_payload()
    {
        var server = new StubHdaServer();
        var dispatcher = new OpcHdaServerDispatcher(server);

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCHDA_Server.InterfaceId,
            5,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        var reader = new NdrReader(result.ResponsePayload.Span);
        uint status = reader.ReadUInt32();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.GetStatusCallCount).IsEqualTo(1);
        await Assert.That(status).IsEqualTo(1u);
    }

    [Test]
    public async Task DispatchValidateItemIds_decodes_items_and_encodes_results()
    {
        var server = new StubHdaServer { ValidateResults = [0, OpcResultId.UnknownItemId.Code] };
        var dispatcher = new OpcHdaServerDispatcher(server);
        byte[] request = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(2);
            writer.WriteUnicodeStringPtr("Random.Real8");
            writer.WriteUnicodeStringPtr("Missing.Tag");
        });

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCHDA_Server.InterfaceId,
            IOPCHDA_Server.Opnums.ValidateItemIDsAsync,
            request,
            CancellationToken.None);

        var reader = new NdrReader(result.ResponsePayload.Span);
        uint count = reader.ReadUInt32();
        int first = reader.ReadInt32();
        int second = reader.ReadInt32();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.LastItemIds).IsEquivalentTo(["Random.Real8", "Missing.Tag"]);
        await Assert.That(count).IsEqualTo(2u);
        await Assert.That(first).IsEqualTo(0);
        await Assert.That(second).IsEqualTo(OpcResultId.UnknownItemId.Code);
    }

    private static byte[] WritePayload(NdrWriteAction write, int capacity = 512)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static OpcServerStatus BuildStatus() => new()
    {
        Spec = OpcStatusSpec.Hda,
        StartTime = DateTimeOffset.UnixEpoch,
        CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(1),
        LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(2),
        State = OpcServerState.Running,
        ServerVersion = new Version(1, 20, 1),
        MaxReturnValues = 1000,
        VendorInfo = "HDA Dispatcher Test Server",
    };

    private sealed class StubHdaServer : IOpcHdaServer
    {
        public int[] ValidateResults { get; init; } = [];

        public string[] LastItemIds { get; private set; } = [];

        public int GetStatusCallCount { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            GetStatusCallCount++;
            return Task.FromResult(BuildStatus());
        }

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
        {
            LastItemIds = itemIds;
            return Task.FromResult(ValidateResults);
        }
    }
}
