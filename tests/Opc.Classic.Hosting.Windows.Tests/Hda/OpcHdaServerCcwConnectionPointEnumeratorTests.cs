// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Raw CCW tests assert HRESULT constants.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcHdaServerCcwConnectionPointEnumeratorTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task IConnectionPointContainer_enumconnectionpoints_yields_data_callback_connection_point()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = IntPtr.Zero;
        IntPtr cpcPtr = IntPtr.Zero;
        IntPtr cpPtr = IntPtr.Zero;
        IntPtr enumPtr = IntPtr.Zero;
        IntPtr queriedEnumPtr = IntPtr.Zero;
        IntPtr clonePtr = IntPtr.Zero;
        Native.ConnectionPointsResult all = default;
        Native.ConnectionPointsResult afterSkip = default;
        Native.ConnectionPointsResult first = default;
        Native.ConnectionPointsResult originalAfterReset = default;
        Native.ConnectionPointsResult cloneAtEnd = default;
        Native.ConnectionPointsResult cloneAfterReset = default;
        int skipHr;
        int resetOriginalHr;
        int cloneHr;
        int resetCloneHr;
        Native.GuidResult connectionInterface;

        try
        {
            ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);
            cpcPtr = Native.InvokeQI(ccw, OpcGuids.IID_IConnectionPointContainer);
            cpPtr = Native.InvokeFindConnectionPoint(cpcPtr, IOPCHDA_DataCallback.InterfaceId).Pointer;
            enumPtr = Native.InvokeEnumConnectionPoints(cpcPtr).Pointer;
            queriedEnumPtr = Native.InvokeQI(enumPtr, OpcGuids.IID_IEnumConnectionPoints);

            all = Native.InvokeEnumConnectionPointsNext(queriedEnumPtr, 2);
            Native.InvokeEnumReset(queriedEnumPtr);
            skipHr = Native.InvokeEnumSkip(queriedEnumPtr, 1);
            afterSkip = Native.InvokeEnumConnectionPointsNext(queriedEnumPtr, 1);
            resetOriginalHr = Native.InvokeEnumReset(queriedEnumPtr);
            first = Native.InvokeEnumConnectionPointsNext(queriedEnumPtr, 1);
            (cloneHr, clonePtr) = Native.InvokeEnumClone(queriedEnumPtr);
            Native.InvokeEnumReset(queriedEnumPtr);
            originalAfterReset = Native.InvokeEnumConnectionPointsNext(queriedEnumPtr, 1);
            cloneAtEnd = Native.InvokeEnumConnectionPointsNext(clonePtr, 1);
            resetCloneHr = Native.InvokeEnumReset(clonePtr);
            cloneAfterReset = Native.InvokeEnumConnectionPointsNext(clonePtr, 1);
            connectionInterface = Native.InvokeGetConnectionInterface(first.Points[0]);
        }
        finally
        {
            Native.ReleasePointers(all.Points);
            Native.ReleasePointers(afterSkip.Points);
            Native.ReleasePointers(first.Points);
            Native.ReleasePointers(originalAfterReset.Points);
            Native.ReleasePointers(cloneAtEnd.Points);
            Native.ReleasePointers(cloneAfterReset.Points);
            Native.ReleaseIfNonZero(clonePtr);
            Native.ReleaseIfNonZero(queriedEnumPtr);
            Native.ReleaseIfNonZero(enumPtr);
            Native.ReleaseIfNonZero(cpPtr);
            Native.ReleaseIfNonZero(cpcPtr);
            Native.ReleaseIfNonZero(ccw);
        }

        await Assert.That(all.Hr).IsEqualTo(S_FALSE);
        await Assert.That(all.Fetched).IsEqualTo(1u);
        await Assert.That(all.Points[0]).IsEqualTo(cpPtr);
        await Assert.That(skipHr).IsEqualTo(S_OK);
        await Assert.That(afterSkip.Hr).IsEqualTo(S_FALSE);
        await Assert.That(afterSkip.Fetched).IsEqualTo(0u);
        await Assert.That(resetOriginalHr).IsEqualTo(S_OK);
        await Assert.That(first.Points[0]).IsEqualTo(cpPtr);
        await Assert.That(cloneHr).IsEqualTo(S_OK);
        await Assert.That(originalAfterReset.Points[0]).IsEqualTo(cpPtr);
        await Assert.That(cloneAtEnd.Hr).IsEqualTo(S_FALSE);
        await Assert.That(cloneAtEnd.Fetched).IsEqualTo(0u);
        await Assert.That(resetCloneHr).IsEqualTo(S_OK);
        await Assert.That(cloneAfterReset.Points[0]).IsEqualTo(cpPtr);
        await Assert.That(connectionInterface.Hr).IsEqualTo(S_OK);
        await Assert.That(connectionInterface.Value).IsEqualTo(IOPCHDA_DataCallback.InterfaceId);
    }

    [Test]
    public async Task IConnectionPoint_enumconnections_yields_active_data_callback_sinks()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr sink1 = Native.CreateDataCallbackStub();
        IntPtr sink2 = Native.CreateDataCallbackStub();
        IntPtr ccw = IntPtr.Zero;
        IntPtr cpcPtr = IntPtr.Zero;
        IntPtr cpPtr = IntPtr.Zero;
        IntPtr enumPtr = IntPtr.Zero;
        IntPtr queriedEnumPtr = IntPtr.Zero;
        IntPtr clonePtr = IntPtr.Zero;
        Native.AdviseResult advised1 = default;
        Native.AdviseResult advised2 = default;
        Native.ConnectionDataResult all = default;
        Native.ConnectionDataResult afterSkip = default;
        Native.ConnectionDataResult exhausted = default;
        Native.ConnectionDataResult first = default;
        Native.ConnectionDataResult originalAfterReset = default;
        Native.ConnectionDataResult cloneAtSavedCursor = default;
        int skipHr;
        int resetHr;
        int skipBeyondHr;
        int cloneHr;

        try
        {
            ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);
            cpcPtr = Native.InvokeQI(ccw, OpcGuids.IID_IConnectionPointContainer);
            cpPtr = Native.InvokeFindConnectionPoint(cpcPtr, IOPCHDA_DataCallback.InterfaceId).Pointer;
            advised1 = Native.InvokeAdvise(cpPtr, sink1);
            advised2 = Native.InvokeAdvise(cpPtr, sink2);
            enumPtr = Native.InvokeEnumConnections(cpPtr).Pointer;
            queriedEnumPtr = Native.InvokeQI(enumPtr, OpcGuids.IID_IEnumConnections);

            all = Native.InvokeEnumConnectionsNext(queriedEnumPtr, 3);
            Native.InvokeEnumReset(queriedEnumPtr);
            skipHr = Native.InvokeEnumSkip(queriedEnumPtr, 1);
            afterSkip = Native.InvokeEnumConnectionsNext(queriedEnumPtr, 1);
            resetHr = Native.InvokeEnumReset(queriedEnumPtr);
            skipBeyondHr = Native.InvokeEnumSkip(queriedEnumPtr, 99);
            exhausted = Native.InvokeEnumConnectionsNext(queriedEnumPtr, 1);
            Native.InvokeEnumReset(queriedEnumPtr);
            first = Native.InvokeEnumConnectionsNext(queriedEnumPtr, 1);
            (cloneHr, clonePtr) = Native.InvokeEnumClone(queriedEnumPtr);
            Native.InvokeEnumReset(queriedEnumPtr);
            originalAfterReset = Native.InvokeEnumConnectionsNext(queriedEnumPtr, 1);
            cloneAtSavedCursor = Native.InvokeEnumConnectionsNext(clonePtr, 1);
        }
        finally
        {
            Native.ReleaseConnections(all.Connections);
            Native.ReleaseConnections(afterSkip.Connections);
            Native.ReleaseConnections(exhausted.Connections);
            Native.ReleaseConnections(first.Connections);
            Native.ReleaseConnections(originalAfterReset.Connections);
            Native.ReleaseConnections(cloneAtSavedCursor.Connections);
            Native.ReleaseIfNonZero(clonePtr);
            Native.ReleaseIfNonZero(queriedEnumPtr);
            Native.ReleaseIfNonZero(enumPtr);
            if (advised1.Cookie != 0)
            {
                _ = Native.InvokeUnadvise(cpPtr, advised1.Cookie);
            }
            if (advised2.Cookie != 0)
            {
                _ = Native.InvokeUnadvise(cpPtr, advised2.Cookie);
            }
            Native.ReleaseIfNonZero(cpPtr);
            Native.ReleaseIfNonZero(cpcPtr);
            Native.ReleaseIfNonZero(ccw);
            Native.DestroyDataCallbackStub(sink1);
            Native.DestroyDataCallbackStub(sink2);
        }

        await Assert.That(advised1.Hr).IsEqualTo(S_OK);
        await Assert.That(advised2.Hr).IsEqualTo(S_OK);
        await Assert.That(all.Hr).IsEqualTo(S_FALSE);
        await Assert.That(all.Fetched).IsEqualTo(2u);
        await Assert.That(all.Connections.Select(static connection => connection.Cookie))
            .IsEquivalentTo(new[] { advised1.Cookie, advised2.Cookie });
        await Assert.That(all.Connections.All(connection => connection.Unknown == sink1 || connection.Unknown == sink2)).IsTrue();
        await Assert.That(skipHr).IsEqualTo(S_OK);
        await Assert.That(afterSkip.Connections[0].Cookie).IsEqualTo(advised2.Cookie);
        await Assert.That(resetHr).IsEqualTo(S_OK);
        await Assert.That(skipBeyondHr).IsEqualTo(S_FALSE);
        await Assert.That(exhausted.Hr).IsEqualTo(S_FALSE);
        await Assert.That(exhausted.Fetched).IsEqualTo(0u);
        await Assert.That(first.Connections[0].Cookie).IsEqualTo(advised1.Cookie);
        await Assert.That(cloneHr).IsEqualTo(S_OK);
        await Assert.That(originalAfterReset.Connections[0].Cookie).IsEqualTo(advised1.Cookie);
        await Assert.That(cloneAtSavedCursor.Connections[0].Cookie).IsEqualTo(advised2.Cookie);
    }

    private sealed class StubHdaServer : IOpcHdaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds.Length]);
    }

    private static unsafe class Native
    {
        private const int DataCallbackVtableSlotCount = 12;
        private static readonly ConcurrentDictionary<IntPtr, DataCallbackStubSession> s_dataCallbackStubs = new();

        internal readonly record struct GuidResult(int Hr, Guid Value);
        internal readonly record struct PointerResult(int Hr, IntPtr Pointer);
        internal readonly record struct AdviseResult(int Hr, int Cookie);
        internal readonly record struct ConnectionDataResult(int Hr, uint Fetched, NativeConnectionData[] Connections);
        internal readonly record struct NativeConnectionData(IntPtr Unknown, int Cookie);
        internal readonly record struct ConnectionPointsResult(int Hr, uint Fetched, IntPtr[] Points);

        [StructLayout(LayoutKind.Sequential)]
        private struct CONNECTDATA_NATIVE
        {
            public IntPtr pUnk;
            public uint dwCookie;
        }

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var qi = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
            Guid local = iid;
            IntPtr returned;
            int hr = qi(ccw, &local, &returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static void ReleaseIfNonZero(IntPtr ccw)
        {
            if (ccw == IntPtr.Zero)
            {
                return;
            }

            IntPtr* vtable = *(IntPtr**)ccw;
            var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
            release(ccw);
        }

        internal static void ReleaseConnections(NativeConnectionData[]? connections)
        {
            if (connections is null)
            {
                return;
            }

            foreach (NativeConnectionData connection in connections)
            {
                ReleaseIfNonZero(connection.Unknown);
            }
        }

        internal static void ReleasePointers(IntPtr[]? pointers)
        {
            if (pointers is null)
            {
                return;
            }

            foreach (IntPtr pointer in pointers)
            {
                ReleaseIfNonZero(pointer);
            }
        }

        internal static GuidResult InvokeGetConnectionInterface(IntPtr cpPtr)
        {
            IntPtr* vtable = *(IntPtr**)cpPtr;
            var getInterface = (delegate* unmanaged<IntPtr, Guid*, int>)vtable[3];
            Guid iid;
            int hr = getInterface(cpPtr, &iid);
            return new GuidResult(hr, iid);
        }

        internal static PointerResult InvokeFindConnectionPoint(IntPtr cpcPtr, Guid iid)
        {
            IntPtr* vtable = *(IntPtr**)cpcPtr;
            var find = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[4];
            Guid local = iid;
            IntPtr pointer;
            int hr = find(cpcPtr, &local, &pointer);
            return new PointerResult(hr, pointer);
        }

        internal static AdviseResult InvokeAdvise(IntPtr cpPtr, IntPtr sink)
        {
            IntPtr* vtable = *(IntPtr**)cpPtr;
            var advise = (delegate* unmanaged<IntPtr, IntPtr, uint*, int>)vtable[5];
            uint cookie;
            int hr = advise(cpPtr, sink, &cookie);
            return new AdviseResult(hr, unchecked((int)cookie));
        }

        internal static int InvokeUnadvise(IntPtr cpPtr, int cookie)
        {
            IntPtr* vtable = *(IntPtr**)cpPtr;
            var unadvise = (delegate* unmanaged<IntPtr, uint, int>)vtable[6];
            return unadvise(cpPtr, unchecked((uint)cookie));
        }

        internal static PointerResult InvokeEnumConnections(IntPtr cpPtr)
        {
            IntPtr* vtable = *(IntPtr**)cpPtr;
            var enumConnections = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[7];
            IntPtr pointer;
            int hr = enumConnections(cpPtr, &pointer);
            return new PointerResult(hr, pointer);
        }

        internal static PointerResult InvokeEnumConnectionPoints(IntPtr cpcPtr)
        {
            IntPtr* vtable = *(IntPtr**)cpcPtr;
            var enumConnectionPoints = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[3];
            IntPtr pointer;
            int hr = enumConnectionPoints(cpcPtr, &pointer);
            return new PointerResult(hr, pointer);
        }

        internal static ConnectionDataResult InvokeEnumConnectionsNext(IntPtr enumPtr, uint count)
        {
            IntPtr* vtable = *(IntPtr**)enumPtr;
            var next = (delegate* unmanaged<IntPtr, uint, CONNECTDATA_NATIVE*, uint*, int>)vtable[3];
            int elementCount = checked((int)count);
            int byteCount = checked(elementCount * Marshal.SizeOf<CONNECTDATA_NATIVE>());
            IntPtr buffer = Marshal.AllocCoTaskMem(byteCount);
            try
            {
                uint fetched;
                int hr = next(enumPtr, count, (CONNECTDATA_NATIVE*)buffer, &fetched);
                return new ConnectionDataResult(hr, fetched, ReadConnectionData(buffer, (int)fetched));
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        internal static ConnectionPointsResult InvokeEnumConnectionPointsNext(IntPtr enumPtr, uint count)
        {
            IntPtr* vtable = *(IntPtr**)enumPtr;
            var next = (delegate* unmanaged<IntPtr, uint, IntPtr*, uint*, int>)vtable[3];
            int elementCount = checked((int)count);
            int byteCount = checked(elementCount * IntPtr.Size);
            IntPtr buffer = Marshal.AllocCoTaskMem(byteCount);
            try
            {
                uint fetched;
                int hr = next(enumPtr, count, (IntPtr*)buffer, &fetched);
                return new ConnectionPointsResult(hr, fetched, ReadConnectionPoints(buffer, (int)fetched));
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        internal static int InvokeEnumSkip(IntPtr enumPtr, uint count)
        {
            IntPtr* vtable = *(IntPtr**)enumPtr;
            var skip = (delegate* unmanaged<IntPtr, uint, int>)vtable[4];
            return skip(enumPtr, count);
        }

        internal static int InvokeEnumReset(IntPtr enumPtr)
        {
            IntPtr* vtable = *(IntPtr**)enumPtr;
            var reset = (delegate* unmanaged<IntPtr, int>)vtable[5];
            return reset(enumPtr);
        }

        internal static (int Hr, IntPtr Pointer) InvokeEnumClone(IntPtr enumPtr)
        {
            IntPtr* vtable = *(IntPtr**)enumPtr;
            var clone = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[6];
            IntPtr pointer;
            int hr = clone(enumPtr, &pointer);
            return (hr, pointer);
        }

        internal static IntPtr CreateDataCallbackStub()
        {
            IntPtr* vtable = AllocateDataCallbackStubVtable();
            IntPtr instance = AllocateDataCallbackStubInstance(vtable);
            s_dataCallbackStubs[instance] = new DataCallbackStubSession(vtable);
            return instance;
        }

        internal static void DestroyDataCallbackStub(IntPtr stub)
        {
            if (!s_dataCallbackStubs.TryRemove(stub, out DataCallbackStubSession? session))
            {
                return;
            }

            NativeMemory.Free((void*)stub);
            NativeMemory.Free(session.Vtable);
        }

        private static NativeConnectionData[] ReadConnectionData(IntPtr buffer, int count)
        {
            var connections = new NativeConnectionData[count];
            for (int i = 0; i < count; i++)
            {
                IntPtr current = IntPtr.Add(buffer, i * Marshal.SizeOf<CONNECTDATA_NATIVE>());
                CONNECTDATA_NATIVE native = Marshal.PtrToStructure<CONNECTDATA_NATIVE>(current);
                connections[i] = new NativeConnectionData(native.pUnk, unchecked((int)native.dwCookie));
            }

            return connections;
        }

        private static IntPtr[] ReadConnectionPoints(IntPtr buffer, int count)
        {
            var points = new IntPtr[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = Marshal.ReadIntPtr(buffer, i * IntPtr.Size);
            }

            return points;
        }

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr* AllocateDataCallbackStubVtable()
        {
            IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(DataCallbackVtableSlotCount * sizeof(IntPtr)));
            vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&DataCallbackStubQueryInterface;
            vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&DataCallbackStubAddRef;
            vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&DataCallbackStubRelease;
            vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnItems;
            vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnItems;
            vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnItems;
            vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnAttributes;
            vtable[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnItems;
            vtable[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnItems;
            vtable[9] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnItems;
            vtable[10] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnItems;
            vtable[11] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&DataCallbackStubOnCancelComplete;
            return vtable;
        }

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr AllocateDataCallbackStubInstance(IntPtr* vtable)
        {
            IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
            instance[0] = (IntPtr)vtable;
            return (IntPtr)instance;
        }

        [UnmanagedCallersOnly]
        private static int DataCallbackStubQueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
        {
            if (ppv == null)
            {
                return unchecked((int)0x80070057);
            }

            if (!s_dataCallbackStubs.TryGetValue(pThis, out DataCallbackStubSession? session) || riid == null)
            {
                *ppv = IntPtr.Zero;
                return global::Opc.Classic.OpcResultId.NoInterface.Code;
            }

            if (*riid == IID_IUnknown || *riid == IOPCHDA_DataCallback.InterfaceId)
            {
                *ppv = pThis;
                Interlocked.Increment(ref session.RefCount);
                return S_OK;
            }

            *ppv = IntPtr.Zero;
            return global::Opc.Classic.OpcResultId.NoInterface.Code;
        }

        [UnmanagedCallersOnly]
        private static uint DataCallbackStubAddRef(IntPtr pThis)
        {
            if (!s_dataCallbackStubs.TryGetValue(pThis, out DataCallbackStubSession? session))
            {
                return 1;
            }

            return (uint)Interlocked.Increment(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        private static uint DataCallbackStubRelease(IntPtr pThis)
        {
            if (!s_dataCallbackStubs.TryGetValue(pThis, out DataCallbackStubSession? session))
            {
                return 0;
            }

            return (uint)Interlocked.Decrement(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        private static int DataCallbackStubOnItems(IntPtr pThis, uint transactionId, int status, uint count, IntPtr values, IntPtr errors)
        {
            _ = pThis;
            _ = transactionId;
            _ = status;
            _ = count;
            _ = values;
            _ = errors;
            return S_OK;
        }

        [UnmanagedCallersOnly]
        private static int DataCallbackStubOnAttributes(IntPtr pThis, uint transactionId, int status, uint clientHandle, uint count, IntPtr values, IntPtr errors)
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

        [UnmanagedCallersOnly]
        private static int DataCallbackStubOnCancelComplete(IntPtr pThis, uint cancelId)
        {
            _ = pThis;
            _ = cancelId;
            return S_OK;
        }

        private sealed class DataCallbackStubSession
        {
            public DataCallbackStubSession(IntPtr* vtable)
            {
                Vtable = vtable;
            }

            public IntPtr* Vtable { get; }
            public long RefCount = 1;
        }
    }
}
