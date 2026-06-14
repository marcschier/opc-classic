//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.Versioning;

namespace Opc.Classic.Hda.Hosting.Windows;

[SupportedOSPlatform("windows")]
internal sealed unsafe class OpcHdaEnumConnectionsEnumerator : IDisposable
{
    private readonly Lock _syncRoot = new();
    private readonly OpcHdaConnectData[] _snapshot;
    private int _position;
    private bool _disposed;

    public OpcHdaEnumConnectionsEnumerator(OpcHdaConnectData[] snapshot)
        : this(snapshot, 0)
    {
    }

    private OpcHdaEnumConnectionsEnumerator(OpcHdaConnectData[] snapshot, int position)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _snapshot = snapshot;
        _position = Math.Clamp(position, 0, snapshot.Length);
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            ReleaseSnapshot(_snapshot);
        }
    }

    internal int Next(uint cConnections, OpcHdaConnectData* rgcd)
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            int requested = cConnections > int.MaxValue ? int.MaxValue : (int)cConnections;
            int fetched = Math.Min(requested, _snapshot.Length - _position);
            for (int i = 0; i < fetched; i++)
            {
                OpcHdaConnectData connection = _snapshot[_position++];
                AddRef(connection.pUnk);
                rgcd[i] = connection;
            }
            return fetched;
        }
    }

    internal int Skip(uint cConnections)
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            long requested = cConnections;
            int skipped = (int)Math.Min(requested, _snapshot.Length - _position);
            _position += skipped;
            return skipped;
        }
    }

    internal void Reset()
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            _position = 0;
        }
    }

    internal OpcHdaEnumConnectionsEnumerator Clone()
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            return new OpcHdaEnumConnectionsEnumerator(AddRefSnapshot(_snapshot), _position);
        }
    }

    private static OpcHdaConnectData[] AddRefSnapshot(OpcHdaConnectData[] snapshot)
    {
        var clone = new OpcHdaConnectData[snapshot.Length];
        int copied = 0;
        try
        {
            for (int i = 0; i < snapshot.Length; i++)
            {
                AddRef(snapshot[i].pUnk);
                clone[i] = snapshot[i];
                copied = i + 1;
            }
            return clone;
        }
        catch
        {
            for (int i = 0; i < copied; i++)
            {
                Release(clone[i].pUnk);
            }
            throw;
        }
    }

    private static void ReleaseSnapshot(OpcHdaConnectData[] snapshot)
    {
        for (int i = 0; i < snapshot.Length; i++)
        {
            IntPtr pointer = snapshot[i].pUnk;
            if (pointer != IntPtr.Zero)
            {
                snapshot[i].pUnk = IntPtr.Zero;
                Release(pointer);
            }
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private static void AddRef(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
        {
            return;
        }
        IntPtr* vtable = *(IntPtr**)pointer;
        var addRef = (delegate* unmanaged<IntPtr, uint>)vtable[1];
        _ = addRef(pointer);
    }

    private static void Release(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
        {
            return;
        }
        IntPtr* vtable = *(IntPtr**)pointer;
        var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
        _ = release(pointer);
    }
}
