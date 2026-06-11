//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Smoke + wire fixture tests for the IRemUnknown proxy generated from
// src/Opc.Classic.Dcom/Remoting/IRemUnknown.cs. Verifies that:
//   - The generator produces a working IRemUnknownClientProxy.
//   - RemQueryInterface (opnum 3) encodes the request body per MS-DCOM §3.1.1.5.6.1.
//   - The proxy decodes a unique-pointer-prefixed REMQIRESULT[] response.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Remoting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests.Remoting;

public sealed class IRemUnknownProxyTests
{
    private static readonly Guid IRemUnknownIid = new("00000131-0000-0000-c000-000000000046");
    private static readonly Guid SampleRipid = new("11111111-2222-3333-4444-555555555555");
    private static readonly Guid Iid1 = new("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
    private static readonly Guid Iid2 = new("39C13A50-011E-11D0-9675-0020AFD8ADB3");

    [Test]
    public async Task RemQueryInterface_routes_to_IRemUnknown_at_opnum_3()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            byte[] payload = new byte[8];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), 0x00020000u);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), 0u);
            return Task.FromResult(new NdrCallResult(0, payload));
        });

        var proxy = new IRemUnknownClientProxy(channel);
        OpcRemQIResult[] results = await proxy.RemQueryInterfaceAsync(
            ripid: SampleRipid,
            cRefs: 5,
            cIids: 0,
            iids: Array.Empty<Guid>(),
            cancellationToken: CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IRemUnknownIid);
        await Assert.That(observedOpnum).IsEqualTo(3);
        await Assert.That(results.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RemQueryInterface_request_body_carries_ripid_cRefs_cIids_iids_array()
    {
        ReadOnlyMemory<byte> captured = ReadOnlyMemory<byte>.Empty;
        var channel = new InMemoryCallChannel((_, _, payload, _) =>
        {
            captured = payload.ToArray();
            byte[] response = new byte[8];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(0, 4), 0x00020000u);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(4, 4), 0u);
            return Task.FromResult(new NdrCallResult(0, response));
        });
        var proxy = new IRemUnknownClientProxy(channel);

        _ = await proxy.RemQueryInterfaceAsync(
            ripid: SampleRipid,
            cRefs: 5,
            cIids: 2,
            iids: new[] { Iid1, Iid2 },
            cancellationToken: CancellationToken.None);

        byte[] wire = captured.ToArray();
        // Per [MS-DCOM] §3.1.1.5.6.1 RemQueryInterface request body:
        //   [0..15]  ripid (Guid)
        //   [16..19] cRefs (UInt32)
        //   [20..21] cIids (UInt16)
        //   [22..23] padding to 4-byte boundary before the conformant array
        //   [24..27] conformant max_count = cIids
        //   [28..]   IID[max_count] (each 16 bytes)
        await Assert.That(new Guid(wire.AsSpan(0, 16))).IsEqualTo(SampleRipid);
        await Assert.That(System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(wire.AsSpan(16, 4))).IsEqualTo(5u);
        await Assert.That(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(wire.AsSpan(20, 2))).IsEqualTo((ushort)2);
        await Assert.That(System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(wire.AsSpan(24, 4))).IsEqualTo(2u);
        await Assert.That(new Guid(wire.AsSpan(28, 16))).IsEqualTo(Iid1);
        await Assert.That(new Guid(wire.AsSpan(44, 16))).IsEqualTo(Iid2);
    }

    [Test]
    public async Task RemQueryInterface_decodes_REMQIRESULT_array_with_referent_prefix()
    {
        var expected1 = new OpcRemQIResult(hresult: 0, flags: 0, publicRefs: 5, oxid: 0xAAAA, oid: 0xBBBB, ipid: Iid1);
        var expected2 = new OpcRemQIResult(hresult: unchecked((int)0x80004002u), flags: 0, publicRefs: 0, oxid: 0, oid: 0, ipid: Guid.Empty);

        byte[] response = new byte[256];
        var w = new NdrWriter(response);
        w.WriteUInt32(0x00020000u);
        w.WriteUInt32(2u);
        NdrRemQIResultCodec.Write(ref w, expected1);
        NdrRemQIResultCodec.Write(ref w, expected2);

        byte[] payload = response.AsMemory(0, w.Position).ToArray();
        var channel = new InMemoryCallChannel((_, _, _, _) =>
            Task.FromResult(new NdrCallResult(0, payload)));
        var proxy = new IRemUnknownClientProxy(channel);

        OpcRemQIResult[] actual = await proxy.RemQueryInterfaceAsync(
            ripid: SampleRipid, cRefs: 5, cIids: 2, iids: new[] { Iid1, Iid2 }, cancellationToken: CancellationToken.None);

        await Assert.That(actual.Length).IsEqualTo(2);
        await Assert.That(actual[0].Hresult).IsEqualTo(0);
        await Assert.That(actual[0].PublicRefs).IsEqualTo(5u);
        await Assert.That(actual[0].Oxid).IsEqualTo(0xAAAAul);
        await Assert.That(actual[0].Ipid).IsEqualTo(Iid1);
        await Assert.That(actual[1].Hresult).IsEqualTo(unchecked((int)0x80004002u));
        await Assert.That(actual[1].Ipid).IsEqualTo(Guid.Empty);
    }
}
