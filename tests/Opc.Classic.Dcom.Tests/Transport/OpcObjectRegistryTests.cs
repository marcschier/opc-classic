//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class OpcObjectRegistryTests
{
    private static readonly Guid Iface1 = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid Iface2 = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa");

    [Test]
    public async Task Register_returns_fresh_ipid_each_call()
    {
        var registry = new OpcObjectRegistry();
        var dispatchers = new Dictionary<Guid, IOpcServerDispatcher> { [Iface1] = new StubDispatcher() };

        Guid first = registry.Register(dispatchers);
        Guid second = registry.Register(dispatchers);

        await Assert.That(first).IsNotEqualTo(second);
        await Assert.That(registry.Count).IsEqualTo(2);
    }

    [Test]
    public async Task TryGetDispatcher_returns_registered_dispatcher_for_known_pair()
    {
        var registry = new OpcObjectRegistry();
        var dispatcher = new StubDispatcher();
        Guid ipid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher> { [Iface1] = dispatcher });

        bool found = registry.TryGetDispatcher(ipid, Iface1, out IOpcServerDispatcher resolved);

        await Assert.That(found).IsTrue();
        await Assert.That(resolved).IsSameReferenceAs(dispatcher);
    }

    [Test]
    public async Task TryGetDispatcher_returns_false_for_unknown_ipid()
    {
        var registry = new OpcObjectRegistry();
        registry.Register(new Dictionary<Guid, IOpcServerDispatcher> { [Iface1] = new StubDispatcher() });

        bool found = registry.TryGetDispatcher(Guid.NewGuid(), Iface1, out _);

        await Assert.That(found).IsFalse();
    }

    [Test]
    public async Task TryGetDispatcher_returns_false_for_unknown_interface_on_known_ipid()
    {
        var registry = new OpcObjectRegistry();
        Guid ipid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher> { [Iface1] = new StubDispatcher() });

        bool found = registry.TryGetDispatcher(ipid, Iface2, out _);

        await Assert.That(found).IsFalse();
    }

    [Test]
    public async Task Unregister_removes_object()
    {
        var registry = new OpcObjectRegistry();
        Guid ipid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher> { [Iface1] = new StubDispatcher() });

        await Assert.That(registry.Unregister(ipid)).IsTrue();
        await Assert.That(registry.Contains(ipid)).IsFalse();
        await Assert.That(registry.Unregister(ipid)).IsFalse();
    }

    [Test]
    public async Task RegisterWithIpid_accepts_caller_supplied_id()
    {
        var registry = new OpcObjectRegistry();
        var dispatcher = new StubDispatcher();
        Guid ipid = Guid.Parse("12345678-1234-1234-1234-1234567890ab");

        bool added = registry.RegisterWithIpid(ipid, new Dictionary<Guid, IOpcServerDispatcher> { [Iface1] = dispatcher });

        await Assert.That(added).IsTrue();
        await Assert.That(registry.TryGetDispatcher(ipid, Iface1, out IOpcServerDispatcher resolved)).IsTrue();
        await Assert.That(resolved).IsSameReferenceAs(dispatcher);
    }

    [Test]
    public async Task RegisterWithIpid_rejects_duplicates()
    {
        var registry = new OpcObjectRegistry();
        Guid ipid = Guid.NewGuid();
        registry.RegisterWithIpid(ipid, new Dictionary<Guid, IOpcServerDispatcher> { [Iface1] = new StubDispatcher() });

        bool secondAttempt = registry.RegisterWithIpid(ipid, new Dictionary<Guid, IOpcServerDispatcher> { [Iface2] = new StubDispatcher() });

        await Assert.That(secondAttempt).IsFalse();
    }

    [Test]
    public async Task Register_throws_on_null_dispatcher_map()
    {
        var registry = new OpcObjectRegistry();
        await Assert.That(() => { _ = registry.Register(null!); }).Throws<ArgumentNullException>();
    }

    private sealed class StubDispatcher : IOpcServerDispatcher
    {
        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
            ValueTask.FromResult(DispatchResult.Success(Array.Empty<byte>()));
    }
}
