//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Text;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Smb;
using Opc.Classic.Dcom.Smb.Rpc;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class NcacnNpTransportTests
{
    [Test]
    public async Task ConnectAsync_opens_pipe_and_round_trips_payload()
    {
        var mock = CreateOpenPipeServer();
        byte[] request = CreateRpcPdu(0x31, 0x32, 0x33);
        byte[] response = CreateRpcPdu(0x41, 0x42, 0x43, 0x44);
        mock.OnIoctlTransceive(response);
        mock.OnStatus(Smb2Command.Close);
        mock.OnStatus(Smb2Command.TreeDisconnect);
        mock.OnStatus(Smb2Command.Logoff);

        await using IAsyncTransport transport = await ConnectAsync(mock, "\\PIPE\\winreg");
        Memory<byte> output = transport.Output.GetMemory(request.Length);
        request.AsSpan().CopyTo(output.Span);
        transport.Output.Advance(request.Length);

        await transport.FlushAsync(TestContext.Current!.CancellationToken);
        ReadResult read = await transport.Input.ReadAsync(TestContext.Current.CancellationToken);
        byte[] actual = read.Buffer.ToArray();
        transport.Input.AdvanceTo(read.Buffer.End);

        await Assert.That(mock.OpenedPipeName).IsEqualTo("winreg");
        await Assert.That(mock.LastTransceivePayload).IsEquivalentTo(request);
        await Assert.That(actual).IsEquivalentTo(response);
    }

    [Test]
    public async Task LegacyRpcTransport_uses_smb2_adapter()
    {
        var mock = CreateOpenPipeServer();
        byte[] request = CreateRpcPdu(0x55, 0x56);
        byte[] response = CreateRpcPdu(0x65, 0x66, 0x67);
        mock.OnIoctlTransceive(response);
        mock.OnStatus(Smb2Command.Close);
        mock.OnStatus(Smb2Command.TreeDisconnect);
        mock.OnStatus(Smb2Command.Logoff);
        Smb2TransportConnector connector = (_, _, _, _) => Task.FromResult<ISmb2Transport>(mock);
        using var transport = new Opc.Classic.Dcom.Rpc.Ncacn_Np.RpcTransport(
            "ncacn_np:server[\\PIPE\\winreg]",
            new PropertyBag(),
            connector);
        _ = transport.Attach(new PresentationSyntax("338cd001-2244-31f1-aaaa-900038001003:1.0"));
        var send = new NdrBuffer(new byte[64], 0);
        send.WriteOctetArray(request, 0, request.Length);
        var receive = new NdrBuffer(new byte[64], 0);

        transport.Send(send);
        transport.Receive(receive);

        await Assert.That(mock.OpenedPipeName).IsEqualTo("winreg");
        await Assert.That(mock.LastTransceivePayload).IsEquivalentTo(request);
        await Assert.That(receive.Buf.AsSpan(0, receive.Length).ToArray()).IsEquivalentTo(response);
    }

    [Test]
    public async Task ConnectAsync_propagates_smb2_message_quota()
    {
        var mock = CreateOpenPipeServer();
        mock.OnStatus(Smb2Command.Close);
        mock.OnStatus(Smb2Command.TreeDisconnect);
        mock.OnStatus(Smb2Command.Logoff);
        var properties = new PropertyBag();
        properties.SetProperty("rpc.maxSmb2MessageSize", 4096);
        int capturedQuota = 0;
        Smb2TransportConnector connector = (host, port, maxSmb2MessageSize, cancellationToken) =>
        {
            _ = host;
            _ = port;
            _ = cancellationToken;
            capturedQuota = maxSmb2MessageSize;
            return Task.FromResult<ISmb2Transport>(mock);
        };
        var factory = new NcacnNpTransportFactory(NoOpAuthContext.Instance, properties, connector);

        await using IAsyncTransport transport = await factory.ConnectAsync(
            new NcacnNpEndPoint("server", "\\PIPE\\winreg"),
            TestContext.Current!.CancellationToken);

        await Assert.That(capturedQuota).IsEqualTo(4096);
    }

    [Test]
    [Arguments("\\PIPE\\winreg")]
    [Arguments("\\pipe\\winreg")]
    [Arguments("\\PIPE\\\\winreg")]
    public async Task NormalizePipeName_accepts_pipe_prefix_variants(string endpoint)
    {
        await Assert.That(NcacnNpEndPoint.NormalizePipeName(endpoint)).IsEqualTo("winreg");
    }

    [Test]
    public async Task DisposeAsync_closes_pipe_tree_session_and_transport()
    {
        var mock = CreateOpenPipeServer();
        mock.OnStatus(Smb2Command.Close);
        mock.OnStatus(Smb2Command.TreeDisconnect);
        mock.OnStatus(Smb2Command.Logoff);

        IAsyncTransport transport = await ConnectAsync(mock, "ncacn_np:[\\pipe\\winreg]");
        await transport.DisposeAsync();

        await Assert.That(mock.IsDisposed).IsTrue();
        await Assert.That(mock.SentCommands.Contains(Smb2Command.Close)).IsTrue();
        await Assert.That(mock.SentCommands.Contains(Smb2Command.TreeDisconnect)).IsTrue();
        await Assert.That(mock.SentCommands.Contains(Smb2Command.Logoff)).IsTrue();
    }

    private static async ValueTask<IAsyncTransport> ConnectAsync(MockSmb2Transport mock, string pipeEndpoint)
    {
        var factory = new NcacnNpTransportFactory(
            NoOpAuthContext.Instance,
            transportConnector: (_, _, _, _) => Task.FromResult<ISmb2Transport>(mock));
        return await factory.ConnectAsync(
            new NcacnNpEndPoint("server", pipeEndpoint),
            TestContext.Current!.CancellationToken);
    }

    private static MockSmb2Transport CreateOpenPipeServer()
    {
        var mock = new MockSmb2Transport();
        mock.OnNegotiate();
        mock.OnSessionSetupSuccess(sessionId: 0x1122334455667788UL);
        mock.OnTreeConnectSuccess(treeId: 0xAABBCCDD);
        mock.OnCreateSuccess(
            fileIdPersistent: 0x0102030405060708UL,
            fileIdVolatile: 0x1112131415161718UL);
        return mock;
    }

    private static byte[] CreateRpcPdu(params byte[] body)
    {
        byte[] pdu = new byte[4 + body.Length];
        pdu[0] = 5;
        pdu[1] = 0;
        pdu[2] = 0;
        pdu[3] = 0x03;
        body.CopyTo(pdu.AsSpan(4));
        return pdu;
    }

    private sealed class MockSmb2Transport : ISmb2Transport
    {
        private const int HeaderSize = 64;
        private const uint FlagsServerToRedir = 0x00000001;

        private readonly Queue<Func<Smb2PacketHeader, byte[], ReadOnlyMemory<byte>>> _responders = new();
        private Smb2PacketHeader _lastRequestHeader;
        private byte[] _lastRequestPacket = [];

        public List<Smb2Command> SentCommands { get; } = [];
        public string? OpenedPipeName { get; private set; }
        public byte[] LastTransceivePayload { get; private set; } = [];
        public bool IsDisposed { get; private set; }

        public void OnNegotiate()
        {
            _responders.Enqueue((header, _) =>
            {
                byte[] response = CreateResponsePacket(header, 72);
                int bodyOffset = HeaderSize;
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 0), 65);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 2), 0);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 4), (ushort)Smb2Dialect.Smb210);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 6), 0);
                Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00").TryWriteBytes(response.AsSpan(bodyOffset + 8, 16));
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 24), 0);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 28), 0x10000);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 32), 0x10000);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 36), 0x10000);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 56), 64 + 64);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 58), 8);
                return response;
            });
        }

        public void OnSessionSetupSuccess(ulong sessionId)
        {
            _responders.Enqueue((header, _) =>
            {
                byte[] response = CreateResponsePacket(header with { SessionId = sessionId }, 16);
                int bodyOffset = HeaderSize;
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 0), 9);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 2), 0);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 4), 64 + 8);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 6), 0);
                return response;
            });
        }

        public void OnTreeConnectSuccess(uint treeId)
        {
            _responders.Enqueue((header, _) =>
            {
                byte[] response = CreateResponsePacket(header with { TreeId = treeId }, 16);
                int bodyOffset = HeaderSize;
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 0), 16);
                response[bodyOffset + 2] = 0x02;
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 4), 0);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 8), 0);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 12), 0x001F01FF);
                return response;
            });
        }

        public void OnCreateSuccess(ulong fileIdPersistent, ulong fileIdVolatile)
        {
            _responders.Enqueue((header, request) =>
            {
                int nameOffset = BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(HeaderSize + 44));
                int nameLength = BinaryPrimitives.ReadUInt16LittleEndian(request.AsSpan(HeaderSize + 46));
                OpenedPipeName = Encoding.Unicode.GetString(request.AsSpan(nameOffset, nameLength));

                byte[] response = CreateResponsePacket(header, 88);
                int bodyOffset = HeaderSize;
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 0), 89);
                BinaryPrimitives.WriteUInt64LittleEndian(response.AsSpan(bodyOffset + 64), fileIdPersistent);
                BinaryPrimitives.WriteUInt64LittleEndian(response.AsSpan(bodyOffset + 72), fileIdVolatile);
                return response;
            });
        }

        public void OnIoctlTransceive(byte[] output)
        {
            _responders.Enqueue((header, request) =>
            {
                int inputOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(request.AsSpan(HeaderSize + 24)));
                int inputCount = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(request.AsSpan(HeaderSize + 28)));
                LastTransceivePayload = request.AsSpan(inputOffset, inputCount).ToArray();

                const int BodyFixedSize = 48;
                byte[] response = CreateResponsePacket(header, BodyFixedSize + output.Length);
                int bodyOffset = HeaderSize;
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 0), 49);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 4), 0x0011C017);
                BinaryPrimitives.WriteUInt64LittleEndian(response.AsSpan(bodyOffset + 8), 0x0102030405060708UL);
                BinaryPrimitives.WriteUInt64LittleEndian(response.AsSpan(bodyOffset + 16), 0x1112131415161718UL);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 32), HeaderSize + BodyFixedSize);
                BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(bodyOffset + 36), (uint)output.Length);
                output.CopyTo(response.AsSpan(HeaderSize + BodyFixedSize));
                return response;
            });
        }

        public void OnStatus(Smb2Command expectedCommand)
        {
            _responders.Enqueue((header, _) =>
            {
                if (header.Command != expectedCommand)
                {
                    throw new InvalidOperationException($"Expected SMB2 {expectedCommand}, got {header.Command}.");
                }

                byte[] response = CreateResponsePacket(header, 4);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(HeaderSize), 4);
                return response;
            });
        }

        public Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            _lastRequestPacket = packet.ToArray();
            _lastRequestHeader = Smb2PacketHeader.Read(packet.Span);
            SentCommands.Add(_lastRequestHeader.Command);
            return Task.CompletedTask;
        }

        public Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            if (_responders.Count == 0)
            {
                throw new InvalidOperationException("No queued SMB2 response.");
            }

            return Task.FromResult(_responders.Dequeue()(_lastRequestHeader, _lastRequestPacket));
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }

        private static byte[] CreateResponsePacket(Smb2PacketHeader requestHeader, int bodyLength)
        {
            byte[] response = new byte[HeaderSize + bodyLength];
            var responseHeader = requestHeader with
            {
                Status = 0,
                Flags = FlagsServerToRedir,
            };
            responseHeader.Write(response);
            return response;
        }
    }
}
