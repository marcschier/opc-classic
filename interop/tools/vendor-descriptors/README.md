# External OPC Classic vendor scenarios

This catalog drives OPC Classic interoperability probes from non-sensitive JSON descriptors. It never downloads, installs, registers, or commits vendor software.

Descriptors are test metadata, not product redistributions or endorsements.
Every external server, client, SDK, installer, license, registration, and
credential remains operator-supplied and subject to its vendor's terms.

## Catalogs

- `descriptors/matrikon-opc-simulation-server.json` and `descriptors/opc-foundation-testserver.json` describe externally installed DA products.
- `descriptors/generic-opc-classic-template.json` is the product-neutral template for a new DA, AE, or HDA server.
- `schemas/vendor-probe-catalog-v1.schema.json` fixes the versioned top-level shape; `tools/vendor_probe_catalog.py` performs fail-closed semantic and security validation.
- `descriptors/fixtures/*.hex` are tiny synthetic, redistributable fixtures only. No proprietary binaries or captures are present.

Each probe declares `requires`. Selection occurs only when every required capability is declared. The generic catalog covers DA optional-interface queries, group lifecycle/callbacks, deadband/sampling, browse/property variants, sync/async I/O, reconnect/failover; AE subscriptions, filters, returned attributes, refresh/cancel and condition-state variants; and HDA browser, raw/processed/modified reads, annotations, advise/playback, aggregates and relative time.

Malformed and truncated fixture decodes are expected failures. Synthetic
vendor-extension fixtures are expected successes. Fixture probes run
specification-specific DA extension, AE condition-state, or HDA relative-time
decoders after hex parsing; valid hex with an invalid protocol shape fails.
Missing install-root mappings or external executables produce `BLOCKED`, never
an external-product `REGRESSION`.

Normalized reports include descriptor/catalog versions, descriptor identity, product/vendor, target kind, capability IDs, probe ID, allow-listed expected/actual comparison metadata, normalized expectation codes, and verdict.

Every selected probe produces exactly one result row, even when several probe IDs map to the same MCP tool. An unavailable MCP tool produces an explicit failure row, and an unmapped selected probe produces `PROBE_MAPPING_MISSING`/`REGRESSION`; neither is silently omitted. `probeId` remains the stable report identity. Reconnect and failover probes are real multi-step workflows: they reset the connection, connect, disconnect, reconnect, and perform a status or browse follow-up. DA write expectations select the returned item by `expected.itemId` before comparing its HRESULT, and a missing item is a regression. Tool result objects with `success`/`succeeded` set to false or a failed/error/canceled status also fail the probe even when the MCP call itself returned without throwing. Gated Matrikon and TestServer plans contain only probes with executable mappings.

To add a product, clone the generic descriptor, replace the placeholder target and item arguments, remove unsupported capabilities, and point prerequisites at `${OPERATOR_ROOT}` plus a relative artifact path. Do not add credentials, absolute customer paths, setup commands, or product files.

## Security rules

- Descriptor identity, schema version, target kind, capability names, probe
  references, expected results, and prerequisite paths are validated
  fail-closed.
- The Python loader enforces the checked-in JSON schema recursively, including
  nested `required`, `additionalProperties`, type, range, pattern, and array
  constraints. Duplicate properties and all non-finite numbers are rejected.
- Artifact prerequisites use an allow-listed root token plus a relative path.
  Absolute paths, parent traversal, environment expansion inside paths, setup
  commands, and executable arguments from descriptors are rejected. The final
  resolved artifact and fixture paths must remain beneath their verified roots,
  including after symlink resolution.
- Each selected probe runs independently. A missing external installation or
  root mapping is `BLOCKED`; it is not converted into a product regression.
- Matrix reports persist only allow-listed comparison fields and descriptor metadata by default. Raw arguments, OPC values, payload-derived fields, full errors, local paths, expected item IDs, free-form expectation failures, and free-form aggregate regression details are omitted;
  aggregate regression rows are recursively reduced to fixed identity,
  normalized-code, outcome, and numeric count fields. Use
  `run-cross-impl-matrix.ps1 -IncludeSensitiveResults` (or Python
  `--include-sensitive-results`) only when the resulting artifacts will be
  handled as sensitive data. Wire captures remain a separate explicit opt-in.

For the `testserver` profile, `OPC_TESTSERVER_INSTALL_ROOT` can be supplied by
the operator. When it is absent, the matrix accepts only a verified existing
`LocalServer32` executable and derives the install root from its parent
directory. The self-hosted workflow exports that verified root for subsequent
matrix steps.

## Licensing boundary

The schema, loader, synthetic fixtures, and repository-owned descriptors are
covered by this repository's MIT license. Product names and marks identify
external interoperability targets only. Vendor binaries, SDKs, captures,
licenses, and documentation are not included and are not granted any rights by
this repository. Operators are responsible for acquisition, license
compliance, installation, registration, access control, and removal.

The Matrikon and OPC Foundation descriptors contain only non-sensitive target
metadata and expected probe shapes. Synthetic fixture bytes are
repository-authored and redistributable; proprietary captures must not be
committed.
