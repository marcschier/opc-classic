// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;

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

        IntPtr ccw = OpcEnumOpcItemAttributesCcw.Create(
            new OpcDaItemAttributesEnumerator(BuildSnapshot(3, static i => i == 0 ? OpcVariant.FromString("Volts") : OpcVariant.Empty)));
        int skipHr = Helpers.InvokeSkip(ccw, 1);
        int resetHr = Helpers.InvokeReset(ccw);
        Helpers.EnumNextResult next = Helpers.InvokeNext(ccw, 1);

        await Assert.That(skipHr).IsEqualTo(S_OK);
        await Assert.That(resetHr).IsEqualTo(S_OK);
        await Assert.That(next.Hr).IsEqualTo(S_OK);
        await Assert.That(next.ItemIds[0]).IsEqualTo("Tag.0");
        await Assert.That(next.EUInfos[0].Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(next.EUInfos[0].AsString()).IsEqualTo("Volts");
    }

    [Test]
    public async Task Next_round_trips_empty_EUInfo_variant()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcEnumOpcItemAttributesCcw.Create(new OpcDaItemAttributesEnumerator(BuildSnapshot(1)));
        Helpers.EnumNextResult next = Helpers.InvokeNext(ccw, 1);

        await Assert.That(next.Hr).IsEqualTo(S_OK);
        await Assert.That(next.EUInfos[0].Type).IsEqualTo(VarType.VT_EMPTY);
        await Assert.That(next.EUInfos[0].IsEmpty).IsTrue();
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

    private static OpcItemAttributes[] BuildSnapshot(int count) =>
        BuildSnapshot(count, static _ => OpcVariant.Empty);

    private static OpcItemAttributes[] BuildSnapshot(int count, Func<int, OpcVariant> euInfoFactory)
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
                EUInfo: euInfoFactory(i));
        }
        return snapshot;
    }

    private static unsafe class Helpers
    {
        internal readonly record struct EnumNextResult(int Hr, uint Fetched, string?[] ItemIds, OpcVariant[] EUInfos);

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
            (string?[] itemIds, OpcVariant[] euInfos) = ReadItemsAndFree(ppItems, (int)fetched);
            return new EnumNextResult(hr, fetched, itemIds, euInfos);
        }

        private static (string?[] ItemIds, OpcVariant[] EUInfos) ReadItemsAndFree(IntPtr ptr, int count)
        {
            var itemIds = new string?[count];
            var euInfos = new OpcVariant[count];
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                IntPtr slot = IntPtr.Add(ptr, i * s_opcItemAttributesSize);
                itemIds[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(slot, s_szItemIdOffset));
                euInfos[i] = ComVariantMarshaler.ReadVariant(IntPtr.Add(slot, s_vEUInfoOffset));
            }
            if (ptr != IntPtr.Zero)
            {
                FreeNativeAttributes(ptr, count);
            }
            Marshal.FreeCoTaskMem(ptr);
            return (itemIds, euInfos);
        }

        private static void FreeNativeAttributes(IntPtr ptr, int count)
        {
            for (int i = 0; i < count; i++)
            {
                IntPtr slot = IntPtr.Add(ptr, i * s_opcItemAttributesSize);
                Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, SzAccessPathOffset));
                Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, s_szItemIdOffset));
                Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(slot, s_pBlobOffset));
            }
            for (int i = 0; i < count; i++)
            {
                ComVariantMarshaler.ClearVariant(IntPtr.Add(ptr, (i * s_opcItemAttributesSize) + s_vEUInfoOffset));
            }
        }
    }
}
