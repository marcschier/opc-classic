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
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests;

public sealed class Smb2DecoderBoundsFuzzTests
{
    private const int PacketHeaderSize = 64;
    private const uint FlagsServerToRedir = 0x00000001;

    [Test]
    [Arguments("netbios-oversized-read")]
    [Arguments("connection-response-over-quota")]
    [Arguments("negotiate-truncated")]
    [Arguments("negotiate-bad-structure")]
    [Arguments("negotiate-security-underflow")]
    [Arguments("negotiate-security-out-of-range")]
    [Arguments("negotiate-oversized-body")]
    [Arguments("session-truncated")]
    [Arguments("session-bad-structure")]
    [Arguments("session-security-underflow")]
    [Arguments("session-security-out-of-range")]
    [Arguments("tree-truncated")]
    [Arguments("tree-bad-structure")]
    [Arguments("create-truncated")]
    [Arguments("create-bad-structure")]
    [Arguments("ioctl-truncated")]
    [Arguments("ioctl-bad-structure")]
    [Arguments("ioctl-output-underflow")]
    [Arguments("ioctl-output-out-of-range")]
    [Arguments("ioctl-output-negative-offset")]
    public async Task Smb2Parsers_RejectMalformedBoundedInputs(string caseName)
    {
        await Assert.That(async () => await ExecuteSmbCaseAsync(caseName)).Throws<Exception>();
    }

    private static async Task ExecuteSmbCaseAsync(string caseName)
    {
        switch (caseName)
        {
            case "netbios-oversized-read":
                _ = NetBiosFraming.ReadPayloadLength([0x00, 0x02, 0x00, 0x00]);
                break;
            case "connection-response-over-quota":
                await ExpectNegotiateRejectsAsync(new byte[256], maxMessageSize: 128).ConfigureAwait(false);
                break;
            case "negotiate-truncated":
                await ExpectNegotiateRejectsAsync(new byte[10]).ConfigureAwait(false);
                break;
            case "negotiate-bad-structure":
                await ExpectNegotiateRejectsAsync(new byte[64]).ConfigureAwait(false);
                break;
            case "negotiate-security-underflow":
                await ExpectNegotiateRejectsAsync(NegotiateBody(securityOffset: 0, securityLength: 1)).ConfigureAwait(false);
                break;
            case "negotiate-security-out-of-range":
                await ExpectNegotiateRejectsAsync(NegotiateBody(securityOffset: 128, securityLength: 1)).ConfigureAwait(false);
                break;
            case "negotiate-oversized-body":
                _ = Smb2NegotiateResponse.Read(new byte[0x1FFFF]);
                break;
            case "session-truncated":
                await ExpectSessionSetupRejectsAsync(new byte[4]).ConfigureAwait(false);
                break;
            case "session-bad-structure":
                await ExpectSessionSetupRejectsAsync(new byte[8]).ConfigureAwait(false);
                break;
            case "session-security-underflow":
                await ExpectSessionSetupRejectsAsync(SessionSetupBody(securityOffset: 0, securityLength: 1)).ConfigureAwait(false);
                break;
            case "session-security-out-of-range":
                await ExpectSessionSetupRejectsAsync(SessionSetupBody(securityOffset: 80, securityLength: 1)).ConfigureAwait(false);
                break;
            case "tree-truncated":
                _ = Smb2TreeConnectResponse.Read(new byte[8]);
                break;
            case "tree-bad-structure":
                _ = Smb2TreeConnectResponse.Read(new byte[16]);
                break;
            case "create-truncated":
                await ExpectCreateRejectsAsync(new byte[40]).ConfigureAwait(false);
                break;
            case "create-bad-structure":
                await ExpectCreateRejectsAsync(new byte[88]).ConfigureAwait(false);
                break;
            case "ioctl-truncated":
                await ExpectIoctlRejectsAsync(new byte[16]).ConfigureAwait(false);
                break;
            case "ioctl-bad-structure":
                await ExpectIoctlRejectsAsync(new byte[48]).ConfigureAwait(false);
                break;
            case "ioctl-output-underflow":
                await ExpectIoctlRejectsAsync(IoctlBody(outputOffset: 0, outputCount: 1)).ConfigureAwait(false);
                break;
            case "ioctl-output-out-of-range":
                await ExpectIoctlRejectsAsync(IoctlBody(outputOffset: 128, outputCount: 1)).ConfigureAwait(false);
                break;
            case "ioctl-output-negative-offset":
                await ExpectIoctlRejectsAsync(IoctlBody(outputOffset: uint.MaxValue, outputCount: 1)).ConfigureAwait(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "Unknown SMB2 fuzz case.");
        }
    }

    private static async Task ExpectNegotiateRejectsAsync(byte[] body, int maxMessageSize = 0x1FFFF)
    {
        var mock = new MockSmb2Transport(Packet(Smb2Command.Negotiate, body));
        await using var conn = new Smb2Connection(new Smb2ConnectionOptions("test") { MaxSmb2MessageSize = maxMessageSize }, mock);
        _ = await conn.NegotiateAsync().ConfigureAwait(false);
    }

    private static async Task ExpectSessionSetupRejectsAsync(byte[] body)
    {
        var mock = new MockSmb2Transport(
            Packet(Smb2Command.Negotiate, NegotiateBody()),
            Packet(Smb2Command.SessionSetup, body, sessionId: 0x1000));
        await using var conn = new Smb2Connection(new Smb2ConnectionOptions("test"), mock);
        _ = await conn.NegotiateAsync().ConfigureAwait(false);
        await conn.SessionSetupAsync(OneRoundTripBlobProvider()).ConfigureAwait(false);
    }

    private static async Task ExpectCreateRejectsAsync(byte[] body)
    {
        var mock = new MockSmb2Transport(
            Packet(Smb2Command.Negotiate, NegotiateBody()),
            Packet(Smb2Command.SessionSetup, SessionSetupBody(), sessionId: 0x1000),
            Packet(Smb2Command.TreeConnect, TreeBody(), sessionId: 0x1000, treeId: 0x2000),
            Packet(Smb2Command.Create, body, sessionId: 0x1000, treeId: 0x2000));
        await using var conn = new Smb2Connection(new Smb2ConnectionOptions("test"), mock);
        await EstablishTreeAsync(conn).ConfigureAwait(false);
        _ = await conn.OpenNamedPipeAsync("winreg").ConfigureAwait(false);
    }

    private static async Task ExpectIoctlRejectsAsync(byte[] body)
    {
        var mock = new MockSmb2Transport(
            Packet(Smb2Command.Negotiate, NegotiateBody()),
            Packet(Smb2Command.SessionSetup, SessionSetupBody(), sessionId: 0x1000),
            Packet(Smb2Command.TreeConnect, TreeBody(), sessionId: 0x1000, treeId: 0x2000),
            Packet(Smb2Command.Create, CreateBody(), sessionId: 0x1000, treeId: 0x2000),
            Packet(Smb2Command.Ioctl, body, sessionId: 0x1000, treeId: 0x2000));
        await using var conn = new Smb2Connection(new Smb2ConnectionOptions("test"), mock);
        await EstablishTreeAsync(conn).ConfigureAwait(false);
        var pipe = await conn.OpenNamedPipeAsync("winreg").ConfigureAwait(false);
        _ = await pipe.TransceiveAsync(new byte[] { 0x01 }).ConfigureAwait(false);
    }

    private static async Task EstablishTreeAsync(Smb2Connection conn)
    {
        _ = await conn.NegotiateAsync().ConfigureAwait(false);
        await conn.SessionSetupAsync(OneRoundTripBlobProvider()).ConfigureAwait(false);
        _ = await conn.TreeConnectIpcAsync().ConfigureAwait(false);
    }

    private static NtlmsspBlobProvider OneRoundTripBlobProvider()
    {
        var calls = 0;
        return _ => calls++ == 0 ? new byte[] { 0x01 } : null;
    }

    private static byte[] Packet(
        Smb2Command command,
        byte[] body,
        ulong sessionId = 0,
        uint treeId = 0,
        uint status = 0)
    {
        var packet = new byte[PacketHeaderSize + body.Length];
        var header = new Smb2PacketHeader(
            CreditCharge: 1,
            Status: status,
            Command: command,
            CreditRequestResponse: 1,
            Flags: FlagsServerToRedir,
            NextCommand: 0,
            MessageId: 1,
            ProcessId: 0,
            TreeId: treeId,
            SessionId: sessionId,
            Signature: ReadOnlyMemory<byte>.Empty);
        header.Write(packet);
        body.CopyTo(packet.AsSpan(PacketHeaderSize));
        return packet;
    }

    private static byte[] NegotiateBody(ushort securityOffset = 0, ushort securityLength = 0)
    {
        var body = new byte[64];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 65);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(4), (ushort)Smb2Dialect.Smb300);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(28), 0x10000);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(32), 0x10000);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(36), 0x10000);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(56), securityOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(58), securityLength);
        return body;
    }

    private static byte[] SessionSetupBody(ushort securityOffset = 0, ushort securityLength = 0)
    {
        var body = new byte[8];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(4), securityOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(6), securityLength);
        return body;
    }

    private static byte[] TreeBody()
    {
        var body = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 16);
        return body;
    }

    private static byte[] CreateBody()
    {
        var body = new byte[88];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 89);
        BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(64), 1);
        BinaryPrimitives.WriteUInt64LittleEndian(body.AsSpan(72), 2);
        return body;
    }

    private static byte[] IoctlBody(uint outputOffset = 0, uint outputCount = 0)
    {
        var body = new byte[48];
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(0), 49);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(32), outputOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(36), outputCount);
        return body;
    }

    private sealed class MockSmb2Transport : ISmb2Transport
    {
        private readonly Queue<ReadOnlyMemory<byte>> _responses;

        public MockSmb2Transport(params byte[][] responses)
        {
            _responses = new Queue<ReadOnlyMemory<byte>>();
            foreach (var response in responses)
            {
                _responses.Enqueue(response);
            }
        }

        public Task SendAsync(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_responses.Count == 0)
            {
                throw new InvalidOperationException("No queued SMB2 fuzz response.");
            }
            return Task.FromResult(_responses.Dequeue());
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
