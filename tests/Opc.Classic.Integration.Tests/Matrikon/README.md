# Matrikon OPC Simulation conformance

Tests under this folder target the Matrikon OPC Simulation Server -
the canonical Windows DCOM OPC reference implementation used by every
OPC client developer for interop validation.

## Tag tree

The Matrikon Simulation Server exposes a well-known tag tree:

- `Bucket Brigade.*` - writable tags whose value is whatever the client wrote
- `Random.*` - read-only tags returning a random value of the appropriate type
- `Saw-toothed Waves.*`, `Square Waves.*`, `Triangle Waves.*` - periodic-signal tags
- `Read Error.*`, `Write Error.*` - error-simulator tags (return OPC_E_BADRIGHTS etc.)

See https://www.matrikonopc.com/products/opc-desktop-tools/opc-simulation-server.aspx

## Loopback equivalent

`Category=MatrikonConformance.Loopback` tests run without Matrikon installed.
They simulate the well-known tags in `StubDaServer`, route the generated
`IOPCServer` client proxy through `InMemoryCallChannel` into
`OpcDaServerDispatcher`, and assert the typed status/error responses plus tag
catalog assumptions.

## Running real Matrikon tests

Matrikon Simulation Server is distributed by Matrikon (now Honeywell).
Free download requires registration at matrikonopc.com.

The `windows-conformance` CI job does NOT install Matrikon
(would require a CI secret holding the installer URL). To enable
matrikon-conformance in CI, add a `matrikon-installer-url` secret and
update the workflow to download + install before running the
matrikon-conformance test filter.

Locally on Windows, install Matrikon, then:
```bash
dotnet test --filter "Category=MatrikonConformance"
```

Tests soft-skip when Matrikon isn't installed. When Matrikon is present but
no real `DcomCallChannel` factory is injected, the tests assert the generated
proxy, probe, category-tag, and tag-tree plumbing.

## Status

The folder now has loopback-backed assertions for the generated proxy and DA
server dispatcher. Full Matrikon end-to-end tests still require the installer,
license/registration, and an injected real `DcomCallChannel` connection to the server.
