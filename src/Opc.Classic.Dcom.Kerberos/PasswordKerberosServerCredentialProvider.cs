// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Thread-safe, rotatable in-memory Kerberos service password provider.
/// </summary>
public sealed class PasswordKerberosServerCredentialProvider : IKerberosServerCredentialProvider
{
    private readonly Lock _gate = new();
    private char[] _password;
    private bool _disposed;
    private long _version;

    /// <summary>
    /// Initializes a password credential provider.
    /// </summary>
    public PasswordKerberosServerCredentialProvider(
        string principal,
        string realm,
        ReadOnlySpan<char> password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        if (password.IsEmpty)
        {
            throw new ArgumentException("A Kerberos service password cannot be empty.", nameof(password));
        }

        Principal = principal;
        Realm = realm;
        _password = password.ToArray();
        _version = 1;
    }

    /// <inheritdoc />
    public string Principal { get; }

    /// <inheritdoc />
    public string Realm { get; }

    /// <inheritdoc />
    public long Version => Interlocked.Read(ref _version);

    /// <inheritdoc />
    public KerberosServerCredential AcquireCredential()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return new KerberosPasswordCredential(Principal, Realm, _password);
        }
    }

    /// <summary>
    /// Replaces the current password and clears the prior character buffer.
    /// </summary>
    public void Rotate(ReadOnlySpan<char> password)
    {
        if (password.IsEmpty)
        {
            throw new ArgumentException("A Kerberos service password cannot be empty.", nameof(password));
        }

        char[] replacement = password.ToArray();
        lock (_gate)
        {
            try
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                char[] prior = _password;
                _password = replacement;
                replacement = [];
                Array.Clear(prior);
                _version++;
            }
            finally
            {
                Array.Clear(replacement);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                Array.Clear(_password);
                _disposed = true;
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{nameof(PasswordKerberosServerCredentialProvider)} {{ Principal = {Principal}, Realm = {Realm}, Password = [REDACTED], Version = {Version} }}";
}
