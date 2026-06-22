// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Security.Dcom;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// Creates OPC Security client state for a session.
/// </summary>
public interface IOpcSecurityClientFactory
{
    /// <summary>
    /// Creates or resolves an OPC Security client for the supplied session.
    /// </summary>
    Task<SecurityClientState> CreateAsync(OpcSession session, CancellationToken cancellationToken = default);
}

/// <summary>
/// MCP tools for optional OPC Security interfaces.
/// </summary>
public sealed class SecurityTools
{
    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcSecurityClientFactory _clientFactory;

    /// <summary>
    /// Creates the OPC Security tool set.
    /// </summary>
    public SecurityTools(IOpcSessionManager sessionManager, IEnumerable<IOpcSecurityClientFactory> clientFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(clientFactories);
        _clientFactory = clientFactories.FirstOrDefault() ?? new DefaultOpcSecurityClientFactory();
    }

    /// <summary>
    /// Checks whether Windows-integrated OPC Security is available.
    /// </summary>
    [McpServerTool(Name = "opcclassic.security.is_available_nt", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Checks whether the connected OPC server supports IOPCSecurityNT Windows-integrated authentication.")]
    public async Task<OpcSecurityInfoDto> IsAvailableNt(
        [Description("The sessionId returned by opcclassic.session.create, typically connected to a DCOM OPC server.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        SecurityClientState state = await GetSecurityClientAsync(sessionId, cancellationToken).ConfigureAwait(false);
        bool nt = await state.Client.IsAvailableNtAsync(cancellationToken).ConfigureAwait(false);
        bool priv = await state.Client.IsAvailablePrivateAsync(cancellationToken).ConfigureAwait(false);
        return ToInfo(state.Client, nt, priv, nt ? "OPC Security NT is available." : "OPC Security NT is not available.");
    }

    /// <summary>
    /// Checks whether private OPC Security is available.
    /// </summary>
    [McpServerTool(Name = "opcclassic.security.is_available_private", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Checks whether the connected OPC server supports IOPCSecurityPrivate username/password authentication.")]
    public async Task<OpcSecurityInfoDto> IsAvailablePrivate(
        [Description("The sessionId returned by opcclassic.session.create, typically connected to a DCOM OPC server.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        SecurityClientState state = await GetSecurityClientAsync(sessionId, cancellationToken).ConfigureAwait(false);
        bool nt = await state.Client.IsAvailableNtAsync(cancellationToken).ConfigureAwait(false);
        bool priv = await state.Client.IsAvailablePrivateAsync(cancellationToken).ConfigureAwait(false);
        return ToInfo(state.Client, nt, priv, priv ? "OPC Security private authentication is available." : "OPC Security private authentication is not available.");
    }

    /// <summary>
    /// Logs on using IOPCSecurityPrivate credentials.
    /// </summary>
    [McpServerTool(Name = "opcclassic.security.logon", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Logs on to IOPCSecurityPrivate with server-managed username/password credentials.")]
    public async Task<OpcResultDto> Logon(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server-private username.")]
        string username,
        [Description("Server-private password. This is sent to the OPC server according to the server's configured DCOM security.")]
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentNullException.ThrowIfNull(password);
        SecurityClientState state = await GetSecurityClientAsync(sessionId, cancellationToken).ConfigureAwait(false);
        bool succeeded = await state.Client.LogonPrivateAsync(username, password, cancellationToken).ConfigureAwait(false);
        return succeeded
            ? new OpcResultDto(0, $"OPC Security logon succeeded for '{state.Client.CurrentIdentity}'.", Succeeded: true, ItemName: state.Client.CurrentIdentity)
            : new OpcResultDto(1, "OPC Security logon failed.", Succeeded: false, ItemName: username);
    }

    /// <summary>
    /// Logs off from private OPC Security.
    /// </summary>
    [McpServerTool(Name = "opcclassic.security.logoff", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Logs off IOPCSecurityPrivate and returns to the connection's default identity.")]
    public async Task<OpcResultDto> Logoff(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        SecurityClientState state = await GetSecurityClientAsync(sessionId, cancellationToken).ConfigureAwait(false);
        await state.Client.LogoffAsync(cancellationToken).ConfigureAwait(false);
        return new OpcResultDto(0, "OPC Security logoff succeeded.", Succeeded: true);
    }

    private async Task<SecurityClientState> GetSecurityClientAsync(string sessionId, CancellationToken cancellationToken)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        if (session.SecurityClient is not null)
        {
            return session.SecurityClient;
        }

        SecurityClientState state = await _clientFactory.CreateAsync(session, cancellationToken).ConfigureAwait(false);
        session.SecurityClient = state;
        return state;
    }

    private static OpcSecurityInfoDto ToInfo(IOpcSecurityClient client, bool supportsNt, bool supportsPrivate, string message) =>
        new(supportsNt, supportsPrivate, client.IsAuthenticated, client.CurrentIdentity, message);

    private sealed class DefaultOpcSecurityClientFactory : IOpcSecurityClientFactory
    {
        public Task<SecurityClientState> CreateAsync(OpcSession session, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(session);
            cancellationToken.ThrowIfCancellationRequested();
            DaClientState daClient = session.DaClient ?? throw new McpException("OPC Security tools require an existing DCOM client in the session. Connect DA first or register IOpcSecurityClientFactory.");
            return Task.FromResult(new SecurityClientState(new DcomOpcSecurityClient(daClient.CallChannel)));
        }
    }

    private sealed class DcomOpcSecurityClient : IOpcSecurityClient
    {
        private readonly IOPCSecurityNTClientProxy _nt;
        private readonly IOPCSecurityPrivateClientProxy _priv;

        public DcomOpcSecurityClient(ICallChannel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);
            _nt = new IOPCSecurityNTClientProxy(channel);
            _priv = new IOPCSecurityPrivateClientProxy(channel);
        }

        public bool IsAuthenticated { get; private set; }
        public string CurrentIdentity { get; private set; } = string.Empty;

        public async Task<bool> IsAvailableNtAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _nt.IsAvailableNTAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OpcException)
            {
                return false;
            }
            catch (NotImplementedException)
            {
                return false;
            }
        }

        public async Task<bool> IsAvailablePrivateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _priv.IsAvailablePrivAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OpcException)
            {
                return false;
            }
            catch (NotImplementedException)
            {
                return false;
            }
        }

        public async Task<bool> LogonPrivateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            if (!await IsAvailablePrivateAsync(cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            try
            {
                await _priv.LogonAsync(username, password, cancellationToken).ConfigureAwait(false);
                IsAuthenticated = true;
                CurrentIdentity = "private:" + username;
                return true;
            }
            catch (OpcException)
            {
                IsAuthenticated = false;
                CurrentIdentity = string.Empty;
                return false;
            }
        }

        public async Task LogoffAsync(CancellationToken cancellationToken = default)
        {
            await _priv.LogoffAsync(cancellationToken).ConfigureAwait(false);
            IsAuthenticated = false;
            CurrentIdentity = string.Empty;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
