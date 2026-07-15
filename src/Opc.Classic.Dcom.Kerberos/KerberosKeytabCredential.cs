// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Disposable keytab credential snapshot.
/// </summary>
public sealed class KerberosKeytabCredential : KerberosServerCredential
{
    private readonly byte[] _keytab;

    /// <summary>
    /// Initializes a keytab credential by copying the supplied bytes.
    /// </summary>
    public KerberosKeytabCredential(
        string principal,
        string realm,
        ReadOnlySpan<byte> keytab)
        : base(principal, realm)
    {
        if (keytab.Length < 2 || keytab[0] != 5 || (keytab[1] != 1 && keytab[1] != 2))
        {
            throw new ArgumentException("The credential is not a recognized MIT keytab.", nameof(keytab));
        }

        _keytab = keytab.ToArray();
    }

    /// <inheritdoc />
    public override KerberosServerCredentialKind Kind => KerberosServerCredentialKind.Keytab;

    /// <summary>
    /// Gets the keytab byte length.
    /// </summary>
    public int SecretLength => _keytab.Length;

    /// <summary>
    /// Copies the keytab into a caller-owned destination.
    /// </summary>
    public void CopyKeytabTo(Span<byte> destination)
    {
        ThrowIfDisposed();
        if (destination.Length < _keytab.Length)
        {
            throw new ArgumentException("The destination is too short.", nameof(destination));
        }

        _keytab.CopyTo(destination);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{nameof(KerberosKeytabCredential)} {{ Principal = {Principal}, Realm = {Realm}, Keytab = [REDACTED] }}";

    /// <inheritdoc />
    protected override void DisposeSecret() =>
        CryptographicOperations.ZeroMemory(_keytab);
}
