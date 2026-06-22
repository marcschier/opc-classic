// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hosting.Windows;

/// <summary>
/// Outbound Windows COM proxy for a client-supplied <c>IOPCShutdown</c> sink.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed unsafe class OpcShutdownSinkProxy : IDisposable
{
    private static readonly int E_NOINTERFACE = global::Opc.Classic.OpcResultId.NoInterface.Code;
    private static readonly int E_POINTER = global::Opc.Classic.OpcResultId.Pointer.Code;

    private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private readonly Lock _syncRoot = new();
    private IntPtr _sinkPtr;

    /// <summary>
    /// Initializes a new proxy from a client-supplied <c>IUnknown</c> pointer.
    /// </summary>
    public OpcShutdownSinkProxy(IntPtr clientUnknown)
    {
        if (clientUnknown == IntPtr.Zero)
        {
            throw new COMException("Client IUnknown pointer is null.", E_POINTER);
        }

        InvokeAddRef(clientUnknown);
        try
        {
            _sinkPtr = QueryInterface(clientUnknown, OpcGuids.IID_IOPCShutdown, "Client sink does not implement IOPCShutdown.");
        }
        finally
        {
            InvokeRelease(clientUnknown);
        }
    }

    internal IntPtr AddRefCallbackUnknown()
    {
        lock (_syncRoot)
        {
            IntPtr sinkPtr = _sinkPtr;
            ObjectDisposedException.ThrowIf(sinkPtr == IntPtr.Zero, this);
            return QueryInterface(sinkPtr, s_iidUnknown, "Client sink does not expose IUnknown.");
        }
    }

    /// <summary>
    /// Calls <c>IOPCShutdown::ShutdownRequest</c> (opnum 3).
    /// </summary>
    public void ShutdownRequest(string reason)
    {
        IntPtr sinkPtr = GetSinkPtr();
        IntPtr* vtable = *(IntPtr**)sinkPtr;
        var shutdownRequest = (delegate* unmanaged<IntPtr, IntPtr, int>)vtable[3];
        IntPtr reasonPtr = IntPtr.Zero;
        try
        {
            reasonPtr = Marshal.StringToCoTaskMemUni(reason);
            int hr = shutdownRequest(sinkPtr, reasonPtr);
            ThrowIfFailed(hr, "IOPCShutdown::ShutdownRequest");
        }
        finally
        {
            Marshal.FreeCoTaskMem(reasonPtr);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        IntPtr sinkPtr;
        lock (_syncRoot)
        {
            sinkPtr = _sinkPtr;
            _sinkPtr = IntPtr.Zero;
        }

        if (sinkPtr != IntPtr.Zero)
        {
            InvokeRelease(sinkPtr);
        }

        GC.SuppressFinalize(this);
    }

    private static IntPtr QueryInterface(IntPtr instance, Guid iid, string failureMessage)
    {
        IntPtr* vtable = *(IntPtr**)instance;
        var queryInterface = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
        Guid local = iid;
        IntPtr returned = IntPtr.Zero;
        int hr = queryInterface(instance, &local, &returned);
        if (hr < 0)
        {
            throw new COMException(failureMessage, hr);
        }
        if (returned == IntPtr.Zero)
        {
            throw new COMException(failureMessage, E_NOINTERFACE);
        }
        return returned;
    }

    private static void InvokeAddRef(IntPtr unknown)
    {
        IntPtr* vtable = *(IntPtr**)unknown;
        var addRef = (delegate* unmanaged<IntPtr, uint>)vtable[1];
        _ = addRef(unknown);
    }

    private static void InvokeRelease(IntPtr unknown)
    {
        IntPtr* vtable = *(IntPtr**)unknown;
        var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
        _ = release(unknown);
    }

    private IntPtr GetSinkPtr()
    {
        IntPtr sinkPtr = _sinkPtr;
        ObjectDisposedException.ThrowIf(sinkPtr == IntPtr.Zero, this);
        return sinkPtr;
    }

    [SuppressMessage("Design", "CA1031", Justification = "COM HRESULT mapping helper.")]
    private static void ThrowIfFailed(int hr, string operation)
    {
        if (hr < 0)
        {
            throw new COMException(operation, hr);
        }
    }
}
