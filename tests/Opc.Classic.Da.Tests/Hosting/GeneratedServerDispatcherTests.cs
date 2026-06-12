//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Per-interface unit tests for the source-generated <c>*ServerDispatcher</c>
/// classes for the highest-traffic DA root/group interfaces:
/// <see cref="IOPCCommon" />, <see cref="IOPCGroupStateMgt" />,
/// <see cref="IOPCGroupStateMgt2" />.
/// </summary>
/// <remarks>
/// These tests exercise the generated dispatcher dispatch tables directly
/// (request NDR payload → impl method invocation → response NDR payload).
/// The remaining DA interfaces (<c>IOPCItemMgt</c>, <c>IOPCSyncIO</c>,
/// <c>IOPCAsyncIO2/3</c>, <c>IOPCBrowse</c>, <c>IOPCItemProperties</c>, etc.)
/// have generator-level coverage in
/// <c>tests/Opc.Classic.Generators.Tests/ServerDispatchGeneratorTests.cs</c>
/// and integration-level coverage will land with ocom-5 (compat matrix
/// wireup). Per-interface unit tests for those interfaces are tracked as
/// a follow-up.
/// </remarks>
public sealed class GeneratedServerDispatcherTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    // ===== IOPCCommon =====

    [Test]
    public async Task IOPCCommon_SetLocaleId_dispatches_to_implementation()
    {
        var impl = new StubCommon();
        var dispatcher = new IOPCCommonServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(1033));

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCCommon.Opnums.SetLocaleIdAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.LastLocaleId).IsEqualTo(1033);
    }

    [Test]
    public async Task IOPCCommon_GetLocaleId_returns_current_locale_in_payload()
    {
        var impl = new StubCommon { CurrentLocaleId = 2052 };
        var dispatcher = new IOPCCommonServerDispatcher(impl);

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCCommon.Opnums.GetLocaleIdAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        int locale = ReadInt32(result.Payload);
        await Assert.That(locale).IsEqualTo(2052);
    }

    [Test]
    public async Task IOPCCommon_unknown_opnum_returns_E_NOTIMPL()
    {
        var dispatcher = new IOPCCommonServerDispatcher(new StubCommon());

        DispatchResult result = await dispatcher.DispatchAsync(
            opnum: 99,
            requestPayload: ReadOnlyMemory<byte>.Empty,
            cancellationToken: CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
    }

    // ===== IOPCGroupStateMgt =====

    [Test]
    public async Task IOPCGroupStateMgt_GetState_returns_current_state()
    {
        var impl = new StubGroupStateMgt
        {
            CurrentState = new OpcGroupState(
                ClientHandle: 7,
                ServerHandle: 42,
                Name: "G",
                Active: true,
                UpdateRate: 500,
                TimeBias: 0,
                PercentDeadband: 0f,
                LocaleId: 1033),
        };
        var dispatcher = new IOPCGroupStateMgtServerDispatcher(impl);

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCGroupStateMgt.Opnums.GetStateAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.GetStateCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task IOPCGroupStateMgt_SetName_dispatches_to_implementation()
    {
        var impl = new StubGroupStateMgt();
        var dispatcher = new IOPCGroupStateMgtServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Renamed"));

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCGroupStateMgt.Opnums.SetNameAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.LastSetName).IsEqualTo("Renamed");
    }

    [Test]
    public async Task IOPCGroupStateMgt_unknown_opnum_returns_E_NOTIMPL()
    {
        var dispatcher = new IOPCGroupStateMgtServerDispatcher(new StubGroupStateMgt());

        DispatchResult result = await dispatcher.DispatchAsync(
            opnum: 99, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
    }

    // ===== IOPCGroupStateMgt2 =====

    [Test]
    public async Task IOPCGroupStateMgt2_SetKeepAlive_returns_previous_value()
    {
        var impl = new StubGroupStateMgt2 { CurrentKeepAlive = 1000 };
        var dispatcher = new IOPCGroupStateMgt2ServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(5000));

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCGroupStateMgt2.Opnums.SetKeepAliveAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        int previous = ReadInt32(result.Payload);
        await Assert.That(previous).IsEqualTo(1000);
        await Assert.That(impl.CurrentKeepAlive).IsEqualTo(5000);
    }

    [Test]
    public async Task IOPCGroupStateMgt2_GetKeepAlive_returns_current_value()
    {
        var impl = new StubGroupStateMgt2 { CurrentKeepAlive = 3500 };
        var dispatcher = new IOPCGroupStateMgt2ServerDispatcher(impl);

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCGroupStateMgt2.Opnums.GetKeepAliveAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        int current = ReadInt32(result.Payload);
        await Assert.That(current).IsEqualTo(3500);
    }

    // ===== IConnectionPoint =====

    [Test]
    public async Task IConnectionPoint_GetConnectionInterface_returns_iid()
    {
        Guid expectedIid = Guid.Parse("39C13A70-011E-11D0-9675-0020AFD8ADB3"); // IID_IOPCDataCallback
        var impl = new StubConnectionPoint { ConnectionIid = expectedIid };
        var dispatcher = new IConnectionPointServerDispatcher(impl);

        DispatchResult result = await dispatcher.DispatchAsync(
            IConnectionPoint.Opnums.GetConnectionInterfaceAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        var reader = new NdrReader(result.Payload.Span);
        Guid returned = reader.ReadGuid();
        await Assert.That(returned).IsEqualTo(expectedIid);
    }

    [Test]
    public async Task IConnectionPoint_Unadvise_dispatches_cookie()
    {
        var impl = new StubConnectionPoint();
        var dispatcher = new IConnectionPointServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(42));

        DispatchResult result = await dispatcher.DispatchAsync(
            IConnectionPoint.Opnums.UnadviseAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.LastUnadvisedCookie).IsEqualTo(42);
    }

    // ===== IOPCItemIO =====

    [Test]
    public async Task IOPCItemIO_WriteVqt_dispatches_with_item_ids()
    {
        var impl = new StubItemIO();
        var dispatcher = new IOPCItemIOServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) =>
        {
            // [in] DWORD dwCount = 1 (sibling count for itemIds, emitted before
            // the conformant array via [OpcEmitArrayCount]).
            writer.WriteInt32(1);
            // string[] itemIds = ["Item.A"]
            writer.WriteInt32(1);
            writer.WriteUnicodeStringPtr("Item.A");
            // OpcItemVqt[] values = [{ value=42, no quality, no timestamp }]
            writer.WriteInt32(1);
            // value variant
            Opc.Classic.Ndr.NdrVariantExtensions.WriteVariant(ref writer, new OpcVariant(VarType.VT_I4, 42));
            // bQualitySpecified, wQuality, wReserved
            writer.WriteInt32(0);
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            // bTimeStampSpecified, dwReserved, ftTimeStamp
            writer.WriteInt32(0);
            writer.WriteUInt32(0);
            writer.WriteInt64(0);
        });

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCItemIO.Opnums.WriteVqtAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.LastItemIds).IsEquivalentTo(new[] { "Item.A" });
    }

    [Test]
    public async Task IOPCItemIO_unknown_opnum_returns_E_NOTIMPL()
    {
        var dispatcher = new IOPCItemIOServerDispatcher(new StubItemIO());

        DispatchResult result = await dispatcher.DispatchAsync(99, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
    }

    // ===== IOPCEnumGUID =====

    [Test]
    public async Task IOPCEnumGUID_Next_returns_count_and_guids()
    {
        var impl = new StubEnumGuid
        {
            NextGuids = [Guid.Parse("11111111-1111-1111-1111-111111111111")],
        };
        var dispatcher = new IOPCEnumGUIDServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(5));

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCEnumGUID.Opnums.NextAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.LastNextCount).IsEqualTo(5);
    }

    [Test]
    public async Task IOPCEnumGUID_Reset_dispatches()
    {
        var impl = new StubEnumGuid();
        var dispatcher = new IOPCEnumGUIDServerDispatcher(impl);

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCEnumGUID.Opnums.ResetAsync, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.ResetCallCount).IsEqualTo(1);
    }

    // ===== IOPCServerList =====

    [Test]
    public async Task IOPCServerList_ClsidFromProgId_dispatches_with_string()
    {
        Guid expected = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var impl = new StubServerList { LookupResult = expected };
        var dispatcher = new IOPCServerListServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Some.Server.1"));

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCServerList.Opnums.ClsidFromProgIdAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        var reader = new NdrReader(result.Payload.Span);
        Guid returned = reader.ReadGuid();
        await Assert.That(returned).IsEqualTo(expected);
        await Assert.That(impl.LastProgId).IsEqualTo("Some.Server.1");
    }

    // ===== IOPCShutdown =====

    [Test]
    public async Task IOPCShutdown_ShutdownRequest_dispatches_with_reason_string()
    {
        var impl = new StubShutdown();
        var dispatcher = new IOPCShutdownServerDispatcher(impl);
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Server going down for maintenance"));

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCShutdown.Opnums.ShutdownRequestAsync, payload, CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(impl.LastReason).IsEqualTo("Server going down for maintenance");
    }

    [Test]
    public async Task IOPCShutdown_unknown_opnum_returns_E_NOTIMPL()
    {
        var dispatcher = new IOPCShutdownServerDispatcher(new StubShutdown());

        DispatchResult result = await dispatcher.DispatchAsync(99, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
    }

    // ===== helpers =====

    private static byte[] WritePayload(NdrWriteAction action)
    {
        byte[] buffer = new byte[1024];
        var writer = new NdrWriter(buffer);
        action(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static int ReadInt32(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return reader.ReadInt32();
    }

    private sealed class StubCommon : IOPCCommon
    {
        public int LastLocaleId { get; private set; }

        public int CurrentLocaleId { get; set; }

        public Task SetLocaleIdAsync(int localeId, CancellationToken cancellationToken = default)
        {
            LastLocaleId = localeId;
            return Task.CompletedTask;
        }

        public Task<int> GetLocaleIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CurrentLocaleId);

        public Task<int[]> QueryAvailableLocaleIdsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new[] { 1033 });

        public Task<string> GetErrorStringAsync(int errorCode, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");

        public Task SetClientNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class StubGroupStateMgt : IOPCGroupStateMgt
    {
        public int GetStateCallCount { get; private set; }

        public string? LastSetName { get; private set; }

        public OpcGroupState? CurrentState { get; set; }

        public Task<OpcGroupState> GetStateAsync(CancellationToken cancellationToken = default)
        {
            GetStateCallCount++;
            return Task.FromResult(CurrentState ?? new OpcGroupState(0, 0, "", false, 0, 0, 0f, 0));
        }

        public Task SetStateAsync(int requestedUpdateRate, bool active, int timeBias, float percentDeadband,
            int localeId, int clientGroupHandle, out int revisedUpdateRate,
            CancellationToken cancellationToken = default)
        {
            revisedUpdateRate = requestedUpdateRate;
            return Task.CompletedTask;
        }

        public Task SetNameAsync(string name, CancellationToken cancellationToken = default)
        {
            LastSetName = name;
            return Task.CompletedTask;
        }

        public Task<Opc.Classic.Dcom.IOpcInterfaceRef> CloneGroupAsync(string name, Guid requestedInterfaceId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Opc.Classic.Dcom.IOpcInterfaceRef>(new Opc.Classic.Dcom.OpcInterfaceRef(
                iid: requestedInterfaceId, flags: 0, publicRefs: 1, oxid: 1, oid: 0,
                ipid: Guid.NewGuid(), securityOffset: 0, resolverBindings: Array.Empty<ushort>()));
    }

    private sealed class StubGroupStateMgt2 : IOPCGroupStateMgt2
    {
        public int CurrentKeepAlive { get; set; }

        public Task<int> SetKeepAliveAsync(int keepAliveTime, CancellationToken cancellationToken = default)
        {
            int previous = CurrentKeepAlive;
            CurrentKeepAlive = keepAliveTime;
            return Task.FromResult(previous);
        }

        public Task<int> GetKeepAliveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CurrentKeepAlive);
    }

    private sealed class StubConnectionPoint : IConnectionPoint
    {
        public Guid ConnectionIid { get; set; }

        public int LastUnadvisedCookie { get; private set; }

        public Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(ConnectionIid);

        public Task<int> AdviseAsync(Opc.Classic.Dcom.IOpcInterfaceRef sink, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default)
        {
            LastUnadvisedCookie = cookie;
            return Task.CompletedTask;
        }
    }

    private sealed class StubShutdown : IOPCShutdown
    {
        public string? LastReason { get; private set; }

        public Task ShutdownRequestAsync(string reason, CancellationToken cancellationToken = default)
        {
            LastReason = reason;
            return Task.CompletedTask;
        }
    }

    private sealed class StubItemIO : IOPCItemIO
    {
        public string[] LastItemIds { get; private set; } = Array.Empty<string>();

        public Task ReadAsync(string[] itemIds, int[] maxAges,
            out OpcVariant[] values, out ushort[] qualities, out long[] timestamps, out int[] errors,
            CancellationToken cancellationToken = default)
        {
            LastItemIds = itemIds;
            values = new OpcVariant[itemIds.Length];
            qualities = new ushort[itemIds.Length];
            timestamps = new long[itemIds.Length];
            errors = new int[itemIds.Length];
            return Task.CompletedTask;
        }

        public Task<int[]> WriteVqtAsync(string[] itemIds, OpcItemVqt[] values, CancellationToken cancellationToken = default)
        {
            LastItemIds = itemIds;
            return Task.FromResult(new int[itemIds.Length]);
        }
    }

    private sealed class StubEnumGuid : IOPCEnumGUID
    {
        public int LastNextCount { get; private set; }

        public int ResetCallCount { get; private set; }

        public Guid[] NextGuids { get; set; } = Array.Empty<Guid>();

        public Task<Guid[]> NextAsync(int count, CancellationToken cancellationToken = default)
        {
            LastNextCount = count;
            return Task.FromResult(NextGuids);
        }

        public Task SkipAsync(int count, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task ResetAsync(CancellationToken cancellationToken = default)
        {
            ResetCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class StubServerList : IOPCServerList
    {
        public string? LastProgId { get; private set; }

        public Guid LookupResult { get; set; }

        public Task<Guid> ClsidFromProgIdAsync(string progId, CancellationToken cancellationToken = default)
        {
            LastProgId = progId;
            return Task.FromResult(LookupResult);
        }

        public Task<global::Opc.Classic.Dcom.IOpcInterfaceRef> EnumClassesOfCategoriesAsync(
            Guid[] implementedCategories,
            Guid[] requiredCategories,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<global::Opc.Classic.Dcom.IOpcInterfaceRef>(
                new global::Opc.Classic.Dcom.OpcInterfaceRef(Guid.Empty, 0, 0, 0, 0, Guid.Empty, 0, Array.Empty<ushort>()));

        public Task GetClassDetailsAsync(
            Guid clsid,
            out string progId,
            out string userType,
            CancellationToken cancellationToken = default)
        {
            progId = "Stub.ProgId";
            userType = "Stub.UserType";
            return Task.CompletedTask;
        }
    }
}
