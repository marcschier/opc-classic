//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Discovery.Tests;

public sealed class LocalEnumTests {
    [Test]
    public async Task LocalEnum_yields_entries_from_configuration() {
        var configured = new OpcServerEntry(
            Guid.Parse("10138C2C-0000-0000-0000-000000000001"),
            "Matrikon.OPC.Simulation.1",
            "Matrikon Sim",
            "localhost",
            new[] { OpcGuids.CATID_OPCDAServer20 });
        var discovery = new LocalEnum(CreateConfiguration(configured), includeWindowsRegistry: false);

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(1);
        await Assert.That(entries[0].Clsid).IsEqualTo(configured.Clsid);
        await Assert.That(entries[0].ProgId).IsEqualTo(configured.ProgId);
        await Assert.That(entries[0].FriendlyName).IsEqualTo(configured.FriendlyName);
        await Assert.That(entries[0].Host).IsEqualTo(configured.Host);
        await Assert.That(entries[0].SupportedCategories.Count).IsEqualTo(1);
        await Assert.That(entries[0].SupportedCategories[0]).IsEqualTo(OpcGuids.CATID_OPCDAServer20);
    }

    [Test]
    public async Task LocalEnum_empty_configuration_yields_no_entries() {
        var discovery = new LocalEnum(CreateConfiguration(), includeWindowsRegistry: false);

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(0);
    }

    [Test]
    public async Task OpcServerEntry_record_equality() {
        var categories = Array.Empty<Guid>();
        var first = new OpcServerEntry(
            Guid.Parse("10138C2C-0000-0000-0000-000000000002"),
            "Vendor.Server.1",
            "Vendor Server",
            "localhost",
            categories);
        var second = new OpcServerEntry(
            Guid.Parse("10138C2C-0000-0000-0000-000000000002"),
            "Vendor.Server.1",
            "Vendor Server",
            "localhost",
            categories);

        await Assert.That(first).IsEqualTo(second);
    }

    [Test]
    public async Task IDiscovery_contract_compiles() {
        IOpcDiscovery[] implementations =
        {
            new LocalEnum(Array.Empty<OpcServerEntry>(), includeWindowsRegistry: false),
            new RemoteRegistryEnum("localhost", new NetworkCredential()),
            new OpcEnumClient(OpcUrl.Parse("opcda://localhost/OPC.ServerList.1")),
        };

        await Assert.That(implementations.Length).IsEqualTo(3);
        await Assert.That(implementations.All(static discovery => discovery is not null)).IsTrue();
    }

    private static async Task<List<OpcServerEntry>> ToListAsync(IOpcDiscovery discovery) {
        var entries = new List<OpcServerEntry>();
        await foreach (var entry in discovery.DiscoverAsync()) {
            entries.Add(entry);
        }

        return entries;
    }

    private static IConfiguration CreateConfiguration(params OpcServerEntry[] entries) {
        var root = new InMemoryConfigurationSection(string.Empty, string.Empty);
        var servers = root.GetOrAddSection("Opc.Classic").GetOrAddSection("Servers");

        for (var i = 0; i < entries.Length; i++) {
            var entry = entries[i];
            var section = servers.GetOrAddSection(i.ToString(CultureInfo.InvariantCulture));
            section.GetOrAddSection("Clsid").Value = entry.Clsid.ToString("D");
            section.GetOrAddSection("ProgId").Value = entry.ProgId;
            section.GetOrAddSection("FriendlyName").Value = entry.FriendlyName;
            section.GetOrAddSection("Host").Value = entry.Host;

            var categories = section.GetOrAddSection("SupportedCategories");
            for (var categoryIndex = 0; categoryIndex < entry.SupportedCategories.Count; categoryIndex++) {
                categories.GetOrAddSection(categoryIndex.ToString(CultureInfo.InvariantCulture)).Value =
                    entry.SupportedCategories[categoryIndex].ToString("D");
            }
        }

        return root;
    }

    private sealed class InMemoryConfigurationSection : IConfigurationSection {
        private readonly List<InMemoryConfigurationSection> _children = new();

        public InMemoryConfigurationSection(string key, string path) {
            Key = key;
            Path = path;
        }

        public string Key { get; }

        public string Path { get; }

        public string? Value { get; set; }

        public string? this[string key] {
            get => GetSection(key).Value;
            set => GetOrAddSection(key).Value = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => _children;

        public IChangeToken GetReloadToken() => NoopChangeToken.Instance;

        public IConfigurationSection GetSection(string key) {
            var current = this;
            foreach (var segment in key.Split(':', StringSplitOptions.RemoveEmptyEntries)) {
                var next = current._children.FirstOrDefault(child =>
                    string.Equals(child.Key, segment, StringComparison.OrdinalIgnoreCase));
                if (next is null) {
                    return new InMemoryConfigurationSection(segment, CreateChildPath(current.Path, segment));
                }

                current = next;
            }

            return current;
        }

        public InMemoryConfigurationSection GetOrAddSection(string key) {
            var current = this;
            foreach (var segment in key.Split(':', StringSplitOptions.RemoveEmptyEntries)) {
                var next = current._children.FirstOrDefault(child =>
                    string.Equals(child.Key, segment, StringComparison.OrdinalIgnoreCase));
                if (next is null) {
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

    private sealed class NoopChangeToken : IChangeToken {
        public static NoopChangeToken Instance { get; } = new();

        public bool HasChanged => false;

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NoopDisposable.Instance;
    }

    private sealed class NoopDisposable : IDisposable {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose() {
        }
    }
}
