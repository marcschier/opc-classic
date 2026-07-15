// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security.Principal;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Mechanism-neutral state established after server authentication succeeds.
/// </summary>
public sealed class RpcServerAuthenticationSession
{
    /// <summary>
    /// Initializes an established authentication session.
    /// </summary>
    /// <remarks>
    /// Integrity and privacy sessions must supply a matching protection context with a non-empty
    /// verifier. Lower protection levels may omit packet protection.
    /// </remarks>
    public RpcServerAuthenticationSession(
        int authenticationService,
        IPrincipal principal,
        OpcProtectionLevel protectionLevel,
        IRpcServerProtectionContext? protectionContext = null)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if ((uint)authenticationService > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(authenticationService),
                "RPC authentication service identifiers must fit in one byte.");
        }
        if (protectionLevel >= OpcProtectionLevel.Integrity
            && protectionContext is null)
        {
            throw new ArgumentException(
                "Integrity and privacy sessions require a packet-protection context.",
                nameof(protectionContext));
        }
        if (protectionContext is not null
            && protectionContext.AuthenticationService != authenticationService)
        {
            throw new ArgumentException(
                "The protection context authentication service must match the session.",
                nameof(protectionContext));
        }
        if (protectionContext is not null
            && protectionContext.ProtectionLevel != protectionLevel)
        {
            throw new ArgumentException(
                "The protection context level must match the session.",
                nameof(protectionContext));
        }
        if (protectionLevel >= OpcProtectionLevel.Integrity
            && (protectionContext!.VerifierLength <= 0
                || protectionContext.VerifierLength > ushort.MaxValue))
        {
            throw new ArgumentException(
                "Integrity and privacy sessions require a non-empty verifier that fits the RPC auth-length field.",
                nameof(protectionContext));
        }

        AuthenticationService = authenticationService;
        Principal = principal;
        ProtectionLevel = protectionLevel;
        ProtectionContext = protectionContext;
    }

    /// <summary>
    /// Gets the RPC authentication service identifier.
    /// </summary>
    public int AuthenticationService { get; }

    /// <summary>
    /// Gets the authenticated or authorization-mapped principal.
    /// </summary>
    public IPrincipal Principal { get; }

    /// <summary>
    /// Gets the negotiated protection level.
    /// </summary>
    public OpcProtectionLevel ProtectionLevel { get; }

    /// <summary>
    /// Gets the packet protection context, which is required for integrity and privacy sessions.
    /// </summary>
    public IRpcServerProtectionContext? ProtectionContext { get; }

    internal RpcServerAuthenticationSession WithPrincipal(IPrincipal principal) =>
        new(AuthenticationService, principal, ProtectionLevel, ProtectionContext);
}
