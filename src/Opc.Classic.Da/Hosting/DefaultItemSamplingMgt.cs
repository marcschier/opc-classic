// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default managed implementation of <see cref="IOPCItemSamplingMgt"/> (DA 3.0
/// per-item sampling-rate / buffer management). Returns
/// <c>OPC_E_RATENOTSET</c> / <c>OPC_E_NOBUFFERING</c> for every handle so
/// DA 3.0 clients see a deterministic "feature absent" response.
/// </summary>
public sealed class DefaultItemSamplingMgt : IOPCItemSamplingMgt
{
    public Task SetItemSamplingRateAsync(
        int[] serverHandles,
        int[] requestedSamplingRates,
        out int[] revisedSamplingRates,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(requestedSamplingRates);
        cancellationToken.ThrowIfCancellationRequested();
        revisedSamplingRates = new int[serverHandles.Length];
        errors = FillErrors(serverHandles.Length, OpcResultId.RateNotSet.Code);
        return Task.CompletedTask;
    }

    public Task GetItemSamplingRateAsync(
        int[] serverHandles,
        out int[] samplingRates,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        samplingRates = new int[serverHandles.Length];
        errors = FillErrors(serverHandles.Length, OpcResultId.RateNotSet.Code);
        return Task.CompletedTask;
    }

    public Task<int[]> ClearItemSamplingRateAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(FillErrors(serverHandles.Length, OpcResultId.RateNotSet.Code));
    }

    public Task<int[]> SetItemBufferEnableAsync(int[] serverHandles, bool[] enabled, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        _ = enabled;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(FillErrors(serverHandles.Length, OpcResultId.NoBuffering.Code));
    }

    public Task GetItemBufferEnableAsync(
        int[] serverHandles,
        out bool[] enabled,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        enabled = new bool[serverHandles.Length];
        errors = FillErrors(serverHandles.Length, OpcResultId.NoBuffering.Code);
        return Task.CompletedTask;
    }

    private static int[] FillErrors(int length, int code)
    {
        var errors = new int[length];
        Array.Fill(errors, code);
        return errors;
    }
}
