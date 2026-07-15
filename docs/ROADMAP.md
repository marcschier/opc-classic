# Opc.Classic Roadmap

Forward-looking gates and remaining gaps for the first stable release and later
compatibility work. Current capabilities are documented in
[CONFORMANCE.md](CONFORMANCE.md), [ARCHITECTURE.md](ARCHITECTURE.md), and the
per-spec reviews under [conformance/](conformance/).

## Stable-release gates

- **Live Windows Server / Active Directory verification.** Exercise NTLMv2,
  direct Kerberos, and Kerberos-first SPNEGO listener authentication against a
  real AD environment, including SPN/keytab rotation, packet integrity/privacy,
  explicit NTLM fallback policy, `mechListMIC`, and channel-binding enforcement.
- **Independent authentication and cryptography review.** Review the in-tree
  NTLMv2, MD4, RC4, signing/sealing, channel-binding, SPNEGO policy, credential
  lifetime, and capture auth-unwrapping boundaries. See
  [security/NTLMSSP_AUDIT_GUIDE.md](security/NTLMSSP_AUDIT_GUIDE.md).
- **Windows native/container fleet execution and triage.** Run the complete
  managed/native fleet under [interop/docker/](../interop/docker/) on supported
  self-hosted Windows infrastructure and resolve release-blocking differences.
- **Stable compatibility matrix.** Record repeatable DA/AE/HDA results for the
  Matrikon and OPC Foundation baselines plus at least one additional external
  vendor scenario using the descriptor-driven probe catalog.

## Remaining runtime and conformance gaps

### DA and COM hosting

- Register the top-level DA 3.0 `IOPCItemIO` implementation on the default
  managed OPCServer host.
- Expose the OPCServer-level `IConnectionPointContainer` for `IOPCShutdown` and
  complete the Windows CCW `IOPCCommon` locale/error-string methods.
- Keep optional public-group management (`IOPCServerPublicGroups` and
  `IOPCPublicGroupStateMgt`) and legacy DA 1.x `IDataObject` callbacks as
  compatibility work driven by adopter demand.
- Expand external native-client coverage for Windows CCW edge cases and
  client-side per-group connection-point binding.

### Authentication and authorization

- Add host-level authorization policy that evaluates the authenticated
  principal together with CLSID, IID, opnum, and application resource scope.
- Add structured authentication and per-call audit events with correlation
  identifiers and redaction guarantees.
- Complete live-realm replay, clock-skew, credential-rotation, and cross-realm
  validation for the Kerberos acceptor.

### OPC DX

- Provide a generic DA-visible DX Database subtree with standardized status,
  connection, and source-server branches.
- Complete DirtyFlag/`E_PERSISTING` save timing, subscription-driven queue
  semantics, conversion policy, the full section 6 target-write truth table,
  and XML-DA source/service mapping.

### OPC Complex Data and XML-DA

- Add explicitly selected vendor CPX type-system codecs without guessing
  unknown payload formats.
- Broaden XML optional-element and vendor-carrier coverage while preserving the
  converter/filter bounds.
- Expand XML-DA serializer coverage for uncommon SOAP and vendor payload shapes.

### Capture and interoperability tooling

- Extend external trace decoding beyond Ethernet IPv4/IPv6 TCP where operator
  scenarios require additional link types or encapsulations.
- Add broader real-capture corpora for fragmented, retransmitted, overlapping,
  and protected DCOM traffic from external vendors.
- Keep vendor software acquisition, installation, registration, credentials,
  and licensing outside the repository; descriptors remain non-sensitive
  operator-supplied metadata.

## Post-stable extensions

- Additional OPC extensions such as Web-DA or future published compliance
  profiles.
- More native-server interoperability fixtures for vendor-specific DA, AE, and
  HDA behavior.
- Follow-up hardening required by independent security review findings.
