// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Disposable password credential snapshot.
/// </summary>
public sealed class KerberosPasswordCredential : KerberosServerCredential
{
    private readonly char[] _password;

    /// <summary>
    /// Initializes a password credential by copying the supplied characters.
    /// </summary>
    public KerberosPasswordCredential(
        string principal,
        string realm,
        ReadOnlySpan<char> password)
        : base(principal, realm)
    {
        if (password.IsEmpty)
        {
            throw new ArgumentException("A Kerberos service password cannot be empty.", nameof(password));
        }

        _password = password.ToArray();
    }

    /// <inheritdoc />
    public override KerberosServerCredentialKind Kind => KerberosServerCredentialKind.Password;

    /// <summary>
    /// Gets the password character count.
    /// </summary>
    public int SecretLength => _password.Length;

    /// <summary>
    /// Copies the password into a caller-owned destination.
    /// </summary>
    public void CopyPasswordTo(Span<char> destination)
    {
        ThrowIfDisposed();
        if (destination.Length < _password.Length)
        {
            throw new ArgumentException("The destination is too short.", nameof(destination));
        }

        _password.CopyTo(destination);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{nameof(KerberosPasswordCredential)} {{ Principal = {Principal}, Realm = {Realm}, Password = [REDACTED] }}";

    /// <inheritdoc />
    protected override void DisposeSecret() =>
        Array.Clear(_password);
}
