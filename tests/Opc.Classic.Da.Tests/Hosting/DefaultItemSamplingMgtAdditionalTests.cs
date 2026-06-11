//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using Opc.Classic;
using Opc.Classic.Da.Hosting;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class DefaultItemSamplingMgtAdditionalTests
{
    [Test]
    public async Task SetItemSamplingRate_ReturnsZeroRevisedRatesAndRateNotSetErrors()
    {
        var manager = new DefaultItemSamplingMgt();

        await manager.SetItemSamplingRateAsync(
            [1, 2, 3],
            [100, 200, 300],
            out int[] revisedSamplingRates,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(revisedSamplingRates).IsEquivalentTo(new[] { 0, 0, 0 });
        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.RateNotSet.Code,
            OpcResultId.RateNotSet.Code,
            OpcResultId.RateNotSet.Code,
        });
    }

    [Test]
    public async Task GetItemSamplingRate_ReturnsZeroRatesAndRateNotSetErrors()
    {
        var manager = new DefaultItemSamplingMgt();

        await manager.GetItemSamplingRateAsync(
            [11, 12],
            out int[] samplingRates,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(samplingRates).IsEquivalentTo(new[] { 0, 0 });
        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.RateNotSet.Code,
            OpcResultId.RateNotSet.Code,
        });
    }

    [Test]
    public async Task ClearItemSamplingRate_ReturnsRateNotSetForEveryHandle()
    {
        var manager = new DefaultItemSamplingMgt();

        int[] errors = await manager.ClearItemSamplingRateAsync([21, 22], TestContext.Current!.CancellationToken);

        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.RateNotSet.Code,
            OpcResultId.RateNotSet.Code,
        });
    }

    [Test]
    public async Task SetItemBufferEnable_ReturnsNoBufferingForEveryHandleAndAcceptsNullEnabled()
    {
        var manager = new DefaultItemSamplingMgt();

        int[] errors = await manager.SetItemBufferEnableAsync([31, 32], null!, TestContext.Current!.CancellationToken);

        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.NoBuffering.Code,
            OpcResultId.NoBuffering.Code,
        });
    }

    [Test]
    public async Task GetItemBufferEnable_ReturnsFalseEnabledFlagsAndNoBufferingErrors()
    {
        var manager = new DefaultItemSamplingMgt();

        await manager.GetItemBufferEnableAsync(
            [41, 42, 43],
            out bool[] enabled,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(enabled).IsEquivalentTo(new[] { false, false, false });
        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.NoBuffering.Code,
            OpcResultId.NoBuffering.Code,
            OpcResultId.NoBuffering.Code,
        });
    }

    [Test]
    public async Task Methods_WithEmptyHandles_ReturnEmptyArrays()
    {
        var manager = new DefaultItemSamplingMgt();

        await manager.SetItemSamplingRateAsync([], [], out int[] revised, out int[] setErrors, TestContext.Current!.CancellationToken);
        await manager.GetItemSamplingRateAsync([], out int[] rates, out int[] getErrors, TestContext.Current!.CancellationToken);
        int[] clearErrors = await manager.ClearItemSamplingRateAsync([], TestContext.Current!.CancellationToken);
        int[] bufferSetErrors = await manager.SetItemBufferEnableAsync([], [], TestContext.Current!.CancellationToken);
        await manager.GetItemBufferEnableAsync([], out bool[] enabled, out int[] bufferGetErrors, TestContext.Current!.CancellationToken);

        await Assert.That(revised.Length).IsEqualTo(0);
        await Assert.That(setErrors.Length).IsEqualTo(0);
        await Assert.That(rates.Length).IsEqualTo(0);
        await Assert.That(getErrors.Length).IsEqualTo(0);
        await Assert.That(clearErrors.Length).IsEqualTo(0);
        await Assert.That(bufferSetErrors.Length).IsEqualTo(0);
        await Assert.That(enabled.Length).IsEqualTo(0);
        await Assert.That(bufferGetErrors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task Methods_NullRequiredArrays_ThrowArgumentNullException()
    {
        var manager = new DefaultItemSamplingMgt();

        await Assert.That(() => manager.SetItemSamplingRateAsync(null!, [], out _, out _, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(() => manager.SetItemSamplingRateAsync([1], null!, out _, out _, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(() => manager.GetItemSamplingRateAsync(null!, out _, out _, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(() => manager.ClearItemSamplingRateAsync(null!, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(() => manager.SetItemBufferEnableAsync(null!, [], TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(() => manager.GetItemBufferEnableAsync(null!, out _, out _, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Methods_CanceledToken_ThrowOperationCanceledException()
    {
        var manager = new DefaultItemSamplingMgt();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.That(() => manager.SetItemSamplingRateAsync([1], [100], out _, out _, cts.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(() => manager.GetItemSamplingRateAsync([1], out _, out _, cts.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(() => manager.ClearItemSamplingRateAsync([1], cts.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(() => manager.SetItemBufferEnableAsync([1], [true], cts.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(() => manager.GetItemBufferEnableAsync([1], out _, out _, cts.Token))
            .Throws<OperationCanceledException>();
    }
}
