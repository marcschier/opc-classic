// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Hosting;

namespace Opc.Classic.Dcom.Transport;

public interface IRpcRequestContextDispatcher : IOpcServerDispatcher
{
    ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        RpcRequestContext requestContext,
        CancellationToken cancellationToken = default);
}
