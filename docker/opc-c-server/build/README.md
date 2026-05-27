# `opc-c-server` build assets

This folder contains the modern Visual Studio project files that compile the
hand-rolled native OPC DA smoke server into `opc_exe.exe`.

## Current status: hand-rolled MVP is wired up

A hand-rolled ~500-line MVP is shipped at `opc-sample-server.cpp` with matching
`opc-sample-server.vcxproj` and `opc-sample-server.sln`. The Dockerfile now
builds it with MSBuild and emits `C:/out/opc_exe.exe`; use
`docker compose up c-server` on a Windows Docker host for smoke validation.

## TODO: full Batch sample conversion

The original full-sample route remains useful if the native image needs Batch
coverage beyond the DA smoke server. The vendored `.dsp` files (`opc_exe.dsp`,
`opc_dll.dsp`) are Visual Studio 6 project files; `msbuild` cannot consume them
directly. Conversion path:

1. **Inventory**: list all `.cpp` / `.c` / `.h` sources referenced by each
   `.dsp` (66 sources for `opc_exe.dsp`; ~50 for `opc_dll.dsp`).
2. **Hand-author `opc-c-server.vcxproj`** with:
   - `<ConfigurationType>Application</ConfigurationType>` for `opc_exe`;
     `DynamicLibrary` for `opc_dll`.
   - `<CharacterSet>Unicode</CharacterSet>` (sample sources use `wchar_t`
     throughout but some `.cpp` files lack the `_UNICODE` define).
   - Include paths: `External/Include` + the sample directory.
   - Link: `ole32.lib`, `oleaut32.lib`, `advapi32.lib`, `uuid.lib`, `ws2_32.lib`.
   - Possible MFC dependency: some sample files (`BatchItemSupport.h/.cpp`)
     contain MFC references (`#include <afxwin.h>`). The MFC component is an
     OPTIONAL VS workload not present in the
     `mcr.microsoft.com/dotnet/framework/sdk:4.8.1-windowsservercore-ltsc2022`
     image. Either:
       (a) install MFC via the VS Build Tools workload at image build time
           (substantial size increase), or
       (b) strip the MFC-dependent code from the build set (likely loses
           the in-process diag UI but server core functionality remains).
3. **MIDL regen** (optional): the sample directory ships pre-generated
   `opcbc_i.c`, `opcda_i.c`, etc. from an older MIDL. They likely compile
   under modern MSVC as-is; if not, regenerate from the matching `.idl`
   files vendored at `External/Include/`.
4. **Author a separate full-sample `.sln`** referencing the converted
   `.vcxproj` files.
5. **Test locally** with MSBuild on a Windows dev box with VS Build Tools 2022
   installed.
6. **Run smoke test**: `docker compose up c-server` followed by
   `docker exec opc-classic-c-server reg query "HKLM\Software\Classes\CLSID"`
   should list the OPC.SampleServer CLSID.

## Build risks for the full Batch sample route

| Risk | Mitigation |
|---|---|
| MFC dependency in BatchItemSupport.cpp | Either install MFC in the build image (~3 GB workload) or strip the MFC code |
| 66 source files; some have legacy VS6 idioms | Build incrementally; fix `__declspec(thread)` / `_strdup` / wide-char issues case-by-case |
| MIDL output may target an old `.h` header | Regenerate `.h` from the IDL if linker complains about missing symbols |
| Sample uses `#pragma optimize("",off)` | Modern MSVC honors these, but they slow the build; consider removing for Release |
| EXE main loop uses `MessageBox` for fatal errors | Patch to log-and-exit via `OutputDebugString` for headless container use |

## Shipped MVP scope

`opc-sample-server.cpp` implements `IOPCServer`, `IOPCCommon`,
`IOPCGroupStateMgt`, `IOPCItemMgt`, and `IOPCSyncIO` on top of
`External/Include/opcda.h` / `opccomn.h`. It uses `CoRegisterClassObject` for
out-of-process activation, self-registers `OPC.SampleServer.1`, and exposes the
`Sin`, `Square`, and `Random` sample tags for CTT smoke.

Browsing, async I/O, subscriptions, and item/group enumerators intentionally
return `E_NOTIMPL` in this MVP.

## See also

- `docker/opc-c-server/Dockerfile` — current image definition
- `docker/opc-c-server/server-init.ps1` — runtime entrypoint
- `External/OPC Batch 2.00 Sample Code/Sample Server/opc_exe.dsp` — original VS6 project
- `External/OPC Batch 2.00 Sample Code/Sample Server/README.TXT` — OPC Foundation sample notes
