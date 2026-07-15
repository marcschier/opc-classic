// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Loads a keytab from disk and automatically rotates cached bytes when the file changes.
/// </summary>
/// <remarks>
/// Writers must publish complete keytabs by atomically replacing the path. In-place mutation,
/// unstable metadata, torn keytabs, and oversized files are rejected before rotation.
/// </remarks>
public sealed class FileKerberosKeytabCredentialProvider : IKerberosServerCredentialProvider
{
    private const int MaximumKeytabSize = 16 * 1024 * 1024;
    private const int StableReadAttempts = 3;
    private readonly Lock _gate = new();
    private byte[] _keytab = [];
    private bool _disposed;
    private long _version;

    /// <summary>
    /// Initializes a file-backed keytab provider and loads the initial keytab.
    /// </summary>
    public FileKerberosKeytabCredentialProvider(
        string principal,
        string realm,
        string keytabPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        ArgumentException.ThrowIfNullOrWhiteSpace(keytabPath);

        Principal = principal;
        Realm = realm;
        KeytabPath = Path.GetFullPath(keytabPath);
        Reload();
    }

    /// <inheritdoc />
    public string Principal { get; }

    /// <inheritdoc />
    public string Realm { get; }

    /// <summary>
    /// Gets the absolute keytab path.
    /// </summary>
    public string KeytabPath { get; }

    /// <inheritdoc />
    public long Version => Interlocked.Read(ref _version);

    /// <inheritdoc />
    public KerberosServerCredential AcquireCredential()
    {
        Reload();
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return new KerberosKeytabCredential(Principal, Realm, _keytab);
        }
    }

    /// <summary>
    /// Reloads the keytab when its bytes have changed.
    /// </summary>
    /// <returns><see langword="true"/> when a new credential version was installed.</returns>
    public bool Reload()
    {
        byte[] loaded = ReadKeytab(KeytabPath, Principal, Realm);
        lock (_gate)
        {
            try
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (_keytab.AsSpan().SequenceEqual(loaded))
                {
                    return false;
                }

                byte[] prior = _keytab;
                _keytab = loaded;
                loaded = [];
                CryptographicOperations.ZeroMemory(prior);
                _version++;
                return true;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(loaded);
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
                CryptographicOperations.ZeroMemory(_keytab);
                _disposed = true;
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{nameof(FileKerberosKeytabCredentialProvider)} {{ Principal = {Principal}, Realm = {Realm}, Keytab = [REDACTED], Version = {Version} }}";

    private static byte[] ReadKeytab(string path, string principal, string realm)
    {
        for (int attempt = 0; attempt < StableReadAttempts; attempt++)
        {
            byte[]? bytes = TryReadStableKeytab(path, principal, realm);
            if (bytes is not null)
            {
                return bytes;
            }

            Thread.Yield();
        }

        throw new InvalidDataException("The keytab changed while it was being read.");
    }

    private static byte[]? TryReadStableKeytab(string path, string principal, string realm)
    {
        var before = new FileInfo(path);
        before.Refresh();
        long expectedLength = before.Length;
        DateTime expectedLastWriteTimeUtc = before.LastWriteTimeUtc;
        ValidateLength(expectedLength);

        byte[] bytes = GC.AllocateUninitializedArray<byte>(checked((int)expectedLength));
        bool accepted = false;
        try
        {
            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read | FileShare.Delete))
            {
                if (stream.Length != expectedLength)
                {
                    return null;
                }

                try
                {
                    stream.ReadExactly(bytes);
                }
                catch (EndOfStreamException)
                {
                    return null;
                }
                if (stream.Length != expectedLength || stream.ReadByte() != -1)
                {
                    return null;
                }
            }

            var after = new FileInfo(path);
            after.Refresh();
            if (after.Length != expectedLength
                || after.LastWriteTimeUtc != expectedLastWriteTimeUtc)
            {
                return null;
            }

            ValidateKeytab(bytes, principal, realm);
            accepted = true;
            return bytes;
        }
        finally
        {
            if (!accepted)
            {
                CryptographicOperations.ZeroMemory(bytes);
            }
        }
    }

    private static void ValidateLength(long length)
    {
        if (length <= 0)
        {
            throw new InvalidDataException("Kerberos keytabs cannot be empty.");
        }
        if (length > MaximumKeytabSize)
        {
            throw new InvalidDataException("Kerberos keytabs larger than 16 MiB are not accepted.");
        }
    }

    private static void ValidateKeytab(byte[] bytes, string principal, string realm)
    {
        KeyTable keyTable;
        try
        {
            keyTable = new KeyTable(bytes);
        }
        catch (Exception exception) when (
            exception is ArgumentException
                or EndOfStreamException
                or IndexOutOfRangeException
                or InvalidDataException
                or OverflowException)
        {
            throw new InvalidDataException("The credential file is not a valid MIT keytab.", exception);
        }

        try
        {
            if (keyTable.Entries.Count == 0)
            {
                throw new InvalidDataException("The credential file contains no keytab entries.");
            }

            string expectedPrincipal = $"{principal}@{realm}";
            if (!keyTable.Entries.Any(entry =>
                    string.Equals(entry.Principal.FullyQualifiedName, principal, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(entry.Principal.Realm, realm, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDataException(
                    $"The credential file does not contain the configured service principal '{expectedPrincipal}'.");
            }
        }
        finally
        {
            ZeroParsedSecrets(keyTable, bytes);
        }
    }

    private static void ZeroParsedSecrets(KeyTable keyTable, byte[] ownedKeytab)
    {
        foreach (KeyEntry entry in keyTable.Entries)
        {
            ZeroMemory(entry.Key.PasswordBytes, ownedKeytab);
            ZeroMemory(entry.Key.SaltBytes, ownedKeytab);
            ZeroMemory(entry.Key.IterationParameter, ownedKeytab);
        }
    }

    private static void ZeroMemory(ReadOnlyMemory<byte> memory, byte[] ownedKeytab)
    {
        if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment)
            && segment.Array is not null
            && !ReferenceEquals(segment.Array, ownedKeytab))
        {
            CryptographicOperations.ZeroMemory(
                segment.Array.AsSpan(segment.Offset, segment.Count));
        }
    }
}
