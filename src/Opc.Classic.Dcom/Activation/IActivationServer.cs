//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Server-side contract for the legacy <c>IActivation</c> interface.
/// </summary>
public interface IActivationServer : IActivation
{
    /// <summary>
    /// Handles a decoded <c>RemoteActivation</c> request.
    /// </summary>
    Task<RemoteActivationResponse> RemoteActivationAsync(
        RemoteActivationRequest request,
        CancellationToken cancellationToken = default);
}
