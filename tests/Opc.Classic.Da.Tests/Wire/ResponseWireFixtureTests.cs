//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Track AL2: byte-exact response-decoding fixtures. Each test synthesizes
// a wire payload that matches the MIDL canonical layout for one of the
// Matrikon-OK methods, then asserts the proxy decodes it to the expected
// managed result. Companion to AL1 (request encoding).
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class ResponseWireFixtureTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WritePayload(NdrWriteAction write, int capacity = 1024)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static ICallChannel ChannelReturning(byte[] response) =>
        new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, response)));

    /// <summary>
    /// IOPCSyncIO::Write response: unique-pointer-to-HRESULT[2] — referent + max_count + 2 ints.
    /// </summary>
    [Test]
    public async Task SyncIO_Write_DecodesUniquePointerHresultArray()
    {
        byte[] response = WritePayload((ref NdrWriter w) =>
        {
            w.WriteUniquePointerReferent(true);   // outer referent for ppErrors
            w.WriteUInt32(2);                      // max_count
            w.WriteInt32(0);                       // errors[0] = S_OK
            w.WriteInt32(unchecked((int)0xC0040004u)); // errors[1] = OPC_E_BADTYPE
        });

        var proxy = new IOPCSyncIOClientProxy(ChannelReturning(response));
        int[] errors = await proxy.WriteAsync(
            new[] { 100, 101 },
            new[] { OpcVariant.FromInt32(42), OpcVariant.FromInt32(7) },
            CancellationToken.None);

        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(errors[1]).IsEqualTo(unchecked((int)0xC0040004u));
    }

    /// <summary>
    /// IOPCItemMgt::AddItems response: unique-pointer-to-OPCITEMRESULT[] + unique-pointer-to-HRESULT[].
    /// Uses <see cref="NdrOpcItemResultCodec.WriteConformantArray"/> which already emits the canonical
    /// referent + max_count + inline + deferred shape.
    /// </summary>
    [Test]
    public async Task ItemMgt_AddItems_DecodesUniquePointerResultsAndErrors()
    {
        byte[] response = WritePayload((ref NdrWriter w) =>
        {
            // ppAddResults: referent + max_count + 2 OPCITEMRESULT inline + 2 deferred (empty blobs).
            NdrOpcItemResultCodec.WriteConformantArray(ref w,
            [
                new OpcItemResult(ServerHandle: 0x4ABCD00, CanonicalDataType: VarType.VT_I4, AccessRights: 3, Blob: Array.Empty<byte>()),
                new OpcItemResult(ServerHandle: 0x4ABCD68, CanonicalDataType: VarType.VT_BSTR, AccessRights: 3, Blob: Array.Empty<byte>()),
            ]);
            // ppErrors: referent + max_count + 2 HRESULTs.
            w.WriteUniquePointerReferent(true);
            w.WriteUInt32(2);
            w.WriteInt32(0);
            w.WriteInt32(0);
        });

        var proxy = new IOPCItemMgtClientProxy(ChannelReturning(response));
        await proxy.AddItemsAsync(
            new[]
            {
                new OpcItemDef(null, "Bucket Brigade.Int4", true, 1, Array.Empty<byte>(), VarType.VT_EMPTY),
                new OpcItemDef(null, "Bucket Brigade.String", true, 2, Array.Empty<byte>(), VarType.VT_EMPTY),
            },
            out OpcItemResult[] addResults,
            out int[] errors,
            CancellationToken.None);

        await Assert.That(addResults.Length).IsEqualTo(2);
        await Assert.That(addResults[0].ServerHandle).IsEqualTo(0x4ABCD00);
        await Assert.That(addResults[0].CanonicalDataType).IsEqualTo(VarType.VT_I4);
        await Assert.That(addResults[0].AccessRights).IsEqualTo(3);
        await Assert.That(addResults[1].ServerHandle).IsEqualTo(0x4ABCD68);
        await Assert.That(addResults[1].CanonicalDataType).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors.All(static e => e == 0)).IsTrue();
    }

    /// <summary>
    /// Empty-array case: a null outer referent on ppErrors must decode to <c>int[0]</c> without
    /// trying to read max_count past end-of-buffer (Track AG4 null-referent decode safety).
    /// </summary>
    [Test]
    public async Task SyncIO_Write_NullErrorsReferent_DecodesToEmptyArray()
    {
        byte[] response = WritePayload((ref NdrWriter w) =>
        {
            w.WriteUniquePointerReferent(false);  // null referent for ppErrors
        });

        var proxy = new IOPCSyncIOClientProxy(ChannelReturning(response));
        int[] errors = await proxy.WriteAsync(
            Array.Empty<int>(),
            Array.Empty<OpcVariant>(),
            CancellationToken.None);

        await Assert.That(errors.Length).IsEqualTo(0);
    }
}
