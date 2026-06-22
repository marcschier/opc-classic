// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>
/// Windows-only unit tests for <see cref="OpcDaServerCcw"/>. Exercises the
/// CCW lifecycle (Create / QueryInterface / AddRef / Release) and verifies
/// that all 9 IOPCServer vtable slots return E_NOTIMPL today.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcDaServerCcwTests
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_NOTIMPL = unchecked((int)0x80004001);
    private const int E_INVALIDARG = unchecked((int)0x80070057);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid IID_IClassFactory = Guid.Parse("00000001-0000-0000-C000-000000000046");

    [Test]
    public async Task Create_returns_zero_for_unsupported_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IClassFactory);

        await Assert.That(ccw).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IID_IUnknown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcDaServerCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IOPCServer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IOPCServer.InterfaceId);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcDaServerCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task QueryInterface_returns_same_pointer_for_supported_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);
        QueryInterfaceResult result = InvokeQueryInterface(ccw, IOPCServer.InterfaceId);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Returned).IsEqualTo(ccw);
        // QI implies AddRef => refcount went from 1 to 2.
        await Assert.That(OpcDaServerCcw.GetReferenceCount(ccw)).IsEqualTo(2L);
    }

    [Test]
    public async Task QueryInterface_returns_E_NOINTERFACE_for_unknown_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);
        QueryInterfaceResult result = InvokeQueryInterface(ccw, Guid.NewGuid());

        await Assert.That(result.Hr).IsEqualTo(E_NOINTERFACE);
        await Assert.That(result.Returned).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task AddRef_and_Release_drive_refcount()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);
        AddRefReleaseResult result = InvokeAddRefRelease(ccw);

        await Assert.That(result.After1stAddRef).IsEqualTo(2u);
        await Assert.That(result.After2ndAddRef).IsEqualTo(3u);
        await Assert.That(result.After1stRelease).IsEqualTo(2u);
        await Assert.That(result.After2ndRelease).IsEqualTo(1u);
    }

    [Test]
    public async Task GetStatus_via_CCW_returns_populated_OPCSERVERSTATUS_struct()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);

        (int hr, IntPtr statusPtr) = InvokeGetStatus(ccw);

        try
        {
            await Assert.That(hr).IsEqualTo(S_OK);
            await Assert.That(statusPtr).IsNotEqualTo(IntPtr.Zero);
            // Skim a few well-known offsets: StartTime (offset 0), State (offset 24).
            long startTime = System.Runtime.InteropServices.Marshal.ReadInt64(statusPtr, 0);
            int state = System.Runtime.InteropServices.Marshal.ReadInt32(statusPtr, 24);
            await Assert.That(startTime).IsGreaterThan(0L);
            await Assert.That(state).IsEqualTo((int)OpcServerState.Running);
        }
        finally
        {
            // Free the allocated CoTaskMem (caller-owned per OPC contract).
            // OPCSERVERSTATUS contains szVendorInfo LPWSTR at offset 48 (after
            // wReserved at off 42-43 + 4-byte padding to 8-align the pointer
            // on x64 -- natural alignment per DR7 fix to OPCSERVERSTATUS_NATIVE).
            IntPtr vendorInfoPtr = System.Runtime.InteropServices.Marshal.ReadIntPtr(statusPtr, 48);
            if (vendorInfoPtr != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(vendorInfoPtr);
            }
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(statusPtr);
        }
    }

    [Test]
    public async Task GetErrorString_via_CCW_returns_LPWSTR()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);

        (int hr, IntPtr stringPtr) = InvokeGetErrorString(ccw, dwError: unchecked((int)0x80004005), dwLocale: 1033);

        try
        {
            await Assert.That(hr).IsEqualTo(S_OK);
            await Assert.That(stringPtr).IsNotEqualTo(IntPtr.Zero);
            string? text = System.Runtime.InteropServices.Marshal.PtrToStringUni(stringPtr);
            await Assert.That(text).IsEqualTo("ok");
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(stringPtr);
        }
    }

    [Test]
    public async Task RemoveGroup_via_CCW_delegates_to_managed_server()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);

        int hr = InvokeRemoveGroup(ccw, hServerGroup: 12345, bForce: 1);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(stub.LastRemovedGroupHandle).IsEqualTo(12345);
        await Assert.That(stub.LastRemoveGroupForce).IsTrue();
        await Assert.That(stub.RemoveGroupCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task IOPCServer_AddGroup_returns_ccw_pointer_via_OpcDaGroupCcw()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // ocom-6d wires AddGroup to allocate a managed group through the
        // stub server and return an OpcDaGroupCcw pointer. cap-a5 wires
        // GetGroupByName -> ResolveGroupByNameAsync; CreateGroupEnumerator
        // still returns E_NOTIMPL pending IEnumUnknown CCW infrastructure.
        var stub = new StubDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);
        (int hrAddGroup, IntPtr ppUnkAdd, int hrGetGroupByName, int hrCreateGroupEnumerator) = InvokeRemainingStubs(ccw);

        await Assert.That(hrAddGroup).IsEqualTo(S_OK);
        await Assert.That(ppUnkAdd).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(stub.AddGroupCallCount).IsEqualTo(1);
        // GetGroupByName receives IntPtr.Zero for szName -> E_INVALIDARG.
        await Assert.That(hrGetGroupByName).IsEqualTo(E_INVALIDARG);
        await Assert.That(hrCreateGroupEnumerator).IsEqualTo(E_NOTIMPL);
    }

    [Test]
    public async Task GetGroupByName_with_unknown_name_returns_OPC_E_UNKNOWNPATH()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new StubDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);
        int hr = InvokeGetGroupByName(ccw, "Nope");

        await Assert.That(hr).IsEqualTo(OpcResultId.UnknownPath.Code);
    }

    [Test]
    public async Task GetGroupByName_with_resolved_group_returns_OpcDaGroupCcw()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new GroupResolvingDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);
        (int hr, IntPtr returned) = InvokeGetGroupByNameWithPointer(ccw, "G1");

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(returned).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task AddGroup_with_null_phServerGroup_returns_E_INVALIDARG()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new StubDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);
        int hr = InvokeAddGroupWithNullOuts(ccw);

        await Assert.That(hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(stub.AddGroupCallCount).IsEqualTo(0);
    }

    [Test]
    public async Task GetStatus_with_null_ppServerStatus_returns_E_INVALIDARG()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);
        int hr = InvokeGetStatusWithNullPpStatus(ccw);

        await Assert.That(hr).IsEqualTo(E_INVALIDARG);
    }

    [Test]
    public async Task GetErrorString_maps_managed_ArgumentException_to_E_INVALIDARG()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new ThrowingArgServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);

        (int hr, _) = InvokeGetErrorString(ccw, dwError: 0, dwLocale: 0);

        await Assert.That(hr).IsEqualTo(E_INVALIDARG);
    }

    [Test]
    public async Task SupportsInterface_returns_true_for_known_iids()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(OpcDaServerCcw.SupportsInterface(IID_IUnknown)).IsTrue();
        await Assert.That(OpcDaServerCcw.SupportsInterface(IOPCServer.InterfaceId)).IsTrue();
        await Assert.That(OpcDaServerCcw.SupportsInterface(IOPCCommon.InterfaceId)).IsTrue();
        await Assert.That(OpcDaServerCcw.SupportsInterface(IID_IClassFactory)).IsFalse();
        await Assert.That(OpcDaServerCcw.SupportsInterface(Guid.NewGuid())).IsFalse();
    }

    [Test]
    public async Task IOPCCommon_SetLocaleID_via_CCW_delegates_to_IDaServer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingCommonDaServer(localeId: 1033, supportedLocales: [1033, 1031]);
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCCommon.InterfaceId);

        int hr = InvokeCommonSetLocaleId(ccw, 1031);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(stub.LocaleId).IsEqualTo(1031);
        await Assert.That(stub.LastSetLocaleId).IsEqualTo(1031);
        await Assert.That(stub.SetLocaleCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task IOPCCommon_GetLocaleID_via_CCW_reads_IDaServer_locale()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingCommonDaServer(localeId: 1041, supportedLocales: [1033, 1041]);
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCCommon.InterfaceId);

        (int hr, uint lcid) = InvokeCommonGetLocaleId(ccw);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(lcid).IsEqualTo(1041u);
    }

    [Test]
    public async Task IOPCCommon_QueryAvailableLocaleIDs_via_CCW_returns_IDaServer_supported_locales()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingCommonDaServer(localeId: 1033, supportedLocales: [1033, 1031, 1041]);
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCCommon.InterfaceId);

        (int hr, uint[] locales) = InvokeCommonQueryAvailableLocaleIds(ccw);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(locales).IsEquivalentTo([1033u, 1031u, 1041u]);
        await Assert.That(stub.GetSupportedLocalesCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task IOPCCommon_GetErrorString_via_CCW_delegates_to_IDaServer_error_text()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        const int Error = unchecked((int)0x80004005);
        var stub = new RecordingCommonDaServer(localeId: 1031, supportedLocales: [1031]);
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCCommon.InterfaceId);

        (int hr, string? text) = InvokeCommonGetErrorString(ccw, Error);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(text).IsEqualTo("text:80004005:1031");
        await Assert.That(stub.LastErrorTextResultId).IsEqualTo(new OpcResultId(Error, null));
        await Assert.That(stub.GetErrorTextCallCount).IsEqualTo(1);
    }

    // ----- sync unsafe helpers (await is illegal in unsafe context) -----

    private readonly record struct QueryInterfaceResult(IntPtr Returned, int Hr);

    private readonly record struct AddRefReleaseResult(
        uint After1stAddRef, uint After2ndAddRef,
        uint After1stRelease, uint After2ndRelease);

    private static unsafe (int HrAddGroup, IntPtr PpUnkAdd, int HrGetGroupByName, int HrCreateGroupEnumerator) InvokeRemainingStubs(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;

        // slot 3 = AddGroup (12 params); slot 5 = GetGroupByName (3 params); slot 8 = CreateGroupEnumerator (3 params)
        var addGroup = (delegate* unmanaged<IntPtr, IntPtr, int, uint, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[3];
        IntPtr ppUnk1;
        Guid iid = Guid.Empty;
        IntPtr phServer = Marshal.AllocCoTaskMem(sizeof(int));
        IntPtr pRevised = Marshal.AllocCoTaskMem(sizeof(int));
        int hrAdd = addGroup(ccw, IntPtr.Zero, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, 0, phServer, pRevised, &iid, &ppUnk1);
        Marshal.FreeCoTaskMem(phServer);
        Marshal.FreeCoTaskMem(pRevised);

        var getGroupByName = (delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[5];
        IntPtr ppUnk2;
        int hrByName = getGroupByName(ccw, IntPtr.Zero, &iid, &ppUnk2);

        var createGroupEnumerator = (delegate* unmanaged<IntPtr, uint, Guid*, IntPtr*, int>)vtable[8];
        IntPtr ppUnk3;
        int hrEnum = createGroupEnumerator(ccw, 0, &iid, &ppUnk3);

        return (hrAdd, ppUnk1, hrByName, hrEnum);
    }

    private static unsafe QueryInterfaceResult InvokeQueryInterface(IntPtr ccw, Guid iid)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var qi = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
        Guid local = iid;
        IntPtr returned;
        int hr = qi(ccw, &local, &returned);
        return new QueryInterfaceResult(returned, hr);
    }

    private static unsafe AddRefReleaseResult InvokeAddRefRelease(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var addRef = (delegate* unmanaged<IntPtr, uint>)vtable[1];
        var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
        uint a1 = addRef(ccw);
        uint a2 = addRef(ccw);
        uint r1 = release(ccw);
        uint r2 = release(ccw);
        return new AddRefReleaseResult(a1, a2, r1, r2);
    }

    private static unsafe (int Hr, IntPtr StatusPtr) InvokeGetStatus(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var getStatus = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[6];
        IntPtr statusOut;
        int hr = getStatus(ccw, &statusOut);
        return (hr, statusOut);
    }

    private static unsafe (int Hr, IntPtr StringPtr) InvokeGetErrorString(IntPtr ccw, int dwError, uint dwLocale)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var getErrorString = (delegate* unmanaged<IntPtr, int, uint, IntPtr*, int>)vtable[4];
        IntPtr stringOut;
        int hr = getErrorString(ccw, dwError, dwLocale, &stringOut);
        return (hr, stringOut);
    }

    private static unsafe int InvokeCommonSetLocaleId(IntPtr ccw, uint dwLcid)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var setLocaleId = (delegate* unmanaged<IntPtr, uint, int>)vtable[3];
        return setLocaleId(ccw, dwLcid);
    }

    private static unsafe (int Hr, uint Lcid) InvokeCommonGetLocaleId(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var getLocaleId = (delegate* unmanaged<IntPtr, uint*, int>)vtable[4];
        uint lcid;
        int hr = getLocaleId(ccw, &lcid);
        return (hr, lcid);
    }

    private static unsafe (int Hr, uint[] Locales) InvokeCommonQueryAvailableLocaleIds(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var queryAvailableLocaleIds = (delegate* unmanaged<IntPtr, uint*, IntPtr*, int>)vtable[5];
        uint count;
        IntPtr localesPtr;
        int hr = queryAvailableLocaleIds(ccw, &count, &localesPtr);
        try
        {
            var locales = new uint[count];
            for (int i = 0; i < locales.Length; i++)
            {
                locales[i] = unchecked((uint)Marshal.ReadInt32(localesPtr, i * sizeof(uint)));
            }

            return (hr, locales);
        }
        finally
        {
            Marshal.FreeCoTaskMem(localesPtr);
        }
    }

    private static unsafe (int Hr, string? Text) InvokeCommonGetErrorString(IntPtr ccw, int dwError)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var getErrorString = (delegate* unmanaged<IntPtr, int, IntPtr*, int>)vtable[6];
        IntPtr stringOut;
        int hr = getErrorString(ccw, dwError, &stringOut);
        try
        {
            return (hr, Marshal.PtrToStringUni(stringOut));
        }
        finally
        {
            Marshal.FreeCoTaskMem(stringOut);
        }
    }

    private static unsafe int InvokeRemoveGroup(IntPtr ccw, uint hServerGroup, int bForce)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var removeGroup = (delegate* unmanaged<IntPtr, uint, int, int>)vtable[7];
        return removeGroup(ccw, hServerGroup, bForce);
    }

    private static unsafe int InvokeAddGroupWithNullOuts(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var addGroup = (delegate* unmanaged<IntPtr, IntPtr, int, uint, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[3];
        // pass IntPtr.Zero for phServerGroup and pRevisedUpdateRate -> expect E_INVALIDARG
        IntPtr ppUnk;
        Guid iid = Guid.Empty;
        return addGroup(ccw, IntPtr.Zero, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, &iid, &ppUnk);
    }

    private static unsafe int InvokeGetStatusWithNullPpStatus(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var getStatus = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[6];
        // Pass null pointer for the OUT param.
        return getStatus(ccw, null);
    }

    private static unsafe int InvokeGetGroupByName(IntPtr ccw, string name)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var getByName = (delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[5];
        IntPtr namePtr = Marshal.StringToCoTaskMemUni(name);
        try
        {
            IntPtr ppUnk;
            Guid iid = Guid.Empty;
            return getByName(ccw, namePtr, &iid, &ppUnk);
        }
        finally
        {
            Marshal.FreeCoTaskMem(namePtr);
        }
    }

    private static unsafe (int Hr, IntPtr ReturnedCcw) InvokeGetGroupByNameWithPointer(IntPtr ccw, string name)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var getByName = (delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[5];
        IntPtr namePtr = Marshal.StringToCoTaskMemUni(name);
        try
        {
            IntPtr ppUnk;
            Guid iid = Guid.Empty;
            int hr = getByName(ccw, namePtr, &iid, &ppUnk);
            return (hr, ppUnk);
        }
        finally
        {
            Marshal.FreeCoTaskMem(namePtr);
        }
    }

    private sealed class RecordingDaServer : IOpcDaServer
    {
        public int RemoveGroupCallCount { get; private set; }
        public int LastRemovedGroupHandle { get; private set; }
        public bool LastRemoveGroupForce { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "recording-test",
            });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
        {
            RemoveGroupCallCount++;
            LastRemovedGroupHandle = serverGroupHandle;
            LastRemoveGroupForce = force;
            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");
    }

    private sealed class RecordingCommonDaServer : IOpcDaServer, IDaServer
    {
        private readonly int[] _supportedLocales;

        public RecordingCommonDaServer(int localeId, int[] supportedLocales)
        {
            LocaleId = localeId;
            _supportedLocales = supportedLocales;
        }

        public int LocaleId { get; private set; }
        public int LastSetLocaleId { get; private set; }
        public int SetLocaleCallCount { get; private set; }
        public int GetSupportedLocalesCallCount { get; private set; }
        public int GetErrorTextCallCount { get; private set; }
        public OpcResultId LastErrorTextResultId { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "recording-common-test",
            });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default)
        {
            SetLocaleCallCount++;
            LastSetLocaleId = localeId;
            LocaleId = localeId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default)
        {
            GetSupportedLocalesCallCount++;
            return Task.FromResult<IReadOnlyList<int>>(_supportedLocales);
        }

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default)
        {
            GetErrorTextCallCount++;
            LastErrorTextResultId = resultId;
            var hex = unchecked((uint)resultId.Code).ToString("X8", System.Globalization.CultureInfo.InvariantCulture);
            return Task.FromResult($"text:{hex}:{LocaleId}");
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            GetErrorTextAsync(new OpcResultId(errorCode, null), cancellationToken);

        public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemValueResult>>(Array.Empty<ItemValueResult>());

        public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(IReadOnlyList<ItemIdentifier> itemIds, IReadOnlyList<PropertyID> propertyIds, bool returnValues, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemPropertyResult>>(Array.Empty<ItemPropertyResult>());

        public Task<IDaSubscription> CreateSubscriptionAsync(SubscriptionState state, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<BrowseElement> BrowseAsync(string itemPath, BrowseFilters filters, CancellationToken cancellationToken = default) =>
            EmptyBrowse();

        private static async IAsyncEnumerable<BrowseElement> EmptyBrowse()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        public event EventHandler<ServerShutdownEventArgs>? ServerShutdown { add { } remove { } }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class StubDaServer : IOpcDaServer
    {
        public int AddGroupCallCount { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "ccw-test",
            });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default)
        {
            AddGroupCallCount++;
            return Task.FromResult(1);
        }

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");
    }

    private sealed class ThrowingArgServer : IOpcDaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            throw new ArgumentException("bad");

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            throw new ArgumentException("bad");

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) =>
            throw new ArgumentException("bad");

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            throw new ArgumentException("bad");
    }

    private sealed class GroupResolvingDaServer : IOpcDaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Da });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");

        public Task<Opc.Classic.Da.Hosting.OpcDaGroup?> ResolveGroupByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult<Opc.Classic.Da.Hosting.OpcDaGroup?>(new Opc.Classic.Da.Hosting.OpcDaGroup(
                name: name,
                serverHandle: 1,
                clientHandle: 1,
                active: true,
                requestedUpdateRate: 1000,
                timeBias: 0,
                percentDeadband: 0f,
                localeId: 1033));
    }
}
