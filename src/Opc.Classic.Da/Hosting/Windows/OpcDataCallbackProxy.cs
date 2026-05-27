//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Outbound Windows COM proxy for a client-supplied
/// <c>IOPCDataCallback</c> sink pointer.
/// </summary>
/// <remarks>
/// This proxy is used by the Windows SCM-activated hosting path, where clients
/// hand the server an <c>IUnknown</c> through <c>IConnectionPoint::Advise</c>.
/// The constructor takes ownership of one queried <c>IOPCDataCallback</c>
/// reference and <see cref="Dispose"/> releases it. The MVP wires the
/// simple <c>OnCancelComplete</c> callback fully; value-bearing callbacks keep
/// the vtable signatures and native-buffer cleanup skeletons but defer full
/// <c>VARIANT</c> array marshaling to cap-a8-followup.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed unsafe class OpcDataCallbackProxy : IDisposable
{
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_POINTER = unchecked((int)0x80004003);

    private static readonly Guid s_iidDataCallback = IOPCDataCallback.InterfaceId;

    private IntPtr _callbackPtr;

    /// <summary>
    /// Initializes a new proxy from a client-supplied <c>IUnknown</c> pointer.
    /// </summary>
    /// <param name="clientUnknown">Client sink <c>IUnknown</c> pointer.</param>
    /// <exception cref="COMException">
    /// Thrown when the pointer is null or does not support <c>IOPCDataCallback</c>.
    /// </exception>
    public OpcDataCallbackProxy(IntPtr clientUnknown)
    {
        if (clientUnknown == IntPtr.Zero)
        {
            throw new COMException("Client IUnknown pointer is null.", E_POINTER);
        }

        InvokeAddRef(clientUnknown);
        try
        {
            _callbackPtr = QueryDataCallback(clientUnknown);
        }
        finally
        {
            InvokeRelease(clientUnknown);
        }
    }

    /// <summary>Calls <c>IOPCDataCallback::OnDataChange</c> (opnum 3).</summary>
    /// <param name="payload">Data-change payload to deliver.</param>
    public void OnDataChange(OpcDaGroup.DataChangePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var onDataChange =
            (delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)vtable[3];
        _ = onDataChange;
        MarshalDataValuePayload(payload);
    }

    /// <summary>Calls <c>IOPCDataCallback::OnReadComplete</c> (opnum 4).</summary>
    /// <param name="payload">Read-complete payload to deliver.</param>
    public void OnReadComplete(OpcDaGroup.DataChangePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var onReadComplete =
            (delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)vtable[4];
        _ = onReadComplete;
        MarshalDataValuePayload(payload);
    }

    /// <summary>Calls <c>IOPCDataCallback::OnWriteComplete</c> (opnum 5).</summary>
    /// <param name="transactionId">Transaction identifier echoed to the client.</param>
    /// <param name="groupHandle">Client group handle.</param>
    /// <param name="masterError">Master HRESULT for the write operation.</param>
    /// <param name="clientHandles">Client item handles.</param>
    /// <param name="errors">Per-item HRESULT values.</param>
    public void OnWriteComplete(
        int transactionId,
        int groupHandle,
        int masterError,
        int[] clientHandles,
        int[] errors)
    {
        ArgumentNullException.ThrowIfNull(clientHandles);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var onWriteComplete =
            (delegate* unmanaged<IntPtr, uint, uint, int, uint, IntPtr, IntPtr, int>)vtable[5];
        _ = onWriteComplete;
        MarshalWriteCompletePayload(transactionId, groupHandle, masterError, clientHandles, errors);
    }

    /// <summary>Calls <c>IOPCDataCallback::OnCancelComplete</c> (opnum 6).</summary>
    /// <param name="payload">Cancel-complete payload to deliver.</param>
    public void OnCancelComplete(OpcDaGroup.CancelCompletePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var onCancelComplete = (delegate* unmanaged<IntPtr, uint, uint, int>)vtable[6];
        int hr = onCancelComplete(
            callbackPtr,
            unchecked((uint)payload.TransactionId),
            unchecked((uint)payload.GroupHandle));
        ThrowIfFailed(hr, "IOPCDataCallback::OnCancelComplete");
    }

    /// <summary>Releases the held <c>IOPCDataCallback</c> interface pointer.</summary>
    public void Dispose()
    {
        IntPtr callbackPtr = Interlocked.Exchange(ref _callbackPtr, IntPtr.Zero);
        if (callbackPtr != IntPtr.Zero)
        {
            InvokeRelease(callbackPtr);
        }
        GC.SuppressFinalize(this);
    }

    private static IntPtr QueryDataCallback(IntPtr clientUnknown)
    {
        IntPtr* vtable = *(IntPtr**)clientUnknown;
        var queryInterface = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
        Guid iid = s_iidDataCallback;
        IntPtr callbackPtr = IntPtr.Zero;
        int hr = queryInterface(clientUnknown, &iid, &callbackPtr);
        if (hr < 0)
        {
            throw new COMException("Client sink does not implement IOPCDataCallback.", hr);
        }
        if (callbackPtr == IntPtr.Zero)
        {
            throw new COMException("Client sink returned a null IOPCDataCallback pointer.", E_NOINTERFACE);
        }
        return callbackPtr;
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

    private IntPtr GetCallbackPtr()
    {
        IntPtr callbackPtr = _callbackPtr;
        ObjectDisposedException.ThrowIf(callbackPtr == IntPtr.Zero, this);
        return callbackPtr;
    }

    private static void MarshalDataValuePayload(OpcDaGroup.DataChangePayload payload)
    {
        IntPtr clientItems = IntPtr.Zero;
        IntPtr values = IntPtr.Zero;
        IntPtr qualities = IntPtr.Zero;
        IntPtr timestamps = IntPtr.Zero;
        IntPtr errors = IntPtr.Zero;
        try
        {
            clientItems = AllocInt32Array(payload.ClientHandles);
            qualities = AllocUInt16Array(payload.Qualities);
            timestamps = AllocInt64Array(payload.Timestamps);
            errors = AllocInt32Array(payload.Errors);
            // TODO(cap-a8-followup): allocate VARIANT* pvValues from payload.Values,
            // then invoke opnum 3/4. Native layout is OPCHANDLE*, VARIANT*, WORD*,
            // FILETIME* (64-bit little-endian file times), and HRESULT* arrays.
            _ = values;
        }
        finally
        {
            FreeCoTaskMem(clientItems, values, qualities, timestamps, errors);
        }
    }

    private static void MarshalWriteCompletePayload(
        int transactionId,
        int groupHandle,
        int masterError,
        int[] clientHandles,
        int[] errors)
    {
        IntPtr handlesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            handlesPtr = AllocInt32Array(clientHandles);
            errorsPtr = AllocInt32Array(errors);
            // TODO(cap-a8-followup): invoke opnum 5 with dwTransid, hGroup,
            // hrMastererr, dwCount, OPCHANDLE* phClientItems, HRESULT* pErrors.
            _ = (transactionId, groupHandle, masterError, handlesPtr, errorsPtr);
        }
        finally
        {
            FreeCoTaskMem(handlesPtr, errorsPtr);
        }
    }

    private static IntPtr AllocInt32Array(int[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(int)));
        Marshal.Copy(values, 0, ptr, values.Length);
        return ptr;
    }

    private static IntPtr AllocUInt16Array(ushort[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(ushort)));
        ushort* target = (ushort*)ptr;
        for (int i = 0; i < values.Length; i++)
        {
            target[i] = values[i];
        }
        return ptr;
    }

    private static IntPtr AllocInt64Array(long[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(long)));
        long* target = (long*)ptr;
        for (int i = 0; i < values.Length; i++)
        {
            target[i] = values[i];
        }
        return ptr;
    }

    private static void FreeCoTaskMem(params IntPtr[] pointers)
    {
        foreach (IntPtr pointer in pointers)
        {
            if (pointer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pointer);
            }
        }
    }

    private static void ThrowIfFailed(int hr, string method)
    {
        if (hr < 0)
        {
            throw new COMException($"{method} failed.", hr);
        }
    }
}
