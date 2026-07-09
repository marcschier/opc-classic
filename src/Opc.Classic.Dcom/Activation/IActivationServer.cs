// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

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
