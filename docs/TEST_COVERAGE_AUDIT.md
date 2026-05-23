# Test coverage audit and gap analysis

Snapshot: Phase 12 audit of the working tree test inventory and coverlet Cobertura output.
The inventory counts source `*.cs` files under each test project and excludes `bin/` and `obj/`.
The raw directory-based audit command can also report `tests/Opc.Classic.Dcom.Tests/Tests` as a nested `Tests` folder; it is not counted as a separate project below.

## Summary

| Test project | Tests |
|---|---:|
| Opc.Classic.Core.Tests | 233 |
| Opc.Classic.Da.Tests | 136 |
| Opc.Classic.Xml.Tests | 97 |
| Opc.Classic.Hda.Tests | 66 |
| Opc.Classic.Ae.Tests | 45 |
| Opc.Classic.Generators.Tests | 30 |
| Opc.Classic.Dcom.Tests | 24 |
| Opc.Classic.Integration.Tests | 16 |
| Opc.Classic.Dcom.Crypto.Tests | 15 |
| Opc.Classic.Security.Tests | 14 |
| Opc.Classic.Dcom.Kerberos.Tests | 12 |
| Opc.Classic.Batch.Tests | 12 |
| Opc.Classic.PropertyTests | 12 |
| Opc.Classic.Cpx.Tests | 11 |
| Opc.Classic.Hosting.Tests | 10 |
| Opc.Classic.Commands.Tests | 9 |
| Opc.Classic.Discovery.Tests | 8 |
| Opc.Classic.Dx.Tests | 7 |
| Opc.Classic.Dcom.Logging.Tests | 0 |
| **Total** | **757** |

All 15 `src/Opc.Classic.*` projects have a corresponding `tests/Opc.Classic.*.Tests` project. Two additional test projects cover cross-cutting work: `Opc.Classic.Integration.Tests` for Phase 13 plus 14B/C/D conformance work, and `Opc.Classic.PropertyTests` for CsCheck properties.

## Coverage seed

`dotnet test Opc.Classic.slnx --no-build -c Release --collect:"XPlat Code Coverage" --settings coverlet.runsettings` produced:

| Metric | Value |
|---|---:|
| Line coverage | 81.69% |
| Branch coverage | 69.73% |

The run emitted a `run failed` error from `Opc.Classic.Batch.Tests` after collection, so these numbers are a seed for gap prioritization rather than a green quality gate.

## Completeness against p12

### Opc.Classic.Dcom PDU round-trips

Dedicated source-level references to the DCOM PDU types were not found under `tests/Opc.Classic.Dcom.Tests`. Current coverage is therefore **0 of the requested 13 round-trip slots**. Required gaps are: `BindPdu`, `BindAcknowledgePdu`, `BindNoAcknowledgePdu`, `AlterContextPdu`, `AlterContextResponsePdu`, `Auth3Pdu`, `RequestCoPdu`, `ResponseCoPdu`, `FaultCoPdu`, `CancelCoPdu`, `OrphanedPdu`, `ShutdownPdu`, and the shared connection-oriented header/framing path.

Existing DCOM tests exercise activation records, default NTLM/protection flags, property bags, and reflection dispatch, but not byte-for-byte PDU encode/decode.

### NTLM and crypto

`Opc.Classic.Dcom.Crypto.Tests` covers RFC 1320 MD4 vectors, RFC 6229 RC4 vectors, RC4 inverse behavior, and MS-NLMP §4.2.4.1 NTLMv2 key-derivation vectors. It also checks valid/invalid NT proof handling and client/server signing-key agreement.

Remaining MS-NLMP vector gaps are §4.2.4.2 through §4.2.5, including alternate NTLMv2 samples, NTLM2-session/legacy compatibility vectors, MIC/AV-pair handling, and full sign/seal sequence-number message tests. HMAC-MD5 is covered indirectly through NTLMv2 derivation; there is no focused sign/seal round-trip vector suite yet.

### Kerberos and SPNEGO

`Opc.Classic.Dcom.Kerberos.Tests` validates auth-info construction, cancellation, AP-REP processing, SPNEGO token encode/decode, known-good `NegTokenResp`, and token-builder OIDs. `Opc.Classic.Integration.Tests` includes a KDC fixture smoke test and skipped loopback auth placeholders.

Phase 3D follow-up remains open for a successful Kerberos.NET in-memory KDC ticket acquisition path and DCOM auth integration. SPNEGO has token-shape tests, but not a full negotiation state-machine test that selects NTLMv2 versus Kerberos and verifies failure states.

### Opc.Classic.Core FILETIME

`FileTimeHelperTests` covers epoch, zero, one-second offsets, low/high word recomposition, UTC normalization, pre-epoch and negative guards, span decoding, span writing, and word round-trips. `Opc.Classic.PropertyTests` adds CsCheck FILETIME round-trip and low/high recomposition properties. This area is **covered for Phase 12**.

### Opc.Classic.Core OpcUrl fuzz

`OpcUrlTests` covers concrete schemes, CLSID parsing, query trimming, invalid schemes, missing server IDs, invalid ports, null input, equality, and `Parse` throwing behavior. `Opc.Classic.PropertyTests` adds CsCheck round-trip coverage across all schemes and port extraction. This area is **covered for Phase 12**.

### Per-spec shim tests

| Area | Test project | Tests | Assessment |
|---|---|---:|---|
| DA | Opc.Classic.Da.Tests | 136 | Strongest per-spec suite: codecs, interface IDs/contracts, hosting, subscriptions, and DCOM proxy shims. |
| AE | Opc.Classic.Ae.Tests | 45 | Result IDs, interface contracts, event/condition codecs; end-to-end event sink loopback is still skipped in integration. |
| HDA | Opc.Classic.Hda.Tests | 66 | Interface contracts, status/types, and NDR codecs; cancellation loopback remains skipped. |
| XML-DA | Opc.Classic.Xml.Tests | 97 | Serializer/client/server-state coverage; not part of original p12 shim list but mature. |
| Cpx | Opc.Classic.Cpx.Tests | 11 | Type and ID smoke coverage only. |
| DX | Opc.Classic.Dx.Tests | 7 | Type and ID smoke coverage only. |
| Batch | Opc.Classic.Batch.Tests | 12 | NDR summary/filter codecs and IDs; coverage run reported a Batch test-host failure. |
| Commands | Opc.Classic.Commands.Tests | 9 | Command type and interface ID coverage. |
| Security | Opc.Classic.Security.Tests | 14 | Interface IDs, logon request, impersonation, and security type coverage. |

## Identified gaps and recommendations

| Priority | Missing test class or suite | Recommendation |
|---|---|---|
| blocking 1.0 | `tests/Opc.Classic.Dcom.Tests/Rpc/Pdu/*RoundTripTests.cs` | Add byte fixtures and encode/decode equality for all 13 DCOM PDU slots before declaring the managed RPC layer stable. |
| blocking 1.0 | `NtlmMsNlmpVectorTests` | Add remaining MS-NLMP §4.2.4.2-§4.2.5 vectors, including negotiated flags, target-info AV pairs, MIC, and compatibility modes. |
| blocking 1.0 | `NtlmSignSealRoundTripTests` | Add RC4/HMAC-MD5 sign/seal round-trips with sequence-number mutation checks so message integrity regressions are caught. |
| blocking 1.0 | `KerberosInMemoryKdcTests` | Complete Phase 3D follow-up: acquire an AP-REQ from an in-memory Kerberos.NET KDC and validate the success path, not only expected KDC failure. |
| before 1.0 | `SpnegoNegotiationStateMachineTests` | Drive accept-incomplete, accept-complete, reject, fallback, and no-common-mechanism transitions across NTLMv2 and Kerberos. |
| before 1.0 | `Opc.Classic.Integration.Tests/Loopback/F4_Auth` | Unskip NTLMv2/Kerberos/SPNEGO loopback auth once Phase 3 auth wiring lands. |
| before 1.0 | AE/HDA loopback shim suites | Unskip event-delivery and HDA-cancellation integration tests when hosting contracts land. |
| before 1.0 | Cpx/DX/Batch/Commands shim contract suites | Expand from type/ID smoke tests to mocked `IComObject` call-shape and error-path assertions. |
| post-1.0 | `Opc.Classic.Dcom.Logging.Tests` | Either add logger/diagnostic assertions or remove/merge the zero-test project. |
| post-1.0 | Additional CsCheck fuzzers | Add malformed URL, NDR boundary, HRESULT, and PDU-fragment property tests after the blocking deterministic vectors are in place. |

Phase 12 can close as an aggregate audit item: the main shipping gaps are now explicit, scoped, and ready to become follow-up todos as their dependency phases land.
