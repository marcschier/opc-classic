// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>
/// Windows-only unit tests for <see cref="OpcDataCallbackProxy"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcDataCallbackProxyTests
{
    [Test]
    public async Task Construction_succeeds_when_client_supports_IOPCDataCallback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);

            await Assert.That(proxy).IsNotNull();
            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(2L);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task Construction_throws_when_client_lacks_IOPCDataCallback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: false);
        try
        {
            await Assert.That(() =>
            {
                using var proxy = new OpcDataCallbackProxy(stub);
            }).Throws<COMException>();
            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(1L);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnCancelComplete_invokes_opnum_6_with_payload_values()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnCancelComplete(new OpcDaGroup.CancelCompletePayload(123, 456));

            Helpers.Invocation invocation = Helpers.GetLastInvocation(stub);
            await Assert.That(invocation.Opnum).IsEqualTo(6);
            await Assert.That(invocation.TransactionId).IsEqualTo(123U);
            await Assert.That(invocation.GroupHandle).IsEqualTo(456U);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnDataChange_writes_correct_handle_count_to_stub_vtable()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnDataChange(new OpcDaGroup.DataChangePayload(
                10,
                20,
                30,
                40,
                [101, 102],
                [OpcVariant.FromInt32(42), OpcVariant.FromString("hello")],
                [192, 216],
                [123456789L, 987654321L],
                [0, unchecked((int)0x80004005)]));

            Helpers.DataCallbackInvocation invocation = Helpers.GetLastDataInvocation(stub);
            await Assert.That(invocation.Opnum).IsEqualTo(3);
            await Assert.That(invocation.Count).IsEqualTo(2U);
            await Assert.That(invocation.ClientHandles).IsEquivalentTo(new[] { 101, 102 });
            await Assert.That(invocation.Values[0]).IsEqualTo(OpcVariant.FromInt32(42));
            await Assert.That(invocation.Values[1]).IsEqualTo(OpcVariant.FromString("hello"));
            await Assert.That(invocation.Qualities).IsEquivalentTo(new ushort[] { 192, 216 });
            await Assert.That(invocation.Timestamps).IsEquivalentTo(new[] { 123456789L, 987654321L });
            await Assert.That(invocation.Errors).IsEquivalentTo(new[] { 0, unchecked((int)0x80004005) });
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnDataChange_with_VT_I4_value_passes_through()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnDataChange(CreateSingleValuePayload(OpcVariant.FromInt32(42)));

            OpcVariant actual = Helpers.GetLastDataInvocation(stub).Values[0];
            await Assert.That(actual.Type).IsEqualTo(VarType.VT_I4);
            await Assert.That(actual.AsInt32()).IsEqualTo(42);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnDataChange_with_VT_BSTR_value_passes_through()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnDataChange(CreateSingleValuePayload(OpcVariant.FromString("hello")));

            OpcVariant actual = Helpers.GetLastDataInvocation(stub).Values[0];
            await Assert.That(actual.Type).IsEqualTo(VarType.VT_BSTR);
            await Assert.That(actual.AsString()).IsEqualTo("hello");
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnDataChange_with_empty_payload_count_zero()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnDataChange(new OpcDaGroup.DataChangePayload(
                1,
                2,
                3,
                4,
                [],
                [],
                [],
                [],
                []));

            Helpers.DataCallbackInvocation invocation = Helpers.GetLastDataInvocation(stub);
            await Assert.That(invocation.Count).IsEqualTo(0U);
            await Assert.That(invocation.ClientItemsWasNull).IsTrue();
            await Assert.That(invocation.ValuesWasNull).IsTrue();
            await Assert.That(invocation.ErrorsWasNull).IsTrue();
            await Assert.That(invocation.Values).IsEquivalentTo(Array.Empty<OpcVariant>());
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnReadComplete_invokes_vtable_slot_4()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnReadComplete(CreateSingleValuePayload(OpcVariant.FromInt32(7)));

            Helpers.DataCallbackInvocation invocation = Helpers.GetLastDataInvocation(stub);
            await Assert.That(invocation.Opnum).IsEqualTo(4);
            await Assert.That(invocation.Values[0]).IsEqualTo(OpcVariant.FromInt32(7));
            await Assert.That(invocation.ClientHandles).IsEquivalentTo(new[] { 1001 });
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnWriteComplete_invokes_vtable_slot_5_with_handles_and_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            int[] errors = [0, unchecked((int)0x80070005)];
            proxy.OnWriteComplete(11, 22, unchecked((int)0x80004005), [301, 302], errors);

            Helpers.WriteCallbackInvocation invocation = Helpers.GetLastWriteInvocation(stub);
            await Assert.That(invocation.Opnum).IsEqualTo(5);
            await Assert.That(invocation.TransactionId).IsEqualTo(11U);
            await Assert.That(invocation.GroupHandle).IsEqualTo(22U);
            await Assert.That(invocation.MasterError).IsEqualTo(unchecked((int)0x80004005));
            await Assert.That(invocation.ClientHandles).IsEquivalentTo(new[] { 301, 302 });
            await Assert.That(invocation.Errors).IsEquivalentTo(errors);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnDataChange_frees_all_CoTaskMem_allocations_after_callback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnDataChange(CreateSingleValuePayload(OpcVariant.FromString("pinned")));

            Helpers.DataCallbackInvocation invocation = Helpers.GetLastDataInvocation(stub);
            await Assert.That(invocation.Values[0].AsString()).IsEqualTo("pinned");
            await Assert.That(Helpers.GetObservedBstrCount(stub)).IsEqualTo(1L);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    private static OpcDaGroup.DataChangePayload CreateSingleValuePayload(OpcVariant value) =>
        new(
            1,
            2,
            3,
            4,
            [1001],
            [value],
            [192],
            [DateTimeOffset.UnixEpoch.ToFileTime()],
            [0]);

    [Test]
    public async Task Dispose_releases_held_IOPCDataCallback_pointer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            var proxy = new OpcDataCallbackProxy(stub);
            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(2L);

            proxy.Dispose();

            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(1L);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    private static unsafe class Helpers
    {
        private const int S_OK = 0;
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int E_NOINTERFACE = unchecked((int)0x80004002);
        private const int E_POINTER = unchecked((int)0x80004003);
        private const int VariantValueOffset = 8;
        private const int VtableSlotCount = 7;
        private const ushort VtBstr = 8;

        private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
        private static readonly Guid s_iidDataCallback = IOPCDataCallback.InterfaceId;
        private static readonly ConcurrentDictionary<IntPtr, StubSession> s_sessions = new();

        internal readonly record struct Invocation(int Opnum, uint TransactionId, uint GroupHandle);

        internal sealed record DataCallbackInvocation(
            int Opnum,
            uint TransactionId,
            uint GroupHandle,
            int MasterQuality,
            int MasterError,
            uint Count,
            int[] ClientHandles,
            OpcVariant[] Values,
            ushort[] Qualities,
            long[] Timestamps,
            int[] Errors,
            bool ClientItemsWasNull,
            bool ValuesWasNull,
            bool QualitiesWasNull,
            bool TimestampsWasNull,
            bool ErrorsWasNull)
        {
            internal static DataCallbackInvocation Empty { get; } = new(
                0,
                0,
                0,
                0,
                0,
                0,
                Array.Empty<int>(),
                Array.Empty<OpcVariant>(),
                Array.Empty<ushort>(),
                Array.Empty<long>(),
                Array.Empty<int>(),
                true,
                true,
                true,
                true,
                true);
        }

        internal sealed record WriteCallbackInvocation(
            int Opnum,
            uint TransactionId,
            uint GroupHandle,
            int MasterError,
            uint Count,
            int[] ClientHandles,
            int[] Errors,
            bool ClientItemsWasNull,
            bool ErrorsWasNull)
        {
            internal static WriteCallbackInvocation Empty { get; } = new(
                0,
                0,
                0,
                0,
                0,
                Array.Empty<int>(),
                Array.Empty<int>(),
                true,
                true);
        }

        internal static IntPtr CreateStub(bool supportsDataCallback)
        {
            IntPtr* vtable = AllocateVtable();
            IntPtr instance = AllocateInstance(vtable);
            s_sessions[instance] = new StubSession(vtable, supportsDataCallback);
            return instance;
        }

        internal static void DestroyStub(IntPtr stub)
        {
            if (!s_sessions.TryRemove(stub, out StubSession? session))
            {
                return;
            }

            NativeMemory.Free((void*)stub);
            NativeMemory.Free(session.Vtable);
        }

        internal static long GetReferenceCount(IntPtr stub) =>
            s_sessions.TryGetValue(stub, out StubSession? session)
                ? Interlocked.Read(ref session.RefCount)
                : -1L;

        internal static Invocation GetLastInvocation(IntPtr stub) =>
            s_sessions.TryGetValue(stub, out StubSession? session)
                ? session.LastInvocation
                : default;

        internal static DataCallbackInvocation GetLastDataInvocation(IntPtr stub) =>
            s_sessions.TryGetValue(stub, out StubSession? session)
                ? session.LastDataInvocation
                : DataCallbackInvocation.Empty;

        internal static WriteCallbackInvocation GetLastWriteInvocation(IntPtr stub) =>
            s_sessions.TryGetValue(stub, out StubSession? session)
                ? session.LastWriteInvocation
                : WriteCallbackInvocation.Empty;

        internal static long GetObservedBstrCount(IntPtr stub) =>
            s_sessions.TryGetValue(stub, out StubSession? session)
                ? Interlocked.Read(ref session.ObservedBstrCount)
                : 0L;

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr* AllocateVtable()
        {
            IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(VtableSlotCount * sizeof(IntPtr)));
            vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
            vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
            vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
            vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OnDataChange;
            vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OnReadComplete;
            vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, uint, IntPtr, IntPtr, int>)&OnWriteComplete;
            vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int>)&OnCancelComplete;
            return vtable;
        }

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr AllocateInstance(IntPtr* vtable)
        {
            IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
            instance[0] = (IntPtr)vtable;
            return (IntPtr)instance;
        }

        [UnmanagedCallersOnly]
        private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
        {
            if (ppv == null)
            {
                return E_POINTER;
            }
            if (!s_sessions.TryGetValue(pThis, out StubSession? session) || riid == null)
            {
                *ppv = IntPtr.Zero;
                return E_NOINTERFACE;
            }

            if (*riid == s_iidUnknown || (session.SupportsDataCallback && *riid == s_iidDataCallback))
            {
                *ppv = pThis;
                Interlocked.Increment(ref session.RefCount);
                return S_OK;
            }

            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
        }

        [UnmanagedCallersOnly]
        private static uint AddRef(IntPtr pThis)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return 1;
            }
            return (uint)Interlocked.Increment(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        private static uint Release(IntPtr pThis)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return 0;
            }
            return (uint)Interlocked.Decrement(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        [SuppressMessage("Design", "CA1031", Justification = "Cross-unmanaged-boundary catch.")]
        private static int OnDataChange(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterQuality,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr values,
            IntPtr qualities,
            IntPtr timestamps,
            IntPtr errors)
        {
            try
            {
                return RecordDataChange(
                    pThis,
                    3,
                    transactionId,
                    groupHandle,
                    masterQuality,
                    masterError,
                    count,
                    clientItems,
                    values,
                    qualities,
                    timestamps,
                    errors);
            }
            catch
            {
                return E_FAIL;
            }
        }

        [UnmanagedCallersOnly]
        [SuppressMessage("Design", "CA1031", Justification = "Cross-unmanaged-boundary catch.")]
        private static int OnReadComplete(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterQuality,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr values,
            IntPtr qualities,
            IntPtr timestamps,
            IntPtr errors)
        {
            try
            {
                return RecordDataChange(
                    pThis,
                    4,
                    transactionId,
                    groupHandle,
                    masterQuality,
                    masterError,
                    count,
                    clientItems,
                    values,
                    qualities,
                    timestamps,
                    errors);
            }
            catch
            {
                return E_FAIL;
            }
        }

        [UnmanagedCallersOnly]
        [SuppressMessage("Design", "CA1031", Justification = "Cross-unmanaged-boundary catch.")]
        private static int OnWriteComplete(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr errors)
        {
            try
            {
                return RecordWriteComplete(
                    pThis,
                    transactionId,
                    groupHandle,
                    masterError,
                    count,
                    clientItems,
                    errors);
            }
            catch
            {
                return E_FAIL;
            }
        }

        [UnmanagedCallersOnly]
        private static int OnCancelComplete(IntPtr pThis, uint transactionId, uint groupHandle) =>
            Record(pThis, 6, transactionId, groupHandle);

        private static int RecordDataChange(
            IntPtr pThis,
            int opnum,
            uint transactionId,
            uint groupHandle,
            int masterQuality,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr values,
            IntPtr qualities,
            IntPtr timestamps,
            IntPtr errors)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return E_NOINTERFACE;
            }
            int itemCount = checked((int)count);
            session.LastInvocation = new Invocation(opnum, transactionId, groupHandle);
            session.LastDataInvocation = new DataCallbackInvocation(
                opnum,
                transactionId,
                groupHandle,
                masterQuality,
                masterError,
                count,
                ReadInt32Array(clientItems, itemCount),
                ReadVariantArray(values, itemCount, session),
                ReadUInt16Array(qualities, itemCount),
                ReadInt64Array(timestamps, itemCount),
                ReadInt32Array(errors, itemCount),
                clientItems == IntPtr.Zero,
                values == IntPtr.Zero,
                qualities == IntPtr.Zero,
                timestamps == IntPtr.Zero,
                errors == IntPtr.Zero);
            return S_OK;
        }

        private static int RecordWriteComplete(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr errors)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return E_NOINTERFACE;
            }
            int itemCount = checked((int)count);
            session.LastInvocation = new Invocation(5, transactionId, groupHandle);
            session.LastWriteInvocation = new WriteCallbackInvocation(
                5,
                transactionId,
                groupHandle,
                masterError,
                count,
                ReadInt32Array(clientItems, itemCount),
                ReadInt32Array(errors, itemCount),
                clientItems == IntPtr.Zero,
                errors == IntPtr.Zero);
            return S_OK;
        }

        private static int Record(IntPtr pThis, int opnum, uint transactionId, uint groupHandle)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return E_NOINTERFACE;
            }
            session.LastInvocation = new Invocation(opnum, transactionId, groupHandle);
            return S_OK;
        }

        private static int[] ReadInt32Array(IntPtr ptr, int count)
        {
            EnsureNativeArrayPointer(ptr, count);
            var values = new int[count];
            if (count > 0)
            {
                Marshal.Copy(ptr, values, 0, count);
            }
            return values;
        }

        private static ushort[] ReadUInt16Array(IntPtr ptr, int count)
        {
            EnsureNativeArrayPointer(ptr, count);
            var values = new ushort[count];
            ushort* source = (ushort*)ptr;
            for (int i = 0; i < count; i++)
            {
                values[i] = source[i];
            }
            return values;
        }

        private static long[] ReadInt64Array(IntPtr ptr, int count)
        {
            EnsureNativeArrayPointer(ptr, count);
            var values = new long[count];
            if (count > 0)
            {
                Marshal.Copy(ptr, values, 0, count);
            }
            return values;
        }

        private static OpcVariant[] ReadVariantArray(IntPtr ptr, int count, StubSession session)
        {
            EnsureNativeArrayPointer(ptr, count);
            var values = new OpcVariant[count];
            int variantSize = ComVariantMarshaler.VariantSize;
            for (int i = 0; i < count; i++)
            {
                IntPtr slot = ptr + (i * variantSize);
                ObserveBstrIfPresent(slot, session);
                values[i] = ComVariantMarshaler.ReadVariant(slot);
            }
            return values;
        }

        private static void ObserveBstrIfPresent(IntPtr variantPtr, StubSession session)
        {
            if (unchecked((ushort)Marshal.ReadInt16(variantPtr)) != VtBstr)
            {
                return;
            }
            IntPtr bstr = Marshal.ReadIntPtr(variantPtr, VariantValueOffset);
            if (bstr != IntPtr.Zero)
            {
                Interlocked.Increment(ref session.ObservedBstrCount);
            }
        }

        private static void EnsureNativeArrayPointer(IntPtr ptr, int count)
        {
            if (count > 0 && ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Native array pointer is null for a non-empty payload.");
            }
        }

        private sealed class StubSession(IntPtr* vtable, bool supportsDataCallback)
        {
            internal readonly IntPtr* Vtable = vtable;
            internal readonly bool SupportsDataCallback = supportsDataCallback;
            internal long ObservedBstrCount;
            internal long RefCount = 1;
            internal Invocation LastInvocation;
            internal DataCallbackInvocation LastDataInvocation = DataCallbackInvocation.Empty;
            internal WriteCallbackInvocation LastWriteInvocation = WriteCallbackInvocation.Empty;
        }
    }
}
