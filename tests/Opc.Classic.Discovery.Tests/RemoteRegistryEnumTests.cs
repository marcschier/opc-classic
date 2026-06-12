//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Net;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Discovery.Tests;

public sealed class RemoteRegistryEnumTests
{
    private const string Host = "opc-host";

    [Test]
    public async Task RemoteRegistryEnum_yields_servers_from_winreg_transport()
    {
        var clsid = Guid.Parse("10138C2C-0000-0000-0000-000000000021");
        var registry = new FakeRemoteRegistryReader();
        AddOpcServerRegistration(
            registry,
            clsid,
            "Vendor.Remote.1",
            "Vendor Remote Server",
            OpcGuids.CATID_OPCDAServer20);
        var factory = new FakeRemoteRegistryReaderFactory(registry);
        var discovery = new RemoteRegistryEnum(Host, new NetworkCredential("user", "password", "domain"), factory);

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(1);
        await Assert.That(entries[0].Clsid).IsEqualTo(clsid);
        await Assert.That(entries[0].ProgId).IsEqualTo("Vendor.Remote.1");
        await Assert.That(entries[0].FriendlyName).IsEqualTo("Vendor Remote Server");
        await Assert.That(entries[0].Host).IsEqualTo(Host);
        await Assert.That(entries[0].SupportedCategories.Count).IsEqualTo(1);
        await Assert.That(entries[0].SupportedCategories[0]).IsEqualTo(OpcGuids.CATID_OPCDAServer20);
        await Assert.That(factory.OpenedHost).IsEqualTo(Host);
        await Assert.That(registry.IsDisposed).IsTrue();
    }

    [Test]
    public async Task RemoteRegistryEnum_returns_empty_when_host_unreachable()
    {
        var logger = new RecordingLogger<RemoteRegistryEnum>();
        var discovery = new RemoteRegistryEnum(
            Host,
            new NetworkCredential(),
            new FakeRemoteRegistryReaderFactory(new IOException("host unreachable")),
            logger);

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(0);
        await Assert.That(logger.Records.Count).IsEqualTo(1);
        await Assert.That(logger.Records[0].LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(logger.Records[0].Message).Contains("Remote-registry enumeration failed");
    }

    [Test]
    public async Task RemoteRegistryEnum_returns_empty_when_access_denied()
    {
        var logger = new RecordingLogger<RemoteRegistryEnum>();
        var discovery = new RemoteRegistryEnum(
            Host,
            new NetworkCredential(),
            new FakeRemoteRegistryReaderFactory(new UnauthorizedAccessException("access denied")),
            logger);

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(0);
        await Assert.That(logger.Records.Count).IsEqualTo(1);
        await Assert.That(logger.Records[0].LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(logger.Records[0].Exception).IsNotNull();
    }

    [Test]
    public async Task RemoteRegistryEnum_returns_empty_when_no_servers_are_registered()
    {
        var registry = new FakeRemoteRegistryReader()
            .AddKey(@"SOFTWARE\Classes\CLSID")
            .AddKey($@"SOFTWARE\Classes\Component Categories\{OpcGuids.CATID_OPCDAServer20:B}\Implementations");
        var discovery = new RemoteRegistryEnum(
            Host,
            new NetworkCredential(),
            new FakeRemoteRegistryReaderFactory(registry));

        var entries = await ToListAsync(discovery);

        await Assert.That(entries.Count).IsEqualTo(0);
        await Assert.That(registry.IsDisposed).IsTrue();
    }

    private static async Task<List<OpcServerEntry>> ToListAsync(IOpcDiscovery discovery)
    {
        var entries = new List<OpcServerEntry>();
        await foreach (var entry in discovery.DiscoverAsync())
        {
            entries.Add(entry);
        }

        return entries;
    }

    private static void AddOpcServerRegistration(
        FakeRemoteRegistryReader registry,
        Guid clsid,
        string progId,
        string friendlyName,
        Guid categoryId)
    {
        var clsidText = clsid.ToString("B");
        var clsidKeyPath = $@"SOFTWARE\Classes\CLSID\{clsidText}";
        var opcServerPath = $@"{clsidKeyPath}\OPCServer";
        var implementedCategoryPath = $@"{clsidKeyPath}\Implemented Categories\{categoryId:B}";
        var categoryImplementationPath =
            $@"SOFTWARE\Classes\Component Categories\{categoryId:B}\Implementations\{clsidText}";

        registry
            .AddKey(opcServerPath)
            .AddStringValue(opcServerPath, "ProgID", progId)
            .AddStringValue(clsidKeyPath, null, friendlyName)
            .AddKey(implementedCategoryPath)
            .AddKey(categoryImplementationPath);
    }

    private sealed class FakeRemoteRegistryReaderFactory : IRemoteRegistryReaderFactory
    {
        private readonly IRemoteRegistryReader? _reader;
        private readonly Exception? _exception;

        public FakeRemoteRegistryReaderFactory(IRemoteRegistryReader reader)
        {
            _reader = reader;
        }

        public FakeRemoteRegistryReaderFactory(Exception exception)
        {
            _exception = exception;
        }

        public string? OpenedHost { get; private set; }

        public IRemoteRegistryReader Open(string host, NetworkCredential credentials)
        {
            OpenedHost = host;
            if (_exception is not null)
            {
                throw _exception;
            }

            return _reader ?? throw new InvalidOperationException("A fake registry reader was not configured.");
        }
    }

    private sealed class FakeRemoteRegistryReader : IRemoteRegistryReader
    {
        private readonly Dictionary<string, FakeRegistryKey> _keys = new(StringComparer.OrdinalIgnoreCase);

        public bool IsDisposed { get; private set; }

        public FakeRemoteRegistryReader AddKey(string keyPath)
        {
            _ = GetOrAddKey(Normalize(keyPath));
            return this;
        }

        public FakeRemoteRegistryReader AddStringValue(string keyPath, string? valueName, string value)
        {
            GetOrAddKey(Normalize(keyPath)).Values[ValueName(valueName)] = value;
            return this;
        }

        public IReadOnlyList<string> EnumerateSubKeyNames(string keyPath) =>
            _keys.TryGetValue(Normalize(keyPath), out var key)
                ? key.SubKeyNames.ToArray()
                : Array.Empty<string>();

        public bool KeyExists(string keyPath) => _keys.ContainsKey(Normalize(keyPath));

        public string? ReadStringValue(string keyPath, string? valueName = null) =>
            _keys.TryGetValue(Normalize(keyPath), out var key)
            && key.Values.TryGetValue(ValueName(valueName), out var value)
                ? value
                : null;

        public void Dispose() => IsDisposed = true;

        private FakeRegistryKey GetOrAddKey(string keyPath)
        {
            if (_keys.TryGetValue(keyPath, out var key))
            {
                return key;
            }

            key = new FakeRegistryKey();
            _keys.Add(keyPath, key);

            var separatorIndex = keyPath.LastIndexOf("\\", StringComparison.Ordinal);
            if (separatorIndex > 0)
            {
                var parentPath = keyPath[..separatorIndex];
                var subKeyName = keyPath[(separatorIndex + 1)..];
                GetOrAddKey(parentPath).AddSubKey(subKeyName);
            }

            return key;
        }

        private static string Normalize(string keyPath) => keyPath.Trim('\\');
        private static string ValueName(string? valueName) => valueName ?? string.Empty;
    }

    private sealed class FakeRegistryKey
    {
        private readonly List<string> _subKeyNames = new();

        public IReadOnlyList<string> SubKeyNames => _subKeyNames;
        public Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void AddSubKey(string name)
        {
            if (!_subKeyNames.Any(subKeyName => string.Equals(subKeyName, name, StringComparison.OrdinalIgnoreCase)))
            {
                _subKeyNames.Add(name);
            }
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogRecord> Records { get; } = new();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Records.Add(new LogRecord(logLevel, eventId, formatter(state, exception), exception));
        }
    }

    private sealed record LogRecord(LogLevel LogLevel, EventId EventId, string Message, Exception? Exception);

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
