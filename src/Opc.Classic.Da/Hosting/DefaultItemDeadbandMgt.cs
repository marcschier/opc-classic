// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default managed implementation of <see cref="IOPCItemDeadbandMgt"/> (DA 3.0
/// per-item deadband). Returns <c>OPC_E_DEADBANDNOTSUPPORTED</c> for every
/// handle so DA 3.0 clients see a deterministic "feature absent" response.
/// </summary>
public sealed class DefaultItemDeadbandMgt : IOPCItemDeadbandMgt
{
    public Task<int[]> SetItemDeadbandAsync(int[] serverHandles, float[] percentDeadbands, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        _ = percentDeadbands;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(FillErrors(serverHandles.Length, OpcResultId.DeadbandNotSupported.Code));
    }

    public Task GetItemDeadbandAsync(
        int[] serverHandles,
        out float[] percentDeadbands,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        percentDeadbands = new float[serverHandles.Length];
        errors = FillErrors(serverHandles.Length, OpcResultId.DeadbandNotSet.Code);
        return Task.CompletedTask;
    }

    public Task<int[]> ClearItemDeadbandAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(FillErrors(serverHandles.Length, OpcResultId.DeadbandNotSet.Code));
    }

    private static int[] FillErrors(int length, int code)
    {
        var errors = new int[length];
        Array.Fill(errors, code);
        return errors;
    }
}
