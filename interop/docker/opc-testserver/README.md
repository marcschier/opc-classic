# `opc-classic/testserver` — OPC Foundation TestServer

Windows-container image that builds and runs `OpcTestServer_x64.exe` from the
vendored OPC Foundation `external` tree.

## Prerequisites

- Windows Docker host using Windows containers.
- `external` present in the build context.
- `opc-test-net` l2bridge network from `docker`.
- OPERATOR: verify the Visual Studio Build Tools component IDs in the
  Dockerfile against the host's current VS 2022 bootstrapper if the cold build
  fails.

## Build and run

```pwsh
docker build --file interop/docker/opc-testserver/Dockerfile --tag opc-classic/testserver .
docker run --rm --network opc-test-net --hostname opc-classic-testserver opc-classic/testserver
```

The cold Docker build installs VS Build Tools 2022 with VCTools, ATL, and CMake
and runs build-testserver. Subsequent builds
should reuse Docker layers or the CI cache for
`Release`.

At startup, `server-init.ps1` imports the shared DCOM ACL policy, invokes
register-testserver for the no-MSI registration path, starts
`OpcTestServer_x64.exe`, and unregisters on shutdown.

The registered ProgID is `OpcTestServer_x64.1`; the CLSID is
`{F8582CF9-88FB-11DA-A5ED-0060B0692061}`.
