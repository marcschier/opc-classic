// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable MA0048 // Configuration persistence contracts are kept together.

namespace Opc.Classic.Dx;

/// <summary>
/// A complete DX configuration persisted by an <see cref="IDxConfigurationStore"/>.
/// </summary>
public sealed record DxConfiguration
{
    /// <summary>
    /// An empty configuration.
    /// </summary>
    public static DxConfiguration Empty { get; } = new();

    /// <summary>
    /// Creates a configuration from source servers and transfer connections.
    /// </summary>
    public DxConfiguration(
        DxSourceServer[]? sourceServers = null,
        DxConnection[]? connections = null)
    {
        SourceServers = CloneSourceServers(sourceServers);
        Connections = CloneConnections(connections);
    }

    /// <summary>
    /// Registered source servers.
    /// </summary>
    public DxSourceServer[] SourceServers { get; init; }

    /// <summary>
    /// Configured source-to-target connections.
    /// </summary>
    public DxConnection[] Connections { get; init; }

    internal DxConfiguration Copy() => new(SourceServers, Connections);

    private static DxSourceServer[] CloneSourceServers(DxSourceServer[]? sourceServers) =>
        sourceServers is null || sourceServers.Length == 0
            ? Array.Empty<DxSourceServer>()
            : (DxSourceServer[])sourceServers.Clone();

    private static DxConnection[] CloneConnections(DxConnection[]? connections)
    {
        if (connections is null || connections.Length == 0)
        {
            return Array.Empty<DxConnection>();
        }

        var copy = new DxConnection[connections.Length];
        for (var i = 0; i < connections.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(connections[i]);
            copy[i] = connections[i] with
            {
                BrowsePaths = (string[])connections[i].BrowsePaths.Clone(),
                DefaultOverrideValue = DxConfigurationValueCloner.Clone(
                    connections[i].DefaultOverrideValue),
                SubstituteValue = DxConfigurationValueCloner.Clone(
                    connections[i].SubstituteValue),
            };
        }

        return copy;
    }
}

/// <summary>
/// A configuration and its monotonically increasing store version.
/// </summary>
public sealed record DxConfigurationSnapshot(long Version, DxConfiguration Configuration)
{
    /// <summary>
    /// The initial empty snapshot.
    /// </summary>
    public static DxConfigurationSnapshot Empty { get; } = new(0, DxConfiguration.Empty);
}

/// <summary>
/// Persists versioned DX configuration without relying on runtime reflection.
/// </summary>
public interface IDxConfigurationStore
{
    /// <summary>
    /// Loads the latest atomically committed configuration.
    /// </summary>
    ValueTask<DxConfigurationSnapshot> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically saves a configuration when <paramref name="expectedVersion"/> is current.
    /// </summary>
    ValueTask<DxConfigurationSnapshot> SaveAsync(
        DxConfiguration configuration,
        long expectedVersion,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Base exception for DX configuration persistence failures.
/// </summary>
public class DxConfigurationStoreException : Exception
{
    /// <summary>
    /// Creates a configuration-store exception.
    /// </summary>
    public DxConfigurationStoreException() { }

    /// <summary>
    /// Creates a configuration-store exception.
    /// </summary>
    public DxConfigurationStoreException(string message) : base(message) { }

    /// <summary>
    /// Creates a configuration-store exception with an inner exception.
    /// </summary>
    public DxConfigurationStoreException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Indicates that persisted configuration is malformed or incomplete.
/// </summary>
public sealed class DxConfigurationCorruptException : DxConfigurationStoreException
{
    /// <summary>
    /// Creates a corruption exception.
    /// </summary>
    public DxConfigurationCorruptException() { }

    /// <summary>
    /// Creates a corruption exception.
    /// </summary>
    public DxConfigurationCorruptException(string message) : base(message) { }

    /// <summary>
    /// Creates a corruption exception with an inner exception.
    /// </summary>
    public DxConfigurationCorruptException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Indicates an unsupported persisted JSON format version.
/// </summary>
public sealed class DxConfigurationFormatVersionException : DxConfigurationStoreException
{
    /// <summary>
    /// Creates a format-version exception.
    /// </summary>
    public DxConfigurationFormatVersionException() { }

    /// <summary>
    /// Creates a format-version exception.
    /// </summary>
    public DxConfigurationFormatVersionException(string message) : base(message) { }

    /// <summary>
    /// Creates a format-version exception with an inner exception.
    /// </summary>
    public DxConfigurationFormatVersionException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Creates a format-version exception.
    /// </summary>
    public DxConfigurationFormatVersionException(int actualVersion, int supportedVersion)
        : base($"DX configuration format version {actualVersion} is not supported; expected {supportedVersion}.")
    {
        ActualVersion = actualVersion;
        SupportedVersion = supportedVersion;
    }

    /// <summary>
    /// Version found in the persisted document.
    /// </summary>
    public int ActualVersion { get; }

    /// <summary>
    /// Version understood by this implementation.
    /// </summary>
    public int SupportedVersion { get; }
}

/// <summary>
/// Indicates an optimistic-concurrency conflict while saving configuration.
/// </summary>
public sealed class DxConfigurationVersionException : DxConfigurationStoreException
{
    /// <summary>
    /// Creates a configuration-version conflict.
    /// </summary>
    public DxConfigurationVersionException() { }

    /// <summary>
    /// Creates a configuration-version conflict.
    /// </summary>
    public DxConfigurationVersionException(string message) : base(message) { }

    /// <summary>
    /// Creates a configuration-version conflict with an inner exception.
    /// </summary>
    public DxConfigurationVersionException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Creates a configuration-version conflict.
    /// </summary>
    public DxConfigurationVersionException(long expectedVersion, long actualVersion)
        : base($"DX configuration version conflict: expected {expectedVersion}, current version is {actualVersion}.")
    {
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    /// <summary>
    /// Version supplied by the writer.
    /// </summary>
    public long ExpectedVersion { get; }

    /// <summary>
    /// Current version in the store.
    /// </summary>
    public long ActualVersion { get; }
}

/// <summary>
/// Thread-safe, versioned in-memory DX configuration persistence.
/// </summary>
public sealed class InMemoryDxConfigurationStore : IDxConfigurationStore, IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DxConfigurationSnapshot _snapshot;

    /// <summary>
    /// Creates an empty store or seeds it with an initial configuration.
    /// </summary>
    public InMemoryDxConfigurationStore(
        DxConfiguration? initialConfiguration = null,
        long initialVersion = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialVersion);
        _snapshot = new(
            initialVersion,
            (initialConfiguration ?? DxConfiguration.Empty).Copy());
    }

    /// <inheritdoc />
    public async ValueTask<DxConfigurationSnapshot> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return CloneSnapshot(_snapshot);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<DxConfigurationSnapshot> SaveAsync(
        DxConfiguration configuration,
        long expectedVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedVersion);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_snapshot.Version != expectedVersion)
            {
                throw new DxConfigurationVersionException(expectedVersion, _snapshot.Version);
            }

            _snapshot = new(checked(expectedVersion + 1), configuration.Copy());
            return CloneSnapshot(_snapshot);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose() => _gate.Dispose();

    internal static DxConfigurationSnapshot CloneSnapshot(DxConfigurationSnapshot snapshot) =>
        new(snapshot.Version, snapshot.Configuration.Copy());
}

internal static class DxConfigurationValueCloner
{
    public static OpcVariant? Clone(OpcVariant? value) =>
        value.HasValue ? Clone(value.Value) : null;

    private static OpcVariant Clone(OpcVariant value)
    {
        if (value.AsSafeArray() is { } safeArray)
        {
            return new OpcVariant(value.Type, Clone(safeArray));
        }
        if (value.AsRecord() is { } record)
        {
            return new OpcVariant(value.Type, Clone(record));
        }
        if (value.Type == VarType.VT_VARIANT && value.Boxed is OpcVariant nested)
        {
            return new OpcVariant(value.Type, Clone(nested));
        }

        return new OpcVariant(value.Type, CloneObject(value.Boxed));
    }

    private static OpcSafeArray Clone(OpcSafeArray value) =>
        new(
            value.ElementType,
            CloneArray(value.Data),
            value.Lengths.ToArray(),
            value.LowerBounds.ToArray(),
            value.Features);

    private static OpcRecordValue Clone(OpcRecordValue value) =>
        new(
            value.RecordInfoId,
            value.Values.Select(CloneObject).ToArray());

    private static Array CloneArray(Array value)
    {
        var copy = (Array)value.Clone();
        for (var i = 0; i < copy.Length; i++)
        {
            var original = value.GetValue(i);
            var cloned = CloneObject(original);
            if (!ReferenceEquals(original, cloned))
            {
                copy.SetValue(cloned, i);
            }
        }

        return copy;
    }

    private static object? CloneObject(object? value) =>
        value switch
        {
            OpcVariant variant => Clone(variant),
            OpcSafeArray safeArray => Clone(safeArray),
            OpcRecordValue record => Clone(record),
            Array array => CloneArray(array),
            _ => value,
        };
}
