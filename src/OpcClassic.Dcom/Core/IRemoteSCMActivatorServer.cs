//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace SharpInterop.Core;

/// <summary>
/// Server-side contract for IRemoteSCMActivator activation handling.
/// </summary>
public interface IRemoteSCMActivatorServer
{
    /// <summary>
    /// Handles a decoded RemoteCreateInstance request.
    /// </summary>
    Task<RemoteCreateInstanceResponse> RemoteCreateInstanceAsync(
        RemoteCreateInstanceRequest request,
        CancellationToken cancellationToken = default);
}
