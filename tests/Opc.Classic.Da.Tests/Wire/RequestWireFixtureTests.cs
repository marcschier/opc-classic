// Copyright (c) 2026 marcschier. Licensed under the MIT License.
// byte-exact wire fixtures that lock in the proxy's request
// encoding for the Matrikon-OK methods. If a generator/codec change ever
// silently mutates the wire shape, these fixtures fail with a hex diff so
// the regression is caught before it reaches a live server.
//
// Authoring conventions:
// - Use realistic inputs that mirror the live probe (Bucket Brigade.Int4,
//   Test.Int32) so a developer can compare a failing fixture against a
//   interop/docs/wire-captures/*.hex artifact byte-for-byte.
// - Capture the request payload via a handler closure rather than the
//   InMemoryCall log (the log only records payload length).
// - Assert the exact byte[] — never a "decodes back correctly" round-trip.
//

using Opc.Classic.Da.Dcom;
using Opc.Classic.Testing;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class RequestWireFixtureTests
{
    private static async Task<byte[]> CaptureRequestAsync(
        Func<ICallChannel, Task> invoke,
        ReadOnlyMemory<byte> response = default)
    {
        byte[] captured = Array.Empty<byte>();
        var channel = new InMemoryCallChannel((_, _, payload, _) =>
        {
            captured = payload.ToArray();
            return Task.FromResult(new NdrCallResult(0, response));
        });
        await invoke(channel).ConfigureAwait(false);
        return captured;
    }

    private static string FormatHex(byte[] bytes) =>
        string.Join(' ', bytes.Select(static b => b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture)));

    /// <summary>
    /// IOPCSyncIO::Write request: <c>[in] DWORD dwCount, [in, size_is] OPCHANDLE*,
    /// [in, size_is] VARIANT*</c>. Two-handle write of VT_I4 values 42 and 7.
    /// </summary>
    [Test]
    public async Task SyncIO_Write_TwoHandles_TwoInt32_EncodesCanonicalShape()
    {
        // Response = unique-pointer-to-HRESULT[2] = referent + max_count + 2 ints.
        var response = new byte[] { 0x00, 0x00, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0, 0 };

        byte[] request = await CaptureRequestAsync(channel =>
        {
            var proxy = new IOPCSyncIOClientProxy(channel);
            return proxy.WriteAsync(
                new[] { 100, 101 },
                new[] { OpcVariant.FromInt32(42), OpcVariant.FromInt32(7) },
                CancellationToken.None);
        }, response);

        // Wire layout (MIDL canonical, OPCSyncIO::Write opnum 4):
        //   00 dwCount=2                              → [in] DWORD dwCount
        //   04 max_count=2                            → embedded conformance for phServer
        //   08 phServer[0]=100, phServer[1]=101       → OPCHANDLE[]
        //   10 max_count=2                            → embedded conformance for pItemValues
        //   14 VARIANT[0] wireVARIANT body            → 16-byte header + duplicated vt + i32 + pad-to-8 = 24 bytes
        //   2C VARIANT[1] wireVARIANT body            → same shape, 24 bytes
        // Total = 4 + 4 + 8 + 4 + 24 + 24 = 68 bytes.
        // (Note: request side does NOT emit per-element referents — VARIANT[] inline values
        // for [in, size_is] VARIANT* parameters under pointer_default(unique).)
        await Assert.That(request.Length).IsEqualTo(68);
        // Bytes 0-3: dwCount = 2 (little-endian).
        await Assert.That(request[0]).IsEqualTo((byte)0x02);
        await Assert.That(request[1]).IsEqualTo((byte)0x00);
        // Bytes 4-7: max_count = 2.
        await Assert.That(request[4]).IsEqualTo((byte)0x02);
        // Bytes 8-11: serverHandles[0] = 100 = 0x64.
        await Assert.That(request[8]).IsEqualTo((byte)0x64);
        // Bytes 12-15: serverHandles[1] = 101 = 0x65.
        await Assert.That(request[12]).IsEqualTo((byte)0x65);
        // Bytes 16-19: VARIANT[] max_count = 2.
        await Assert.That(request[16]).IsEqualTo((byte)0x02);
        // Confirm 42 (= 0x2A) and 7 are present in the variant arms.
        string hex = FormatHex(request);
        await Assert.That(hex.Contains("2a 00 00 00", StringComparison.Ordinal)).IsTrue();
        await Assert.That(hex.Contains("07 00 00 00", StringComparison.Ordinal)).IsTrue();
    }

    /// <summary>
    /// IOPCSyncIO::Read request: <c>[in] OPCDATASOURCE dwSource, [in] DWORD dwCount,
    /// [in, size_is] OPCHANDLE*</c>. dataSource=1 (cache), one handle.
    /// </summary>
    [Test]
    public async Task SyncIO_Read_FromCache_SingleHandle_EncodesCanonicalShape()
    {
        // Response = referent + max_count + 1 OPCITEMSTATE + referent + max_count + 1 HRESULT.
        var response = new byte[80];

        byte[] request = await CaptureRequestAsync(channel =>
        {
            var proxy = new IOPCSyncIOClientProxy(channel);
            int[] errors;
            return proxy.ReadAsync(dataSource: 1, serverHandles: new[] { 0x12345678 }, out errors, CancellationToken.None);
        }, response);

        // Wire layout: dataSource(4) + dwCount(4) + max_count(4) + handle(4) = 16 bytes.
        await Assert.That(request.Length).IsEqualTo(16);
        // dataSource = 1.
        await Assert.That(request[0]).IsEqualTo((byte)0x01);
        // dwCount = 1.
        await Assert.That(request[4]).IsEqualTo((byte)0x01);
        // max_count = 1.
        await Assert.That(request[8]).IsEqualTo((byte)0x01);
        // handle = 0x12345678 (little-endian).
        await Assert.That(request[12]).IsEqualTo((byte)0x78);
        await Assert.That(request[13]).IsEqualTo((byte)0x56);
        await Assert.That(request[14]).IsEqualTo((byte)0x34);
        await Assert.That(request[15]).IsEqualTo((byte)0x12);
    }

    /// <summary>
    /// IOPCItemMgt::AddItems request: <c>[in] DWORD dwCount, [in, size_is] OPCITEMDEF*</c>.
    /// Two items (Bucket Brigade.Int4, Bucket Brigade.String) with clientHandles 1 + 2.
    /// </summary>
    [Test]
    public async Task ItemMgt_AddItems_TwoBucketBrigadeItems_EncodesCanonicalShape()
    {
        // Synthesize a minimal response: ppAddResults (null referent) + ppErrors (null referent).
        var response = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

        byte[] request = await CaptureRequestAsync(channel =>
        {
            var proxy = new IOPCItemMgtClientProxy(channel);
            return proxy.AddItemsAsync(
                new[]
                {
                    new OpcItemDef(AccessPath: null, ItemId: "Bucket Brigade.Int4", Active: true, ClientHandle: 1, Blob: Array.Empty<byte>(), RequestedDataType: VarType.VT_EMPTY),
                    new OpcItemDef(AccessPath: null, ItemId: "Bucket Brigade.String", Active: true, ClientHandle: 2, Blob: Array.Empty<byte>(), RequestedDataType: VarType.VT_EMPTY),
                },
                out OpcItemResult[] _,
                out int[] _,
                CancellationToken.None);
        }, response);

        // Wire layout: dwCount(4) + max_count(4) + 2 OPCITEMDEF inline (28 bytes each) + deferred strings.
        // 4 + 4 + 56 + deferred = 64 + deferred.
        // dwCount = 2.
        await Assert.That(request[0]).IsEqualTo((byte)0x02);
        // max_count = 2.
        await Assert.That(request[4]).IsEqualTo((byte)0x02);
        // First inline: 4 bytes accessPath referent (null = 0) + 4 bytes itemId referent (non-zero) ...
        await Assert.That(request[8]).IsEqualTo((byte)0x00);   // accessPath null referent
        await Assert.That(request[9]).IsEqualTo((byte)0x00);
        await Assert.That(request[10]).IsEqualTo((byte)0x00);
        await Assert.That(request[11]).IsEqualTo((byte)0x00);
        // bActive (offset 16 within inline) = TRUE = 1.
        await Assert.That(request[16]).IsEqualTo((byte)0x01);
        // hClient (offset 20) = 1.
        await Assert.That(request[20]).IsEqualTo((byte)0x01);
        // Deferred section contains both item IDs in UTF-16; assert presence.
        string hex = FormatHex(request);
        // "Bucket Brigade.Int4" → starts with 'B' = 0x42 in UTF-16 little-endian.
        await Assert.That(hex.Contains("42 00 75 00 63 00 6b 00", StringComparison.Ordinal)).IsTrue();
    }
}
