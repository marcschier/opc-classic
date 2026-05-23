// SPDX-License-Identifier: EPL-1.0

namespace OpcClassic.Dcom.Internal.Ntlm;

[System.Flags]
public enum NtlmFlags : uint
{
    None = 0,
    NtlmsspNegotiateUnicode = 0x00000001,
    NtlmsspNegotiateOem = 0x00000002,
    NtlmsspRequestTarget = 0x00000004,
    NtlmsspNegotiateSign = 0x00000010,
    NtlmsspNegotiateSeal = 0x00000020,
    NtlmsspNegotiateDatagram = 0x00000040,
    NtlmsspNegotiateDatagramStyle = NtlmsspNegotiateDatagram,
    NtlmsspNegotiateLanManagerKey = 0x00000080,
    NtlmsspNegotiateLmKey = NtlmsspNegotiateLanManagerKey,
    NtlmsspNegotiateNetware = 0x00000100,
    NtlmsspNegotiateNtlm = 0x00000200,
    NtlmsspNegotiateAnonymous = 0x00000800,
    NtlmsspNegotiateOemDomainSupplied = 0x00001000,
    NtlmsspNegotiateOemWorkstationSupplied = 0x00002000,
    NtlmsspNegotiateLocalCall = 0x00004000,
    NtlmsspNegotiateAlwaysSign = 0x00008000,
    NtlmsspTargetTypeDomain = 0x00010000,
    NtlmsspTargetTypeServer = 0x00020000,
    NtlmsspTargetTypeShare = 0x00040000,
    NtlmsspNegotiateExtendedSessionSecurity = 0x00080000,
    NtlmsspNegotiateNtlm2 = NtlmsspNegotiateExtendedSessionSecurity,
    NtlmsspNegotiateIdentify = 0x00100000,
    NtlmsspRequestInitResponse = NtlmsspNegotiateIdentify,
    NtlmsspRequestAcceptResponse = 0x00200000,
    NtlmsspRequestNonNtSessionKey = 0x00400000,
    NtlmsspNegotiateTargetInfo = 0x00800000,
    NtlmsspNegotiateVersion = 0x02000000,
    NtlmsspNegotiate128 = 0x20000000,
    NtlmsspNegotiateKeyExch = 0x40000000,
    NtlmsspNegotiate56 = 0x80000000,
}
