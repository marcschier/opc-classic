// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Supplies short-lived server credential snapshots for Kerberos ticket validation.
/// </summary>
public interface IKerberosServerCredentialProvider : IDisposable
{
    /// <summary>
    /// Gets the service principal represented by the credential.
    /// </summary>
    string Principal { get; }

    /// <summary>
    /// Gets the Kerberos realm represented by the credential.
    /// </summary>
    string Realm { get; }

    /// <summary>
    /// Gets the monotonically increasing credential version.
    /// </summary>
    long Version { get; }

    /// <summary>
    /// Acquires an independently disposable snapshot of the current credential.
    /// </summary>
    KerberosServerCredential AcquireCredential();
}
