//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // CCW tests assert HRESULT constants and raw vtable outputs.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

/// <summary>Windows-only raw-vtable tests for <see cref="OpcHdaBrowserCcw" />.</summary>
[SupportedOSPlatform("windows")]
public sealed class OpcHdaBrowserCcwTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_INVALIDARG = unchecked((int)0x80070057);

    [Test]
    public async Task GetEnum_returns_IEnumString_over_dispatcher_results()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new TestHdaDispatcher { BrowseValues = ["Area", "Unit"] };
        IntPtr browser = OpcHdaBrowserCcw.Create(dispatcher, []);
        try
        {
            GetEnumDelegate getEnum = GetMethod<GetEnumDelegate>(browser, 3);
            int hr = getEnum(browser, (uint)HdaBrowseType.Branch, out IntPtr enumString);
            string[] values = ReadEnumStrings(enumString, 2, out int nextHr);

            await Assert.That(hr).IsEqualTo(S_OK);
            await Assert.That(nextHr).IsEqualTo(S_OK);
            await Assert.That(values).IsEquivalentTo(["Area", "Unit"]);
            await Assert.That(dispatcher.LastBrowseType).IsEqualTo(HdaBrowseType.Branch);
            await Assert.That(dispatcher.LastBrowsePosition).IsEqualTo(string.Empty);
        }
        finally
        {
            InvokeRelease(browser);
        }
    }

    [Test]
    public async Task ChangeBrowsePosition_updates_branch_via_dispatcher()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new TestHdaDispatcher();
        IntPtr browser = OpcHdaBrowserCcw.Create(dispatcher, []);
        IntPtr branch = Marshal.StringToCoTaskMemUni("Plant");
        try
        {
            ChangeBrowsePositionDelegate change = GetMethod<ChangeBrowsePositionDelegate>(browser, 4);
            GetBranchPositionDelegate getBranch = GetMethod<GetBranchPositionDelegate>(browser, 6);

            int changeHr = change(browser, 2, branch);
            int branchHr = getBranch(browser, out IntPtr branchPosition);
            string? branchText = ReadAndFreeBstr(branchPosition);

            await Assert.That(changeHr).IsEqualTo(S_OK);
            await Assert.That(branchHr).IsEqualTo(S_OK);
            await Assert.That(branchText).IsEqualTo("Plant");
            await Assert.That(dispatcher.LastBrowseDirection).IsEqualTo(2);
            await Assert.That(dispatcher.LastBrowseString).IsEqualTo("Plant");
        }
        finally
        {
            Marshal.FreeCoTaskMem(branch);
            InvokeRelease(browser);
        }
    }

    [Test]
    public async Task GetItemID_resolves_item_id_at_current_branch()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new TestHdaDispatcher();
        IntPtr browser = OpcHdaBrowserCcw.Create(dispatcher, []);
        IntPtr branch = Marshal.StringToCoTaskMemUni("Plant");
        IntPtr node = Marshal.StringToCoTaskMemUni("Temperature");
        try
        {
            ChangeBrowsePositionDelegate change = GetMethod<ChangeBrowsePositionDelegate>(browser, 4);
            GetItemIDDelegate getItemId = GetMethod<GetItemIDDelegate>(browser, 5);

            int changeHr = change(browser, 3, branch);
            int itemHr = getItemId(browser, node, out IntPtr itemId);
            string? itemText = ReadAndFreeBstr(itemId);

            await Assert.That(changeHr).IsEqualTo(S_OK);
            await Assert.That(itemHr).IsEqualTo(S_OK);
            await Assert.That(itemText).IsEqualTo("Plant.Temperature");
            await Assert.That(dispatcher.LastItemIdBranch).IsEqualTo("Plant");
            await Assert.That(dispatcher.LastItemIdNode).IsEqualTo("Temperature");
        }
        finally
        {
            Marshal.FreeCoTaskMem(branch);
            Marshal.FreeCoTaskMem(node);
            InvokeRelease(browser);
        }
    }

    [Test]
    public async Task GetBranchPosition_returns_dispatcher_branch_string()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new TestHdaDispatcher();
        IntPtr browser = OpcHdaBrowserCcw.Create(dispatcher, []);
        IntPtr branch = Marshal.StringToCoTaskMemUni("Plant.Area");
        try
        {
            ChangeBrowsePositionDelegate change = GetMethod<ChangeBrowsePositionDelegate>(browser, 4);
            GetBranchPositionDelegate getBranch = GetMethod<GetBranchPositionDelegate>(browser, 6);

            int changeHr = change(browser, 3, branch);
            int branchHr = getBranch(browser, out IntPtr branchPosition);
            string? branchText = ReadAndFreeBstr(branchPosition);

            await Assert.That(changeHr).IsEqualTo(S_OK);
            await Assert.That(branchHr).IsEqualTo(S_OK);
            await Assert.That(branchText).IsEqualTo("Plant.Area");
            await Assert.That(dispatcher.LastBranchPosition).IsEqualTo("Plant.Area");
        }
        finally
        {
            Marshal.FreeCoTaskMem(branch);
            InvokeRelease(browser);
        }
    }

    [Test]
    public async Task GetEnum_rejects_invalid_browse_type_without_dispatching()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new TestHdaDispatcher { BrowseValues = ["Ignored"] };
        IntPtr browser = OpcHdaBrowserCcw.Create(dispatcher, []);
        try
        {
            GetEnumDelegate getEnum = GetMethod<GetEnumDelegate>(browser, 3);
            int hr = getEnum(browser, 99, out IntPtr enumString);

            await Assert.That(hr).IsEqualTo(E_INVALIDARG);
            await Assert.That(enumString).IsEqualTo(IntPtr.Zero);
            await Assert.That(dispatcher.BrowseCalls).IsEqualTo(0);
        }
        finally
        {
            InvokeRelease(browser);
        }
    }

    [Test]
    public async Task CreateBrowse_rejects_mismatched_filter_arrays()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr server = OpcHdaServerCcw.Create(new StubHdaServer(), IOPCHDA_Server.InterfaceId);
        try
        {
            CreateBrowseDelegate createBrowse = GetMethod<CreateBrowseDelegate>(server, 9);
            int hr = createBrowse(server, 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out IntPtr browser, out IntPtr errors);

            await Assert.That(hr).IsEqualTo(E_INVALIDARG);
            await Assert.That(browser).IsEqualTo(IntPtr.Zero);
            await Assert.That(errors).IsEqualTo(IntPtr.Zero);
        }
        finally
        {
            InvokeRelease(server);
        }
    }

    [Test]
    public async Task CreateBrowse_returns_browser_and_per_filter_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr server = OpcHdaServerCcw.Create(new StubHdaServer(), IOPCHDA_Server.InterfaceId);
        IntPtr attributes = AllocateInt32Array([1, 0]);
        IntPtr operators = AllocateInt32Array([1, 1]);
        IntPtr variants = AllocateEmptyVariants(2);
        try
        {
            CreateBrowseDelegate createBrowse = GetMethod<CreateBrowseDelegate>(server, 9);
            int hr = createBrowse(server, 2, attributes, operators, variants, out IntPtr browser, out IntPtr errors);
            int[] errorValues = ReadAndFreeInt32Array(errors, 2);

            await Assert.That(hr).IsEqualTo(S_FALSE);
            await Assert.That(browser).IsNotEqualTo(IntPtr.Zero);
            await Assert.That(errorValues[0]).IsEqualTo(S_OK);
            await Assert.That(errorValues[1]).IsEqualTo(OpcHdaErrors.OPCHDA_E_INVALIDATTRID);
            InvokeRelease(browser);
        }
        finally
        {
            Marshal.FreeCoTaskMem(attributes);
            Marshal.FreeCoTaskMem(operators);
            Marshal.FreeCoTaskMem(variants);
            InvokeRelease(server);
        }
    }

    private static string[] ReadEnumStrings(IntPtr enumString, int count, out int hr)
    {
        EnumNextDelegate next = GetMethod<EnumNextDelegate>(enumString, 3);
        IntPtr array = Marshal.AllocCoTaskMem(count * IntPtr.Size);
        try
        {
            for (int i = 0; i < count; i++)
            {
                Marshal.WriteIntPtr(array, i * IntPtr.Size, IntPtr.Zero);
            }

            hr = next(enumString, (uint)count, array, out uint fetched);
            var values = new string[fetched];
            for (int i = 0; i < fetched; i++)
            {
                IntPtr valuePtr = Marshal.ReadIntPtr(array, i * IntPtr.Size);
                values[i] = Marshal.PtrToStringUni(valuePtr) ?? string.Empty;
                Marshal.FreeCoTaskMem(valuePtr);
            }

            return values;
        }
        finally
        {
            Marshal.FreeCoTaskMem(array);
            InvokeRelease(enumString);
        }
    }

    private static string? ReadAndFreeBstr(IntPtr value)
    {
        try
        {
            return Marshal.PtrToStringBSTR(value);
        }
        finally
        {
            if (value != IntPtr.Zero)
            {
                Marshal.FreeBSTR(value);
            }
        }
    }

    private static T GetMethod<T>(IntPtr tearoff, int slot)
        where T : Delegate
    {
        IntPtr vtable = Marshal.ReadIntPtr(tearoff);
        IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<T>(method);
    }

    private static void InvokeRelease(IntPtr ccw)
    {
        if (ccw == IntPtr.Zero)
        {
            return;
        }

        ReleaseDelegate release = GetMethod<ReleaseDelegate>(ccw, 2);
        release(ccw);
    }

    private static IntPtr AllocateInt32Array(int[] values)
    {
        IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
        if (values.Length > 0)
        {
            Marshal.Copy(values, 0, ptr, values.Length);
        }

        return ptr;
    }

    private static IntPtr AllocateEmptyVariants(int count)
    {
        int size = IntPtr.Size == 8 ? 24 : 16;
        IntPtr ptr = Marshal.AllocCoTaskMem(count * size);
        var zero = new byte[count * size];
        Marshal.Copy(zero, 0, ptr, zero.Length);
        return ptr;
    }

    private static int[] ReadAndFreeInt32Array(IntPtr ptr, int count)
    {
        var values = new int[count];
        if (ptr != IntPtr.Zero && count > 0)
        {
            Marshal.Copy(ptr, values, 0, count);
            Marshal.FreeCoTaskMem(ptr);
        }

        return values;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int GetEnumDelegate(IntPtr pThis, uint dwBrowseType, out IntPtr ppIEnumString);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int ChangeBrowsePositionDelegate(IntPtr pThis, uint dwBrowseDirection, IntPtr szString);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int GetItemIDDelegate(IntPtr pThis, IntPtr szNode, out IntPtr pszItemID);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int GetBranchPositionDelegate(IntPtr pThis, out IntPtr pszBranchPos);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int EnumNextDelegate(IntPtr pThis, uint celt, IntPtr rgelt, out uint pceltFetched);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate uint ReleaseDelegate(IntPtr pThis);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CreateBrowseDelegate(
        IntPtr pThis,
        uint dwCount,
        IntPtr pdwAttrID,
        IntPtr pOperator,
        IntPtr vFilter,
        out IntPtr pphBrowser,
        out IntPtr ppErrors);

    private sealed class TestHdaDispatcher : IOpcHdaServerDispatcher
    {
        public IReadOnlyList<string> BrowseValues { get; init; } = [];

        public int BrowseCalls { get; private set; }

        public HdaBrowseType LastBrowseType { get; private set; }

        public string LastBrowsePosition { get; private set; } = string.Empty;

        public int LastBrowseDirection { get; private set; }

        public string? LastBrowseString { get; private set; }

        public string LastItemIdBranch { get; private set; } = string.Empty;

        public string LastItemIdNode { get; private set; } = string.Empty;

        public string LastBranchPosition { get; private set; } = string.Empty;

        public Task<NdrCallResult> DispatchAsync(
            Guid interfaceId,
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken)
        {
            _ = interfaceId;
            _ = opnum;
            _ = requestPayload;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        public Task<IReadOnlyList<string>> BrowseAsync(
            string branchPosition,
            HdaBrowseType browseType,
            IReadOnlyList<OpcHdaBrowseFilter> filters,
            CancellationToken cancellationToken = default)
        {
            _ = filters;
            cancellationToken.ThrowIfCancellationRequested();
            BrowseCalls++;
            LastBrowsePosition = branchPosition;
            LastBrowseType = browseType;
            return Task.FromResult(BrowseValues);
        }

        public Task<string> ChangeBrowsePositionAsync(
            string currentBranchPosition,
            int browseDirection,
            string? browseString,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastBrowseDirection = browseDirection;
            LastBrowseString = browseString;
            return Task.FromResult(browseDirection switch
            {
                1 => MoveUp(currentBranchPosition),
                2 => string.IsNullOrEmpty(currentBranchPosition) ? browseString ?? string.Empty : currentBranchPosition + "." + browseString,
                3 => browseString ?? string.Empty,
                _ => throw new OpcException(OpcResultId.InvalidArg),
            });
        }

        public Task<string> GetItemIdAsync(string branchPosition, string node, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastItemIdBranch = branchPosition;
            LastItemIdNode = node;
            return Task.FromResult(string.IsNullOrEmpty(branchPosition) ? node : branchPosition + "." + node);
        }

        public Task<string> GetBranchPositionAsync(string branchPosition, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastBranchPosition = branchPosition;
            return Task.FromResult(branchPosition);
        }

        private static string MoveUp(string position)
        {
            int lastDot = position.LastIndexOf('.');
            return lastDot < 0 ? string.Empty : position[..lastDot];
        }
    }

    private sealed class StubHdaServer : IOpcHdaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds.Length]);
    }
}
