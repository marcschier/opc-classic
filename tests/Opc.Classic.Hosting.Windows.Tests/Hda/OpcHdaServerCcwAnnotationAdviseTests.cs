//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcHdaServerCcwAnnotationAdviseTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task SyncAnnotationsInsert_dispatches_annotations_and_returns_per_item_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new DispatcherServer();
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        IntPtr syncAnnotations = Native.InvokeQI(ccw, IOPCHDA_SyncAnnotations.InterfaceId);
        using Native.IntArray handles = Native.IntArray.From([101, 404, 103]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From(SampleTimestamps(3));
        using Native.AnnotationArray annotations = Native.AnnotationArray.From([501, 502, 503], 2);

        server.InsertErrors = [S_OK, S_OK, S_OK];
        Native.InsertResult ok = Native.InvokeSyncAnnotationsInsert(syncAnnotations, handles.Pointer, timestamps.Pointer, annotations.Pointer, 3);

        await Assert.That(ok.Hr).IsEqualTo(S_OK);
        await Assert.That(ok.Errors).IsEquivalentTo([S_OK, S_OK, S_OK]);
        await Assert.That(server.LastInsertHandles).IsEquivalentTo([101, 404, 103]);
        await Assert.That(server.LastInsertAnnotationValueCounts).IsEquivalentTo([2, 2, 2]);

        server.InsertErrors = [S_OK, OpcResultId.InvalidHandle.Code, S_OK];
        Native.InsertResult partial = Native.InvokeSyncAnnotationsInsert(syncAnnotations, handles.Pointer, timestamps.Pointer, annotations.Pointer, 3);

        await Assert.That(partial.Hr).IsEqualTo(S_FALSE);
        await Assert.That(partial.Errors).IsEquivalentTo([S_OK, OpcResultId.InvalidHandle.Code, S_OK]);

        Native.InsertResult invalid = Native.InvokeSyncAnnotationsInsert(syncAnnotations, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0);

        await Assert.That(invalid.Hr).IsEqualTo(E_INVALIDARG);
    }

    [Test]
    public async Task AsyncAnnotationsInsert_fires_insert_annotations_callback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new DispatcherServer { InsertErrors = [S_OK, OpcResultId.InvalidHandle.Code, S_OK] };
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        using var callback = new CallbackCcw();
        uint cookie = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncAnnotations = Native.InvokeQI(ccw, IOPCHDA_AsyncAnnotations.InterfaceId);
        using Native.IntArray handles = Native.IntArray.From([101, 404, 103]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From(SampleTimestamps(3));
        using Native.AnnotationArray annotations = Native.AnnotationArray.From([701, 702, 703], 2);

        Native.AsyncResult result = Native.InvokeAsyncAnnotationsInsert(asyncAnnotations, 77, handles.Pointer, timestamps.Pointer, annotations.Pointer, 3);
        bool observed = SpinWait.SpinUntil(() => callback.InsertAnnotationsCount == 1, TimeSpan.FromSeconds(5));

        await Assert.That(observed).IsTrue();
        await Assert.That(result.Hr).IsEqualTo(S_FALSE);
        await Assert.That(result.CancelId).IsNotEqualTo(0u);
        await Assert.That(callback.LastInsertAnnotationsTransactionId).IsEqualTo(77u);
        await Assert.That(callback.LastInsertAnnotationClientHandles).IsEquivalentTo([701, 702, 703]);
        await Assert.That(callback.LastInsertAnnotationErrors).IsEquivalentTo([S_OK, OpcResultId.InvalidHandle.Code, S_OK]);

        Native.Unadvise(ccw, cookie);
    }

    [Test]
    public async Task AsyncAdviseRaw_fires_data_change_until_cancelled()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new DispatcherServer { RawDelay = TimeSpan.FromMilliseconds(20) };
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        using var callback = new CallbackCcw();
        uint cookie = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncRead = Native.InvokeQI(ccw, IOPCHDA_AsyncRead.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([101, 102]);

        Native.AsyncResult result = Native.InvokeAsyncAdviseRaw(asyncRead, 88, start.Pointer, TimeSpan.FromMilliseconds(20).Ticks, handles.Pointer, 2);
        bool observed = SpinWait.SpinUntil(() => callback.DataChangeCount >= 2, TimeSpan.FromSeconds(5));
        int cancelHr = Native.InvokeAsyncCancel(asyncRead, result.CancelId);
        int countAfterCancel = callback.DataChangeCount;
        await Task.Delay(120);

        await Assert.That(observed).IsTrue();
        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.CancelId).IsNotEqualTo(0u);
        await Assert.That(cancelHr).IsEqualTo(S_OK);
        await Assert.That(callback.LastDataChangeTransactionId).IsEqualTo(88u);
        await Assert.That(callback.CancelId).IsEqualTo(result.CancelId);
        await Assert.That(callback.DataChangeCount).IsEqualTo(countAfterCancel);

        Native.Unadvise(ccw, cookie);
    }

    [Test]
    public async Task AsyncAdviseProcessed_fires_data_change_with_interval_count_values()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new DispatcherServer
        {
            ProcessedDelay = TimeSpan.FromMilliseconds(20),
            ProcessedMaxUpdates = 1,
        };
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        using var callback = new CallbackCcw();
        uint cookie = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncRead = Native.InvokeQI(ccw, IOPCHDA_AsyncRead.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([101, 102]);
        using Native.IntArray aggregates = Native.IntArray.From([1, 4]);

        Native.AsyncResult result = Native.InvokeAsyncAdviseProcessed(asyncRead, 99, start.Pointer, TimeSpan.FromMilliseconds(20).Ticks, handles.Pointer, aggregates.Pointer, 3, 2);
        bool observed = SpinWait.SpinUntil(() => callback.DataChangeCount == 1, TimeSpan.FromSeconds(5));
        await Task.Delay(80);

        await Assert.That(observed).IsTrue();
        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(server.LastProcessedIntervalCount).IsEqualTo(3);
        await Assert.That(callback.LastDataChangeTransactionId).IsEqualTo(99u);
        await Assert.That(callback.LastFirstItemValueCount).IsEqualTo(3);
        await Assert.That(callback.DataChangeCount).IsEqualTo(1);

        Native.Unadvise(ccw, cookie);
    }

    private static DateTimeOffset[] SampleTimestamps(int count)
    {
        var timestamps = new DateTimeOffset[count];
        DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(-count);
        for (int i = 0; i < timestamps.Length; i++)
        {
            timestamps[i] = start.AddMinutes(i);
        }

        return timestamps;
    }

    private sealed class DispatcherServer : IOpcHdaServer, IOpcHdaServerDispatcher
    {
        public int[] InsertErrors { get; set; } = [];
        public TimeSpan RawDelay { get; init; } = TimeSpan.FromMilliseconds(10);
        public TimeSpan ProcessedDelay { get; init; } = TimeSpan.FromMilliseconds(10);
        public int ProcessedMaxUpdates { get; init; } = int.MaxValue;
        public int[] LastInsertHandles { get; private set; } = [];
        public int[] LastInsertAnnotationValueCounts { get; private set; } = [];
        public int LastProcessedIntervalCount { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds.Length]);

        public Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
        {
            _ = interfaceId;
            _ = opnum;
            _ = requestPayload;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        public Task<int[]> InsertAnnotationsAsync(int[] serverHandles, long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default)
        {
            _ = timestampFileTimes;
            cancellationToken.ThrowIfCancellationRequested();
            LastInsertHandles = Copy(serverHandles);
            LastInsertAnnotationValueCounts = new int[annotationValues.Length];
            for (int i = 0; i < annotationValues.Length; i++)
            {
                LastInsertAnnotationValueCounts[i] = annotationValues[i].Annotations.Length;
            }

            return Task.FromResult(InsertErrors.Length == 0 ? new int[serverHandles.Length] : Copy(InsertErrors));
        }

        public Task<OpcHdaAdviseSubscription> AdviseRawAsync(int[] serverHandles, OpcHdaTime startTime, long updateIntervalFileTime, CancellationToken cancellationToken = default)
        {
            _ = startTime;
            _ = updateIntervalFileTime;
            cancellationToken.ThrowIfCancellationRequested();
            int[] handles = Copy(serverHandles);
            return Task.FromResult(new OpcHdaAdviseSubscription(new int[handles.Length], RawUpdatesAsync(handles, cancellationToken)));
        }

        public Task<OpcHdaAdviseSubscription> AdviseProcessedAsync(int[] serverHandles, OpcHdaTime startTime, long resampleIntervalFileTime, int[] aggregateHandles, int intervalCount, CancellationToken cancellationToken = default)
        {
            _ = startTime;
            _ = resampleIntervalFileTime;
            cancellationToken.ThrowIfCancellationRequested();
            LastProcessedIntervalCount = intervalCount;
            int[] handles = Copy(serverHandles);
            int[] aggregates = Copy(aggregateHandles);
            return Task.FromResult(new OpcHdaAdviseSubscription(new int[handles.Length], ProcessedUpdatesAsync(handles, aggregates, intervalCount, cancellationToken)));
        }

        private async IAsyncEnumerable<OpcHdaDataUpdate> RawUpdatesAsync(int[] handles, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            int sequence = 0;
            while (true)
            {
                await Task.Delay(RawDelay, cancellationToken);
                yield return new OpcHdaDataUpdate(BuildItems(handles, null, 1, sequence++), new int[handles.Length]);
            }
        }

        private async IAsyncEnumerable<OpcHdaDataUpdate> ProcessedUpdatesAsync(int[] handles, int[] aggregates, int intervalCount, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < ProcessedMaxUpdates; i++)
            {
                await Task.Delay(ProcessedDelay, cancellationToken);
                yield return new OpcHdaDataUpdate(BuildItems(handles, aggregates, intervalCount, i), new int[handles.Length]);
            }
        }

        private static OpcHdaItem[] BuildItems(int[] handles, int[]? aggregates, int valueCount, int sequence)
        {
            var items = new OpcHdaItem[handles.Length];
            DateTimeOffset start = DateTimeOffset.UtcNow.AddSeconds(sequence);
            for (int i = 0; i < handles.Length; i++)
            {
                var timestamps = new DateTimeOffset[valueCount];
                var qualities = new uint[valueCount];
                var values = new OpcVariant[valueCount];
                for (int j = 0; j < valueCount; j++)
                {
                    timestamps[j] = start.AddMilliseconds(j);
                    qualities[j] = (uint)OpcQuality.Good.RawValue;
                    values[j] = OpcVariant.FromDouble(handles[i] + sequence + (j / 10.0));
                }

                items[i] = new OpcHdaItem(handles[i], aggregates is null ? 0 : aggregates[i], timestamps, qualities, values);
            }

            return items;
        }

        private static int[] Copy(int[] values)
        {
            var copy = new int[values.Length];
            Array.Copy(values, copy, values.Length);
            return copy;
        }
    }

    private static class Native
    {
        private static int PtrOffsetAfterTwoDwords => 8;
        private static int HdaTimeSize => IntPtr.Size == 8 ? 24 : 16;
        private static int HdaTimeFileTimeOffset => IntPtr.Size == 8 ? 16 : 8;
        private static int ItemSize => (IntPtr.Size == 8 ? 16 : 12) + (3 * IntPtr.Size);
        private static int AnnotationSize => PtrOffsetAfterTwoDwords + (4 * IntPtr.Size);

        internal readonly record struct InsertResult(int Hr, int[] Errors);
        internal readonly record struct AsyncResult(int Hr, uint CancelId, int[] Errors);

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static InsertResult InvokeSyncAnnotationsInsert(IntPtr syncAnnotations, IntPtr handles, IntPtr timestamps, IntPtr annotations, int count)
        {
            SyncAnnotationsInsertDelegate method = GetMethod<SyncAnnotationsInsertDelegate>(syncAnnotations, 5);
            int hr = method(syncAnnotations, unchecked((uint)count), handles, timestamps, annotations, out IntPtr errors);
            return new InsertResult(hr, ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncAnnotationsInsert(IntPtr asyncAnnotations, uint transactionId, IntPtr handles, IntPtr timestamps, IntPtr annotations, int count)
        {
            AsyncAnnotationsInsertDelegate method = GetMethod<AsyncAnnotationsInsertDelegate>(asyncAnnotations, 5);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncAnnotations, transactionId, unchecked((uint)count), handles, timestamps, annotations, cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncAdviseRaw(IntPtr asyncRead, uint transactionId, IntPtr start, long updateInterval, IntPtr handles, int count)
        {
            AsyncAdviseRawDelegate method = GetMethod<AsyncAdviseRawDelegate>(asyncRead, 4);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncRead, transactionId, start, updateInterval, unchecked((uint)count), handles, cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncAdviseProcessed(IntPtr asyncRead, uint transactionId, IntPtr start, long resampleInterval, IntPtr handles, IntPtr aggregates, int intervalCount, int count)
        {
            AsyncAdviseProcessedDelegate method = GetMethod<AsyncAdviseProcessedDelegate>(asyncRead, 6);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncRead, transactionId, start, resampleInterval, unchecked((uint)count), handles, aggregates, unchecked((uint)intervalCount), cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static int InvokeAsyncCancel(IntPtr asyncRead, uint cancelId)
        {
            AsyncCancelDelegate method = GetMethod<AsyncCancelDelegate>(asyncRead, 10);
            return method(asyncRead, cancelId);
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

        internal static void Unadvise(IntPtr ccw, uint cookie)
        {
            IntPtr cp = InvokeQI(ccw, OpcGuids.IID_IConnectionPoint);
            UnadviseDelegate unadvise = GetMethod<UnadviseDelegate>(cp, 6);
            _ = unadvise(cp, cookie);
        }

        private static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate
        {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        private static int[] ReadAndFreeErrors(IntPtr ptr, int count)
        {
            var values = new int[count];
            if (ptr != IntPtr.Zero && count > 0)
            {
                Marshal.Copy(ptr, values, 0, count);
            }
            FreeIfNonZero(ptr);
            return values;
        }

        private static void FreeIfNonZero(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SyncAnnotationsInsertDelegate(IntPtr pThis, uint count, IntPtr handles, IntPtr timestamps, IntPtr annotations, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncAnnotationsInsertDelegate(IntPtr pThis, uint transactionId, uint count, IntPtr handles, IntPtr timestamps, IntPtr annotations, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncAdviseRawDelegate(IntPtr pThis, uint transactionId, IntPtr start, long updateInterval, uint count, IntPtr handles, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncAdviseProcessedDelegate(IntPtr pThis, uint transactionId, IntPtr start, long resampleInterval, uint count, IntPtr handles, IntPtr aggregates, uint intervalCount, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncCancelDelegate(IntPtr pThis, uint cancelId);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int FindConnectionPointDelegate(IntPtr pThis, ref Guid iid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AdviseDelegate(IntPtr pThis, IntPtr callback, out uint cookie);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int UnadviseDelegate(IntPtr pThis, uint cookie);

        internal readonly struct TimeBlock : IDisposable
        {
            public TimeBlock(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static TimeBlock From(DateTimeOffset value)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(HdaTimeSize);
                Span<byte> zero = stackalloc byte[HdaTimeSize];
                Marshal.Copy(zero.ToArray(), 0, ptr, HdaTimeSize);
                Marshal.WriteInt64(ptr, HdaTimeFileTimeOffset, value.ToFileTime());
                return new TimeBlock(ptr);
            }
            public void Dispose() => FreeIfNonZero(Pointer);
        }

        internal readonly struct IntArray : IDisposable
        {
            public IntArray(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static IntArray From(int[] values)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
                Marshal.Copy(values, 0, ptr, values.Length);
                return new IntArray(ptr);
            }
            public void Dispose() => FreeIfNonZero(Pointer);
        }

        internal readonly struct FileTimeArray : IDisposable
        {
            public FileTimeArray(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static FileTimeArray From(DateTimeOffset[] values)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(long));
                for (int i = 0; i < values.Length; i++)
                {
                    Marshal.WriteInt64(ptr, i * sizeof(long), values[i].ToFileTime());
                }

                return new FileTimeArray(ptr);
            }
            public void Dispose() => FreeIfNonZero(Pointer);
        }

        internal sealed class AnnotationArray : IDisposable
        {
            private readonly int _count;

            private AnnotationArray(IntPtr pointer, int count)
            {
                Pointer = pointer;
                _count = count;
            }

            public IntPtr Pointer { get; }

            public static AnnotationArray From(int[] clientHandles, int valueCount)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(clientHandles.Length * AnnotationSize);
                Span<byte> zero = stackalloc byte[AnnotationSize];
                for (int i = 0; i < clientHandles.Length; i++)
                {
                    IntPtr slot = IntPtr.Add(ptr, i * AnnotationSize);
                    Marshal.Copy(zero.ToArray(), 0, slot, AnnotationSize);
                    Marshal.WriteInt32(slot, clientHandles[i]);
                    Marshal.WriteInt32(slot, sizeof(int), valueCount);
                    Marshal.WriteIntPtr(slot, PtrOffsetAfterTwoDwords, AllocateFileTimes(valueCount, i));
                    Marshal.WriteIntPtr(slot, PtrOffsetAfterTwoDwords + IntPtr.Size, AllocateStrings(valueCount, $"note-{i}"));
                    Marshal.WriteIntPtr(slot, PtrOffsetAfterTwoDwords + (2 * IntPtr.Size), AllocateFileTimes(valueCount, i + 10));
                    Marshal.WriteIntPtr(slot, PtrOffsetAfterTwoDwords + (3 * IntPtr.Size), AllocateStrings(valueCount, $"user-{i}"));
                }

                return new AnnotationArray(ptr, clientHandles.Length);
            }

            public void Dispose()
            {
                for (int i = 0; i < _count; i++)
                {
                    FreeAnnotation(IntPtr.Add(Pointer, i * AnnotationSize));
                }

                FreeIfNonZero(Pointer);
            }

            private static IntPtr AllocateFileTimes(int count, int offsetMinutes)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(count * sizeof(long));
                DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(offsetMinutes);
                for (int i = 0; i < count; i++)
                {
                    Marshal.WriteInt64(ptr, i * sizeof(long), start.AddSeconds(i).ToFileTime());
                }

                return ptr;
            }

            private static IntPtr AllocateStrings(int count, string prefix)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(count * IntPtr.Size);
                for (int i = 0; i < count; i++)
                {
                    Marshal.WriteIntPtr(ptr, i * IntPtr.Size, Marshal.StringToCoTaskMemUni($"{prefix}-{i}"));
                }

                return ptr;
            }

            private static void FreeAnnotation(IntPtr slot)
            {
                int valueCount = Math.Max(0, Marshal.ReadInt32(slot, sizeof(int)));
                FreeIfNonZero(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords));
                FreeStringArray(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + IntPtr.Size), valueCount);
                FreeIfNonZero(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (2 * IntPtr.Size)));
                FreeStringArray(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (3 * IntPtr.Size)), valueCount);
            }

            private static void FreeStringArray(IntPtr ptr, int count)
            {
                if (ptr == IntPtr.Zero)
                {
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    FreeIfNonZero(Marshal.ReadIntPtr(ptr, i * IntPtr.Size));
                }

                FreeIfNonZero(ptr);
            }
        }

        private readonly struct CoTaskMemBlock : IDisposable
        {
            public CoTaskMemBlock(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static CoTaskMemBlock Allocate(int byteCount)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
                Span<byte> zero = stackalloc byte[byteCount];
                Marshal.Copy(zero.ToArray(), 0, ptr, byteCount);
                return new CoTaskMemBlock(ptr);
            }
            public void Dispose() => FreeIfNonZero(Pointer);
        }
    }

    private sealed class CallbackCcw : IDisposable
    {
        private static readonly ConcurrentDictionary<IntPtr, CallbackCcw> s_instances = new();
        private static readonly QueryInterfaceCallback s_queryInterface = QueryInterface;
        private static readonly RefCountCallback s_addRef = AddRef;
        private static readonly RefCountCallback s_release = Release;
        private static readonly HdaItemsCallback s_onDataChange = OnDataChange;
        private static readonly HdaItemsCallback s_onIgnoredItems = OnIgnoredItems;
        private static readonly HdaAttributeCallback s_onIgnoredAttribute = OnIgnoredAttribute;
        private static readonly InsertAnnotationsCallback s_onInsertAnnotations = OnInsertAnnotations;
        private static readonly CancelCallback s_onCancelComplete = OnCancelComplete;
        private readonly object _syncRoot = new();
        private readonly IntPtr _vtable;
        private int _dataChangeCount;
        private int _insertAnnotationsCount;
        private int _cancelId;
        private int _lastFirstItemValueCount;
        private uint _lastDataChangeTransactionId;
        private uint _lastInsertAnnotationsTransactionId;
        private int[] _lastInsertAnnotationClientHandles = [];
        private int[] _lastInsertAnnotationErrors = [];

        public CallbackCcw()
        {
            _vtable = AllocateVtable();
            Pointer = Marshal.AllocCoTaskMem(IntPtr.Size);
            Marshal.WriteIntPtr(Pointer, _vtable);
            s_instances[Pointer] = this;
        }

        public IntPtr Pointer { get; }
        public int DataChangeCount => Volatile.Read(ref _dataChangeCount);
        public int InsertAnnotationsCount => Volatile.Read(ref _insertAnnotationsCount);
        public uint CancelId => unchecked((uint)Volatile.Read(ref _cancelId));
        public int LastFirstItemValueCount => Volatile.Read(ref _lastFirstItemValueCount);
        public uint LastDataChangeTransactionId => Volatile.Read(ref _lastDataChangeTransactionId);
        public uint LastInsertAnnotationsTransactionId => Volatile.Read(ref _lastInsertAnnotationsTransactionId);
        public int[] LastInsertAnnotationClientHandles => CopyLast(_lastInsertAnnotationClientHandles);
        public int[] LastInsertAnnotationErrors => CopyLast(_lastInsertAnnotationErrors);

        public void Dispose()
        {
            s_instances.TryRemove(Pointer, out _);
            Marshal.FreeCoTaskMem(Pointer);
            Marshal.FreeCoTaskMem(_vtable);
        }

        private static IntPtr AllocateVtable()
        {
            IntPtr vtable = Marshal.AllocCoTaskMem(12 * IntPtr.Size);
            WriteVtableSlot(vtable, 0, s_queryInterface);
            WriteVtableSlot(vtable, 1, s_addRef);
            WriteVtableSlot(vtable, 2, s_release);
            WriteVtableSlot(vtable, 3, s_onDataChange);
            WriteVtableSlot(vtable, 4, s_onIgnoredItems);
            WriteVtableSlot(vtable, 5, s_onIgnoredItems);
            WriteVtableSlot(vtable, 6, s_onIgnoredAttribute);
            WriteVtableSlot(vtable, 7, s_onIgnoredItems);
            WriteVtableSlot(vtable, 8, s_onInsertAnnotations);
            WriteVtableSlot(vtable, 9, s_onIgnoredItems);
            WriteVtableSlot(vtable, 10, s_onIgnoredItems);
            WriteVtableSlot(vtable, 11, s_onCancelComplete);
            return vtable;
        }

        private static void WriteVtableSlot(IntPtr vtable, int slot, Delegate method) =>
            Marshal.WriteIntPtr(vtable, slot * IntPtr.Size, Marshal.GetFunctionPointerForDelegate(method));

        private static int QueryInterface(IntPtr pThis, ref Guid riid, out IntPtr ppv)
        {
            if (riid == IID_IUnknown || riid == IOPCHDA_DataCallback.InterfaceId)
            {
                ppv = pThis;
                return S_OK;
            }

            ppv = IntPtr.Zero;
            return unchecked((int)0x80004002);
        }

        private static uint AddRef(IntPtr pThis)
        {
            _ = pThis;
            return 2;
        }

        private static uint Release(IntPtr pThis)
        {
            _ = pThis;
            return 1;
        }

        private static int OnDataChange(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors)
        {
            _ = status;
            _ = errors;
            CallbackCcw instance = s_instances[pThis];
            Volatile.Write(ref instance._lastDataChangeTransactionId, transactionId);
            if (values != IntPtr.Zero && count > 0)
            {
                Volatile.Write(ref instance._lastFirstItemValueCount, Marshal.ReadInt32(values, 8));
            }

            Interlocked.Increment(ref instance._dataChangeCount);
            return S_OK;
        }

        private static int OnInsertAnnotations(IntPtr pThis, uint transactionId, int status, uint count, IntPtr handles, IntPtr errors)
        {
            _ = status;
            CallbackCcw instance = s_instances[pThis];
            int itemCount = checked((int)count);
            int[] clientHandles = ReadInt32Array(handles, itemCount);
            int[] itemErrors = ReadInt32Array(errors, itemCount);
            lock (instance._syncRoot)
            {
                instance._lastInsertAnnotationsTransactionId = transactionId;
                instance._lastInsertAnnotationClientHandles = clientHandles;
                instance._lastInsertAnnotationErrors = itemErrors;
            }

            Interlocked.Increment(ref instance._insertAnnotationsCount);
            return S_OK;
        }

        private static int OnIgnoredItems(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors)
        {
            _ = pThis;
            _ = transactionId;
            _ = status;
            _ = count;
            _ = values;
            _ = errors;
            return S_OK;
        }

        private static int OnIgnoredAttribute(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors)
        {
            _ = pThis;
            _ = transactionId;
            _ = status;
            _ = clientHandle;
            _ = count;
            _ = values;
            _ = errors;
            return S_OK;
        }

        private static int OnCancelComplete(IntPtr pThis, uint cancelId)
        {
            Volatile.Write(ref s_instances[pThis]._cancelId, unchecked((int)cancelId));
            return S_OK;
        }

        private static int[] ReadInt32Array(IntPtr ptr, int count)
        {
            var values = new int[count];
            if (ptr != IntPtr.Zero && count > 0)
            {
                Marshal.Copy(ptr, values, 0, count);
            }

            return values;
        }

        private int[] CopyLast(int[] values)
        {
            lock (_syncRoot)
            {
                var copy = new int[values.Length];
                Array.Copy(values, copy, values.Length);
                return copy;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int QueryInterfaceCallback(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate uint RefCountCallback(IntPtr pThis);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int HdaItemsCallback(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int HdaAttributeCallback(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int InsertAnnotationsCallback(IntPtr pThis, uint transactionId, int status, uint count, IntPtr handles, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int CancelCallback(IntPtr pThis, uint cancelId);
    }
}
