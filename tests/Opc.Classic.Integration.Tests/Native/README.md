# Native COM conformance

Tests under this folder connect from the managed .NET 10 client (via
OpcProxyGenerator-emitted shims) against the C++ OPC Foundation Sample
Servers preserved from the vendored OPC Foundation native samples.

## Loopback equivalent

`Category=NativeConformance.Loopback` tests run without external COM
infrastructure. They route the generated `IOPCServer` client proxy through
`InMemoryCallChannel` into `OpcDaServerDispatcher` with a `StubDaServer`.
This proves the assertion path and generated proxy/dispatcher pipeline work
in-process before a real native DCOM endpoint is available.

## Running real native-server tests

1. CI handles this automatically via the `windows-conformance` job
   on `windows-2022` (see `.github/workflows/build.yml`).

2. Locally on Windows, make the OPC Foundation Core Components available
   (places `opcproxy.dll` etc.; the redistributable installers are no longer
   vendored, so build/register `external` or install the
   official package externally), build the native `.vcxproj` sample servers as
   documented in the vendored native samples README, and register them:
   ```cmd
   .\interop\samples\regserver.cmd
   ```

3. Run only the conformance subset:
   ```bash
   dotnet test --filter "Category=NativeConformance"
   ```

4. Tests soft-skip when the native servers aren't registered (with a
   message logged via the test framework). When the servers are registered
   but no real `DcomCallChannel` factory is injected, the tests assert the
   generated proxy, dispatcher, probe, and category-tag plumbing.

## Verified native server identifiers

| Sample | ProgID | CLSID |
| --- | --- | --- |
| DA | `OPCSample.OpcDaServer.1` | `{625C49A1-BE1C-45D7-9A8A-14BEDCF5CE6C}` |
| AE | `OPCSample.OPCEventServer.1` | `{65168852-5783-11D1-84A0-00608CB8A7E9}` |
| HDA | `OPCSample.OpcHdaServer.1` | `{6A5EEDEC-1509-4627-997F-993CCB65AB7C}` |

## Status

The folder is populated: DA loopback tests exercise the
managed proxy-to-dispatcher pipeline today. Full native COM end-to-end tests
still require a registered native server plus a real `DcomCallChannel` wired
to listener-side transport.
