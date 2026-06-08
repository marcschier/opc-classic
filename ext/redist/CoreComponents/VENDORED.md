# Vendored OPC Classic Core Components

This directory is a pruned and restructured vendoring of the OPC Foundation
[OPC-Classic-CoreComponents](https://github.com/OPCFoundation/OPC-Classic-CoreComponents)
repository. It is checked into the Opc.Classic tree so that the
native C++ TestServer + TestClient + supporting proxy/stub DLLs can
be built without an external clone — see `docs/interop/testserver.md`
in the repo root for the end-to-end build, install, and run flow.

Local restructuring removes upstream `.github/` and `WiX/`, renames `Source/`
to `src/`, and relocates native test applications to repo-level
`samples/OpcTestServer/` and `samples/OpcTestClient/`.

## License

OPC Foundation MIT License 1.00 — every C++ source file carries the
license header verbatim. The top-level `LICENSE.md` is the OPC
Foundation umbrella license; individual source files are the
authoritative grant for code reuse.

## Snapshot freshness

The snapshot was imported as of `version.txt` (1.0.0.1). Re-sync by
running (from the repo root):

```powershell
robocopy ..\OPC-Classic-CoreComponents ext\redist\CoreComponents /MIR /XD build .git .github .vs .vscode WiX
```

then re-apply the documented local restructuring before committing. Do not
modify files in this directory without documenting the divergence — patches we
apply locally belong in a separate `tools/coreComponents-patches/` folder.

### Local divergences from upstream

The following deliberate patches diverge from upstream and must be
re-applied after every `robocopy /MIR` re-sync. Each is tagged with
the originating Track ID and documented in-place via comments.

| File | Track | What changed |
|------|-------|--------------|
| `samples/OpcTestServer/COpcTestServer.h` | AB5 | Added `OPC_INTERFACE_ENTRY(IOPCBrowse)` and `OPC_INTERFACE_ENTRY(IOPCItemIO)` so the managed Opc.Classic client can exercise DA 3.0 against TestServer (the base class `COpcDaServer` implements both — upstream just doesn't expose them in TestServer's interface table). |
| `samples/OpcTestServer/OpcTestServer.cpp` | AB5 | Added `OPC_CATEGORY_TABLE_ENTRY(..., CATID_OPCDAServer30, OPC_CATEGORY_DESCRIPTION_DA30)` for both the x64 and x86 entry points so OPCEnum / discovery tools list TestServer as DA 3.0-capable. |
| `samples/OpcTestClient/OpcTestClient.cpp` | AB6 | Added `RunLifecycle()` + `ConnectAndExercise()` so the native client mirrors the managed `mcp_driver.py --testserver` flow (`AddGroup` → `AddItems(Test.Int32, Test.Float, Test.String)` → `SyncRead` → `SyncWrite Test.Int32=100` → verify → `RemoveItems` → `RemoveGroup`). TestServer + TestClient now form an in-tree symmetric loopback. |

## What's in here

| Path | Purpose |
|------|---------|
| `samples/OpcTestServer/` | DA 2.05a Test Server (`OpcTestServer_x64.exe` / `_x86.exe`) — minimal 3-item address space used by the managed proxy interop tests. |
| `samples/OpcTestClient/` | Native console exerciser that enumerates DA 2.0 servers via OpcEnum and calls `GetStatus` plus the repo loopback lifecycle. |
| `src/Shared/` | Sample server scaffolding (`OpcUtilityClasses`, `SampleServerClasses`, `SampleDevice`, `SampleServer205`) that TestServer derives from. |
| `src/Common/` | OpcEnum (server enumerator), CategoryManager, and OPC Common proxy/stub. |
| `src/DataAccess/`, `src/AlarmsAndEvents/`, etc. | Per-spec IDLs + proxy/stub builds for the 8 OPC Classic specs. |
| `src/Include/` | Shared headers (CATID GUIDs, error codes). |
| `CMakeLists.txt`, `cmake/` | CMake harness — `cmake -S . -B build/x64 -A x64 && cmake --build build/x64 --config Release`. |
| `docker/` | Windows Server Core 2022 + VS 2022 build-tools docker harness for reproducible builds without a local VS install. |
| `build.ps1`, `docker-build.ps1` | One-shot build entry points (native + docker). |
## Why vendor upstream sources

The Opc.Classic managed proxy/dispatcher must marshal bytes that match
the MIDL-generated stubs in this tree exactly. When a managed-vs-MIDL
disagreement arises, the MIDL format strings under
`src/Common/ProxyStub/`, `src/DataAccess/ProxyStub/`, etc. are the
authoritative reference. The local path restructuring keeps build outputs and
repo-level sample applications aligned with the Opc.Classic test fleet while
preserving the upstream source contents needed for conformance comparisons.
