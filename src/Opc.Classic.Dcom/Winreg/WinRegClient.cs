//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Registry;
using Opc.Classic.Dcom.Registry.Smb;

namespace Opc.Classic.Dcom.Winreg;

/// <summary>
/// Minimal asynchronous client for the MS-RRP WINREG named-pipe interface.
/// </summary>
public sealed class WinRegClient : IAsyncDisposable {
    /// <summary>Default named pipe endpoint for WINREG over SMB.</summary>
    public const string PipeName = "winreg";

    /// <summary>MS-RRP WINREG RPC interface identifier.</summary>
    public static Guid InterfaceId => OpcGuids.IID_WINREG;

    private readonly DisposableRegistryStub _registry;
    private readonly SemaphoreSlim _callLock = new(1, 1);
    private bool _disposed;

    private WinRegClient(DisposableRegistryStub registry) =>
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));

    /// <summary>
    /// Creates a WINREG client for <c>\\host\PIPE\winreg</c> using explicit NTLM credentials.
    /// </summary>
    public static Task<WinRegClient> ConnectAsync(
        string host,
        string userName,
        string password,
        string domain = "TESTDOMAIN",
        CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentNullException.ThrowIfNull(password);
        cancellationToken.ThrowIfCancellationRequested();

        var authInfo = new DefaultAuthInfoImpl(domain ?? string.Empty, userName, password);
        return Task.FromResult(new WinRegClient(new DisposableRegistryStub(authInfo, host)));
    }

    /// <summary>Calls <c>OpenLocalMachine</c> (opnum 2) and returns an HKLM context handle.</summary>
    public Task<PolicyHandle> OpenHKLMAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(static registry => registry.OpenHKLM(), cancellationToken);

    /// <summary>Calls <c>BaseRegEnumKey</c> (opnum 9) for the supplied handle and index.</summary>
    public Task<string[]> EnumKeyAsync(
        PolicyHandle handle,
        int index,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(handle);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return InvokeAsync(registry => registry.EnumKey(handle, index), cancellationToken);
    }

    /// <summary>Calls <c>BaseRegCloseKey</c> (opnum 5) for the supplied handle.</summary>
    public Task CloseKeyAsync(PolicyHandle handle, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(handle);
        return InvokeAsync(registry => registry.CloseKey(handle), cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        await _callLock.WaitAsync().ConfigureAwait(false);
        try {
            if (_disposed) {
                return;
            }

            _disposed = true;
            _registry.Dispose();
        }
        finally {
            _callLock.Release();
            _callLock.Dispose();
        }
    }

    private async Task<T> InvokeAsync<T>(
        Func<RegistryStub, T> operation,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        await _callLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return await Task.Run(() => operation(_registry), cancellationToken).ConfigureAwait(false);
        }
        finally {
            _callLock.Release();
        }
    }

    private async Task InvokeAsync(
        Action<RegistryStub> operation,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        await _callLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            ObjectDisposedException.ThrowIf(_disposed, this);
            await Task.Run(() => operation(_registry), cancellationToken).ConfigureAwait(false);
        }
        finally {
            _callLock.Release();
        }
    }

    private sealed class DisposableRegistryStub : RegistryStub, IDisposable {
        public DisposableRegistryStub(IAuthInfo authInfo, string serverName)
            : base(authInfo, serverName) {
        }

        public void Dispose() {
            try {
                Detach();
            }
            catch (IOException) {
            }
        }
    }
}
