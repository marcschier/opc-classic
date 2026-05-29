# `opc-classic/c-client` — native OPC DA smoke client

Windows-container image that builds and runs the hand-rolled native OPC DA smoke client used by the Docker interop fleet.

## Build and run

```pwsh
docker build --file docker/opc-c-client/Dockerfile --tag opc-classic/c-client .
docker run --rm --network opc-test-net opc-classic/c-client `
    -ProgId Opc.Classic.DaSample.1 `
    -TargetHost opc-classic-managed
```

The image compiles `docker\opc-c-client\build\opc-test.cpp` against headers from `ext\inc`; `client.ps1` forwards `-ProgId` and `-TargetHost` to `opc-test.exe`.

See `build\README.md` for the native project details and `docker\README.md` for the full fleet.
