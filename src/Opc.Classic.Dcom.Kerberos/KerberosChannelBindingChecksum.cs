// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Kerberos.NET.Entities;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Builds the MS-KILE GSS-API authenticator checksum that carries channel bindings.
/// </summary>
public static class KerberosChannelBindingChecksum
{
    /// <summary>
    /// The KRB_AP_CHKSUM_TYPE_GSS checksum type value from MS-KILE §2.2.10.
    /// </summary>
    public const int KrbApChecksumTypeGss = 0x8003;

    /// <summary>
    /// Creates a KRB_AP_CHKSUM_TYPE_GSS checksum containing the RFC 2744 channel-bindings hash.
    /// </summary>
    /// <param name="channelBindingsHash">The 16-byte MD5 hash of the GSS channel-bindings structure.</param>
    /// <param name="flags">GSS context establishment flags to encode next to the channel binding.</param>
    /// <returns>The Kerberos authenticator checksum for the AP-REQ.</returns>
    public static KrbChecksum Create(ReadOnlyMemory<byte> channelBindingsHash, GssContextEstablishmentFlag flags)
    {
        if (channelBindingsHash.Length != 16)
        {
            throw new ArgumentException("The Kerberos channel-bindings hash must be exactly 16 bytes.", nameof(channelBindingsHash));
        }

        return KrbChecksum.EncodeDelegationChecksum(new DelegationInfo
        {
            ChannelBinding = channelBindingsHash,
            Flags = flags,
        });
    }
}
