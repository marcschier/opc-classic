# `opc-c-server` build assets

This folder contains the modern Visual Studio project files that compile the
hand-rolled native OPC DA smoke server into `opc_exe.exe`.

## Current status: hand-rolled MVP is wired up

A hand-rolled ~500-line MVP is shipped at `opc-sample-server.cpp` with matching
`opc-sample-server.vcxproj` and `opc-sample-server.sln`. The Dockerfile now
builds it with MSBuild and emits `C:/out/opc_exe.exe`; use
`docker compose up c-server` on a Windows Docker host for smoke validation.

## Shipped MVP scope

`opc-sample-server.cpp` implements `IOPCServer`, `IOPCCommon`,
`IOPCGroupStateMgt`, `IOPCItemMgt`, and `IOPCSyncIO` on top of
`opcda` / `opccomn.h`. It uses `CoRegisterClassObject` for
out-of-process activation, self-registers `OPC.SampleServer.1`, and exposes the
`Sin`, `Square`, and `Random` sample tags for interop smoke runs.

Browsing, async I/O, subscriptions, and item/group enumerators intentionally
return `E_NOTIMPL` in this MVP.

## See also

- `Dockerfile` — current image definition
- server-init — runtime entrypoint
