//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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
/// reference and <see cref="Dispose"/> releases it. The proxy invokes the
/// callback vtable synchronously and owns cleanup of all temporary native IN
/// buffers after the callback returns.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed unsafe class OpcDataCallbackProxy : IOpcDataCallbackSink
{
    private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid s_iidDataCallback = IOPCDataCallback.InterfaceId;
    private readonly Lock _syncRoot = new();
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
            throw new COMException("Client IUnknown pointer is null.", global::Opc.Classic.OpcResultId.Pointer.Code);
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

    internal IntPtr AddRefCallbackUnknown()
    {
        lock (_syncRoot)
        {
            IntPtr callbackPtr = _callbackPtr;
            ObjectDisposedException.ThrowIf(callbackPtr == IntPtr.Zero, this);
            return QueryInterface(callbackPtr, s_iidUnknown, "Client sink does not expose IUnknown.");
        }
    }

    /// <summary>
    /// Calls <c>IOPCDataCallback::OnDataChange</c> (opnum 3).
    /// </summary>
    /// <param name="payload">Data-change payload to deliver.</param>
    public void OnDataChange(OpcDaGroup.DataChangePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var onDataChange =
            (delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)vtable[3];
        InvokeDataValueCallback(
            callbackPtr,
            onDataChange,
            payload,
            "IOPCDataCallback::OnDataChange");
    }

    /// <summary>
    /// Calls <c>IOPCDataCallback::OnReadComplete</c> (opnum 4).
    /// </summary>
    /// <param name="payload">Read-complete payload to deliver.</param>
    public void OnReadComplete(OpcDaGroup.DataChangePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var onReadComplete =
            (delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)vtable[4];
        InvokeDataValueCallback(
            callbackPtr,
            onReadComplete,
            payload,
            "IOPCDataCallback::OnReadComplete");
    }

    /// <summary>
    /// Calls <c>IOPCDataCallback::OnWriteComplete</c> (opnum 5).
    /// </summary>
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
        InvokeWriteCompleteCallback(
            callbackPtr,
            onWriteComplete,
            transactionId,
            groupHandle,
            masterError,
            clientHandles,
            errors);
    }

    /// <summary>
    /// Calls <c>IOPCDataCallback::OnCancelComplete</c> (opnum 6).
    /// </summary>
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

    /// <summary>
    /// Releases the held <c>IOPCDataCallback</c> interface pointer.
    /// </summary>
    public void Dispose()
    {
        IntPtr callbackPtr;
        lock (_syncRoot)
        {
            callbackPtr = _callbackPtr;
            _callbackPtr = IntPtr.Zero;
        }
        if (callbackPtr != IntPtr.Zero)
        {
            InvokeRelease(callbackPtr);
        }
        GC.SuppressFinalize(this);
    }

    private static IntPtr QueryDataCallback(IntPtr clientUnknown) =>
        QueryInterface(clientUnknown, s_iidDataCallback, "Client sink does not implement IOPCDataCallback.");

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
            throw new COMException(failureMessage, global::Opc.Classic.OpcResultId.NoInterface.Code);
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

    private IntPtr GetCallbackPtr()
    {
        IntPtr callbackPtr = _callbackPtr;
        ObjectDisposedException.ThrowIf(callbackPtr == IntPtr.Zero, this);
        return callbackPtr;
    }

    private static void InvokeDataValueCallback(
        IntPtr callbackPtr,
        delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int> callback,
        OpcDaGroup.DataChangePayload payload,
        string method)
    {
        int count = ValidateDataValuePayload(payload);
        IntPtr clientItems = IntPtr.Zero;
        IntPtr values = IntPtr.Zero;
        IntPtr qualities = IntPtr.Zero;
        IntPtr timestamps = IntPtr.Zero;
        IntPtr errors = IntPtr.Zero;
        try
        {
            clientItems = AllocateUInt32Array(payload.ClientHandles);
            values = AllocateVariantArray(payload.Values);
            qualities = AllocateUInt16Array(payload.Qualities);
            timestamps = AllocateInt64Array(payload.Timestamps);
            errors = AllocateInt32Array(payload.Errors);
            int hr = callback(
                callbackPtr,
                unchecked((uint)payload.TransactionId),
                unchecked((uint)payload.GroupHandle),
                payload.MasterQuality,
                payload.MasterError,
                (uint)count,
                clientItems,
                values,
                qualities,
                timestamps,
                errors);
            ThrowIfFailed(hr, method);
        }
        finally
        {
            FreeVariantArray(values, count);
            FreeCoTaskMem(clientItems, qualities, timestamps, errors);
        }
    }

    private static void InvokeWriteCompleteCallback(
        IntPtr callbackPtr,
        delegate* unmanaged<IntPtr, uint, uint, int, uint, IntPtr, IntPtr, int> callback,
        int transactionId,
        int groupHandle,
        int masterError,
        int[] clientHandles,
        int[] errors)
    {
        int count = ValidateWriteCompletePayload(clientHandles, errors);
        IntPtr handlesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            handlesPtr = AllocateUInt32Array(clientHandles);
            errorsPtr = AllocateInt32Array(errors);
            int hr = callback(
                callbackPtr,
                unchecked((uint)transactionId),
                unchecked((uint)groupHandle),
                masterError,
                (uint)count,
                handlesPtr,
                errorsPtr);
            ThrowIfFailed(hr, "IOPCDataCallback::OnWriteComplete");
        }
        finally
        {
            FreeCoTaskMem(handlesPtr, errorsPtr);
        }
    }

    private static int ValidateDataValuePayload(OpcDaGroup.DataChangePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ClientHandles);
        ArgumentNullException.ThrowIfNull(payload.Values);
        ArgumentNullException.ThrowIfNull(payload.Qualities);
        ArgumentNullException.ThrowIfNull(payload.Timestamps);
        ArgumentNullException.ThrowIfNull(payload.Errors);
        int count = payload.ClientHandles.Length;
        if (payload.Values.Length != count || payload.Qualities.Length != count ||
            payload.Timestamps.Length != count || payload.Errors.Length != count)
        {
            throw new ArgumentException("Data callback payload array lengths must match.", nameof(payload));
        }
        return count;
    }

    private static int ValidateWriteCompletePayload(int[] clientHandles, int[] errors)
    {
        ArgumentNullException.ThrowIfNull(clientHandles);
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Length != clientHandles.Length)
        {
            throw new ArgumentException("Write callback array lengths must match.", nameof(errors));
        }
        return clientHandles.Length;
    }

    private static IntPtr AllocateUInt32Array(int[] values) => AllocateInt32Array(values);

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInt32Array(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(int)));
        Marshal.Copy(values, 0, ptr, values.Length);
        return ptr;
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateUInt16Array(ushort[] values)
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

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInt64Array(long[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(long)));
        Marshal.Copy(values, 0, ptr, values.Length);
        return ptr;
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateVariantArray(OpcVariant[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        int variantSize = ComVariantMarshaler.VariantSize;
        int byteCount = checked(values.Length * variantSize);
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        NativeMemory.Clear((void*)ptr, (nuint)byteCount);
        bool completed = false;
        try
        {
            for (int i = 0; i < values.Length; i++)
            {
                ComVariantMarshaler.WriteVariant(ptr + (i * variantSize), values[i]);
            }
            completed = true;
            return ptr;
        }
        finally
        {
            if (!completed)
            {
                FreeVariantArray(ptr, values.Length);
            }
        }
    }

    private static void FreeVariantArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }
        int variantSize = ComVariantMarshaler.VariantSize;
        for (int i = 0; i < count; i++)
        {
            ComVariantMarshaler.ClearVariant(ptr + (i * variantSize));
        }
        Marshal.FreeCoTaskMem(ptr);
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
