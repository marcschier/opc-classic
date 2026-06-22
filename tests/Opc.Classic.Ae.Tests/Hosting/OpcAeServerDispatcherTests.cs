// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants and captured call values.

using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;

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

    [Test]
    public async Task DispatchCommon_round_trips_locale_error_text_and_client_name()
    {
        var server = new StubAeServer { SupportedLocales = [0x0409, 0x0407] };
        var dispatcher = new OpcAeServerDispatcher(server);

        NdrCallResult setLocale = await dispatcher.DispatchAsync(
            OpcCommonClientProxy.InterfaceId,
            OpcCommonClientProxy.Opnums.SetLocaleId,
            WritePayload((ref NdrWriter writer) => writer.WriteInt32(0x0407)),
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
            WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("ae-client")),
            CancellationToken.None);

        await Assert.That(setLocale.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(ReadInt32(getLocale.ResponsePayload)).IsEqualTo(0x0407);
        await Assert.That(ReadInt32Array(locales.ResponsePayload)).IsEquivalentTo([0x0409, 0x0407]);
        await Assert.That(ReadString(errorText.ResponsePayload)).IsEqualTo("AE text 0x80004005");
        await Assert.That(clientName.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.LocaleId).IsEqualTo(0x0407);
        await Assert.That(server.ClientName).IsEqualTo("ae-client");
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

    private static byte[] WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[512];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

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

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed class StubAeServer : IOpcAeServer
    {
        public int FilterMask { get; init; }
        public IReadOnlyList<int> SupportedLocales { get; init; } = [0];
        public int LocaleId { get; private set; }
        public string ClientName { get; private set; } = string.Empty;
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

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default)
        {
            LocaleId = localeId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(SupportedLocales);

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult($"AE text 0x{resultId.Code:X8}");

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            ClientName = clientName;
            return Task.CompletedTask;
        }
    }
}
