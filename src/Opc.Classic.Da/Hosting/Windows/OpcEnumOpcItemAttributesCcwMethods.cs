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
internal static unsafe class OpcEnumOpcItemAttributesCcwMethods
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Next(IntPtr pThis, uint dwNumAttributes, IntPtr* ppItemArray, uint* pdwNumAttributes)
    {
        if (ppItemArray != null)
        {
            *ppItemArray = IntPtr.Zero;
        }
        if (pdwNumAttributes != null)
        {
            *pdwNumAttributes = 0;
        }
        if (ppItemArray == null)
        {
            return OpcEnumOpcItemAttributesCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator))
        {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }

        try
        {
            int requested = dwNumAttributes > int.MaxValue ? int.MaxValue : (int)dwNumAttributes;
#pragma warning disable VSTHRD002
            OpcItemAttributes[] attributes = enumerator!.NextAsync(requested, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppItemArray = AllocateOpcItemAttributesArray(attributes);
            if (pdwNumAttributes != null)
            {
                *pdwNumAttributes = (uint)attributes.Length;
            }
            return attributes.Length == requested ? OpcEnumOpcItemAttributesCcw.S_OK : OpcEnumOpcItemAttributesCcw.S_FALSE;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Skip(IntPtr pThis, uint celt)
    {
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator))
        {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }
        try
        {
            int requested = celt > int.MaxValue ? int.MaxValue : (int)celt;
            long target = (long)enumerator!.Position + requested;
#pragma warning disable VSTHRD002
            enumerator.SkipAsync(requested, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return celt <= int.MaxValue && target <= enumerator.Length
                ? OpcEnumOpcItemAttributesCcw.S_OK
                : OpcEnumOpcItemAttributesCcw.S_FALSE;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Reset(IntPtr pThis)
    {
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator))
        {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            enumerator!.ResetAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcEnumOpcItemAttributesCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Clone(IntPtr pThis, IntPtr* ppEnum)
    {
        if (ppEnum != null)
        {
            *ppEnum = IntPtr.Zero;
        }
        if (ppEnum == null)
        {
            return OpcEnumOpcItemAttributesCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcDaItemAttributesEnumerator? enumerator))
        {
            return OpcEnumOpcItemAttributesCcw.E_FAIL;
        }
        try
        {
            OpcDaItemAttributesEnumerator clone = CloneEnumeratorForCcw(enumerator!);
            *ppEnum = OpcEnumOpcItemAttributesCcw.Create(clone);
            return OpcEnumOpcItemAttributesCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct OPCITEMATTRIBUTES_NATIVE
    {
        public IntPtr szAccessPath;
        public IntPtr szItemID;
        public int bActive;
        public uint hClient;
        public uint hServer;
        public uint dwAccessRights;
        public uint dwBlobSize;
        public IntPtr pBlob;
        public ushort vtRequestedDataType;
        public ushort vtCanonicalDataType;
        public ushort wReserved1;
        public ushort wReserved2;
        public uint dwEUType;
        public VARIANT_EMPTY_NATIVE vEUInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct VARIANT_EMPTY_NATIVE
    {
        public long Part0;
        public long Part1;
    }

    private static bool TryResolve(IntPtr pThis, out OpcDaItemAttributesEnumerator? enumerator)
    {
        enumerator = OpcEnumOpcItemAttributesCcw.ResolveEnumerator(pThis);
        return enumerator is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentNullException => OpcEnumOpcItemAttributesCcw.E_INVALIDARG,
        ArgumentException => OpcEnumOpcItemAttributesCcw.E_INVALIDARG,
        _ => OpcEnumOpcItemAttributesCcw.E_FAIL,
    };

    private static OpcDaItemAttributesEnumerator CloneEnumeratorForCcw(OpcDaItemAttributesEnumerator enumerator)
    {
        int position = enumerator.Position;
#pragma warning disable VSTHRD002
        enumerator.ResetAsync(CancellationToken.None).GetAwaiter().GetResult();
        OpcItemAttributes[] snapshot = enumerator.NextAsync(enumerator.Length, CancellationToken.None).GetAwaiter().GetResult();
        enumerator.ResetAsync(CancellationToken.None).GetAwaiter().GetResult();
        enumerator.SkipAsync(position, CancellationToken.None).GetAwaiter().GetResult();
        var clone = new OpcDaItemAttributesEnumerator(snapshot);
        clone.SkipAsync(position, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        return clone;
    }

    private static IntPtr AllocateOpcItemAttributesArray(OpcItemAttributes[] attributes)
    {
        if (attributes.Length == 0)
        {
            return IntPtr.Zero;
        }
        int size = Marshal.SizeOf<OPCITEMATTRIBUTES_NATIVE>();
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(attributes.Length * size));
        for (int i = 0; i < attributes.Length; i++)
        {
            Marshal.StructureToPtr(ToNative(attributes[i]), IntPtr.Add(ptr, i * size), fDeleteOld: false);
        }
        return ptr;
    }

    private static OPCITEMATTRIBUTES_NATIVE ToNative(OpcItemAttributes attributes)
    {
        byte[] blob = attributes.Blob ?? Array.Empty<byte>();
        return new OPCITEMATTRIBUTES_NATIVE
        {
            szAccessPath = AllocateLpwStr(attributes.AccessPath),
            szItemID = AllocateLpwStr(attributes.ItemId),
            bActive = attributes.Active ? 1 : 0,
            hClient = unchecked((uint)attributes.ClientHandle),
            hServer = unchecked((uint)attributes.ServerHandle),
            dwAccessRights = unchecked((uint)attributes.AccessRights),
            dwBlobSize = unchecked((uint)blob.Length),
            pBlob = AllocateBlob(blob),
            vtRequestedDataType = (ushort)attributes.RequestedDataType,
            vtCanonicalDataType = (ushort)attributes.CanonicalDataType,
            dwEUType = unchecked((uint)attributes.EUType),
            vEUInfo = default,
        };
    }

    private static IntPtr AllocateLpwStr(string? value)
    {
        if (value is null)
        {
            return IntPtr.Zero;
        }
        int byteCount = (value.Length + 1) * sizeof(char);
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        Marshal.Copy(value.ToCharArray(), 0, ptr, value.Length);
        Marshal.WriteInt16(ptr, value.Length * sizeof(char), 0);
        return ptr;
    }

    private static IntPtr AllocateBlob(byte[] blob)
    {
        if (blob.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(blob.Length);
        Marshal.Copy(blob, 0, ptr, blob.Length);
        return ptr;
    }
}
