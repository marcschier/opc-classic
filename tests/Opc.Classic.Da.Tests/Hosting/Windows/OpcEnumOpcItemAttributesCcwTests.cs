//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>
/// Windows-only tests for the IEnumOPCItemAttributes CCW.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcEnumOpcItemAttributesCcwTests
{
    private const int S_OK = 0;

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task Create_and_QueryInterface_return_expected_interfaces()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcEnumOpcItemAttributesCcw.Create(NewEnumerator());
        IntPtr unknown = Helpers.InvokeQI(ccw, IID_IUnknown);
        IntPtr enumPtr = Helpers.InvokeQI(ccw, IEnumOPCItemAttributes.InterfaceId);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(unknown).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(enumPtr).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Release_to_zero_cleans_up_entry()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcEnumOpcItemAttributesCcw.Create(NewEnumerator());
        Helpers.InvokeRelease(ccw);

        await Assert.That(OpcEnumOpcItemAttributesCcw.GetReferenceCount(ccw)).IsEqualTo(-1L);
    }

    [Test]
    public async Task Skip_and_Reset_round_trip_to_first_item()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcEnumOpcItemAttributesCcw.Create(NewEnumerator());
        int skipHr = Helpers.InvokeSkip(ccw, 1);
        int resetHr = Helpers.InvokeReset(ccw);
        Helpers.EnumNextResult next = Helpers.InvokeNext(ccw, 1);

        await Assert.That(skipHr).IsEqualTo(S_OK);
        await Assert.That(resetHr).IsEqualTo(S_OK);
        await Assert.That(next.Hr).IsEqualTo(S_OK);
        await Assert.That(next.ItemIds[0]).IsEqualTo("Tag.0");
    }

    [Test]
    public async Task Clone_returns_nonzero_distinct_ccw()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcEnumOpcItemAttributesCcw.Create(NewEnumerator());
        (int hr, IntPtr clone) = Helpers.InvokeClone(ccw);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(clone).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(clone).IsNotEqualTo(ccw);
    }

    private static OpcDaItemAttributesEnumerator NewEnumerator() =>
        new(BuildSnapshot(3));

    private static OpcItemAttributes[] BuildSnapshot(int count)
    {
        var snapshot = new OpcItemAttributes[count];
        for (int i = 0; i < count; i++)
        {
            snapshot[i] = new OpcItemAttributes(
                AccessPath: "",
                ItemId: $"Tag.{i}",
                Active: true,
                ClientHandle: i,
                ServerHandle: i + 100,
                AccessRights: 0x3,
                Blob: Array.Empty<byte>(),
                RequestedDataType: VarType.VT_I4,
                CanonicalDataType: VarType.VT_I4,
                EUType: 0,
                EUInfo: OpcVariant.Empty);
        }
        return snapshot;
    }

    private static unsafe class Helpers
    {
        internal readonly record struct EnumNextResult(int Hr, uint Fetched, string?[] ItemIds);

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
            public long vEUInfo0;
            public long vEUInfo1;
        }

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var qi = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
            Guid local = iid;
            IntPtr returned;
            int hr = qi(ccw, &local, &returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static void InvokeRelease(IntPtr ccw)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
            release(ccw);
        }

        internal static int InvokeSkip(IntPtr ccw, uint count)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var skip = (delegate* unmanaged<IntPtr, uint, int>)vtable[4];
            return skip(ccw, count);
        }

        internal static int InvokeReset(IntPtr ccw)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var reset = (delegate* unmanaged<IntPtr, int>)vtable[5];
            return reset(ccw);
        }

        internal static (int Hr, IntPtr Clone) InvokeClone(IntPtr ccw)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var clone = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[6];
            IntPtr ppEnum;
            int hr = clone(ccw, &ppEnum);
            return (hr, ppEnum);
        }

        internal static EnumNextResult InvokeNext(IntPtr ccw, uint count)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var next = (delegate* unmanaged<IntPtr, uint, IntPtr*, uint*, int>)vtable[3];
            IntPtr ppItems;
            uint fetched;
            int hr = next(ccw, count, &ppItems, &fetched);
            string?[] itemIds = ReadItemIdsAndFree(ppItems, (int)fetched);
            return new EnumNextResult(hr, fetched, itemIds);
        }

        private static string?[] ReadItemIdsAndFree(IntPtr ptr, int count)
        {
            var itemIds = new string?[count];
            int size = Marshal.SizeOf<OPCITEMATTRIBUTES_NATIVE>();
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                var native = Marshal.PtrToStructure<OPCITEMATTRIBUTES_NATIVE>(IntPtr.Add(ptr, i * size));
                itemIds[i] = Marshal.PtrToStringUni(native.szItemID);
                FreeNativeAttributes(native);
            }
            Marshal.FreeCoTaskMem(ptr);
            return itemIds;
        }

        private static void FreeNativeAttributes(OPCITEMATTRIBUTES_NATIVE native)
        {
            Marshal.FreeCoTaskMem(native.szAccessPath);
            Marshal.FreeCoTaskMem(native.szItemID);
            Marshal.FreeCoTaskMem(native.pBlob);
        }
    }
}
