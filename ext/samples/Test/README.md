# OPC Foundation DA 2.05a TestServer + TestClient

Imported from the OPC Foundation
[OPC Classic Core Components](https://github.com/OPCFoundation/OPC-Classic-CoreComponents)
repository (`Source/Test/`) for use as a deterministic interop target
when validating the managed Opc.Classic stack.

See `docs/interop/testserver.md` in the repo root for the build and
install instructions and for the `mcp_driver.py --testserver` flag
that drives this server from our managed client.

The source files here track the upstream verbatim — when re-syncing
from upstream, copy the entire `Source/Test/` tree into `TestServer/`
and `TestClient/` without modification so the comparison stays clean.

## Files

- `TestServer/`
  - `COpcTestServer.{h,cpp}` — server class derived from `COpcDaServer`
    (provided by `ext/samples/Sample Server/Da/Core/`).
  - `COpcTestGroup.h` — group class derived from `COpcDaGroup`.
  - `OpcTestServer.{cpp,idl,rc,ico,config.xml}` — local-server entry
    point, MIDL IDL, resource bundle, and 3-item address-space config.
  - `StdAfx.{h,cpp}`, `resource.h` — precompiled-header stub and IDs.
- `TestClient/OpcTestClient.cpp` — 179-line CoCreate / GetStatus
  console exerciser.
- `docker/Dockerfile`, `docker/entrypoint.cmd` — upstream Windows
  Server Core docker harness for building the test binaries plus the
  proxy/stub DLLs without a local VS install.

## License

MIT, OPC Foundation. See `LICENSE.md` at the root of the upstream
repository.
