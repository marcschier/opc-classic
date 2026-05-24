//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace System.Collections.Generic;

/// <summary>
/// Extensions
/// </summary>
public static class CollectionsEx {

    /// <summary>
    /// Mimics java
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static V GetAndRemove<K, V>(this IDictionary<K, V> dictionary, K key) {
        if (dictionary.ContainsKey(key)) {
            var value = dictionary[key];
            dictionary.Remove(key);
            return value;
        }
        return default;
    }

    /// <summary>
    /// Get or return default
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="deflt"></param>
    /// <returns></returns>
    public static V GetOrDefault<K, V>(this IDictionary<K, V> dictionary, K key,
        V deflt = default) {
        if (dictionary.ContainsKey(key)) {
            var value = dictionary[key];
            return value;
        }
        return deflt;
    }

    /// <summary>
    /// Add or update
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static void AddOrUpdate<K, V>(this IDictionary<K, V> dictionary, K key, V value) {
        if (dictionary.ContainsKey(key)) {
            dictionary[key] = value;
        }
        else {
            dictionary.Add(key, value);
        }
    }

    /// <summary>
    /// Mimics java
    /// </summary>
    /// <param name="list"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static object GetAndRemoveAt(this IList<object> list, int index) {
        if (index < list.Count) {
            var value = list[index];
            list.Remove(index);
            return value;
        }
        return null;
    }
}
