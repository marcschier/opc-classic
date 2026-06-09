//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Wire-format fixture tests for IOPCServer::AddGroup (opnum 3) — the Y2/Y9
// poster-child for the NDR unique-pointer + MInterfacePointer work.
//
// Request layout per opcda.idl:
//   szName              [in, string] LPCWSTR       (unique-pointer LPWSTR)
//   bActive             [in] BOOL                  (4-byte inline)
//   dwRequestedUpdateRate [in] DWORD               (4-byte inline)
//   hClientGroup        [in] OPCHANDLE             (4-byte inline)
//   pTimeBias           [in, unique] LONG *        (referent + Int32)
//   pPercentDeadband    [in, unique] FLOAT *       (referent + Single)
//   dwLCID              [in] DWORD                 (4-byte inline)
//   riid                [in] REFIID                (16-byte Guid)
//   (response: phServerGroup + pRevisedUpdateRate inline, ppUnk wrapped in MInterfacePointer)
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class IOPCServerAddGroupWireFixtures {
    private static readonly Guid SampleIid = new("39C13A4E-011E-11D0-9675-0020AFD8ADB3");   // IID_IOPCGroupStateMgt

    [Test]
    public async Task Request_AddGroup_emits_unique_pointer_referents_for_pTimeBias_and_pPercentDeadband() {
        ReadOnlyMemory<byte> captured = ReadOnlyMemory<byte>.Empty;
        var channel = new InMemoryCallChannel((interfaceId, opnum, payload, _) => {
            captured = payload.ToArray();
            // Return a minimal success response so the proxy doesn't fault during decode;
            // serverGroupHandle + revisedUpdateRate + null ppUnk referent.
            byte[] response = new byte[12];
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(0, 4), 42);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(4, 4), 1000);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(8, 4), 0u); // null ppUnk
            return Task.FromResult(new NdrCallResult(0, response));
        });
        var proxy = new IOPCServerClientProxy(channel);

        try {
            await proxy.AddGroupAsync(
                name: "G",
                active: true,
                requestedUpdateRate: 1000,
                clientGroupHandle: 1,
                timeBias: -300,
                percentDeadband: 1.5f,
                localeId: 0x0409,
                requestedInterfaceId: SampleIid,
                serverGroupHandle: out _,
                revisedUpdateRate: out _,
                group: out _,
                cancellationToken: CancellationToken.None);
        }
        catch {
            // Proxy may throw on null ppUnk; the test only inspects the captured REQUEST.
        }

        ReadOnlySpan<byte> wire = captured.ToArray();

        // szName = "G\0" → bare conformant-varying string at [0..]
        //   (per DCE 1.1 §4.2.2.7 top-level [ref] LPCWSTR has no referent prefix):
        //   max_count(2) + offset(0) + actual_count(2) + WCHAR[2] ('G'\0)
        var wireBytes = captured.ToArray();
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 0)).IsEqualTo(2u);   // max_count
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 4)).IsEqualTo(0u);   // offset
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 8)).IsEqualTo(2u);   // actual_count
        await Assert.That(wireBytes[12]).IsEqualTo((byte)'G');
        await Assert.That(wireBytes[13]).IsEqualTo((byte)0);
        await Assert.That(wireBytes[14]).IsEqualTo((byte)0);
        await Assert.That(wireBytes[15]).IsEqualTo((byte)0);

        // bActive=true → -1 at [16..19] (current codec emits VARIANT_BOOL TRUE = 0xFFFFFFFF).
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 16)).IsEqualTo(0xFFFFFFFFu);
        // dwRequestedUpdateRate=1000 → 0x3E8 at [20..23]
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 20)).IsEqualTo(1000u);
        // hClientGroup=1 → [24..27]
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 24)).IsEqualTo(1u);

        // pTimeBias is [OpcUniquePointer] non-nullable Int32 → referent + value.
        // Referent IDs auto-increment per NdrWriter (DCE 1.1 §14.3.10) — szName
        // no longer takes a referent slot ([OpcRefString]), so pTimeBias is the
        // 1st unique referent.
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 28)).IsEqualTo(0x00020000u);
        // -300 = 0xFFFFFED4 as Int32 little-endian.
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 32)).IsEqualTo(0xFFFFFED4u);

        // pPercentDeadband is [OpcUniquePointer] non-nullable Single → referent + value.
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 36)).IsEqualTo(0x00020004u);
        // 1.5f IEEE-754 single = 0x3FC00000.
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 40)).IsEqualTo(0x3FC00000u);

        // dwLCID=0x0409 → [48..51]
        // dwLCID=0x0409 → [44..47]
        await Assert.That(WireAssert.ReadUInt32At(wireBytes, 44)).IsEqualTo(0x0409u);

        // REFIID at [48..63] (16-byte Guid). Total request = 64 bytes (was 68 before
        // [OpcRefString] removed szName's 4-byte referent slot).
        await Assert.That(wireBytes.Length).IsEqualTo(64);
    }

    [Test]
    public async Task Response_AddGroup_decodes_MInterfacePointer_wrapped_ppUnk() {
        // Server side: encode serverHandle + revisedUpdateRate + MInterfacePointer-wrapped group OBJREF.
        var groupRef = new OpcInterfaceRef(
            iid: SampleIid,
            flags: 0u,
            publicRefs: 5u,
            oxid: 0x1111_2222_3333_4444ul,
            oid: 0x5555_6666_7777_8888ul,
            ipid: new Guid("00000001-0000-0000-c000-000000000046"),
            securityOffset: 0,
            resolverBindings: System.Array.Empty<ushort>());

        var bufferOwner = System.Buffers.ArrayPool<byte>.Shared.Rent(512);
        ReadOnlyMemory<byte> response;
        try {
            var writer = new Opc.Classic.Ndr.NdrWriter(bufferOwner.AsSpan());
            writer.WriteInt32(0xABCD);                    // serverHandle
            writer.WriteInt32(0x1234);                    // revisedUpdateRate
            OpcMInterfacePointerCodec.Write(ref writer, groupRef);
            response = bufferOwner.AsMemory(0, writer.Position).ToArray();
        }
        finally {
            System.Buffers.ArrayPool<byte>.Shared.Return(bufferOwner);
        }

        var channel = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, response)));
        var proxy = new IOPCServerClientProxy(channel);

        await proxy.AddGroupAsync(
            name: "G",
            active: true,
            requestedUpdateRate: 1000,
            clientGroupHandle: 1,
            timeBias: 0,
            percentDeadband: 0f,
            localeId: 0x0409,
            requestedInterfaceId: SampleIid,
            serverGroupHandle: out int serverHandle,
            revisedUpdateRate: out int revisedRate,
            group: out IOpcInterfaceRef decodedGroup,
            cancellationToken: CancellationToken.None);

        await Assert.That(serverHandle).IsEqualTo(0xABCD);
        await Assert.That(revisedRate).IsEqualTo(0x1234);
        await Assert.That(decodedGroup).IsNotNull();
        await Assert.That(decodedGroup.Iid).IsEqualTo(SampleIid);
        await Assert.That(decodedGroup.Oxid).IsEqualTo(0x1111_2222_3333_4444ul);
    }
}
