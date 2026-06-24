# Native Matrikon proxy scaffold

This Windows-container image is the automated proxy for a Matrikon OPC Explorer run against the Linux managed `Opc.Classic.Samples.SimulationServer`.

What CI can do today:

- build the native C OPC DA exerciser (`opc-test.exe`) from `interop/docker/opc-c-client/build`;
- validate the compose wiring and keep the scaffold reviewable.

What requires an operator/Windows runner with routable DCOM:

- a Linux SimulationServer reachable on TCP 135 plus its activation/object port;
- matching credentials configured on the Linux host with `OPC_CLASSIC_DCOM_USER`, `OPC_CLASSIC_DCOM_PASSWORD`, and `OPC_CLASSIC_DCOM_DOMAIN`;
- Windows DCOM policy/container identity able to present those credentials.

Example:

```powershell
$env:OPC_CLASSIC_TARGET_HOST = 'linux-opc-host'
$env:OPC_CLASSIC_TARGET_PROGID = 'Opc.Classic.Simulation.DA.1'
$env:OPC_CLASSIC_TARGET_ITEMID = 'Plant.Reactor1.Temperature'
docker compose --file interop/docker/docker-compose.test.yml --profile native-matrikon-proxy run --rm native-matrikon-proxy
```

The native exerciser follows the Matrikon shape: remote activation, DA group creation, item add, synchronous read, and cleanup. Matrikon itself should additionally browse OpcEnum, write, and subscribe as described in `samples/Opc.Classic.Samples.SimulationServer/README.md`; those behaviors are covered in-sandbox by `ManagedDcomFullStackE2ETests`.
