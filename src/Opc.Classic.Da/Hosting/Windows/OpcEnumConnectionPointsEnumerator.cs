//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.Versioning;

namespace Opc.Classic.Da.Hosting.Windows;

[SupportedOSPlatform("windows")]
internal sealed unsafe class OpcEnumConnectionPointsEnumerator : IDisposable
{
    private readonly Lock _syncRoot = new();
    private readonly IntPtr[] _connectionPoints;
    private int _position;
    private bool _disposed;

    public OpcEnumConnectionPointsEnumerator(IntPtr[] connectionPoints)
        : this(connectionPoints, 0)
    {
    }

    private OpcEnumConnectionPointsEnumerator(IntPtr[] connectionPoints, int position)
    {
        ArgumentNullException.ThrowIfNull(connectionPoints);
        _connectionPoints = connectionPoints;
        _position = Math.Clamp(position, 0, connectionPoints.Length);
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
            ReleaseSnapshot(_connectionPoints);
        }
    }

    internal int Next(uint cConnections, IntPtr* ppCP)
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            int requested = cConnections > int.MaxValue ? int.MaxValue : (int)cConnections;
            int fetched = Math.Min(requested, _connectionPoints.Length - _position);
            for (int i = 0; i < fetched; i++)
            {
                IntPtr connectionPoint = _connectionPoints[_position++];
                AddRef(connectionPoint);
                ppCP[i] = connectionPoint;
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
            int skipped = (int)Math.Min(requested, _connectionPoints.Length - _position);
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

    internal OpcEnumConnectionPointsEnumerator Clone()
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            return new OpcEnumConnectionPointsEnumerator(AddRefSnapshot(_connectionPoints), _position);
        }
    }

    private static IntPtr[] AddRefSnapshot(IntPtr[] snapshot)
    {
        var clone = new IntPtr[snapshot.Length];
        int copied = 0;
        try
        {
            for (int i = 0; i < snapshot.Length; i++)
            {
                AddRef(snapshot[i]);
                clone[i] = snapshot[i];
                copied = i + 1;
            }
            return clone;
        }
        catch
        {
            for (int i = 0; i < copied; i++)
            {
                Release(clone[i]);
            }
            throw;
        }
    }

    private static void ReleaseSnapshot(IntPtr[] snapshot)
    {
        for (int i = 0; i < snapshot.Length; i++)
        {
            IntPtr pointer = snapshot[i];
            if (pointer != IntPtr.Zero)
            {
                snapshot[i] = IntPtr.Zero;
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
