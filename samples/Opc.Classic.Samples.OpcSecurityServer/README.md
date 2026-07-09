# Opc.Classic.Samples.OpcSecurityServer

Managed OPC DA reference server that also publishes the OPC Security 1.00 `IOPCSecurityNT` and `IOPCSecurityPrivate` interfaces. It demonstrates server-side wiring for the managed DCOM transport while keeping DA behavior intentionally minimal.

## Run

```powershell
dotnet run --project .\samples\Opc.Classic.Samples.OpcSecurityServer\Opc.Classic.Samples.OpcSecurityServer.csproj
```

The sample registers as ProgID `Opc.Classic.Samples.OpcSecurityServer.1` with CLSID `5A0DA9C7-56D2-4768-9CB3-6FC5E57B6D51` and is included in `Opc.Classic.slnx`.

With no environment variables set, the server listens on `0.0.0.0:51304`. Set `OPC_CLASSIC_SAMPLE_PORT` to change the default port or `OPC_CLASSIC_LISTEN_ADDRESS` to override the full bind address.

## Demo credentials

- Windows-integrated path: `IOPCSecurityNT::ChangeUser` captures the current process identity on Windows. On non-Windows, set `OPC_CLASSIC_SAMPLE_IDENTITY` to choose the identity string shown by the stub.
- Private path: `IOPCSecurityPrivate::Logon` accepts only `operator` / `demo`.
- `Logoff` clears the in-memory identity and returns the connection to anonymous/default state.

## Critical caveat

This is STUB ACL logic for sample purposes only. It keeps state in memory, hard-codes one password, and does not authorize access to real resources. Never use this implementation in production; replace it with impersonation, role checks, or a hashed credential store.

See [Implementing OPC Security](../../docs/cookbook/08-implementing-opc-security.md) for production guidance.
