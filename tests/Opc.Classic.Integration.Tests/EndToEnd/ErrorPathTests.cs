// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // End-to-end tests assert captured pipeline state.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Integration.Tests.EndToEnd;

public sealed class ErrorPathTests
{
    [Test, Category("EndToEnd")]
    public async Task ServerReturnsENotImpl_Then_ClientSurfacesOpcException()
    {
        InMemoryCallChannel channel = new InMemoryCallChannelBuilder().Build();
        var proxy = new IOPCGroupStateMgtClientProxy(channel);

        OpcException exception = await CaptureAsync<OpcException>(() => proxy.GetStateAsync(CancellationToken.None));

        await Assert.That(exception.ResultId.Code).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(exception.ResultId.IsFailure).IsTrue();
        await Assert.That(channel.CallLog.Count).IsEqualTo(1);
        await Assert.That(channel.CallLog[0].InterfaceId).IsEqualTo(IOPCGroupStateMgt.InterfaceId);
        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCGroupStateMgt.Opnums.GetStateAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task NdrDecodeFailure_Then_InvalidOperationExceptionNamesEndOfBuffer()
    {
        var channel = new InMemoryCallChannel((_, _, _, _) =>
            Task.FromResult(new NdrCallResult(OpcResultId.Ok.Code, new byte[] { 0x01, 0x02, 0x03 })));
        var proxy = new IOPCServerClientProxy(channel);

        InvalidOperationException exception = await CaptureAsync<InvalidOperationException>(() => proxy.GetStatusAsync(CancellationToken.None));

        await Assert.That(exception.Message).Contains("NdrReader past end-of-buffer");
        await Assert.That(exception.Message).Contains("need");
        await Assert.That(exception.Message).Contains("remain");
        await Assert.That(channel.CallLog.Count).IsEqualTo(1);
        await Assert.That(channel.CallLog[0].InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task Cancellation_Then_InFlightCallIsCanceledAndLoggedOnce()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var channel = new InMemoryCallChannel(async (_, _, _, cancellationToken) =>
        {
            entered.TrySetResult();
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);
            return new NdrCallResult(OpcResultId.Ok.Code, EncodeStatus());
        });
        var proxy = new IOPCServerClientProxy(channel);
        using var cts = new CancellationTokenSource();

        Task<OpcServerStatus> call = proxy.GetStatusAsync(cts.Token);
        await entered.Task.ConfigureAwait(false);
        await cts.CancelAsync().ConfigureAwait(false);
        OperationCanceledException exception = await CaptureAsync<OperationCanceledException>(async () =>
            _ = await call.ConfigureAwait(false));

        await Assert.That(exception.CancellationToken).IsEqualTo(cts.Token);
        await Assert.That(channel.CallLog.Count).IsEqualTo(1);
        await Assert.That(channel.CallLog[0].InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task ChannelDisposalMidCall_Then_ObjectDisposedExceptionIsGraceful()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var channel = new DisposableCallChannel(async (_, _, _, cancellationToken) =>
        {
            entered.TrySetResult();
            await release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new NdrCallResult(OpcResultId.Ok.Code, EncodeStatus());
        });
        var proxy = new IOPCServerClientProxy(channel);

        Task<OpcServerStatus> call = proxy.GetStatusAsync(CancellationToken.None);
        await entered.Task.ConfigureAwait(false);
        channel.Dispose();
        release.TrySetResult();
        ObjectDisposedException exception = await CaptureAsync<ObjectDisposedException>(async () =>
            _ = await call.ConfigureAwait(false));

        await Assert.That(exception.ObjectName).IsEqualTo(nameof(DisposableCallChannel));
        await Assert.That(exception.Message).Contains("in-flight");
    }

    private static ReadOnlyMemory<byte> EncodeStatus()
    {
        var status = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentTime = new DateTimeOffset(2026, 1, 1, 0, 0, 1, TimeSpan.Zero),
            LastUpdateTime = new DateTimeOffset(2026, 1, 1, 0, 0, 1, TimeSpan.Zero),
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 2, 3),
            VendorInfo = "Error path status",
            GroupCount = 0,
            BandWidth = 0,
        };
        return EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            // [out] OPCSERVERSTATUS **ppServerStatus is wire-encoded as a NDR unique
            // pointer (MS-RPCE §14.3.10): 4-byte referent ID + struct.
            writer.WriteUInt32(0x00020000u);
            NdrOpcServerStatusCodec.Write(ref writer, status);
        });
    }

    private static async Task<TException> CaptureAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Expected {typeof(TException).Name}, but caught {exception.GetType().Name}.",
                exception);
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}, but no exception was thrown.");
    }
}
