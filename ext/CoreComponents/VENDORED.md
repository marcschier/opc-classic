# Vendored OPC Classic Core Components

This directory is a verbatim snapshot of the OPC Foundation
[OPC-Classic-CoreComponents](https://github.com/OPCFoundation/OPC-Classic-CoreComponents)
repository. It is checked into the Opc.Classic tree so that the
native C++ TestServer + TestClient + supporting proxy/stub DLLs can
be built without an external clone — see `docs/interop/testserver.md`
in the repo root for the end-to-end build, install, and run flow.

## License

OPC Foundation MIT License 1.00 — every C++ source file carries the
license header verbatim. The top-level `LICENSE.md` is the OPC
Foundation umbrella license; individual source files are the
authoritative grant for code reuse.

## Snapshot freshness

The snapshot was imported as of `version.txt` (1.0.0.1). Re-sync by
running (from the repo root):

```powershell
robocopy ..\OPC-Classic-CoreComponents ext\CoreComponents /MIR /XD build .git .vs .vscode
```

then commit. Do not modify files in this directory without
documenting the divergence — patches we apply locally belong in a
separate `tools/coreComponents-patches/` folder so the snapshot
remains a clean upstream mirror.

## What's in here

| Path | Purpose |
|------|---------|
| `Source/Test/TestServer/` | DA 2.05a Test Server (`OpcTestServer_x64.exe` / `_x86.exe`) — minimal 3-item address space used by the managed proxy interop tests. |
| `Source/Test/TestClient/` | 179-line native console exerciser that enumerates DA 2.0 servers via OpcEnum and calls `GetStatus`. |
| `Source/Shared/` | Sample server scaffolding (`OpcUtilityClasses`, `SampleServerClasses`, `SampleDevice`, `SampleServer205`) that TestServer derives from. |
| `Source/Common/` | OpcEnum (server enumerator), CategoryManager, and OPC Common proxy/stub. |
| `Source/DataAccess/`, `Source/AlarmsAndEvents/`, etc. | Per-spec IDLs + proxy/stub builds for the 8 OPC Classic specs. |
| `Source/Include/` | Shared headers (CATID GUIDs, error codes). |
| `CMakeLists.txt`, `cmake/` | CMake harness — `cmake -S . -B build/x64 -A x64 && cmake --build build/x64 --config Release`. |
| `docker/` | Windows Server Core 2022 + VS 2022 build-tools docker harness for reproducible builds without a local VS install. |
| `build.ps1`, `docker-build.ps1` | One-shot build entry points (native + docker). |
| `WiX/` | MSI installer scaffolding (optional). |

## Why mirror upstream verbatim

The Opc.Classic managed proxy/dispatcher must marshal bytes that match
the MIDL-generated stubs in this tree exactly. When a managed-vs-MIDL
disagreement arises, the MIDL format strings under
`Source/Common/ProxyStub/`, `Source/DataAccess/ProxyStub/`, etc. are
the authoritative reference — preserving the upstream layout lets us
diff against the public OPC Foundation tag without renaming gymnastics.
