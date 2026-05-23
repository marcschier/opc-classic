<!-- SPDX-License-Identifier: EPL-1.0 -->

# SharpCifs.Std reimplementation status (toward N7.6 drop)

This checklist covers the `src/OpcClassic.Dcom/Common/` compatibility wrappers only. `SharpCifs.Std` must remain referenced until every delegating wrapper below is self-contained and the remaining non-wrapper SharpCifs usages have been removed by their owning milestones.

| Wrapper | Status | LOC needed |
|---|---|---|
| LegacyNdr.NdrException | ✅ self-contained | — |
| LegacyNdr.NdrFormat | ✅ self-contained | — |
| LegacyNdr.NdrCodec (NetworkDataRepresentation equivalent) | ✅ self-contained | — |
| LegacyNdr.NdrBuffer | ✅ self-contained | — |
| LegacyNdr.NdrObject | ✅ self-contained | — |
| Ntlm.Type1Message | ⏳ delegating | ~200 (per MS-NLMP §2.2.1.1) |
| Ntlm.Type2Message | ⏳ delegating | ~250 (per MS-NLMP §2.2.1.2) |
| Ntlm.Type3Message | ⏳ delegating | ~300 (per MS-NLMP §2.2.1.3) |
| Ntlm.NtlmMessage abstract base | ⏳ delegating | ~50 |

**Total deferred wrapper work**: ~800 LOC for the NTLMSSP message implementation before this boundary is clear. The package can be removed only after these rows show ✅ and repository-wide `SharpCifs.` references are gone.
