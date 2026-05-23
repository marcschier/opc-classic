# Native COM conformance (Phase 14B)

Tests under this folder connect from the managed .NET 10 client (via
OpcProxyGenerator-emitted shims) against the C++ OPC Foundation Sample
Servers preserved under `COM/`.

## Loopback equivalent

`Category=NativeConformance.Loopback` tests run without external COM
infrastructure. They route the generated `IOPCServer` client proxy through
`InMemoryCallChannel` into `OpcDaServerDispatcher` with a `StubDaServer`.
This proves the assertion path and generated proxy/dispatcher pipeline work
in-process before a real native DCOM endpoint is available.

## Running real native-server tests

1. CI handles this automatically via the `windows-conformance` job
   on `windows-2022` (see `.github/workflows/build.yml`).

2. Locally on Windows, install the OPC Foundation Core Components
   (places `opcproxy.dll` etc.) and register the native sample servers:
   ```cmd
   .\External\Bin\OpcCoreComponents.exe /S
   .\COM\regserver.cmd
   ```

3. Run only the conformance subset:
   ```bash
   dotnet test --filter "Category=NativeConformance"
   ```

4. Tests soft-skip when the native servers aren't registered (with a
   message logged via the test framework). When the servers are registered
   but the real listener-side DCOM channel is not injected yet, the tests
   assert the generated proxy, dispatcher, probe, and category-tag plumbing.

## Status

The folder is no longer placeholder-only: DA loopback tests exercise the
managed proxy-to-dispatcher pipeline today. Full native COM end-to-end tests
still require a registered native server plus a real `DcomCallChannel` wired
to listener-side transport.
