# `opc-c-server` build assets (PHASE-2 SCAFFOLD)

This folder will contain the modern Visual Studio project files that compile
the OPC Batch 2.0 Sample Server sources (vendored at
`External/OPC Batch 2.00 Sample Code/Sample Server/`, 66 C++ source files +
the vendored IDL-generated `*_i.c` / `*_p.c` files) into `opc_exe.exe` and
`opc_dll.dll`.

## Current status: build is NOT wired up

The Dockerfile at `docker/opc-c-server/Dockerfile` currently emits a
placeholder `opc_exe.exe` because the `.vcxproj` / `.sln` files in this folder
are not yet authored. `docker build` will succeed but the runtime container
will not be functional (server-init.ps1 logs a warning and idles).

## Conversion checklist

The vendored `.dsp` files (`opc_exe.dsp`, `opc_dll.dsp`) are Visual Studio 6
project files; `msbuild` cannot consume them directly. Conversion path:

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
4. **Author `opc-c-server.sln`** referencing the two `.vcxproj` files.
5. **Test locally** with
   `msbuild docker/opc-c-server/build/opc-c-server.sln /p:Configuration=Release /p:Platform=x64`
   on a Windows dev box with VS Build Tools 2022 installed.
6. **Uncomment the MSBuild invocation** in `docker/opc-c-server/Dockerfile`.
7. **Run smoke test**: `docker compose up c-server` followed by
   `docker exec opc-classic-c-server reg query "HKLM\Software\Classes\CLSID"`
   should list the OPC.SampleServer CLSID.

## Build risks (per the plan in `plan.md`)

| Risk | Mitigation |
|---|---|
| MFC dependency in BatchItemSupport.cpp | Either install MFC in the build image (~3 GB workload) or strip the MFC code |
| 66 source files; some have legacy VS6 idioms | Build incrementally; fix `__declspec(thread)` / `_strdup` / wide-char issues case-by-case |
| MIDL output may target an old `.h` header | Regenerate `.h` from the IDL if linker complains about missing symbols |
| Sample uses `#pragma optimize("",off)` | Modern MSVC honors these, but they slow the build; consider removing for Release |
| EXE main loop uses `MessageBox` for fatal errors | Patch to log-and-exit via `OutputDebugString` for headless container use |

## Alternative: build a smaller hand-rolled server

If the Batch sample's MFC dependency proves too expensive to strip, consider
authoring a fresh ~500-line `opc-sample-server.cpp` that:

- Implements `IOPCServer` + `IOPCGroupStateMgt` + `IOPCItemMgt` + `IOPCSyncIO`
  on top of `External/Include/opcda.h`.
- Uses `CoRegisterClassObject` for self-registration.
- Exposes 2-3 sample tags (`Sin`, `Square`, `Random`) for CTT smoke.

This is the same approach OPC Foundation uses for the smallest demo servers.
~500 lines is cheaper to author than reconstructing the build of 66 vendored
files with possibly-missing toolchain support.

## See also

- `docker/opc-c-server/Dockerfile` — current image definition (build stage stubbed)
- `docker/opc-c-server/server-init.ps1` — runtime entrypoint
- `External/OPC Batch 2.00 Sample Code/Sample Server/opc_exe.dsp` — original VS6 project
- `External/OPC Batch 2.00 Sample Code/Sample Server/README.TXT` — OPC Foundation sample notes
