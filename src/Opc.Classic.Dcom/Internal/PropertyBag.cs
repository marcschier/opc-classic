// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Text;

namespace Opc.Classic.Dcom.Internal;

/// <summary>
/// Managed replacement for Opc.Classic.Dcom.Common.Ntlm.Properties.
/// Drop-in API-compatible at the Opc.Classic.Dcom call sites that use
/// GetProperty, SetProperty, Load, Store, and the copy constructor for defaults.
/// </summary>
public sealed class PropertyBag
{
    private readonly ConcurrentDictionary<string, object?> _items = new(StringComparer.Ordinal);

    public PropertyBag()
    {
    }

    public PropertyBag(PropertyBag? defaults)
    {
        if (defaults is null)
        {
            return;
        }

        foreach (var kvp in defaults._items)
        {
            _items[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Returns the value for <paramref name="key"/>, or <see langword="null"/> if not present.
    /// </summary>
    public object? GetProperty(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _items.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Returns the value for <paramref name="key"/>, or <paramref name="defaultValue"/> if not present.
    /// </summary>
    public string GetProperty(string key, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(defaultValue);
        return GetProperty(key) as string ?? defaultValue;
    }

    /// <summary>
    /// Returns the value for <paramref name="key"/>, or <paramref name="defaultValue"/> if not present.
    /// </summary>
    public object? GetProperty(string key, object? defaultValue)
    {
        return GetProperty(key) ?? defaultValue;
    }

    /// <summary>
    /// Sets <paramref name="key"/> to <paramref name="value"/>.
    /// </summary>
    public void SetProperty(string key, object? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _items[key] = value;
    }

    /// <summary>
    /// Loads simple key/value pairs from a Java-properties-style stream.
    /// </summary>
    public void Load(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var reader = new StreamReader(input, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed[0] == '#' || trimmed[0] == '!')
            {
                continue;
            }

            var separator = trimmed.IndexOf('=');
            if (separator < 0)
            {
                separator = trimmed.IndexOf(':');
            }

            if (separator < 0)
            {
                SetProperty(trimmed, string.Empty);
                continue;
            }

            SetProperty(trimmed[..separator].Trim(), trimmed[(separator + 1)..].Trim());
        }
    }

    /// <summary>
    /// Stores simple key/value pairs to a Java-properties-style stream.
    /// </summary>
    public void Store(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);

        using var writer = new StreamWriter(output, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
        foreach (var kvp in _items)
        {
            writer.Write(kvp.Key);
            writer.Write('=');
            writer.WriteLine(kvp.Value?.ToString() ?? string.Empty);
        }
    }
}
