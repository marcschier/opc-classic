// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Native marshaling tests assert HRESULT constants and pointer-backed values.

using System.Reflection;
using System.Runtime.InteropServices;
using Opc.Classic.Ae.Hosting.Windows;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

public sealed class OpcAeArrayMarshalerAdditionalTests
{
    private const int S_OK = 0;

    [Test]
    public async Task AllocateBstrArray_ConcreteStrings_RoundTripsAndAppendsNullSentinel()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        int count = 0;
        IntPtr arrayPtr = IntPtr.Zero;
        object?[] args = [new string?[] { "Area", null, "Source" }, count, arrayPtr];

        try
        {
            GetArrayMarshalerMethod("AllocateBstrArray").Invoke(null, args);
            count = (int)args[1]!;
            arrayPtr = (IntPtr)args[2]!;
            string[] values = (string[])GetArrayMarshalerMethod("ReadBstrArray").Invoke(null, [arrayPtr, count])!;
            IntPtr sentinel = Marshal.ReadIntPtr(arrayPtr, count * IntPtr.Size);

            await Assert.That(count).IsEqualTo(3);
            await Assert.That(values).IsEquivalentTo(["Area", string.Empty, "Source"]);
            await Assert.That(sentinel).IsEqualTo(IntPtr.Zero);
        }
        finally
        {
            GetArrayMarshalerMethod("FreeBstrArray").Invoke(null, [arrayPtr, count]);
        }
    }

    [Test]
    public async Task AllocateDwordAndGuidArrays_ConcreteValues_UseExpectedNativeLayout()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        int dwordCount = 0;
        IntPtr dwordPtr = IntPtr.Zero;
        int guidCount = 0;
        IntPtr guidPtr = IntPtr.Zero;
        Guid first = Guid.Parse("11111111-2222-3333-4444-555555555555");
        Guid second = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        try
        {
            object?[] dwordArgs = [new[] { 42, -1, 700 }, dwordCount, dwordPtr];
            GetArrayMarshalerMethod("AllocateDwordArray").Invoke(null, dwordArgs);
            dwordCount = (int)dwordArgs[1]!;
            dwordPtr = (IntPtr)dwordArgs[2]!;
            int[] dwords = (int[])GetArrayMarshalerMethod("ReadDwordArray").Invoke(null, [dwordPtr, dwordCount])!;

            object?[] guidArgs = [new[] { first, second }, guidCount, guidPtr];
            GetArrayMarshalerMethod("AllocateGuidArray").Invoke(null, guidArgs);
            guidCount = (int)guidArgs[1]!;
            guidPtr = (IntPtr)guidArgs[2]!;
            Guid firstNative = Marshal.PtrToStructure<Guid>(guidPtr);
            Guid secondNative = Marshal.PtrToStructure<Guid>(IntPtr.Add(guidPtr, Marshal.SizeOf<Guid>()));

            await Assert.That(dwordCount).IsEqualTo(3);
            await Assert.That(dwords).IsEquivalentTo([42, -1, 700]);
            await Assert.That(guidCount).IsEqualTo(2);
            await Assert.That(firstNative).IsEqualTo(first);
            await Assert.That(secondNative).IsEqualTo(second);
        }
        finally
        {
            GetArrayMarshalerMethod("FreeCoTaskMem").Invoke(null, [dwordPtr]);
            GetArrayMarshalerMethod("FreeCoTaskMem").Invoke(null, [guidPtr]);
        }
    }

    [Test]
    public async Task OpcEnumStringCcw_NextSkipResetAndClone_ReturnConcreteEnumeratorState()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr enumerator = CreateEnumString(["One", "Two", "Three"]);
        IntPtr clone = IntPtr.Zero;
        IntPtr slots = Marshal.AllocCoTaskMem(2 * IntPtr.Size);
        try
        {
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
            await Assert.That(firstBatch).IsEquivalentTo(["One", "Two"]);
            await Assert.That(skipHr).IsEqualTo(S_OK);
            await Assert.That(resetHr).IsEqualTo(S_OK);
            await Assert.That(resetSkipHr).IsEqualTo(S_OK);
            await Assert.That(cloneHr).IsEqualTo(S_OK);
            await Assert.That(cloneNextHr).IsEqualTo(S_OK);
            await Assert.That(cloneFetched).IsEqualTo(2u);
            await Assert.That(cloneBatch).IsEquivalentTo(["Two", "Three"]);
        }
        finally
        {
            if (clone != IntPtr.Zero)
            {
                _ = GetMethod<ReleaseDelegate>(clone, 2)(clone);
            }
            _ = GetMethod<ReleaseDelegate>(enumerator, 2)(enumerator);
            Marshal.FreeCoTaskMem(slots);
        }
    }

    private static MethodInfo GetArrayMarshalerMethod(string name) =>
        ArrayMarshalerType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException(ArrayMarshalerType.FullName, name);

    private static Type ArrayMarshalerType =>
        typeof(OpcAeServerCcw).Assembly.GetType("Opc.Classic.Ae.Hosting.Windows.OpcAeArrayMarshaler", throwOnError: true)!;

    private static Type EnumStringCcwType =>
        typeof(OpcAeServerCcw).Assembly.GetType("Opc.Classic.Ae.Hosting.Windows.OpcEnumStringCcw", throwOnError: true)!;

    private static IntPtr CreateEnumString(IReadOnlyList<string> values)
    {
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
        where T : Delegate
    {
        IntPtr vtable = Marshal.ReadIntPtr(instance);
        IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<T>(method);
    }

    private static void ClearSlots(IntPtr slots, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Marshal.WriteIntPtr(slots, i * IntPtr.Size, IntPtr.Zero);
        }
    }

    private static string[] ReadAndFreeLpwStrSlots(IntPtr slots, int count)
    {
        var values = new string[count];
        for (int i = 0; i < count; i++)
        {
            IntPtr valuePtr = Marshal.ReadIntPtr(slots, i * IntPtr.Size);
            values[i] = valuePtr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(valuePtr) ?? string.Empty;
            if (valuePtr != IntPtr.Zero)
            {
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
