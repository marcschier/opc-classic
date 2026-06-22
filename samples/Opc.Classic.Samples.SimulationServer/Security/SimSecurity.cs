// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Security;

/// <summary>
/// Creates per-session OPC Security clients for the simulation server MCP host.
/// </summary>
public sealed class SimSecurityClientFactory : IOpcSecurityClientFactory
{
    private readonly Dictionary<string, string> _accounts;
    private readonly bool _supportsNt;
    private readonly bool _supportsPrivate;

    /// <summary>
    /// Creates the default simulation security client factory.
    /// </summary>
    public SimSecurityClientFactory()
        : this(CreateDefaultAccounts(), supportsNt: true, supportsPrivate: true)
    {
    }

    /// <summary>
    /// Creates a simulation security client factory with explicit capabilities and accounts.
    /// </summary>
    /// <param name="accounts">Server-private username/password account map.</param>
    /// <param name="supportsNt">Whether Windows-integrated OPC Security is reported as available.</param>
    /// <param name="supportsPrivate">Whether private username/password OPC Security is reported as available.</param>
    public SimSecurityClientFactory(IReadOnlyDictionary<string, string> accounts, bool supportsNt, bool supportsPrivate)
    {
        ArgumentNullException.ThrowIfNull(accounts);

        _accounts = CopyAccounts(accounts);
        _supportsNt = supportsNt;
        _supportsPrivate = supportsPrivate;
    }

    /// <inheritdoc />
    public Task<SecurityClientState> CreateAsync(OpcSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        var client = new SimSecurityClient(_accounts, _supportsNt, _supportsPrivate);
        return Task.FromResult(new SecurityClientState(client));
    }

    private static Dictionary<string, string> CreateDefaultAccounts() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["operator"] = "correct",
            ["engineer"] = "calibrate",
            ["supervisor"] = "approve",
        };

    private static Dictionary<string, string> CopyAccounts(IReadOnlyDictionary<string, string> accounts)
    {
        var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> account in accounts)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(account.Key);
            ArgumentNullException.ThrowIfNull(account.Value);
            copy.Add(account.Key, account.Value);
        }

        return copy;
    }
}

/// <summary>
/// In-memory OPC Security client used by the simulation server.
/// </summary>
public sealed class SimSecurityClient : IOpcSecurityClient
{
    private readonly Dictionary<string, string> _accounts;
    private readonly bool _supportsNt;
    private readonly bool _supportsPrivate;

    /// <summary>
    /// Creates a simulation OPC Security client.
    /// </summary>
    /// <param name="accounts">Server-private username/password account map.</param>
    /// <param name="supportsNt">Whether Windows-integrated OPC Security is available.</param>
    /// <param name="supportsPrivate">Whether private username/password OPC Security is available.</param>
    public SimSecurityClient(IReadOnlyDictionary<string, string> accounts, bool supportsNt, bool supportsPrivate)
    {
        ArgumentNullException.ThrowIfNull(accounts);

        _accounts = CopyAccounts(accounts);
        _supportsNt = supportsNt;
        _supportsPrivate = supportsPrivate;
    }

    private static Dictionary<string, string> CopyAccounts(IReadOnlyDictionary<string, string> accounts)
    {
        var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> account in accounts)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(account.Key);
            ArgumentNullException.ThrowIfNull(account.Value);
            copy.Add(account.Key, account.Value);
        }

        return copy;
    }

    /// <inheritdoc />
    public bool IsAuthenticated { get; private set; }

    /// <inheritdoc />
    public string CurrentIdentity { get; private set; } = string.Empty;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public Task<bool> IsAvailableNtAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_supportsNt);
    }

    /// <inheritdoc />
    public Task<bool> IsAvailablePrivateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_supportsPrivate);
    }

    /// <inheritdoc />
    public Task<bool> LogonPrivateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentNullException.ThrowIfNull(password);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_supportsPrivate ||
            !_accounts.TryGetValue(username, out string? expectedPassword) ||
            !string.Equals(expectedPassword, password, StringComparison.Ordinal))
        {
            return Task.FromResult(false);
        }

        IsAuthenticated = true;
        CurrentIdentity = "private:" + username;
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task LogoffAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsAuthenticated = false;
        CurrentIdentity = string.Empty;
        return Task.CompletedTask;
    }
}
