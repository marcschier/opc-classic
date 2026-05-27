//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

/// <summary>
/// Windows-only smoke tests for <see cref="OpcAeServerCcw"/> — the AE
/// parity to OpcDaServerCcw, providing IUnknown identity for SCM activation.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcAeServerCcwTests
{
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task Create_returns_zero_for_unsupported_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcAeServerCcw.Create(new StubAeServer(), Guid.NewGuid());

        await Assert.That(ccw).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IID_IUnknown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcAeServerCcw.Create(new StubAeServer(), IID_IUnknown);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcAeServerCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IOPCEventServer_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcAeServerCcw.Create(new StubAeServer(), IOPCEventServer.InterfaceId);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task SupportsInterface_returns_true_for_known_iids()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(OpcAeServerCcw.SupportsInterface(IID_IUnknown)).IsTrue();
        await Assert.That(OpcAeServerCcw.SupportsInterface(IOPCEventServer.InterfaceId)).IsTrue();
        await Assert.That(OpcAeServerCcw.SupportsInterface(Guid.NewGuid())).IsFalse();
    }

    private sealed class StubAeServer : IOpcAeServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Ae });

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }
}
