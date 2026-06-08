# `opc-classic/c-server` — native OPC DA smoke server

Windows-container image that builds and runs the hand-rolled native OPC DA smoke server used by the Docker interop fleet.

## Build and run

```pwsh
docker build --file external/docker/opc-c-server/Dockerfile --tag opc-classic/c-server .
docker run --rm --network opc-test-net opc-classic/c-server
```

The image compiles `external\docker\opc-c-server\build\opc-sample-server.cpp` against headers from `external\inc`, registers `OPC.SampleServer.1`, and exposes the DCOM endpoint mapper plus the pinned dynamic range `49152-49200`.

See `build\README.md` for the native project details and `external\docker\README.md` for the full fleet.
