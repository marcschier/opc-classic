// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Base class for an independently disposable Kerberos server credential snapshot.
/// </summary>
public abstract class KerberosServerCredential : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Initializes a credential snapshot.
    /// </summary>
    protected KerberosServerCredential(string principal, string realm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        Principal = principal;
        Realm = realm;
    }

    /// <summary>
    /// Gets the service principal represented by this credential.
    /// </summary>
    public string Principal { get; }

    /// <summary>
    /// Gets the Kerberos realm represented by this credential.
    /// </summary>
    public string Realm { get; }

    /// <summary>
    /// Gets the kind of secret material carried by this credential.
    /// </summary>
    public abstract KerberosServerCredentialKind Kind { get; }

    /// <summary>
    /// Gets a value indicating whether this snapshot has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Clears owned resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DisposeSecret();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Throws when the snapshot has been disposed.
    /// </summary>
    protected void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>
    /// Clears owned secret material.
    /// </summary>
    protected abstract void DisposeSecret();
}
