//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Net;
using System.Runtime.Versioning;
using System.Security.Principal;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Factory that creates <see cref="LocalNamedPipeTransport"/> instances for
/// <see cref="NcacnNpEndPoint"/> values whose host resolves to the local
/// machine. Remote pipe endpoints are rejected; pair this factory with
/// <see cref="NcacnNpTransportFactory"/> (SMB2) for cross-machine
/// <c>ncacn_np</c> activations.
/// </summary>
/// <remarks>
/// Used by the in-repo DCOM activation flow as the kernel-pipe optimisation
/// for local servers that bind LRPC instead of <c>ncacn_ip_tcp</c> (e.g. the
/// OPC Foundation native TestServer). The factory accepts any of the common
/// local-host spellings (<c>.</c>, <c>localhost</c>, <c>127.0.0.1</c>,
/// <c>::1</c>, the machine's NetBIOS name) so OBJREF string-bindings
/// returned by the local SCM dispatch correctly regardless of how the
/// server populated the resolver block.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class LocalNamedPipeTransportFactory : IAsyncTransportFactory
{
    private readonly TokenImpersonationLevel _impersonationLevel;
    private readonly Lazy<HashSet<string>> _localAliases;

    /// <summary>
    /// Initializes a new local named-pipe transport factory.
    /// </summary>
    /// <param name="impersonationLevel">Token impersonation level the kernel
    /// pipe should expose to the server. Defaults to
    /// <see cref="TokenImpersonationLevel.Impersonation"/>, matching the
    /// DCOM <c>RPC_C_IMP_LEVEL_IMPERSONATE</c> default.</param>
    public LocalNamedPipeTransportFactory(
        TokenImpersonationLevel impersonationLevel = TokenImpersonationLevel.Impersonation)
    {
        _impersonationLevel = impersonationLevel;
        _localAliases = new Lazy<HashSet<string>>(BuildLocalHostAliases, isThreadSafe: true);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the supplied host string
    /// references the local machine (and therefore matches this factory).
    /// </summary>
    public bool IsLocalHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }
        return _localAliases.Value.Contains(host.Trim());
    }

    /// <inheritdoc />
    public async ValueTask<IAsyncTransport> ConnectAsync(
        EndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (endpoint is not NcacnNpEndPoint namedPipeEndpoint)
        {
            throw new NotSupportedException(
                $"Endpoint type '{endpoint.GetType().FullName}' is not an ncacn_np endpoint.");
        }

        if (!IsLocalHost(namedPipeEndpoint.Host))
        {
            throw new NotSupportedException(
                $"Host '{namedPipeEndpoint.Host}' is not local; use NcacnNpTransportFactory (SMB2) for remote pipe activations.");
        }

        return await LocalNamedPipeTransport.ConnectAsync(
            namedPipeEndpoint.PipeName,
            _impersonationLevel,
            cancellationToken).ConfigureAwait(false);
    }

    private static HashSet<string> BuildLocalHostAliases()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".",
            "localhost",
            "127.0.0.1",
            "::1",
        };
        try
        {
            set.Add(Dns.GetHostName());
            string fqdn = Dns.GetHostEntry(string.Empty).HostName;
            if (!string.IsNullOrWhiteSpace(fqdn))
            {
                set.Add(fqdn);
            }
        }
        catch (System.Net.Sockets.SocketException)
        {
            // Host name lookup failed (typical on isolated CI agents); the
            // canonical aliases above are sufficient for the common cases.
        }
        return set;
    }
}
