//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

/// <summary>Windows-only method dispatch tests for <see cref="OpcHdaServerCcw" />.</summary>
[SupportedOSPlatform("windows")]
public sealed class OpcHdaServerCcwMethodsTests
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_NOTIMPL = unchecked((int)0x80004001);
    private const int E_FAIL = unchecked((int)0x80004005);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task QueryInterface_for_supported_iids_returns_nonzero_tearoffs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);
        IntPtr server = Helpers.InvokeQI(ccw, IOPCHDA_Server.InterfaceId);
        IntPtr syncRead = Helpers.InvokeQI(ccw, IOPCHDA_SyncRead.InterfaceId);
        IntPtr asyncRead = Helpers.InvokeQI(ccw, IOPCHDA_AsyncRead.InterfaceId);

        await Assert.That(server).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(syncRead).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(asyncRead).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(server).IsNotEqualTo(ccw);
    }

    [Test]
    public async Task QueryInterface_for_IUnknown_on_any_tearoff_returns_canonical_identity()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);
        IntPtr server = Helpers.InvokeQI(ccw, IOPCHDA_Server.InterfaceId);
        IntPtr syncRead = Helpers.InvokeQI(ccw, IOPCHDA_SyncRead.InterfaceId);
        IntPtr asyncRead = Helpers.InvokeQI(ccw, IOPCHDA_AsyncRead.InterfaceId);

        await Assert.That(Helpers.InvokeQI(server, IID_IUnknown)).IsEqualTo(ccw);
        await Assert.That(Helpers.InvokeQI(syncRead, IID_IUnknown)).IsEqualTo(ccw);
        await Assert.That(Helpers.InvokeQI(asyncRead, IID_IUnknown)).IsEqualTo(ccw);
    }

    [Test]
    public async Task QueryInterface_for_unsupported_iid_returns_E_NOINTERFACE()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);
        (int hr, IntPtr returned) = Helpers.InvokeQIRaw(ccw, Guid.NewGuid());

        await Assert.That(hr).IsEqualTo(E_NOINTERFACE);
        await Assert.That(returned).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Release_to_zero_removes_ccw_from_registry()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);
        Helpers.InvokeRelease(ccw);

        await Assert.That(OpcHdaServerCcw.GetReferenceCount(ccw)).IsEqualTo(-1L);
    }

    [Test]
    public async Task GetHistorianStatus_dispatches_through_managed_server()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new StubHdaServer();
        IntPtr ccw = OpcHdaServerCcw.Create(stub, IID_IUnknown);
        IntPtr server = Helpers.InvokeQI(ccw, IOPCHDA_Server.InterfaceId);
        Helpers.HistorianStatusResult result = Helpers.InvokeGetHistorianStatus(server);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(stub.StatusCalls).IsEqualTo(1);
        await Assert.That(result.Status).IsEqualTo(1);
        await Assert.That(result.MajorVersion).IsEqualTo((ushort)1);
        await Assert.That(result.MinorVersion).IsEqualTo((ushort)2);
        await Assert.That(result.BuildNumber).IsEqualTo((ushort)3);
        await Assert.That(result.MaxReturnValues).IsEqualTo(42u);
        await Assert.That(result.VendorInfo).IsEqualTo("Test HDA");
    }

    [Test]
    public async Task ValidateItemIDs_dispatches_and_returns_per_item_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new StubHdaServer();
        IntPtr ccw = OpcHdaServerCcw.Create(stub, IID_IUnknown);
        IntPtr server = Helpers.InvokeQI(ccw, IOPCHDA_Server.InterfaceId);
        Helpers.ErrorsResult result = Helpers.InvokeValidateItemIDs(server, new[] { "good", "bad" });

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(stub.LastValidatedItemIds![0]).IsEqualTo("good");
        await Assert.That(stub.LastValidatedItemIds[1]).IsEqualTo("bad");
        await Assert.That(result.Errors[0]).IsEqualTo(S_OK);
        await Assert.That(result.Errors[1]).IsEqualTo(E_FAIL);
    }

    [Test]
    public async Task GetItemHandles_and_ReleaseItemHandles_dispatch_with_int_array_marshaling()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new StubHdaServer();
        IntPtr ccw = OpcHdaServerCcw.Create(stub, IID_IUnknown);
        IntPtr server = Helpers.InvokeQI(ccw, IOPCHDA_Server.InterfaceId);
        Helpers.ItemHandlesResult handles = Helpers.InvokeGetItemHandles(server, new[] { "a", "b" }, new[] { 11, 12 });
        Helpers.ErrorsResult release = Helpers.InvokeReleaseItemHandles(server, handles.ServerHandles);

        await Assert.That(handles.Hr).IsEqualTo(S_OK);
        await Assert.That(handles.ServerHandles[0]).IsEqualTo(501);
        await Assert.That(handles.ServerHandles[1]).IsEqualTo(502);
        await Assert.That(stub.LastClientHandles![1]).IsEqualTo(12);
        await Assert.That(release.Hr).IsEqualTo(S_OK);
        await Assert.That(stub.LastReleasedHandles![0]).IsEqualTo(501);
        await Assert.That(release.Errors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
    }

    [Test]
    public async Task ReadRaw_tearoffs_exist_and_return_E_NOTIMPL_until_complex_marshaling_is_wired()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);
        IntPtr syncRead = Helpers.InvokeQI(ccw, IOPCHDA_SyncRead.InterfaceId);
        IntPtr asyncRead = Helpers.InvokeQI(ccw, IOPCHDA_AsyncRead.InterfaceId);

        await Assert.That(syncRead).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(asyncRead).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(Helpers.InvokeSyncReadRaw(syncRead)).IsEqualTo(E_NOTIMPL);
        await Assert.That(Helpers.InvokeAsyncReadRaw(asyncRead)).IsEqualTo(E_NOTIMPL);
    }

    private sealed class StubHdaServer : IOpcHdaServer
    {
        public int StatusCalls { get; private set; }

        public string[]? LastValidatedItemIds { get; private set; }

        public int[]? LastClientHandles { get; private set; }

        public int[]? LastReleasedHandles { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            StatusCalls++;
            return Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Hda,
                State = OpcServerState.Running,
                CurrentTime = DateTimeOffset.FromUnixTimeSeconds(20),
                StartTime = DateTimeOffset.FromUnixTimeSeconds(10),
                ServerVersion = new Version(1, 2, 3),
                MaxReturnValues = 42,
                VendorInfo = "Test HDA",
            });
        }

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
        {
            LastValidatedItemIds = itemIds;
            return Task.FromResult(new[] { S_OK, E_FAIL });
        }

        Task<int[]> IOPCHDA_Server.GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken)
        {
            LastValidatedItemIds = itemIds;
            LastClientHandles = clientHandles;
            return Task.FromResult(new[] { 501, 502 });
        }

        Task<int[]> IOPCHDA_Server.ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken)
        {
            LastReleasedHandles = serverHandles;
            return Task.FromResult(new[] { S_OK, OpcResultId.InvalidHandle.Code });
        }
    }

    private static class Helpers
    {
        internal readonly record struct HistorianStatusResult(
            int Hr,
            int Status,
            DateTimeOffset CurrentTime,
            DateTimeOffset StartTime,
            ushort MajorVersion,
            ushort MinorVersion,
            ushort BuildNumber,
            uint MaxReturnValues,
            string? StatusString,
            string? VendorInfo);

        internal readonly record struct ErrorsResult(int Hr, int[] Errors);

        internal readonly record struct ItemHandlesResult(int Hr, int[] ServerHandles, int[] Errors);

        private readonly record struct StringArrayAllocation(IntPtr Array, IntPtr[] Strings);

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static (int Hr, IntPtr Returned) InvokeQIRaw(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return (hr, returned);
        }

        internal static void InvokeRelease(IntPtr ccw)
        {
            ReleaseDelegate release = GetMethod<ReleaseDelegate>(ccw, 2);
            release(ccw);
        }

        internal static HistorianStatusResult InvokeGetHistorianStatus(IntPtr server)
        {
            GetHistorianStatusDelegate getStatus = GetMethod<GetHistorianStatusDelegate>(server, 5);
            using CoTaskMemBlock status = CoTaskMemBlock.Allocate(sizeof(int));
            using CoTaskMemBlock major = CoTaskMemBlock.Allocate(sizeof(ushort));
            using CoTaskMemBlock minor = CoTaskMemBlock.Allocate(sizeof(ushort));
            using CoTaskMemBlock build = CoTaskMemBlock.Allocate(sizeof(ushort));
            using CoTaskMemBlock max = CoTaskMemBlock.Allocate(sizeof(int));
            using CoTaskMemBlock statusString = CoTaskMemBlock.Allocate(IntPtr.Size);
            using CoTaskMemBlock vendor = CoTaskMemBlock.Allocate(IntPtr.Size);
            int hr = getStatus(server, status.Pointer, out IntPtr currentTime, out IntPtr startTime,
                major.Pointer, minor.Pointer, build.Pointer, max.Pointer, statusString.Pointer, vendor.Pointer);
            return ReadHistorianStatus(hr, status, currentTime, startTime, major, minor, build, max, statusString, vendor);
        }

        internal static ErrorsResult InvokeValidateItemIDs(IntPtr server, string[] itemIds)
        {
            ValidateItemIDsDelegate validate = GetMethod<ValidateItemIDsDelegate>(server, 8);
            StringArrayAllocation ids = AllocateStringPointerArray(itemIds);
            try
            {
                int hr = validate(server, (uint)itemIds.Length, ids.Array, out IntPtr ppErrors);
                return new ErrorsResult(hr, ReadAndFreeInt32Array(ppErrors, itemIds.Length));
            }
            finally
            {
                FreeStringPointerArray(ids);
            }
        }

        internal static ItemHandlesResult InvokeGetItemHandles(IntPtr server, string[] itemIds, int[] clientHandles)
        {
            GetItemHandlesDelegate getHandles = GetMethod<GetItemHandlesDelegate>(server, 6);
            StringArrayAllocation ids = AllocateStringPointerArray(itemIds);
            IntPtr pClientHandles = AllocateInt32Array(clientHandles);
            try
            {
                int hr = getHandles(server, (uint)itemIds.Length, ids.Array, pClientHandles, out IntPtr pServer, out IntPtr pErrors);
                int[] handles = ReadAndFreeInt32Array(pServer, itemIds.Length);
                int[] errors = ReadAndFreeInt32Array(pErrors, itemIds.Length);
                return new ItemHandlesResult(hr, handles, errors);
            }
            finally
            {
                FreeStringPointerArray(ids);
                Marshal.FreeCoTaskMem(pClientHandles);
            }
        }

        internal static ErrorsResult InvokeReleaseItemHandles(IntPtr server, int[] serverHandles)
        {
            ReleaseItemHandlesDelegate release = GetMethod<ReleaseItemHandlesDelegate>(server, 7);
            IntPtr pServerHandles = AllocateInt32Array(serverHandles);
            try
            {
                int hr = release(server, (uint)serverHandles.Length, pServerHandles, out IntPtr pErrors);
                return new ErrorsResult(hr, ReadAndFreeInt32Array(pErrors, serverHandles.Length));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pServerHandles);
            }
        }

        internal static int InvokeSyncReadRaw(IntPtr syncRead)
        {
            SyncReadRawDelegate readRaw = GetMethod<SyncReadRawDelegate>(syncRead, 3);
            int hr = readRaw(syncRead, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, IntPtr.Zero, out IntPtr items, out IntPtr errors);
            FreeIfNonZero(items);
            FreeIfNonZero(errors);
            return hr;
        }

        internal static int InvokeAsyncReadRaw(IntPtr asyncRead)
        {
            AsyncReadRawDelegate readRaw = GetMethod<AsyncReadRawDelegate>(asyncRead, 3);
            using CoTaskMemBlock cancel = CoTaskMemBlock.Allocate(sizeof(int));
            int hr = readRaw(asyncRead, 1, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, IntPtr.Zero, cancel.Pointer, out IntPtr errors);
            FreeIfNonZero(errors);
            return hr;
        }

        private static HistorianStatusResult ReadHistorianStatus(
            int hr,
            CoTaskMemBlock status,
            IntPtr currentTime,
            IntPtr startTime,
            CoTaskMemBlock major,
            CoTaskMemBlock minor,
            CoTaskMemBlock build,
            CoTaskMemBlock max,
            CoTaskMemBlock statusString,
            CoTaskMemBlock vendor)
        {
            IntPtr statusTextPtr = Marshal.ReadIntPtr(statusString.Pointer);
            IntPtr vendorPtr = Marshal.ReadIntPtr(vendor.Pointer);
            try
            {
                return new HistorianStatusResult(hr, Marshal.ReadInt32(status.Pointer), ReadFileTime(currentTime),
                    ReadFileTime(startTime), ReadUInt16(major.Pointer), ReadUInt16(minor.Pointer),
                    ReadUInt16(build.Pointer), unchecked((uint)Marshal.ReadInt32(max.Pointer)),
                    Marshal.PtrToStringUni(statusTextPtr), Marshal.PtrToStringUni(vendorPtr));
            }
            finally
            {
                FreeIfNonZero(currentTime);
                FreeIfNonZero(startTime);
                FreeIfNonZero(statusTextPtr);
                FreeIfNonZero(vendorPtr);
            }
        }

        private static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate
        {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        private static StringArrayAllocation AllocateStringPointerArray(string[] values)
        {
            IntPtr array = Marshal.AllocCoTaskMem(values.Length * IntPtr.Size);
            var strings = new IntPtr[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                strings[i] = Marshal.StringToCoTaskMemUni(values[i]);
                Marshal.WriteIntPtr(array, i * IntPtr.Size, strings[i]);
            }
            return new StringArrayAllocation(array, strings);
        }

        private static void FreeStringPointerArray(StringArrayAllocation allocation)
        {
            for (int i = 0; i < allocation.Strings.Length; i++)
            {
                FreeIfNonZero(allocation.Strings[i]);
            }
            FreeIfNonZero(allocation.Array);
        }

        private static IntPtr AllocateInt32Array(int[] values)
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
            if (values.Length > 0)
            {
                Marshal.Copy(values, 0, ptr, values.Length);
            }
            return ptr;
        }

        private static int[] ReadAndFreeInt32Array(IntPtr ptr, int count)
        {
            var values = new int[count];
            if (ptr != IntPtr.Zero && count > 0)
            {
                Marshal.Copy(ptr, values, 0, count);
            }
            FreeIfNonZero(ptr);
            return values;
        }

        private static DateTimeOffset ReadFileTime(IntPtr ptr)
        {
            long fileTime = ptr == IntPtr.Zero ? 0L : Marshal.ReadInt64(ptr);
            return fileTime == 0L ? default : DateTimeOffset.FromFileTime(fileTime);
        }

        private static ushort ReadUInt16(IntPtr ptr) => unchecked((ushort)Marshal.ReadInt16(ptr));

        private static void FreeIfNonZero(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate uint ReleaseDelegate(IntPtr pThis);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int GetHistorianStatusDelegate(
            IntPtr pThis,
            IntPtr pwStatus,
            out IntPtr pftCurrentTime,
            out IntPtr pftStartTime,
            IntPtr pwMajorVersion,
            IntPtr pwMinorVersion,
            IntPtr pwBuildNumber,
            IntPtr pdwMaxReturnValues,
            IntPtr ppszStatusString,
            IntPtr ppszVendorInfo);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int ValidateItemIDsDelegate(IntPtr pThis, uint dwCount, IntPtr pszItemID, out IntPtr ppErrors);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int GetItemHandlesDelegate(
            IntPtr pThis,
            uint dwCount,
            IntPtr pszItemID,
            IntPtr phClient,
            out IntPtr pphServer,
            out IntPtr ppErrors);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int ReleaseItemHandlesDelegate(IntPtr pThis, uint dwCount, IntPtr phServer, out IntPtr ppErrors);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int SyncReadRawDelegate(
            IntPtr pThis,
            IntPtr htStartTime,
            IntPtr htEndTime,
            uint dwNumValues,
            int bBounds,
            uint dwNumItems,
            IntPtr phServer,
            out IntPtr ppItemValues,
            out IntPtr ppErrors);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int AsyncReadRawDelegate(
            IntPtr pThis,
            uint dwTransactionID,
            IntPtr htStartTime,
            IntPtr htEndTime,
            uint dwNumValues,
            int bBounds,
            uint dwNumItems,
            IntPtr phServer,
            IntPtr pdwCancelID,
            out IntPtr ppErrors);

        private readonly struct CoTaskMemBlock : IDisposable
        {
            public CoTaskMemBlock(IntPtr pointer)
            {
                Pointer = pointer;
            }

            public IntPtr Pointer { get; }

            public static CoTaskMemBlock Allocate(int byteCount)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
                byte[] zero = new byte[byteCount];
                Marshal.Copy(zero, 0, ptr, byteCount);
                return new CoTaskMemBlock(ptr);
            }

            public void Dispose()
            {
                FreeIfNonZero(Pointer);
            }
        }
    }
}
