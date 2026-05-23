# OpcClassic.Dcom.Internal.Ntlm

N7.4 introduces local NTLMSSP message types so OpcClassic.Dcom call sites no longer reference `SharpCifs.Ntlmssp` directly.

These types are a transitional type-forwarding shim: `NtlmFlags`, `NtlmMessage`, `Type1Message`, `Type2Message`, and `Type3Message` live under `OpcClassic.Dcom.Internal.Ntlm`, while message parsing and serialization still delegate to `SharpCifs.Std` internally. This keeps Type1/Type2/Type3 byte output identical during Phase 2D.4.

Follow-up work replaces the forwarding internals with a self-contained MS-NLMP implementation and removes the remaining `SharpCifs.Std` runtime dependency after the Dcerpc/NDR migration lands.
