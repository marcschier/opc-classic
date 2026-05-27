//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

/// <summary>
/// Windows-only smoke tests for <see cref="OpcHdaServerCcw"/> — the HDA
/// parity to OpcDaServerCcw, providing IUnknown identity for SCM activation.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcHdaServerCcwTests
{
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task Create_returns_zero_for_unsupported_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), Guid.NewGuid());

        await Assert.That(ccw).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IID_IUnknown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcHdaServerCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IOPCHDA_Server_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IOPCHDA_Server.InterfaceId);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task SupportsInterface_returns_true_for_known_iids()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(OpcHdaServerCcw.SupportsInterface(IID_IUnknown)).IsTrue();
        await Assert.That(OpcHdaServerCcw.SupportsInterface(IOPCHDA_Server.InterfaceId)).IsTrue();
        await Assert.That(OpcHdaServerCcw.SupportsInterface(Guid.NewGuid())).IsFalse();
    }

    private sealed class StubHdaServer : IOpcHdaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds?.Length ?? 0]);

        public Task<Opc.Classic.Hda.OpcHdaItem[]> ReadRawAsync(
            int[] serverHandles,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int numValues,
            bool returnBounds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Array.Empty<Opc.Classic.Hda.OpcHdaItem>());

        public Task<Opc.Classic.Hda.OpcHdaItem[]> ReadProcessedAsync(
            int[] serverHandles,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            long resampleInterval,
            int[] aggregates,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Array.Empty<Opc.Classic.Hda.OpcHdaItem>());
    }
}
