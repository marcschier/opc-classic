//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Opc.Classic.Hosting.Tests;

public sealed class ClsidRegistryTests
{
    [Test]
    public async Task InMemoryClsidRegistry_Register_then_TryResolve_round_trips()
    {
        var registration = CreateRegistration();
        var registry = new InMemoryClsidRegistry();

        registry.Register(registration);
        var resolved = registry.TryResolve(registration.Clsid, out var actual);

        await Assert.That(resolved).IsTrue();
        await Assert.That(actual).IsEqualTo(registration);
    }

    [Test]
    public async Task TryResolveProgId_is_case_insensitive()
    {
        var registration = CreateRegistration(progId: "Vendor.Server.1");
        var registry = new InMemoryClsidRegistry([registration]);

        var resolved = registry.TryResolveProgId("vendor.server.1", out var actual);

        await Assert.That(resolved).IsTrue();
        await Assert.That(actual).IsEqualTo(registration);
    }

    [Test]
    public async Task Enumerate_returns_all_registered_entries()
    {
        var first = CreateRegistration(
            clsid: Guid.Parse("10138C2C-0000-0000-0000-000000000001"),
            progId: "Vendor.First.1");
        var second = CreateRegistration(
            clsid: Guid.Parse("10138C2C-0000-0000-0000-000000000002"),
            progId: "Vendor.Second.1");
        var registry = new InMemoryClsidRegistry([first, second]);

        var entries = registry.Enumerate()
            .OrderBy(static entry => entry.ProgId, StringComparer.Ordinal)
            .ToArray();

        await Assert.That(entries.Length).IsEqualTo(2);
        await Assert.That(entries[0]).IsEqualTo(first);
        await Assert.That(entries[1]).IsEqualTo(second);
    }

    [Test]
    public async Task Unregister_removes_clsid_and_progid_entries()
    {
        var registration = CreateRegistration();
        var registry = new InMemoryClsidRegistry([registration]);

        registry.Unregister(registration.Clsid);
        var foundByClsid = registry.TryResolve(registration.Clsid, out _);
        var foundByProgId = registry.TryResolveProgId(registration.ProgId, out _);
        var remaining = registry.Enumerate().Count();

        await Assert.That(foundByClsid).IsFalse();
        await Assert.That(foundByProgId).IsFalse();
        await Assert.That(remaining).IsEqualTo(0);
    }

    [Test]
    public async Task ConfigurationClsidRegistry_binds_from_ClassicServers_section()
    {
        var expectedCategory = Guid.Parse("63D5F432-CFE4-11D1-B2C8-0060083BA1FB");
        var registration = CreateRegistration(
            clsid: Guid.Parse("10138C2C-0000-0000-0000-000000000003"),
            progId: "Matrikon.OPC.Simulation.1",
            friendlyName: "Matrikon Sim",
            implementedCategories: [expectedCategory]);

        var registry = ConfigurationClsidRegistry.FromConfiguration(CreateConfiguration(registration));
        var resolved = registry.TryResolve(registration.Clsid, out var actual);

        await Assert.That(resolved).IsTrue();
        await Assert.That(actual.Clsid).IsEqualTo(registration.Clsid);
        await Assert.That(actual.ProgId).IsEqualTo(registration.ProgId);
        await Assert.That(actual.AssemblyName).IsEqualTo(registration.AssemblyName);
        await Assert.That(actual.TypeName).IsEqualTo(registration.TypeName);
        await Assert.That(actual.FriendlyName).IsEqualTo(registration.FriendlyName);
        await Assert.That(actual.ImplementedCategories).IsNotNull();
        await Assert.That(actual.ImplementedCategories!.Count).IsEqualTo(1);
        await Assert.That(actual.ImplementedCategories[0]).IsEqualTo(expectedCategory);
    }

    [Test]
    public async Task OpcClsidRegistration_record_equality_value_semantics()
    {
        var clsid = Guid.Parse("10138C2C-0000-0000-0000-000000000004");
        var first = CreateRegistration(clsid: clsid, friendlyName: "Vendor Server");
        var second = CreateRegistration(clsid: clsid, friendlyName: "Vendor Server");

        var equalOperatorResult = first == second;

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(equalOperatorResult).IsTrue();
    }

    private static OpcClsidRegistration CreateRegistration(
        Guid? clsid = null,
        string progId = "Vendor.Server.1",
        string assemblyName = "Vendor.Server",
        string typeName = "Vendor.Server.ServerClass",
        string? friendlyName = null,
        IReadOnlyList<Guid>? implementedCategories = null) =>
        new(
            clsid ?? Guid.Parse("10138C2C-0000-0000-0000-000000000000"),
            progId,
            assemblyName,
            typeName,
            friendlyName,
            implementedCategories);

    private static InMemoryConfigurationSection CreateConfiguration(params OpcClsidRegistration[] registrations)
    {
        InMemoryConfigurationSection root = new(string.Empty, string.Empty);
        var servers = root.GetOrAddSection("Opc.Classic").GetOrAddSection("Servers");

        for (var i = 0; i < registrations.Length; i++)
        {
            var registration = registrations[i];
            var section = servers.GetOrAddSection(i.ToString(CultureInfo.InvariantCulture));
            section.GetOrAddSection("Clsid").Value = registration.Clsid.ToString("D");
            section.GetOrAddSection("ProgId").Value = registration.ProgId;
            section.GetOrAddSection("AssemblyName").Value = registration.AssemblyName;
            section.GetOrAddSection("TypeName").Value = registration.TypeName;

            if (registration.FriendlyName is not null)
            {
                section.GetOrAddSection("FriendlyName").Value = registration.FriendlyName;
            }

            if (registration.ImplementedCategories is not null)
            {
                var categories = section.GetOrAddSection("ImplementedCategories");
                for (var categoryIndex = 0; categoryIndex < registration.ImplementedCategories.Count; categoryIndex++)
                {
                    categories.GetOrAddSection(categoryIndex.ToString(CultureInfo.InvariantCulture)).Value =
                        registration.ImplementedCategories[categoryIndex].ToString("D");
                }
            }
        }

        return root;
    }

    private sealed class InMemoryConfigurationSection : IConfigurationSection
    {
        private readonly List<InMemoryConfigurationSection> _children = new();

        public InMemoryConfigurationSection(string key, string path)
        {
            Key = key;
            Path = path;
        }

        public string Key { get; }

        public string Path { get; }

        public string? Value { get; set; }

        public string? this[string key]
        {
            get => GetSection(key).Value;
            set => GetOrAddSection(key).Value = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => _children;

        public IChangeToken GetReloadToken() => NoopChangeToken.Instance;

        public IConfigurationSection GetSection(string key)
        {
            var current = this;
            foreach (var segment in key.Split(':', StringSplitOptions.RemoveEmptyEntries))
            {
                var next = current._children.FirstOrDefault(child =>
                    string.Equals(child.Key, segment, StringComparison.OrdinalIgnoreCase));
                if (next is null)
                {
                    return new InMemoryConfigurationSection(segment, CreateChildPath(current.Path, segment));
                }

                current = next;
            }

            return current;
        }

        public InMemoryConfigurationSection GetOrAddSection(string key)
        {
            var current = this;
            foreach (var segment in key.Split(':', StringSplitOptions.RemoveEmptyEntries))
            {
                var next = current._children.FirstOrDefault(child =>
                    string.Equals(child.Key, segment, StringComparison.OrdinalIgnoreCase));
                if (next is null)
                {
                    next = new InMemoryConfigurationSection(segment, CreateChildPath(current.Path, segment));
                    current._children.Add(next);
                }

                current = next;
            }

            return current;
        }

        private static string CreateChildPath(string parentPath, string key) =>
            string.IsNullOrEmpty(parentPath) ? key : string.Concat(parentPath, ":", key);
    }

    private sealed class NoopChangeToken : IChangeToken
    {
        public static NoopChangeToken Instance { get; } = new();

        public bool HasChanged => false;

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NoopDisposable.Instance;
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
