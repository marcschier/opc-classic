# External OPC Classic vendor scenarios

This catalog drives OPC Classic interoperability probes from non-sensitive JSON descriptors. It never downloads, installs, registers, or commits vendor software.

## Catalogs

- `descriptors/matrikon-opc-simulation-server.json` and `descriptors/opc-foundation-testserver.json` describe externally installed DA products.
- `descriptors/generic-opc-classic-template.json` is the product-neutral template for a new DA, AE, or HDA server.
- `schemas/vendor-probe-catalog-v1.schema.json` fixes the versioned top-level shape; `tools/vendor_probe_catalog.py` performs fail-closed semantic and security validation.
- `descriptors/fixtures/*.hex` are tiny synthetic, redistributable fixtures only. No proprietary binaries or captures are present.

Each probe declares `requires`. Selection occurs only when every required capability is declared. The generic catalog covers DA optional-interface queries, group lifecycle/callbacks, deadband/sampling, browse/property variants, sync/async I/O, reconnect/failover; AE subscriptions, filters, returned attributes, refresh/cancel and condition-state variants; and HDA browser, raw/processed/modified reads, annotations, advise/playback, aggregates and relative time.

Malformed and truncated fixture decodes are expected failures. Synthetic vendor-extension fixtures are expected successes. Missing install-root mappings or external executables produce `BLOCKED`, never `REGRESSION`.

Normalized reports include descriptor/catalog versions, descriptor identity, product/vendor, target kind, capability IDs, probe ID, expected/actual results and verdict.

Every selected probe is executed independently, even when several probe IDs
map to the same MCP tool; `probeId` remains the stable report identity. DA write
expectations select the returned item by `expected.itemId` before comparing its
HRESULT, and a missing item is a regression. Descriptor parsing and JSON-RPC
dispatch reject `NaN`, positive infinity, and negative infinity.

To add a product, clone the generic descriptor, replace the placeholder target and item arguments, remove unsupported capabilities, and point prerequisites at `${OPERATOR_ROOT}` plus a relative artifact path. Do not add credentials, absolute customer paths, setup commands, or product files.
