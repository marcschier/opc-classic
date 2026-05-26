# `opc-c-client` build assets (PHASE-3 SCAFFOLD)

This folder will contain the modern Visual Studio project files that compile
the OPC Security 1.0 Sample Client's headless console programs:

| Source | Output | Purpose |
|---|---|---|
| `OPCTEST.cpp` + `OPCCOMN_i.c` + `OPCDA_i.c` | `opc-test.exe` | Single-shot DA smoke (connect, AddGroup, AddItem, Read, Release) |
| `OPCSPEED.cpp` + same IDL outputs | `opc-speed.exe` | Throughput driver — measures items-per-second over a sustained loop |

Located in: `External/OPC Security 1.00 Sample Code/Sample Client/`

## Current status: build is NOT wired up

The Dockerfile at `docker/opc-c-client/Dockerfile` currently emits placeholder
exes. `docker build` will succeed but the runtime container's `client.ps1`
ENTRYPOINT will log a warning and either idle (with `-Interactive`) or exit 1.

## Conversion checklist

These samples are SMALLER and SIMPLER than the Batch server (4-5 source
files vs 66), AND non-MFC. The `.vcxproj` author should be straightforward:

1. **`opc-c-client.vcxproj` (opc-test.exe)**:
   - Sources: `OPCTEST.cpp`, `OPCCOMN_i.c`, `OPCDA_i.c`, `WCSUTIL.cpp`
   - `<ConfigurationType>Application</ConfigurationType>`
   - `<CharacterSet>Unicode</CharacterSet>`
   - Include paths: `External/Include`, the sample directory
   - Link: `ole32.lib`, `oleaut32.lib`, `advapi32.lib`, `uuid.lib`
2. **`opc-c-speed.vcxproj` (opc-speed.exe)**:
   - Same as above but substitute `OPCTEST.cpp` with `OPCSPEED.cpp`
3. **`opc-c-client.sln`** referencing both `.vcxproj` files
4. **Test locally** with msbuild on a Windows dev box with VS Build Tools
   2022 (no MFC workload required for these samples)
5. **Uncomment the MSBuild invocation** in `docker/opc-c-client/Dockerfile`

## Smoke usage (post-build)

```pwsh
# Single-shot DA smoke against the managed server
docker run --rm --network opc-test-net opc-classic/c-client `
    -ProgId Opc.Classic.DaSample.1 `
    -TargetHost opc-classic-managed

# Throughput driver against the native C server
docker run --rm --network opc-test-net opc-classic/c-client `
    -Speed `
    -ProgId OPC.SampleServer.1 `
    -TargetHost opc-classic-c-server

# Interactive shell for ad-hoc debugging
docker run --rm -it --network opc-test-net opc-classic/c-client -Interactive
# then: docker exec -it <container> powershell
```

## See also

- `docker/opc-c-client/Dockerfile` — current image definition (build stage stubbed)
- `docker/opc-c-client/client.ps1` — runtime entrypoint
- `External/OPC Security 1.00 Sample Code/Sample Client/OPCSPEED.DSP` — original VS6 project
- `docker/opc-c-server/build/README.md` — sister scaffold for the server side
