// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace SharpCifs.Util.Sharpen;

public static class SharpenCompatibilityExtensions
{
    public static Iterator<T> Iterator<T>(this IEnumerable<T> source) => new EnumerableIterator<T>(source);

    public static T Remove<T>(this IList<T> list, int index)
    {
        var value = list[index];
        list.RemoveAt(index);
        return value;
    }

    public static void RemoveAll<T>(this ICollection<T> collection, IEnumerable<T> values)
    {
        foreach (var value in values.ToArray())
        {
            collection.Remove(value);
        }
    }

    public static IEnumerable<T> SubList<T>(this IReadOnlyList<T> list, int start, int end) =>
        list.Skip(start).Take(Math.Max(0, end - start));

    public static bool Contains<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull => dictionary.ContainsKey(key);

    public static int GetLocalPort(this System.Net.Sockets.Socket socket) =>
        socket.LocalEndPoint is IPEndPoint endpoint ? endpoint.Port : 0;

    public static int GetPort(this System.Net.Sockets.Socket socket) =>
        socket.RemoteEndPoint is IPEndPoint endpoint ? endpoint.Port : 0;

    public static bool RegionMatches(this string value, bool ignoreCase, int toffset,
        string other, int ooffset, int length)
    {
        if (value is null || other is null || toffset < 0 || ooffset < 0 || length < 0 ||
            toffset + length > value.Length || ooffset + length > other.Length)
        {
            return false;
        }

        return string.Compare(value, toffset, other, ooffset, length,
            ignoreCase, CultureInfo.InvariantCulture) == 0;
    }

    private sealed class EnumerableIterator<T> : Iterator<T>
    {
        private readonly ICollection<T>? _collection;
        private readonly IEnumerator<T> _enumerator;
        private T? _current;
        private bool _canRemove;

        public EnumerableIterator(IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            _collection = source as ICollection<T>;
            _enumerator = source.GetEnumerator();
        }

        public override bool HasNext()
        {
            if (_canRemove)
            {
                return true;
            }

            if (!_enumerator.MoveNext())
            {
                return false;
            }

            _current = _enumerator.Current;
            _canRemove = true;
            return true;
        }

        public override T Next()
        {
            if (!HasNext())
            {
                throw new NoSuchElementException();
            }

            _canRemove = false;
            return _current!;
        }

        public override void Remove()
        {
            if (_collection is null || _current is null)
            {
                return;
            }

            try
            {
                _collection.Remove(_current);
            }
            catch (NotSupportedException)
            {
            }
        }
    }
}
