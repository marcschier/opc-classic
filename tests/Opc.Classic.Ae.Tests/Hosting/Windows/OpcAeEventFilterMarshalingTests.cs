//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcAeEventFilterMarshalingTests {
    private const int S_OK = 0;

    [Test]
    public async Task SetFilter_then_GetFilter_roundtrips_full_BSTR_payload() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        var subscription = new RecordingSubscription();
        IntPtr ccw = OpcAeSubscriptionCcw.Create(subscription, IOPCEventSubscriptionMgt.InterfaceId);

        int setHr = FilterHelpers.InvokeSetFilter(
            ccw,
            eventType: (int)(EventType.Simple | EventType.Tracking | EventType.Condition),
            categories: [1001, 1002, 1003],
            lowSeverity: 125,
            highSeverity: 875,
            areas: ["Plant1.AreaA", "Plant1.AreaB"],
            sources: ["Plant1.AreaA.Tank7", "Plant1.AreaB.Pump1"]);
        FilterHelpers.FilterResult result = FilterHelpers.InvokeGetFilter(ccw);

        await Assert.That(setHr).IsEqualTo(S_OK);
        await Assert.That(subscription.EventType).IsEqualTo((int)EventType.All);
        await Assert.That(subscription.Categories).IsEquivalentTo([1001, 1002, 1003]);
        await Assert.That(subscription.LowSeverity).IsEqualTo(125);
        await Assert.That(subscription.HighSeverity).IsEqualTo(875);
        await Assert.That(subscription.Areas).IsEquivalentTo(["Plant1.AreaA", "Plant1.AreaB"]);
        await Assert.That(subscription.Sources).IsEquivalentTo(["Plant1.AreaA.Tank7", "Plant1.AreaB.Pump1"]);
        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.EventType).IsEqualTo((int)EventType.All);
        await Assert.That(result.Categories).IsEquivalentTo([1001, 1002, 1003]);
        await Assert.That(result.LowSeverity).IsEqualTo(125);
        await Assert.That(result.HighSeverity).IsEqualTo(875);
        await Assert.That(result.Areas).IsEquivalentTo(["Plant1.AreaA", "Plant1.AreaB"]);
        await Assert.That(result.Sources).IsEquivalentTo(["Plant1.AreaA.Tank7", "Plant1.AreaB.Pump1"]);
        await Assert.That(result.ObservedBstrCount).IsEqualTo(4);
    }

    private sealed class RecordingSubscription : IOPCEventSubscriptionMgt {
        public int EventType { get; private set; }
        public int[] Categories { get; private set; } = [];
        public int LowSeverity { get; private set; }
        public int HighSeverity { get; private set; }
        public string[] Areas { get; private set; } = [];
        public string[] Sources { get; private set; } = [];

        public Task SetFilterAsync(int eventType, int[] eventCategories, int lowSeverity, int highSeverity, string[] areas, string[] sources, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            EventType = eventType;
            Categories = eventCategories;
            LowSeverity = lowSeverity;
            HighSeverity = highSeverity;
            Areas = areas;
            Sources = sources;
            return Task.CompletedTask;
        }

        public Task GetFilterAsync(out int eventType, out int[] eventCategories, out int lowSeverity, out int highSeverity, out string[] areas, out string[] sources, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            eventType = EventType;
            eventCategories = Categories;
            lowSeverity = LowSeverity;
            highSeverity = HighSeverity;
            areas = Areas;
            sources = Sources;
            return Task.CompletedTask;
        }

        public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<int>());
        public Task RefreshAsync(int connection, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken cancellationToken = default) {
            active = true;
            bufferTime = 0;
            maxSize = 0;
            clientSubscription = 0;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(bool active, int bufferTime, int maxSize, int clientSubscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken cancellationToken = default) {
            _ = active;
            revisedBufferTime = bufferTime;
            revisedMaxSize = maxSize;
            return Task.CompletedTask;
        }
    }

    private static class FilterHelpers {
        internal readonly record struct FilterResult(int Hr, int EventType, int[] Categories, int LowSeverity, int HighSeverity, string[] Areas, string[] Sources, int ObservedBstrCount);

        internal static int InvokeSetFilter(IntPtr subscription, int eventType, int[] categories, int lowSeverity, int highSeverity, string[] areas, string[] sources) {
            SetFilterDelegate setFilter = GetMethod<SetFilterDelegate>(subscription, 3);
            IntPtr categoriesPtr = AllocateInt32Array(categories);
            IntPtr areasPtr = AllocateBstrPointerArray(areas);
            IntPtr sourcesPtr = AllocateBstrPointerArray(sources);
            try {
                return setFilter(subscription, eventType, categories.Length, categoriesPtr, lowSeverity, highSeverity, areas.Length, areasPtr, sources.Length, sourcesPtr);
            }
            finally {
                FreeCoTaskMem(categoriesPtr);
                FreeBstrPointerArray(areasPtr, areas.Length);
                FreeBstrPointerArray(sourcesPtr, sources.Length);
            }
        }

        internal static FilterResult InvokeGetFilter(IntPtr subscription) {
            GetFilterDelegate getFilter = GetMethod<GetFilterDelegate>(subscription, 4);
            IntPtr pEventType = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pCategoryCount = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pLowSeverity = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pHighSeverity = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pAreaCount = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pSourceCount = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr categoriesPtr = IntPtr.Zero;
            IntPtr areasPtr = IntPtr.Zero;
            IntPtr sourcesPtr = IntPtr.Zero;
            try {
                int hr = getFilter(subscription, pEventType, pCategoryCount, out categoriesPtr, pLowSeverity, pHighSeverity, pAreaCount, out areasPtr, pSourceCount, out sourcesPtr);
                int categoryCount = Marshal.ReadInt32(pCategoryCount);
                int areaCount = Marshal.ReadInt32(pAreaCount);
                int sourceCount = Marshal.ReadInt32(pSourceCount);
                string[] areas = ReadBstrPointerArray(areasPtr, areaCount);
                string[] sources = ReadBstrPointerArray(sourcesPtr, sourceCount);
                return new FilterResult(
                    hr,
                    Marshal.ReadInt32(pEventType),
                    ReadInt32Array(categoriesPtr, categoryCount),
                    Marshal.ReadInt32(pLowSeverity),
                    Marshal.ReadInt32(pHighSeverity),
                    areas,
                    sources,
                    CountBstrPointers(areasPtr) + CountBstrPointers(sourcesPtr));
            }
            finally {
                Marshal.FreeCoTaskMem(pEventType);
                Marshal.FreeCoTaskMem(pCategoryCount);
                Marshal.FreeCoTaskMem(pLowSeverity);
                Marshal.FreeCoTaskMem(pHighSeverity);
                Marshal.FreeCoTaskMem(pAreaCount);
                Marshal.FreeCoTaskMem(pSourceCount);
                FreeCoTaskMem(categoriesPtr);
                FreeBstrPointerArray(areasPtr, CountBstrPointers(areasPtr));
                FreeBstrPointerArray(sourcesPtr, CountBstrPointers(sourcesPtr));
            }
        }

        private static IntPtr AllocateInt32Array(int[] values) {
            if (values.Length == 0) {
                return IntPtr.Zero;
            }
            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
            Marshal.Copy(values, 0, ptr, values.Length);
            return ptr;
        }

        private static IntPtr AllocateBstrPointerArray(string[] values) {
            if (values.Length == 0) {
                return IntPtr.Zero;
            }
            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * IntPtr.Size);
            for (int i = 0; i < values.Length; i++) {
                Marshal.WriteIntPtr(ptr, i * IntPtr.Size, Marshal.StringToBSTR(values[i]));
            }
            return ptr;
        }

        private static int[] ReadInt32Array(IntPtr ptr, int count) {
            if (count == 0) {
                return [];
            }
            var values = new int[count];
            Marshal.Copy(ptr, values, 0, count);
            return values;
        }

        private static string[] ReadBstrPointerArray(IntPtr ptr, int count) {
            if (count == 0) {
                return [];
            }
            var values = new string[count];
            for (int i = 0; i < count; i++) {
                values[i] = Marshal.PtrToStringBSTR(Marshal.ReadIntPtr(ptr, i * IntPtr.Size)) ?? string.Empty;
            }
            return values;
        }

        private static int CountBstrPointers(IntPtr ptr) {
            if (ptr == IntPtr.Zero) {
                return 0;
            }
            int count = 0;
            while (Marshal.ReadIntPtr(ptr, count * IntPtr.Size) != IntPtr.Zero) {
                count++;
            }
            return count;
        }

        private static void FreeBstrPointerArray(IntPtr ptr, int count) {
            if (ptr == IntPtr.Zero) {
                return;
            }
            for (int i = 0; i < count; i++) {
                IntPtr bstr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                if (bstr != IntPtr.Zero) {
                    Marshal.FreeBSTR(bstr);
                }
            }
            Marshal.FreeCoTaskMem(ptr);
        }

        private static void FreeCoTaskMem(IntPtr ptr) {
            if (ptr != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        private static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetFilterDelegate(IntPtr pThis, int eventType, int categoryCount, IntPtr eventCategories, int lowSeverity, int highSeverity, int areaCount, IntPtr areas, int sourceCount, IntPtr sources);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetFilterDelegate(IntPtr pThis, IntPtr pEventType, IntPtr pCategoryCount, out IntPtr ppEventCategories, IntPtr pLowSeverity, IntPtr pHighSeverity, IntPtr pAreaCount, out IntPtr ppAreaList, IntPtr pSourceCount, out IntPtr ppSourceList);
    }
}
