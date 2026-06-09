//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting.Windows;

/// <summary>Outbound Windows COM proxy for a client-supplied <c>IOPCEventSink</c>.</summary>
[SupportedOSPlatform("windows")]
public sealed unsafe class OpcAeEventSinkProxy : IOPCEventSink, IDisposable {
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_POINTER = unchecked((int)0x80004003);
    private const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);
    private const int Win32BoolFalse = 0;

    private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid s_iidEventSink = IOPCEventSink.InterfaceId;

    private readonly Lock _syncRoot = new();
    private IntPtr _sinkPtr;

    /// <summary>Initializes a new proxy from a client-supplied <c>IUnknown</c> pointer.</summary>
    /// <param name="clientUnknown">Client sink <c>IUnknown</c> pointer.</param>
    /// <exception cref="COMException">Thrown when the pointer is null or does not support <c>IOPCEventSink</c>.</exception>
    public OpcAeEventSinkProxy(IntPtr clientUnknown) {
        if (clientUnknown == IntPtr.Zero) {
            throw new COMException("Client IUnknown pointer is null.", E_POINTER);
        }

        InvokeAddRef(clientUnknown);
        try {
            _sinkPtr = QueryInterface(clientUnknown, s_iidEventSink, "Client sink does not implement IOPCEventSink.");
        }
        finally {
            InvokeRelease(clientUnknown);
        }
    }

    internal IntPtr AddRefCallbackUnknown() {
        lock (_syncRoot) {
            IntPtr sinkPtr = _sinkPtr;
            ObjectDisposedException.ThrowIf(sinkPtr == IntPtr.Zero, this);
            return QueryInterface(sinkPtr, s_iidUnknown, "Client sink does not expose IUnknown.");
        }
    }

    /// <inheritdoc />
    public Task OnEventAsync(
        int clientSubscription,
        bool refresh,
        bool lastRefresh,
        OpcEventNotification[] events,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        OnEvent(clientSubscription, refresh, lastRefresh, events);
        return Task.CompletedTask;
    }

    /// <summary>Calls <c>IOPCEventSink::OnEvent</c> (opnum 3).</summary>
    public void OnEvent(int clientSubscription, bool refresh, bool lastRefresh, OpcEventNotification[] events) {
        ArgumentNullException.ThrowIfNull(events);
        IntPtr sinkPtr = GetSinkPtr();
        IntPtr* vtable = *(IntPtr**)sinkPtr;
        var onEvent = (delegate* unmanaged<IntPtr, uint, int, int, uint, IntPtr, int>)vtable[3];
        IntPtr eventsPtr = IntPtr.Zero;
        try {
            eventsPtr = AllocateEventArray(events);
            int hr = onEvent(
                sinkPtr,
                unchecked((uint)clientSubscription),
                refresh ? Win32BoolTrue : Win32BoolFalse,
                lastRefresh ? Win32BoolTrue : Win32BoolFalse,
                unchecked((uint)events.Length),
                eventsPtr);
            ThrowIfFailed(hr, "IOPCEventSink::OnEvent");
        }
        finally {
            FreeEventArray(eventsPtr, events.Length);
        }
    }

    /// <summary>Releases the held <c>IOPCEventSink</c> pointer.</summary>
    public void Dispose() {
        IntPtr sinkPtr;
        lock (_syncRoot) {
            sinkPtr = _sinkPtr;
            _sinkPtr = IntPtr.Zero;
        }

        if (sinkPtr != IntPtr.Zero) {
            InvokeRelease(sinkPtr);
        }

        GC.SuppressFinalize(this);
    }

    private static IntPtr QueryInterface(IntPtr instance, Guid iid, string failureMessage) {
        IntPtr* vtable = *(IntPtr**)instance;
        var queryInterface = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
        Guid local = iid;
        IntPtr returned = IntPtr.Zero;
        int hr = queryInterface(instance, &local, &returned);
        if (hr < 0) {
            throw new COMException(failureMessage, hr);
        }
        if (returned == IntPtr.Zero) {
            throw new COMException(failureMessage, E_NOINTERFACE);
        }
        return returned;
    }

    private static void InvokeAddRef(IntPtr unknown) {
        IntPtr* vtable = *(IntPtr**)unknown;
        var addRef = (delegate* unmanaged<IntPtr, uint>)vtable[1];
        _ = addRef(unknown);
    }

    private static void InvokeRelease(IntPtr unknown) {
        IntPtr* vtable = *(IntPtr**)unknown;
        var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
        _ = release(unknown);
    }

    private IntPtr GetSinkPtr() {
        IntPtr sinkPtr = _sinkPtr;
        ObjectDisposedException.ThrowIf(sinkPtr == IntPtr.Zero, this);
        return sinkPtr;
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateEventArray(OpcEventNotification[] events) {
        if (events.Length == 0) {
            return IntPtr.Zero;
        }

        int structSize = sizeof(ONEVENTSTRUCT_NATIVE);
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(events.Length * structSize));
        NativeMemory.Clear((void*)ptr, (nuint)(events.Length * structSize));
        bool completed = false;
        try {
            byte* basePtr = (byte*)ptr;
            for (int i = 0; i < events.Length; i++) {
                WriteEvent((ONEVENTSTRUCT_NATIVE*)(basePtr + (i * structSize)), events[i]);
            }
            completed = true;
            return ptr;
        }
        finally {
            if (!completed) {
                FreeEventArray(ptr, events.Length);
            }
        }
    }

    private static void WriteEvent(ONEVENTSTRUCT_NATIVE* target, OpcEventNotification notification) {
        ArgumentNullException.ThrowIfNull(notification);
        target->wChangeMask = notification.ChangeMask;
        target->wNewState = notification.NewState;
        target->szSource = AllocateBstr(notification.Source);
        target->ftTime = FileTimeHelper.ToFileTime(notification.Time);
        target->szMessage = AllocateBstr(notification.Message);
        target->dwEventType = notification.EventType;
        target->dwEventCategory = notification.EventCategory;
        target->dwSeverity = notification.Severity;
        target->szConditionName = AllocateBstr(notification.ConditionName);
        target->szSubconditionName = AllocateBstr(notification.SubconditionName);
        target->wQuality = notification.Quality.RawValue;
        target->wReserved = 0;
        target->bAckRequired = notification.AckRequired ? Win32BoolTrue : Win32BoolFalse;
        target->ftActiveTime = FileTimeHelper.ToFileTime(notification.ActiveTime);
        target->dwCookie = notification.Cookie;
        target->dwNumEventAttrs = unchecked((uint)notification.EventAttributes.Length);
        target->pEventAttributes = VariantMarshaler.AllocateVariantArray(notification.EventAttributes);
        target->szActorID = AllocateBstr(notification.ActorId);
    }

    private static IntPtr AllocateBstr(string? value) =>
        value is null ? IntPtr.Zero : Marshal.StringToBSTR(value);

    private static void FreeEventArray(IntPtr ptr, int count) {
        if (ptr == IntPtr.Zero) {
            return;
        }

        int structSize = sizeof(ONEVENTSTRUCT_NATIVE);
        byte* basePtr = (byte*)ptr;
        for (int i = 0; i < count; i++) {
            ONEVENTSTRUCT_NATIVE* item = (ONEVENTSTRUCT_NATIVE*)(basePtr + (i * structSize));
            FreeBstr(item->szSource);
            FreeBstr(item->szMessage);
            FreeBstr(item->szConditionName);
            FreeBstr(item->szSubconditionName);
            VariantMarshaler.FreeVariantArray(item->pEventAttributes, checked((int)item->dwNumEventAttrs));
            FreeBstr(item->szActorID);
        }
        Marshal.FreeCoTaskMem(ptr);
    }

    private static void FreeBstr(IntPtr value) {
        if (value != IntPtr.Zero) {
            Marshal.FreeBSTR(value);
        }
    }

    private static void ThrowIfFailed(int hr, string method) {
        if (hr < 0) {
            throw new COMException($"{method} failed.", hr);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ONEVENTSTRUCT_NATIVE {
        public ushort wChangeMask;
        public ushort wNewState;
        public IntPtr szSource;
        public long ftTime;
        public IntPtr szMessage;
        public uint dwEventType;
        public uint dwEventCategory;
        public uint dwSeverity;
        public IntPtr szConditionName;
        public IntPtr szSubconditionName;
        public ushort wQuality;
        public ushort wReserved;
        public int bAckRequired;
        public long ftActiveTime;
        public uint dwCookie;
        public uint dwNumEventAttrs;
        public IntPtr pEventAttributes;
        public IntPtr szActorID;
    }

    private static class VariantMarshaler {
        private const ushort VT_EMPTY = 0;
        private const ushort VT_NULL = 1;
        private const ushort VT_I2 = 2;
        private const ushort VT_I4 = 3;
        private const ushort VT_R4 = 4;
        private const ushort VT_R8 = 5;
        private const ushort VT_DATE = 7;
        private const ushort VT_BSTR = 8;
        private const ushort VT_ERROR = 10;
        private const ushort VT_BOOL = 11;
        private const ushort VT_I1 = 16;
        private const ushort VT_UI1 = 17;
        private const ushort VT_UI2 = 18;
        private const ushort VT_UI4 = 19;
        private const ushort VT_I8 = 20;
        private const ushort VT_UI8 = 21;
        private const ushort VT_ARRAY = 0x2000;

        private static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

        internal static IntPtr AllocateVariantArray(OpcVariant[] values) {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length == 0) {
                return IntPtr.Zero;
            }

            int variantSize = VariantSize;
            IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * variantSize));
            NativeMemory.Clear((void*)ptr, (nuint)(values.Length * variantSize));
            bool completed = false;
            try {
                for (int i = 0; i < values.Length; i++) {
                    WriteVariant(ptr + (i * variantSize), values[i]);
                }
                completed = true;
                return ptr;
            }
            finally {
                if (!completed) {
                    FreeVariantArray(ptr, values.Length);
                }
            }
        }

        internal static void FreeVariantArray(IntPtr ptr, int count) {
            if (ptr == IntPtr.Zero) {
                return;
            }

            int variantSize = VariantSize;
            for (int i = 0; i < count; i++) {
                ClearVariant(ptr + (i * variantSize));
            }
            Marshal.FreeCoTaskMem(ptr);
        }

        private static int ValueOffset => 8;

        private static void WriteVariant(IntPtr dest, OpcVariant variant) {
            NativeMemory.Clear((void*)dest, (nuint)VariantSize);
            ushort vt = (ushort)variant.Type;
            Marshal.WriteInt16(dest, (short)vt);
            if ((vt & VT_ARRAY) != 0) {
                WriteSafeArrayPayload(dest, variant);
                return;
            }
            WriteScalarPayload(dest, vt, variant.Boxed);
        }

        private static void ClearVariant(IntPtr ptr) {
            ushort vt = unchecked((ushort)Marshal.ReadInt16(ptr));
            if (vt == VT_BSTR) {
                IntPtr bstr = Marshal.ReadIntPtr(ptr, ValueOffset);
                if (bstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(bstr);
                }
            }
            else if ((vt & VT_ARRAY) != 0) {
                IntPtr safeArrayPtr = Marshal.ReadIntPtr(ptr, ValueOffset);
                if (safeArrayPtr != IntPtr.Zero) {
                    FreeSafeArray(safeArrayPtr, (ushort)(vt & ~VT_ARRAY));
                }
            }
            NativeMemory.Clear((void*)ptr, (nuint)VariantSize);
        }

        [SuppressMessage("Design", "CA1502:Avoid excessive complexity", Justification = "VARIANT scalar dispatch requires one branch per VARENUM code.")]
        private static void WriteScalarPayload(IntPtr dest, ushort vt, object? boxed) {
            IntPtr value = dest + ValueOffset;
            switch (vt) {
                case VT_I1:
                    Marshal.WriteByte(value, (byte)(sbyte)(boxed ?? (sbyte)0));
                    break;
                case VT_UI1:
                    Marshal.WriteByte(value, (byte)(boxed ?? (byte)0));
                    break;
                case VT_I2:
                    Marshal.WriteInt16(value, (short)(boxed ?? (short)0));
                    break;
                case VT_UI2:
                    Marshal.WriteInt16(value, (short)(ushort)(boxed ?? (ushort)0));
                    break;
                case VT_BOOL:
                    Marshal.WriteInt16(value, (boxed is bool b && b) ? unchecked((short)0xFFFF) : (short)0);
                    break;
                case VT_I4:
                case VT_ERROR:
                    Marshal.WriteInt32(value, (int)(boxed ?? 0));
                    break;
                case VT_UI4:
                    Marshal.WriteInt32(value, unchecked((int)(uint)(boxed ?? 0u)));
                    break;
                case VT_R4:
                    Marshal.WriteInt32(value, BitConverter.SingleToInt32Bits((float)(boxed ?? 0f)));
                    break;
                case VT_I8:
                    Marshal.WriteInt64(value, (long)(boxed ?? 0L));
                    break;
                case VT_UI8:
                    Marshal.WriteInt64(value, unchecked((long)(ulong)(boxed ?? 0ul)));
                    break;
                case VT_R8:
                    Marshal.WriteInt64(value, BitConverter.DoubleToInt64Bits((double)(boxed ?? 0d)));
                    break;
                case VT_DATE:
                    Marshal.WriteInt64(value, BitConverter.DoubleToInt64Bits(((DateTime?)boxed ?? DateTime.UnixEpoch).ToOADate()));
                    break;
                case VT_BSTR:
                    Marshal.WriteIntPtr(value, boxed is string text ? Marshal.StringToBSTR(text) : IntPtr.Zero);
                    break;
                case VT_EMPTY:
                case VT_NULL:
                    break;
                default:
                    break;
            }
        }

        private static void WriteSafeArrayPayload(IntPtr dest, OpcVariant variant) {
            OpcSafeArray? array = variant.AsSafeArray();
            if (array is null || array.Data.Length == 0) {
                Marshal.WriteIntPtr(dest, ValueOffset, IntPtr.Zero);
                return;
            }
            Marshal.WriteIntPtr(dest, ValueOffset, AllocateSafeArray(array));
        }

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr AllocateSafeArray(OpcSafeArray array) {
            ushort baseVt = (ushort)array.ElementType;
            uint elementSize = (uint)ElementSizeOf(baseVt);
            uint count = (uint)array.Data.Length;
            int pvDataOffset = 8 + IntPtr.Size;
            int boundsOffset = pvDataOffset + IntPtr.Size;
            int totalDescriptorSize = boundsOffset + 8;
            IntPtr descriptor = Marshal.AllocCoTaskMem(totalDescriptorSize);
            IntPtr dataBuffer = Marshal.AllocCoTaskMem(checked((int)(elementSize * count)));

            Marshal.WriteInt16(descriptor, 0, 1);
            Marshal.WriteInt16(descriptor, 2, 0x10);
            Marshal.WriteInt32(descriptor, 4, (int)elementSize);
            Marshal.WriteInt32(descriptor, 8, 0);
            Marshal.WriteIntPtr(descriptor, pvDataOffset, dataBuffer);
            Marshal.WriteInt32(descriptor, boundsOffset, (int)count);
            Marshal.WriteInt32(descriptor, boundsOffset + 4, 0);
            WriteSafeArrayData(dataBuffer, array, baseVt, elementSize);
            return descriptor;
        }

        private static void FreeSafeArray(IntPtr descriptor, ushort baseVt) {
            int pvDataOffset = 8 + IntPtr.Size;
            int boundsOffset = pvDataOffset + IntPtr.Size;
            IntPtr dataBuffer = Marshal.ReadIntPtr(descriptor, pvDataOffset);
            if (dataBuffer != IntPtr.Zero) {
                if (baseVt == VT_BSTR) {
                    uint count = unchecked((uint)Marshal.ReadInt32(descriptor, boundsOffset));
                    for (uint i = 0; i < count; i++) {
                        IntPtr bstr = Marshal.ReadIntPtr(dataBuffer, checked((int)(i * (uint)IntPtr.Size)));
                        if (bstr != IntPtr.Zero) {
                            Marshal.FreeBSTR(bstr);
                        }
                    }
                }
                Marshal.FreeCoTaskMem(dataBuffer);
            }
            Marshal.FreeCoTaskMem(descriptor);
        }

        private static int ElementSizeOf(ushort baseVt) => baseVt switch {
            VT_I1 or VT_UI1 => 1,
            VT_I2 or VT_UI2 or VT_BOOL => 2,
            VT_I4 or VT_UI4 or VT_ERROR or VT_R4 => 4,
            VT_I8 or VT_UI8 or VT_R8 or VT_DATE => 8,
            VT_BSTR => IntPtr.Size,
            _ => IntPtr.Size,
        };

        [SuppressMessage("Design", "CA1502:Avoid excessive complexity", Justification = "SAFEARRAY element dispatch requires one branch per VARENUM code.")]
        private static void WriteSafeArrayData(IntPtr dataBuffer, OpcSafeArray array, ushort baseVt, uint elementSize) {
            Array data = array.Data;
            for (int i = 0; i < data.Length; i++) {
                IntPtr slot = dataBuffer + checked((int)(i * elementSize));
                object? value = data.GetValue(i);
                switch (baseVt) {
                    case VT_I1: Marshal.WriteByte(slot, (byte)(sbyte)(value ?? (sbyte)0)); break;
                    case VT_UI1: Marshal.WriteByte(slot, (byte)(value ?? (byte)0)); break;
                    case VT_I2: Marshal.WriteInt16(slot, (short)(value ?? (short)0)); break;
                    case VT_UI2: Marshal.WriteInt16(slot, (short)(ushort)(value ?? (ushort)0)); break;
                    case VT_BOOL: Marshal.WriteInt16(slot, (value is bool b && b) ? unchecked((short)0xFFFF) : (short)0); break;
                    case VT_I4: case VT_ERROR: Marshal.WriteInt32(slot, (int)(value ?? 0)); break;
                    case VT_UI4: Marshal.WriteInt32(slot, unchecked((int)(uint)(value ?? 0u))); break;
                    case VT_R4: Marshal.WriteInt32(slot, BitConverter.SingleToInt32Bits((float)(value ?? 0f))); break;
                    case VT_I8: Marshal.WriteInt64(slot, (long)(value ?? 0L)); break;
                    case VT_UI8: Marshal.WriteInt64(slot, unchecked((long)(ulong)(value ?? 0ul))); break;
                    case VT_R8: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits((double)(value ?? 0d))); break;
                    case VT_DATE: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits(((DateTime?)value ?? DateTime.UnixEpoch).ToOADate())); break;
                    case VT_BSTR: Marshal.WriteIntPtr(slot, value is string text ? Marshal.StringToBSTR(text) : IntPtr.Zero); break;
                    default: break;
                }
            }
        }
    }
}
