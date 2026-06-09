// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Runtime.Versioning;
using System.Security.Principal;
using Opc.Classic;
using Opc.Classic.Security;
using Opc.Classic.Security.Dcom;

namespace Opc.Classic.Samples.OpcSecurityServer;

/// <summary>
/// STUB OPC Security implementation for the sample server.
/// </summary>
/// <remarks>
/// This class keeps authentication state in memory and accepts one demo
/// credential. Real OPC Security 1.00 servers would call CoImpersonateClient,
/// CoQueryClientBlanket, and AccessCheck (or equivalent managed policy checks)
/// per OPC Security 1.00 §4.5 before allowing access to protected objects.
/// </remarks>
public sealed class StubOpcSecurityServer : IOpcSecurity, IOPCSecurityNT, IOPCSecurityPrivate {
    private static readonly OpcResultId PrivateActiveResult = new(
        OpcSecurityErrors.OPC_E_PRIVATE_ACTIVE,
        nameof(OpcSecurityErrors.OPC_E_PRIVATE_ACTIVE));

    private const string OperatorUserName = "operator";
    private const string OperatorPassword = "demo";
    private const string PrivateIdentityPrefix = "private:";
    private const string IdentityEnvironmentVariable = "OPC_CLASSIC_SAMPLE_IDENTITY";

    private readonly object _stateLock = new();
    private bool _isAuthenticated;
    private string _currentIdentity = string.Empty;

    public bool SupportsWindowsAuthentication => true;

    public bool SupportsPrivateAuthentication => true;

    public bool IsAuthenticated {
        get {
            lock (_stateLock) {
                return _isAuthenticated;
            }
        }
    }

    public string CurrentIdentity {
        get {
            lock (_stateLock) {
                return _currentIdentity;
            }
        }
    }

    public Task<bool> LoginAsCurrentUserAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        string? configuredIdentity = Environment.GetEnvironmentVariable(IdentityEnvironmentVariable);
        string identity = string.IsNullOrWhiteSpace(configuredIdentity)
            ? GetCurrentPlatformIdentity()
            : configuredIdentity;
        SetAuthenticated(identity);
        return Task.FromResult(true);
    }

    public Task<bool> LoginPrivateAsync(string username, string password, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsOperatorCredential(username, password)) {
            return Task.FromResult(false);
        }

        SetAuthenticated(PrivateIdentityPrefix + username);
        return Task.FromResult(true);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_stateLock) {
            _isAuthenticated = false;
            _currentIdentity = string.Empty;
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsAvailableNTAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SupportsWindowsAuthentication);
    }

    public Task<int> QueryMinImpersonationLevelAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((int)OpcImpersonationLevel.Impersonate);
    }

    public async Task ChangeUserAsync(CancellationToken cancellationToken = default) {
        bool success = await LoginAsCurrentUserAsync(cancellationToken).ConfigureAwait(false);
        if (!success) {
            throw new OpcException(OpcResultId.Fail, "OPC Security NT authentication failed.");
        }
    }

    public Task<bool> IsAvailablePrivAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SupportsPrivateAuthentication);
    }

    public async Task LogonAsync(string userId, string password, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(password);

        if (IsPrivateIdentityActive()) {
            throw new OpcException(PrivateActiveResult, "A private OPC Security identity is already active.");
        }

        bool success = await LoginPrivateAsync(userId, password, cancellationToken).ConfigureAwait(false);
        if (!success) {
            throw new OpcException(OpcResultId.Fail, "OPC Security private authentication failed.");
        }
    }

    public Task LogoffAsync(CancellationToken cancellationToken = default) => LogoutAsync(cancellationToken);

    private static bool IsOperatorCredential(string username, string password) =>
        string.Equals(username, OperatorUserName, StringComparison.Ordinal)
        && string.Equals(password, OperatorPassword, StringComparison.Ordinal);

    private static string GetCurrentPlatformIdentity() {
        if (OperatingSystem.IsWindows()) {
            return GetWindowsIdentityName();
        }

        return string.IsNullOrWhiteSpace(Environment.UserName)
            ? "sample-current-user"
            : Environment.UserName;
    }

    [SupportedOSPlatform("windows")]
    private static string GetWindowsIdentityName() {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        return string.IsNullOrWhiteSpace(identity.Name) ? "sample-current-user" : identity.Name;
    }

    private bool IsPrivateIdentityActive() {
        lock (_stateLock) {
            return _isAuthenticated && _currentIdentity.StartsWith(PrivateIdentityPrefix, StringComparison.Ordinal);
        }
    }

    private void SetAuthenticated(string identity) {
        lock (_stateLock) {
            _isAuthenticated = true;
            _currentIdentity = identity;
        }
    }
}
