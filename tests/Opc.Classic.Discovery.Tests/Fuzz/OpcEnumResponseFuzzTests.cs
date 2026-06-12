//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using CsCheck;
using Opc.Classic.Discovery.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Tests.Fuzz;

namespace Opc.Classic.Discovery.Tests.Fuzz;

public sealed class OpcEnumResponseFuzzTests
{
    private static readonly Type[] AllowedOpcEnumDecodeExceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(InvalidOperationException),
    ];

    private static readonly byte[] ValidResponse = WritePayload(static (ref NdrWriter writer) =>
    {
        writer.WriteUInt32(1);
        writer.WriteUInt32(0);
        writer.WriteUInt32(1);
        writer.WriteGuid(new Guid("01234567-89ab-cdef-0123-456789abcdef"));
        writer.WriteInt32(1);
    });

    [Test]
    [Category("Fuzz")]
    public async Task OpcEnum_ServerListResponse_Decode_RandomBytes_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.BytesEdgeWeighted.Sample(bytes =>
        {
            exercised++;
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                DecodeServerListResponse,
                AllowedOpcEnumDecodeExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    public async Task OpcEnum_ServerListResponse_Decode_MutatedValid_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.MutateValid(ValidResponse).Sample(bytes =>
        {
            exercised++;
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                DecodeServerListResponse,
                AllowedOpcEnumDecodeExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    public async Task OpcEnum_ServerListResponse_Decode_OverlargeCount_Bounded()
    {
        byte[] input = WritePayload(static (ref NdrWriter writer) =>
        {
            writer.WriteUInt32(int.MaxValue);
            writer.WriteUInt32(0);
            writer.WriteUInt32(int.MaxValue);
        });

        FuzzHarness.AssertParseDoesNotCrash(
            input,
            DecodeServerListResponse,
            AllowedOpcEnumDecodeExceptions);

        await Assert.That(input.Length).IsGreaterThan(0);
    }

    private static OpcEnumGuidNextResult DecodeServerListResponse(ReadOnlyMemory<byte> payload)
    {
        var proxy = new IOPCEnumGUIDClientProxy(new FixedResponseCallChannel(payload.ToArray()));
        return proxy.NextAsync(1).GetAwaiter().GetResult();
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WritePayload(NdrWriteAction action)
    {
        var buffer = new byte[256];
        var writer = new NdrWriter(buffer);
        action(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private sealed class FixedResponseCallChannel(byte[] responsePayload) : ICallChannel
    {
        public Task<NdrCallResult> InvokeAsync(
            Guid interfaceId,
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new NdrCallResult(0, responsePayload));
    }
}
