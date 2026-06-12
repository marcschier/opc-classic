//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Tests;

public sealed class ICallChannelAndSafeArrayExtensionsTests
{
    // ---- NdrCallResult ----

    [Test]
    public async Task NdrCallResult_OkHresult_IsSuccess()
    {
        var r = new NdrCallResult(0, ReadOnlyMemory<byte>.Empty);
        await Assert.That(r.IsSuccess).IsTrue();
        await Assert.That(r.IsFailure).IsFalse();
    }

    [Test]
    public async Task NdrCallResult_OpcEHresult_IsFailure()
    {
        var r = new NdrCallResult(unchecked((int)0xC0040001u), ReadOnlyMemory<byte>.Empty);
        await Assert.That(r.IsFailure).IsTrue();
        await Assert.That(r.IsSuccess).IsFalse();
    }

    [Test]
    public async Task NdrCallResult_PreservesResponsePayload()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        var r = new NdrCallResult(0, payload);
        await Assert.That(r.ResponsePayload.Length).IsEqualTo(4);
        await Assert.That(r.ResponsePayload.Span[2]).IsEqualTo((byte)3);
    }

    // ---- OpcVariant.FromSafeArray + AsSafeArray ----

    [Test]
    public async Task FromSafeArray_TypeCarriesVtArrayModifier()
    {
        var arr = OpcSafeArray.OfInt32(new[] { 1, 2, 3 });
        var v = OpcVariant.FromSafeArray(arr);
        await Assert.That(VarTypeMask.IsArray(v.Type)).IsTrue();
        await Assert.That(VarTypeMask.BaseOf(v.Type)).IsEqualTo(VarType.VT_I4);
    }

    [Test]
    public async Task AsSafeArray_RoundTrips()
    {
        var arr = OpcSafeArray.OfDouble(new[] { 1.0, 2.0, 3.0 });
        var v = OpcVariant.FromSafeArray(arr);
        await Assert.That(v.AsSafeArray()).IsEqualTo(arr);
    }

    [Test]
    public async Task AsSafeArray_OnNonArrayVariant_IsNull()
    {
        var v = OpcVariant.FromInt32(42);
        await Assert.That(v.AsSafeArray()).IsNull();
    }

    [Test]
    public async Task FromSafeArray_RejectsNull()
    {
        bool threw = false;
        try
        {
            _ = OpcVariant.FromSafeArray(null!);
        }
        catch (ArgumentNullException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    // ---- ICallChannel contract (test double demonstrates the API surface) ----

    private sealed class TestCallChannel : ICallChannel
    {
        public Guid? LastInterfaceId { get; private set; }
        public int LastOpnum { get; private set; }
        public byte[] LastRequest { get; private set; } = Array.Empty<byte>();
        public NdrCallResult Result { get; set; } = new(0, ReadOnlyMemory<byte>.Empty);

        public Task<NdrCallResult> InvokeAsync(
            Guid interfaceId,
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            System.Threading.CancellationToken cancellationToken = default)
        {
            LastInterfaceId = interfaceId;
            LastOpnum = opnum;
            LastRequest = requestPayload.ToArray();
            return Task.FromResult(Result);
        }
    }

    [Test]
    public async Task ICallChannel_DemoUsage_CapturesAndReturns()
    {
        var channel = new TestCallChannel
        {
            Result = new NdrCallResult(0, new byte[] { 0x42 }),
        };
        var iid = new Guid("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        var result = await channel.InvokeAsync(iid, 3, new byte[] { 0x01 });

        await Assert.That(channel.LastInterfaceId).IsEqualTo(iid);
        await Assert.That(channel.LastOpnum).IsEqualTo(3);
        await Assert.That(channel.LastRequest.Length).IsEqualTo(1);
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.ResponsePayload.Span[0]).IsEqualTo((byte)0x42);
    }
}
