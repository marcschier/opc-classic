<!-- Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License. -->

# NTLM compatibility boundary

This document describes the `Common` compatibility wrappers that the NTLM
implementation depends on.

| Wrapper | Status |
| --- | --- |
| LegacyNdr.NdrException | ✅ self-contained |
| LegacyNdr.NdrFormat | ✅ self-contained |
| LegacyNdr.NdrCodec (NetworkDataRepresentation equivalent) | ✅ self-contained |
| LegacyNdr.NdrBuffer | ✅ self-contained |
| LegacyNdr.NdrObject | ✅ self-contained |
| Ntlm.Type1Message | ✅ self-contained |
| Ntlm.Type2Message | ✅ self-contained |
| Ntlm.Type3Message | ✅ self-contained |
| Ntlm.NtlmMessage abstract base | ✅ self-contained |

The Common compatibility boundary is clear and no external NTLM/SMB package
is required by the NTLMSSP message wrappers.

The current SMB work in `Opc.Classic.Dcom` is package-free: it provides its
own SMB2 packet/state-machine types plus `Smb2RpcTransportAdapter` for the
`ncacn_np` handoff. Legacy DCOM code references types under the
`Opc.Classic.Dcom.Common.Ntlm` namespace; those resolve to the in-tree
compatibility shims in this repository.
