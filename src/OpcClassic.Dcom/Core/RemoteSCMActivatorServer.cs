//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Hosting;

namespace SharpInterop.Core;

/// <summary>
/// Server-side scaffold for IRemoteSCMActivator::RemoteCreateInstance.
/// </summary>
public sealed class RemoteSCMActivatorServer : IRemoteSCMActivatorServer
{
    internal const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154u);
    internal const int E_NOTIMPL = unchecked((int)0x80004001u);

    private readonly IClsidRegistry _registry;

    /// <summary>
    /// Initializes a new server-side activation handler.
    /// </summary>
    public RemoteSCMActivatorServer(IClsidRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <inheritdoc />
    public Task<RemoteCreateInstanceResponse> RemoteCreateInstanceAsync(
        RemoteCreateInstanceRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (!_registry.TryResolve(request.Clsid, out _))
        {
            return Task.FromResult(new RemoteCreateInstanceResponse(
                REGDB_E_CLASSNOTREG,
                Guid.Empty,
                Guid.Empty,
                []));
        }

        // Full implementation will instantiate LocalCoClass, allocate OXID/IPID,
        // encode STDOBJREF + DUALSTRINGARRAY, and wrap it as a MEOW OBJREF.
        return Task.FromResult(new RemoteCreateInstanceResponse(
            E_NOTIMPL,
            Guid.Empty,
            Guid.Empty,
            []));
    }
}
