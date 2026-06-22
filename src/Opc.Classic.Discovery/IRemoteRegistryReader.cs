// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Discovery;

/// <summary>
/// Reads registry data relative to <c>HKEY_LOCAL_MACHINE</c> on a remote machine.
/// </summary>
public interface IRemoteRegistryReader : IDisposable
{
    /// <summary>
    /// Enumerates the direct subkey names under <paramref name="keyPath" />.
    /// </summary>
    IReadOnlyList<string> EnumerateSubKeyNames(string keyPath);

    /// <summary>
    /// Returns whether <paramref name="keyPath" /> exists.
    /// </summary>
    bool KeyExists(string keyPath);

    /// <summary>
    /// Reads a string value from <paramref name="keyPath" />.
    /// </summary>
    string? ReadStringValue(string keyPath, string? valueName = null);
}
