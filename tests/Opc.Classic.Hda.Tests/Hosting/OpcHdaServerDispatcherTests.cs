// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants and captured call values.

using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;

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
        // Per [OpcEmitArrayCount, OpcDeferredElements] on IOPCHDA_Server.ValidateItemIDsAsync:
        // sibling DWORD count, then conformant array of LPWSTR with per-element
        // referent IDs followed by per-element string bodies (DCE 1.1 §14.3.12.3).
        // This matches the wire shape opchda_ps.dll (MS-DCOM proxy/stub) emits.
        byte[] request = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(2);             // sibling count (from [OpcEmitArrayCount])
            writer.WriteUInt32(2);             // array conformance (from [OpcDeferredElements])
            _ = writer.WriteReferentId();      // per-element referent for element 0
            _ = writer.WriteReferentId();      // per-element referent for element 1
            writer.WriteUnicodeString("Random.Real8");
            writer.WriteUnicodeString("Missing.Tag");
        });

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCHDA_Server.InterfaceId,
            IOPCHDA_Server.Opnums.ValidateItemIDsAsync,
            request,
            CancellationToken.None);

        var reader = new NdrReader(result.ResponsePayload.Span);
        // [return: OpcUniquePointer] on ValidateItemIDsAsync wraps the array
        // in a 4-byte unique-pointer referent before the conformance count.
        _ = reader.TryReadReferentId(out _);
        uint count = reader.ReadUInt32();
        int first = reader.ReadInt32();
        int second = reader.ReadInt32();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.LastItemIds).IsEquivalentTo(["Random.Real8", "Missing.Tag"]);
        await Assert.That(count).IsEqualTo(2u);
        await Assert.That(first).IsEqualTo(0);
        await Assert.That(second).IsEqualTo(OpcResultId.UnknownItemId.Code);
    }

    [Test]
    public async Task DispatchCommon_round_trips_locale_error_text_and_client_name()
    {
        var server = new StubHdaServer { SupportedLocales = [0x0409, 0x0411] };
        var dispatcher = new OpcHdaServerDispatcher(server);

        NdrCallResult setLocale = await dispatcher.DispatchAsync(
            OpcCommonClientProxy.InterfaceId,
            OpcCommonClientProxy.Opnums.SetLocaleId,
            WritePayload((ref NdrWriter writer) => writer.WriteInt32(0x0411)),
            CancellationToken.None);
        NdrCallResult getLocale = await dispatcher.DispatchAsync(
            OpcCommonClientProxy.InterfaceId,
            OpcCommonClientProxy.Opnums.GetLocaleId,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);
        NdrCallResult locales = await dispatcher.DispatchAsync(
            OpcCommonClientProxy.InterfaceId,
            OpcCommonClientProxy.Opnums.QueryAvailableLocaleIds,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);
        NdrCallResult errorText = await dispatcher.DispatchAsync(
            OpcCommonClientProxy.InterfaceId,
            OpcCommonClientProxy.Opnums.GetErrorString,
            WritePayload((ref NdrWriter writer) => writer.WriteInt32(OpcResultId.Fail.Code)),
            CancellationToken.None);
        NdrCallResult clientName = await dispatcher.DispatchAsync(
            OpcCommonClientProxy.InterfaceId,
            OpcCommonClientProxy.Opnums.SetClientName,
            WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("hda-client")),
            CancellationToken.None);

        await Assert.That(setLocale.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(ReadInt32(getLocale.ResponsePayload)).IsEqualTo(0x0411);
        await Assert.That(ReadInt32Array(locales.ResponsePayload)).IsEquivalentTo([0x0409, 0x0411]);
        await Assert.That(ReadString(errorText.ResponsePayload)).IsEqualTo("HDA text 0x80004005");
        await Assert.That(clientName.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.LocaleId).IsEqualTo(0x0411);
        await Assert.That(server.ClientName).IsEqualTo("hda-client");
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

    private static int ReadInt32(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return reader.ReadInt32();
    }

    private static int[] ReadInt32Array(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return reader.ReadConformantInt32Array();
    }

    private static string ReadString(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return reader.ReadUnicodeStringPtr() ?? string.Empty;
    }

    private sealed class StubHdaServer : IOpcHdaServer
    {
        public int[] ValidateResults { get; init; } = [];
        public IReadOnlyList<int> SupportedLocales { get; init; } = [0];
        public int LocaleId { get; private set; }
        public string ClientName { get; private set; } = string.Empty;
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

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default)
        {
            LocaleId = localeId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(SupportedLocales);

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult($"HDA text 0x{resultId.Code:X8}");

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            ClientName = clientName;
            return Task.CompletedTask;
        }
    }
}
