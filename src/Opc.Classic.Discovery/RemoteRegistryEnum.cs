//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic;
using SharpCifs.Util.Sharpen;
using SharpInterop.Common;
using SharpInterop.Registry;

namespace Opc.Classic.Discovery;

/// <summary>
/// Discovers OPC Classic server registrations through the WINREG RPC protocol over SMB.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The Phase 10 public API intentionally names discovery strategies after OPC enumeration sources.")]
public sealed class RemoteRegistryEnum : IOpcDiscovery
{
    private const string ClassesPath = @"SOFTWARE\Classes";
    private const string ClsidPath = ClassesPath + @"\CLSID";
    private const string ComponentCategoriesPath = ClassesPath + @"\Component Categories";
    private const string ImplementationsSubKey = "Implementations";
    private const string ImplementedCategoriesSubKey = "Implemented Categories";
    private const string OpcServerSubKey = "OPCServer";
    private const int RegistryStringBufferSize = 4096;

    private static readonly Guid[] OpcCategoryIds =
    {
        OpcGuids.CATID_OPCDAServer10,
        OpcGuids.CATID_OPCDAServer20,
        OpcGuids.CATID_OPCDAServer30,
        OpcGuids.CATID_OPCAEServer10,
        OpcGuids.CATID_OPCHDAServer10,
        OpcGuids.CATID_OPCDXServer10,
        OpcGuids.CATID_OPCBatchServer10,
        OpcGuids.CATID_OPCBatchServer20,
        OpcGuids.CATID_OPCCMDServer10,
        OpcGuids.CATID_XMLDAServer10,
    };

    private static readonly Action<ILogger, string, Exception?> RemoteRegistryEnumerationFailed =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(RemoteRegistryEnumerationFailed)),
            "Remote-registry enumeration failed for host {Host}; returning no OPC servers. Consider using OpcEnumClient (OPC.ServerList.1) instead.");

    private readonly IRemoteRegistryReaderFactory _readerFactory;
    private readonly ILogger<RemoteRegistryEnum> _logger;

    /// <summary>
    /// Initializes a remote-registry discovery strategy using the managed SMB/WINREG client.
    /// </summary>
    public RemoteRegistryEnum(string host, NetworkCredential credentials)
        : this(host, credentials, WinRegRemoteRegistryReaderFactory.Instance, NullLogger<RemoteRegistryEnum>.Instance)
    {
    }

    /// <summary>
    /// Initializes a remote-registry discovery strategy with an injectable registry reader.
    /// </summary>
    public RemoteRegistryEnum(
        string host,
        NetworkCredential credentials,
        IRemoteRegistryReaderFactory readerFactory,
        ILogger<RemoteRegistryEnum>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(readerFactory);

        Host = host;
        Credentials = credentials;
        _readerFactory = readerFactory;
        _logger = logger ?? NullLogger<RemoteRegistryEnum>.Instance;
    }

    /// <summary>The remote host whose registry will be enumerated by default.</summary>
    public string Host { get; }

    /// <summary>The credentials that authenticate to the remote registry transport.</summary>
    public NetworkCredential Credentials { get; }

    /// <inheritdoc />
    public async IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        var targetHost = string.IsNullOrWhiteSpace(host) ? Host : host;
        foreach (var entry in EnumerateOrLogFailure(targetHost, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return entry;
        }
    }

    private IReadOnlyList<OpcServerEntry> EnumerateOrLogFailure(string host, CancellationToken cancellationToken)
    {
        try
        {
            return EnumerateRemoteRegistry(host, cancellationToken);
        }
        catch (InteropException ex)
        {
            LogRemoteRegistryEnumerationFailed(host, ex);
        }
        catch (UnknownHostException ex)
        {
            LogRemoteRegistryEnumerationFailed(host, ex);
        }
        catch (IOException ex)
        {
            LogRemoteRegistryEnumerationFailed(host, ex);
        }
        catch (SocketException ex)
        {
            LogRemoteRegistryEnumerationFailed(host, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogRemoteRegistryEnumerationFailed(host, ex);
        }

        return Array.Empty<OpcServerEntry>();
    }

    private List<OpcServerEntry> EnumerateRemoteRegistry(string host, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var reader = _readerFactory.Open(host, Credentials);
        var candidates = new Dictionary<Guid, HashSet<Guid>>();

        AddCandidatesFromCategoryIndexes(reader, candidates, cancellationToken);
        AddCandidatesFromClsidHive(reader, candidates, cancellationToken);

        var clsids = new List<Guid>(candidates.Keys);
        clsids.Sort();

        var entries = new List<OpcServerEntry>();
        foreach (var clsid in clsids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = ReadServerEntry(reader, host, clsid, candidates[clsid]);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private static void AddCandidatesFromCategoryIndexes(
        IRemoteRegistryReader reader,
        Dictionary<Guid, HashSet<Guid>> candidates,
        CancellationToken cancellationToken)
    {
        foreach (var categoryId in OpcCategoryIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var implementationsPath = CombineRegistryPath(
                CombineRegistryPath(ComponentCategoriesPath, categoryId.ToString("B")),
                ImplementationsSubKey);

            foreach (var clsidText in reader.EnumerateSubKeyNames(implementationsPath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Guid.TryParse(clsidText, out var clsid))
                {
                    AddCategory(candidates, clsid, categoryId);
                }
            }
        }
    }

    private static void AddCandidatesFromClsidHive(
        IRemoteRegistryReader reader,
        Dictionary<Guid, HashSet<Guid>> candidates,
        CancellationToken cancellationToken)
    {
        foreach (var clsidText in reader.EnumerateSubKeyNames(ClsidPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Guid.TryParse(clsidText, out var clsid))
            {
                continue;
            }

            var clsidKeyPath = CombineRegistryPath(ClsidPath, clsidText);
            var opcServerPath = CombineRegistryPath(clsidKeyPath, OpcServerSubKey);
            var implementedCategories = ReadImplementedCategories(reader, clsidKeyPath);
            if (!reader.KeyExists(opcServerPath) && implementedCategories.Count == 0)
            {
                continue;
            }

            _ = GetOrAddCandidate(candidates, clsid);
            foreach (var categoryId in implementedCategories)
            {
                AddCategory(candidates, clsid, categoryId);
            }
        }
    }

    private static OpcServerEntry? ReadServerEntry(
        IRemoteRegistryReader reader,
        string host,
        Guid clsid,
        HashSet<Guid> categoriesFromIndex)
    {
        var clsidText = clsid.ToString("B");
        var clsidKeyPath = CombineRegistryPath(ClsidPath, clsidText);
        var opcServerPath = CombineRegistryPath(clsidKeyPath, OpcServerSubKey);
        if (!reader.KeyExists(opcServerPath) && categoriesFromIndex.Count == 0)
        {
            return null;
        }

        var categoryIds = ReadSupportedCategories(reader, clsidKeyPath, categoriesFromIndex);
        var progId = FirstNonEmpty(
            reader.ReadStringValue(opcServerPath, "ProgID"),
            reader.ReadStringValue(CombineRegistryPath(clsidKeyPath, "ProgID")),
            reader.ReadStringValue(CombineRegistryPath(clsidKeyPath, "VersionIndependentProgID")),
            clsidText);
        var friendlyName = FirstNonEmpty(
            reader.ReadStringValue(clsidKeyPath),
            reader.ReadStringValue(opcServerPath, "Description"),
            reader.ReadStringValue(opcServerPath),
            progId);

        return new OpcServerEntry(clsid, progId, friendlyName, host, categoryIds);
    }

    private static Guid[] ReadSupportedCategories(
        IRemoteRegistryReader reader,
        string clsidKeyPath,
        HashSet<Guid> categoriesFromIndex)
    {
        var categoryIds = new HashSet<Guid>(categoriesFromIndex);
        foreach (var categoryId in ReadImplementedCategories(reader, clsidKeyPath))
        {
            categoryIds.Add(categoryId);
        }

        var ordered = new List<Guid>();
        foreach (var categoryId in OpcCategoryIds)
        {
            if (categoryIds.Contains(categoryId))
            {
                ordered.Add(categoryId);
            }
        }

        return ordered.ToArray();
    }

    private static List<Guid> ReadImplementedCategories(IRemoteRegistryReader reader, string clsidKeyPath)
    {
        var implementedCategoriesPath = CombineRegistryPath(clsidKeyPath, ImplementedCategoriesSubKey);
        var categoryIds = new List<Guid>();
        foreach (var categoryText in reader.EnumerateSubKeyNames(implementedCategoriesPath))
        {
            if (Guid.TryParse(categoryText, out var categoryId) && IsOpcCategory(categoryId))
            {
                categoryIds.Add(categoryId);
            }
        }

        return categoryIds;
    }

    private static void AddCategory(Dictionary<Guid, HashSet<Guid>> candidates, Guid clsid, Guid categoryId) =>
        GetOrAddCandidate(candidates, clsid).Add(categoryId);

    private static HashSet<Guid> GetOrAddCandidate(Dictionary<Guid, HashSet<Guid>> candidates, Guid clsid)
    {
        if (!candidates.TryGetValue(clsid, out var categoryIds))
        {
            categoryIds = new HashSet<Guid>();
            candidates.Add(clsid, categoryIds);
        }

        return categoryIds;
    }

    private static bool IsOpcCategory(Guid categoryId)
    {
        foreach (var opcCategoryId in OpcCategoryIds)
        {
            if (opcCategoryId == categoryId)
            {
                return true;
            }
        }

        return false;
    }

    private static string FirstNonEmpty(string? first, string? second, string? third, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(first))
        {
            return first;
        }

        if (!string.IsNullOrWhiteSpace(second))
        {
            return second;
        }

        return string.IsNullOrWhiteSpace(third) ? fallback : third;
    }

    private static string CombineRegistryPath(string parent, string child) => string.Concat(parent, "\\", child);

    private void LogRemoteRegistryEnumerationFailed(string host, Exception exception)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            RemoteRegistryEnumerationFailed(_logger, host, exception);
        }
    }

    private sealed class WinRegRemoteRegistryReaderFactory : IRemoteRegistryReaderFactory
    {
        public static WinRegRemoteRegistryReaderFactory Instance { get; } = new();

        public IRemoteRegistryReader Open(string host, NetworkCredential credentials)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);
            ArgumentNullException.ThrowIfNull(credentials);

            var registry = CreateRegistryClient(host, credentials);
            try
            {
                return new WinRegRemoteRegistryReader(registry, registry.OpenHKLM());
            }
            catch (InteropException)
            {
                registry.CloseConnection();
                throw;
            }
        }

        private static IRegistry CreateRegistryClient(string host, NetworkCredential credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.UserName))
            {
                return RegistryFactory.Instance.GetRegistryClient(host, smbTransport: true);
            }

            var authInfo = new DefaultAuthInfoImpl(
                credentials.Domain ?? string.Empty,
                credentials.UserName,
                credentials.Password ?? string.Empty);
            return RegistryFactory.Instance.GetRegistryClient(authInfo, host, smbTransport: true);
        }
    }

    private sealed class WinRegRemoteRegistryReader : IRemoteRegistryReader
    {
        private readonly IRegistry _registry;
        private readonly PolicyHandle _hklm;
        private bool _disposed;

        public WinRegRemoteRegistryReader(IRegistry registry, PolicyHandle hklm)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _hklm = hklm ?? throw new ArgumentNullException(nameof(hklm));
        }

        public IReadOnlyList<string> EnumerateSubKeyNames(string keyPath)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            var key = TryOpenKey(keyPath);
            if (key is null)
            {
                return Array.Empty<string>();
            }

            try
            {
                var names = new List<string>();
                for (var index = 0; ; index++)
                {
                    try
                    {
                        var keyData = _registry.EnumKey(key, index);
                        if (keyData.Length > 0 && !string.IsNullOrWhiteSpace(keyData[0]))
                        {
                            names.Add(keyData[0]);
                        }
                    }
                    catch (InteropException ex) when (IsNoMoreItems(ex))
                    {
                        break;
                    }
                }

                return names;
            }
            finally
            {
                _registry.CloseKey(key);
            }
        }

        public bool KeyExists(string keyPath)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            var key = TryOpenKey(keyPath);
            if (key is null)
            {
                return false;
            }

            _registry.CloseKey(key);
            return true;
        }

        public string? ReadStringValue(string keyPath, string? valueName = null)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentException.ThrowIfNullOrWhiteSpace(keyPath);

            var key = TryOpenKey(keyPath);
            if (key is null)
            {
                return null;
            }

            try
            {
                return valueName is null
                    ? DecodeRegistryString(_registry.QueryValue(key, RegistryStringBufferSize))
                    : DecodeNamedRegistryString(_registry.QueryValue(key, valueName, RegistryStringBufferSize));
            }
            catch (InteropException ex) when (IsMissingRegistryItem(ex))
            {
                return null;
            }
            finally
            {
                _registry.CloseKey(key);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                _registry.CloseKey(_hklm);
            }
            catch (InteropException)
            {
            }

            try
            {
                _registry.CloseConnection();
            }
            catch (InteropException)
            {
            }
        }

        private PolicyHandle? TryOpenKey(string keyPath)
        {
            try
            {
                return _registry.OpenKey(_hklm, keyPath, RegKeyAccess.KEY_READ);
            }
            catch (InteropException ex) when (IsMissingRegistryItem(ex))
            {
                return null;
            }
        }

        private static string? DecodeNamedRegistryString(object[] valueData)
        {
            if (valueData.Length < 2)
            {
                return null;
            }

            return valueData[1] is byte[] bytes ? DecodeRegistryString(bytes) : null;
        }

        private static string? DecodeRegistryString(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return null;
            }

            var value = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static bool IsNoMoreItems(InteropException exception) =>
            exception.ErrorCode == ErrorCode.ERROR_NO_MORE_ITEMS;

        private static bool IsMissingRegistryItem(InteropException exception) =>
            exception.ErrorCode == ErrorCode.ERROR_FILE_NOT_FOUND
            || exception.ErrorCode == ErrorCode.ERROR_PATH_NOT_FOUND
            || exception.ErrorCode == ErrorCode.REGDB_E_CLASSNOTREG;
    }
}
