// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Hosting;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class DefaultItemDeadbandMgtAdditionalTests
{
    [Test]
    public async Task SetItemDeadband_ReturnsNotSupportedForEveryHandleAndAcceptsNullDeadbands()
    {
        var manager = new DefaultItemDeadbandMgt();

        int[] errors = await manager.SetItemDeadbandAsync([10, 20, 30], null!, TestContext.Current!.CancellationToken);

        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.DeadbandNotSupported.Code,
            OpcResultId.DeadbandNotSupported.Code,
            OpcResultId.DeadbandNotSupported.Code,
        });
    }

    [Test]
    public async Task GetItemDeadband_ReturnsZeroDeadbandsAndNotSetErrors()
    {
        var manager = new DefaultItemDeadbandMgt();

        await manager.GetItemDeadbandAsync(
            [10, 20],
            out float[] percentDeadbands,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(percentDeadbands).IsEquivalentTo(new[] { 0f, 0f });
        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.DeadbandNotSet.Code,
            OpcResultId.DeadbandNotSet.Code,
        });
    }

    [Test]
    public async Task ClearItemDeadband_ReturnsNotSetForEveryHandle()
    {
        var manager = new DefaultItemDeadbandMgt();

        int[] errors = await manager.ClearItemDeadbandAsync([5, 6], TestContext.Current!.CancellationToken);

        await Assert.That(errors).IsEquivalentTo(new[]
        {
            OpcResultId.DeadbandNotSet.Code,
            OpcResultId.DeadbandNotSet.Code,
        });
    }

    [Test]
    public async Task Methods_WithEmptyHandles_ReturnEmptyArrays()
    {
        var manager = new DefaultItemDeadbandMgt();

        int[] setErrors = await manager.SetItemDeadbandAsync([], [], TestContext.Current!.CancellationToken);
        await manager.GetItemDeadbandAsync(
            [],
            out float[] deadbands,
            out int[] getErrors,
            TestContext.Current!.CancellationToken);
        int[] clearErrors = await manager.ClearItemDeadbandAsync([], TestContext.Current!.CancellationToken);

        await Assert.That(setErrors.Length).IsEqualTo(0);
        await Assert.That(deadbands.Length).IsEqualTo(0);
        await Assert.That(getErrors.Length).IsEqualTo(0);
        await Assert.That(clearErrors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task Methods_NullServerHandles_ThrowArgumentNullException()
    {
        var manager = new DefaultItemDeadbandMgt();

        await Assert.That(() => manager.SetItemDeadbandAsync(null!, [], TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(() => manager.GetItemDeadbandAsync(null!, out _, out _, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(() => manager.ClearItemDeadbandAsync(null!, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Methods_CanceledToken_ThrowOperationCanceledException()
    {
        var manager = new DefaultItemDeadbandMgt();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.That(() => manager.SetItemDeadbandAsync([1], [1f], cts.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(() => manager.GetItemDeadbandAsync([1], out _, out _, cts.Token))
            .Throws<OperationCanceledException>();
        await Assert.That(() => manager.ClearItemDeadbandAsync([1], cts.Token))
            .Throws<OperationCanceledException>();
    }
}
