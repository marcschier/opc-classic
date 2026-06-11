//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Core;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests.Activation;

public sealed class ClassFactoryRegistryTests
{
    [Test]
    public async Task Register_then_lookup_returns_factory()
    {
        Guid clsid = Guid.NewGuid();
        var registry = new ClassFactoryRegistry();
        registry.Register(clsid, _ => new TestServer());

        bool found = registry.TryResolve(clsid, out IClassFactory factory);

        await Assert.That(found).IsTrue();
        await Assert.That(factory).IsNotNull();
        await Assert.That(registry.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Unregister_removes_factory()
    {
        Guid clsid = Guid.NewGuid();
        var registry = new ClassFactoryRegistry();
        registry.Register(clsid, _ => new TestServer());

        bool removed = registry.Unregister(clsid);
        bool found = registry.TryResolve(clsid, out _);

        await Assert.That(removed).IsTrue();
        await Assert.That(found).IsFalse();
        await Assert.That(registry.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Register_replaces_existing_factory()
    {
        Guid clsid = Guid.NewGuid();
        var registry = new ClassFactoryRegistry();
        registry.Register(clsid, _ => new TestServer { Name = "first" });
        registry.Register(clsid, _ => new TestServer { Name = "second" });

        registry.TryResolve(clsid, out IClassFactory factory);
        ClassFactoryActivationResult result = factory.CreateInstance(new ClassFactoryActivationContext(
            clsid,
            Guid.NewGuid(),
            ActivationProperties.Empty));

        await Assert.That(((TestServer)result.Instance).Name).IsEqualTo("second");
        await Assert.That(registry.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Concurrent_register_and_lookup_preserves_entries()
    {
        const int count = 128;
        var clsids = new Guid[count];
        for (int i = 0; i < clsids.Length; i++)
        {
            clsids[i] = Guid.NewGuid();
        }

        var registry = new ClassFactoryRegistry();

        Parallel.For(0, count, i =>
        {
            registry.Register(clsids[i], _ => new TestServer { Name = i.ToString() });
            _ = registry.TryResolve(clsids[i], out _);
        });

        int resolved = 0;
        for (int i = 0; i < clsids.Length; i++)
        {
            if (registry.TryResolve(clsids[i], out _))
            {
                resolved++;
            }
        }

        await Assert.That(resolved).IsEqualTo(count);
        await Assert.That(registry.Count).IsEqualTo(count);
    }

    private sealed class TestServer
    {
        public string Name { get; init; } = string.Empty;
    }
}
