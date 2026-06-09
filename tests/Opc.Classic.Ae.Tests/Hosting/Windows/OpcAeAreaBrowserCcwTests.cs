//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcAeAreaBrowserCcwTests {
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_NOTIMPL = unchecked((int)0x80004001);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task CreateAreaBrowser_returns_browser_pointer_from_dispatcher() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);

        (int hr, IntPtr browser) = Helpers.InvokeCreateAreaBrowser(eventServer, IOPCEventAreaBrowser.InterfaceId);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(browser).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(dispatcher.LastRequestedInterfaceId).IsEqualTo(IOPCEventAreaBrowser.InterfaceId);
    }

    [Test]
    public async Task CreateAreaBrowser_returns_E_NOTIMPL_when_dispatcher_has_no_browser() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        IntPtr eventServer = Helpers.CreateEventServer(new NoAreaBrowserDispatcher());

        (int hr, IntPtr browser) = Helpers.InvokeCreateAreaBrowser(eventServer, IOPCEventAreaBrowser.InterfaceId);

        await Assert.That(hr).IsEqualTo(E_NOTIMPL);
        await Assert.That(browser).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task ChangeBrowsePosition_dispatches_direction_and_position() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr browser = Helpers.CreateAreaBrowser(dispatcher);

        int hr = Helpers.InvokeChangeBrowsePosition(browser, browseDirection: 1, position: "AreaA");

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(dispatcher.LastBrowseDirection).IsEqualTo(1);
        await Assert.That(dispatcher.LastBrowsePosition).IsEqualTo("AreaA");
    }

    [Test]
    public async Task BrowseOPCAreas_returns_IEnumString_with_child_names() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        var dispatcher = new StubAeServerDispatcher { BrowseNames = ["AreaA", "AreaB"] };
        IntPtr browser = Helpers.CreateAreaBrowser(dispatcher);

        Helpers.BrowseResult result = Helpers.InvokeBrowseOPCAreas(browser, browseFilterType: 1, filterCriteria: "Area*");

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.EnumNextHr).IsEqualTo(S_FALSE);
        await Assert.That(result.Names).IsEquivalentTo(["AreaA", "AreaB"]);
        await Assert.That(dispatcher.LastBrowseFilterType).IsEqualTo(1);
        await Assert.That(dispatcher.LastFilterCriteria).IsEqualTo("Area*");
    }

    [Test]
    public async Task GetQualifiedAreaName_returns_BSTR() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr browser = Helpers.CreateAreaBrowser(dispatcher);

        (int hr, string? name) = Helpers.InvokeGetQualifiedAreaName(browser, "AreaA");

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(name).IsEqualTo("Plant1.AreaA");
        await Assert.That(dispatcher.LastAreaName).IsEqualTo("AreaA");
    }

    [Test]
    public async Task GetQualifiedSourceName_returns_BSTR() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        var dispatcher = new StubAeServerDispatcher();
        IntPtr browser = Helpers.CreateAreaBrowser(dispatcher);

        (int hr, string? name) = Helpers.InvokeGetQualifiedSourceName(browser, "Tank7");

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(name).IsEqualTo("Plant1.AreaA.Tank7");
        await Assert.That(dispatcher.LastSourceName).IsEqualTo("Tank7");
    }

    private sealed class StubAeServerDispatcher : IOpcAeServerDispatcher, IOpcAeAreaBrowserDispatcher {
        public Guid LastRequestedInterfaceId { get; private set; }
        public int LastBrowseDirection { get; private set; }
        public string? LastBrowsePosition { get; private set; }
        public int LastBrowseFilterType { get; private set; }
        public string? LastFilterCriteria { get; private set; }
        public string? LastAreaName { get; private set; }
        public string? LastSourceName { get; private set; }
        public string[] BrowseNames { get; init; } = [];

        public Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
            Task.FromResult(new NdrCallResult(E_NOTIMPL, ReadOnlyMemory<byte>.Empty));

        public Task<IOpcAeAreaBrowserDispatcher> CreateAreaBrowserAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default) {
            LastRequestedInterfaceId = requestedInterfaceId;
            return Task.FromResult<IOpcAeAreaBrowserDispatcher>(this);
        }

        public Task ChangeBrowsePositionAsync(int browseDirection, string? position, CancellationToken cancellationToken = default) {
            LastBrowseDirection = browseDirection;
            LastBrowsePosition = position;
            return Task.CompletedTask;
        }

        public Task<string[]> BrowseAreasAsync(int browseFilterType, string filterCriteria, CancellationToken cancellationToken = default) {
            LastBrowseFilterType = browseFilterType;
            LastFilterCriteria = filterCriteria;
            return Task.FromResult(BrowseNames);
        }

        public Task<string> GetQualifiedAreaNameAsync(string areaName, CancellationToken cancellationToken = default) {
            LastAreaName = areaName;
            return Task.FromResult($"Plant1.{areaName}");
        }

        public Task<string> GetQualifiedSourceNameAsync(string sourceName, CancellationToken cancellationToken = default) {
            LastSourceName = sourceName;
            return Task.FromResult($"Plant1.AreaA.{sourceName}");
        }
    }

    private sealed class NoAreaBrowserDispatcher : IOpcAeServerDispatcher {
        public Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
            Task.FromResult(new NdrCallResult(E_NOTIMPL, ReadOnlyMemory<byte>.Empty));
    }

    private static class Helpers {
        internal readonly record struct BrowseResult(int Hr, int EnumNextHr, string[] Names);

        internal static IntPtr CreateEventServer(IOpcAeServerDispatcher dispatcher) {
            IntPtr ccw = OpcAeServerCcw.Create(dispatcher, IID_IUnknown);
            return InvokeQI(ccw, IOPCEventServer.InterfaceId);
        }

        internal static IntPtr CreateAreaBrowser(IOpcAeServerDispatcher dispatcher) {
            IntPtr eventServer = CreateEventServer(dispatcher);
            (int hr, IntPtr browser) = InvokeCreateAreaBrowser(eventServer, IOPCEventAreaBrowser.InterfaceId);
            if (hr != S_OK) {
                throw new InvalidOperationException($"CreateAreaBrowser failed with 0x{hr:X8}.");
            }
            return browser;
        }

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid) {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static (int Hr, IntPtr Browser) InvokeCreateAreaBrowser(IntPtr eventServer, Guid iid) {
            CreateAreaBrowserDelegate create = GetMethod<CreateAreaBrowserDelegate>(eventServer, 18);
            int hr = create(eventServer, ref iid, out IntPtr browser);
            return (hr, browser);
        }

        internal static int InvokeChangeBrowsePosition(IntPtr browser, int browseDirection, string position) {
            ChangeBrowsePositionDelegate change = GetMethod<ChangeBrowsePositionDelegate>(browser, 3);
            IntPtr positionPtr = Marshal.StringToCoTaskMemUni(position);
            try {
                return change(browser, browseDirection, positionPtr);
            }
            finally {
                Marshal.FreeCoTaskMem(positionPtr);
            }
        }

        internal static BrowseResult InvokeBrowseOPCAreas(IntPtr browser, int browseFilterType, string filterCriteria) {
            BrowseOPCAreasDelegate browse = GetMethod<BrowseOPCAreasDelegate>(browser, 4);
            IntPtr criteriaPtr = Marshal.StringToCoTaskMemUni(filterCriteria);
            try {
                int hr = browse(browser, browseFilterType, criteriaPtr, out IntPtr enumString);
                if (hr != S_OK || enumString == IntPtr.Zero) {
                    return new BrowseResult(hr, 0, []);
                }
                (int nextHr, string[] names) = ReadEnumString(enumString, 8);
                return new BrowseResult(hr, nextHr, names);
            }
            finally {
                Marshal.FreeCoTaskMem(criteriaPtr);
            }
        }

        internal static (int Hr, string? Name) InvokeGetQualifiedAreaName(IntPtr browser, string areaName) {
            GetQualifiedNameDelegate getName = GetMethod<GetQualifiedNameDelegate>(browser, 5);
            return InvokeGetQualifiedName(browser, areaName, getName);
        }

        internal static (int Hr, string? Name) InvokeGetQualifiedSourceName(IntPtr browser, string sourceName) {
            GetQualifiedNameDelegate getName = GetMethod<GetQualifiedNameDelegate>(browser, 6);
            return InvokeGetQualifiedName(browser, sourceName, getName);
        }

        private static (int Hr, string? Name) InvokeGetQualifiedName(IntPtr browser, string name, GetQualifiedNameDelegate getName) {
            IntPtr namePtr = Marshal.StringToCoTaskMemUni(name);
            try {
                int hr = getName(browser, namePtr, out IntPtr qualifiedNamePtr);
                string? qualifiedName = qualifiedNamePtr == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(qualifiedNamePtr);
                if (qualifiedNamePtr != IntPtr.Zero) {
                    Marshal.FreeBSTR(qualifiedNamePtr);
                }
                return (hr, qualifiedName);
            }
            finally {
                Marshal.FreeCoTaskMem(namePtr);
            }
        }

        private static (int Hr, string[] Names) ReadEnumString(IntPtr enumString, int requested) {
            EnumStringNextDelegate next = GetMethod<EnumStringNextDelegate>(enumString, 3);
            IntPtr strings = Marshal.AllocCoTaskMem(requested * IntPtr.Size);
            try {
                for (int i = 0; i < requested; i++) {
                    Marshal.WriteIntPtr(strings, i * IntPtr.Size, IntPtr.Zero);
                }

                int hr = next(enumString, (uint)requested, strings, out uint fetched);
                var names = new string[fetched];
                for (int i = 0; i < fetched; i++) {
                    IntPtr valuePtr = Marshal.ReadIntPtr(strings, i * IntPtr.Size);
                    names[i] = Marshal.PtrToStringUni(valuePtr) ?? string.Empty;
                    Marshal.FreeCoTaskMem(valuePtr);
                }
                return (hr, names);
            }
            finally {
                Marshal.FreeCoTaskMem(strings);
            }
        }

        private static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CreateAreaBrowserDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppUnk);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ChangeBrowsePositionDelegate(IntPtr pThis, int browseDirection, IntPtr position);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int BrowseOPCAreasDelegate(IntPtr pThis, int browseFilterType, IntPtr filterCriteria, out IntPtr ppEnumString);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetQualifiedNameDelegate(IntPtr pThis, IntPtr name, out IntPtr qualifiedName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int EnumStringNextDelegate(IntPtr pThis, uint celt, IntPtr rgelt, out uint pceltFetched);
    }
}
