// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Raw CCW tests assert HRESULT constants.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcHdaPlaybackCcwTests
{
    private const int S_OK = 0;
    private const int E_FAIL = unchecked((int)0x80004005);
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task ReadRawWithUpdate_streams_playback_callbacks()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new PlaybackServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        _ = Native.Advise(ccw, callback.Pointer);
        IntPtr playback = Native.InvokeQI(ccw, IOPCHDA_Playback.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow.AddMinutes(-1));
        using Native.TimeBlock end = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([10, 11]);
        using Native.CoTaskMemBlock cancel = Native.CoTaskMemBlock.Allocate(sizeof(int));

        Native.PlaybackRawDelegate raw = Native.GetMethod<Native.PlaybackRawDelegate>(playback, 3);
        int hr = raw(playback, 401, start.Pointer, end.Pointer, 2, TimeSpan.FromMilliseconds(10).Ticks, TimeSpan.FromMilliseconds(5).Ticks, 2, handles.Pointer, cancel.Pointer, out IntPtr errorsPtr);
        int[] errors = Native.ReadAndFreeErrors(errorsPtr, 2);
        SpinWait.SpinUntil(() => callback.PlaybackCount >= 2, TimeSpan.FromSeconds(5));

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(errors[0]).IsEqualTo(S_OK);
        await Assert.That(callback.PlaybackCount).IsGreaterThanOrEqualTo(2);
        await Assert.That(callback.LastTransactionId).IsEqualTo(401u);
    }

    [Test]
    public async Task ReadProcessedWithUpdate_streams_playback_callbacks()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new PlaybackServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        _ = Native.Advise(ccw, callback.Pointer);
        IntPtr playback = Native.InvokeQI(ccw, IOPCHDA_Playback.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow.AddMinutes(-1));
        using Native.TimeBlock end = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([10, 11]);
        using Native.IntArray aggregates = Native.IntArray.From([1, 4]);
        using Native.CoTaskMemBlock cancel = Native.CoTaskMemBlock.Allocate(sizeof(int));

        Native.PlaybackProcessedDelegate processed = Native.GetMethod<Native.PlaybackProcessedDelegate>(playback, 4);
        int hr = processed(playback, 402, start.Pointer, end.Pointer, TimeSpan.FromSeconds(1).Ticks, 2, TimeSpan.FromMilliseconds(5).Ticks, 2, handles.Pointer, aggregates.Pointer, cancel.Pointer, out IntPtr errorsPtr);
        int[] errors = Native.ReadAndFreeErrors(errorsPtr, 2);
        SpinWait.SpinUntil(() => callback.PlaybackCount >= 2, TimeSpan.FromSeconds(5));

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(errors[1]).IsEqualTo(S_OK);
        await Assert.That(callback.PlaybackCount).IsGreaterThanOrEqualTo(2);
        await Assert.That(callback.LastTransactionId).IsEqualTo(402u);
    }

    [Test]
    public async Task Cancel_stops_mid_stream_and_fires_cancel_complete()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new PlaybackServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        _ = Native.Advise(ccw, callback.Pointer);
        IntPtr playback = Native.InvokeQI(ccw, IOPCHDA_Playback.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow.AddMinutes(-1));
        using Native.TimeBlock end = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([10]);
        using Native.CoTaskMemBlock cancel = Native.CoTaskMemBlock.Allocate(sizeof(int));
        Native.PlaybackRawDelegate raw = Native.GetMethod<Native.PlaybackRawDelegate>(playback, 3);
        Native.CancelDelegate cancelMethod = Native.GetMethod<Native.CancelDelegate>(playback, 5);

        int beginHr = raw(playback, 403, start.Pointer, end.Pointer, 1, TimeSpan.FromSeconds(1).Ticks, TimeSpan.FromMilliseconds(50).Ticks, 1, handles.Pointer, cancel.Pointer, out IntPtr errorsPtr);
        Native.ReadAndFreeErrors(errorsPtr, 1);
        uint cancelId = unchecked((uint)Marshal.ReadInt32(cancel.Pointer));
        SpinWait.SpinUntil(() => callback.PlaybackCount >= 1, TimeSpan.FromSeconds(5));
        int cancelHr = cancelMethod(playback, cancelId);
        SpinWait.SpinUntil(() => callback.CancelId == cancelId, TimeSpan.FromSeconds(5));

        await Assert.That(beginHr).IsEqualTo(S_OK);
        await Assert.That(cancelHr).IsEqualTo(S_OK);
        await Assert.That(callback.CancelId).IsEqualTo(cancelId);
    }

    [Test]
    public async Task Playback_methods_reject_count_zero_and_unknown_cancel_id()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new PlaybackServer(), IID_IUnknown);
        IntPtr playback = Native.InvokeQI(ccw, IOPCHDA_Playback.InterfaceId);
        using Native.TimeBlock time = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.CoTaskMemBlock cancel = Native.CoTaskMemBlock.Allocate(sizeof(int));
        Native.PlaybackRawDelegate raw = Native.GetMethod<Native.PlaybackRawDelegate>(playback, 3);
        Native.PlaybackProcessedDelegate processed = Native.GetMethod<Native.PlaybackProcessedDelegate>(playback, 4);
        Native.CancelDelegate cancelMethod = Native.GetMethod<Native.CancelDelegate>(playback, 5);

        int rawHr = raw(playback, 1, time.Pointer, time.Pointer, 0, 0, 0, 0, IntPtr.Zero, cancel.Pointer, out IntPtr rawErrors);
        int processedHr = processed(playback, 1, time.Pointer, time.Pointer, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, cancel.Pointer, out IntPtr processedErrors);
        int cancelHr = cancelMethod(playback, 123456);

        await Assert.That(rawHr).IsEqualTo(E_INVALIDARG);
        await Assert.That(processedHr).IsEqualTo(E_INVALIDARG);
        await Assert.That(rawErrors).IsEqualTo(IntPtr.Zero);
        await Assert.That(processedErrors).IsEqualTo(IntPtr.Zero);
        await Assert.That(cancelHr).IsEqualTo(E_FAIL);
    }

    private sealed class PlaybackServer : IOpcHdaServer, IOPCHDA_SyncRead
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds.Length]);

        public Task<OpcHdaItem[]> ReadRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            _ = startTime; _ = endTime; _ = maxValues; _ = bounds;
            return Task.FromResult(serverHandles.Select(handle => Item(handle, 0)).ToArray());
        }

        public Task<OpcHdaItem[]> ReadProcessedAsync(OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default)
        {
            _ = startTime; _ = endTime; _ = resampleIntervalFileTime;
            return Task.FromResult(serverHandles.Select((handle, i) => Item(handle, aggregateIds[i])).ToArray());
        }

        public Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<OpcHdaItem>());
        public Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<OpcHdaModifiedItem>());
        public Task<OpcHdaAttribute[]> ReadAttributeAsync(OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<OpcHdaAttribute>());

        private static OpcHdaItem Item(int handle, int aggregate)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return new OpcHdaItem(handle, aggregate, [now], [(uint)OpcQuality.Good.RawValue], [OpcVariant.FromDouble(handle)]);
        }
    }

    private static class Native
    {
        private static int HdaTimeSize => IntPtr.Size == 8 ? 24 : 16;
        private static int HdaTimeFileTimeOffset => IntPtr.Size == 8 ? 16 : 8;

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid) { QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0); int hr = qi(ccw, ref iid, out IntPtr returned); return hr == S_OK ? returned : IntPtr.Zero; }
        internal static uint Advise(IntPtr ccw, IntPtr callback) { IntPtr cpc = InvokeQI(ccw, OpcGuids.IID_IConnectionPointContainer); FindConnectionPointDelegate find = GetMethod<FindConnectionPointDelegate>(cpc, 4); Guid iid = IOPCHDA_DataCallback.InterfaceId; int hr = find(cpc, ref iid, out IntPtr cp); if (hr != S_OK) { return 0; } AdviseDelegate advise = GetMethod<AdviseDelegate>(cp, 5); hr = advise(cp, callback, out uint cookie); return hr == S_OK ? cookie : 0; }
        internal static T GetMethod<T>(IntPtr tearoff, int slot) where T : Delegate { IntPtr vtable = Marshal.ReadIntPtr(tearoff); IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size); return Marshal.GetDelegateForFunctionPointer<T>(method); }
        internal static int[] ReadAndFreeErrors(IntPtr ptr, int count) { var values = new int[count]; if (ptr != IntPtr.Zero && count > 0) { Marshal.Copy(ptr, values, 0, count); Marshal.FreeCoTaskMem(ptr); } return values; }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int PlaybackRawDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, uint numValues, long updateDuration, long updateInterval, uint count, IntPtr handles, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int PlaybackProcessedDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, long resampleInterval, uint intervalCount, long updateInterval, uint count, IntPtr handles, IntPtr aggregates, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int CancelDelegate(IntPtr pThis, uint cancelId);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int FindConnectionPointDelegate(IntPtr pThis, ref Guid iid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AdviseDelegate(IntPtr pThis, IntPtr callback, out uint cookie);

        internal sealed class IntArray : IDisposable { private IntArray(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static IntArray From(int[] values) { IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int)); Marshal.Copy(values, 0, ptr, values.Length); return new IntArray(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
        internal sealed class TimeBlock : IDisposable { private TimeBlock(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static TimeBlock From(DateTimeOffset value) { IntPtr ptr = Marshal.AllocCoTaskMem(HdaTimeSize); Marshal.WriteInt32(ptr, 0); Marshal.WriteIntPtr(ptr, IntPtr.Size == 8 ? 8 : 4, IntPtr.Zero); Marshal.WriteInt64(ptr, HdaTimeFileTimeOffset, value.ToFileTime()); return new TimeBlock(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
        internal sealed class CoTaskMemBlock : IDisposable { private CoTaskMemBlock(IntPtr pointer) => Pointer = pointer; public IntPtr Pointer { get; } public static CoTaskMemBlock Allocate(int byteCount) { IntPtr ptr = Marshal.AllocCoTaskMem(byteCount); for (int i = 0; i < byteCount; i++) { Marshal.WriteByte(ptr, i, 0); } return new CoTaskMemBlock(ptr); } public void Dispose() => Marshal.FreeCoTaskMem(Pointer); }
    }

    private sealed class CallbackCcw : IDisposable
    {
        private static readonly ConcurrentDictionary<IntPtr, CallbackCcw> s_instances = new();
        private static readonly QueryInterfaceCallback s_queryInterface = QueryInterface;
        private static readonly RefCountCallback s_addRef = AddRef;
        private static readonly RefCountCallback s_release = Release;
        private static readonly HdaItemsCallback s_ignoredItems = OnIgnoredItems;
        private static readonly AttributeCallback s_ignoredAttribute = OnIgnoredAttribute;
        private static readonly HdaItemsCallback s_onPlayback = OnPlayback;
        private static readonly CancelCallback s_onCancelComplete = OnCancelComplete;
        private readonly IntPtr _vtable;
        private int _playbackCount;
        private int _cancelId;
        private int _transactionId;

        public CallbackCcw()
        {
            _vtable = Marshal.AllocCoTaskMem(12 * IntPtr.Size);
            WriteSlot(0, s_queryInterface); WriteSlot(1, s_addRef); WriteSlot(2, s_release); WriteSlot(3, s_ignoredItems); WriteSlot(4, s_ignoredItems); WriteSlot(5, s_ignoredItems); WriteSlot(6, s_ignoredAttribute); WriteSlot(7, s_ignoredItems); WriteSlot(8, s_ignoredItems); WriteSlot(9, s_onPlayback); WriteSlot(10, s_ignoredItems); WriteSlot(11, s_onCancelComplete);
            Pointer = Marshal.AllocCoTaskMem(IntPtr.Size);
            Marshal.WriteIntPtr(Pointer, _vtable);
            s_instances[Pointer] = this;
        }

        public IntPtr Pointer { get; }
        public int PlaybackCount => Volatile.Read(ref _playbackCount);
        public uint CancelId => unchecked((uint)Volatile.Read(ref _cancelId));
        public uint LastTransactionId => unchecked((uint)Volatile.Read(ref _transactionId));
        public void Dispose() { s_instances.TryRemove(Pointer, out _); Marshal.FreeCoTaskMem(Pointer); Marshal.FreeCoTaskMem(_vtable); }
        private void WriteSlot(int slot, Delegate method) => Marshal.WriteIntPtr(_vtable, slot * IntPtr.Size, Marshal.GetFunctionPointerForDelegate(method));
        private static int QueryInterface(IntPtr pThis, ref Guid riid, out IntPtr ppv) { if (riid == IID_IUnknown || riid == IOPCHDA_DataCallback.InterfaceId) { ppv = pThis; return S_OK; } ppv = IntPtr.Zero; return unchecked((int)0x80004002); }
        private static uint AddRef(IntPtr pThis) { _ = pThis; return 2; }
        private static uint Release(IntPtr pThis) { _ = pThis; return 1; }
        private static int OnPlayback(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) { _ = status; _ = count; _ = values; _ = errors; Volatile.Write(ref s_instances[pThis]._transactionId, unchecked((int)transactionId)); Interlocked.Increment(ref s_instances[pThis]._playbackCount); return S_OK; }
        private static int OnCancelComplete(IntPtr pThis, uint cancelId) { Volatile.Write(ref s_instances[pThis]._cancelId, unchecked((int)cancelId)); return S_OK; }
        private static int OnIgnoredItems(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) { _ = pThis; _ = transactionId; _ = status; _ = count; _ = values; _ = errors; return S_OK; }
        private static int OnIgnoredAttribute(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors) { _ = pThis; _ = transactionId; _ = status; _ = clientHandle; _ = count; _ = values; _ = errors; return S_OK; }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int QueryInterfaceCallback(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate uint RefCountCallback(IntPtr pThis);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int HdaItemsCallback(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AttributeCallback(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int CancelCallback(IntPtr pThis, uint cancelId);
    }
}
