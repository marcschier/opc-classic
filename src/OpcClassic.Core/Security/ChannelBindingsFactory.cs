//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Security;

/// <summary>
/// Convenience factory for channel-bindings structures that carry TLS
/// endpoint application data.
/// </summary>
public static class ChannelBindingsFactory
{
    /// <summary>
    /// Per MS-CSSP §2.1.1.2: the tls-server-end-point application data is
    /// the literal prefix "tls-server-end-point:" followed by the SHA-256
    /// (or SHA-384/512) hash of the DER-encoded server certificate.
    /// </summary>
    public static ChannelBindings ForTlsServerEndpoint(System.ReadOnlySpan<byte> serverCertDer)
    {
        var hashAlg = SelectHashAlgorithmForCert(serverCertDer);
        byte[] hash = hashAlg switch
        {
            HashKind.Sha256 => System.Security.Cryptography.SHA256.HashData(serverCertDer),
            HashKind.Sha384 => System.Security.Cryptography.SHA384.HashData(serverCertDer),
            HashKind.Sha512 => System.Security.Cryptography.SHA512.HashData(serverCertDer),
            _ => throw new System.NotSupportedException("Certificate signature hash algorithm is not supported."),
        };

        const string prefix = "tls-server-end-point:";
        byte[] prefixBytes = System.Text.Encoding.ASCII.GetBytes(prefix);
        var appData = new byte[prefixBytes.Length + hash.Length];
        System.Buffer.BlockCopy(prefixBytes, 0, appData, 0, prefixBytes.Length);
        System.Buffer.BlockCopy(hash, 0, appData, prefixBytes.Length, hash.Length);

        return new ChannelBindings(
            InitiatorAddrType: 0,
            InitiatorAddress: System.ReadOnlyMemory<byte>.Empty,
            AcceptorAddrType: 0,
            AcceptorAddress: System.ReadOnlyMemory<byte>.Empty,
            ApplicationData: appData);
    }

    private enum HashKind
    {
        Sha256,
        Sha384,
        Sha512,
    }

    private static HashKind SelectHashAlgorithmForCert(System.ReadOnlySpan<byte> certDer)
    {
        _ = certDer;

        // Future enhancement: parse the X.509 SignatureAlgorithm OID and select SHA-384/512 when required.
        return HashKind.Sha256;
    }
}
