# Native COM conformance (Phase 14B)

Tests under this folder connect from the managed .NET 10 client (via
OpcProxyGenerator-emitted shims) against the C++ OPC Foundation Sample
Servers preserved under `COM/`.

## Running

1. CI handles this automatically via the `windows-conformance` job
   on `windows-2022` (see `.github/workflows/build.yml`).

2. Locally on Windows, install the OPC Foundation Core Components
   (places `opcproxy.dll` etc.) and register the native sample servers:
   ```cmd
   .\External\Bin\OpcCoreComponents.exe /S
   .\COM\regserver.cmd
   ```

3. Run only the conformance subset:
   ```bash
   dotnet test --filter "Category=NativeConformance"
   ```

4. Tests soft-skip when the native servers aren't registered (with a
   message logged via the test framework).

## Status

Currently SCAFFOLD-ONLY. The Phase 4 LocalCoClass DCOM client modernization
(Phase 4A follow-up) and the per-method generator bodies for the full
IOPCServer surface (Phase 6B follow-up) are prerequisites. The tests
will activate once those land.
