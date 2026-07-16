// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security.Principal;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Maps a mechanism-authenticated principal to the principal exposed to server dispatchers.
/// </summary>
public interface IRpcServerAuthorizationMapper
{
    /// <summary>
    /// Maps an authenticated principal to an application authorization principal.
    /// </summary>
    IPrincipal MapPrincipal(IPrincipal authenticatedPrincipal);
}
