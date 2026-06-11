# Opc.Classic.Dcom.Internal.Ntlm

The local NTLMSSP message implementation lives under `Opc.Classic.Dcom.Internal.Ntlm`. `NtlmFlags`, `NtlmMessage`, `Type1Message`, `Type2Message`, and `Type3Message` serialize/parse NEGOTIATE, CHALLENGE, and AUTHENTICATE messages directly per MS-NLMP §2.2.1.

The message wrappers are self-contained and do not delegate to `SharpCifs.Std`; response cryptography lives in `Responses` and the in-tree crypto primitives. MIC, channel-binding, SPNEGO MIC provider, and message-signature helpers are part of the same audit surface.

For security review scope, abuse-test guidance, and current coverage notes, see `docs\security\NTLMSSP_AUDIT_GUIDE.md`.
