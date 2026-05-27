//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Ae.Hosting;

/// <summary>Dispatches NDR-encoded AE DCOM calls to a managed AE server implementation.</summary>
public interface IOpcAeServerDispatcher
{
    /// <summary>Routes an incoming interface/opnum request and returns an HRESULT plus NDR response body.</summary>
    Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken);

    /// <summary>Creates a dispatcher for an <c>IOPCEventAreaBrowser</c> instance.</summary>
    Task<IOpcAeAreaBrowserDispatcher> CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);
}
