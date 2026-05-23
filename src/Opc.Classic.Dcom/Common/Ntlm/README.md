# Opc.Classic.Dcom.Internal.Ntlm

N7.6 completes the local NTLMSSP message implementation. `NtlmFlags`, `NtlmMessage`, `Type1Message`, `Type2Message`, and `Type3Message` live under `Opc.Classic.Dcom.Internal.Ntlm` and serialize/parse NEGOTIATE, CHALLENGE, and AUTHENTICATE messages directly per MS-NLMP §2.2.1.

The message wrappers are self-contained and no longer delegate to `SharpCifs.Std`; response cryptography remains in `rpc/Auth/Responses.cs` and the in-tree crypto primitives.
