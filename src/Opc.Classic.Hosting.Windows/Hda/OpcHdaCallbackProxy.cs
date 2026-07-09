// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Outbound Windows COM proxy for a client-supplied <c>IOPCHDA_DataCallback</c> sink.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed unsafe class OpcHdaCallbackProxy : IDisposable
{
    private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid s_iidDataCallback = IOPCHDA_DataCallback.InterfaceId;
    private readonly Lock _syncRoot = new();
    private IntPtr _callbackPtr;

    public OpcHdaCallbackProxy(IntPtr clientUnknown)
    {
        if (clientUnknown == IntPtr.Zero)
        {
            throw new COMException("Client IUnknown pointer is null.", global::Opc.Classic.OpcResultId.Pointer.Code);
        }

        InvokeAddRef(clientUnknown);
        try
        {
            _callbackPtr = QueryInterface(clientUnknown, s_iidDataCallback, "Client sink does not implement IOPCHDA_DataCallback.");
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

    public void OnDataChange(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors) =>
        InvokeItemCallback(3, transactionId, status, itemValues, errors, "IOPCHDA_DataCallback::OnDataChange");

    public void OnReadComplete(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors) =>
        InvokeItemCallback(4, transactionId, status, itemValues, errors, "IOPCHDA_DataCallback::OnReadComplete");

    public void OnReadModifiedComplete(int transactionId, int status, OpcHdaModifiedItem[] itemValues, int[] errors)
    {
        ArgumentNullException.ThrowIfNull(itemValues);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)vtable[5];
        IntPtr valuesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            valuesPtr = OpcHdaItemMarshaler.AllocateModifiedItemArray(itemValues);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int hr = callback(callbackPtr, unchecked((uint)transactionId), status, unchecked((uint)itemValues.Length), valuesPtr, errorsPtr);
            ThrowIfFailed(hr, "IOPCHDA_DataCallback::OnReadModifiedComplete");
        }
        finally
        {
            OpcHdaItemMarshaler.FreeModifiedItemArray(valuesPtr, itemValues.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    public void OnReadAttributeComplete(int transactionId, int status, int clientHandle, OpcHdaAttribute[] attributeValues, int[] errors)
    {
        ArgumentNullException.ThrowIfNull(attributeValues);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int, uint, uint, IntPtr, IntPtr, int>)vtable[6];
        IntPtr valuesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            valuesPtr = OpcHdaItemMarshaler.AllocateAttributeArray(attributeValues);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int hr = callback(callbackPtr, unchecked((uint)transactionId), status, unchecked((uint)clientHandle), unchecked((uint)attributeValues.Length), valuesPtr, errorsPtr);
            ThrowIfFailed(hr, "IOPCHDA_DataCallback::OnReadAttributeComplete");
        }
        finally
        {
            OpcHdaItemMarshaler.FreeAttributeArray(valuesPtr, attributeValues.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    public void OnReadAnnotations(int transactionId, int status, OpcHdaAnnotation[] annotationValues, int[] errors)
    {
        ArgumentNullException.ThrowIfNull(annotationValues);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)vtable[7];
        IntPtr valuesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            valuesPtr = OpcHdaItemMarshaler.AllocateAnnotationArray(annotationValues);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int hr = callback(callbackPtr, unchecked((uint)transactionId), status, unchecked((uint)annotationValues.Length), valuesPtr, errorsPtr);
            ThrowIfFailed(hr, "IOPCHDA_DataCallback::OnReadAnnotations");
        }
        finally
        {
            OpcHdaItemMarshaler.FreeAnnotationArray(valuesPtr, annotationValues.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    public void OnInsertAnnotations(int transactionId, int status, int[] clientHandles, int[] errors)
    {
        ArgumentNullException.ThrowIfNull(clientHandles);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)vtable[8];
        IntPtr handlesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            handlesPtr = OpcHdaItemMarshaler.AllocateInt32Array(clientHandles);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int hr = callback(callbackPtr, unchecked((uint)transactionId), status, unchecked((uint)clientHandles.Length), handlesPtr, errorsPtr);
            ThrowIfFailed(hr, "IOPCHDA_DataCallback::OnInsertAnnotations");
        }
        finally
        {
            Marshal.FreeCoTaskMem(handlesPtr);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    public void OnPlayback(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors)
    {
        ArgumentNullException.ThrowIfNull(itemValues);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)vtable[9];
        IntPtr valuesPtr = IntPtr.Zero;
        IntPtr itemPointersPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            valuesPtr = OpcHdaItemMarshaler.AllocateItemArray(itemValues);
            itemPointersPtr = AllocateItemPointerArray(valuesPtr, itemValues.Length);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int hr = callback(callbackPtr, unchecked((uint)transactionId), status, unchecked((uint)itemValues.Length), itemPointersPtr, errorsPtr);
            ThrowIfFailed(hr, "IOPCHDA_DataCallback::OnPlayback");
        }
        finally
        {
            Marshal.FreeCoTaskMem(itemPointersPtr);
            OpcHdaItemMarshaler.FreeItemArray(valuesPtr, itemValues.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    public void OnUpdateComplete(int transactionId, int status, int[] clientHandles, int[] errors)
    {
        ArgumentNullException.ThrowIfNull(clientHandles);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)vtable[10];
        IntPtr handlesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            handlesPtr = OpcHdaItemMarshaler.AllocateInt32Array(clientHandles);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int hr = callback(callbackPtr, unchecked((uint)transactionId), status, unchecked((uint)clientHandles.Length), handlesPtr, errorsPtr);
            ThrowIfFailed(hr, "IOPCHDA_DataCallback::OnUpdateComplete");
        }
        finally
        {
            Marshal.FreeCoTaskMem(handlesPtr);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    public void OnCancelComplete(int cancelId)
    {
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int>)vtable[11];
        int hr = callback(callbackPtr, unchecked((uint)cancelId));
        ThrowIfFailed(hr, "IOPCHDA_DataCallback::OnCancelComplete");
    }

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

    private void InvokeItemCallback(int slot, int transactionId, int status, OpcHdaItem[] itemValues, int[] errors, string method)
    {
        ArgumentNullException.ThrowIfNull(itemValues);
        ArgumentNullException.ThrowIfNull(errors);
        IntPtr callbackPtr = GetCallbackPtr();
        IntPtr* vtable = *(IntPtr**)callbackPtr;
        var callback = (delegate* unmanaged<IntPtr, uint, int, uint, IntPtr, IntPtr, int>)vtable[slot];
        IntPtr valuesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            valuesPtr = OpcHdaItemMarshaler.AllocateItemArray(itemValues);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int hr = callback(callbackPtr, unchecked((uint)transactionId), status, unchecked((uint)itemValues.Length), valuesPtr, errorsPtr);
            ThrowIfFailed(hr, method);
        }
        finally
        {
            OpcHdaItemMarshaler.FreeItemArray(valuesPtr, itemValues.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    private static IntPtr AllocateItemPointerArray(IntPtr itemValues, int count)
    {
        if (count == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(count * IntPtr.Size));
        for (int i = 0; i < count; i++)
        {
            Marshal.WriteIntPtr(ptr, checked(i * IntPtr.Size), IntPtr.Add(itemValues, checked(i * OpcHdaItemMarshaler.ItemSize)));
        }

        return ptr;
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

    private static void ThrowIfFailed(int hr, string method)
    {
        if (hr < 0)
        {
            throw new COMException($"{method} failed.", hr);
        }
    }
}
