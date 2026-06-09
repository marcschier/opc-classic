//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Method bodies for the <see cref="OpcEnumOpcItemAttributesCcw"/> vtable.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcEnumOpcItemAttributesCcwMethods {
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Next(IntPtr pThis, uint dwNumAttributes, IntPtr* ppItemArray, uint* pdwNumAttributes) {
        if (ppItemArray != null) {
            *ppItemArray = IntPtr.Zero;
        }
        if (pdwNumAttributes != null) {
            *pdwNumAttributes = 0;
        }
        if (ppItemArray == null) {
            return OpcEnumOpcItemAttributesCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator)) {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }

        try {
            int requested = dwNumAttributes > int.MaxValue ? int.MaxValue : (int)dwNumAttributes;
#pragma warning disable VSTHRD002
            enumerator!.NextAsync(requested, out OpcItemAttributes[] attributes, out int fetched, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppItemArray = AllocateOpcItemAttributesArray(attributes);
            if (pdwNumAttributes != null) {
                *pdwNumAttributes = (uint)fetched;
            }
            return fetched == requested ? OpcEnumOpcItemAttributesCcw.S_OK : OpcEnumOpcItemAttributesCcw.S_FALSE;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Skip(IntPtr pThis, uint celt) {
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator)) {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }
        try {
            int requested = celt > int.MaxValue ? int.MaxValue : (int)celt;
            long target = (long)enumerator!.Position + requested;
#pragma warning disable VSTHRD002
            enumerator.SkipAsync(requested, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return celt <= int.MaxValue && target <= enumerator.Length
                ? OpcEnumOpcItemAttributesCcw.S_OK
                : OpcEnumOpcItemAttributesCcw.S_FALSE;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Reset(IntPtr pThis) {
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator)) {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }
        try {
#pragma warning disable VSTHRD002
            enumerator!.ResetAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcEnumOpcItemAttributesCcw.S_OK;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Clone(IntPtr pThis, IntPtr* ppEnum) {
        if (ppEnum != null) {
            *ppEnum = IntPtr.Zero;
        }
        if (ppEnum == null) {
            return OpcEnumOpcItemAttributesCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator)) {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }
        try {
            OpcDaItemAttributesEnumerator clone = CloneEnumeratorForCcw(enumerator!);
            *ppEnum = OpcEnumOpcItemAttributesCcw.Create(clone);
            return OpcEnumOpcItemAttributesCcw.S_OK;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    private const int Int32Size = 4;
    private const int UInt16Size = 2;
    private const int VariantSlotStride = 16;
    private const int SzAccessPathOffset = 0;

    private static readonly int s_pointerSize = IntPtr.Size;
    private static readonly int s_szItemIdOffset = SzAccessPathOffset + s_pointerSize;
    private static readonly int s_bActiveOffset = s_szItemIdOffset + s_pointerSize;
    private static readonly int s_hClientOffset = s_bActiveOffset + Int32Size;
    private static readonly int s_hServerOffset = s_hClientOffset + Int32Size;
    private static readonly int s_dwAccessRightsOffset = s_hServerOffset + Int32Size;
    private static readonly int s_dwBlobSizeOffset = s_dwAccessRightsOffset + Int32Size;
    private static readonly int s_pBlobOffset = s_dwBlobSizeOffset + Int32Size;
    private static readonly int s_vtRequestedDataTypeOffset = s_pBlobOffset + s_pointerSize;
    private static readonly int s_vtCanonicalDataTypeOffset = s_vtRequestedDataTypeOffset + UInt16Size;
    private static readonly int s_wReserved1Offset = s_vtCanonicalDataTypeOffset + UInt16Size;
    private static readonly int s_wReserved2Offset = s_wReserved1Offset + UInt16Size;
    private static readonly int s_dwEUTypeOffset = s_wReserved2Offset + UInt16Size;
    private static readonly int s_vEUInfoOffset = s_dwEUTypeOffset + Int32Size;
    private static readonly int s_opcItemAttributesSize = s_vEUInfoOffset + VariantSlotStride;
    // WriteVariant emits the platform VARIANT size; pad the final packed slot for the x64 tail bytes.
    private static readonly int s_variantTailPadding = ComVariantMarshaler.VariantSize > VariantSlotStride
        ? ComVariantMarshaler.VariantSize - VariantSlotStride
        : 0;

    private static bool TryResolve(IntPtr pThis, out OpcDaItemAttributesEnumerator? enumerator) {
        enumerator = OpcEnumOpcItemAttributesCcw.ResolveEnumerator(pThis);
        return enumerator is not null;
    }

    private static int MapHResult(Exception ex) => ex switch {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentNullException => OpcEnumOpcItemAttributesCcw.E_INVALIDARG,
        ArgumentException => OpcEnumOpcItemAttributesCcw.E_INVALIDARG,
        _ => OpcEnumOpcItemAttributesCcw.E_FAIL,
    };

    private static OpcDaItemAttributesEnumerator CloneEnumeratorForCcw(OpcDaItemAttributesEnumerator enumerator) {
        int position = enumerator.Position;
#pragma warning disable VSTHRD002
        enumerator.ResetAsync(CancellationToken.None).GetAwaiter().GetResult();
        enumerator.NextAsync(enumerator.Length, out OpcItemAttributes[] snapshot, out _, CancellationToken.None).GetAwaiter().GetResult();
        enumerator.ResetAsync(CancellationToken.None).GetAwaiter().GetResult();
        enumerator.SkipAsync(position, CancellationToken.None).GetAwaiter().GetResult();
        var clone = new OpcDaItemAttributesEnumerator(snapshot);
        clone.SkipAsync(position, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        return clone;
    }

    private static IntPtr AllocateOpcItemAttributesArray(OpcItemAttributes[] attributes) {
        if (attributes.Length == 0) {
            return IntPtr.Zero;
        }
        int byteCount = checked((attributes.Length * s_opcItemAttributesSize) + s_variantTailPadding);
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        NativeMemory.Clear((void*)ptr, (nuint)byteCount);

        int slotsTouched = 0;
        try {
            for (int i = 0; i < attributes.Length; i++) {
                slotsTouched = i + 1;
                WriteNativeAttribute(IntPtr.Add(ptr, i * s_opcItemAttributesSize), attributes[i]);
            }
            return ptr;
        }
        catch {
            FreeNativeAttributesArray(ptr, slotsTouched);
            throw;
        }
    }

    private static void WriteNativeAttribute(IntPtr destination, OpcItemAttributes attributes) {
        byte[] blob = attributes.Blob ?? Array.Empty<byte>();

        Marshal.WriteIntPtr(destination, SzAccessPathOffset, AllocateLpwStr(attributes.AccessPath));
        Marshal.WriteIntPtr(destination, s_szItemIdOffset, AllocateLpwStr(attributes.ItemId));
        Marshal.WriteInt32(destination, s_bActiveOffset, attributes.Active ? 1 : 0);
        Marshal.WriteInt32(destination, s_hClientOffset, attributes.ClientHandle);
        Marshal.WriteInt32(destination, s_hServerOffset, attributes.ServerHandle);
        Marshal.WriteInt32(destination, s_dwAccessRightsOffset, attributes.AccessRights);
        Marshal.WriteInt32(destination, s_dwBlobSizeOffset, blob.Length);
        Marshal.WriteIntPtr(destination, s_pBlobOffset, AllocateBlob(blob));
        Marshal.WriteInt16(destination, s_vtRequestedDataTypeOffset, unchecked((short)(ushort)attributes.RequestedDataType));
        Marshal.WriteInt16(destination, s_vtCanonicalDataTypeOffset, unchecked((short)(ushort)attributes.CanonicalDataType));
        Marshal.WriteInt32(destination, s_dwEUTypeOffset, attributes.EUType);
        ComVariantMarshaler.WriteVariant(IntPtr.Add(destination, s_vEUInfoOffset), attributes.EUInfo);
    }

    private static void FreeNativeAttributesArray(IntPtr ptr, int count) {
        for (int i = 0; i < count; i++) {
            IntPtr slot = IntPtr.Add(ptr, i * s_opcItemAttributesSize);
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, SzAccessPathOffset));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, s_szItemIdOffset));
            Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, s_pBlobOffset));
        }
        // Clear variants after pointer fields so x64 VARIANT tail bytes cannot hide a later slot allocation.
        for (int i = 0; i < count; i++) {
            ComVariantMarshaler.ClearVariant(IntPtr.Add(ptr, (i * s_opcItemAttributesSize) + s_vEUInfoOffset));
        }
        Marshal.FreeCoTaskMem(ptr);
    }

    private static IntPtr AllocateLpwStr(string? value) {
        if (value is null) {
            return IntPtr.Zero;
        }
        int byteCount = (value.Length + 1) * sizeof(char);
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        Marshal.Copy(value.ToCharArray(), 0, ptr, value.Length);
        Marshal.WriteInt16(ptr, value.Length * sizeof(char), 0);
        return ptr;
    }

    private static IntPtr AllocateBlob(byte[] blob) {
        if (blob.Length == 0) {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(blob.Length);
        Marshal.Copy(blob, 0, ptr, blob.Length);
        return ptr;
    }
}
