//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Security.Authentication;

namespace Opc.Classic.Security;

/// <summary>
/// Computes the 16-byte MD5 hash of the GSS-API channel-bindings struct
/// for embedding in NTLMv2's MsvAvChannelBindings AV-pair or Kerberos's
/// KRB_AP_CHKSUM_TYPE_GSS authenticator checksum.
/// </summary>
public static class ChannelBindingsHash
{
    /// <summary>
    /// Serializes a <see cref="ChannelBindings" /> struct per RFC 2744 §3.11 and
    /// returns its MD5 hash. The wire layout is:
    /// <list type="bullet">
    /// <item><description>UINT32 initiatorAddrType</description></item>
    /// <item><description>UINT32 initiatorAddressLength</description></item>
    /// <item><description>BYTE[] initiatorAddress</description></item>
    /// <item><description>UINT32 acceptorAddrType</description></item>
    /// <item><description>UINT32 acceptorAddressLength</description></item>
    /// <item><description>BYTE[] acceptorAddress</description></item>
    /// <item><description>UINT32 applicationDataLength</description></item>
    /// <item><description>BYTE[] applicationData</description></item>
    /// </list>
    /// All UINT32s are little-endian.
    /// </summary>
    public static byte[] Compute(ChannelBindings bindings)
    {
        System.ArgumentNullException.ThrowIfNull(bindings);

        int totalLen = checked(4 + 4 + bindings.InitiatorAddress.Length
                             + 4 + 4 + bindings.AcceptorAddress.Length
                             + 4 + bindings.ApplicationData.Length);
        var buffer = new byte[totalLen];
        int offset = 0;

        void WriteU32(uint value)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), value);
            offset += 4;
        }

        WriteU32((uint)bindings.InitiatorAddrType);
        WriteU32((uint)bindings.InitiatorAddress.Length);
        bindings.InitiatorAddress.Span.CopyTo(buffer.AsSpan(offset));
        offset += bindings.InitiatorAddress.Length;

        WriteU32((uint)bindings.AcceptorAddrType);
        WriteU32((uint)bindings.AcceptorAddress.Length);
        bindings.AcceptorAddress.Span.CopyTo(buffer.AsSpan(offset));
        offset += bindings.AcceptorAddress.Length;

        WriteU32((uint)bindings.ApplicationData.Length);
        bindings.ApplicationData.Span.CopyTo(buffer.AsSpan(offset));

#pragma warning disable CA5351 // MS-NLMP/MS-CSSP require MD5 for the channel-bindings checksum.
        return System.Security.Cryptography.MD5.HashData(buffer);
#pragma warning restore CA5351
    }

    /// <summary>
    /// Computes the NTLM/Kerberos channel-bindings hash directly from a
    /// DER-encoded TLS server certificate.
    /// </summary>
    public static byte[] ForTlsServerCert(System.ReadOnlySpan<byte> serverCertDer) =>
        Compute(ChannelBindingsFactory.ForTlsServerEndpoint(serverCertDer));

    /// <summary>
    /// Computes the NTLM/Kerberos channel-bindings hash directly from a
    /// DER-encoded TLS server certificate and negotiated TLS protocol.
    /// </summary>
    public static byte[] ForTlsServerCert(System.ReadOnlySpan<byte> serverCertDer, SslProtocols sslProtocol) =>
        Compute(ChannelBindingsFactory.ForTlsServerEndpoint(serverCertDer, sslProtocol));
}
