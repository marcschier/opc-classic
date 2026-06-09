//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // Native marshaling tests assert HRESULT constants and pointer-backed values.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Opc.Classic.Hda.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

public sealed class OpcHdaVariantMarshalerAdditionalTests {
    private const int S_OK = 0;
    private const int VariantValueOffset = 8;
    private const ushort VT_BOOL = 11;
    private const ushort VT_BSTR = 8;
    private const ushort VT_R8 = 5;
    private const ushort VT_ARRAY = 0x2000;
    private const ushort VT_I4 = 3;

    [Test]
    public async Task WriteVariant_ScalarValues_UseExpectedNativeVariantLayout() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        int variantSize = GetVariantSize();
        IntPtr variantPtr = Marshal.AllocCoTaskMem(variantSize);
        try {
            WriteVariant(variantPtr, OpcVariant.FromString("History"));
            ushort bstrType = ReadVariantType(variantPtr);
            string? bstrValue = Marshal.PtrToStringBSTR(Marshal.ReadIntPtr(variantPtr, VariantValueOffset));
            ClearVariant(variantPtr);

            WriteVariant(variantPtr, OpcVariant.FromDouble(12.5d));
            ushort doubleType = ReadVariantType(variantPtr);
            double doubleValue = BitConverter.Int64BitsToDouble(Marshal.ReadInt64(variantPtr, VariantValueOffset));
            ClearVariant(variantPtr);

            WriteVariant(variantPtr, OpcVariant.FromBoolean(true));
            ushort boolType = ReadVariantType(variantPtr);
            short boolValue = Marshal.ReadInt16(variantPtr, VariantValueOffset);

            await Assert.That(variantSize).IsEqualTo(IntPtr.Size == 8 ? 24 : 16);
            await Assert.That(bstrType).IsEqualTo(VT_BSTR);
            await Assert.That(bstrValue).IsEqualTo("History");
            await Assert.That(doubleType).IsEqualTo(VT_R8);
            await Assert.That(doubleValue).IsEqualTo(12.5d);
            await Assert.That(boolType).IsEqualTo(VT_BOOL);
            await Assert.That(boolValue).IsEqualTo(unchecked((short)0xFFFF));
        }
        finally {
            ClearVariant(variantPtr);
            Marshal.FreeCoTaskMem(variantPtr);
        }
    }

    [Test]
    public async Task WriteVariant_SafeArrayInt32_WritesDescriptorBoundsAndData() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        IntPtr variantPtr = Marshal.AllocCoTaskMem(GetVariantSize());
        try {
            WriteVariant(variantPtr, OpcVariant.FromSafeArray(OpcSafeArray.OfInt32([10, 20, 30])));

            ushort variantType = ReadVariantType(variantPtr);
            IntPtr descriptor = Marshal.ReadIntPtr(variantPtr, VariantValueOffset);
            int pvDataOffset = 8 + IntPtr.Size;
            int boundsOffset = pvDataOffset + IntPtr.Size;
            IntPtr data = Marshal.ReadIntPtr(descriptor, pvDataOffset);
            int count = Marshal.ReadInt32(descriptor, boundsOffset);
            int lowerBound = Marshal.ReadInt32(descriptor, boundsOffset + 4);
            int first = Marshal.ReadInt32(data);
            int second = Marshal.ReadInt32(data, sizeof(int));
            int third = Marshal.ReadInt32(data, 2 * sizeof(int));

            await Assert.That(variantType).IsEqualTo((ushort)(VT_ARRAY | VT_I4));
            await Assert.That(descriptor).IsNotEqualTo(IntPtr.Zero);
            await Assert.That(count).IsEqualTo(3);
            await Assert.That(lowerBound).IsEqualTo(0);
            await Assert.That(first).IsEqualTo(10);
            await Assert.That(second).IsEqualTo(20);
            await Assert.That(third).IsEqualTo(30);
        }
        finally {
            ClearVariant(variantPtr);
            Marshal.FreeCoTaskMem(variantPtr);
        }
    }

    [Test]
    public async Task OpcHdaEnumStringCcwMethods_NextSkipResetAndClone_ReturnConcreteStrings() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        IntPtr enumerator = CreateEnumString(["Area", "Unit", "Tag"]);
        IntPtr clone = IntPtr.Zero;
        IntPtr slots = Marshal.AllocCoTaskMem(2 * IntPtr.Size);
        try {
            ClearSlots(slots, 2);
            int nextHr = GetMethod<NextDelegate>(enumerator, 3)(enumerator, 2, slots, out uint fetched);
            string[] firstBatch = ReadAndFreeLpwStrSlots(slots, 2);
            int skipHr = GetMethod<SkipDelegate>(enumerator, 4)(enumerator, 1);
            int resetHr = GetMethod<ResetDelegate>(enumerator, 5)(enumerator);
            int resetSkipHr = GetMethod<SkipDelegate>(enumerator, 4)(enumerator, 1);
            int cloneHr = GetMethod<CloneDelegate>(enumerator, 6)(enumerator, out clone);

            ClearSlots(slots, 2);
            int cloneNextHr = GetMethod<NextDelegate>(clone, 3)(clone, 2, slots, out uint cloneFetched);
            string[] cloneBatch = ReadAndFreeLpwStrSlots(slots, 2);

            await Assert.That(nextHr).IsEqualTo(S_OK);
            await Assert.That(fetched).IsEqualTo(2u);
            await Assert.That(firstBatch).IsEquivalentTo(["Area", "Unit"]);
            await Assert.That(skipHr).IsEqualTo(S_OK);
            await Assert.That(resetHr).IsEqualTo(S_OK);
            await Assert.That(resetSkipHr).IsEqualTo(S_OK);
            await Assert.That(cloneHr).IsEqualTo(S_OK);
            await Assert.That(cloneNextHr).IsEqualTo(S_OK);
            await Assert.That(cloneFetched).IsEqualTo(2u);
            await Assert.That(cloneBatch).IsEquivalentTo(["Unit", "Tag"]);
        }
        finally {
            if (clone != IntPtr.Zero) {
                _ = GetMethod<ReleaseDelegate>(clone, 2)(clone);
            }
            _ = GetMethod<ReleaseDelegate>(enumerator, 2)(enumerator);
            Marshal.FreeCoTaskMem(slots);
        }
    }

    private static Type VariantMarshalerType =>
        typeof(OpcHdaBrowserCcw).Assembly.GetType("Opc.Classic.Hda.Hosting.Windows.OpcHdaVariantMarshaler", throwOnError: true)!;

    private static Type EnumStringCcwType =>
        typeof(OpcHdaBrowserCcw).Assembly.GetType("Opc.Classic.Hda.Hosting.Windows.OpcHdaEnumStringCcw", throwOnError: true)!;

    private static int GetVariantSize() =>
        (int)(VariantMarshalerType.GetProperty("VariantSize", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)
            ?? throw new MissingMemberException(VariantMarshalerType.FullName, "VariantSize"));

    private static void WriteVariant(IntPtr destination, OpcVariant variant) =>
        GetVariantMarshalerMethod("WriteVariant").Invoke(null, [destination, variant]);

    private static void ClearVariant(IntPtr destination) =>
        GetVariantMarshalerMethod("ClearVariant").Invoke(null, [destination]);

    private static ushort ReadVariantType(IntPtr variantPtr) =>
        unchecked((ushort)Marshal.ReadInt16(variantPtr));

    private static MethodInfo GetVariantMarshalerMethod(string name) =>
        VariantMarshalerType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException(VariantMarshalerType.FullName, name);

    private static IntPtr CreateEnumString(IReadOnlyList<string> values) {
        MethodInfo create = EnumStringCcwType.GetMethod(
            "Create",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(IReadOnlyList<string>)],
            modifiers: null)
            ?? throw new MissingMethodException(EnumStringCcwType.FullName, "Create");
        return (IntPtr)create.Invoke(null, [values])!;
    }

    private static T GetMethod<T>(IntPtr instance, int slot)
        where T : Delegate {
        IntPtr vtable = Marshal.ReadIntPtr(instance);
        IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<T>(method);
    }

    private static void ClearSlots(IntPtr slots, int count) {
        for (int i = 0; i < count; i++) {
            Marshal.WriteIntPtr(slots, i * IntPtr.Size, IntPtr.Zero);
        }
    }

    private static string[] ReadAndFreeLpwStrSlots(IntPtr slots, int count) {
        var values = new string[count];
        for (int i = 0; i < count; i++) {
            IntPtr valuePtr = Marshal.ReadIntPtr(slots, i * IntPtr.Size);
            values[i] = valuePtr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(valuePtr) ?? string.Empty;
            if (valuePtr != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(valuePtr);
                Marshal.WriteIntPtr(slots, i * IntPtr.Size, IntPtr.Zero);
            }
        }

        return values;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int NextDelegate(IntPtr pThis, uint celt, IntPtr rgelt, out uint pceltFetched);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int SkipDelegate(IntPtr pThis, uint celt);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int ResetDelegate(IntPtr pThis);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CloneDelegate(IntPtr pThis, out IntPtr ppEnum);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint ReleaseDelegate(IntPtr pThis);
}
