//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace System.Collections.Generic;

/// <summary>
/// Extensions
/// </summary>
public static class CollectionsEx
{
    /// <summary>
    /// Mimics java
    /// </summary>
    /// <param name="dictionary">Dictionary that stores the decoded values by key.</param>
    /// <param name="key">Lookup key used to select the value from the collection.</param>
    /// <returns>The removed value, or <c>default</c> when the key was not present.</returns>
    public static V GetAndRemove<K, V>(this IDictionary<K, V> dictionary, K key)
    {
        if (dictionary.ContainsKey(key))
        {
            var value = dictionary[key];
            dictionary.Remove(key);
            return value;
        }
        return default;
    }

    /// <summary>
    /// Get or return default
    /// </summary>
    /// <param name="dictionary">Dictionary that stores the decoded values by key.</param>
    /// <param name="key">Lookup key used to select the value from the collection.</param>
    /// <param name="deflt">Default value returned when the key is not present.</param>
    /// <returns>The value associated with <paramref name="key"/>, or <paramref name="deflt"/> when the key is not present.</returns>
    public static V GetOrDefault<K, V>(this IDictionary<K, V> dictionary, K key,
        V deflt = default)
    {
        if (dictionary.ContainsKey(key))
        {
            var value = dictionary[key];
            return value;
        }
        return deflt;
    }

    /// <summary>
    /// Add or update
    /// </summary>
    /// <param name="dictionary">Dictionary that stores the decoded values by key.</param>
    /// <param name="key">Lookup key used to select the value from the collection.</param>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <returns>No value is returned; the dictionary is updated in place.</returns>
    public static void AddOrUpdate<K, V>(this IDictionary<K, V> dictionary, K key, V value)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }

    /// <summary>
    /// Mimics java
    /// </summary>
    /// <param name="list">List from which the indexed item should be removed.</param>
    /// <param name="index">Zero-based index at which the read or write operation begins.</param>
    /// <returns>The requested and remove at value.</returns>
    public static object GetAndRemoveAt(this IList<object> list, int index)
    {
        if (index < list.Count)
        {
            var value = list[index];
            list.Remove(index);
            return value;
        }
        return null;
    }
}
