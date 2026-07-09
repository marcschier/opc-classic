// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants and captured call values.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class OpcDaServerDispatcherTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task DispatchGetStatus_calls_server_and_returns_status()
    {
        var server = new StubDaServer { Status = BuildStatus() };
        var dispatcher = new OpcDaServerDispatcher(server);

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCServer.InterfaceId,
            IOPCServer.Opnums.GetStatusAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        OpcServerStatus status = ReadStatus(result.ResponsePayload);
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.GetStatusCallCount).IsEqualTo(1);
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Da);
        await Assert.That(status.VendorInfo).IsEqualTo(ReadVendorInfo());
        await Assert.That(status.GroupCount).IsEqualTo(7);
    }

    [Test]
    public async Task DispatchGetStatus_HRESULT_propagates_through_NdrCallResult()
    {
        int expectedHresult = ReadFailHresult();
        var server = new StubDaServer
        {
            GetStatusException = new OpcException(new OpcResultId(expectedHresult, "E_FAIL")),
        };
        var dispatcher = new OpcDaServerDispatcher(server);

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCServer.InterfaceId,
            IOPCServer.Opnums.GetStatusAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(expectedHresult);
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
    }

    [Test]
    public async Task DispatchRemoveGroup_decodes_params_correctly()
    {
        var server = new StubDaServer();
        var dispatcher = new OpcDaServerDispatcher(server);
        byte[] request = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteInt32(1234);
            writer.WriteInt32(-1);
        });

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCServer.InterfaceId,
            IOPCServer.Opnums.RemoveGroupAsync,
            request,
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.RemoveGroupCallCount).IsEqualTo(1);
        await Assert.That(server.LastRemovedGroupHandle).IsEqualTo(1234);
        await Assert.That(server.LastRemoveGroupForce).IsTrue();
    }

    [Test]
    public async Task DispatchUnknownOpnum_returns_E_NOTIMPL()
    {
        var dispatcher = new OpcDaServerDispatcher(new StubDaServer());

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCServer.InterfaceId,
            ReadUnknownOpnum(),
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
    }

    [Test]
    public async Task DispatchUnknownInterface_returns_E_NOTIMPL()
    {
        var dispatcher = new OpcDaServerDispatcher(new StubDaServer());

        NdrCallResult result = await dispatcher.DispatchAsync(
            ReadUnknownInterfaceId(),
            IOPCServer.Opnums.GetStatusAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
    }

    [Test]
    public async Task DispatchGetErrorString_decodes_params_and_encodes_response()
    {
        var server = new StubDaServer { ErrorString = ReadErrorString() };
        var dispatcher = new OpcDaServerDispatcher(server);
        byte[] request = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteInt32(ReadFailHresult());
            writer.WriteInt32(1033);
        });

        NdrCallResult result = await dispatcher.DispatchAsync(
            IOPCServer.InterfaceId,
            IOPCServer.Opnums.GetErrorStringAsync,
            request,
            CancellationToken.None);

        string? decoded = ReadString(result.ResponsePayload);
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.LastErrorCode).IsEqualTo(ReadFailHresult());
        await Assert.That(server.LastLocaleId).IsEqualTo(1033);
        await Assert.That(decoded).IsEqualTo(ReadErrorString());
    }

    private static byte[] WritePayload(NdrWriteAction write, int capacity = 512)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static OpcServerStatus ReadStatus(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        // Server emits OPCSERVERSTATUS as a unique pointer (referent + struct) per
        // IDL [out] T**; skip the referent before invoking the struct codec.
        _ = reader.ReadUInt32();
        return NdrOpcServerStatusCodec.Read(ref reader);
    }

    private static string? ReadString(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return reader.ReadUnicodeStringPtr();
    }

    private static OpcServerStatus BuildStatus() => new()
    {
        Spec = OpcStatusSpec.Da,
        StartTime = DateTimeOffset.UnixEpoch,
        CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(1),
        LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(2),
        State = OpcServerState.Running,
        ServerVersion = new Version(1, 2, 3),
        GroupCount = 7,
        BandWidth = 99,
        VendorInfo = ReadVendorInfo(),
    };

    private static Guid ReadUnknownInterfaceId() => Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static int ReadUnknownOpnum() => 999;
    private static int ReadFailHresult() => unchecked((int)0x80004005u);
    private static string ReadVendorInfo() => "DA Dispatcher Test Server";
    private static string ReadErrorString() => "The operation failed.";

    private sealed class StubDaServer : IOpcDaServer
    {
        public OpcServerStatus Status { get; init; } = BuildStatus();
        public OpcException? GetStatusException { get; init; }
        public string ErrorString { get; init; } = string.Empty;
        public int GetStatusCallCount { get; private set; }
        public int RemoveGroupCallCount { get; private set; }
        public int LastRemovedGroupHandle { get; private set; }
        public bool LastRemoveGroupForce { get; private set; }
        public int LastErrorCode { get; private set; }
        public int LastLocaleId { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            GetStatusCallCount++;
            if (GetStatusException is not null)
            {
                throw GetStatusException;
            }

            return Task.FromResult(Status);
        }

        public Task<int> AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task RemoveGroupAsync(
            int serverGroupHandle,
            bool force,
            CancellationToken cancellationToken = default)
        {
            RemoveGroupCallCount++;
            LastRemovedGroupHandle = serverGroupHandle;
            LastRemoveGroupForce = force;
            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(
            int errorCode,
            int localeId,
            CancellationToken cancellationToken = default)
        {
            LastErrorCode = errorCode;
            LastLocaleId = localeId;
            return Task.FromResult(ErrorString);
        }
    }
}
