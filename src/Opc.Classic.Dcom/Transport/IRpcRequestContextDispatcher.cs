//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
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
