// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers;
using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

internal static class SensitiveBufferPool
{
    private static Action<string, byte[], int>? s_returnObserver;

    public static byte[] Rent(int minimumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);
        return minimumLength == 0 ? Array.Empty<byte>() : ArrayPool<byte>.Shared.Rent(minimumLength);
    }

    public static void Return(string purpose, byte[]? buffer, int usedLength)
    {
        if (buffer is null || buffer.Length == 0)
        {
            return;
        }

        int length = Math.Clamp(usedLength, 0, buffer.Length);
        CryptographicOperations.ZeroMemory(buffer.AsSpan(0, length));
        Volatile.Read(ref s_returnObserver)?.Invoke(purpose, buffer, length);
        ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
    }

    internal static IDisposable SetReturnObserverForTests(Action<string, byte[], int>? observer)
    {
        var previous = Interlocked.Exchange(ref s_returnObserver, observer);
        return new ObserverScope(previous);
    }

    private sealed class ObserverScope : IDisposable
    {
        private Action<string, byte[], int>? _previous;

        public ObserverScope(Action<string, byte[], int>? previous) => _previous = previous;

        public void Dispose()
        {
            var previous = Interlocked.Exchange(ref _previous, null);
            Interlocked.Exchange(ref s_returnObserver, previous);
        }
    }
}
