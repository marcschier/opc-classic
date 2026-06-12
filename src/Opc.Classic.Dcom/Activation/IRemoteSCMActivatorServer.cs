//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Server-side contract for IRemoteSCMActivator activation handling.
/// </summary>
public interface IRemoteSCMActivatorServer : IRemoteSCMActivator
{
    /// <summary>
    /// Handles a decoded RemoteCreateInstance request.
    /// </summary>
    Task<RemoteCreateInstanceResponse> RemoteCreateInstanceAsync(
        RemoteCreateInstanceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a decoded RemoteGetClassObject request.
    /// </summary>
    Task<RemoteGetClassObjectResponse> RemoteGetClassObjectAsync(
        RemoteGetClassObjectRequest request,
        CancellationToken cancellationToken = default);
}
