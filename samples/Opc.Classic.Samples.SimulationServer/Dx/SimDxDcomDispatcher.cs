// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dx.Dcom;

namespace Opc.Classic.Samples.SimulationServer.Dx;

internal sealed class SimDxDcomDispatcher
{
    private readonly IOPCConfigurationServerDispatcher _dispatcher;

    public SimDxDcomDispatcher(IOPCConfiguration server)
    {
        ArgumentNullException.ThrowIfNull(server);
        _dispatcher = new IOPCConfigurationServerDispatcher(server);
    }

    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCConfiguration.InterfaceId)
        {
            return new NdrCallResult(
                OpcResultId.NotImplemented.Code,
                ReadOnlyMemory<byte>.Empty);
        }

        global::Opc.Classic.Hosting.DispatchResult result =
            await _dispatcher.DispatchAsync(
            opnum,
            requestPayload,
            cancellationToken).ConfigureAwait(false);
        return new NdrCallResult(result.Hresult, result.Payload);
    }
}
