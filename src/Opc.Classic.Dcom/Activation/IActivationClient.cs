//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Activation;

/// <summary>Client contract for legacy <c>IActivation::RemoteActivation</c>.</summary>
public interface IActivationClient
{
    /// <summary>Invokes <c>IActivation::RemoteActivation</c> using protocol sequence names.</summary>
    Task<RemoteActivationResponse> RemoteActivationAsync(
        Guid clsid,
        string[] protseqs,
        string? objectStorage,
        Guid[] iids,
        CancellationToken cancellationToken = default);

    /// <summary>Invokes <c>IActivation::RemoteActivation</c> using a fully populated request model.</summary>
    Task<RemoteActivationResponse> RemoteActivationAsync(
        RemoteActivationRequest request,
        CancellationToken cancellationToken = default);
}
