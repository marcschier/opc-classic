// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcHdaServerCcwReadTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task Sync_read_methods_dispatch_and_marshal_hda_payloads()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new ReadServer();
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        IntPtr syncRead = Native.InvokeQI(ccw, IOPCHDA_SyncRead.InterfaceId);
        IntPtr syncAnnotations = Native.InvokeQI(ccw, IOPCHDA_SyncAnnotations.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow.AddHours(-1));
        using Native.TimeBlock end = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([101, 102]);
        using Native.IntArray aggregates = Native.IntArray.From([1, 4]);
        using Native.IntArray attributes = Native.IntArray.From([1, 2]);
        using Native.FileTimeArray times = Native.FileTimeArray.From([DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow]);

        Native.HdaItemsResult raw = Native.InvokeSyncReadRaw(syncRead, start.Pointer, end.Pointer, handles.Pointer, 2);
        Native.HdaItemsResult processed = Native.InvokeSyncReadProcessed(syncRead, start.Pointer, end.Pointer, handles.Pointer, aggregates.Pointer, 2);
        Native.HdaItemsResult atTime = Native.InvokeSyncReadAtTime(syncRead, times.Pointer, 2, handles.Pointer, 2);
        Native.HdaModifiedResult modified = Native.InvokeSyncReadModified(syncRead, start.Pointer, end.Pointer, handles.Pointer, 2);
        Native.HdaAttributesResult attribute = Native.InvokeSyncReadAttribute(syncRead, start.Pointer, end.Pointer, 101, attributes.Pointer, 2);
        Native.HdaAnnotationsResult annotations = Native.InvokeSyncReadAnnotations(syncAnnotations, start.Pointer, end.Pointer, handles.Pointer, 2);

        await Assert.That(raw.Hr).IsEqualTo(S_OK);
        await Assert.That(raw.Items[0].Count).IsEqualTo(3);
        await Assert.That(raw.Items[0].FirstValue).IsEqualTo(101.0);
        await Assert.That(processed.Items[1].Aggregate).IsEqualTo(4);
        await Assert.That(atTime.Items[0].Count).IsEqualTo(2);
        await Assert.That(modified.Items[0].User).IsEqualTo("historian");
        await Assert.That(attribute.Attributes[1].AttributeId).IsEqualTo(2);
        await Assert.That(attribute.Attributes[1].FirstValueText).IsEqualTo("attribute-2");
        await Assert.That(annotations.Annotations[0].User).IsEqualTo("operator");
        await Assert.That(server.RawCalls).IsEqualTo(1);
    }

    [Test]
    public async Task Sync_read_methods_reject_empty_counts()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new ReadServer(), IID_IUnknown);
        IntPtr syncRead = Native.InvokeQI(ccw, IOPCHDA_SyncRead.InterfaceId);
        IntPtr syncAnnotations = Native.InvokeQI(ccw, IOPCHDA_SyncAnnotations.InterfaceId);
        using Native.TimeBlock time = Native.TimeBlock.From(DateTimeOffset.UtcNow);

        await Assert.That(Native.InvokeSyncReadRaw(syncRead, time.Pointer, time.Pointer, IntPtr.Zero, 0).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Native.InvokeSyncReadProcessed(syncRead, time.Pointer, time.Pointer, IntPtr.Zero, IntPtr.Zero, 0).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Native.InvokeSyncReadAtTime(syncRead, IntPtr.Zero, 0, IntPtr.Zero, 0).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Native.InvokeSyncReadModified(syncRead, time.Pointer, time.Pointer, IntPtr.Zero, 0).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Native.InvokeSyncReadAttribute(syncRead, time.Pointer, time.Pointer, 0, IntPtr.Zero, 0).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Native.InvokeSyncReadAnnotations(syncAnnotations, time.Pointer, time.Pointer, IntPtr.Zero, 0).Hr).IsEqualTo(E_INVALIDARG);
    }

    [Test]
    public async Task SyncReadRaw_returns_per_item_error_for_missing_result()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new ReadServer { DropLastRawResult = true };
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        IntPtr syncRead = Native.InvokeQI(ccw, IOPCHDA_SyncRead.InterfaceId);
        using Native.TimeBlock time = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([101, 404]);

        Native.HdaItemsResult result = Native.InvokeSyncReadRaw(syncRead, time.Pointer, time.Pointer, handles.Pointer, 2);

        await Assert.That(result.Hr).IsEqualTo(S_FALSE);
        await Assert.That(result.Errors[0]).IsEqualTo(S_OK);
        await Assert.That(result.Errors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
    }

    [Test]
    public async Task Async_read_methods_return_cancel_ids_and_fire_callbacks()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new ReadServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        uint cookie = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncRead = Native.InvokeQI(ccw, IOPCHDA_AsyncRead.InterfaceId);
        IntPtr asyncAnnotations = Native.InvokeQI(ccw, IOPCHDA_AsyncAnnotations.InterfaceId);
        using Native.TimeBlock time = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([101, 102]);
        using Native.IntArray aggregates = Native.IntArray.From([1, 4]);
        using Native.IntArray attributes = Native.IntArray.From([1, 2]);
        using Native.FileTimeArray times = Native.FileTimeArray.From([DateTimeOffset.UtcNow]);

        Native.AsyncResult raw = Native.InvokeAsyncReadRaw(asyncRead, 10, time.Pointer, time.Pointer, handles.Pointer, 2);
        Native.AsyncResult processed = Native.InvokeAsyncReadProcessed(asyncRead, 11, time.Pointer, time.Pointer, handles.Pointer, aggregates.Pointer, 2);
        Native.AsyncResult atTime = Native.InvokeAsyncReadAtTime(asyncRead, 12, times.Pointer, 1, handles.Pointer, 2);
        Native.AsyncResult modified = Native.InvokeAsyncReadModified(asyncRead, 13, time.Pointer, time.Pointer, handles.Pointer, 2);
        Native.AsyncResult attribute = Native.InvokeAsyncReadAttribute(asyncRead, 14, time.Pointer, time.Pointer, 101, attributes.Pointer, 2);
        Native.AsyncResult annotations = Native.InvokeAsyncReadAnnotations(asyncAnnotations, 15, time.Pointer, time.Pointer, handles.Pointer, 2);

        SpinWait.SpinUntil(() => callback.ReadCompleteCount >= 3 && callback.ModifiedCount == 1 && callback.AttributeCount == 1 && callback.AnnotationCount == 1, TimeSpan.FromSeconds(5));

        await Assert.That(raw.Hr).IsEqualTo(S_OK);
        await Assert.That(processed.CancelId).IsNotEqualTo(0u);
        await Assert.That(atTime.Hr).IsEqualTo(S_OK);
        await Assert.That(atTime.Errors[0]).IsEqualTo(S_OK);
        await Assert.That(modified.CancelId).IsNotEqualTo(attribute.CancelId);
        await Assert.That(annotations.CancelId).IsNotEqualTo(0u);
        await Assert.That(callback.ReadCompleteCount).IsEqualTo(3);
        await Assert.That(callback.ModifiedCount).IsEqualTo(1);
        await Assert.That(callback.AttributeCount).IsEqualTo(1);
        await Assert.That(callback.AnnotationCount).IsEqualTo(1);

        Native.Unadvise(ccw, cookie);
    }

    [Test]
    public async Task AsyncCancel_returns_cancel_complete_for_pending_read()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new ReadServer(), IID_IUnknown);
        using var callback = new CallbackCcw();
        _ = Native.Advise(ccw, callback.Pointer);
        IntPtr asyncRead = Native.InvokeQI(ccw, IOPCHDA_AsyncRead.InterfaceId);
        using Native.TimeBlock time = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([101]);

        Native.AsyncResult raw = Native.InvokeAsyncReadRaw(asyncRead, 20, time.Pointer, time.Pointer, handles.Pointer, 1);
        int cancelHr = Native.InvokeAsyncCancel(asyncRead, raw.CancelId);
        SpinWait.SpinUntil(() => callback.CancelId == raw.CancelId, TimeSpan.FromSeconds(5));

        await Assert.That(cancelHr).IsEqualTo(S_OK);
        await Assert.That(callback.CancelId).IsEqualTo(raw.CancelId);
    }

    private sealed class ReadServer : IOpcHdaServer, IOPCHDA_SyncRead, IOPCHDA_SyncAnnotations
    {
        public int RawCalls { get; private set; }
        public bool DropLastRawResult { get; init; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds.Length]);

        public Task<OpcHdaItem[]> ReadRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            RawCalls++;
            _ = startTime; _ = endTime; _ = maxValues; _ = bounds;
            int length = DropLastRawResult ? serverHandles.Length - 1 : serverHandles.Length;
            var items = new OpcHdaItem[length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = SampleItem(serverHandles[i], 0, serverHandles[i]);
            }
            return Task.FromResult(items);
        }

        public Task<OpcHdaItem[]> ReadProcessedAsync(OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default)
        {
            _ = startTime; _ = endTime; _ = resampleIntervalFileTime;
            return Task.FromResult(serverHandles.Select((handle, i) => SampleItem(handle, aggregateIds[i], handle + 0.5)).ToArray());
        }

        public Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default) =>
            Task.FromResult(serverHandles.Select(handle => new OpcHdaItem(handle, 0, timestampFileTimes.Select(DateTimeOffset.FromFileTime).ToArray(), Enumerable.Repeat((uint)OpcQuality.Good.RawValue, timestampFileTimes.Length).ToArray(), timestampFileTimes.Select((_, i) => OpcVariant.FromDouble(handle + i)).ToArray())).ToArray());

        public Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            _ = startTime; _ = endTime; _ = maxValues;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return Task.FromResult(serverHandles.Select(handle => new OpcHdaModifiedItem(handle, [now], [(uint)OpcQuality.Good.RawValue], [OpcVariant.FromDouble(handle)], [now.AddSeconds(1)], [1], ["historian"])).ToArray());
        }

        public Task<OpcHdaAttribute[]> ReadAttributeAsync(OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            _ = startTime; _ = endTime;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return Task.FromResult(attributeIds.Select(id => new OpcHdaAttribute(serverHandle, id, [now], [OpcVariant.FromString($"attribute-{id}")])).ToArray());
        }

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

        public Task<OpcHdaAnnotation[]> ReadAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            _ = startTime; _ = endTime;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return Task.FromResult(serverHandles.Select(handle => new OpcHdaAnnotation(handle, [now], ["note"], [now.AddSeconds(1)], ["operator"])).ToArray());
        }

        public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[serverHandles.Length]);

        private static OpcHdaItem SampleItem(int clientHandle, int aggregate, double value)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return new OpcHdaItem(
                clientHandle,
                aggregate,
                [now, now.AddSeconds(1), now.AddSeconds(2)],
                [(uint)OpcQuality.Good.RawValue, (uint)OpcQuality.Good.RawValue, (uint)OpcQuality.Good.RawValue],
                [OpcVariant.FromDouble(value), OpcVariant.FromDouble(value + 1), OpcVariant.FromDouble(value + 2)]);
        }
    }

    private static class Native
    {
        private static int PtrOffsetAfterThreeDwords => IntPtr.Size == 8 ? 16 : 12;
        private static int PtrOffsetAfterTwoDwords => IntPtr.Size == 8 ? 8 : 8;
        private static int HdaTimeSize => IntPtr.Size == 8 ? 24 : 16;
        private static int HdaTimeFileTimeOffset => IntPtr.Size == 8 ? 16 : 8;
        private static int ItemSize => PtrOffsetAfterThreeDwords + (3 * IntPtr.Size);
        private static int AttributeSize => PtrOffsetAfterThreeDwords + (2 * IntPtr.Size);
        private static int ModifiedSize => PtrOffsetAfterTwoDwords + (6 * IntPtr.Size);
        private static int AnnotationSize => PtrOffsetAfterTwoDwords + (4 * IntPtr.Size);
        private static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

        internal readonly record struct ItemInfo(int ClientHandle, int Aggregate, int Count, double FirstValue);
        internal readonly record struct ModifiedInfo(string? User);
        internal readonly record struct AttributeInfo(int AttributeId, string? FirstValueText);
        internal readonly record struct AnnotationInfo(string? User);
        internal readonly record struct HdaItemsResult(int Hr, ItemInfo[] Items, int[] Errors);
        internal readonly record struct HdaModifiedResult(int Hr, ModifiedInfo[] Items, int[] Errors);
        internal readonly record struct HdaAttributesResult(int Hr, AttributeInfo[] Attributes, int[] Errors);
        internal readonly record struct HdaAnnotationsResult(int Hr, AnnotationInfo[] Annotations, int[] Errors);
        internal readonly record struct AsyncResult(int Hr, uint CancelId, int[] Errors);

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static HdaItemsResult InvokeSyncReadRaw(IntPtr syncRead, IntPtr start, IntPtr end, IntPtr handles, int count)
        {
            SyncReadRawDelegate method = GetMethod<SyncReadRawDelegate>(syncRead, 3);
            int hr = method(syncRead, start, end, 10, 1, unchecked((uint)count), handles, out IntPtr items, out IntPtr errors);
            return new HdaItemsResult(hr, ReadAndFreeItems(items, count), ReadAndFreeErrors(errors, count));
        }

        internal static HdaItemsResult InvokeSyncReadProcessed(IntPtr syncRead, IntPtr start, IntPtr end, IntPtr handles, IntPtr aggregates, int count)
        {
            SyncReadProcessedDelegate method = GetMethod<SyncReadProcessedDelegate>(syncRead, 4);
            int hr = method(syncRead, start, end, TimeSpan.FromSeconds(1).Ticks, unchecked((uint)count), handles, aggregates, out IntPtr items, out IntPtr errors);
            return new HdaItemsResult(hr, ReadAndFreeItems(items, count), ReadAndFreeErrors(errors, count));
        }

        internal static HdaItemsResult InvokeSyncReadAtTime(IntPtr syncRead, IntPtr times, int timeCount, IntPtr handles, int count)
        {
            SyncReadAtTimeDelegate method = GetMethod<SyncReadAtTimeDelegate>(syncRead, 5);
            int hr = method(syncRead, unchecked((uint)timeCount), times, unchecked((uint)count), handles, out IntPtr items, out IntPtr errors);
            return new HdaItemsResult(hr, ReadAndFreeItems(items, count), ReadAndFreeErrors(errors, count));
        }

        internal static HdaModifiedResult InvokeSyncReadModified(IntPtr syncRead, IntPtr start, IntPtr end, IntPtr handles, int count)
        {
            SyncReadModifiedDelegate method = GetMethod<SyncReadModifiedDelegate>(syncRead, 6);
            int hr = method(syncRead, start, end, 10, unchecked((uint)count), handles, out IntPtr items, out IntPtr errors);
            return new HdaModifiedResult(hr, ReadAndFreeModified(items, count), ReadAndFreeErrors(errors, count));
        }

        internal static HdaAttributesResult InvokeSyncReadAttribute(IntPtr syncRead, IntPtr start, IntPtr end, int serverHandle, IntPtr attributes, int count)
        {
            SyncReadAttributeDelegate method = GetMethod<SyncReadAttributeDelegate>(syncRead, 7);
            int hr = method(syncRead, start, end, unchecked((uint)serverHandle), unchecked((uint)count), attributes, out IntPtr values, out IntPtr errors);
            return new HdaAttributesResult(hr, ReadAndFreeAttributes(values, count), ReadAndFreeErrors(errors, count));
        }

        internal static HdaAnnotationsResult InvokeSyncReadAnnotations(IntPtr syncAnnotations, IntPtr start, IntPtr end, IntPtr handles, int count)
        {
            SyncReadAnnotationsDelegate method = GetMethod<SyncReadAnnotationsDelegate>(syncAnnotations, 4);
            int hr = method(syncAnnotations, start, end, unchecked((uint)count), handles, out IntPtr values, out IntPtr errors);
            return new HdaAnnotationsResult(hr, ReadAndFreeAnnotations(values, count), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncReadRaw(IntPtr asyncRead, uint transactionId, IntPtr start, IntPtr end, IntPtr handles, int count)
        {
            AsyncReadRawDelegate method = GetMethod<AsyncReadRawDelegate>(asyncRead, 3);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncRead, transactionId, start, end, 10, 1, unchecked((uint)count), handles, cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncReadProcessed(IntPtr asyncRead, uint transactionId, IntPtr start, IntPtr end, IntPtr handles, IntPtr aggregates, int count)
        {
            AsyncReadProcessedDelegate method = GetMethod<AsyncReadProcessedDelegate>(asyncRead, 5);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncRead, transactionId, start, end, TimeSpan.FromSeconds(1).Ticks, unchecked((uint)count), handles, aggregates, cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncReadAtTime(IntPtr asyncRead, uint transactionId, IntPtr times, int timeCount, IntPtr handles, int count)
        {
            AsyncReadAtTimeDelegate method = GetMethod<AsyncReadAtTimeDelegate>(asyncRead, 7);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncRead, transactionId, unchecked((uint)timeCount), times, unchecked((uint)count), handles, cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncReadModified(IntPtr asyncRead, uint transactionId, IntPtr start, IntPtr end, IntPtr handles, int count)
        {
            AsyncReadModifiedDelegate method = GetMethod<AsyncReadModifiedDelegate>(asyncRead, 8);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncRead, transactionId, start, end, 10, unchecked((uint)count), handles, cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncReadAttribute(IntPtr asyncRead, uint transactionId, IntPtr start, IntPtr end, int serverHandle, IntPtr attributes, int count)
        {
            AsyncReadAttributeDelegate method = GetMethod<AsyncReadAttributeDelegate>(asyncRead, 9);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncRead, transactionId, start, end, unchecked((uint)serverHandle), unchecked((uint)count), attributes, cancel.Pointer, out IntPtr errors);
            return new AsyncResult(hr, unchecked((uint)Marshal.ReadInt32(cancel.Pointer)), ReadAndFreeErrors(errors, count));
        }

        internal static AsyncResult InvokeAsyncReadAnnotations(IntPtr asyncAnnotations, uint transactionId, IntPtr start, IntPtr end, IntPtr handles, int count)
        {
            AsyncReadAnnotationsDelegate method = GetMethod<AsyncReadAnnotationsDelegate>(asyncAnnotations, 4);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = method(asyncAnnotations, transactionId, start, end, unchecked((uint)count), handles, cancel.Pointer, out IntPtr errors);
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

        private static ItemInfo[] ReadAndFreeItems(IntPtr ptr, int count)
        {
            var values = new ItemInfo[count];
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                IntPtr slot = IntPtr.Add(ptr, i * ItemSize);
                int sampleCount = Marshal.ReadInt32(slot, 8);
                IntPtr variants = Marshal.ReadIntPtr(slot, PtrOffsetAfterThreeDwords + (2 * IntPtr.Size));
                values[i] = new ItemInfo(Marshal.ReadInt32(slot), Marshal.ReadInt32(slot, 4), sampleCount, sampleCount > 0 ? ReadDoubleVariant(variants) : 0);
                FreeItem(slot);
            }
            FreeIfNonZero(ptr);
            return values;
        }

        private static ModifiedInfo[] ReadAndFreeModified(IntPtr ptr, int count)
        {
            var values = new ModifiedInfo[count];
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                IntPtr slot = IntPtr.Add(ptr, i * ModifiedSize);
                IntPtr users = Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (5 * IntPtr.Size));
                values[i] = new ModifiedInfo(Marshal.PtrToStringUni(Marshal.ReadIntPtr(users)));
                FreeModified(slot);
            }
            FreeIfNonZero(ptr);
            return values;
        }

        private static AttributeInfo[] ReadAndFreeAttributes(IntPtr ptr, int count)
        {
            var values = new AttributeInfo[count];
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                IntPtr slot = IntPtr.Add(ptr, i * AttributeSize);
                IntPtr variants = Marshal.ReadIntPtr(slot, PtrOffsetAfterThreeDwords + IntPtr.Size);
                values[i] = new AttributeInfo(Marshal.ReadInt32(slot, 8), ReadStringVariant(variants));
                FreeAttribute(slot);
            }
            FreeIfNonZero(ptr);
            return values;
        }

        private static AnnotationInfo[] ReadAndFreeAnnotations(IntPtr ptr, int count)
        {
            var values = new AnnotationInfo[count];
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                IntPtr slot = IntPtr.Add(ptr, i * AnnotationSize);
                IntPtr users = Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (3 * IntPtr.Size));
                values[i] = new AnnotationInfo(Marshal.PtrToStringUni(Marshal.ReadIntPtr(users)));
                FreeAnnotation(slot);
            }
            FreeIfNonZero(ptr);
            return values;
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

        private static double ReadDoubleVariant(IntPtr variant) => BitConverter.Int64BitsToDouble(Marshal.ReadInt64(variant, 8));
        private static string? ReadStringVariant(IntPtr variant) => Marshal.PtrToStringBSTR(Marshal.ReadIntPtr(variant, 8));

        private static void FreeItem(IntPtr slot)
        {
            int sampleCount = Math.Max(0, Marshal.ReadInt32(slot, 8));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterThreeDwords));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterThreeDwords + IntPtr.Size));
            FreeVariants(Marshal.ReadIntPtr(slot, PtrOffsetAfterThreeDwords + (2 * IntPtr.Size)), sampleCount);
        }

        private static void FreeModified(IntPtr slot)
        {
            int sampleCount = Math.Max(0, Marshal.ReadInt32(slot, 4));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + IntPtr.Size));
            FreeVariants(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (2 * IntPtr.Size)), sampleCount);
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (3 * IntPtr.Size)));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (4 * IntPtr.Size)));
            FreeStringArray(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (5 * IntPtr.Size)), sampleCount);
        }

        private static void FreeAttribute(IntPtr slot)
        {
            int sampleCount = Math.Max(0, Marshal.ReadInt32(slot, 4));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterThreeDwords));
            FreeVariants(Marshal.ReadIntPtr(slot, PtrOffsetAfterThreeDwords + IntPtr.Size), sampleCount);
        }

        private static void FreeAnnotation(IntPtr slot)
        {
            int sampleCount = Math.Max(0, Marshal.ReadInt32(slot, 4));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords));
            FreeStringArray(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + IntPtr.Size), sampleCount);
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (2 * IntPtr.Size)));
            FreeStringArray(Marshal.ReadIntPtr(slot, PtrOffsetAfterTwoDwords + (3 * IntPtr.Size)), sampleCount);
        }

        private static void FreeVariants(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                IntPtr slot = IntPtr.Add(ptr, i * VariantSize);
                ushort vt = unchecked((ushort)Marshal.ReadInt16(slot));
                if (vt == 8)
                {
                    Marshal.FreeBSTR(Marshal.ReadIntPtr(slot, 8));
                }
            }
            FreeIfNonZero(ptr);
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

        private static void FreeIfNonZero(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SyncReadRawDelegate(IntPtr pThis, IntPtr start, IntPtr end, uint numValues, int bounds, uint count, IntPtr handles, out IntPtr values, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SyncReadProcessedDelegate(IntPtr pThis, IntPtr start, IntPtr end, long interval, uint count, IntPtr handles, IntPtr aggregates, out IntPtr values, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SyncReadAtTimeDelegate(IntPtr pThis, uint timeCount, IntPtr times, uint count, IntPtr handles, out IntPtr values, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SyncReadModifiedDelegate(IntPtr pThis, IntPtr start, IntPtr end, uint numValues, uint count, IntPtr handles, out IntPtr values, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SyncReadAttributeDelegate(IntPtr pThis, IntPtr start, IntPtr end, uint serverHandle, uint count, IntPtr attributes, out IntPtr values, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SyncReadAnnotationsDelegate(IntPtr pThis, IntPtr start, IntPtr end, uint count, IntPtr handles, out IntPtr values, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncReadRawDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, uint numValues, int bounds, uint count, IntPtr handles, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncReadProcessedDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, long interval, uint count, IntPtr handles, IntPtr aggregates, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncReadAtTimeDelegate(IntPtr pThis, uint transactionId, uint timeCount, IntPtr times, uint count, IntPtr handles, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncReadModifiedDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, uint numValues, uint count, IntPtr handles, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncReadAttributeDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, uint serverHandle, uint count, IntPtr attributes, IntPtr cancel, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AsyncReadAnnotationsDelegate(IntPtr pThis, uint transactionId, IntPtr start, IntPtr end, uint count, IntPtr handles, IntPtr cancel, out IntPtr errors);
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
        private static readonly HdaItemsCallback s_onReadComplete = OnReadComplete;
        private static readonly HdaItemsCallback s_onReadModifiedComplete = OnReadModifiedComplete;
        private static readonly HdaAttributeCallback s_onReadAttributeComplete = OnReadAttributeComplete;
        private static readonly HdaItemsCallback s_onReadAnnotations = OnReadAnnotations;
        private static readonly HdaItemsCallback s_onIgnored = OnIgnored;
        private static readonly CancelCallback s_onCancelComplete = OnCancelComplete;
        private readonly IntPtr _vtable;

        public CallbackCcw()
        {
            _vtable = AllocateVtable();
            Pointer = Marshal.AllocCoTaskMem(IntPtr.Size);
            Marshal.WriteIntPtr(Pointer, _vtable);
            s_instances[Pointer] = this;
        }

        private int _readCompleteCount;
        private int _modifiedCount;
        private int _attributeCount;
        private int _annotationCount;
        private int _cancelId;

        public IntPtr Pointer { get; }
        public int ReadCompleteCount => Volatile.Read(ref _readCompleteCount);
        public int ModifiedCount => Volatile.Read(ref _modifiedCount);
        public int AttributeCount => Volatile.Read(ref _attributeCount);
        public int AnnotationCount => Volatile.Read(ref _annotationCount);
        public uint CancelId => unchecked((uint)Volatile.Read(ref _cancelId));

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
            WriteVtableSlot(vtable, 4, s_onReadComplete);
            WriteVtableSlot(vtable, 5, s_onReadModifiedComplete);
            WriteVtableSlot(vtable, 6, s_onReadAttributeComplete);
            WriteVtableSlot(vtable, 7, s_onReadAnnotations);
            WriteVtableSlot(vtable, 8, s_onIgnored);
            WriteVtableSlot(vtable, 9, s_onIgnored);
            WriteVtableSlot(vtable, 10, s_onIgnored);
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

        private static uint AddRef(IntPtr pThis) { _ = pThis; return 2; }
        private static uint Release(IntPtr pThis) { _ = pThis; return 1; }
        private static int OnDataChange(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) => OnReadComplete(pThis, transactionId, status, count, values, errors);
        private static int OnReadComplete(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) { _ = transactionId; _ = status; _ = count; _ = values; _ = errors; Interlocked.Increment(ref s_instances[pThis]._readCompleteCount); return S_OK; }
        private static int OnReadModifiedComplete(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) { _ = transactionId; _ = status; _ = count; _ = values; _ = errors; Interlocked.Increment(ref s_instances[pThis]._modifiedCount); return S_OK; }
        private static int OnReadAttributeComplete(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors) { _ = transactionId; _ = status; _ = clientHandle; _ = count; _ = values; _ = errors; Interlocked.Increment(ref s_instances[pThis]._attributeCount); return S_OK; }
        private static int OnReadAnnotations(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) { _ = transactionId; _ = status; _ = count; _ = values; _ = errors; Interlocked.Increment(ref s_instances[pThis]._annotationCount); return S_OK; }
        private static int OnIgnored(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors) { _ = pThis; _ = transactionId; _ = status; _ = count; _ = values; _ = errors; return S_OK; }
        private static int OnCancelComplete(IntPtr pThis, uint cancelId) { Volatile.Write(ref s_instances[pThis]._cancelId, unchecked((int)cancelId)); return S_OK; }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int QueryInterfaceCallback(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate uint RefCountCallback(IntPtr pThis);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int HdaItemsCallback(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int HdaAttributeCallback(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int CancelCallback(IntPtr pThis, uint cancelId);
    }
}
