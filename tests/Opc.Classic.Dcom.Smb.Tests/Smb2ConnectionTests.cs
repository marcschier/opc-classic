//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Smb;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests;

/// <summary>
/// End-to-end state-machine tests that run against an in-memory mock SMB2 server.
/// These validate the request/response sequencing and field-bookkeeping (SessionId,
/// TreeId, MessageId increment) without touching the network.
/// </summary>
public sealed class Smb2ConnectionTests
{
    [Test]
    public async Task Negotiate_SetsDialectFromServerResponse()
    {
        var mock = new MockSmb2Transport();
        mock.OnNegotiate(dialect: Smb2Dialect.Smb300, serverGuid: Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00"));

        await using var conn = new Smb2Connection(
            new Smb2ConnectionOptions("test"), mock);
        var resp = await conn.NegotiateAsync();
        await Assert.That(resp.Dialect).IsEqualTo(Smb2Dialect.Smb300);
        await Assert.That(conn.NegotiatedDialect).IsEqualTo(Smb2Dialect.Smb300);
    }

    [Test]
    public async Task SessionSetup_CapturesSessionIdFromFirstResponse()
    {
        var mock = new MockSmb2Transport();
        mock.OnNegotiate(Smb2Dialect.Smb300, Guid.NewGuid());
        mock.OnSessionSetupSuccess(sessionId: 0x1122334455667788UL);

        await using var conn = new Smb2Connection(
            new Smb2ConnectionOptions("test"), mock);
        await conn.NegotiateAsync();
        await conn.SessionSetupAsync(static _ => new byte[] { 0x01, 0x02, 0x03 });

        await Assert.That(conn.SessionId).IsEqualTo(0x1122334455667788UL);
    }

    [Test]
    public async Task TreeConnectIpcAsync_RequiresPriorSessionSetup()
    {
        var mock = new MockSmb2Transport();
        mock.OnNegotiate(Smb2Dialect.Smb300, Guid.NewGuid());

        await using var conn = new Smb2Connection(
            new Smb2ConnectionOptions("test"), mock);
        await conn.NegotiateAsync();

        bool threw = false;
        try { _ = await conn.TreeConnectIpcAsync(); }
        catch (InvalidOperationException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    private sealed class MockSmb2Transport : ISmb2Transport
    {
        private readonly Queue<Func<Smb2PacketHeader, ReadOnlyMemory<byte>>> _responders = new();
        private Smb2PacketHeader _lastRequestHeader;

        public void OnNegotiate(Smb2Dialect dialect, Guid serverGuid)
        {
            _responders.Enqueue(header =>
            {
                byte[] response = new byte[64 + 72];
                var responseHeader = header with
                {
                    Status = 0,
                    Flags = 0x00000001, // SMB2 FLAGS_SERVER_TO_REDIR
                };
                responseHeader.Write(response);

                int bodyOffset = 64;
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 0), 65);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 2), 0);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 4), (ushort)dialect);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 6), 0);
                serverGuid.TryWriteBytes(response.AsSpan(bodyOffset + 8, 16));
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
            _responders.Enqueue(header =>
            {
                byte[] response = new byte[64 + 16];
                var responseHeader = header with
                {
                    Status = 0,
                    SessionId = sessionId,
                    Flags = 0x00000001, // SMB2 FLAGS_SERVER_TO_REDIR
                };
                responseHeader.Write(response);

                int bodyOffset = 64;
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 0), 9);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 2), 0);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 4), 64 + 8);
                BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(bodyOffset + 6), 8);
                response[bodyOffset + 8] = 0xAA;
                return response;
            });
        }

        public Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
        {
            _lastRequestHeader = Smb2PacketHeader.Read(packet.Span);
            return Task.CompletedTask;
        }

        public Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_responders.Count == 0)
            {
                throw new InvalidOperationException("No queued mock response.");
            }
            var responder = _responders.Dequeue();
            return Task.FromResult(responder(_lastRequestHeader));
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
