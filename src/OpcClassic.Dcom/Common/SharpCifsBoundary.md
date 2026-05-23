<!-- SPDX-License-Identifier: EPL-1.0 -->

# SharpCifs.Std reimplementation status (toward N7.6 drop)

This checklist covers the `src/OpcClassic.Dcom/Common/` compatibility wrappers only. All tracked wrappers are now self-contained and the transitional package reference has been removed.

| Wrapper | Status | LOC needed |
|---|---|---|
| LegacyNdr.NdrException | ✅ self-contained | — |
| LegacyNdr.NdrFormat | ✅ self-contained | — |
| LegacyNdr.NdrCodec (NetworkDataRepresentation equivalent) | ✅ self-contained | — |
| LegacyNdr.NdrBuffer | ✅ self-contained | — |
| LegacyNdr.NdrObject | ✅ self-contained | — |
| Ntlm.Type1Message | ✅ self-contained | — |
| Ntlm.Type2Message | ✅ self-contained | — |
| Ntlm.Type3Message | ✅ self-contained | — |
| Ntlm.NtlmMessage abstract base | ✅ self-contained | — |

**Total deferred wrapper work**: complete. The Common compatibility boundary is clear and `SharpCifs.Std` is no longer required by the NTLMSSP message wrappers.
