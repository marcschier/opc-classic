//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Opc.Classic.Security;

/// <summary>
/// Convenience factory for channel-bindings structures that carry TLS
/// endpoint application data.
/// </summary>
public static class ChannelBindingsFactory
{
    private const string TlsServerEndpointPrefix = "tls-server-end-point:";

    /// <summary>
    /// Per MS-CSSP §2.1.1.2: the tls-server-end-point application data is
    /// the literal prefix "tls-server-end-point:" followed by the certificate
    /// hash selected by RFC 5929, with TLS 1.3 endpoints using SHA-384.
    /// </summary>
    public static ChannelBindings ForTlsServerEndpoint(ReadOnlySpan<byte> serverCertDer) =>
        ForTlsServerEndpoint(serverCertDer, SslProtocols.None);

    /// <summary>
    /// Creates TLS server-end-point channel bindings from a DER-encoded server certificate
    /// and the negotiated TLS protocol.
    /// </summary>
    public static ChannelBindings ForTlsServerEndpoint(ReadOnlySpan<byte> serverCertDer, SslProtocols sslProtocol)
    {
        var hashKind = SelectHashAlgorithmForCert(serverCertDer, sslProtocol);
        byte[] hash = hashKind switch
        {
            HashKind.Sha256 => SHA256.HashData(serverCertDer),
            HashKind.Sha384 => SHA384.HashData(serverCertDer),
            HashKind.Sha512 => SHA512.HashData(serverCertDer),
            _ => throw new NotSupportedException("Certificate signature hash algorithm is not supported."),
        };

        return CreateFromApplicationData(BuildTlsServerEndpointApplicationData(hash));
    }

    /// <summary>
    /// Creates TLS server-end-point channel bindings from an X.509 server certificate
    /// and the negotiated TLS protocol.
    /// </summary>
    public static ChannelBindings ForTlsServerEndpoint(X509Certificate2 serverCertificate, SslProtocols sslProtocol = SslProtocols.None)
    {
        ArgumentNullException.ThrowIfNull(serverCertificate);
        return ForTlsServerEndpoint(serverCertificate.RawData, sslProtocol);
    }

    /// <summary>
    /// Extracts the remote certificate from an authenticated <see cref="SslStream" />
    /// and creates TLS server-end-point channel bindings for that TLS session.
    /// </summary>
    public static ChannelBindings ForTlsServerEndpoint(SslStream sslStream)
    {
        ArgumentNullException.ThrowIfNull(sslStream);
        if (!sslStream.IsAuthenticated)
        {
            throw new InvalidOperationException("The SslStream must be authenticated before channel bindings are computed.");
        }

        var remoteCertificate = sslStream.RemoteCertificate ?? throw new InvalidOperationException(
            "The authenticated SslStream did not expose a remote certificate.");
        return ForTlsServerEndpoint(remoteCertificate.Export(X509ContentType.Cert), sslStream.SslProtocol);
    }

    private enum HashKind
    {
        Sha256,
        Sha384,
        Sha512,
    }

    private static ChannelBindings CreateFromApplicationData(byte[] applicationData) =>
        new(
            InitiatorAddrType: 0,
            InitiatorAddress: ReadOnlyMemory<byte>.Empty,
            AcceptorAddrType: 0,
            AcceptorAddress: ReadOnlyMemory<byte>.Empty,
            ApplicationData: applicationData);

    private static byte[] BuildTlsServerEndpointApplicationData(byte[] certificateHash)
    {
        byte[] prefixBytes = Encoding.ASCII.GetBytes(TlsServerEndpointPrefix);
        var appData = new byte[prefixBytes.Length + certificateHash.Length];
        Buffer.BlockCopy(prefixBytes, 0, appData, 0, prefixBytes.Length);
        Buffer.BlockCopy(certificateHash, 0, appData, prefixBytes.Length, certificateHash.Length);
        return appData;
    }

    private static HashKind SelectHashAlgorithmForCert(ReadOnlySpan<byte> certDer, SslProtocols sslProtocol)
    {
        if (sslProtocol == SslProtocols.Tls13)
        {
            return HashKind.Sha384;
        }

        try
        {
            using var certificate = X509CertificateLoader.LoadCertificate(certDer);
            return SelectHashAlgorithmForSignatureOid(certificate.SignatureAlgorithm.Value);
        }
        catch (CryptographicException)
        {
            return HashKind.Sha256;
        }
    }

    private static HashKind SelectHashAlgorithmForSignatureOid(string? oid) =>
        oid switch
        {
            "1.2.840.113549.1.1.13" => HashKind.Sha512, // sha512WithRSAEncryption
            "1.2.840.10045.4.3.4" => HashKind.Sha512, // ecdsa-with-SHA512
            "2.16.840.1.101.3.4.3.4" => HashKind.Sha512, // dsa-with-sha512
            "1.2.840.113549.1.1.12" => HashKind.Sha384, // sha384WithRSAEncryption
            "1.2.840.10045.4.3.3" => HashKind.Sha384, // ecdsa-with-SHA384
            "2.16.840.1.101.3.4.3.3" => HashKind.Sha384, // dsa-with-sha384
            _ => HashKind.Sha256,
        };
}
