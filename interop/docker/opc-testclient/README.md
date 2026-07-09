# `opc-classic/testclient` — OPC Foundation TestClient

Windows-container image that runs `OpcTestClient_x64.exe` from the
`opc-classic/testserver` build output.

## Build and run

```pwsh
docker build --file interop/docker/opc-testserver/Dockerfile --tag opc-classic/testserver .
docker build --file interop/docker/opc-testclient/Dockerfile --tag opc-classic/testclient .

docker run --rm --network opc-test-net opc-classic/testclient `
    -TargetHost opc-classic-testserver `
    -ProgId OpcTestServer_x64.1
```

`Dockerfile` uses `FROM opc-classic/testserver AS testserver-artifacts` so the
CoreComponents CMake build is not repeated. Build `opc-classic/testserver`
first or use run-matrix, which orders the two
builds correctly.

`client.ps1` registers the proxy/stub DLLs, redirects local OpcEnum activation
to `-TargetHost` with the DCOM `RemoteServerName` AppID value, runs
`OpcTestClient_x64.exe`, and fails if the expected ProgID is absent.

OPERATOR: validate the `RemoteServerName` redirection on the Windows Docker
host. The current upstream client has no explicit target-host command-line
argument, so this shim uses registry-based DCOM remote activation.
