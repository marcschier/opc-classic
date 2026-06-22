// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.IO.Pipelines;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Tests.Transport;

[SupportedOSPlatform("windows")]
public sealed class LocalNamedPipeTransportTests
{
    [Test]
    public async Task ConnectAsync_round_trips_payload_through_local_pipe()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        string pipeName = $"opcclassic-test-{Guid.NewGuid():N}";
        byte[] clientRequest = Encoding.UTF8.GetBytes("ping from client");
        byte[] serverResponse = Encoding.UTF8.GetBytes("pong from server");

        await using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            System.IO.Pipes.PipeOptions.Asynchronous);

        using var serverDone = new SemaphoreSlim(0, 1);
        Task serverTask = Task.Run(async () =>
        {
            try
            {
                await server.WaitForConnectionAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
                byte[] buffer = new byte[clientRequest.Length];
                int read = 0;
                while (read < buffer.Length)
                {
                    int n = await server.ReadAsync(buffer.AsMemory(read), TestContext.Current!.CancellationToken).ConfigureAwait(false);
                    if (n == 0)
                    {
                        break;
                    }
                    read += n;
                }
                await server.WriteAsync(serverResponse, TestContext.Current!.CancellationToken).ConfigureAwait(false);
                await server.FlushAsync(TestContext.Current!.CancellationToken).ConfigureAwait(false);
            }
            finally
            {
                serverDone.Release();
            }
        }, TestContext.Current!.CancellationToken);

        await using LocalNamedPipeTransport transport = await LocalNamedPipeTransport.ConnectAsync(
            pipeName,
            cancellationToken: TestContext.Current!.CancellationToken);

        Memory<byte> outBuffer = transport.Output.GetMemory(clientRequest.Length);
        clientRequest.AsSpan().CopyTo(outBuffer.Span);
        transport.Output.Advance(clientRequest.Length);
        await transport.FlushAsync(TestContext.Current!.CancellationToken);

        ReadResult read1 = await transport.Input.ReadAsync(TestContext.Current!.CancellationToken);
        byte[] readBytes = System.Buffers.BuffersExtensions.ToArray(read1.Buffer);
        transport.Input.AdvanceTo(read1.Buffer.End);

        await Assert.That(readBytes).IsEquivalentTo(serverResponse);
        await serverDone.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);
        await serverTask;
    }

    [Test]
    public async Task ConnectAsync_throws_when_pipe_does_not_exist()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        string pipeName = $"opcclassic-missing-{Guid.NewGuid():N}";
        using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        connectCts.CancelAfter(TimeSpan.FromSeconds(2));

        bool threw = false;
        try
        {
            await using LocalNamedPipeTransport t = await LocalNamedPipeTransport.ConnectAsync(pipeName, cancellationToken: connectCts.Token);
        }
        catch (Exception)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task NormalizePipeName_strips_unc_prefix_and_pipe_token()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(LocalNamedPipeTransport.NormalizePipeName("\\\\.\\pipe\\foo")).IsEqualTo("foo");
        await Assert.That(LocalNamedPipeTransport.NormalizePipeName("\\\\?\\pipe\\foo")).IsEqualTo("foo");
        await Assert.That(LocalNamedPipeTransport.NormalizePipeName("PIPE\\foo")).IsEqualTo("foo");
        await Assert.That(LocalNamedPipeTransport.NormalizePipeName("foo")).IsEqualTo("foo");
        await Assert.That(LocalNamedPipeTransport.NormalizePipeName("[foo]")).IsEqualTo("foo");
        await Assert.That(LocalNamedPipeTransport.NormalizePipeName("\\PIPE\\OPCxxx")).IsEqualTo("OPCxxx");
    }

    [Test]
    public async Task DisposeAsync_closes_underlying_stream_idempotently()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        string pipeName = $"opcclassic-dispose-{Guid.NewGuid():N}";
        await using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            System.IO.Pipes.PipeOptions.Asynchronous);
        Task acceptTask = server.WaitForConnectionAsync(TestContext.Current!.CancellationToken);

        LocalNamedPipeTransport transport = await LocalNamedPipeTransport.ConnectAsync(
            pipeName,
            cancellationToken: TestContext.Current!.CancellationToken);
        await acceptTask;

        await transport.DisposeAsync();
        // Second dispose is a no-op (must not throw).
        await transport.DisposeAsync();
    }
}
