//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants.

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using TUnit.Core;

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
}
