<!-- SPDX-License-Identifier: MIT -->

# Legacy jcifs-port compatibility boundary

This checklist covers the `Common` compatibility wrappers only. All tracked wrappers are self-contained and the transitional package reference has been removed.

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

**Total deferred wrapper work**: complete. The Common compatibility boundary is clear and the external jcifs-port package is not required by the NTLMSSP message wrappers.

Current SMB work in `Opc.Classic.Dcom` is package-free: it provides its own SMB2 packet/state-machine types plus `Smb2RpcTransportAdapter` for the future `ncacn_np` handoff. Legacy DCOM code still references types under the `Opc.Classic.Dcom.Common.Ntlm` namespace, but those resolve to the in-tree compatibility shims in this repository (originally adapted from the jcifs-port codebase) rather than any external package.
