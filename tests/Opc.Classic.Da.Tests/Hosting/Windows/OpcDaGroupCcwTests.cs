//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>
/// Windows-only unit tests for <see cref="OpcDaGroupCcw"/>. The minimal CCW
/// satisfies the IUnknown contract (QI for IID_IUnknown succeeds; all other
/// IIDs return E_NOINTERFACE). Full per-interface vtables are intentionally
/// deferred to a separate follow-up.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcDaGroupCcwTests
{
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid IID_IOPCGroupStateMgt = Guid.Parse("39c13a50-011e-11d0-9675-0020afd8adb3");

    [Test]
    public async Task Create_returns_nonzero_ccw_pointer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup();

        IntPtr ccw = OpcDaGroupCcw.Create(group);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcDaGroupCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task Create_returns_distinct_ccw_per_call()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup g1 = NewGroup("a");
        OpcDaGroup g2 = NewGroup("b");

        IntPtr ccw1 = OpcDaGroupCcw.Create(g1);
        IntPtr ccw2 = OpcDaGroupCcw.Create(g2);

        await Assert.That(ccw1).IsNotEqualTo(ccw2);
    }

    [Test]
    public async Task GetReferenceCount_returns_negative_one_for_unknown_pointer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(OpcDaGroupCcw.GetReferenceCount(new IntPtr(0x12345678))).IsEqualTo(-1L);
    }

    private static OpcDaGroup NewGroup(string name = "TestGroup") => new(
        name: name,
        serverHandle: 1,
        clientHandle: 100,
        active: true,
        requestedUpdateRate: 1000,
        timeBias: 0,
        percentDeadband: 0f,
        localeId: 1033);
}
