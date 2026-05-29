<!-- SPDX-License-Identifier: MIT -->

# SharpCifs.Std compatibility boundary

This checklist covers the `src\Opc.Classic.Dcom\Common\` compatibility wrappers only. All tracked wrappers are self-contained and the transitional package reference has been removed.

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

**Total deferred wrapper work**: complete. The Common compatibility boundary is clear and `SharpCifs.Std` is no longer required by the NTLMSSP message wrappers.

Current SMB work in `src\Opc.Classic.Dcom.Smb\` is SharpCifs-free: it provides its own SMB2 packet/state-machine types plus `Smb2RpcTransportAdapter` for the future `ncacn_np` handoff. Some legacy DCOM code still references `SharpCifs.*` namespaces, but those names resolve to in-tree compatibility shims in this repository rather than the removed package.
