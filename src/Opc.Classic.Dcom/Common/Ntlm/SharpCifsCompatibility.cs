// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SharpCifs
{
    public sealed class UniAddress
    {
        private UniAddress(string hostName) => HostName = hostName;

        public string HostName { get; }

        public static UniAddress GetByName(string hostName) => new(hostName);

        public override string ToString() => HostName;
    }

    public static class Config
    {
        private static readonly Dictionary<string, string> Properties = new(StringComparer.OrdinalIgnoreCase);

        public static string? GetProperty(string key) =>
            Properties.TryGetValue(key, out var value) ? value : Environment.GetEnvironmentVariable(ToEnvironmentName(key));

        public static void SetProperty(string key, string? value)
        {
            if (value is null)
            {
                Properties.Remove(key);
            }
            else
            {
                Properties[key] = value;
            }
        }

        public static bool GetBoolean(string key, bool defaultValue) =>
            bool.TryParse(GetProperty(key), out var value) ? value : defaultValue;

        private static string ToEnvironmentName(string key) => key.Replace('.', '_').Replace('-', '_').ToUpperInvariant();
    }
}

namespace SharpCifs.Util.Sharpen
{
    public abstract class Iterator<T>
    {
        public abstract bool HasNext();

        public abstract T Next();

        public virtual void Remove() => throw new NotSupportedException();
    }

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

    public sealed class ThreadGroup
    {
        private readonly List<Thread> _threads = new();

        public ThreadGroup(string name) => Name = name;

        public string Name { get; }

        internal void Add(Thread thread)
        {
            lock (_threads)
            {
                _threads.Add(thread);
            }
        }

        public void Interrupt()
        {
            lock (_threads)
            {
                foreach (var thread in _threads)
                {
                    thread.Interrupt();
                }
            }
        }
    }

    public class Thread
    {
        private readonly ThreadGroup? _group;
        private readonly string? _name;
        private bool _daemon;
        private System.Threading.Thread? _thread;

        public Thread()
        {
        }

        public Thread(string name) => _name = name;

        public Thread(ThreadGroup group, string name)
        {
            _group = group;
            _name = name;
        }

        protected CancellationTokenSource Canceller { get; } = new();

        protected bool IsCanceled => Canceller.IsCancellationRequested;

        public virtual void Run()
        {
        }

        public string GetName() => _name ?? _thread?.Name ?? string.Empty;

        public void SetDaemon(bool daemon) => _daemon = daemon;

        public void Start()
        {
            _thread = new System.Threading.Thread(Run)
            {
                IsBackground = _daemon
            };
            if (!string.IsNullOrEmpty(_name))
            {
                _thread.Name = _name;
            }
            _group?.Add(this);
            _thread.Start();
        }

        public void Interrupt() => Canceller.Cancel();

        public void Join() => _thread?.Join(TimeSpan.FromSeconds(5));

        public static void Sleep(int millisecondsTimeout) => System.Threading.Thread.Sleep(millisecondsTimeout);
    }

    public sealed class PrintWriter : IDisposable
    {
        private readonly TextWriter _writer;

        public PrintWriter(TextWriter writer) => _writer = writer;

        public void Write(string value) => _writer.Write(value);

        public void Flush() => _writer.Flush();

        public void Close() => _writer.Close();

        public void Dispose() => _writer.Dispose();
    }

    public sealed class UnknownHostException : Exception
    {
        public UnknownHostException()
        {
        }

        public UnknownHostException(string message)
            : base(message)
        {
        }
    }

    public sealed class MissingResourceException : Exception
    {
        public MissingResourceException()
        {
        }
    }

    public sealed class NoSuchElementException : Exception
    {
        public NoSuchElementException()
        {
        }
    }

    public sealed class UnsupportedEncodingException : Exception
    {
        public UnsupportedEncodingException()
        {
        }
    }

    public sealed class InstantiationException : Exception
    {
        public InstantiationException()
        {
        }
    }

    public sealed class StringTokenizer
    {
        private readonly string[] _tokens;
        private int _index;

        public StringTokenizer(string value, string delimiters) =>
            _tokens = value.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        public string NextToken()
        {
            if (_index >= _tokens.Length)
            {
                throw new NoSuchElementException();
            }

            return _tokens[_index++];
        }
    }

    public static class Arrays
    {
        public static bool Equals(byte[]? left, byte[]? right) =>
            left is null ? right is null : right is not null && left.AsSpan().SequenceEqual(right);
    }

    public static class SharpenCompatibilityExtensions
    {
        public static Iterator<T> Iterator<T>(this IEnumerable<T> source) => new EnumerableIterator<T>(source);

        public static T Remove<T>(this List<T> list, int index)
        {
            var value = list[index];
            list.RemoveAt(index);
            return value;
        }

        public static void RemoveAll<T>(this List<T> list, IEnumerable<T> values)
        {
            foreach (var value in values.ToArray())
            {
                list.Remove(value);
            }
        }

        public static IEnumerable<T> SubList<T>(this List<T> list, int start, int end) =>
            list.Skip(start).Take(Math.Max(0, end - start));

        public static bool Contains<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
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
}

namespace SharpCifs.Netbios
{
    public sealed class NbtAddress
    {
        private readonly string _hostName;

        private NbtAddress(string hostName) => _hostName = hostName;

        public static NbtAddress GetLocalHost() => new(Dns.GetHostName());

        public string GetHostName() => _hostName;
    }
}

namespace SharpCifs.Smb
{
    public class SmbException : IOException
    {
        public SmbException()
        {
        }

        public SmbException(string message)
            : base(message)
        {
        }

        public SmbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public virtual int GetNtStatus() => HResult;
    }

    public sealed class SmbAuthException : SmbException
    {
        public SmbAuthException()
        {
        }

        public SmbAuthException(string message)
            : base(message)
        {
        }
    }

    public sealed class NtlmPasswordAuthentication
    {
        public NtlmPasswordAuthentication(string domain, string username, string password)
        {
            Domain = domain;
            Username = username;
            Password = password;
        }

        public string Domain { get; }

        public string Username { get; }

        public string Password { get; }
    }

    public static class SmbSession
    {
        public static void Logon(SharpCifs.UniAddress address, NtlmPasswordAuthentication authentication)
        {
            ArgumentNullException.ThrowIfNull(address);
            ArgumentNullException.ThrowIfNull(authentication);
        }
    }

    public sealed class SmbNamedPipe
    {
        public const int PipeTypeDceTransact = 0x0200;
        public const int PIPE_TYPE_RDWR = 0x0003;
        public const int PIPE_TYPE_DCE_TRANSACT = PipeTypeDceTransact;

        private readonly MemoryStream _input = new();
        private readonly MemoryStream _output = new();

        public SmbNamedPipe(string url, int pipeType)
        {
            Url = url;
            PipeType = pipeType;
        }

        public string Url { get; }

        public int PipeType { get; }

        public Stream GetInputStream() => _input;

        public Stream GetNamedPipeInputStream() => _input;

        public Stream GetNamedPipeOutputStream() => _output;
    }
}

namespace SharpCifs.Dcerpc
{
    public sealed class Uuid
    {
        public Uuid(string value) => Parse(value);

        public int TimeLow { get; set; }

        public short TimeMid { get; set; }

        public short TimeHiAndVersion { get; set; }

        public byte ClockSeqHiAndReserved { get; set; }

        public byte ClockSeqLow { get; set; }

        public byte[] Node { get; set; } = new byte[6];

        public void Parse(string value)
        {
            var parts = value.Split('-');
            if (parts.Length != 5 || parts[3].Length != 4 || parts[4].Length != 12)
            {
                throw new FormatException("Invalid UUID format.");
            }

            TimeLow = unchecked((int)uint.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            TimeMid = unchecked((short)ushort.Parse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            TimeHiAndVersion = unchecked((short)ushort.Parse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            ClockSeqHiAndReserved = byte.Parse(parts[3][..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            ClockSeqLow = byte.Parse(parts[3][2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            Node = new byte[6];
            for (var i = 0; i < Node.Length; i++)
            {
                Node[i] = byte.Parse(parts[4].Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
        }

        public override string ToString() =>
            $"{(uint)TimeLow:x8}-{(ushort)TimeMid:x4}-{(ushort)TimeHiAndVersion:x4}-" +
            $"{ClockSeqHiAndReserved:x2}{ClockSeqLow:x2}-{Convert.ToHexString(Node).ToLowerInvariant()}";
    }
}

