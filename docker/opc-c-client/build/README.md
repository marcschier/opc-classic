# `opc-c-client` build assets (hand-rolled MVP)

This folder ships a small native C++ OPC DA client (`opc-test.cpp`, ~190
lines) that compiles via MSBuild/MSVC into `opc-test.exe`. The container
ENTRYPOINT (`docker/opc-c-client/client.ps1`) invokes the binary with
`<prog-id>` + `<target-host>` arguments forwarded from `docker compose`
or `docker run`.

## What `opc-test.exe` does

1. `CoInitializeEx(MULTITHREADED)`
2. `CLSIDFromProgID` resolves the server's CLSID
3. `CoCreateInstanceEx(CLSCTX_REMOTE_SERVER, COSERVERINFO{pwszName=<host>})`
   returns an `IOPCServer*`
4. `IOPCServer::AddGroup` allocates "opc-test-group" + 1s update rate
5. `IOPCItemMgt::AddItems` adds one item (default ID = `Sin`)
6. `IOPCSyncIO::Read(OPC_DS_CACHE)` and prints quality + VARTYPE
7. RemoveGroup + Release + CoUninitialize

Exits 0 on full success; non-zero exit code identifies which stage
failed (2=CLSIDFromProgID, 3=CoCreateInstanceEx, 4=AddGroup, 5=AddItems,
6=Read). HRESULT printed to stderr on failure.

## Coverage vs the OPC Security sample sources

| Want | Status |
|---|---|
| Single-shot DA smoke | ✅ `opc-test.exe` |
| Sustained throughput driver (`OPCSPEED.cpp`) | ⏳ Future; the OPC Security 1.00 Sample Client `.dsp`-to-`.vcxproj` route is still documented below for anyone who wants the richer driver |

## Smoke usage (post-build)

```pwsh
docker run --rm --network opc-test-net opc-classic/c-client `
    -ProgId Opc.Classic.DaSample.1 `
    -TargetHost opc-classic-managed
```

## Future: wrap the OPC Security sample sources

The original VS6 `.dsp` files live in
`External/OPC Security 1.00 Sample Code/Sample Client/`. If sustained
throughput or interactive UI features are wanted, author a sister
`opc-speed.vcxproj` covering:

| Source | Output | Purpose |
|---|---|---|
| `OPCSPEED.CPP` + `OPCCOMN_i.c` + `OPCDA_i.c` + `WCSUTIL.CPP` | `opc-speed.exe` | Throughput measurement |

The 4-5 files are non-MFC; the vcxproj pattern matches `opc-test.vcxproj`.

## See also

- `docker/opc-c-client/Dockerfile` — current image definition (build wired)
- `docker/opc-c-client/client.ps1` — runtime entrypoint
- `docker/opc-c-client/build/opc-test.cpp` — MVP client source
- `docker/opc-c-client/build/opc-test.vcxproj` — MSBuild project
- `docker/opc-c-server/build/README.md` — sister build for the server side
