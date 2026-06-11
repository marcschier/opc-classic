// SPDX-License-Identifier: MIT

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Opc.Classic.Dcom.Common.Ntlm;

public sealed class Hashtable : IEnumerable<KeyValuePair<object, object>>
{
    private readonly Dictionary<object, object> _inner = new();

    public object this[object key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public int Count => _inner.Count;

    public ICollection<object> Keys => _inner.Keys.ToList();

    public ICollection<object> Values => _inner.Values.ToList();

    public void Clear() => _inner.Clear();

    public bool ContainsKey(object key) => _inner.ContainsKey(key);

    public object? Get(object key) => _inner.TryGetValue(key, out var value) ? value : null;

    public bool TryGetValue(object key, out object? value) => _inner.TryGetValue(key, out value);

    public object? Put(object key, object value)
    {
        _inner.TryGetValue(key, out var previous);
        _inner[key] = value;
        return previous;
    }

    public void Remove(object key) => _inner.Remove(key);

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator() => _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
