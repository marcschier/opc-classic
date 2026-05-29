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

## Smoke usage (post-build)

```pwsh
docker run --rm --network opc-test-net opc-classic/c-client `
    -ProgId Opc.Classic.DaSample.1 `
    -TargetHost opc-classic-managed
```

## See also

- `docker/opc-c-client/Dockerfile` — current image definition (build wired)
- `docker/opc-c-client/client.ps1` — runtime entrypoint
- `docker/opc-c-client/build/opc-test.cpp` — MVP client source
- `docker/opc-c-client/build/opc-test.vcxproj` — MSBuild project
- `docker/opc-c-server/build/README.md` — sister build for the server side
