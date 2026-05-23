# OpcClassic CTT Sample Server

Minimal in-process managed DA server used by the Phase 14E OPC
Compliance Test Tool workflow.

## Run

```bash
dotnet run --project samples/OpcClassic.CttServer
```

The server registers as `OpcClassic.DaSample.1` (CLSID
`8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A`) and listens on
`127.0.0.1:0` (port chosen at startup). The CttDaServer impl
returns minimal responses for the IOpcDaServer surface:

- GetStatus -> Running, vendor "OpcClassic .NET CTT Sample"
- AddGroup -> echoes client handle + 1000 as server handle
- RemoveGroup -> no-op success
- GetErrorString -> formatted HRESULT string

## CI integration

`.github/workflows/opc-ctt.yml` invokes this sample via the
`dotnet run` step before the CTT runs. The CTT then connects
through the registered ProgID and exercises the IOPC* surface.

## Status

Scaffold-grade. Real per-method dispatch beyond the 3 methods
above lands in CttDaServer follow-ups as the OpcDaServerDispatcher
(N6) covers more interface methods.
