// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Hosting.Windows;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

/// <summary>
/// Windows-only tests for the AE server CCW per-method vtables.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcAeServerCcwMethodsTests
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task QueryInterface_for_each_supported_iid_returns_nonzero_tearoff()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcAeServerCcw.Create(new StubAeServer(), IID_IUnknown);
        IntPtr eventServer = Helpers.InvokeQI(ccw, IOPCEventServer.InterfaceId);
        IntPtr subscription = Helpers.InvokeQI(ccw, IOPCEventSubscriptionMgt.InterfaceId);

        await Assert.That(eventServer).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(subscription).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(eventServer).IsNotEqualTo(ccw);
        await Assert.That(subscription).IsNotEqualTo(ccw);
    }

    [Test]
    public async Task QueryInterface_for_IUnknown_on_any_tearoff_returns_canonical_identity()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcAeServerCcw.Create(new StubAeServer(), IID_IUnknown);
        IntPtr eventServer = Helpers.InvokeQI(ccw, IOPCEventServer.InterfaceId);
        IntPtr subscription = Helpers.InvokeQI(ccw, IOPCEventSubscriptionMgt.InterfaceId);

        IntPtr fromEventServer = Helpers.InvokeQI(eventServer, IID_IUnknown);
        IntPtr fromSubscription = Helpers.InvokeQI(subscription, IID_IUnknown);

        await Assert.That(fromEventServer).IsEqualTo(ccw);
        await Assert.That(fromSubscription).IsEqualTo(ccw);
    }

    [Test]
    public async Task QueryInterface_for_unsupported_iid_returns_E_NOINTERFACE()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcAeServerCcw.Create(new StubAeServer(), IID_IUnknown);
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

        IntPtr ccw = OpcAeServerCcw.Create(new StubAeServer(), IID_IUnknown);
        Helpers.InvokeRelease(ccw);

        await Assert.That(OpcAeServerCcw.GetReferenceCount(ccw)).IsEqualTo(-1L);
    }

    [Test]
    public async Task GetStatus_dispatches_through_managed_server()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new StubAeServer();
        IntPtr ccw = OpcAeServerCcw.Create(server, IID_IUnknown);
        IntPtr eventServer = Helpers.InvokeQI(ccw, IOPCEventServer.InterfaceId);

        Helpers.GetStatusResult result = Helpers.InvokeGetStatus(eventServer);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.StartTime).IsEqualTo(server.Status.StartTime.ToFileTime());
        await Assert.That(result.CurrentTime).IsEqualTo(server.Status.CurrentTime.ToFileTime());
        await Assert.That(result.LastUpdateTime).IsEqualTo(server.Status.LastUpdateTime.ToFileTime());
        await Assert.That(result.State).IsEqualTo((int)server.Status.State);
        await Assert.That(result.Version).IsEqualTo((2, 5, 7));
        await Assert.That(result.VendorInfo).IsEqualTo("AE test vendor");
    }

    [Test]
    public async Task QueryAvailableFilters_dispatches_through_managed_server()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new StubAeServer { FilterMask = 0x55 };
        IntPtr ccw = OpcAeServerCcw.Create(server, IID_IUnknown);
        IntPtr eventServer = Helpers.InvokeQI(ccw, IOPCEventServer.InterfaceId);

        (int hr, int filterMask) = Helpers.InvokeQueryAvailableFilters(eventServer);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(filterMask).IsEqualTo(0x55);
    }

    [Test]
    public async Task SubscriptionGetState_dispatches_through_managed_subscription()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new StubAeServer();
        IntPtr ccw = OpcAeServerCcw.Create(server, IID_IUnknown);
        IntPtr subscription = Helpers.InvokeQI(ccw, IOPCEventSubscriptionMgt.InterfaceId);

        Helpers.SubscriptionStateResult result = Helpers.InvokeGetState(subscription);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Active).IsEqualTo(1);
        await Assert.That(result.BufferTime).IsEqualTo(100);
        await Assert.That(result.MaxSize).IsEqualTo(25);
        await Assert.That(result.ClientSubscription).IsEqualTo(400);
    }

    [Test]
    public async Task SubscriptionSetState_dispatches_through_managed_subscription()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new StubAeServer();
        IntPtr ccw = OpcAeServerCcw.Create(server, IID_IUnknown);
        IntPtr subscription = Helpers.InvokeQI(ccw, IOPCEventSubscriptionMgt.InterfaceId);

        Helpers.SetStateResult result = Helpers.InvokeSetState(subscription, active: false, bufferTime: 250, maxSize: 44, clientSubscription: 900);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.RevisedBufferTime).IsEqualTo(251);
        await Assert.That(result.RevisedMaxSize).IsEqualTo(46);
        await Assert.That(server.Active).IsFalse();
        await Assert.That(server.BufferTime).IsEqualTo(250);
        await Assert.That(server.MaxSize).IsEqualTo(44);
        await Assert.That(server.ClientSubscription).IsEqualTo(900);
    }

    private sealed class StubAeServer : IOpcAeServer, IOPCEventSubscriptionMgt
    {
        public OpcServerStatus Status { get; } = new()
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            CurrentTime = new DateTimeOffset(2026, 1, 2, 3, 4, 6, TimeSpan.Zero),
            LastUpdateTime = new DateTimeOffset(2026, 1, 2, 3, 4, 7, TimeSpan.Zero),
            State = OpcServerState.Running,
            ServerVersion = new Version(2, 5, 7),
            VendorInfo = "AE test vendor",
        };

        public int FilterMask { get; init; } = 0x13;
        public bool Active { get; private set; } = true;
        public int BufferTime { get; private set; } = 100;
        public int MaxSize { get; private set; } = 25;
        public int ClientSubscription { get; private set; } = 400;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Status);

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(FilterMask);

        public Task SetFilterAsync(int eventType, int[] eventCategories, int lowSeverity, int highSeverity, string[] areas, string[] sources, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task GetFilterAsync(out int eventType, out int[] eventCategories, out int lowSeverity, out int highSeverity, out string[] areas, out string[] sources, CancellationToken cancellationToken = default)
        {
            eventType = 0;
            eventCategories = Array.Empty<int>();
            lowSeverity = 0;
            highSeverity = 0;
            areas = Array.Empty<string>();
            sources = Array.Empty<string>();
            return Task.CompletedTask;
        }

        public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default) =>
            Task.FromResult(Array.Empty<int>());

        public Task RefreshAsync(int connection, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken cancellationToken = default)
        {
            active = Active;
            bufferTime = BufferTime;
            maxSize = MaxSize;
            clientSubscription = ClientSubscription;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(bool active, int bufferTime, int maxSize, int clientSubscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken cancellationToken = default)
        {
            Active = active;
            BufferTime = bufferTime;
            MaxSize = maxSize;
            ClientSubscription = clientSubscription;
            revisedBufferTime = bufferTime + 1;
            revisedMaxSize = maxSize + 2;
            return Task.CompletedTask;
        }
    }

    private static class Helpers
    {
        internal readonly record struct GetStatusResult(int Hr, long StartTime, long CurrentTime, long LastUpdateTime, int State, (int Major, int Minor, int Build) Version, string? VendorInfo);
        internal readonly record struct SubscriptionStateResult(int Hr, int Active, int BufferTime, int MaxSize, int ClientSubscription);
        internal readonly record struct SetStateResult(int Hr, int RevisedBufferTime, int RevisedMaxSize);

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

        internal static GetStatusResult InvokeGetStatus(IntPtr eventServer)
        {
            GetStatusDelegate getStatus = GetMethod<GetStatusDelegate>(eventServer, 3);
            int hr = getStatus(eventServer, out IntPtr statusPtr);
            if (statusPtr == IntPtr.Zero)
            {
                return new GetStatusResult(hr, 0, 0, 0, 0, default, null);
            }

            var native = Marshal.PtrToStructure<OPCEVENTSERVERSTATUS_NATIVE>(statusPtr);
            string? vendorInfo = Marshal.PtrToStringUni(native.szVendorInfo);
            if (native.szVendorInfo != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(native.szVendorInfo);
            }
            Marshal.FreeCoTaskMem(statusPtr);
            return new GetStatusResult(hr, native.ftStartTime, native.ftCurrentTime, native.ftLastUpdateTime, native.dwServerState, (native.wMajorVersion, native.wMinorVersion, native.wBuildNumber), vendorInfo);
        }

        internal static (int Hr, int FilterMask) InvokeQueryAvailableFilters(IntPtr eventServer)
        {
            QueryAvailableFiltersDelegate queryFilters = GetMethod<QueryAvailableFiltersDelegate>(eventServer, 5);
            IntPtr pFilterMask = Marshal.AllocCoTaskMem(sizeof(int));
            try
            {
                int hr = queryFilters(eventServer, pFilterMask);
                return (hr, Marshal.ReadInt32(pFilterMask));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pFilterMask);
            }
        }

        internal static SubscriptionStateResult InvokeGetState(IntPtr subscription)
        {
            GetStateDelegate getState = GetMethod<GetStateDelegate>(subscription, 9);
            IntPtr pActive = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pClientSubscription = Marshal.AllocCoTaskMem(sizeof(int));
            try
            {
                int hr = getState(subscription, pActive, pBufferTime, pMaxSize, pClientSubscription);
                return new SubscriptionStateResult(hr, Marshal.ReadInt32(pActive), Marshal.ReadInt32(pBufferTime), Marshal.ReadInt32(pMaxSize), Marshal.ReadInt32(pClientSubscription));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pActive);
                Marshal.FreeCoTaskMem(pBufferTime);
                Marshal.FreeCoTaskMem(pMaxSize);
                Marshal.FreeCoTaskMem(pClientSubscription);
            }
        }

        internal static SetStateResult InvokeSetState(IntPtr subscription, bool active, int bufferTime, int maxSize, int clientSubscription)
        {
            SetStateDelegate setState = GetMethod<SetStateDelegate>(subscription, 10);
            IntPtr pActive = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pRevisedBufferTime = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pRevisedMaxSize = Marshal.AllocCoTaskMem(sizeof(int));
            try
            {
                Marshal.WriteInt32(pActive, active ? 1 : 0);
                Marshal.WriteInt32(pBufferTime, bufferTime);
                Marshal.WriteInt32(pMaxSize, maxSize);
                int hr = setState(subscription, pActive, pBufferTime, pMaxSize, clientSubscription, pRevisedBufferTime, pRevisedMaxSize);
                return new SetStateResult(hr, Marshal.ReadInt32(pRevisedBufferTime), Marshal.ReadInt32(pRevisedMaxSize));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pActive);
                Marshal.FreeCoTaskMem(pBufferTime);
                Marshal.FreeCoTaskMem(pMaxSize);
                Marshal.FreeCoTaskMem(pRevisedBufferTime);
                Marshal.FreeCoTaskMem(pRevisedMaxSize);
            }
        }

        private static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate
        {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint ReleaseDelegate(IntPtr pThis);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetStatusDelegate(IntPtr pThis, out IntPtr ppStatus);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryAvailableFiltersDelegate(IntPtr pThis, IntPtr pFilterMask);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetStateDelegate(IntPtr pThis, IntPtr pActive, IntPtr pBufferTime, IntPtr pMaxSize, IntPtr pClientSubscription);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetStateDelegate(IntPtr pThis, IntPtr pActive, IntPtr pBufferTime, IntPtr pMaxSize, int clientSubscription, IntPtr pRevisedBufferTime, IntPtr pRevisedMaxSize);

        // Mirror production OpcAeServerCcwMethods.OPCEVENTSERVERSTATUS_NATIVE
        // which uses natural alignment (no Pack) — see DR7 fix.
        [StructLayout(LayoutKind.Sequential)]
        private struct OPCEVENTSERVERSTATUS_NATIVE
        {
            public long ftStartTime;
            public long ftCurrentTime;
            public long ftLastUpdateTime;
            public int dwServerState;
            public ushort wMajorVersion;
            public ushort wMinorVersion;
            public ushort wBuildNumber;
            public ushort wReserved;
            public IntPtr szVendorInfo;
        }
    }
}
