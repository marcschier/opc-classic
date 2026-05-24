//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hosting;

namespace SharpInterop.Core;

/// <summary>
/// Server-side scaffold for IRemoteActivation v5.4 activation handling.
/// </summary>
public sealed class RemoteActivationV54Server {
    internal const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154u);
    internal const int E_NOTIMPL = unchecked((int)0x80004001u);

    private readonly IClsidRegistry _registry;

    /// <summary>
    /// Initializes a new server-side v5.4 activation handler.
    /// </summary>
    public RemoteActivationV54Server(IClsidRegistry registry) {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <summary>
    /// Handles a decoded IRemoteActivation::RemoteActivation request.
    /// </summary>
    public Task<RemoteActivationResponse> RemoteActivationAsync(
        RemoteActivationRequest request,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (!_registry.TryResolve(request.Clsid, out _)) {
            return Task.FromResult(new RemoteActivationResponse(
                REGDB_E_CLASSNOTREG,
                Guid.Empty,
                Guid.Empty,
                []));
        }

        // Full implementation will allocate OXID/IPID state and encode the
        // v5.4 OBJREF response payload.
        return Task.FromResult(new RemoteActivationResponse(
            E_NOTIMPL,
            Guid.Empty,
            Guid.Empty,
            []));
    }
}

/// <summary>
/// Decoded server-side IRemoteActivation::RemoteActivation request fields.
/// </summary>
public sealed record RemoteActivationRequest(
    Guid Clsid,
    Guid Iid,
    int DwFlags,
    IReadOnlyList<int> ProtocolSeqs);

/// <summary>
/// Server-side IRemoteActivation::RemoteActivation response scaffold.
/// </summary>
public sealed record RemoteActivationResponse(
    int Hresult,
    Guid Oxid,
    Guid Ipid,
    byte[] ObjRef);
