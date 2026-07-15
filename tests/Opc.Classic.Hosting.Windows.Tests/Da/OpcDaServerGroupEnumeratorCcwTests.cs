// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using Opc.Classic.Hosting.Windows;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcDaServerGroupEnumeratorCcwTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private static readonly Guid s_iidIUnknown = new("00000000-0000-0000-C000-000000000046");

    [Test]
    [Arguments(1, true, 2)]
    [Arguments(2, true, 1)]
    [Arguments(3, true, 3)]
    [Arguments(4, false, 2)]
    [Arguments(5, false, 1)]
    [Arguments(6, false, 3)]
    public async Task All_nonempty_scopes_return_S_OK(
        int scope,
        bool connections,
        int expectedCount)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new SnapshotServer(
            [Group("P1", 1), Group("P2", 2)],
            [Group("U1", 3)]);
        IntPtr root = OpcDaServerCcw.Create(server, IOPCServer.InterfaceId);
        Guid iid = connections ? OpcGuids.IID_IEnumUnknown : OpcGuids.IID_IEnumString;
        (int hr, IntPtr enumerator) = CreateEnumerator(root, (uint)scope, iid);
        try
        {
            await Assert.That(hr).IsEqualTo(S_OK);
            if (connections)
            {
                UnknownNextResult next = NextUnknowns(enumerator, 8);
                try
                {
                    await Assert.That(next.Hr).IsEqualTo(S_FALSE);
                    await Assert.That(next.Fetched).IsEqualTo((uint)expectedCount);
                    foreach (IntPtr value in next.Values)
                    {
                        QueryResult identity = QueryInterface(value, s_iidIUnknown);
                        await Assert.That(identity.Hr).IsEqualTo(S_OK);
                        await Assert.That(identity.Pointer).IsEqualTo(value);
                        Release(identity.Pointer);
                    }
                }
                finally
                {
                    foreach (IntPtr value in next.Values)
                    {
                        Release(value);
                    }
                }
            }
            else
            {
                StringNextResult next = NextStrings(enumerator, 8);
                await Assert.That(next.Hr).IsEqualTo(S_FALSE);
                await Assert.That(next.Fetched).IsEqualTo((uint)expectedCount);
            }
        }
        finally
        {
            Release(enumerator);
            RemoveGroup(root, 1);
            RemoveGroup(root, 2);
            RemoveGroup(root, 3);
        }
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    [Arguments(5)]
    [Arguments(6)]
    public async Task Empty_scopes_return_S_FALSE_with_valid_enumerator(int scope)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr root = OpcDaServerCcw.Create(new SnapshotServer([], []), IOPCServer.InterfaceId);
        bool connections = scope <= 3;
        Guid iid = connections ? OpcGuids.IID_IEnumUnknown : OpcGuids.IID_IEnumString;
        (int hr, IntPtr enumerator) = CreateEnumerator(root, (uint)scope, iid);
        try
        {
            await Assert.That(hr).IsEqualTo(S_FALSE);
            await Assert.That(enumerator).IsNotEqualTo(IntPtr.Zero);
            uint fetched = connections
                ? NextUnknowns(enumerator, 1).Fetched
                : NextStrings(enumerator, 1).Fetched;
            await Assert.That(fetched).IsEqualTo(0U);
        }
        finally
        {
            Release(enumerator);
        }
    }

    [Test]
    public async Task Invalid_scope_iid_and_null_outputs_return_required_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr root = OpcDaServerCcw.Create(new SnapshotServer([], []), IOPCServer.InterfaceId);
        InvalidResult result = InvalidCalls(root);
        await Assert.That(result.Zero).IsEqualTo(E_INVALIDARG);
        await Assert.That(result.Seven).IsEqualTo(E_INVALIDARG);
        await Assert.That(result.Overflow).IsEqualTo(E_INVALIDARG);
        await Assert.That(result.ConnectionMismatch).IsEqualTo(E_NOINTERFACE);
        await Assert.That(result.NameMismatch).IsEqualTo(E_NOINTERFACE);
        await Assert.That(result.NullIid).IsEqualTo(E_INVALIDARG);
        await Assert.That(result.NullOutput).IsEqualTo(E_INVALIDARG);
        await Assert.That(result.Output).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Canonical_identity_is_shared_by_AddGroup_GetGroupByName_and_cloned_snapshot()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new SnapshotServer([], []);
        IntPtr root = OpcDaServerCcw.Create(server, IOPCServer.InterfaceId);
        (int addHr, IntPtr added) = AddGroup(root, "Canonical");
        (int byNameHr, IntPtr byName) = GetByName(root, "Canonical");
        (int enumHr, IntPtr enumerator) =
            CreateEnumerator(root, 1, OpcGuids.IID_IEnumUnknown);
        (int cloneHr, IntPtr clone) = Clone(enumerator);
        IntPtr first = NextUnknowns(enumerator, 1).Values.Single();
        IntPtr cloned = IntPtr.Zero;
        try
        {
            await Assert.That(addHr).IsEqualTo(S_OK);
            await Assert.That(byNameHr).IsEqualTo(S_OK);
            await Assert.That(enumHr).IsEqualTo(S_OK);
            await Assert.That(cloneHr).IsEqualTo(S_OK);
            await Assert.That(added).IsEqualTo(byName);
            await Assert.That(added).IsEqualTo(first);

            await Assert.That(RemoveGroup(root, server.LastHandle)).IsEqualTo(S_OK);
            Release(added);
            added = IntPtr.Zero;
            Release(byName);
            byName = IntPtr.Zero;
            Release(first);
            first = IntPtr.Zero;
            Release(enumerator);
            enumerator = IntPtr.Zero;

            cloned = NextUnknowns(clone, 1).Values.Single();
            await Assert.That(cloned).IsEqualTo(server.RemovedIdentity);
            Release(clone);
            clone = IntPtr.Zero;
            await Assert.That(OpcDaGroupCcw.GetReferenceCount(cloned)).IsEqualTo(1L);
            Release(cloned);
            cloned = IntPtr.Zero;
            await Assert.That(OpcDaGroupCcw.GetReferenceCount(server.RemovedIdentity)).IsEqualTo(-1L);
        }
        finally
        {
            Release(cloned);
            Release(first);
            Release(clone);
            Release(enumerator);
            Release(byName);
            Release(added);
        }
    }

    [Test]
    public async Task Name_snapshot_supports_partial_next_skip_reset_clone_and_copies_names()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup firstGroup = Group("P1", 1);
        var server = new SnapshotServer(
            [firstGroup, Group("P2", 2), Group("P3", 3)],
            []);
        IntPtr root = OpcDaServerCcw.Create(server, IOPCServer.InterfaceId);
        (int createHr, IntPtr enumerator) =
            CreateEnumerator(root, 4, OpcGuids.IID_IEnumString);
        await firstGroup.SetNameAsync("Changed");
        IntPtr clone = IntPtr.Zero;
        try
        {
            StringNextResult first = NextStrings(enumerator, 2);
            (int cloneHr, clone) = Clone(enumerator);
            int skip = Skip(enumerator, 1);
            StringNextResult end = NextStrings(enumerator, 1);
            StringNextResult cloneNext = NextStrings(clone, 1);
            int reset = Reset(clone);
            StringNextResult all = NextStrings(clone, 4);

            await Assert.That(createHr).IsEqualTo(S_OK);
            await Assert.That(first.Values).IsEquivalentTo(["P1", "P2"]);
            await Assert.That(cloneHr).IsEqualTo(S_OK);
            await Assert.That(skip).IsEqualTo(S_OK);
            await Assert.That(end.Hr).IsEqualTo(S_FALSE);
            await Assert.That(cloneNext.Values).IsEquivalentTo(["P3"]);
            await Assert.That(reset).IsEqualTo(S_OK);
            await Assert.That(all.Values).IsEquivalentTo(["P1", "P2", "P3"]);
        }
        finally
        {
            Release(clone);
            Release(enumerator);
        }
    }

    [Test]
    public async Task Concurrent_clone_releases_do_not_leak_or_double_free_group_identity()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new SnapshotServer([Group("P1", 1)], []);
        IntPtr root = OpcDaServerCcw.Create(server, IOPCServer.InterfaceId);
        (_, IntPtr enumerator) =
            CreateEnumerator(root, 1, OpcGuids.IID_IEnumUnknown);
        var enumerators = new List<IntPtr> { enumerator };
        for (int i = 1; i < 32; i++)
        {
            enumerators.Add(Clone(enumerator).Pointer);
        }
        IntPtr group = NextUnknowns(enumerator, 1).Values.Single();
        Release(group);
        RemoveGroup(root, 1);
        await Task.WhenAll(enumerators.Select(value => Task.Run(() => Release(value))));

        await Assert.That(OpcDaGroupCcw.GetReferenceCount(group)).IsEqualTo(-1L);
        await Assert.That(enumerators.All(
            value => OpcEnumUnknownCcw.GetReferenceCount(value) == -1L)).IsTrue();
    }

    [Test]
    public async Task Shared_string_enumerator_exposes_reference_count_lifecycle()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr enumerator = OpcEnumStringCcw.Create(["A"]);
        await Assert.That(OpcEnumStringCcw.GetReferenceCount(enumerator)).IsEqualTo(1L);
        Release(enumerator);
        await Assert.That(OpcEnumStringCcw.GetReferenceCount(enumerator)).IsEqualTo(-1L);
    }

    private static OpcDaGroup Group(string name, int handle) =>
        new(name, handle, handle, true, 1000, 0, 0, 1033);

    private static unsafe (int Hr, IntPtr Pointer) AddGroup(IntPtr root, string name)
    {
        IntPtr* vtable = *(IntPtr**)root;
        var method = (delegate* unmanaged<IntPtr, IntPtr, int, uint, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[3];
        IntPtr namePtr = Marshal.StringToCoTaskMemUni(name);
        IntPtr handle = Marshal.AllocCoTaskMem(sizeof(int));
        IntPtr rate = Marshal.AllocCoTaskMem(sizeof(int));
        Guid iid = s_iidIUnknown;
        IntPtr pointer;
        try
        {
            int hr = method(root, namePtr, 1, 1000, 1, IntPtr.Zero, IntPtr.Zero, 1033, handle, rate, &iid, &pointer);
            return (hr, pointer);
        }
        finally
        {
            Marshal.FreeCoTaskMem(namePtr);
            Marshal.FreeCoTaskMem(handle);
            Marshal.FreeCoTaskMem(rate);
        }
    }

    private static unsafe (int Hr, IntPtr Pointer) GetByName(IntPtr root, string name)
    {
        IntPtr* vtable = *(IntPtr**)root;
        var method = (delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[5];
        IntPtr namePtr = Marshal.StringToCoTaskMemUni(name);
        Guid iid = s_iidIUnknown;
        IntPtr pointer;
        try
        {
            return (method(root, namePtr, &iid, &pointer), pointer);
        }
        finally
        {
            Marshal.FreeCoTaskMem(namePtr);
        }
    }

    private static unsafe int RemoveGroup(IntPtr root, int handle)
    {
        IntPtr* vtable = *(IntPtr**)root;
        var method = (delegate* unmanaged<IntPtr, uint, int, int>)vtable[7];
        return method(root, (uint)handle, 1);
    }

    private static unsafe (int Hr, IntPtr Pointer) CreateEnumerator(IntPtr root, uint scope, Guid iid)
    {
        IntPtr* vtable = *(IntPtr**)root;
        var method = (delegate* unmanaged<IntPtr, uint, Guid*, IntPtr*, int>)vtable[8];
        IntPtr pointer;
        return (method(root, scope, &iid, &pointer), pointer);
    }

    private static unsafe InvalidResult InvalidCalls(IntPtr root)
    {
        IntPtr* vtable = *(IntPtr**)root;
        var method = (delegate* unmanaged<IntPtr, uint, Guid*, IntPtr*, int>)vtable[8];
        Guid unknown = OpcGuids.IID_IEnumUnknown;
        Guid strings = OpcGuids.IID_IEnumString;
        IntPtr output = new(1);
        int zero = method(root, 0, &unknown, &output);
        int seven = method(root, 7, &unknown, &output);
        int overflow = method(root, uint.MaxValue, &unknown, &output);
        int connectionMismatch = method(root, 1, &strings, &output);
        int nameMismatch = method(root, 4, &unknown, &output);
        output = new(1);
        int nullIid = method(root, 1, null, &output);
        int nullOutput = method(root, 1, &unknown, null);
        return new(zero, seven, overflow, connectionMismatch, nameMismatch, nullIid, nullOutput, output);
    }

    private static unsafe UnknownNextResult NextUnknowns(IntPtr value, uint count)
    {
        IntPtr* vtable = *(IntPtr**)value;
        var method = (delegate* unmanaged<IntPtr, uint, IntPtr*, uint*, int>)vtable[3];
        IntPtr* buffer = stackalloc IntPtr[Math.Max((int)count, 1)];
        uint fetched = 0;
        int hr = method(value, count, count == 0 ? null : buffer, &fetched);
        var result = new IntPtr[fetched];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = buffer[i];
        }
        return new(hr, fetched, result);
    }

    private static unsafe StringNextResult NextStrings(IntPtr value, uint count)
    {
        IntPtr* vtable = *(IntPtr**)value;
        var method = (delegate* unmanaged<IntPtr, uint, IntPtr*, uint*, int>)vtable[3];
        IntPtr* buffer = stackalloc IntPtr[Math.Max((int)count, 1)];
        uint fetched = 0;
        int hr = method(value, count, count == 0 ? null : buffer, &fetched);
        var result = new string[fetched];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Marshal.PtrToStringUni(buffer[i]) ?? string.Empty;
            Marshal.FreeCoTaskMem(buffer[i]);
        }
        return new(hr, fetched, result);
    }

    private static unsafe (int Hr, IntPtr Pointer) Clone(IntPtr value)
    {
        IntPtr* vtable = *(IntPtr**)value;
        var method = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[6];
        IntPtr pointer;
        return (method(value, &pointer), pointer);
    }

    private static unsafe int Skip(IntPtr value, uint count)
    {
        IntPtr* vtable = *(IntPtr**)value;
        return ((delegate* unmanaged<IntPtr, uint, int>)vtable[4])(value, count);
    }

    private static unsafe int Reset(IntPtr value)
    {
        IntPtr* vtable = *(IntPtr**)value;
        return ((delegate* unmanaged<IntPtr, int>)vtable[5])(value);
    }

    private static unsafe QueryResult QueryInterface(IntPtr value, Guid iid)
    {
        IntPtr* vtable = *(IntPtr**)value;
        var method = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
        IntPtr pointer;
        return new(method(value, &iid, &pointer), pointer);
    }

    private static unsafe void Release(IntPtr value)
    {
        if (value == IntPtr.Zero)
        {
            return;
        }
        IntPtr* vtable = *(IntPtr**)value;
        _ = ((delegate* unmanaged<IntPtr, uint>)vtable[2])(value);
    }

    private readonly record struct UnknownNextResult(int Hr, uint Fetched, IntPtr[] Values);
    private readonly record struct StringNextResult(int Hr, uint Fetched, string[] Values);
    private readonly record struct QueryResult(int Hr, IntPtr Pointer);
    private readonly record struct InvalidResult(
        int Zero,
        int Seven,
        int Overflow,
        int ConnectionMismatch,
        int NameMismatch,
        int NullIid,
        int NullOutput,
        IntPtr Output);

    private sealed class SnapshotServer : IOpcDaServer
    {
        private readonly Lock _lock = new();
        private readonly List<OpcDaGroup> _private;
        private readonly List<OpcDaGroup> _public;
        private int _nextHandle;

        public SnapshotServer(IReadOnlyList<OpcDaGroup> privateGroups, IReadOnlyList<OpcDaGroup> publicGroups)
        {
            _private = [.. privateGroups];
            _public = [.. publicGroups];
            _nextHandle = _private.Concat(_public).Select(static group => group.ServerHandle).DefaultIfEmpty().Max();
        }

        public int LastHandle { get; private set; }
        public IntPtr RemovedIdentity { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Da });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                LastHandle = ++_nextHandle;
                _private.Add(Group(name, LastHandle));
                return Task.FromResult(LastHandle);
            }
        }

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                OpcDaGroup? group = _private.Concat(_public).FirstOrDefault(value => value.ServerHandle == serverGroupHandle);
                if (group is not null)
                {
                    RemovedIdentity = OpcDaGroupCcw.Create(group);
                    Release(RemovedIdentity);
                }
                _private.RemoveAll(value => value.ServerHandle == serverGroupHandle);
                _public.RemoveAll(value => value.ServerHandle == serverGroupHandle);
            }
            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.Empty);

        public Task<OpcDaGroup?> ResolveGroupAsync(int serverHandle, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                return Task.FromResult<OpcDaGroup?>(_private.Concat(_public).FirstOrDefault(value => value.ServerHandle == serverHandle));
            }
        }

        public Task<OpcDaGroup?> ResolveGroupByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                return Task.FromResult<OpcDaGroup?>(_private.Concat(_public).FirstOrDefault(value => value.Name == name));
            }
        }

        public Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                return Task.FromResult<IReadOnlyList<OpcDaGroup>>([.. _private]);
            }
        }

        public Task<IReadOnlyList<OpcDaGroup>> SnapshotPublicGroupsAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                return Task.FromResult<IReadOnlyList<OpcDaGroup>>([.. _public]);
            }
        }

        public Task<OpcDaGroupSetSnapshot> SnapshotAllGroupsAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                return Task.FromResult(new OpcDaGroupSetSnapshot(_private, _public));
            }
        }
    }
}
