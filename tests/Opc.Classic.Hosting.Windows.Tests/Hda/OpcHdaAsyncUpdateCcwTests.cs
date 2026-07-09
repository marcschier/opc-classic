// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Raw CCW tests assert HRESULT constants.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcHdaAsyncUpdateCcwTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_FAIL = unchecked((int)0x80004005);
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task QueryCapabilities_returns_update_capability_mask()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        IntPtr asyncUpdate = Native.InvokeQI(ccw, IOPCHDA_AsyncUpdate.InterfaceId);
        using Native.CoTaskMemBlock capabilities = Native.CoTaskMemBlock.Allocate(sizeof(int));

        int hr = Native.GetMethod<Native.QueryCapabilitiesDelegate>(asyncUpdate, 3)(asyncUpdate, capabilities.Pointer);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(Marshal.ReadInt32(capabilities.Pointer)).IsEqualTo(0x1F);
    }

    [Test]
    [Arguments(4)]
    [Arguments(5)]
    [Arguments(6)]
    public async Task Async_insert_replace_and_insert_replace_return_cancel_ids_and_fire_update_callback(int slot)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        _ = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncUpdate = Native.InvokeQI(ccw, IOPCHDA_AsyncUpdate.InterfaceId);
        using Native.IntArray handles = Native.IntArray.From([10, 11]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From([DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(1)]);
        using Native.DoubleVariantArray values = Native.DoubleVariantArray.From([1.5, 2.5]);
        using Native.IntArray qualities = Native.IntArray.From([192, 192]);
        using Native.CoTaskMemBlock cancel = Native.CoTaskMemBlock.Allocate(sizeof(int));

        Native.AsyncUpdateDelegate method = Native.GetMethod<Native.AsyncUpdateDelegate>(asyncUpdate, slot);
        int hr = method(asyncUpdate, 123, 2, handles.Pointer, timestamps.Pointer, values.Pointer, qualities.Pointer, cancel.Pointer, out IntPtr errorsPtr);
        int[] errors = Native.ReadAndFreeErrors(errorsPtr, 2);
        uint cancelId = unchecked((uint)Marshal.ReadInt32(cancel.Pointer));
        SpinWait.SpinUntil(() => callback.UpdateCount >= 1, TimeSpan.FromSeconds(5));

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(cancelId).IsNotEqualTo(0u);
        await Assert.That(errors[0]).IsEqualTo(slot == 4 ? OpcHdaErrors.OPCHDA_S_INSERTED : slot == 5 ? OpcHdaErrors.OPCHDA_S_REPLACED : S_OK);
        await Assert.That(callback.UpdateCount).IsEqualTo(1);
        await Assert.That(callback.LastTransactionId).IsEqualTo(123u);
    }

    [Test]
    public async Task Async_delete_methods_return_errors_and_callbacks()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        _ = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncUpdate = Native.InvokeQI(ccw, IOPCHDA_AsyncUpdate.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow.AddHours(-1));
        using Native.TimeBlock end = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([10, 404]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From([DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(1)]);
        using Native.CoTaskMemBlock cancelRaw = Native.CoTaskMemBlock.Allocate(sizeof(int));
        using Native.CoTaskMemBlock cancelAtTime = Native.CoTaskMemBlock.Allocate(sizeof(int));

        Native.AsyncDeleteRawDelegate raw = Native.GetMethod<Native.AsyncDeleteRawDelegate>(asyncUpdate, 7);
        Native.AsyncDeleteAtTimeDelegate atTime = Native.GetMethod<Native.AsyncDeleteAtTimeDelegate>(asyncUpdate, 8);
        int rawHr = raw(asyncUpdate, 201, start.Pointer, end.Pointer, 2, handles.Pointer, cancelRaw.Pointer, out IntPtr rawErrorsPtr);
        int atTimeHr = atTime(asyncUpdate, 202, 2, handles.Pointer, timestamps.Pointer, cancelAtTime.Pointer, out IntPtr atTimeErrorsPtr);
        int[] rawErrors = Native.ReadAndFreeErrors(rawErrorsPtr, 2);
        int[] atTimeErrors = Native.ReadAndFreeErrors(atTimeErrorsPtr, 2);
        SpinWait.SpinUntil(() => callback.UpdateCount >= 2, TimeSpan.FromSeconds(5));

        await Assert.That(rawHr).IsEqualTo(S_FALSE);
        await Assert.That(atTimeHr).IsEqualTo(S_FALSE);
        await Assert.That(rawErrors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
        await Assert.That(atTimeErrors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
        await Assert.That(callback.UpdateCount).IsEqualTo(2);
    }

    [Test]
    [Arguments(4)]
    [Arguments(5)]
    [Arguments(6)]
    public async Task Async_write_methods_reject_count_zero(int slot)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        IntPtr asyncUpdate = Native.InvokeQI(ccw, IOPCHDA_AsyncUpdate.InterfaceId);
        using Native.CoTaskMemBlock cancel = Native.CoTaskMemBlock.Allocate(sizeof(int));
        Native.AsyncUpdateDelegate method = Native.GetMethod<Native.AsyncUpdateDelegate>(asyncUpdate, slot);

        int hr = method(asyncUpdate, 123, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, cancel.Pointer, out IntPtr errorsPtr);

        await Assert.That(hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(errorsPtr).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Cancel_returns_cancel_complete_for_pending_update_and_E_FAIL_for_unknown_cancel_id()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        _ = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncUpdate = Native.InvokeQI(ccw, IOPCHDA_AsyncUpdate.InterfaceId);
        using Native.IntArray handles = Native.IntArray.From([10]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From([DateTimeOffset.UtcNow]);
        using Native.DoubleVariantArray values = Native.DoubleVariantArray.From([1.5]);
        using Native.IntArray qualities = Native.IntArray.From([192]);
        using Native.CoTaskMemBlock cancel = Native.CoTaskMemBlock.Allocate(sizeof(int));
        Native.AsyncUpdateDelegate insert = Native.GetMethod<Native.AsyncUpdateDelegate>(asyncUpdate, 4);
        Native.CancelDelegate cancelMethod = Native.GetMethod<Native.CancelDelegate>(asyncUpdate, 9);

        int beginHr = insert(asyncUpdate, 321, 1, handles.Pointer, timestamps.Pointer, values.Pointer, qualities.Pointer, cancel.Pointer, out IntPtr errorsPtr);
        Native.ReadAndFreeErrors(errorsPtr, 1);
        uint cancelId = unchecked((uint)Marshal.ReadInt32(cancel.Pointer));
        int cancelHr = cancelMethod(asyncUpdate, cancelId);
        SpinWait.SpinUntil(() => callback.CancelId == cancelId, TimeSpan.FromSeconds(5));
        int unknownHr = cancelMethod(asyncUpdate, 99999);

        await Assert.That(beginHr).IsEqualTo(S_OK);
        await Assert.That(cancelHr).IsEqualTo(S_OK);
        await Assert.That(callback.CancelId).IsEqualTo(cancelId);
        await Assert.That(unknownHr).IsEqualTo(E_FAIL);
    }

    private sealed class UpdateServer : IOpcHdaServer, IOPCHDA_SyncUpdate
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds.Length]);

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0x1F);
        public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default) => Task.FromResult(Errors(serverHandles, OpcHdaErrors.OPCHDA_S_INSERTED));
        public Task<int[]> ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default) => Task.FromResult(Errors(serverHandles, OpcHdaErrors.OPCHDA_S_REPLACED));
        public Task<int[]> InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default) => Task.FromResult(Errors(serverHandles, S_OK));
        public Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default) { _ = startTime; _ = endTime; return Task.FromResult(Errors(serverHandles, S_OK)); }
        public Task<int[]> DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default) { _ = timestampFileTimes; return Task.FromResult(Errors(serverHandles, S_OK)); }

        private static int[] Errors(int[] handles, int success)
        {
            var errors = new int[handles.Length];
            for (int i = 0; i < handles.Length; i++)
            {
                errors[i] = handles[i] == 404 ? OpcResultId.InvalidHandle.Code : success;
            }
            return errors;
        }
    }

    private static class Native
    {
        private static int HdaTimeSize => IntPtr.Size == 8 ? 24 : 16;
        private static int HdaTimeFileTimeOffset => IntPtr.Size == 8 ? 16 : 8;
        private static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static uint Advise(IntPtr ccw, IntPtr callback)
        {
            IntPtr cpc = InvokeQI(ccw, OpcGuids.IID_IConnectionPointContainer);
            FindConnectionPointDelegate find = GetMethod<FindConnectionPointDelegate>(cpc, 4);
            Guid iid = IOPCHDA_DataCallback.InterfaceId;
            int hr = find(cpc, ref iid, out IntPtr cp);
            if (hr != S_OK)
            {
                return 0;
            }
            AdviseDelegate advise = GetMethod<AdviseDelegate>(cp, 5);
            hr = advise(cp, callback, out uint cookie);
            return hr == S_OK ? cookie : 0;
        }

        internal static T GetMethod<T>(IntPtr tearoff, int slot) where T : Delegate
        {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        internal static int[] ReadAndFreeErrors(IntPtr ptr, int count)
        {
            var values = new int[count];
            if (ptr != IntPtr.Zero && count > 0)
            {
                Marshal.Copy(ptr, values, 0, count);
                Marshal.FreeCoTaskMem(ptr);
            }
            return values;
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int QueryCapabilitiesDelegate(IntPtr pThis, IntPtr capabilities);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int AsyncUpdateDelegate(IntPtr pThis, uint transactionId, uint count, IntPtr handles, IntPtr timestamps, IntPtr values, IntPtr qualities, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int AsyncDeleteRawDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, uint count, IntPtr handles, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int AsyncDeleteAtTimeDelegate(IntPtr pThis, uint transactionId, uint count, IntPtr handles, IntPtr timestamps, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int CancelDelegate(IntPtr pThis, uint cancelId);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int FindConnectionPointDelegate(IntPtr pThis, ref Guid iid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AdviseDelegate(IntPtr pThis, IntPtr callback, out uint cookie);

        internal sealed class IntArray : IDisposable { private IntArray(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static IntArray From(int[] values) { IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int)); Marshal.Copy(values, 0, ptr, values.Length); return new IntArray(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
        internal sealed class FileTimeArray : IDisposable { private FileTimeArray(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static FileTimeArray From(DateTimeOffset[] values) { IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(long)); for (int i = 0; i < values.Length; i++) { Marshal.WriteInt64(ptr, i * sizeof(long), values[i].ToFileTime()); } return new FileTimeArray(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
        internal sealed class DoubleVariantArray : IDisposable { private DoubleVariantArray(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static DoubleVariantArray From(double[] values) { IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * VariantSize); for (int i = 0; i < values.Length; i++) { IntPtr slot = IntPtr.Add(ptr, i * VariantSize); Marshal.WriteInt16(slot, 0, 5); Marshal.WriteInt64(slot, 8, BitConverter.DoubleToInt64Bits(values[i])); } return new DoubleVariantArray(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
        internal sealed class TimeBlock : IDisposable { private TimeBlock(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static TimeBlock From(DateTimeOffset value) { IntPtr ptr = Marshal.AllocCoTaskMem(HdaTimeSize); Marshal.WriteInt32(ptr, 0); Marshal.WriteIntPtr(ptr, IntPtr.Size == 8 ? 8 : 4, IntPtr.Zero); Marshal.WriteInt64(ptr, HdaTimeFileTimeOffset, value.ToFileTime()); return new TimeBlock(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
        internal sealed class CoTaskMemBlock : IDisposable { private CoTaskMemBlock(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static CoTaskMemBlock Allocate(int byteCount) { IntPtr ptr = Marshal.AllocCoTaskMem(byteCount); for (int i = 0; i < byteCount; i++) { Marshal.WriteByte(ptr, i, 0); } return new CoTaskMemBlock(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
    }

    private sealed class CallbackCcw : IDisposable
    {
        private static readonly ConcurrentDictionary<IntPtr, CallbackCcw> s_instances = new();
        private static readonly QueryInterfaceCallback s_queryInterface = QueryInterface;
        private static readonly RefCountCallback s_addRef = AddRef;
        private static readonly RefCountCallback s_release = Release;
        private static readonly UpdateCallback s_onUpdateComplete = OnUpdateComplete;
        private static readonly CancelCallback s_onCancelComplete = OnCancelComplete;
        private static readonly HdaItemsCallback s_ignoredItems = OnIgnoredItems;
        private static readonly AttributeCallback s_ignoredAttribute = OnIgnoredAttribute;
        private readonly IntPtr _vtable;
        private int _updateCount;
        private int _cancelId;
        private int _transactionId;

        public CallbackCcw()
        {
            _vtable = Marshal.AllocCoTaskMem(12 * IntPtr.Size);
            WriteSlot(0, s_queryInterface);
            WriteSlot(1, s_addRef);
            WriteSlot(2, s_release);
            WriteSlot(3, s_ignoredItems);
            WriteSlot(4, s_ignoredItems);
            WriteSlot(5, s_ignoredItems);
            WriteSlot(6, s_ignoredAttribute);
            WriteSlot(7, s_ignoredItems);
            WriteSlot(8, s_ignoredItems);
            WriteSlot(9, s_ignoredItems);
            WriteSlot(10, s_onUpdateComplete);
            WriteSlot(11, s_onCancelComplete);
            Pointer = Marshal.AllocCoTaskMem(IntPtr.Size);
            Marshal.WriteIntPtr(Pointer, _vtable);
            s_instances[Pointer] = this;
        }

        public IntPtr Pointer { get; }
        public int UpdateCount => Volatile.Read(ref _updateCount);
        public uint CancelId => unchecked((uint)Volatile.Read(ref _cancelId));
        public uint LastTransactionId => unchecked((uint)Volatile.Read(ref _transactionId));

        public void Dispose()
        {
            s_instances.TryRemove(Pointer, out _);
            Marshal.FreeCoTaskMem(Pointer);
            Marshal.FreeCoTaskMem(_vtable);
        }

        private void WriteSlot(int slot, Delegate method) => Marshal.WriteIntPtr(_vtable, slot * IntPtr.Size, Marshal.GetFunctionPointerForDelegate(method));
        private static int QueryInterface(IntPtr pThis, ref Guid riid, out IntPtr ppv) { if (riid == IID_IUnknown || riid == IOPCHDA_DataCallback.InterfaceId) { ppv = pThis; return S_OK; } ppv = IntPtr.Zero; return unchecked((int)0x80004002); }
        private static uint AddRef(IntPtr pThis) { _ = pThis; return 2; }
        private static uint Release(IntPtr pThis) { _ = pThis; return 1; }
        private static int OnUpdateComplete(IntPtr pThis, uint transactionId, int status, uint count, IntPtr handles, IntPtr errors) { _ = status; _ = count; _ = handles; _ = errors; Volatile.Write(ref s_instances[pThis]._transactionId, unchecked((int)transactionId)); Interlocked.Increment(ref s_instances[pThis]._updateCount); return S_OK; }
        private static int OnCancelComplete(IntPtr pThis, uint cancelId) { Volatile.Write(ref s_instances[pThis]._cancelId, unchecked((int)cancelId)); return S_OK; }
        private static int OnIgnoredItems(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) { _ = pThis; _ = transactionId; _ = status; _ = count; _ = values; _ = errors; return S_OK; }
        private static int OnIgnoredAttribute(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors) { _ = pThis; _ = transactionId; _ = status; _ = clientHandle; _ = count; _ = values; _ = errors; return S_OK; }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int QueryInterfaceCallback(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate uint RefCountCallback(IntPtr pThis);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int HdaItemsCallback(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AttributeCallback(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int UpdateCallback(IntPtr pThis, uint transactionId, int status, uint count, IntPtr handles, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int CancelCallback(IntPtr pThis, uint cancelId);
    }
}
